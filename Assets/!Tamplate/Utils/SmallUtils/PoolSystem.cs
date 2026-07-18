using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Пул объектов типа R. R должен быть MonoBehaviour и реализовывать IPoolable.
/// </summary>
public class Pool<T> where T : MonoBehaviour, IPoolable
{
    private Dictionary<string, List<T>> _pools = new Dictionary<string, List<T>>();
    private Dictionary<string, T> _prefabs = new Dictionary<string, T>();

    public Pool(params IPoolable[] prefabs) : this(0, prefabs)
    {

    }

    public Pool(int initialCapacity = 0, params IPoolable[] prefabs) : this(prefabs, initialCapacity)
    {

    }
    
    /// <summary>
    /// Создаёт пул с указанными префабами.
    /// </summary>
    /// <param name="prefabs">Массив префабов, каждый с уникальным IDInPool.</param>
    /// <param name="initialCapacity">Количество предварительно созданных объектов для каждого TargetID.</param>
    public Pool(IPoolable[] prefabs, int initialCapacity = 0)
    {
        foreach (var prefab in prefabs)
        {
            T typedPrefab = prefab as T;
            if (typedPrefab == null)
            {
                Debug.LogError($"Pool: префаб {prefab} не может быть приведён к {typeof(T)}");
                continue;
            }
            RegisterPrefabInternal(typedPrefab, initialCapacity);
        }
    }

    /// <summary>
    /// Регистрирует новый префаб в пуле после создания.
    /// </summary>
    public void RegisterPrefab(T prefab, int prewarmCount = 0)
    {
        RegisterPrefabInternal(prefab, prewarmCount);
    }

    private void RegisterPrefabInternal(T prefab, int prewarmCount)
    {
        string id = prefab.IDInPool;
        if (_prefabs.ContainsKey(id))
        {
            Debug.LogWarning($"Pool: TargetID '{id}' уже зарегистрирован");
            return;
        }
        _prefabs.Add(id, prefab);
        _pools.Add(id, new List<T>());
        for (int i = 0; i < prewarmCount; i++)
        {
            T obj = CreateNew(prefab);
            Return(obj);
        }
    }


    public T Get(string id, Vector3 position = default, Quaternion rotation = default)
    {
        return Get(id, out _, position, rotation);
    }

    /// <summary>
    /// Получает объект из пула (или создаёт новый) с заданным TargetID, позицией и поворотом.
    /// </summary>
    public T Get(string id, out bool isCreated, Vector3 position = default, Quaternion rotation = default)
    {
        isCreated = false;

        if (!_pools.ContainsKey(id))
        {
            Debug.LogError($"Pool: TargetID '{id}' не зарегистрирован");
            return null;
        }

        List<T> pool = _pools[id];
        T result;
        if (pool.Count > 0)
        {
            int lastIndex = pool.Count - 1;
            result = pool[lastIndex];
            pool.RemoveAt(lastIndex);
        }
        else
        {
            if (!_prefabs.TryGetValue(id, out T prefab))
            {
                Debug.LogError($"Pool: нет префаба для TargetID '{id}'");
                return null;
            }
            result = CreateNew(prefab);
            isCreated = true;
        }

        result.transform.position = position;
        result.transform.rotation = rotation;
        result.OnSpawn();
        return result;
    }

    /// <summary>
    /// Возвращает объект в пул.
    /// </summary>
    public void Return(T obj)
    {
        if (obj == null) return;
        string id = obj.IDInPool;
        if (!_pools.ContainsKey(id))
        {
            Debug.LogError($"Pool: попытка вернуть объект с незарегистрированным TargetID '{id}'");
            return;
        }
        obj.OnDespawn();
        _pools[id].Add(obj);
    }

    private T CreateNew(T prefab)
    {
        T instance = Object.Instantiate(prefab);
        instance.gameObject.SetActive(false);
        return instance;
    }
}

/// <summary>
/// Интерфейс для объектов, которые могут быть использованы в пуле.
/// </summary>
public interface IPoolable
{
    string IDInPool { get; }
    void OnSpawn();   // вызывается при получении из пула
    void OnDespawn(); // вызывается при возврате в пул
}