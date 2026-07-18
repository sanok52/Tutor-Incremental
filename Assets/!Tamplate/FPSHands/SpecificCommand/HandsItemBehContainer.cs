using System.Collections;

public class HandsItemBehContainer : CommandContainer<HandsItemBehContext>
{
    public void PlayUse(HandsItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            if(behaviour.TryGetComponent(out IHandsItemUsed handsItem))
                handsItem.ExecuteItem(context);
        }
    }

    public void PlayUseStartHold(HandsItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            if (behaviour.TryGetComponent(out IHandsItemUsedHold handsItem))
                handsItem.ExecuteEnter(context);
        }

    }

    public void PlayUseEndHold(HandsItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            if (behaviour.TryGetComponent(out IHandsItemUsedHold handsItem))
                handsItem.ExecuteExit(context);
        }
    }

    public void PlayUseUpdateHold(HandsItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            if (behaviour.TryGetComponent(out IHandsItemUsedHold handsItem))
                handsItem.ExecuteUpdate(context);
        }
    }

    public bool NeedRoutine()
    {
        foreach (var behaviour in behaviours)
        {
            if (behaviour.TryGetComponent(out IHandsItemUsedRoutine usedRoutine))
                return true;
        }
        return false;
    }

    public bool NeedRoutineHold()
    {

        foreach (var behaviour in behaviours)
        {
            if (behaviour.TryGetComponent(out IHandsItemUsedHoldRoutine usedHoldRoutine))
                return true;
        }
        return false;
    }

    public IEnumerator PlayUseUpdateHoldRoutine(HandsItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            if (behaviour.TryGetComponent(out IHandsItemUsedRoutine usedRoutine))
                yield return usedRoutine.ExecuteRoutine(context);
        }
    }

    public IEnumerator PlayUseRoutine(HandsItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            if (behaviour.TryGetComponent(out IHandsItemUsedHoldRoutine usedHoldRoutine))
                yield return usedHoldRoutine.ExecuteUpdateRoutine(context);
        }

    }

    public void PlayUseReciver (HandsItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            if (behaviour.TryGetComponent(out IHandsItemUseReciver usedRoutine))
               usedRoutine.ExecureReciver(context);
        }
    }
}
