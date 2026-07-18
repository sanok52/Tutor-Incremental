using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Компонент на сцене, связывающий модель инвентаря с его визуальным отображением.
/// Теперь транслирует клики по слотам через событие OnSlotClicked.
/// </summary>
public class InventoryViewerInvSys : MonoBehaviour, IInventoryViewerInvSys
{
    [SerializeField] private InventoryObjectViewInvSys[] _slotViews;
    private InventoryInvSys _inventory;

    /// <summary>
    /// Событие, вызываемое при клике на любой слот. Параметры: индекс слота, нажатая кнопка мыши.
    /// </summary>
    public event System.Action<int, PointerEventData.InputButton> OnSlotClicked;

    public void Initialize(InventoryInvSys inventory)
    {
        _inventory = inventory;
        _inventory.OnSlotChanged += OnSlotModelChanged;
        SetItems(_inventory);
    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnSlotChanged -= OnSlotModelChanged;
    }

    private void OnSlotModelChanged(int index, ItemInstanceInvSys item)
    {
        if (index >= 0 && index < _slotViews.Length)
        {
            if (item == null)
                _slotViews[index].SetEmpty();
            else
            {
                _slotViews[index].Init(item.Data, index);
                _slotViews[index].UpdateCount(item.Count);
                if (item.RemainingUses >= 0)
                    _slotViews[index].UpdateBlockUse(item.RemainingUses == 0);
            }
        }
    }

    public void Clear()
    {
        foreach (var view in _slotViews)
            view.SetEmpty();
    }

    public void SetItems(InventoryInvSys inventory)
    {
        for (int i = 0; i < _slotViews.Length; i++)
        {
            var item = inventory.GetItem(i);
            if (item != null)
            {
                _slotViews[i].Init(item.Data, i);
                _slotViews[i].UpdateCount(item.Count);
            }
            else
            {
                _slotViews[i].SetEmpty();
            }
        }
    }

    public void AddItemVisual(int slotIndex, ItemInstanceInvSys item)
    {
        if (slotIndex < 0 || slotIndex >= _slotViews.Length) return;
        _slotViews[slotIndex].Init(item.Data, slotIndex);
        _slotViews[slotIndex].UpdateCount(item.Count);
    }

    public void DropItem(int slotIndex) => _inventory?.DropItem(slotIndex);
    public void UseItem(int slotIndex) => _inventory?.UseItem(slotIndex);
    public void DeleteItem(int slotIndex) => _inventory?.DeleteItem(slotIndex);

    public void SwapItems(int indexA, int indexB) => _inventory?.SwapSlots(indexA, indexB);

    /// <summary>
    /// Подписывает все слоты на передачу кликов в событие OnSlotClicked.
    /// </summary>
    private void Start()
    {
        for (int i = 0; i < _slotViews.Length; i++)
        {
            int idx = i;
            _slotViews[i].RegisterClickCallback((slotIndex, button) =>
            {
                OnSlotClicked?.Invoke(slotIndex, button);
            });
        }
    }

    public void UpdateBlockVisual(bool isBlock)
    {
        foreach (var item in _slotViews)
        {
            item.SetBlock(isBlock);
        }
    }
}