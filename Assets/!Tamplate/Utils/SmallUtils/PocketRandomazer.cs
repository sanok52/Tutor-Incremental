using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class PocketRandomazer
{
    private static readonly Dictionary<(string, Type), object> pockets
        = new Dictionary<(string, Type), object>();


    public static void CreatePocket<T>(string id, params T[] dataCreates)
    {
        CreatePocket(id, 1, dataCreates);
    }

    public static void CreatePocket<T>(string id, int counts, params T[] dataCreates)
    {
        PocketRandomDataCreate<T>[] pockets = new PocketRandomDataCreate<T>[dataCreates.Length];
        for (int i = 0; i < pockets.Length; i++)        
            pockets[i] = new PocketRandomDataCreate<T>(dataCreates[i], counts);        
        CreatePocket(id, pockets);
    }

    public static void CreatePocket<T>(string id, params PocketRandomDataCreate<T>[] dataCreates)
    {
        var key = (id, typeof(T));

        var pocket = new PocketRandomElement<T>
        {
            id = id
        };

        foreach (var data in dataCreates)
        {
            if (data.Count <= 0)
                continue;

            for (int i = 0; i < data.Count; i++)
                pocket.allValues.Add(data.Value);
        }

        pockets[key] = pocket;
    }

    public static void CreatePocketBool (string id, int countTrue, int countFalse)
    {
        bool[] bools = new bool[countTrue + countFalse];
        for (int i = 0; i < countTrue; i++)
            bools[i] = true;

        CreatePocket<bool>(id, bools);
    }

    public static void Clear()
    {
        pockets.Clear();
    }

    public static T GetRandomElement<T>(string id)
    {
        var key = (id, typeof(T));

        if (!pockets.TryGetValue(key, out var obj))
        {
            Debug.LogError($"PocketRandomazer: Pocket '{id}' for type {typeof(T)} not found");
            return default;
        }

        return ((PocketRandomElement<T>)obj).GetValue();
    }

    public static bool ContainsPocket<T> (string id)
    {
        var key = (id, typeof(T));
        return pockets.TryGetValue(key, out var obj);
    }
}

[Serializable]
public struct PocketRandomDataCreate<T>
{
    public T Value;
    public int Count;

    public PocketRandomDataCreate(T value, int count)
    {
        Value = value;
        Count = count;
    }
}

public class PocketRandomElement<T>
{
    public string id;
    public List<T> allValues = new List<T>();

    private List<T> currentValues = new List<T>();

    public T GetValue()
    {
        if (currentValues.Count == 0)
            Reload();

         T value = currentValues[0];
        currentValues.RemoveAt(0);  
        return value;
    }

    private void Reload()
    {
        currentValues.Clear();
        currentValues.AddRange(allValues);
        Shuffle(currentValues);
    }

    private void Shuffle (List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}