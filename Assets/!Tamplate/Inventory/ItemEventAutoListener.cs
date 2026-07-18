using System.Collections.Generic;
using UnityEngine;

public class ItemEventAutoListener : MonoBehaviour
{
    private List<IItemEventListener> itemEventListeners = new List<IItemEventListener>();

    public void Awake()
    {
        GetAllListenersFromComponents();
        foreach (var listener in itemEventListeners)
            ItemExecuterInvSys.Instance.Subscribe(listener);
    }

    private void OnDestroy()
    {
        foreach (var listener in itemEventListeners)
            ItemExecuterInvSys.Instance.Unsubscribe(listener);
    }

    private void GetAllListenersFromComponents()
    {
        Component[] components = GetComponents<Component>();
        foreach (var component in components)
        {
            if (component is IItemEventListener listener)
            {
                itemEventListeners.Add(listener);
            }
        }
    }
}