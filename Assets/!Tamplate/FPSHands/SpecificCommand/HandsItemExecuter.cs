using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsItemExecuter : MonoBehaviour
{
    [SerializeField] private FPSHandsModel _model;

    // Только для возможности остановки корутин
    private Dictionary<FPSItemInstance, Coroutine> _activeRoutines = new();
    private HandsReceiver Receiver;

    private void OnEnable()
    {
        if (_model == null) return;

        _model.OnItemUsed += HandleItemUsed;
        _model.OnLargeItemUsed += HandleLargeItemUsed;

        _model.OnItemUsedHoldEnter += HandleHoldEnter;
        _model.OnItemUsedHoldUpdate += HandleHoldUpdate;
        _model.OnItemUsedHoldExit += HandleHoldExit;

        _model.OnItemDropped += OnItemLost;
        _model.OnItemStowed += OnItemLost;
        _model.OnLargeItemDropped += OnLargeItemLost;
        _model.OnLargeItemStowed += OnLargeItemLost;
        _model.OnItemInteraction += OnItemInteraction;
        _model.OnLargeItemInteraction += OnLargeItemInteraction;

        _model.OnItemDropped += OnItemDrop;
    }

    private void OnDisable()
    {
        if (_model == null) return;

        _model.OnItemUsed -= HandleItemUsed;
        _model.OnLargeItemUsed -= HandleLargeItemUsed;

        _model.OnItemUsedHoldEnter -= HandleHoldEnter;
        _model.OnItemUsedHoldUpdate -= HandleHoldUpdate;
        _model.OnItemUsedHoldExit -= HandleHoldExit;

        _model.OnItemDropped -= OnItemLost;
        _model.OnItemStowed -= OnItemLost;
        _model.OnLargeItemDropped -= OnLargeItemLost;
        _model.OnLargeItemStowed -= OnLargeItemLost;
        _model.OnItemInteraction -= OnItemInteraction;
        _model.OnLargeItemInteraction -= OnLargeItemInteraction;

        foreach (var routine in _activeRoutines.Values)
            if (routine != null) StopCoroutine(routine);
        _activeRoutines.Clear();
    }

    // ========== Однократное использование (canUse) ==========
    private void HandleItemUsed(FPSItemInstance item, bool isLeft) => ExecuteUse(item, isLeft);
    private void HandleLargeItemUsed(FPSItemInstance item) => ExecuteUse(item, false);

    private void ExecuteUse(FPSItemInstance item, bool isLeft)
    {
        if (item?.Data?.behContainer == null) return;
        var prefabContainer = item.Data.behContainer.GetComponent<HandsItemBehContainer>();
        if (prefabContainer == null) return;

        HandsItemBehContext context = GetContext(item, isLeft);

        prefabContainer.PlayUse(context);          // синхронные поведения

        if (prefabContainer.NeedRoutine())
        {
            // Запускаем корутину на себе, используя готовый метод контейнера
            Coroutine routine = StartCoroutine(RunUseRoutine(prefabContainer, context, item));
            _activeRoutines[item] = routine;
        }
    }

    private HandsItemBehContext GetContext(FPSItemInstance item, bool isLeft)
    {
        var context = new HandsItemBehContext(item, _model, isLeft);
        context.ItemAnimator = item.SourceWorldObject.GetComponent<IHItemAnimator>();

        context.Apply(item.SourceWorldObject.overridebleForContextGO.ToArray());
        context.Apply(item.SourceWorldObject.overridebleForContextTr.ToArray());
        context.Apply(item.SourceWorldObject.overridebleForContextFl.ToArray());
        context.Apply(item.SourceWorldObject.overridebleForContextStr.ToArray());

        return context;
    }

    private IEnumerator RunUseRoutine(HandsItemBehContainer prefabContainer, HandsItemBehContext context, FPSItemInstance item)
    {
        yield return prefabContainer.PlayUseRoutine(context);
        _activeRoutines.Remove(item);
    }

    // ========== Удержание (canUseHold) ==========
    private void HandleHoldEnter(FPSItemInstance item, bool isLeft)
    {
        if (item?.Data?.behContainer == null) return;
        var prefabContainer = item.Data.behContainer.GetComponent<HandsItemBehContainer>();
        if (prefabContainer == null) return;

        var context = GetContext(item, isLeft);
        if (context.ItemAnimator != null)
            context.ItemAnimator.StartUse();

        prefabContainer.PlayUseStartHold(context);

        if (prefabContainer.NeedRoutineHold())
        {
            Coroutine routine = StartCoroutine(RunHoldRoutine(prefabContainer, context, item));
            _activeRoutines[item] = routine;
        }
    }

    private void HandleHoldUpdate(FPSItemInstance item, bool isLeft)
    {
        if (item?.Data?.behContainer == null) return;
        var prefabContainer = item.Data.behContainer.GetComponent<HandsItemBehContainer>();
        if (prefabContainer == null) return;

        var context = GetContext(item, isLeft);
        prefabContainer.PlayUseUpdateHold(context);
    }

    private void HandleHoldExit(FPSItemInstance item, bool isLeft)
    {
        if (item?.Data?.behContainer == null) return;
        var prefabContainer = item.Data.behContainer.GetComponent<HandsItemBehContainer>();
        if (prefabContainer == null) return;

        var context = GetContext(item, isLeft);
        if (context.ItemAnimator != null)
            context.ItemAnimator.EndUse();

        prefabContainer.PlayUseEndHold(context);

        if (_activeRoutines.TryGetValue(item, out var routine))
        {
            if (routine != null) StopCoroutine(routine);
            _activeRoutines.Remove(item);
        }
    }

    private IEnumerator RunHoldRoutine(HandsItemBehContainer prefabContainer, HandsItemBehContext context, FPSItemInstance item)
    {
        yield return prefabContainer.PlayUseRoutine(context);
    }

    // ========== Очистка ==========
    private void OnItemLost(FPSItemInstance item, bool isLeft) => CleanupItem(item);
    private void OnLargeItemLost(FPSItemInstance item) => CleanupItem(item);

    private void OnItemInteraction(FPSItemInstance item, bool isLeft, HandsReceiver receiver)
    {
        if (item?.Data?.behContainer != null)
        {
            var ctx = GetContext(item, isLeft);
            ctx.Receiver = receiver;
            item.Data.behContainer.PlayUseReciver(ctx);
        }

        if (receiver.IsInteractoionTakesObject) CleanupItem(item);
    }

    private void OnLargeItemInteraction(FPSItemInstance item, HandsReceiver receiver)
    {
        if (item?.Data?.behContainer != null)
        {
            var ctx = GetContext(item, true);
            ctx.Receiver = receiver;
            item.Data.behContainer.PlayUseReciver(ctx);
        }

        if (receiver.IsInteractoionTakesObject) CleanupItem(item);
    }

    private void CleanupItem(FPSItemInstance item)
    {
        if (_activeRoutines.TryGetValue(item, out var routine))
        {
            if (routine != null) StopCoroutine(routine);
            _activeRoutines.Remove(item);
        }
    }

    private void OnItemDrop(FPSItemInstance instance, bool isLeft)
    {
        instance.SourceWorldObject.DropWork();
    }
}