using UnityEngine;

public class SpawnHItemBeh : HandsItemBeh, IHandsItemUsed
{
    public GameObject PrefabSpawn;
    public string ContextPrefName = "PrefabSpawn";
    public string ContextTrName = "PointTr";

    public void ExecuteItem(HandsItemBehContext context)
    {
        GameObject pref = context.Get<GameObject>("PrefabSpawn");
        if(pref == null)
        {
            if (PrefabSpawn == null)
                return;

            pref = PrefabSpawn;
        }

        Transform ponitTr = context.Get<Transform>("PointTr");
        if (ponitTr == null)
            ponitTr = context.ItemInstance.SourceWorldObject.transform;

        if (pref == null || ponitTr == null)
            return;

        Instantiate(pref, ponitTr.transform.position, ponitTr.transform.rotation);
    }
}