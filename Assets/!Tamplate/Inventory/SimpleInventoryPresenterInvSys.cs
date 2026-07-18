using System;
using UnityEngine.EventSystems;
/// <summary>
/// Простейшая реализация презентера: левая кнопка – использовать, правая – выбросить.
/// </summary>
public class SimpleInventoryPresenterInvSys : InventoryPresenterBaseInvSys
{
    protected override void HandleSlotClick(int slotIndex, PointerEventData.InputButton button)
    {
        if (Viewer == null) return;

        switch (button)
        {
            case PointerEventData.InputButton.Left:
                Viewer.UseItem(slotIndex);
                break;
            case PointerEventData.InputButton.Right:
                Viewer.DropItem(slotIndex);
                break;
                // При необходимости можно добавить реакцию на среднюю кнопку и т.д.
        }
    }
}