using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneExecuter : Singleton<SceneExecuter>
{
    [SerializeField] protected LNodeBehContainer defaultEnterBehContainer;
    [SerializeField] protected LNodeBehContainer defaultBehContainer;

    private LabyrinthObjectTransform player;

    [Space]
    [SerializeField] private bool isDontDestroyOnLoad;
    public override bool IsDontDestroyOnLoad => isDontDestroyOnLoad;

    public Action<SceneExecuterContext> OnBehavioursEnter;
    public Action<SceneExecuterContext> OnBehavioursAfterEnterBehaviour;
    public Action<SceneExecuterContext> OnBehavioursEndBeforeDefault;
    public Action<SceneExecuterContext> OnBehavioursEndAfterDefault;
    public Action<LNodeBehContainer, SceneExecuterContext> OnExecuteBehaviourStart;
    public Action<LNodeBehContainer, SceneExecuterContext> OnExecuteBehaviourEnd;
    private SceneExecuterContext globalContext;

    public virtual IEnumerator ExecuteBehaviours(LNodeBehContainer[] behaviourContainers, SceneExecuterContext context)
    {
        OnBehavioursEnter?.Invoke(context);

        yield return ExecuteEnterDefaultBehaviour(context);

        foreach (var behaviourContainer in behaviourContainers)
        {
            if (context.BreakAllNextBehaviour || context.BreakToEndBehaviour)
                break;

            yield return ExecuteBehaviour(behaviourContainer, context);
        }

        yield return new WaitForSeconds(0.25f);

        yield return ExecuteDefaultBehaviour(context);
    }

    public virtual IEnumerator ExecuteEnterDefaultBehaviour(SceneExecuterContext context)
    {
        yield return ExecuteBehaviour(defaultEnterBehContainer, context);
        OnBehavioursAfterEnterBehaviour?.Invoke(context);
    }

    public virtual IEnumerator ExecuteDefaultBehaviour(SceneExecuterContext context)
    {
        OnBehavioursEndBeforeDefault?.Invoke(context);
        yield return ExecuteBehaviour(defaultBehContainer, context);
        OnBehavioursEndAfterDefault?.Invoke(context);
    }

    public virtual IEnumerator ExecuteBehaviour(LNodeBehContainer behaviourContainer, SceneExecuterContext context)
    {
        if (behaviourContainer == null || context.BreakAllNextBehaviour)
            yield break;

        context = ApplyToContext(context);

        LNodeBehContainer container = Instantiate(behaviourContainer, transform);

        OnExecuteBehaviourStart?.Invoke(behaviourContainer, context);

        container.ExecuteAll(context);
        yield return container.ExecuteAllRoutine(context);

        OnExecuteBehaviourEnd?.Invoke(behaviourContainer, context);

        Destroy(container.gameObject);
    }

    public virtual void Stop()
    {
        StopAllCoroutines();
    }

    public SceneExecuterContext GetContext()
    {
        return ApplyToContext(new SceneExecuterContext(globalContext));
    }

    public virtual SceneExecuterContext ApplyToContext(SceneExecuterContext sceneExecuterContext)
    {
        var player = globalContext.PlayerLTransfom;
        var labyrinthModel = globalContext.LabyrinthModel;

        if (player != null)
            sceneExecuterContext.PlayerLTransfom = player;
        if (labyrinthModel)
            sceneExecuterContext.LabyrinthModel = labyrinthModel;

        return sceneExecuterContext;
    }

    public void SetGlobalExecuterContext(SceneExecuterContext sceneExecuterContext)
    {
        globalContext = sceneExecuterContext;
    }
}