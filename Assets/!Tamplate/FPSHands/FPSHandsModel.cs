using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HandOccupationPolicy
{
    AutoDropOld,
    StowOld,
    StowIfPossibleElseDrop,
    DropIfPossibleElseStow,
    Prevent
}

public class FPSHandsModel : MonoBehaviour
{
    [Header("Политики")]
    public HandOccupationPolicy largeItemPolicy = HandOccupationPolicy.AutoDropOld;
    public HandOccupationPolicy smallItemPolicy = HandOccupationPolicy.StowIfPossibleElseDrop;
    public HandOccupationPolicy smallItemStackPolicy = HandOccupationPolicy.StowIfPossibleElseDrop;

    [Header("Флаги возможностей")]
    public bool canTakeInDifferentHands = true;
    public bool canStowItems = true;
    public bool canDropItems = true;

    // Текущие предметы в руках
    private List<FPSItemInstance> _leftHandItems = new List<FPSItemInstance>();
    private List<FPSItemInstance> _rightHandItems = new List<FPSItemInstance>();

    // Спрятанные предметы (для каждой руки храним список)
    private List<FPSItemInstance> _stowedLeftItems = new List<FPSItemInstance>();
    private List<FPSItemInstance> _stowedRightItems = new List<FPSItemInstance>();

    // Состояние удержания для каждой руки
    private bool _leftHoldActive;
    private bool _rightHoldActive;

    // Свойства для доступа (возвращают первый предмет или null)
    public FPSItemInstance LeftHandItem => _leftHandItems.FirstOrDefault();
    public FPSItemInstance RightHandItem => _rightHandItems.FirstOrDefault();

    public bool IsHoldActive(bool isLeft) => isLeft ? _leftHoldActive : _rightHoldActive;

    // События для маленьких предметов (isLeft = true для левой руки)
    public event Action<FPSItemInstance, bool, int> OnItemTaken;
    public event Action<FPSItemInstance, bool> OnItemUsed;
    public event Action<FPSItemInstance, bool> OnItemStowed;
    public event Action<FPSItemInstance, bool> OnItemDropped;
    public event Action<FPSItemInstance, bool, HandsReceiver> OnItemInteraction;

    // События для больших предметов
    public event Action<FPSItemInstance> OnLargeItemTaken;
    public event Action<FPSItemInstance> OnLargeItemUsed;
    public event Action<FPSItemInstance> OnLargeItemStowed;
    public event Action<FPSItemInstance> OnLargeItemDropped;
    public event Action<FPSItemInstance, HandsReceiver> OnLargeItemInteraction;

    // Новые события: взаимодействие со всем стеком
    public event Action<FPSItemInstance[], bool, HandsReceiver> OnItemStackInteraction;
    public event Action<FPSItemInstance[], HandsReceiver> OnLargeItemStackInteraction;

    // События для удержания (canUseHold)
    public event Action<FPSItemInstance, bool> OnItemUsedHoldEnter;
    public event Action<FPSItemInstance, bool> OnItemUsedHoldUpdate;
    public event Action<FPSItemInstance, bool> OnItemUsedHoldExit;

    // ========== Взятие ==========
    public bool TakeItem(FPSItemInstance item)
    {
        if (item == null || item.Data == null) return false;
        if (item.Data.isLarge) return TakeLargeItem(item);

        if (CanAddToHand(item, false))
            return AddToHand(item, false);

        if (canTakeInDifferentHands && CanAddToHand(item, true))
            return AddToHand(item, true);

        if (TryResolveHandOccupation(false, item)) return AddToHand(item, false);
        if (canTakeInDifferentHands && TryResolveHandOccupation(true, item)) return AddToHand(item, true);

        return false;
    }

    public bool TakeItemInHand(FPSItemInstance item, bool isLeft)
    {
        if (item == null || item.Data == null || item.Data.isLarge) return false;
        if (!CanAddToHand(item, isLeft))
        {
            if (!TryResolveHandOccupation(isLeft, item))
                return false;
        }
        return AddToHand(item, isLeft);
    }

    private bool TakeLargeItem(FPSItemInstance item)
    {
        if (!ClearHandsForLargeItem()) return false;
        _leftHandItems.Clear();
        _rightHandItems.Clear();
        _leftHandItems.Add(item);
        _rightHandItems.Add(item);
        OnLargeItemTaken?.Invoke(item);
        return true;
    }

    // ========== Использование на месте ==========
    public bool UseItem(bool isLeft)
    {
        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return false;
        FPSItemInstance item = items[0];
        if (!item.Data.canUse) return false;

        if (item.Data.isLarge) OnLargeItemUsed?.Invoke(item);
        else OnItemUsed?.Invoke(item, isLeft);
        return true;
    }

    // ========== Использование с удержанием ==========
    public bool StartUseHold(bool isLeft)
    {
        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return false;
        FPSItemInstance item = items[0];
        if (!item.Data.canUseHold) return false;

        // Если уже удерживаем что-то этой рукой – сначала завершим предыдущее
        if (isLeft && _leftHoldActive) ForceStopHold(true);
        if (!isLeft && _rightHoldActive) ForceStopHold(false);

        if (isLeft) _leftHoldActive = true;
        else _rightHoldActive = true;

        if (item.Data.isLarge)
        {
            // для больших используем один вызов, isLeft игнорируется, передаём false
            OnItemUsedHoldEnter?.Invoke(item, false);
        }
        else
        {
            OnItemUsedHoldEnter?.Invoke(item, isLeft);
        }
        return true;
    }

    public void UpdateUseHold(bool isLeft)
    {
        bool active = isLeft ? _leftHoldActive : _rightHoldActive;
        if (!active) return;

        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0)
        {
            ForceStopHold(isLeft);
            return;
        }
        FPSItemInstance item = items[0];
        if (item.Data.isLarge)
            OnItemUsedHoldUpdate?.Invoke(item, false);
        else
            OnItemUsedHoldUpdate?.Invoke(item, isLeft);
    }

    public void StopUseHold(bool isLeft)
    {
        ForceStopHold(isLeft);
    }

    private void ForceStopHold(bool isLeft)
    {
        bool active = isLeft ? _leftHoldActive : _rightHoldActive;
        if (!active) return;

        if (isLeft) _leftHoldActive = false;
        else _rightHoldActive = false;

        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return; // нечего завершать
        FPSItemInstance item = items[0];
        if (item.Data.isLarge)
            OnItemUsedHoldExit?.Invoke(item, false);
        else
            OnItemUsedHoldExit?.Invoke(item, isLeft);
    }

    // ========== Взаимодействие с HandsReceiver ==========
    public bool InteractWithReceiver(bool isLeft, HandsReceiver receiver)
    {
        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return false;
        FPSItemInstance firstItem = items[0];
        if (!firstItem.Data.canUseInteraction) return false;
        if (receiver == null) return false;

        if (!receiver.TryInteract(firstItem, this)) return false;

        if (firstItem.Data.isLarge)
            OnLargeItemInteraction?.Invoke(firstItem, receiver);
        else
            OnItemInteraction?.Invoke(firstItem, isLeft, receiver);

        if (firstItem.Data.isLarge)
            OnLargeItemStackInteraction?.Invoke(items.ToArray(), receiver);
        else
            OnItemStackInteraction?.Invoke(items.ToArray(), isLeft, receiver);

        if (receiver.IsInteractoionTakesObject)
        {
            // Если предметы забираются, сначала завершаем удержание
            if (isLeft && _leftHoldActive) ForceStopHold(true);
            if (!isLeft && _rightHoldActive) ForceStopHold(false);

            items.Remove(firstItem);
            //items.Clear();
            if (firstItem.Data.isLarge)
            {
                _leftHandItems.Clear();
                _rightHandItems.Clear();
            }
        }
        return true;
    }

    // ========== Выброс ==========
    public bool DropItem(bool isLeft)
    {
        if (!canDropItems) return false;
        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return false;
        if (!items[0].Data.canDrop) return false;

        // Прерываем удержание, если активно
        if (isLeft && _leftHoldActive) ForceStopHold(true);
        if (!isLeft && _rightHoldActive) ForceStopHold(false);

        if (items[0].Data.isLarge)
        {
            _leftHandItems.Clear();
            _rightHandItems.Clear();
            OnLargeItemDropped?.Invoke(items[0]);
        }
        else
        {
            foreach (var item in items)
                OnItemDropped?.Invoke(item, isLeft);
            items.Clear();
        }

        return true;
    }

    // ========== Прятать / Достать ==========
    public bool StowItem(bool isLeft)
    {
        if (!canStowItems) return false;
        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return false;
        if (!items[0].Data.canStow) return false;

        var stowed = isLeft ? _stowedLeftItems : _stowedRightItems;
        if (stowed.Count > 0) return false;

        // Прерываем удержание
        if (isLeft && _leftHoldActive) ForceStopHold(true);
        if (!isLeft && _rightHoldActive) ForceStopHold(false);

        if (items[0].Data.isLarge)
        {
            stowed.AddRange(items);
            _leftHandItems.Clear();
            _rightHandItems.Clear();
            OnLargeItemStowed?.Invoke(items[0]);
        }
        else
        {
            foreach (var item in items)
            {
                stowed.Add(item);
                OnItemStowed?.Invoke(item, isLeft);
            }
            items.Clear();
        }
        return true;
    }

    public bool RetrieveStowedItem(bool isLeft)
    {
        var stowed = isLeft ? _stowedLeftItems : _stowedRightItems;
        if (stowed.Count == 0) return false;
        var targetHand = isLeft ? _leftHandItems : _rightHandItems;
        if (targetHand.Count > 0) return false;

        if (stowed[0].Data.isLarge)
        {
            if (_leftHandItems.Count > 0 || _rightHandItems.Count > 0) return false;
            _leftHandItems.AddRange(stowed);
            _rightHandItems.AddRange(stowed);
            OnLargeItemTaken?.Invoke(stowed[0]);
        }
        else
        {
            foreach (var item in stowed)
            {
                targetHand.Add(item);
                int index = targetHand.Count - 1;
                OnItemTaken?.Invoke(item, isLeft, index);
            }
        }
        stowed.Clear();
        return true;
    }

    // ========== Состояние ==========
    public bool HasItemInHands() => _leftHandItems.Count > 0 || _rightHandItems.Count > 0;
    public bool HasItemInHand(bool isLeft) => isLeft ? _leftHandItems.Count > 0 : _rightHandItems.Count > 0;
    public FPSItemInstance GetHeldItem() => _rightHandItems.FirstOrDefault() ?? _leftHandItems.FirstOrDefault();
    public FPSItemInstance GetItemInHand(bool isLeft) => isLeft ? LeftHandItem : RightHandItem;

    public bool DropHeldItem()
    {
        if (HasItemInHand(false)) return DropItem(false);
        if (HasItemInHand(true)) return DropItem(true);
        return false;
    }

    public bool TryRemoveHeldItem(out FPSItemInstance removedItem, bool isLeft = false)
    {
        removedItem = null;
        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return false;
        removedItem = items[0];
        return DropItem(isLeft);
    }

    // ========== Стекирование ==========
    private bool CanAddToHand(FPSItemInstance newItem, bool isLeft)
    {
        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return true;
        var first = items[0];
        if (!first.Data.canStack) return false;
        if (first.Data != newItem.Data) return false;
        if (items.Count >= first.Data.maxCountInHand) return false;
        return true;
    }

    private bool AddToHand(FPSItemInstance item, bool isLeft)
    {
        var items = isLeft ? _leftHandItems : _rightHandItems;
        items.Add(item);
        int index = items.Count - 1;
        OnItemTaken?.Invoke(item, isLeft, index);
        return true;
    }

    // ========== Политики ==========
    private bool TryResolveHandOccupation(bool isLeft, FPSItemInstance item)
    {
        var items = isLeft ? _leftHandItems : _rightHandItems;
        if (items.Count == 0) return true;
        var first = items[0];
        HandOccupationPolicy policy = first.Data.isLarge ? largeItemPolicy : 
            ((items.Count > 0 && item != null && item.Data.canStack && item.Data == first.Data) ? smallItemStackPolicy: smallItemPolicy);

        switch (policy)
        {
            case HandOccupationPolicy.AutoDropOld: return DropItem(isLeft);
            case HandOccupationPolicy.StowOld: return StowItem(isLeft);
            case HandOccupationPolicy.StowIfPossibleElseDrop:
                if (first.Data.canStow && canStowItems) return StowItem(isLeft);
                if (first.Data.canDrop && canDropItems) return DropItem(isLeft);
                return false;
            case HandOccupationPolicy.DropIfPossibleElseStow:
                if (first.Data.canDrop && canDropItems) return DropItem(isLeft);
                if (first.Data.canStow && canStowItems) return StowItem(isLeft);
                return false;
            case HandOccupationPolicy.Prevent:
                return false;
        }
        return false;
    }

    private bool ClearHandsForLargeItem()
    {
        if (_leftHandItems.Count > 0 && _leftHandItems[0].Data.isLarge)
            return TryResolveHandOccupation(false, null);

        bool leftOk = _leftHandItems.Count == 0 || TryResolveHandOccupation(true, null);
        bool rightOk = _rightHandItems.Count == 0 || TryResolveHandOccupation(false, null);
        return leftOk && rightOk;
    }
}