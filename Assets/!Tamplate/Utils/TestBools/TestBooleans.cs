using System;
using System.Linq;
using UnityEngine;

public static class TestBooleans
{
    private static TestBooleansConfig config;

    private static TestBooleansConfig Config
    {
        get
        {
            if (config == null)
            {
                config = Resources.Load<TestBooleansConfig>("TestBooleansConfig");
                if (config == null)
                {
                    Debug.LogError("TestBooleansConfig not found in Resources!");
                }
            }
            return config;
        }
    }

    public static bool IsDebug => Config != null && Config.IsDebug;

    public static bool GetValue(string id)
    {
        if (Config == null)
            return true;

        var tb = Config.testBools.FirstOrDefault(x => x.ID == id);
        if (tb == null)
            return true;

        return tb.Get();
    }
}

[Serializable]
public class TestBool
{
    public string ID;

    public bool testValue = true;
    public bool releaseValue = false;

    public bool Get()
    {
#if UNITY_EDITOR
        return TestBooleans.IsDebug ? testValue : releaseValue;
#else
        return releaseValue;
#endif
    }
}
