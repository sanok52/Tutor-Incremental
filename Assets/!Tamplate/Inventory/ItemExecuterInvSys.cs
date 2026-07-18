
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemEventType
{
    None, BeforeUse, AfterUse, BeforeDrop, AfterDrop
}

[DefaultExecutionOrder(-1)]
public class ItemExecuterInvSys : Singleton<ItemExecuterInvSys>
{
    public override bool IsDontDestroyOnLoad => false;

    public List<IItemEventListener> EventListeners { get; } = new List<IItemEventListener>();
    private Dictionary<ItemEventType, List<IItemEventListener>> _cachedListeners = new Dictionary<ItemEventType, List<IItemEventListener>>();

    public static bool IsExecuting { get; private set; } // Флаг, указывающий, выполняется ли сейчас использование предмета
    public static bool IsDropping { get; private set; } // Флаг, указывающий, выполняется ли сейчас выбрасывание предмета

    private void Awake()
    {
        ItemEventType[] eventTypes = Utils.GetAllEnumArray<ItemEventType>();
        foreach (var eventType in eventTypes)
            _cachedListeners[eventType] = new List<IItemEventListener>();
    }

    public void Subscribe(IItemEventListener listener)
    {
        if (EventListeners.Contains(listener))
            return;

        EventListeners.Add(listener);

        ItemEventType[] eventTypes = Utils.GetAllEnumArray<ItemEventType>();
        foreach (var eventType in eventTypes)
        {
            if (!listener.NeedEventRoutine(eventType, null))
                continue;
            if (!_cachedListeners[eventType].Contains(listener))
                _cachedListeners[eventType].Add(listener);
        }
    }

    public void Unsubscribe(IItemEventListener listener)
    {
        if (!EventListeners.Contains(listener))
            return;

        EventListeners.Remove(listener);

        foreach (var keyValuePair in _cachedListeners)
        {
            if(keyValuePair.Value.Contains(listener))
                keyValuePair.Value.Remove(listener);
        }
    }

    public void Execute(ItemBehContrainerInvSys behaviour, ItemBehContext context)
    {
        StartCoroutine(ExecuteRoutine(behaviour, context));
    }

    public void ExecuteDrop(ItemBehContrainerInvSys behaviour, ItemBehContext itemBehContext)
    {
        StartCoroutine(ExecuteDropRoutine(behaviour, itemBehContext));
    }

    public IEnumerator ExecuteRoutine(ItemBehContrainerInvSys behaviour, ItemBehContext context)
    {
        IsExecuting = true;

        yield return InvokeEventRoutine(ItemEventType.BeforeUse, behaviour, context);

        if (behaviour.NeedUseRoutine)
            yield return behaviour.ExecuteAllRoutine(context);

        yield return InvokeEventRoutine(ItemEventType.AfterUse, behaviour, context);

        IsExecuting = false;
    }

    public IEnumerator ExecuteDropRoutine(ItemBehContrainerInvSys behaviour, ItemBehContext itemBehContext)
    {
        IsDropping = true;
        yield return InvokeEventRoutine(ItemEventType.BeforeDrop, behaviour, itemBehContext);
        if (behaviour.NeedUseDropRoutine)
            yield return behaviour.ExecuteRoutineDrop(itemBehContext);
        yield return InvokeEventRoutine(ItemEventType.AfterDrop, behaviour, itemBehContext);
        IsDropping = false;
    }

    private IEnumerator InvokeEventRoutine(ItemEventType beforeUse, ItemBehContrainerInvSys behaviour, ItemBehContext context)
    {
        //Debug.Log($"InvokeEventRoutine beforeUse ({EventListeners.Count})");
        foreach (var listener in EventListeners)
        {
            if(listener.MyItemInstance == null || listener.MyItemInstance != context.ItemInstance) //Нельзя реагировать на события, от своих же действий
                yield return listener.OnItemEvent(beforeUse, context);
        }
    }
}

public interface IItemEventListener
{
    bool NeedEventRoutine(ItemEventType eventType, ItemBehContext context);
    IEnumerator OnItemEvent(ItemEventType eventType, ItemBehContext context);
    ItemInstanceInvSys MyItemInstance { get; }
}