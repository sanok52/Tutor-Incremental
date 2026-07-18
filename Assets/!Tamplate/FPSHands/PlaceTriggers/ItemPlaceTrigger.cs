using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemPlaceTrigger : MonoBehaviour
{
    public List<string> tagsItem = new List<string>();
    public List<FPSHandsItemObject> itemObjects = new List<FPSHandsItemObject>();
    public event Action<FPSHandsItemObject> OnEnter;
    public event Action<FPSHandsItemObject> OnExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FPSHandsItemObject item))
        {
            if (item.GetTags().Any(x => tagsItem.Contains(x)) && !itemObjects.Contains(item))
            {
                itemObjects.Add(item);
                item.OnGrap += OnItemGrabbed; // подписываемся на взятие предмета
                OnEnter?.Invoke(item);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out FPSHandsItemObject item) && itemObjects.Contains(item))
        {
            RemoveItem(item);
            OnExit?.Invoke(item);
        }
    }

    private void OnItemGrabbed(HandsItemObjectInfo info)
    {
        FPSHandsItemObject item = info.itemInstance.SourceWorldObject;
        if (itemObjects.Contains(item))
        {
            RemoveItem(item);
            OnExit?.Invoke(item);
        }
    }

    private void RemoveItem(FPSHandsItemObject item)
    {
        itemObjects.Remove(item);
        item.OnGrap -= OnItemGrabbed;
    }
}