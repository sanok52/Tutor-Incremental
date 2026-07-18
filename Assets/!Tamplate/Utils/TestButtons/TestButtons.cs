using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor; // если используется в редакторе

public static class TestButtons
{
    // Данные, загруженные из ScriptableObject
    private static TestButtonsData _data;
    private static TestButtonsData Data
    {
        get
        {
            if (_data == null)
                LoadData();
            return _data;
        }
    }

    // Кэш для быстрого доступа по ID (строим при каждой загрузке)
    private static Dictionary<string, TestButtonInfo> _buttonCache;

    // Подписки (сохраняются только в памяти, они не сериализуются)
    private static readonly Dictionary<string, List<Action<TestButtonInfo>>> _subscribers = new();

    // Глобальное событие
    public static event Action<TestButtonInfo> OnClick;

    // ------------------ Загрузка / Сохранение ------------------

    [RuntimeInitializeOnLoadMethod]
    private static void ResetSubscriptionsOnGameStart()
    {
        // Очищаем все подписки при старте игры
        ClearAllSubscriptions();
    }

    private static void LoadData()
    {
        // Ищем ассет в проекте
        _data = AssetDatabase.LoadAssetAtPath<TestButtonsData>("Assets/TestButtonsData.asset");
        if (_data == null)
        {
            // Если ассета нет – создаём
            _data = ScriptableObject.CreateInstance<TestButtonsData>();
            AssetDatabase.CreateAsset(_data, "Assets/TestButtonsData.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Создан новый файл TestButtonsData.asset");
        }

        RebuildCache();
    }

    private static void RebuildCache()
    {
        _buttonCache = new Dictionary<string, TestButtonInfo>();
        foreach (var entry in Data.entries)
        {
            if (!string.IsNullOrEmpty(entry.id))
            {
                _buttonCache[entry.id] = new TestButtonInfo
                {
                    id = entry.id,
                    strData = entry.strData?.ToArray() ?? Array.Empty<string>(),
                    goData = entry.goData?.ToArray() ?? Array.Empty<GameObject>()
                };
            }
        }
    }

    // Сохраняет изменения в ассет
    private static void SaveData()
    {
        EditorUtility.SetDirty(_data);
        AssetDatabase.SaveAssets();
        RebuildCache(); // обновляем кэш
    }

    // ------------------ Публичные методы ------------------

    public static void AddButton(string id, string[] strData, GameObject[] goData)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("ID не может быть пустым.");
        if (_buttonCache.ContainsKey(id))
            throw new InvalidOperationException($"Кнопка с ID '{id}' уже существует.");

        var entry = new TestButtonsData.ButtonEntry
        {
            id = id,
            strData = new List<string>(strData ?? Array.Empty<string>()),
            goData = new List<GameObject>(goData ?? Array.Empty<GameObject>())
        };
        Data.entries.Add(entry);
        SaveData();
    }

    public static bool RemoveButton(string id)
    {
        var removed = Data.entries.RemoveAll(e => e.id == id) > 0;
        if (removed)
            SaveData();
        return removed;
    }

    public static bool TryGetButton(string id, out TestButtonInfo info)
    {
        return _buttonCache.TryGetValue(id, out info);
    }

    public static IEnumerable<KeyValuePair<string, TestButtonInfo>> GetAllButtons()
    {
        return _buttonCache.Select(kv => kv);
    }

    public static void Clear()
    {
        Data.entries.Clear();
        SaveData();
    }

    public static void ReplaceAll(IEnumerable<TestButtonInfo> infos)
    {
        Data.entries.Clear();
        foreach (var info in infos)
        {
            Data.entries.Add(new TestButtonsData.ButtonEntry
            {
                id = info.id,
                strData = new List<string>(info.strData ?? Array.Empty<string>()),
                goData = new List<GameObject>(info.goData ?? Array.Empty<GameObject>())
            });
        }
        SaveData();
    }

    public static bool Contains(string id) => _buttonCache.ContainsKey(id);
    public static int Count => _buttonCache.Count;

    // ------------------ Подписки ------------------

    public static void SubOnClick(string id, Action<TestButtonInfo> callback)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("ID не может быть пустым.");
        if (callback == null) throw new ArgumentNullException(nameof(callback));

        if (!_subscribers.ContainsKey(id))
            _subscribers[id] = new List<Action<TestButtonInfo>>();
        _subscribers[id].Add(callback);
    }

    public static void UnsubOnClick(string id, Action<TestButtonInfo> callback)
    {
        if (string.IsNullOrEmpty(id) || callback == null) return;
        if (_subscribers.TryGetValue(id, out var list))
            list.Remove(callback);
    }

    public static void ClearSubscriptions(string id)
    {
        if (!string.IsNullOrEmpty(id))
            _subscribers.Remove(id);
    }

    public static void ClearAllSubscriptions() => _subscribers.Clear();

    // ------------------ Вызов нажатия ------------------

    public static void ClickButton(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("ID не указан.");
            return;
        }

        if (!_buttonCache.TryGetValue(id, out var info))
        {
            Debug.LogWarning($"Кнопка с ID '{id}' не найдена.");
            return;
        }

        OnClick?.Invoke(info);

        if (_subscribers.TryGetValue(id, out var callbacks))
        {
            var snapshot = callbacks.ToArray();
            foreach (var cb in snapshot)
                cb?.Invoke(info);
        }
    }

    // ------------------ Инициализация при загрузке домена ------------------

    // Этот метод вызывается автоматически при загрузке домена (после рекомпиляции)
    [UnityEditor.InitializeOnLoadMethod]
    private static void Initialize()
    {
        // Просто вызываем LoadData, чтобы подтянуть сохранённые кнопки
        LoadData();
    }
}