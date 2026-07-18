using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSHandsInteractor : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactionLayers;
    [SerializeField] private LayerMask _placementLayers;
    [SerializeField] private float _interactionDistance = 3f;
    [SerializeField] private float _interactionDistanceDown = 3f;
    [SerializeField] private FPSHandsModel _handsModel;
    [SerializeField] private HandsInteractorPlacer _placer;
    [SerializeField] private HandsSurfacesRule[] _surfaceRules;
    [SerializeField] private HandsView _handsView;

    [Header("Бросок")]
    [SerializeField] private float _throwForce = 5f;
    [SerializeField] private Vector3 _throwLocalOffset = Vector3.zero;

    [Header("UI")]
    [SerializeField] private Image _imagePoint;
    [SerializeField] private TMP_Text _tmpAtPoint;
    [SerializeField] private TMP_Text _tmpNote;

    // Параметры для размещения (сохраняются между кадрами)
    private Vector3 _lastPlacementPoint;
    private Vector3 _lastPlacementNormal;
    private bool _lastPlacementAttached;
    private bool _canPlace;

    // Состояние удержания
    private bool _isHolding;
    private bool _holdIsLeft;

    private void Update()
    {
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        float dist = Mathf.Max(_interactionDistance, _interactionDistanceDown);
        bool hasHitInteraction = Physics.Raycast(ray, out RaycastHit interactionHit, dist, _interactionLayers) && 
            TestDitance(ray.origin, interactionHit.point);
        bool hasHitPlacement = Physics.Raycast(ray, out RaycastHit placementHit, dist, _placementLayers) &&
            TestDitance(ray.origin, placementHit.point);

        // ========== ЛКМ ==========
        if (Input.GetMouseButtonDown(0))
        {
            // 1. Подбор предмета
            if (hasHitInteraction)
            {
                FPSHandsItemObject worldItem = interactionHit.collider.GetComponent<FPSHandsItemObject>();
                if (worldItem != null && worldItem.enabled == true)
                {
                    FPSItemInstance instance = worldItem.GetInstance(_handsModel);
                    if (instance != null)
                    {
                        bool isGrap = _handsModel.TakeItem(instance);
                        if (isGrap)
                        {
                            worldItem.Grap(_handsModel);
                            _placer.Hide();
                        }
                    }
                    return;
                }
            }

            // 2. Взаимодействие с HandsReceiver
            if (hasHitInteraction)
            {
                HandsReceiver receiver = interactionHit.collider.GetComponent<HandsReceiver>();
                if (receiver != null)
                {
                    if (_handsModel.HasItemInHand(false))
                        _handsModel.InteractWithReceiver(false, receiver);
                    else if (_handsModel.HasItemInHand(true))
                        _handsModel.InteractWithReceiver(true, receiver);
                    _placer.Hide();
                    return;
                }
            }

            // 3. Размещение на поверхности (если активно)
            if (_canPlace && _placer.IsShown && _handsModel.HasItemInHands())
            {
                PlaceHeldItem();
                return;
            }
        }

        // 4. Использование предмета в руке
        if (_handsModel.HasItemInHands())
        {
            bool useLeft = !_handsModel.HasItemInHand(false) && _handsModel.HasItemInHand(true);
            FPSItemInstance item = useLeft ? _handsModel.LeftHandItem : _handsModel.RightHandItem;

            if (Input.GetKeyDown(GetKeyUse(item.Data)))
            {
                if (item != null && item.Data.canUseHold)
                {
                    if (_handsModel.StartUseHold(useLeft))
                    {
                        _isHolding = true;
                        _holdIsLeft = useLeft;
                    }
                }
                else if (item != null && item.Data.canUse)
                {
                    _handsModel.UseItem(useLeft);
                }
            }
        }

        // ========== Удержание ==========
        if (_isHolding)
        {
            if (!_handsModel.IsHoldActive(_holdIsLeft))
            {
                _isHolding = false;
            }
            else
            {
                _handsModel.UpdateUseHold(_holdIsLeft);
            }
        }

        if (Input.GetMouseButtonUp(0) && _isHolding)
        {
            _handsModel.StopUseHold(_holdIsLeft);
            _isHolding = false;
        }

        // ========== Визуализация размещения ==========
        if (_handsModel.HasItemInHands() && hasHitPlacement)
        {
            if (placementHit.collider.GetComponent<FPSHandsItemObject>() != null ||
                placementHit.collider.GetComponent<HandsReceiver>() != null)
            {
                _placer.Hide();
                _canPlace = false;
            }
            else
            {
                FPSItemInstance heldItem = _handsModel.GetHeldItem();
                if (heldItem != null && CanPlaceOnSurface(heldItem.Data, placementHit.transform, placementHit.point, placementHit.normal,
                        out _lastPlacementPoint, out _lastPlacementNormal, out _lastPlacementAttached))
                {
                    _placer.Show(_lastPlacementPoint, _lastPlacementNormal);
                    _canPlace = true;
                }
                else
                {
                    _placer.Hide();
                    _canPlace = false;
                }
            }
        }
        else
        {
            _placer.Hide();
            _canPlace = false;
        }

        // ПКМ – выброс
        if (Input.GetMouseButtonDown(1))
        {
            DropHeldItemWithPhysics(false);
        }

        // G – бросок
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropHeldItemWithPhysics(true);
        }

        // Обновление UI подсказок
        UpdateUI(hasHitInteraction, interactionHit, hasHitPlacement, placementHit);
    }

    private bool TestDitance(Vector3 origin, Vector3 point)
    {
        Vector3 delta = point - origin;        
        return new Vector2(delta.x, delta.z).magnitude <= _interactionDistance && Mathf.Abs(delta.y) <= _interactionDistanceDown;
    }

    private KeyCode GetKeyUse(FPSItemData data)
    {
        return data.OverrideUseButton == KeyCode.None ? KeyCode.Mouse0 : data.OverrideUseButton;
    }

    private void UpdateUI(bool hasHitInteraction, RaycastHit interactionHit, bool hasHitPlacement, RaycastHit placementHit)
    {
        // Сброс
        _imagePoint.color = Color.white;
        _tmpAtPoint.text = "";
        _tmpNote.text = "";

        FPSItemInstance heldItem = _handsModel.GetHeldItem();

        // 1. Подбор
        if (hasHitInteraction)
        {
            FPSHandsItemObject worldItem = interactionHit.collider.GetComponent<FPSHandsItemObject>();
            if (worldItem != null && worldItem.enabled)
            {
                _imagePoint.color = new Color(0f, 0.5f, 1f); // синий
                _tmpAtPoint.text = "[ЛКМ] подобрать";
                return;
            }

            // 2. Взаимодействие с Receiver
            HandsReceiver receiver = interactionHit.collider.GetComponent<HandsReceiver>();
            if (receiver != null && heldItem != null && receiver.CanInteract(heldItem))
            {
                _imagePoint.color = Color.yellow;
                _tmpAtPoint.text = "[ЛКМ] применить";
                return;
            }
        }

        // 3. Размещение
        if (heldItem != null)
        {
            if (_canPlace && _placer.IsShown)
            {
                _imagePoint.color = Color.yellow;
                _tmpAtPoint.text = "[ЛКМ] положить\n[Колёсико] - вертеть";
            }
            else if (hasHitPlacement)
            {
                _imagePoint.color = Color.gray;
                _tmpAtPoint.text = "";
            }
        }

        // Подсказка действий с предметом
        if (heldItem != null)
        {
            string note = "";
            KeyCode useKey = GetKeyUse(heldItem.Data);
            if (heldItem.Data.canUse || heldItem.Data.canUseHold)
            {
                note += $"[{useKey}] Использовать";
                if (heldItem.Data.canUseHold) note += " (зажать)";
                note += "\n";
            }
            if (_handsModel.canDropItems && heldItem.Data.canDrop)
            {
                note += "[ПКМ] Выкинуть\n[G] Бросить\n";
            }
            _tmpNote.text = note.TrimEnd('\n');
        }
    }

    /// <summary>Выбрасывает или бросает предмет из рук. При броске добавляет силу.</summary>
    private void DropHeldItemWithPhysics(bool throwItem)
    {
        if (!_handsModel.HasItemInHands()) return;

        FPSItemInstance item = _handsModel.GetHeldItem();
        if (item?.SourceWorldObject == null) return;
        GameObject obj = item.SourceWorldObject.gameObject;

        Transform dropPoint = _handsView != null ? _handsView.GetDropPoint() : null;
        if (dropPoint != null)
        {
            obj.transform.position = dropPoint.position;
            obj.transform.rotation = dropPoint.rotation;
        }

        _handsModel.DropHeldItem();
        _isHolding = false;

        if (throwItem)
        {
            Rigidbody rb = obj.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                Vector3 worldOffset = _camera.transform.TransformDirection(_throwLocalOffset);
                Vector3 force = _camera.transform.forward * _throwForce + worldOffset;
                rb.AddForce(force, ForceMode.Impulse);
            }
        }

        _placer.Hide();
        _canPlace = false;
    }

    private void PlaceHeldItem()
    {
        if (!_handsModel.HasItemInHands() || !_canPlace) return;

        FPSItemInstance item = _handsModel.GetHeldItem();
        if (item?.SourceWorldObject == null) return;
        GameObject obj = item.SourceWorldObject.gameObject;

        _handsModel.DropHeldItem();
        _isHolding = false;

        var (pos, rot) = _placer.GetPlacementInfo();
        obj.transform.position = pos;
        obj.transform.rotation = rot;

        Rigidbody[] rbs = obj.GetComponentsInChildren<Rigidbody>();
        Collider[] cols = obj.GetComponentsInChildren<Collider>();

        if (_lastPlacementAttached)
        {
            foreach (var rb in rbs)
            {
                if (!rb.TryGetComponent(out ItemStaticElement itemStatic))
                    rb.isKinematic = true;
            }
            foreach (var col in cols)
            {
                if (!col.TryGetComponent(out ItemStaticElement itemStatic))
                    col.enabled = false;
            }
        }

        _placer.Hide();
        _canPlace = false;
    }

    private bool CanPlaceOnSurface(FPSItemData itemData, Transform target, Vector3 hitPoint, Vector3 hitNormal,
        out Vector3 point, out Vector3 normal, out bool attached)
    {
        point = hitPoint;
        normal = hitNormal;
        attached = false;

        float angle = Vector3.Angle(Vector3.up, hitNormal);
        string surfaceName = null;
        foreach (var rule in _surfaceRules)
        {
            if (angle >= rule.angleReference.x && angle <= rule.angleReference.y &&
                (rule.containsTag.Length == 0 || (target.gameObject.TryGetComponent(out MyTagsContainer myTags) && myTags.All(rule.containsTag))))
            {
                surfaceName = rule.SurfaceName;
                attached = rule.attached;
                break;
            }
        }
        if (string.IsNullOrEmpty(surfaceName)) return false;

        if (itemData.surfaces == null || itemData.surfaces.Length == 0) return false;

        InfoElement surfaceElement = new InfoElement { all = new[] { surfaceName } };
        foreach (var container in itemData.surfaces)
        {
            if (container.Contains(surfaceElement))
                return true;
        }
        return false;
    }
}