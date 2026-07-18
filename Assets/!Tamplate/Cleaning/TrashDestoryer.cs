using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class TrashDestoryer : MonoBehaviour
{
    [SerializeField] private Transform point;
    [SerializeField] private float duration = 1f;

    public event Action OnDestoryBug;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out FPSHandsItemObject itemObject))
        {
            if(itemObject.GetTags().Contains("TrashBug"))
            {
                DestoryBug(itemObject);
                //StartCoroutine(DestoryBug(itemObject));
            }
        }
        
    }

    private void DestoryBug(FPSHandsItemObject itemObject)
    {
        //yield return itemObject.transform.DOMove(point.position, duration).WaitForCompletion();
        Destroy(itemObject.gameObject);
        OnDestoryBug?.Invoke();
    }
}
