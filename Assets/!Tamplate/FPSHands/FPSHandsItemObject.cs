using System;
using System.Collections.Generic;
using UnityEngine;

public class FPSHandsItemObject : MonoBehaviour
{
    [SerializeField] private FPSItemData _itemData;
    public bool destroyOnGrap = false;
    public bool hideOnGrap = false;

    [SerializeField] private List<string> tags;
    [SerializeField] private bool isOverrideTags;

    [Space]
    public Vector3 OffsetPlace;

    [Space]
    public List<OverridebleForContext<GameObject>> overridebleForContextGO;
    public List<OverridebleForContext<Transform>> overridebleForContextTr;
    public List<OverridebleForContext<float>> overridebleForContextFl;
    public List<OverridebleForContext<string>> overridebleForContextStr;
    public Action<string, object[]> behaviourAction;

    public FPSItemData ItemData => _itemData;
    public bool IsOverrideTags => isOverrideTags;

    public List<string> OverTags => tags;

    public event Action<HandsItemObjectInfo> OnGrap;
    public event Action<FPSHandsItemObject> OnDrop;

    public FPSItemInstance GetInstance(FPSHandsModel handsModel)
    {
        if (_itemData == null) return null;

        FPSItemInstance instance = new FPSItemInstance(_itemData, this);
        return instance;
    }

    public void Grap(FPSHandsModel handsModel)
    {
        FPSItemInstance instance = GetInstance(handsModel);

        if (destroyOnGrap)
            Destroy(gameObject);
        else if (hideOnGrap)
            gameObject.SetActive(false);

        OnGrap?.Invoke(new HandsItemObjectInfo()
        {
            handsModel = handsModel,
            itemInstance = instance
        });
    }

    public void DropWork()
    {
        OnDrop?.Invoke(this);
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    public string[] GetTags()
    {
        List<string> tgs = new List<string>();
        // —начала добавл€ем локальные теги, если они есть
        if (tags != null)
            tgs.AddRange(tags);
        // ≈сли isOverrideTags, то не добавл€ем теги из ItemData
        if (isOverrideTags)
            return tgs.ToArray();
        // »наче добавл€ем теги из данных, если есть
        if (_itemData != null && _itemData.tags != null)
            tgs.AddRange(_itemData.tags);
        return tgs.ToArray();
    }
}

[Serializable]
public class HandsItemObjectInfo
{
    public FPSItemInstance itemInstance;
    public FPSHandsModel handsModel;
}

[Serializable]
public class OverridebleForContext<T>
{
    public string id;
    public T value;
}