using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CustomObjectWindow : EditorWindow
{
    private Vector2 _listScrollPos;
    private Vector2 _inspectorScrollPos;
    private IListable _selectedObject;
    private string _searchFilter = "";
    private List<IListable> _filteredItems = new List<IListable>();

    [MenuItem("Window/Custom Object Inspector")]
    public static void ShowWindow() => GetWindow<CustomObjectWindow>("Custom Inspector");

    private Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.4f));
        DrawListPanel();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        DrawInspectorPanel();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawListPanel()
    {
        GUILayout.Label("Search", EditorStyles.boldLabel);
        _searchFilter = EditorGUILayout.TextField(_searchFilter);

        UpdateFilteredList();

        _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos);

        foreach (var item in _filteredItems)
        {
            string displayName = GetDisplayName(item);
            if (GUILayout.Button(displayName))
            {
                _selectedObject = item;
                Repaint();
            }

            // Двойной клик для перехода (если объект является UnityEngine.Object)
            Rect rect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && Event.current.clickCount == 2)
            {
                if (item is UnityEngine.Object unityObj)
                {
                    Selection.activeObject = unityObj;
                    EditorGUIUtility.PingObject(unityObj);
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void UpdateFilteredList()
    {
        if (string.IsNullOrEmpty(_searchFilter))
            _filteredItems = ObjectRegistry.Items.ToList();
        else
            _filteredItems = ObjectRegistry.Items
                .Where(item => GetDisplayName(item).ToLower().Contains(_searchFilter.ToLower()))
                .ToList();
    }

    private string GetDisplayName(IListable item)
    {
        var nameField = item.GetType().GetField("Name");
        if (nameField != null)
        {
            var val = nameField.GetValue(item);
            if (val != null) return val.ToString();
        }

        var idField = item.GetType().GetField("Id");
        if (idField != null)
        {
            var val = idField.GetValue(item);
            if (val != null) return val.ToString();
        }

        return item.GetType().Name;
    }

    private void DrawInspectorPanel()
    {
        if (_selectedObject == null)
        {
            GUILayout.Label("No object selected", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        GUILayout.Label($"Inspector: {GetDisplayName(_selectedObject)}", EditorStyles.boldLabel);
        _inspectorScrollPos = EditorGUILayout.BeginScrollView(_inspectorScrollPos);

        DrawFields(_selectedObject);

        EditorGUILayout.EndScrollView();
    }

    // ⬇️ ЭТОТ МЕТОД ИСПРАВЛЕН – НЕТ SerializedObject
    private void DrawFields(object obj, string path = "")
    {
        if (obj == null) return;

        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (System.Attribute.IsDefined(field, typeof(HideInInspector)))
                continue;

            var value = field.GetValue(obj);
            var fieldType = field.FieldType;
            string fieldPath = string.IsNullOrEmpty(path) ? field.Name : path + "." + field.Name;

            // --- Обработка примитивных типов (как раньше) ---
            if (fieldType == typeof(string))
            {
                string newValue = EditorGUILayout.TextField(field.Name, (string)value);
                if (newValue != (string)value) field.SetValue(obj, newValue);
                continue;
            }
            if (fieldType == typeof(int))
            {
                int newValue = EditorGUILayout.IntField(field.Name, (int)value);
                if (newValue != (int)value) field.SetValue(obj, newValue);
                continue;
            }
            if (fieldType == typeof(float))
            {
                float newValue = EditorGUILayout.FloatField(field.Name, (float)value);
                if (newValue != (float)value) field.SetValue(obj, newValue);
                continue;
            }
            if (fieldType == typeof(bool))
            {
                bool newValue = EditorGUILayout.Toggle(field.Name, (bool)value);
                if (newValue != (bool)value) field.SetValue(obj, newValue);
                continue;
            }
            if (fieldType.IsEnum)
            {
                Enum newValue = EditorGUILayout.EnumPopup(field.Name, (Enum)value);
                if (!newValue.Equals(value)) field.SetValue(obj, newValue);
                continue;
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                UnityEngine.Object newValue = EditorGUILayout.ObjectField(field.Name, (UnityEngine.Object)value, fieldType, true);
                if (newValue != (UnityEngine.Object)value) field.SetValue(obj, newValue);
                continue;
            }
            if (typeof(IListable).IsAssignableFrom(fieldType))
            {
                if (GUILayout.Button($"Open {field.Name}"))
                {
                    _selectedObject = (IListable)value;
                }
                continue;
            }

            // --- НОВОЕ: Обработка списков и массивов ---
            if (typeof(System.Collections.IList).IsAssignableFrom(fieldType))
            {
                var list = (System.Collections.IList)value;
                int count = list?.Count ?? 0;

                // Получаем или создаём состояние свёрнутости
                if (!_foldoutStates.ContainsKey(fieldPath))
                    _foldoutStates[fieldPath] = false;

                EditorGUILayout.BeginHorizontal();
                _foldoutStates[fieldPath] = EditorGUILayout.Foldout(_foldoutStates[fieldPath], $"{field.Name} (Count = {count})", true);
                // Кнопка "Очистить" (опционально)
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                {
                    list?.Clear();
                }
                EditorGUILayout.EndHorizontal();

                if (_foldoutStates[fieldPath] && list != null)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        string itemPath = fieldPath + $"[{i}]";

                        // Рисуем каждый элемент
                        EditorGUILayout.BeginHorizontal();
                        // Если элемент примитив — рисуем прямо здесь
                        if (item == null)
                        {
                            EditorGUILayout.LabelField($"[{i}]", "null");
                        }
                        else if (item.GetType().IsPrimitive || item is string || item is Enum || item is UnityEngine.Object)
                        {
                            // Для примитивов используем упрощённое редактирование
                            DrawPrimitiveField($"[{i}]", ref item);
                            // Применяем изменение обратно в список
                            list[i] = item;
                        }
                        else
                        {
                            // Для сложных объектов — кнопка для перехода или вложенный просмотр
                            if (GUILayout.Button($"[{i}] {item.GetType().Name}", GUILayout.ExpandWidth(true)))
                            {
                                // Если элемент реализует IListable, делаем его выбранным
                                if (item is IListable listable)
                                    _selectedObject = listable;
                                else
                                    Debug.Log($"Cannot navigate to {item.GetType().Name} because it doesn't implement IListable");
                            }
                        }

                        // Кнопка удаления элемента
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            list.RemoveAt(i);
                            Repaint();
                            break; // выходим, чтобы обновить цикл
                        }
                        EditorGUILayout.EndHorizontal();

                        // Если объект сложный и не IListable, можно дополнительно показать его поля (но осторожно, чтобы не зациклиться)
                        // Например, можно рекурсивно вызвать DrawFields(item, itemPath) — но это может углубить рекурсию.
                        // Для простоты мы этого не делаем.
                    }

                    // Кнопка добавления нового элемента (только если знаем тип)
                    Type elementType = fieldType.IsArray ? fieldType.GetElementType() : fieldType.GetGenericArguments()[0];
                    if (GUILayout.Button($"Add {elementType.Name}"))
                    {
                        object newItem = Activator.CreateInstance(elementType);
                        list.Add(newItem);
                        Repaint();
                    }
                    EditorGUI.indentLevel--;
                }
                continue;
            }

            // --- Для всех остальных сложных объектов ---
            EditorGUILayout.LabelField(field.Name, value?.ToString() ?? "null");
        }
    }

    private void DrawPrimitiveField(string label, ref object value)
    {
        var type = value.GetType();
        if (type == typeof(string))
        {
            string newVal = EditorGUILayout.TextField(label, (string)value);
            if (newVal != (string)value) value = newVal;
        }
        else if (type == typeof(int))
        {
            int newVal = EditorGUILayout.IntField(label, (int)value);
            if (newVal != (int)value) value = newVal;
        }
        else if (type == typeof(float))
        {
            float newVal = EditorGUILayout.FloatField(label, (float)value);
            if (newVal != (float)value) value = newVal;
        }
        else if (type == typeof(bool))
        {
            bool newVal = EditorGUILayout.Toggle(label, (bool)value);
            if (newVal != (bool)value) value = newVal;
        }
        else if (type.IsEnum)
        {
            Enum newVal = EditorGUILayout.EnumPopup(label, (Enum)value);
            if (!newVal.Equals(value)) value = newVal;
        }
        else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            UnityEngine.Object newVal = EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, type, true);
            if (newVal != (UnityEngine.Object)value) value = newVal;
        }
        else
        {
            EditorGUILayout.LabelField(label, value.ToString());
        }
    }
}