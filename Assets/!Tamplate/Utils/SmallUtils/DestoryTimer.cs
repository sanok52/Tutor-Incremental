using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DestoryTimer : MonoBehaviour
{
    [SerializeField] private float delay = 5f;
    [SerializeField] private bool isDestory = true;
    public UnityEvent<DestoryTimer> OnDestory;

    public void OnEnable()
    {
        StartCoroutine(StartTimer(delay));
    }

    private IEnumerator StartTimer (float delay)
    {
        yield return new WaitForSeconds(delay);
        InvokeDestory();
    }

    private void InvokeDestory()
    {
        OnDestory?.Invoke(this);
        if (isDestory)
            Destroy(gameObject);
    }
}