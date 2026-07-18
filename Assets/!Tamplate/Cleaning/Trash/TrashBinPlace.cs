using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

public class TrashBinPlace : MonoBehaviour
{
    [SerializeField] private HandsReceiver _receiver;
    [SerializeField] private FPSHandsItemObject _itemObject;
    [SerializeField] private int countMax;
    [SerializeField] private Transform tarshRoot;
    [SerializeField] private float trashOffset = 0.2f;
    [SerializeField] private Transform bottom;
    private int count;

    private List<GameObject> countGOs = new List<GameObject>();

    public event Action<FPSHandsItemObject> OnTrash;
    public event Action OnOverfull;

    void Start()
    {
        _receiver.IsInteractoionTakesObject = true;
        _receiver.OnInteraction += HandlePlacement;
        _itemObject.OnDrop += DropWork;
        Clear();
    }

    private void DropWork(FPSHandsItemObject itemObject)
    {
        UpdateCount();
    }

    private void OnDestroy()
    {
        _receiver.OnInteraction -= HandlePlacement;
        _itemObject.OnDrop -= DropWork;
    }

    private void HandlePlacement(HandInteractionInfo info)
    {
        if (count >= countMax)
            return;

        if (!info.item.GetTags().Contains("Trash"))
            return;

        OnTrash?.Invoke(info.item.SourceWorldObject);

        GameObject go = info.item.SourceWorldObject.gameObject;
        go.GetComponent<Rigidbody>().isKinematic = true;
        go.GetComponent<Collider>().enabled = false;
        go.SetActive(true);
        go.transform.parent = tarshRoot;
        countGOs.Add(go);

        float up = trashOffset * (countGOs.Count - 1);
        float amount = Mathf.Clamp(count / (float)countMax, 0.1f, 0.7f);
        float r1 = Random.Range(amount / 2f, amount) * 0.25f * Random.Range(-1, 1);
        float r2 = Random.Range(amount / 2f, amount) * 0.25f * Random.Range(-1, 1);

        go.transform.DOLocalMove(new Vector3(r1, up, r2), 0.5f);
        go.transform.DOScale(0.7f, 0.5f);
        bottom.transform.localPosition = new Vector3(0, up, 0);

        AddCount(1);
    }

    private void AddCount(int v)
    {
        count += v;
        UpdateCount();

        if (count >= countMax)
        {
            _receiver.enabled = false;
            OnOverfull?.Invoke();
        }
    }

    public void Clear()
    {
        count = 0;

        foreach (var item in countGOs)
        {
            Destroy(item.gameObject);
        }
        bottom.transform.localPosition = new Vector3(0, 0, 0);

        countGOs.Clear();

        UpdateCount();
        _receiver.enabled = true;
    }

    private void UpdateCount()
    {
        SetCountVisual(count);
    }

    private void SetCountVisual(int count)
    {
        for (int i = 0; i < countGOs.Count; i++)
        {
            if (countGOs[i].TryGetComponent(out Rigidbody rigidbody))
                rigidbody.isKinematic = true;
            if (countGOs[i].TryGetComponent(out Collider collider))
            {
                Collider[] colliders = countGOs[i].GetComponents<Collider>();
                foreach (var coll in colliders)
                    coll.enabled = false;
            }

            Transform[] childs = countGOs[i].transform.GetAllChildOfChild();
            foreach (var child in childs)
            {
                if (child.TryGetComponent(out rigidbody))
                    rigidbody.isKinematic = true;
                if (child.TryGetComponent(out collider))
                {
                    Collider[] colliders = child.gameObject.GetComponents<Collider>();
                    foreach (var coll in colliders)                    
                        coll.enabled = false;                    
                }
            }
        }
    }
}