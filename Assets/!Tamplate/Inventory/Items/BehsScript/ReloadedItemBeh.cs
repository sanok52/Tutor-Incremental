using UnityEngine;

public class ReloadedItemBeh : ItemBehActCmd
{
    public override bool NeedUseRoutine => false;
    public override bool NeedUseDropRoutine => false;

    public int count = -1;
    public string reloadTag = "Oxygen";

    [Space]
    public string ReloadText = "";
    public string NotNeedReloadText = "";

    public override void Execute(ItemBehContext context)
    {
    }

    public override string ExecuteReload(string cmd, ItemInstanceInvSys item)
    {
        if (cmd != reloadTag)
            return "";

        int countStart = item.RemainingUses;
        if (count == -1)        
            item.ReloadUses();        
        else
            item.AddUses(count);
        //G.PlayerInventory.UpdateSlotsInfo();
        return (countStart == item.RemainingUsesMax) ? $"\n{NotNeedReloadText}" : $"\n{ReloadText}";
    }
}