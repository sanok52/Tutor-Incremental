using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlaceHandsReceiver : MonoBehaviour
{
    [SerializeField] private Transform _placementPoint;
    private HandsReceiver _receiver;
    private FPSItemInstance _placedItem;
    [SerializeField] private bool CanReplace = true;
    [SerializeField] private bool IsChangeParent = true;
    [SerializeField] private bool IsOnColliders = true;
    [SerializeField] private bool IsLocalableScale = false;
    [SerializeField] private Vector3 localScaling = Vector3.one;

    private Vector3 localScaleInside;

    public FPSItemInstance PlacedItem => _placedItem;

    public event Action<FPSItemInstance> OnItemPlaced;

    private void Awake()
    {
        _receiver = GetComponent<HandsReceiver>();
        if (_receiver != null)
        {
            _receiver.IsInteractoionTakesObject = true;
            _receiver.OnInteraction += HandlePlacement;
        }
    }

    private void OnDestroy()
    {
        if (_receiver != null)
            _receiver.OnInteraction -= HandlePlacement;

        // Отписываемся от текущего размещённого объекта
        if (_placedItem?.SourceWorldObject != null)
            _placedItem.SourceWorldObject.OnGrap -= OnPlacedItemGrapped;
    }

    private void HandlePlacement(HandInteractionInfo info)
    {
        if (_placedItem != null && !CanReplace)
            return;

        if (info.item.SourceWorldObject == null) return;
        FPSHandsModel model = info.handsModel;
        if (model == null) return;

        // Отписываемся от предыдущего размещённого предмета (если есть)
        if (_placedItem?.SourceWorldObject != null)
            _placedItem.SourceWorldObject.OnGrap -= OnPlacedItemGrapped;

        FPSItemInstance oldItem = _placedItem;
        GameObject newObj = info.item.SourceWorldObject.gameObject;

        // Размещаем новый предмет
        newObj.transform.position = _placementPoint.position;
        newObj.transform.rotation = _placementPoint.rotation;

        if(IsChangeParent)
            newObj.transform.parent = _placementPoint;

        Rigidbody[] rbs = newObj.GetComponentsInChildren<Rigidbody>();
        Collider[] cols = newObj.GetComponentsInChildren<Collider>();
        foreach (var rb in rbs) rb.isKinematic = true;   // чтобы не падал
        foreach (var col in cols) col.enabled = IsOnColliders;

        if (IsLocalableScale)
        {
            localScaleInside = newObj.transform.localScale;
            newObj.transform.localScale = localScaling;
        }

        _placedItem = info.item;   // обновляем лежащий предмет

        // Подписываемся на событие Grap нового объекта, чтобы сбросить _placedItem при подборе
        if (_placedItem.SourceWorldObject != null)
            _placedItem.SourceWorldObject.OnGrap += OnPlacedItemGrapped;

        // Если на точке уже лежал другой предмет – возвращаем его в руки
        if (oldItem != null)
        {
            model.TakeItem(oldItem);
        }

        OnItemPlaced?.Invoke(_placedItem);
    }

    // Обработчик: когда размещённый предмет подбирают, сбрасываем ссылку
    private void OnPlacedItemGrapped(HandsItemObjectInfo info)
    {
        if (_placedItem != null && !CanReplace)
            return;

        if (_placedItem != null && _placedItem.SourceWorldObject == info.itemInstance.SourceWorldObject)
        {
            // Отписываемся, чтобы избежать утечек
            info.itemInstance.SourceWorldObject.OnGrap -= OnPlacedItemGrapped;

            if (IsLocalableScale)
                _placedItem.SourceWorldObject.transform.localScale = localScaleInside;

            if (info.handsModel.HasItemInHands())
            {
                var item = info.handsModel.GetHeldItem();
                if (info.handsModel.LeftHandItem != null && info.handsModel.LeftHandItem.Data != null)
                    info.handsModel.DropItem(true);
                if (info.handsModel.RightHandItem != null && info.handsModel.RightHandItem.Data != null)
                    info.handsModel.DropItem(false);
                HandlePlacement(new HandInteractionInfo(_receiver, item, info.handsModel));
            }
            else
                _placedItem = null;
        }
    }
}