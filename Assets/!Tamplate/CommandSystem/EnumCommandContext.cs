using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public abstract class EnumCommandContext<E> : CommandContext
    where E : Enum
{
    protected readonly Dictionary<E, List<object>> EnumObject = new();

    public void Add<EType>(E key, EType value)
    {
        if (!EnumObject.TryGetValue(key, out var list))
        {
            list = new List<object>();
            EnumObject[key] = list;
        }
        list.Add(value);
    }

    public EType[] Gets<EType>(E key)
    {
        if (!EnumObject.TryGetValue(key, out var list) || list == null)
        {
            Debug.LogWarning($"[CommandContext] Вы хотите получить объект по enum‑ключу '{key}', " +
                             $"но его нет в этом контексте.");
            return Array.Empty<EType>();
        }

        var result = list.OfType<EType>().ToArray();

        if (result.Length == 0)
        {
            Debug.LogWarning($"[CommandContext] Объекты по enum‑ключу '{key}' существуют, " +
                             $"но ни один не является типом {typeof(EType).Name}.");
        }

        return result;
    }

    public EType Get<EType>(E key)
    {
        var arr = Gets<EType>(key);
        return arr.Length > 0 ? arr[0] : default;
    }
}
