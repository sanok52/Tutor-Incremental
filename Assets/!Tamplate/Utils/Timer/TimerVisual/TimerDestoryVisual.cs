using System;
using UnityEngine;
using UnityEngine.Events;

public class TimerDestoryVisual : MonoBehaviour
{
    [SerializeField] private float timer;
    private float timerStart;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool isStartOnAwake;

    [Space]
    [SerializeField] private TimerView timerView;

    [Space]
    public UnityEvent<float> OnTickEv;
    public UnityEvent<TimerDestoryVisual> OnDestroyEv;

    public event Action<float> OnTick;
    public event Action<TimerDestoryVisual> OnDes;

    private bool isPlay;

    public bool IsPlay => isPlay;

    private void Awake()
    {
        timerStart = timer;

        if (isStartOnAwake)
            StartTimer();
    }

    public void StartTimer()
    {
        isPlay = true;
        timerView?.Init(0f, timer);
    }

    private void Update()
    {
        if (isPlay == false || GamePause.IsPause)
            return;

        Tick();
    }

    private void Tick()
    {
        timer -= Time.deltaTime;

        OnTickEv?.Invoke(timer);
        OnTick?.Invoke(timer);
        timerView?.UpdateValue(timer);

        if (timer <= 0f)        
            End();        
    }

    private void End()
    {
        Stop();
        OnDestroyEv?.Invoke(this);
        OnDes?.Invoke(this);
        Destroy(gameObject);
    }

    public void Stop()
    {
        isPlay = false;
        timer = timerStart;
        timerView.Stop();
    }
}
