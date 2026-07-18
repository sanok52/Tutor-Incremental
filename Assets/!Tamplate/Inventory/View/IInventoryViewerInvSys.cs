
/// <summary>
/// Интерфейс, который должно реализовать любое представление инвентаря.
/// </summary>
public interface IInventoryViewerInvSys
{
    void Clear();
    void SetItems(InventoryInvSys inventory);          // полное обновление из модели
    void AddItemVisual(int slotIndex, ItemInstanceInvSys item);
    void DropItem(int slotIndex);                     // вызвано UI (выбросить)
    void UseItem(int slotIndex);                      // вызвано UI (использовать)
    void DeleteItem(int slotIndex);                   // вызвано UI (удалить)
    void SwapItems(int indexA, int indexB);           // вызвано UI (поменять местами)
}