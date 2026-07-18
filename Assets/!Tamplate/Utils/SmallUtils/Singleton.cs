using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                if (_instance == null)
                {
                    var go = new GameObject(typeof(T).Name);
                    _instance = go.AddComponent<T>();
                    if((_instance as Singleton<T>).IsDontDestroyOnLoad)
                        DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public abstract bool IsDontDestroyOnLoad { get; }
}