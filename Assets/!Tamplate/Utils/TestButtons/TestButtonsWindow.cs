using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class TestButtonsWindow : EditorWindow
{
    // Меню Tools -> TestButtons
    [MenuItem("Tools/TestButtons")]
    public static void ShowWindow() => GetWindow<TestButtonsWindow>("Test Buttons");

    // Данные для отображения в окне
    private List<ButtonDisplayData> _displayData = new();
    private string _newId = "";

    private void OnEnable() => LoadData();

    private void LoadData()
    {
        _displayData.Clear();
        foreach (var kv in TestButtons.GetAllButtons())
            _displayData.Add(new ButtonDisplayData(kv.Key, kv.Value, OnDataChanged));
    }

    // Вызывается при любом изменении данных
    private void OnDataChanged()
    {
        // Синхронизируем статический словарь с текущим списком
        var infos = _displayData.Select(d => d.ToInfo());
        TestButtons.ReplaceAll(infos);
        Repaint();
    }

    private void OnGUI()
    {
        DrawAddSection();
        EditorGUILayout.Space(10);
        DrawButtonsList();
    }

    private void DrawAddSection()
    {
        EditorGUILayout.BeginHorizontal();
        _newId = EditorGUILayout.TextField("New ID", _newId);
        GUI.enabled = !string.IsNullOrEmpty(_newId) && !_displayData.Any(d => d.id == _newId);
        if (GUILayout.Button("Add", GUILayout.Width(60)))
        {
            var data = new ButtonDisplayData(_newId, new TestButtonInfo
            {
                id = _newId,
                strData = Array.Empty<string>(),
                goData = Array.Empty<GameObject>()
            }, OnDataChanged);
            _displayData.Add(data);
            OnDataChanged();
            _newId = "";
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawButtonsList()
    {
        if (_displayData.Count == 0)
        {
            EditorGUILayout.HelpBox("Нет кнопок. Создайте первую.", MessageType.Info);
            return;
        }

        for (int i = 0; i < _displayData.Count; i++)
        {
            var data = _displayData[i];
            EditorGUILayout.BeginVertical("box");
            DrawButtonHeader(data, i);
            if (data.foldout)
            {
                DrawButtonDetails(data);
            }
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawButtonHeader(ButtonDisplayData data, int index)
    {
        EditorGUILayout.BeginHorizontal();

        // Поле для ID с проверкой уникальности
        string newId = EditorGUILayout.TextField(data.id, GUILayout.MinWidth(100));
        if (newId != data.id)
        {
            if (string.IsNullOrEmpty(newId))
                EditorGUILayout.HelpBox("ID не может быть пустым.", MessageType.Error);
            else if (_displayData.Any(d => d != data && d.id == newId))
                EditorGUILayout.HelpBox("ID уже используется.", MessageType.Error);
            else
                data.id = newId;
        }

        data.foldout = EditorGUILayout.Foldout(data.foldout, "", true);

        if (GUILayout.Button("Click", GUILayout.Width(50)))
        {
            // Имитация нажатия
            TestButtons.ClickButton(data.id);
        }

        if (GUILayout.Button("Delete", GUILayout.Width(60)))
        {
            _displayData.RemoveAt(index);
            OnDataChanged();
            GUIUtility.ExitGUI(); // предотвращает ошибки отрисовки
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawButtonDetails(ButtonDisplayData data)
    {
        EditorGUILayout.LabelField("String Data", EditorStyles.boldLabel);
        DrawReorderableList(ref data.strReorderableList, data.strData,
            (list, index) => data.strData[index] = EditorGUILayout.TextField(data.strData[index]),
            () => { data.strData.Add(""); OnDataChanged(); },
            (index) => { data.strData.RemoveAt(index); OnDataChanged(); });

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("GameObject Data", EditorStyles.boldLabel);
        DrawReorderableList(ref data.goReorderableList, data.goData,
            (list, index) => data.goData[index] = (GameObject)EditorGUILayout.ObjectField(data.goData[index], typeof(GameObject), true),
            () => { data.goData.Add(null); OnDataChanged(); },
            (index) => { data.goData.RemoveAt(index); OnDataChanged(); });
    }

    // Вспомогательный метод для рисования ReorderableList
    private void DrawReorderableList<T>(ref ReorderableList list, List<T> items,
    Action<ReorderableList, int> drawElement,
    Action onAdd, Action<int> onRemove)
    {
        if (list == null)
        {
            // Сначала создаём список
            var newList = new ReorderableList(items, typeof(T));

            // Затем устанавливаем свойства, используя newList в лямбдах
            newList.drawElementCallback = (rect, index, active, focused) =>
            {
                drawElement(newList, index);
            };
            newList.elementHeight = EditorGUIUtility.singleLineHeight + 2;
            newList.onAddCallback = (l) => onAdd();
            newList.onRemoveCallback = (l) =>
            {
                if (l.index >= 0 && l.index < items.Count)
                    onRemove(l.index);
            };

            list = newList;
        }
        list.DoLayoutList();
    }
}

/// <summary>Вспомогательный класс для хранения данных одной кнопки в окне редактора.</summary>
public class ButtonDisplayData
{
    public string id;
    public List<string> strData;
    public List<GameObject> goData;
    public bool foldout;

    // Ссылки на ReorderableList (кэшируются)
    public ReorderableList strReorderableList;
    public ReorderableList goReorderableList;

    private Action _onChanged;

    public ButtonDisplayData(string id, TestButtonInfo info, Action onChanged)
    {
        this.id = id;
        strData = new List<string>(info.strData ?? Array.Empty<string>());
        goData = new List<GameObject>(info.goData ?? Array.Empty<GameObject>());
        _onChanged = onChanged;
        foldout = false;
    }

    public TestButtonInfo ToInfo() => new()
    {
        id = id,
        strData = strData.ToArray(),
        goData = goData.ToArray()
    };
}