using System;
using System.Collections.Generic;
using UnityEngine;

public partial class ItemPlaceTriggers : MonoBehaviour
{
    [SerializeField] private ItemPlaceTrigger[] itemPlaceTriggers;
    [SerializeField] private bool lockIfAllInside;
    [SerializeField] private bool lockIfCountInside;
    [SerializeField] private int countToLock = 4;

    [Space]
    [SerializeField] private GameObject vfxIsLock;

    public bool isWork = true;

    public event Action OnLock;
    public event Action<FPSHandsItemObject> OnEnterItem;
    public event Action<FPSHandsItemObject> OnExitItem;

    void Start()
    {
        foreach (var trigger in itemPlaceTriggers)
        {
            trigger.OnEnter += OnEnter;
            trigger.OnExit += OnExit;
        }
    }

    private void OnEnter(FPSHandsItemObject item)
    {
        OnEnterItem?.Invoke(item);
        OnChangeTriggers(item);
    }


    private void OnExit(FPSHandsItemObject item)
    {
        OnExitItem?.Invoke(item);
        OnChangeTriggers(item);
    }

    private void OnChangeTriggers(FPSHandsItemObject obj = null)
    {
        if (!isWork) return;

        if (lockIfAllInside)
            LockIfAllInside();

        if (lockIfCountInside)
            LockIfCountInside();
    }

    /// <summary>
    /// Возвращает все предметы, находящиеся в любом из триггеров.
    /// </summary>
    public FPSHandsItemObject[] GetItems()
    {
        List<FPSHandsItemObject> allItems = new List<FPSHandsItemObject>();
        foreach (var trigger in itemPlaceTriggers)
        {
            allItems.AddRange(trigger.itemObjects);
        }
        return allItems.ToArray();
    }

    private void LockIfAllInside()
    {
        foreach (var trigger in itemPlaceTriggers)
        {
            if (trigger.itemObjects.Count == 0)
                return;
        }

        foreach (var trigger in itemPlaceTriggers)
        {
            foreach (var item in trigger.itemObjects)
            {
                LockItemObject(item);
            }
        }

        LockMy(); // добавлено, чтобы отключить систему и показать VFX
    }

    private void LockIfCountInside()
    {
        int totalCount = 0;
        foreach (var trigger in itemPlaceTriggers)
        {
            totalCount += trigger.itemObjects.Count;
            if (totalCount >= countToLock) break;
        }

        if (totalCount >= countToLock)
        {
            foreach (var trigger in itemPlaceTriggers)
            {
                foreach (var item in trigger.itemObjects)
                {
                    LockItemObject(item);
                }
            }

            LockMy();
        }
    }

    private void LockMy()
    {
        isWork = false;
        CreateVFX();
        OnLock?.Invoke();
    }

    public void LockItemObject(FPSHandsItemObject item)
    {
        if (item == null)
            return;

        if (item.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
        item.enabled = false;
    }

    public void CreateVFX()
    {
        if (vfxIsLock != null)
        {
            var items = GetItems();
            foreach (var item in items)
            {
                if(item != null)
                    Instantiate(vfxIsLock, item.transform.position, Quaternion.identity);
            }
        }
    }
}