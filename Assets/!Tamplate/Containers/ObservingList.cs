using System;
using System.Collections.Generic;
using System.Linq;

public class ObservingList<T>
{
    private List<T> list = new List<T>();
    public IReadOnlyList<T> List => list;

    // Базовые события
    public event Action<T[]> ItemAdded;
    public event Action<T[]> ItemRemoved;

    // События изменения размера
    public event Action OnListEmpty;      // если список стал пустым
    public event Action OnListNotEmpty;   // если список стал непустым (был пуст, добавили элемент)

    // Дополнительные события
    public event Action<T, T> ItemReplaced;   // oldItem, newItem
    public event Action ListCleared;
    public event Action ListChanged;          // при сортировке, реверсе

    /// <summary>Добавляет один или несколько элементов. Вызывает ItemAdded и при необходимости OnListNotEmpty.</summary>
    public void Add(params T[] items)
    {
        if (items.Length == 0) return;

        bool wasEmpty = list.Count == 0;

        if (items.Length == 1)
            list.Add(items[0]);
        else
            list.AddRange(items);

        ItemAdded?.Invoke(items);
        if (wasEmpty && list.Count > 0)
            OnListNotEmpty?.Invoke();
    }

    /// <summary>
    /// Добавляет элемент с возможной проверкой на дубликат.
    /// Если <paramref name="notRepeat"/> == true и элемент уже есть в списке – добавления не происходит.
    /// </summary>
    public void Add(T item, bool notRepeat = false)
    {
        if (notRepeat && list.Contains(item))
            return;
        Add(item); // вызовет основное Add с проверкой пустоты и событиями
    }

    /// <summary>Удаляет указанные элементы. Вызывает ItemRemoved и при необходимости OnListEmpty.</summary>
    public void Remove(params T[] items)
    {
        if (items.Length == 0) return;

        bool wasNotEmpty = list.Count > 0;

        foreach (var item in items)
            list.Remove(item);

        ItemRemoved?.Invoke(items);
        if (wasNotEmpty && list.Count == 0)
            OnListEmpty?.Invoke();
    }

    /// <summary>Вставляет элемент по индексу. Вызывает ItemAdded и OnListNotEmpty, если список был пуст.</summary>
    public void Insert(int index, T item)
    {
        bool wasEmpty = list.Count == 0;
        list.Insert(index, item);
        ItemAdded?.Invoke(new T[] { item });
        if (wasEmpty)
            OnListNotEmpty?.Invoke();
    }

    /// <summary>Вставляет несколько элементов начиная с индекса. Вызывает ItemAdded и OnListNotEmpty при необходимости.</summary>
    public void InsertRange(int index, IEnumerable<T> items)
    {
        var arr = items.ToArray();
        if (arr.Length == 0) return;

        bool wasEmpty = list.Count == 0;
        list.InsertRange(index, arr);
        ItemAdded?.Invoke(arr);
        if (wasEmpty && list.Count > 0)
            OnListNotEmpty?.Invoke();
    }

    /// <summary>Удаляет элемент по индексу. Вызывает ItemRemoved и OnListEmpty, если список опустел.</summary>
    public void RemoveAt(int index)
    {
        bool wasNotEmpty = list.Count > 0;
        T item = list[index];
        list.RemoveAt(index);
        ItemRemoved?.Invoke(new T[] { item });
        if (wasNotEmpty && list.Count == 0)
            OnListEmpty?.Invoke();
    }

    /// <summary>Очищает список. Вызывает ItemRemoved, ListCleared и OnListEmpty.</summary>
    public void Clear()
    {
        if (list.Count == 0) return;

        var removed = list.ToArray();
        bool wasNotEmpty = true; // гарантированно не пуст
        list.Clear();

        ItemRemoved?.Invoke(removed);
        ListCleared?.Invoke();
        OnListEmpty?.Invoke(); // после очистки стал пустым
    }

    /// <summary>Заменяет элемент по индексу. Вызывает ItemReplaced. События пустоты не требуются.</summary>
    public void Replace(int index, T newItem)
    {
        T oldItem = list[index];
        list[index] = newItem;
        ItemReplaced?.Invoke(oldItem, newItem);
    }

    /// <summary>Индексатор. При установке вызывает Replace.</summary>
    public T this[int index]
    {
        get => list[index];
        set => Replace(index, value);
    }

    /// <summary>Проверяет наличие элемента.</summary>
    public bool Contains(T item) => list.Contains(item);

    /// <summary>Возвращает индекс элемента или -1.</summary>
    public int IndexOf(T item) => list.IndexOf(item);

    /// <summary>Количество элементов.</summary>
    public int Count => list.Count;

    /// <summary>Сортирует с использованием стандартного компаратора. Вызывает ListChanged.</summary>
    public void Sort()
    {
        list.Sort();
        ListChanged?.Invoke();
    }

    /// <summary>Сортирует с указанным сравнением. Вызывает ListChanged.</summary>
    public void Sort(Comparison<T> comparison)
    {
        list.Sort(comparison);
        ListChanged?.Invoke();
    }

    /// <summary>Переворачивает порядок элементов. Вызывает ListChanged.</summary>
    public void Reverse()
    {
        list.Reverse();
        ListChanged?.Invoke();
    }

    /// <summary>Возвращает копию элементов в виде массива.</summary>
    public T[] ToArray() => list.ToArray();
}