using System.Collections.Generic;

public static class ObjectRegistry
{
    private static List<IListable> _items = new List<IListable>();
    public static IReadOnlyList<IListable> Items => _items;

    public static void Register(IListable item)
    {
        if (!_items.Contains(item))
            _items.Add(item);
    }

    public static void Unregister(IListable item)
    {
        _items.Remove(item);
    }

    public static void Clear() => _items.Clear();
}