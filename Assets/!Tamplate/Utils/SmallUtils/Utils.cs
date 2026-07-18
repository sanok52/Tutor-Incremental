using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Random = UnityEngine.Random;

public static class Utils
{
    private static readonly System.Random _random = new();

    public static T GetRandomValue<T>() where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        return values[_random.Next(values.Length)];
    }

    public static T GetRandomEnumValue<T>(this T _) where T : Enum
    {
        var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        return values[_random.Next(values.Length)];
    }

    public static T DeepClone<T>(this T obj) where T : class
    {
        string json = JsonUtility.ToJson(obj);
        return JsonUtility.FromJson<T>(json);
    }

    public static T[] GetAllEnumArray<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
    }

    public static T[] GetAllEnumArray<T>(this T _) where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
    }

    public static Transform[] GetAllChildrens(Transform root)
    {
        List<Transform> childs = new List<Transform>();
        for (int i = 0; i < root.childCount; i++)
        {
            childs.Add(root.GetChild(i));
            if (root.GetChild(i).childCount > 0)
                childs.AddRange(GetAllChildrens(root.GetChild(i)));
        }

        return childs.ToArray();
    }
}

public static class TransformExtensions
{
    public static Transform[] GetAllChildOfChild(this Transform root)
    {
        List<Transform> childs = new List<Transform>();
        for (int i = 0; i < root.childCount; i++)
        {
            childs.Add(root.GetChild(i));
            if (root.GetChild(i).childCount > 0)
                childs.AddRange(root.GetChild(i).GetAllChildOfChild());
        }

        return childs.ToArray();
    }
}

public static class ArrayExtensions
{
    public static T RandomElement<T>(this T[] array)
    {
        if (array == null || array.Length == 0)
            return default;

        return array[Random.Range(0, array.Length)];
    }
}


[Serializable]
public class BoolTrigger
{
    private bool value = false;
    public bool Value
    {
        get { return IsTrue(); }
        set {  Set(value); }
    }

    public void Set()
    {
        Set(true);
    }

    private void Set(bool value)
    {
        this.value = value;
    }

    private bool IsTrue()
    {
        if (value)
        {
            value = false;
            return true;
        }
        return false;
    }

    public bool GetSilet()
    {
        return value;
    }
}

