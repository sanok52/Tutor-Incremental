using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsItemBehContext : CommandContext
{
    public FPSItemInstance ItemInstance { get; private set; }
    public FPSHandsModel Model { get; private set; }
    public bool IsLeft { get; private set; }   // для больших предметов будет false
    public IHItemAnimator ItemAnimator;
    public HandsReceiver Receiver;

    public HandsItemBehContext(FPSItemInstance item, FPSHandsModel model, bool isLeft)
    {
        ItemInstance = item;
        Model = model;
        IsLeft = isLeft;
    }
}

public interface IHandsItemUsed
{
    void ExecuteItem(HandsItemBehContext context);
}

public interface IHandsItemUsedRoutine
{
    IEnumerator ExecuteRoutine(HandsItemBehContext context);
}

public interface IHandsItemUsedHold
{
    void ExecuteEnter(HandsItemBehContext context);
    void ExecuteUpdate(HandsItemBehContext context);
    void ExecuteExit(HandsItemBehContext context);
}

public interface IHandsItemUsedHoldRoutine
{
    IEnumerator ExecuteUpdateRoutine(HandsItemBehContext context);
}

public interface IHandsItemUseReciver
{
    void ExecureReciver(HandsItemBehContext context);
}