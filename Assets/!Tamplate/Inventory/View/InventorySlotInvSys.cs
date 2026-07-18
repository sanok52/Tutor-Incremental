
/// <summary>
/// Один слот инвентаря. Может быть пустым или содержать ItemInstanceInvSys.
/// </summary>
public class InventorySlotInvSys
{
    public ItemInstanceInvSys Item { get; private set; }
    public bool IsEmpty => Item == null;
    public int Counts = -1;
    public bool CanUse => Counts == -1 || Counts > 0;

    /// <summary>
    /// Устанавливает предмет в слот. Если в слоте уже что-то есть – заменяет.
    /// </summary>
    public void SetItem(ItemInstanceInvSys item)
    {
        Item = item;
    }

    /// <summary>
    /// Очищает слот.
    /// </summary>
    public void Clear()
    {
        Item = null;
    }
}