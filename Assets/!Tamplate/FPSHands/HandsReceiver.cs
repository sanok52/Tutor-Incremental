using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandsReceiver : MonoBehaviour
{
    public List<string> includedTags = new List<string>();
    public List<string> excludedTags = new List<string>();
    public bool IsInteractoionTakesObject = false;
    public Action<HandInteractionInfo> OnInteraction;

    public void Interact(FPSItemInstance instance, FPSHandsModel handsModel)
    {
        OnInteraction?.Invoke(new HandInteractionInfo(this, instance, handsModel));
    }

    public bool TryInteract(FPSItemInstance instance, FPSHandsModel handsModel)
    {
        Debug.Log($"TryInteract");

        if (enabled == false)
            return false;

        if (CanInteract(instance))
        {
            Interact(instance, handsModel);
            return true;
        }
        return false;
    }

    public bool CanInteract(FPSItemInstance instance)
    {
        string[] itemTags = instance.GetTags();

        foreach (string tag in itemTags)
            if (excludedTags.Contains(tag)) return false;

        if (includedTags.Any(x => itemTags.Contains(x)))
            return true;

        return false;
    }
}

public class HandInteractionInfo
{
    public HandsReceiver receiver;
    public FPSItemInstance item;
    public FPSHandsModel handsModel; // добавлено

    public HandInteractionInfo(HandsReceiver receiver, FPSItemInstance item, FPSHandsModel handsModel)
    {
        this.receiver = receiver;
        this.item = item;
        this.handsModel = handsModel;
    }
}
