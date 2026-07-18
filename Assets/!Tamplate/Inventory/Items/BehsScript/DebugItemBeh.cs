using System.Collections;
using UnityEngine;

public class DebugItemBeh : ItemBehActCmd
{
    public override bool NeedUseRoutine => true;
    public override bool NeedUseDropRoutine => true;

    public override void Execute(ItemBehContext context)
    {
        Debug.Log($"Использован предмет: {context.ItemInstance.Data.ItemId}");
    }

    public IEnumerator ExecuteRoutine(ItemBehContext context)
    {
        Debug.Log($"Использован предмет: {context.ItemInstance.Data.ItemId} start");
        yield return new WaitForSeconds(3f);
        Debug.Log($"Использован предмет: {context.ItemInstance.Data.ItemId} end");
    }

    public override void ExecuteDrop(ItemBehContext context)
    {
        Debug.Log("Предмет выброшен");
    }

    public override IEnumerator ExecuteRoutineDrop(ItemBehContext context)
    {
        Debug.Log($"Выброшен предмет: {context.ItemInstance.Data.ItemId} start");
        yield return new WaitForSeconds(3f);
        Debug.Log($"Выброшен предмет: {context.ItemInstance.Data.ItemId} end");
    }
}