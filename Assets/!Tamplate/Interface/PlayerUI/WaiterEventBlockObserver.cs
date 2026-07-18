using System;
using System.Collections;
using UnityEngine;

public class WaiterEventBlockObserver : MonoBehaviour, IItemEventListener
{
    private IWaiterEventListener[] waiterEvents;

    public ItemInstanceInvSys MyItemInstance => null;

    private void Start()
    {
        waiterEvents = GetComponents<IWaiterEventListener>();
        ItemExecuterInvSys.Instance.Subscribe(this);
        GlobalBlock.OnBlockChanged += SetBlock;
    }

    public bool NeedEventRoutine(ItemEventType eventType, ItemBehContext context)
    {
        return eventType != ItemEventType.None;
    }

    public IEnumerator OnItemEvent(ItemEventType eventType, ItemBehContext context)
    {
        if (eventType == ItemEventType.BeforeUse || eventType == ItemEventType.BeforeDrop)
           SetBlock(true);
        else
           SetBlock(false);

        yield break;
    }

    private void SetBlock(bool isBlock)
    {
        foreach (var waiterEvent in waiterEvents)
            waiterEvent.WaiterBlock(isBlock);
    }
}

public interface IWaiterEventListener
{
   void WaiterBlock(bool isBlock);
}
