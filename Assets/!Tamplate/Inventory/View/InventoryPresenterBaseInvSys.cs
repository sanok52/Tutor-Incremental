using System;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Базовый класс для презентеров инвентаря. Автоматически подписывается на клики в InventoryViewerInvSys.
/// </summary>
public abstract class InventoryPresenterBaseInvSys : MonoBehaviour
{
    private bool isBlock;
    public bool IsBlock => isBlock;

    protected InventoryViewerInvSys Viewer { get; private set; }

    protected virtual void Awake()
    {
        Viewer = GetComponent<InventoryViewerInvSys>();
        if (Viewer == null)
        {
            Debug.LogError($"InventoryPresenterBaseInvSys: Не найден InventoryViewerInvSys на объекте {gameObject.name}");
        }
    }

    protected virtual void OnEnable()
    {
        if (Viewer != null)
            Viewer.OnSlotClicked += TryHandleSlotClick;
    }

    protected virtual void OnDisable()
    {
        if (Viewer != null)
            Viewer.OnSlotClicked -= TryHandleSlotClick;
    }

    /// <summary>
    /// Обработчик клика по слоту. Должен быть реализован в наследниках.
    /// </summary>
    /// <param name="slotIndex">Индекс слота</param>
    /// <param name="button">Какая кнопка мыши нажата</param>
    public void TryHandleSlotClick(int slotIndex, PointerEventData.InputButton button)
    {
        if (Viewer == null || isBlock)
            return;

        if (!CanHandleSlotClick(slotIndex, button))
            return;

        HandleSlotClick(slotIndex, button);
    }

    public virtual bool CanHandleSlotClick(int slotIndex, PointerEventData.InputButton button) => true;

    protected abstract void HandleSlotClick(int slotIndex, PointerEventData.InputButton button);

    public void SetBlock(bool isBlock)
    {
        this.isBlock = isBlock;
        Viewer.UpdateBlockVisual(isBlock);
    }
}