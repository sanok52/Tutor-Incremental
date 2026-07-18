using System;
/// <summary>
/// Основная логика инвентаря. Хранит массив слотов.
/// </summary>
public class InventoryInvSys
{
    private InventorySlotInvSys[] _slots;
    private bool _allowStacking; // глобальный флаг: можно ли стакать предметы в слотах

    // События для оповещения UI
    public Action<int, ItemInstanceInvSys> OnSlotChanged;    // index, новый предмет в слоте (или null если очищен)
    public Action<ItemDataInvSys> OnItemUsed;                // предмет был использован
    public Action<ItemDataInvSys> OnItemDropped;             // предмет был выброшен (удалён из инвентаря без использования)
    public Action<ItemDataInvSys> OnItemAdded;

    public int Capacity => _slots.Length;
    public InventorySlotInvSys[] Slots => _slots;

    public InventoryInvSys(int capacity, bool allowStacking = true)
    {
        _allowStacking = allowStacking;
        _slots = new InventorySlotInvSys[capacity];
        for (int i = 0; i < capacity; i++)
            _slots[i] = new InventorySlotInvSys();
    }

    /// <summary>
    /// Попытаться добавить предмет в инвентарь.
    /// </summary>
    /// <param name="itemData">Конфиг предмета</param>
    /// <param name="count">Количество</param>
    /// <returns>true, если весь запрошенный объём поместился</returns>
    public bool AddItem(ItemDataInvSys itemData, int count = 1)
    {
        int remaining = count;

        // 1. Пытаемся доложить в уже существующие стеки (если разрешено стакание и предмет стакается)
        if (_allowStacking && itemData.CanStack)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].IsEmpty) continue;
                if (_slots[i].Item.Data == itemData)
                {
                    _slots[i].Item.AddToStack(remaining);
                    OnSlotChanged?.Invoke(i, _slots[i].Item);
                    return true; // предположим, что места всегда хватает в стеке (нет лимита на стек)
                }
            }
        }

        // 2. Ищем первый пустой слот
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].IsEmpty)
            {
                ItemInstanceInvSys newItem = new ItemInstanceInvSys(itemData);
                // если разрешено стакание, кладём сразу все оставшиеся
                if (_allowStacking && itemData.CanStack)
                {
                    newItem = new ItemInstanceInvSys(itemData);
                    newItem.AddToStack(remaining - 1); // уже 1 есть в конструкторе
                    _slots[i].SetItem(newItem);
                    OnSlotChanged?.Invoke(i, newItem);
                    OnItemAdded?.Invoke(newItem.Data);
                    return true;
                }
                else
                {
                    // без стакания – кладём 1 штуку
                    _slots[i].SetItem(newItem);
                    OnSlotChanged?.Invoke(i, newItem);
                    OnItemAdded?.Invoke(newItem.Data);
                    remaining--;
                    if (remaining == 0) return true;
                }
            }
        }

        return remaining == 0; // если не хватило места
    }

    /// <summary>
    /// Выбросить предмет (удалить из инвентаря). Срабатывает событие OnItemDropped.
    /// </summary>
    public bool DropItem(int slotIndex, int count = 1)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length || _slots[slotIndex].IsEmpty)
            return false;

        var slot = _slots[slotIndex];
        ItemDataInvSys itemData = slot.Item.Data;
        bool completelyRemoved = false;

        if (!itemData.CanDrop)
            return false;

        ItemMediatorInvSys.ExecuteDrop(itemData.Behaviour, slot.Item);
        if (itemData.Behaviour != null && itemData.Behaviour.NeedUseDropRoutine)
            ItemMediatorInvSys.ExecuteDropAllRoutine(itemData.Behaviour, slot.Item);

        if (slot.Item.Count <= count)
        {
            slot.Clear();
            completelyRemoved = true;
        }
        else
        {
            slot.Item.RemoveFromStack(count);
        }

        OnSlotChanged?.Invoke(slotIndex, slot.Item);
        OnItemDropped?.Invoke(itemData);
        return true;
    }

    /// <summary>
    /// Перегрузка DropItem по IDSpiker предмета (удаляет первый найденный стек).
    /// </summary>
    public bool DropItem(string itemId, int count = 1)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (!_slots[i].IsEmpty && _slots[i].Item.Data.CanDrop && _slots[i].Item.Data.ItemId == itemId)
                return DropItem(i, count);
        }
        return false;
    }

    /// <summary>
    /// Перегрузка DropItem по ссылке на данные предмета.
    /// </summary>
    public bool DropItem(ItemDataInvSys itemData, int count = 1)
    {
        return DropItem(itemData.ItemId, count);
    }

    /// <summary>
    /// Использовать предмет из слота.
    /// </summary>
    /// <param name="slotIndex">Индекс слота</param>
    /// <returns>true, если использование удалось</returns>
    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length || _slots[slotIndex].IsEmpty)
            return false;

        ItemInstanceInvSys item = _slots[slotIndex].Item;
        ItemDataInvSys data = item.Data;
        ItemBehContrainerInvSys behaviour = data.Behaviour;

        if (behaviour == null || !data.CanUse || (item.Data.CanUse && item.RemainingUsesMax > 0 && item.RemainingUses <= 0) || !ItemMediatorInvSys.CanUse(behaviour, item))
            return false;

        // Выполняем использование
        ItemMediatorInvSys.Execute(behaviour, item);
        if(behaviour.NeedUseRoutine)
            ItemMediatorInvSys.ExecuteAllRoutine(behaviour, item);

        OnItemUsed?.Invoke(data);

        // Логика удаления / расхода использований
        bool shouldRemove = false;

        if (data.DeleteAfterUse)
        {
            shouldRemove = true;
        }
        else
        {
            // Уменьшаем счётчик использований, если есть лимит
            shouldRemove = item.UseOne(); // UseOne возвращает true, если предмет исчерпан
        }

        if (shouldRemove && item.Data.DeleteAfterUse)
        {
            if (item.Count > 1)
            {
                item.RemoveFromStack(1);
                // Если стек ещё остался, но лимит использований у отдельного экземпляра может быть исчерпан?
                // При стакании предметов с лимитом использований не должно быть, но для безопасности:
                OnSlotChanged?.Invoke(slotIndex, item.Count > 0 ? item : null);
                if (item.Count == 0)
                    _slots[slotIndex].Clear();
            }
            else
            {
                _slots[slotIndex].Clear();
                OnSlotChanged?.Invoke(slotIndex, null);
            }
        }
        else
        {
            // Предмет остался, но мог измениться RemainingUses – уведомляем
            OnSlotChanged?.Invoke(slotIndex, item);
        }

        return true;
    }

    /// <summary>
    /// Использовать предмет по IDSpiker (первый найденный).
    /// </summary>
    public bool UseItem(string itemId)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (!_slots[i].IsEmpty && _slots[i].Item.Data.ItemId == itemId)
                return UseItem(i);
        }
        return false;
    }

    /// <summary>
    /// Использовать предмет по ссылке на данные.
    /// </summary>
    public bool UseItem(ItemDataInvSys itemData)
    {
        return UseItem(itemData.ItemId);
    }

    /// <summary>
    /// Удалить предмет (полная очистка слота без выброса события дропа).
    /// </summary>
    public bool DeleteItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length || _slots[slotIndex].IsEmpty)
            return false;

        _slots[slotIndex].Clear();
        OnSlotChanged?.Invoke(slotIndex, null);
        return true;
    }

    public bool DeleteItem(string itemId)
    {
        for (int i = 0; i < _slots.Length; i++)
            if (!_slots[i].IsEmpty && _slots[i].Item.Data.ItemId == itemId)
                return DeleteItem(i);
        return false;
    }

    public bool DeleteItem(ItemDataInvSys itemData) => DeleteItem(itemData.ItemId);

    /// <summary>
    /// Поменять два слота местами.
    /// </summary>
    public void SwapSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= _slots.Length || indexB < 0 || indexB >= _slots.Length)
            return;

        var temp = _slots[indexA].Item;
        _slots[indexA].SetItem(_slots[indexB].Item);
        _slots[indexB].SetItem(temp);

        OnSlotChanged?.Invoke(indexA, _slots[indexA].Item);
        OnSlotChanged?.Invoke(indexB, _slots[indexB].Item);
    }

    /// <summary>
    /// Получить предмет в слоте (может быть null).
    /// </summary>
    public ItemInstanceInvSys GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length) return null;
        return _slots[slotIndex].Item;
    }

    public string CommandToAllItem(string cmd)
    {
        string displyText = string.Empty;
        foreach (var slot in Slots)
        {
            if (!slot.IsEmpty)
                displyText += slot.Item.CommandAll(cmd);
        }
        return displyText;
    }

    public void UpdateSlotsInfo()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            InventorySlotInvSys slot = Slots[i];
            OnSlotChanged?.Invoke(i, slot.Item);
        }
    }
}