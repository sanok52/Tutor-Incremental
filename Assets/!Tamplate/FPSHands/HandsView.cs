using UnityEngine;

public abstract class HandsView : MonoBehaviour
{
    [SerializeField] protected FPSHandsModel _model;
    [SerializeField] protected Transform _dropPoint;

    public Transform GetDropPoint() => _dropPoint;

    protected virtual void OnEnable()
    {
        if (_model == null) return;
        _model.OnItemTaken += HandleItemTaken;
        _model.OnItemDropped += HandleItemDropped;
        _model.OnItemStowed += HandleItemStowed;
        _model.OnLargeItemTaken += HandleLargeItemTaken;
        _model.OnLargeItemDropped += HandleLargeItemDropped;
        _model.OnLargeItemStowed += HandleLargeItemStowed;
        _model.OnItemInteraction += HandleItemInteraction;
        _model.OnLargeItemInteraction += HandleLargeItemInteraction;
    }

    protected virtual void OnDisable()
    {
        if (_model == null) return;
        _model.OnItemTaken -= HandleItemTaken;
        _model.OnItemDropped -= HandleItemDropped;
        _model.OnItemStowed -= HandleItemStowed;
        _model.OnLargeItemTaken -= HandleLargeItemTaken;
        _model.OnLargeItemDropped -= HandleLargeItemDropped;
        _model.OnLargeItemStowed -= HandleLargeItemStowed;
        _model.OnItemInteraction -= HandleItemInteraction;
        _model.OnLargeItemInteraction -= HandleLargeItemInteraction;
    }

    // Обновлённая сигнатура с индексом
    protected virtual void HandleItemTaken(FPSItemInstance item, bool isLeft, int index) { }
    protected virtual void HandleItemDropped(FPSItemInstance item, bool isLeft) { }
    protected virtual void HandleItemStowed(FPSItemInstance item, bool isLeft) { }
    protected virtual void HandleLargeItemTaken(FPSItemInstance item) { }
    protected virtual void HandleLargeItemDropped(FPSItemInstance item) { }
    protected virtual void HandleLargeItemStowed(FPSItemInstance item) { }
    protected virtual void HandleItemInteraction(FPSItemInstance item, bool isLeft, HandsReceiver receiver) { }
    protected virtual void HandleLargeItemInteraction(FPSItemInstance item, HandsReceiver receiver) { }
}