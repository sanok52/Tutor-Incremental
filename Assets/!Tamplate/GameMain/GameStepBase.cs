using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public interface IGameStep
{
    string ID { get; }
    int ExecutionOrder { get; }
    bool IsPlayUpdate { get; }
    IEnumerator Execute(PlayGameLoopContext context);
}

public abstract class GameStepBase<T> : IGameStep where T : GameStepBase<T>
{
    public abstract int ExecutionOrder { get; }
    public virtual string ID => GetType().Name;
    public virtual bool IsPlayUpdate => true;

    public virtual IEnumerator Execute(PlayGameLoopContext context)
    {
        PlayStart(context);
        if (IsPlayUpdate)
            yield return PlayUpdate(context);
        PlayEnd(context);
    }

    protected virtual void PlayStart(PlayGameLoopContext context)
    {
        var objects = GameMainPlayer.Instance.GetObjectsForStep<T>();
        foreach (var obj in objects)
            obj.OnStateStart(context);
    }

    protected virtual IEnumerator PlayUpdate(PlayGameLoopContext context)
    {
        var objects = GameMainPlayer.Instance.GetObjectsForStep<T>();
        var objectsRoutine = GameMainPlayer.Instance.GetObjectsForStepRoutine<T>();

        foreach (var obj in objects)
        {
            if (!obj.IsEnable)
                continue;
            obj.OnStateUpdate(context);
        }

        foreach (var obj in objectsRoutine)
        {
            if (!obj.IsEnable)
                continue;
            yield return obj.OnStateUpdate(context);
        }

        yield return new WaitWhile(() => objects.Any(x => !x.IsEndPlay));
    }

    protected virtual void PlayEnd(PlayGameLoopContext context)
    {
        var objects = GameMainPlayer.Instance.GetObjectsForStep<T>();
        foreach (var obj in objects)
            obj.OnStateEnd(context);
    }
}