using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Компонент, хранящий набор тегов для игрового объекта.
/// </summary>
public class MyTagsContainer : MonoBehaviour
{
    [SerializeField] private List<string> initialTags = new List<string>();
    private HashSet<string> tags = new HashSet<string>();

    public IReadOnlyCollection<string> Tags => tags;

    private void Awake()
    {
        // Инициализируем теги из списка
        foreach (var tag in initialTags)
        {
            if (!string.IsNullOrEmpty(tag))
                tags.Add(tag);
        }

        // Регистрируем себя при создании, даже если объект неактивен
        // Важно для Initialize, который может быть вызван до OnEnable
        MyTagsObserver.RegisterContainer(this);
    }

    private void OnEnable()
    {
        // При включении убеждаемся, что мы зарегистрированы
        MyTagsObserver.RegisterContainer(this);
    }

    private void OnDisable()
    {
        // При отключении удаляем из наблюдателя
        MyTagsObserver.UnregisterContainer(this);
    }

    private void OnDestroy()
    {
        // При уничтожении гарантированно удаляем
        MyTagsObserver.UnregisterContainer(this);
    }

    // Метод для принудительной инициализации тегов (если нужно изменить после Awake)
    internal void InitializeTagsFromList()
    {
        tags.Clear();
        foreach (var tag in initialTags)
        {
            if (!string.IsNullOrEmpty(tag))
                tags.Add(tag);
        }
    }

    #region Проверка наличия тегов

    public bool Contains(string tag) => !string.IsNullOrEmpty(tag) && tags.Contains(tag);
    public bool Any(params string[] tagsToCheck) => tagsToCheck.Any(t => !string.IsNullOrEmpty(t) && tags.Contains(t));
    public bool All(params string[] tagsToCheck) => tagsToCheck.All(t => !string.IsNullOrEmpty(t) && tags.Contains(t));

    #endregion

    #region Динамическое изменение тегов

    public void AddTag(string tag)
    {
        if (string.IsNullOrEmpty(tag) || tags.Contains(tag)) return;
        tags.Add(tag);
        MyTagsObserver.UpdateContainer(this);
    }

    public void RemoveTag(string tag)
    {
        if (string.IsNullOrEmpty(tag) || !tags.Contains(tag)) return;
        tags.Remove(tag);
        MyTagsObserver.UpdateContainer(this);
    }

    public void ClearTags()
    {
        if (tags.Count == 0) return;
        tags.Clear();
        MyTagsObserver.UpdateContainer(this);
    }

    #endregion
}