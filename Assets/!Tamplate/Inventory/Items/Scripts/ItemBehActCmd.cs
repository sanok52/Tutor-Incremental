using System;
using System.Collections;
using UnityEngine;

public abstract class ItemBehActCmd : CommandBehaviour<ItemBehContext>
{
    public abstract bool NeedUseRoutine { get; }
    public abstract bool NeedUseDropRoutine { get; }

    public virtual void ExecuteDrop(ItemBehContext context)
    {
    }

    public virtual IEnumerator ExecuteRoutineDrop(ItemBehContext context)
    {
        yield break;
    }

    public virtual bool CanUse(ItemBehContext context) => true;

    public virtual string ExecuteReload(string cmd, ItemInstanceInvSys item) { return ""; }
}