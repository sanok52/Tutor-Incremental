using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
// ===== Контекст (может быть расширен) =====
[Serializable]
public abstract class CommandContext
{
    public bool BreakAllNextBehaviour { get; set; }

    // Логические данные (по ключу)
    protected readonly Dictionary<string, List<object>> DataObject = new();

    // Структурные данные (по типу)
    protected readonly Dictionary<Type, List<object>> DataType = new();

    // ============================================================
    // ADD API
    // ============================================================

    public void Add<T>(string key, T value)
    {
        if (!DataObject.TryGetValue(key, out var list))
        {
            list = new List<object>();
            DataObject[key] = list;
        }
        list.Add(value);
    }

    public void Add<T>(T value)
    {
        var type = typeof(T);

        if (!DataType.TryGetValue(type, out var list))
        {
            list = new List<object>();
            DataType[type] = list;
        }
        list.Add(value);
    }

    // ============================================================
    // GET BY KEY
    // ============================================================

    public T[] Gets<T>(string key)
    {
        if (!DataObject.TryGetValue(key, out var list) || list == null)
        {
            //Debug.LogWarning($"[CommandContext] Вы хотите получить объект по ключу '{key}', " +
            //                 $"но его нет в этом контексте.");
            return Array.Empty<T>();
        }

        var result = list.OfType<T>().ToArray();

        if (result.Length == 0)
        {
           // Debug.LogWarning($"[CommandContext] Объекты по ключу '{key}' существуют, " +
           //                  $"но ни один не является типом {typeof(T).Name}.");
        }

        return result;
    }

    public T Get<T>(string key)
    {
        var arr = Gets<T>(key);
        return arr.Length > 0 ? arr[0] : default;
    }

    // ============================================================
    // GET BY TYPE
    // ============================================================

    public T[] Gets<T>()
    {
        var type = typeof(T);

        if (!DataType.TryGetValue(type, out var list) || list == null)
        {
           // Debug.LogWarning($"[CommandContext] Вы хотите получить объект типа {type.Name}, " +
            //                 $"но его нет в этом контексте.");
            return Array.Empty<T>();
        }

        var result = list.OfType<T>().ToArray();

        if (result.Length == 0)
        {
            //Debug.LogWarning($"[CommandContext] Объекты типа {type.Name} существуют, " +
            //                 $"но они не совпадают с T={typeof(T).Name}.");
        }

        return result;
    }

    public T Get<T>()
    {
        var arr = Gets<T>();
        return arr.Length > 0 ? arr[0] : default;
    }

    public void Apply<T> (params OverridebleForContext<T>[] overridebleForContext)
    {
        foreach (var item in overridebleForContext)
        {
            Add<T>(item.id, item.value);
        }
    }
}
