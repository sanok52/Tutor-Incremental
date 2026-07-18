
using System;
using System.Collections;
using System.Linq;

/// <summary>
/// Абстрактное поведение предмета.
/// Контекст использования будет реализован позже.
/// </summary>
public class ItemBehContrainerInvSys : CommandContainer<ItemBehContext>
{
    public bool NeedUseRoutine => behaviours.Exists(b => (b as ItemBehActCmd)?.NeedUseRoutine == true);
    public bool NeedUseDropRoutine => behaviours.Exists(b => (b as ItemBehActCmd)?.NeedUseDropRoutine == true);

    /// <summary>
    /// Проверяет, можно ли использовать предмет в данный момент.
    /// </summary>
    public virtual bool CanUse(ItemBehContext context) => behaviours.All(x => (x as ItemBehActCmd).CanUse(context));

    public void ExecuteAllDrop(ItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            var beh = (behaviour as ItemBehActCmd);
            beh.ExecuteDrop(context);
        }
    }

    public IEnumerator ExecuteRoutineDrop(ItemBehContext context)
    {
        foreach (var behaviour in behaviours)
        {
            var beh = (behaviour as ItemBehActCmd);
            if (beh.NeedUseDropRoutine)
                yield return beh.ExecuteRoutineDrop(context);
        }
    }

    public string ReloadCommandAll(string cmd, ItemInstanceInvSys itemInstanceInvSys)
    {
        string displyText = "";
        foreach (var behaviour in behaviours)
        {
            var beh = (behaviour as ItemBehActCmd);
            displyText += beh.ExecuteReload(cmd, itemInstanceInvSys);
        }
        return displyText;
    }
}