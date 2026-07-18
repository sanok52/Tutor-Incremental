using System;
using UnityEngine;

public class FullableTriggerObject : TriggerObjectMessageReciver
{
    [Space]
    public float durationStep = 1f;
    public int currentValue;
    public int maxValue = 10;

    private float currentTime;

    public Action<int> OnChangeValue;
    public Action OnOverfullValue;
    public Action OnDownfullValue;

    private void Start()
    {
        OnMessageStay += (ob, tags) => AddTimeValue();

        if (TryGetComponent(out FPSHandsItemObject itemObject))
            itemObject.OnGrap += GrapWork;

        currentTime = durationStep - 0.1f;
    }

    private void GrapWork(HandsItemObjectInfo info)
    {
        ExitAll();
    }

    private void AddTimeValue()
    {
        AddCurrentTime(Time.deltaTime);
    }

    private void AddCurrentTime(float deltaTime)
    {
        currentTime += Time.deltaTime;

        if(currentTime >= durationStep)
        {
            currentTime = 0;
            AddValue();
        }
    }

    private void AddValue()
    {
        if (currentValue >= maxValue)
            return;

        currentValue++;

        OnChangeValue?.Invoke(currentValue);
        if (currentValue >= maxValue)
            OnOverfullValue?.Invoke();
    }

    public void Break()
    {
        currentValue = 0;
        OnChangeValue?.Invoke(0);
        OnDownfullValue?.Invoke();
    }
}
