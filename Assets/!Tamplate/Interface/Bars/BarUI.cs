using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class BarUI : MonoBehaviour
{
    [SerializeField] private string id;
    private float currentValue;
    [SerializeField] private float maxValue;

    [Space]
    public UnityEvent<float> OnChangeValue;
    public UnityEvent<float> OnAddValue;
    public UnityEvent<float> OnRemoveValue;

    public float CurrentValue => currentValue;
    public float MaxValue => maxValue;

    public string ID => id;

    public void Show (float currentValue) 
    {
        if (currentValue > this.currentValue)        
            OnAddValue?.Invoke(currentValue - this.currentValue);        
        else if (currentValue < this.currentValue)        
            OnRemoveValue?.Invoke(this.currentValue - currentValue);

        this.currentValue = currentValue;
        OnChangeValue?.Invoke(currentValue);

        ShowValueInInterface();
    }

    public void SetMaxValue(float maxValue)
    {
        this.maxValue = maxValue;
        ShowMaxValueInInterface();
    }

    private void ShowValueInInterface() => ShowValueInInterface(currentValue);
    private void ShowMaxValueInInterface() => ShowMaxValueInInterface(maxValue);

    public abstract void ShowValueInInterface(float currentValue);
    public abstract void ShowMaxValueInInterface(float maxValue);
}