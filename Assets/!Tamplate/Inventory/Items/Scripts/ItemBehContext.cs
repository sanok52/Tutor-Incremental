using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemBehContext : CommandContext
{
    public ItemInstanceInvSys ItemInstance;

    [Space]
    public Dictionary<Type, Dictionary<string, object>> SpecificParametrs = new Dictionary<Type, Dictionary<string, object>>();

    public T Get<T>(string id) //Найти объект типа T по id
    {
        if (SpecificParametrs.TryGetValue(typeof(T), out var parameters))
        {
            if (parameters.TryGetValue(id, out var value))
            {
                return (T)value;
            }
        }
        return default;
    }

    public void Add<T>(T value, string id)
    {
        if (!SpecificParametrs.TryGetValue(typeof(T), out var parameters))
        {
            parameters = new Dictionary<string, object>();
            SpecificParametrs[typeof(T)] = parameters;
        }
        parameters[id] = value;
    }
}