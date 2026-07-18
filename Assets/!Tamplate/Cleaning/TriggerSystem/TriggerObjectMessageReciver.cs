using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerObjectMessageReciver : MonoBehaviour
{
    public List<string> tagsListener = new List<string>();

    // События для самого объекта
    public event Action<TriggerObjectMessageReciver, string[]> OnMessageEnter;
    public event Action<TriggerObjectMessageReciver, string[]> OnMessageStay;
    public event Action<TriggerObjectMessageReciver, string[]> OnMessageExit;

    private List<TriggerForObject> triggerForObjects = new List<TriggerForObject>();

    public void InvokeEnter(TriggerForObject triggerForObject, string[] tags, bool testTags = true)
    {
        if (testTags && !TestAnyTags(tags))
            return;

        OnMessageEnter?.Invoke(this, tags);
        triggerForObjects.Add(triggerForObject);
    }

    public void InvokeStay(string[] tags, bool testTags = true)
    {
        if (testTags && !TestAnyTags(tags))
            return;

        OnMessageStay?.Invoke(this, tags);
    }

    public void InvokeExit(TriggerForObject triggerForObject, string[] tags, bool testTags = true)
    {
        if (testTags && !TestAnyTags(tags))
            return;

        OnMessageExit?.Invoke(this, tags);
        triggerForObjects.Remove(triggerForObject);
    }

    public void ExitAll()
    {
        var tfo = new List<TriggerForObject>(triggerForObjects);
        foreach (var item in tfo)
        {
            if(item != null)
                item.ExitItem(this);
        }
        triggerForObjects.Clear();
    }

    public bool TestAnyTags(params string[] tags) => tagsListener.Any(x => tags.Contains(x));
}
