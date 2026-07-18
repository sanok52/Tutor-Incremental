using System;
using System.Linq;
using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [SerializeField] private TrashBinPlace binPlace;
    [SerializeField] private FPSHandsItemObject itemObject;
    [SerializeField] private FPSHandsModel fpsHandsModel;
    [SerializeField] private GameObject grapUnactive;

    [Space]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform pointTr;
    [SerializeField] private string contextPrefName = "PrefabSpawn";
    [SerializeField] private string contextTrName = "PointTr";

    private bool isOverfull;

    public event Action OnCreateBug;

    private void Start()
    {
        binPlace.OnOverfull += Overfull;
        binPlace.OnTrash += OnTrashWork;
        fpsHandsModel.OnItemUsed += UsedItem;

        itemObject.OnGrap += GrapWork;
        itemObject.OnDrop += DropWork;
    }

    private void DropWork(FPSHandsItemObject itemObject)
    {
        grapUnactive.SetActive(true);
    }

    private void GrapWork(HandsItemObjectInfo info)
    {
        grapUnactive.SetActive(false);
    }

    private void OnDestroy()
    {
        binPlace.OnOverfull -= Overfull;
        fpsHandsModel.OnItemUsed -= UsedItem;
    }

    private void Overfull()
    {
        isOverfull = true;
    }

    private void OnTrashWork(FPSHandsItemObject itemObject)
    {
        if (this.itemObject.overridebleForContextGO.Any(x => x.id == contextPrefName))
            return;

        this.itemObject.overridebleForContextGO.Add(new OverridebleForContext<GameObject>() { id = contextPrefName, value = prefab });
        this.itemObject.overridebleForContextTr.Add(new OverridebleForContext<Transform>() { id = contextTrName, value = pointTr });
    }

    private void UsedItem(FPSItemInstance instance, bool isLeft)
    {
        if (//!isOverfull || 
            instance.SourceWorldObject != itemObject)
            return;

        itemObject.overridebleForContextGO.RemoveAll(x => x.id == contextPrefName);
        itemObject.overridebleForContextTr.RemoveAll(x => x.id == contextTrName);

        OnCreateBug?.Invoke();

        binPlace.Clear();
        isOverfull = false;
    }
}
