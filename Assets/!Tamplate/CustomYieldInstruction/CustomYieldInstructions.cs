using System;
using UnityEngine;

public class WaitForSecondsAndInvoke : CustomYieldInstruction
{
    private float remainingTime;
    private readonly Action<float> onUpdate;
    private readonly Action onInterrupted;
    private readonly Action onCompleted;

    private bool isInterrupted;
    private bool isFinished;

    public WaitForSecondsAndInvoke(
        float time,
        Action<float> onUpdate = null,        
        Action onCompleted = null,
        Action onInterrupted = null
        )
    {
        remainingTime = time;
        this.onUpdate = onUpdate;
        this.onInterrupted = onInterrupted;
        this.onCompleted = onCompleted;
    }

    public override bool keepWaiting
    {
        get
        {
            if (isFinished)
                return false;

            if (isInterrupted)
            {
                isFinished = true;
                onInterrupted?.Invoke();
                return false;
            }

            if (remainingTime > 0f)
            {
                remainingTime -= Time.deltaTime;
                onUpdate?.Invoke(Mathf.Max(remainingTime, 0f));
                return true;
            }

            isFinished = true;
            onCompleted?.Invoke();
            return false;
        }
    }

    public void Interrupt()
    {
        if (!isFinished)
            isInterrupted = true;
    }
}