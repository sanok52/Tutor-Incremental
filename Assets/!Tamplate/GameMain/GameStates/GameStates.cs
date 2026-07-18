using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class StorehouseGameState : GameStepBase<StorehouseGameState>
{
    public override int ExecutionOrder => 0;

    public override IEnumerator Execute(PlayGameLoopContext context)
    {
        yield return base.Execute(context);
    }
}

public class AuctionGameState : GameStepBase<AuctionGameState>
{
    public override int ExecutionOrder => 5;

    public override IEnumerator Execute(PlayGameLoopContext context)
    {
        yield return base.Execute(context);
    }

}