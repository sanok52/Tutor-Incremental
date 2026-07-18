using System.Collections;
using UnityEngine;

public class AddResourceItemBeh : ItemBehActCmd, ICmdBehaviourWithRoutine<ItemBehContext>
{
    public override bool NeedUseRoutine => true;
    public override bool NeedUseDropRoutine => false;

    public string ResourceID = "Life";
    public int AddCount = 50;

    public override void Execute(ItemBehContext context)
    {
    }

    public IEnumerator ExecuteRoutine(ItemBehContext context)
    {
        //G.ResourceManager.AddResource(ResourceID, AddCount, null, null);
        yield return new WaitForSeconds(2f);
    }
}
