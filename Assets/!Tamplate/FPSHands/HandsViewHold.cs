using System.Collections.Generic;
using UnityEngine;

public class HandsViewHold : HandsView
{
    [Header("Transform рук")]
    [SerializeField] private Transform _leftHandTransform;
    [SerializeField] private Transform _rightHandTransform;
    [SerializeField] private Transform _twoHandTransform;

    [Space]
    [SerializeField] private string layerForGrap;
    [SerializeField] private HandsInteractorPlacer placer;

    [Header("Стек")]
    [SerializeField] private float _offsetStack = 0.1f;   // радиус разброса предметов в руке

    // Храним целевой трансформ и локальное смещение для каждого предмета
    private Dictionary<FPSItemInstance, Transform> _heldItems = new();
    private Dictionary<FPSItemInstance, Vector3> _offsets = new();
    private Dictionary<FPSItemInstance, CachedComponents> _cache = new();

    private class CachedComponents
    {
        public Rigidbody[] rigidbodies;
        public Collider[] colliders;
        public int originalLayer;   // исходный слой корневого объекта
    }

    private int _grapLayer = -1;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(layerForGrap))
        {
            _grapLayer = LayerMask.NameToLayer(layerForGrap);
            if (_grapLayer < 0)
                Debug.LogWarning($"HandsViewHold: Слой '{layerForGrap}' не найден.");
        }
    }

    bool isPlacer;
    private void LateUpdate()
    {
        foreach (var kvp in _heldItems)
        {
            FPSItemInstance item = kvp.Key;
            Transform target = kvp.Value;
            if (item.SourceWorldObject != null)
            {
                GameObject obj = item.SourceWorldObject.gameObject;
                Vector3 offset = _offsets.TryGetValue(item, out var off) ? off : Vector3.zero;
                if (placer.IsShown)
                {
                    obj.transform.position = placer.Target.position;
                    obj.transform.rotation = placer.Target.rotation;

                    if (!isPlacer)
                    {
                        obj.TryGetComponent(out IHItemAnimator handItem);
                        if (handItem != null)
                            handItem.Drop();
                    }
                    isPlacer = true;
                    //obj.transform.localScale = Vector3.one;
                }
                else
                {

                    obj.transform.position = target.position + transform.TransformVector(offset);
                    obj.transform.rotation = target.rotation;

                    if (isPlacer)
                    {
                        obj.TryGetComponent(out IHItemAnimator handItem);
                        if (handItem != null)
                            handItem.Grap();
                    }
                    isPlacer = false;
                    //obj.transform.localScale = Vector3.one * 0.6f;
                }
            }
        }
    }

    // Переопределённый метод с индексом
    protected override void HandleItemTaken(FPSItemInstance item, bool isLeft, int index)
    {
        Transform hand = isLeft ? _leftHandTransform : _rightHandTransform;
        Attach(item, hand, index);
    }

    protected override void HandleLargeItemTaken(FPSItemInstance item)
    {
        // Большой предмет всегда имеет индекс 0
        Attach(item, _twoHandTransform, 0);
    }

    protected override void HandleItemDropped(FPSItemInstance item, bool isLeft)
        => DetachForDrop(item);

    protected override void HandleLargeItemDropped(FPSItemInstance item)
        => DetachForDrop(item);

    protected override void HandleItemStowed(FPSItemInstance item, bool isLeft)
        => DetachForStow(item);

    protected override void HandleLargeItemStowed(FPSItemInstance item)
        => DetachForStow(item);

    protected override void HandleItemInteraction(FPSItemInstance item, bool isLeft, HandsReceiver receiver)
    {
        if (receiver.IsInteractoionTakesObject)
            DetachForInteraction(item);
    }

    protected override void HandleLargeItemInteraction(FPSItemInstance item, HandsReceiver receiver)
    {
        if (receiver.IsInteractoionTakesObject)
            DetachForInteraction(item);
    }

    // ========== Прикрепление с учётом индекса ==========
    private void Attach(FPSItemInstance item, Transform hand, int index)
    {
        if (item.SourceWorldObject == null) return;
        GameObject obj = item.SourceWorldObject.gameObject;

        // Отключаем физику
        Rigidbody[] rbs = obj.GetComponentsInChildren<Rigidbody>();
        Collider[] cols = obj.GetComponentsInChildren<Collider>();
        SetPhysics(obj, rbs, cols, true, false);

        // Сохраняем кэш
        var cached = new CachedComponents { rigidbodies = rbs, colliders = cols };
        _cache[item] = cached;

        // Смена слоя, если задан
        if (_grapLayer >= 0)
        {
            cached.originalLayer = obj.layer;   // запоминаем исходный слой корневого объекта
            SetLayerRecursively(obj, _grapLayer);
        }

        // Детерминированное смещение для предметов с индексом > 0
        Vector3 offset = Vector3.zero;
        if (index > 0)
        {
            Random.InitState(index); // всегда одна и та же последовательность для этого индекса
            offset = Random.insideUnitSphere * _offsetStack;
        }

        _offsets[item] = offset;
        obj.SetActive(true);
        _heldItems[item] = hand;
    }

    // ========== Открепление ==========
    private void DetachForDrop(FPSItemInstance item)
    {
        if (!_cache.TryGetValue(item, out var cached)) return;
        if (item.SourceWorldObject == null) return;
        GameObject obj = item.SourceWorldObject.gameObject;

        // Восстанавливаем физику
        SetPhysics(obj, false, true);

        RestoreOriginalLayer(item, cached);
        RemoveItem(item);
    }

    private void DetachForStow(FPSItemInstance item)
    {
        if (!_cache.TryGetValue(item, out var cached)) return;
        if (item.SourceWorldObject == null) return;
        GameObject obj = item.SourceWorldObject.gameObject;

        // Восстанавливаем физику
        SetPhysics(obj, cached.rigidbodies, cached.colliders, false, true);
        obj.SetActive(false);

        RestoreOriginalLayer(item, cached);
        RemoveItem(item);
    }

    private void SetPhysics (GameObject obj, bool isKinmatic, bool isCollider)
    {
        Rigidbody[] rbs = obj.GetComponentsInChildren<Rigidbody>();
        Collider[] cols = obj.GetComponentsInChildren<Collider>();

        SetPhysics(obj, rbs, cols, isKinmatic, isCollider);
    }

    private void SetPhysics(GameObject obj, Rigidbody[] rbs, Collider[] cols, bool isKinematic, bool isCollider)
    {
        if(obj.TryGetComponent(out Collider collider))
            collider.isTrigger = !isCollider;

        foreach (var rb in rbs)
            if (!rb.TryGetComponent(out ItemStaticElement itemStatic))
                rb.isKinematic = isKinematic;
        foreach (var col in cols)
            if (!col.TryGetComponent(out ItemStaticElement itemStatic))
            {
                col.enabled = isCollider;
            }        
    }

    private void DetachForInteraction(FPSItemInstance item)
    {
        if (!_cache.TryGetValue(item, out var cached)) return;
        // Физику не трогаем, но слой восстанавливаем

        if (item.SourceWorldObject.TryGetComponent(out IHItemAnimator itemAnimator))
            itemAnimator.Drop();

        RestoreOriginalLayer(item, cached);
        RemoveItem(item);
    }

    private void RestoreOriginalLayer(FPSItemInstance item, CachedComponents cached)
    {
        if (_grapLayer >= 0 && item.SourceWorldObject != null)
        {
            SetLayerRecursively(item.SourceWorldObject.gameObject, cached.originalLayer);
        }
    }

    private void RemoveItem(FPSItemInstance item)
    {
        _heldItems.Remove(item);
        _offsets.Remove(item);
        _cache.Remove(item);
    }

    // ========== Вспомогательные методы ==========
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}