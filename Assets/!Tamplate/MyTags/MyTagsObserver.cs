using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Статический класс-наблюдатель для быстрого поиска объектов по тегам.
/// </summary>
public static class MyTagsObserver
{
    private static HashSet<MyTagsContainer> allContainers = new HashSet<MyTagsContainer>();
    private static Dictionary<string, HashSet<MyTagsContainer>> tagToContainers = new Dictionary<string, HashSet<MyTagsContainer>>();

    // Флаг, что индексы были принудительно перестроены через Initialize
    private static bool indexesAreValid = false;

    /// <summary>
    /// Все зарегистрированные контейнеры.
    /// </summary>
    public static IReadOnlyCollection<MyTagsContainer> AllContainers => allContainers;

    #region Регистрация (внутренняя)

    // Эти методы теперь public internal, чтобы их можно было вызвать из Initialize
    // Но они всё ещё предназначены для внутреннего использования

    /// <summary>
    /// Регистрирует контейнер (вызывается из компонента)
    /// </summary>
    public static void RegisterContainer(MyTagsContainer container)
    {
        if (container == null) return;

        // Всегда добавляем в allContainers
        if (allContainers.Add(container))
        {
            // Индексируем теги
            foreach (string tag in container.Tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;

                if (!tagToContainers.TryGetValue(tag, out var set))
                {
                    set = new HashSet<MyTagsContainer>();
                    tagToContainers[tag] = set;
                }
                set.Add(container);
            }
        }
    }

    /// <summary>
    /// Удаляет регистрацию контейнера
    /// </summary>
    public static void UnregisterContainer(MyTagsContainer container)
    {
        if (container == null || !allContainers.Remove(container))
            return;

        foreach (string tag in container.Tags)
        {
            if (string.IsNullOrEmpty(tag)) continue;

            if (tagToContainers.TryGetValue(tag, out var set))
            {
                set.Remove(container);
                if (set.Count == 0)
                    tagToContainers.Remove(tag);
            }
        }
    }

    /// <summary>
    /// Обновляет индексы после изменения тегов
    /// </summary>
    public static void UpdateContainer(MyTagsContainer container)
    {
        // Просто перерегистрируем
        UnregisterContainer(container);
        RegisterContainer(container);
    }

    #endregion

    #region Поиск контейнеров (массивы)

    /// <summary>
    /// Возвращает массив всех контейнеров, содержащих указанный тег.
    /// </summary>
    public static MyTagsContainer[] FindContainersWithTag(string tag)
    {
        if (string.IsNullOrEmpty(tag) || !tagToContainers.TryGetValue(tag, out var set))
            return System.Array.Empty<MyTagsContainer>();

        var result = new MyTagsContainer[set.Count];
        set.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Возвращает первый найденный контейнер с указанным тегом или null.
    /// </summary>
    public static MyTagsContainer FindFirstContainerWithTag(string tag)
    {
        if (string.IsNullOrEmpty(tag) || !tagToContainers.TryGetValue(tag, out var set))
            return null;

        using var enumerator = set.GetEnumerator();
        return enumerator.MoveNext() ? enumerator.Current : null;
    }

    /// <summary>
    /// Возвращает массив всех контейнеров, содержащих хотя бы один из указанных тегов.
    /// </summary>
    public static MyTagsContainer[] FindContainersWithAnyTags(params string[] tags)
    {
        return FindContainersWithAnyTags((IEnumerable<string>)tags);
    }

    /// <summary>
    /// Возвращает массив всех контейнеров, содержащих хотя бы один из указанных тегов.
    /// </summary>
    public static MyTagsContainer[] FindContainersWithAnyTags(IEnumerable<string> tags)
    {
        if (tags == null) return System.Array.Empty<MyTagsContainer>();

        var resultSet = new HashSet<MyTagsContainer>();
        foreach (string tag in tags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            if (tagToContainers.TryGetValue(tag, out var set))
                resultSet.UnionWith(set);
        }

        var result = new MyTagsContainer[resultSet.Count];
        resultSet.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Возвращает первый найденный контейнер с любым из указанных тегов или null.
    /// </summary>
    public static MyTagsContainer FindFirstContainerWithAnyTags(params string[] tags)
    {
        return FindFirstContainerWithAnyTags((IEnumerable<string>)tags);
    }

    /// <summary>
    /// Возвращает первый найденный контейнер с любым из указанных тегов или null.
    /// </summary>
    public static MyTagsContainer FindFirstContainerWithAnyTags(IEnumerable<string> tags)
    {
        if (tags == null) return null;

        foreach (string tag in tags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            if (tagToContainers.TryGetValue(tag, out var set) && set.Count > 0)
            {
                using var enumerator = set.GetEnumerator();
                if (enumerator.MoveNext())
                    return enumerator.Current;
            }
        }
        return null;
    }

    /// <summary>
    /// Возвращает массив всех контейнеров, содержащих все указанные теги.
    /// </summary>
    public static MyTagsContainer[] FindContainersWithAllTags(params string[] tags)
    {
        return FindContainersWithAllTags((IEnumerable<string>)tags);
    }

    /// <summary>
    /// Возвращает массив всех контейнеров, содержащих все указанные теги.
    /// </summary>
    public static MyTagsContainer[] FindContainersWithAllTags(IEnumerable<string> tags)
    {
        if (tags == null) return System.Array.Empty<MyTagsContainer>();

        var tagList = tags.Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();
        if (tagList.Count == 0) return System.Array.Empty<MyTagsContainer>();

        if (!tagToContainers.TryGetValue(tagList[0], out var firstSet))
            return System.Array.Empty<MyTagsContainer>();

        var resultSet = new HashSet<MyTagsContainer>(firstSet);
        for (int i = 1; i < tagList.Count; i++)
        {
            if (!tagToContainers.TryGetValue(tagList[i], out var nextSet))
                return System.Array.Empty<MyTagsContainer>();

            resultSet.IntersectWith(nextSet);
            if (resultSet.Count == 0)
                break;
        }

        var result = new MyTagsContainer[resultSet.Count];
        resultSet.CopyTo(result);
        return result;
    }

    /// <summary>
    /// Возвращает первый найденный контейнер, содержащий все указанные теги, или null.
    /// </summary>
    public static MyTagsContainer FindFirstContainerWithAllTags(params string[] tags)
    {
        return FindFirstContainerWithAllTags((IEnumerable<string>)tags);
    }

    /// <summary>
    /// Возвращает первый найденный контейнер, содержащий все указанные теги, или null.
    /// </summary>
    public static MyTagsContainer FindFirstContainerWithAllTags(IEnumerable<string> tags)
    {
        if (tags == null) return null;

        var tagList = tags.Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();
        if (tagList.Count == 0) return null;

        if (!tagToContainers.TryGetValue(tagList[0], out var firstSet))
            return null;

        foreach (var container in firstSet)
        {
            bool hasAll = true;
            for (int i = 1; i < tagList.Count; i++)
            {
                if (!container.Contains(tagList[i]))
                {
                    hasAll = false;
                    break;
                }
            }
            if (hasAll)
                return container;
        }
        return null;
    }

    #endregion

    #region Поиск игровых объектов (массивы)

    /// <summary>
    /// Возвращает массив игровых объектов, содержащих указанный тег.
    /// </summary>
    public static GameObject[] FindGameObjectsWithTag(string tag)
    {
        var containers = FindContainersWithTag(tag);
        var result = new GameObject[containers.Length];
        for (int i = 0; i < containers.Length; i++)
            result[i] = containers[i].gameObject;
        return result;
    }

    /// <summary>
    /// Возвращает первый найденный игровой объект с указанным тегом или null.
    /// </summary>
    public static GameObject FindFirstGameObjectWithTag(string tag)
    {
        var container = FindFirstContainerWithTag(tag);
        return container != null ? container.gameObject : null;
    }

    /// <summary>
    /// Возвращает массив игровых объектов, содержащих хотя бы один из указанных тегов.
    /// </summary>
    public static GameObject[] FindGameObjectsWithAnyTags(params string[] tags)
    {
        return FindGameObjectsWithAnyTags((IEnumerable<string>)tags);
    }

    /// <summary>
    /// Возвращает массив игровых объектов, содержащих хотя бы один из указанных тегов.
    /// </summary>
    public static GameObject[] FindGameObjectsWithAnyTags(IEnumerable<string> tags)
    {
        var containers = FindContainersWithAnyTags(tags);
        var result = new GameObject[containers.Length];
        for (int i = 0; i < containers.Length; i++)
            result[i] = containers[i].gameObject;
        return result;
    }

    /// <summary>
    /// Возвращает первый найденный игровой объект с любым из указанных тегов или null.
    /// </summary>
    public static GameObject FindFirstGameObjectWithAnyTags(params string[] tags)
    {
        return FindFirstGameObjectWithAnyTags((IEnumerable<string>)tags);
    }

    /// <summary>
    /// Возвращает первый найденный игровой объект с любым из указанных тегов или null.
    /// </summary>
    public static GameObject FindFirstGameObjectWithAnyTags(IEnumerable<string> tags)
    {
        var container = FindFirstContainerWithAnyTags(tags);
        return container != null ? container.gameObject : null;
    }

    /// <summary>
    /// Возвращает массив игровых объектов, содержащих все указанные теги.
    /// </summary>
    public static GameObject[] FindGameObjectsWithAllTags(params string[] tags)
    {
        return FindGameObjectsWithAllTags((IEnumerable<string>)tags);
    }

    /// <summary>
    /// Возвращает массив игровых объектов, содержащих все указанные теги.
    /// </summary>
    public static GameObject[] FindGameObjectsWithAllTags(IEnumerable<string> tags)
    {
        var containers = FindContainersWithAllTags(tags);
        var result = new GameObject[containers.Length];
        for (int i = 0; i < containers.Length; i++)
            result[i] = containers[i].gameObject;
        return result;
    }

    /// <summary>
    /// Возвращает первый найденный игровой объект, содержащий все указанные теги, или null.
    /// </summary>
    public static GameObject FindFirstGameObjectWithAllTags(params string[] tags)
    {
        return FindFirstGameObjectWithAllTags((IEnumerable<string>)tags);
    }

    /// <summary>
    /// Возвращает первый найденный игровой объект, содержащий все указанные теги, или null.
    /// </summary>
    public static GameObject FindFirstGameObjectWithAllTags(IEnumerable<string> tags)
    {
        var container = FindFirstContainerWithAllTags(tags);
        return container != null ? container.gameObject : null;
    }

    #endregion

    #region Инициализация / сброс

    /// <summary>
    /// Принудительно перестраивает индексы, находя все MyTagsContainer в сцене.
    /// Этот метод гарантирует, что даже объекты, не вызвавшие Awake/OnEnable,
    /// будут правильно проиндексированы.
    /// </summary>
    /// <param name="includeInactive">Включать ли неактивные объекты в поиск.</param>
    public static void Initialize(bool includeInactive = true)
    {
        // Полностью очищаем индексы
        allContainers.Clear();
        tagToContainers.Clear();

        // Находим все компоненты в сцене
        var containers = Object.FindObjectsByType<MyTagsContainer>(
            includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (var container in containers)
        {
            // Важно: принудительно инициализируем теги из списка,
            // если Awake ещё не был вызван
            if (container.Tags.Count == 0 && container.GetComponent<MyTagsContainer>() != null)
            {
                // Используем рефлексию или внутренний метод для принудительной инициализации
                // Но лучше добавить публичный метод в MyTagsContainer для этой цели
                container.InitializeTagsFromList();
            }

            // Регистрируем контейнер (теперь теги точно есть)
            RegisterContainer(container);
        }

        indexesAreValid = true;
    }

    /// <summary>
    /// Проверяет, были ли индексы построены через Initialize
    /// </summary>
    public static bool IsInitialized => indexesAreValid;

    #endregion
}