using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UniversalContainerRunner : MonoBehaviour
{
    public GameObject prefContainer;
    private GameObject container;
    public GameObject Container
    {
        get 
        {
            if (!container)
                container = Instantiate(prefContainer, transform);
            return container;
        }
    }

    Dictionary<Type, Component> cachedContainers = new Dictionary<Type, Component>();
    public CommandContainer<C> GetContainer<C>() where C : CommandContext
    {
        if(cachedContainers.ContainsKey(typeof(C)))
            return cachedContainers[typeof(C)] as CommandContainer<C>;

        var cmdContainer = Container.GetComponent<CommandContainer<C>>();
        if (cmdContainer == null)
        {
            Debug.LogError($"Prefab {prefContainer.name} does not have a CommandContainer<{typeof(C).Name}> component.");
            return null;
        }

        cachedContainers.Add(typeof(C), cmdContainer);
        return cmdContainer;
    }

    public void Execute<C>(C context) where C : CommandContext
    {
        CommandContainer<C> container = GetContainer<C>();
        if (container == null)
            return;

        container.ExecuteAll(context);
    }

    public IEnumerator ExecuteAllRoutine<C>(C context, bool waitEach = true) where C : CommandContext
    {
        CommandContainer<C> container = GetContainer<C>();
        if (container == null)
            yield break;

        yield return container.ExecuteAllRoutine(context, waitEach);
    }
}