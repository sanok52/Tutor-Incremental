using System;
using UnityEngine;

public enum TimerDirection
{
    Up,
    Down,
    None
}

[System.Serializable]
public class GameTimer
{
    public int CurrentSeconds { get; private set; }
    public int MaxSeconds { get; private set; }
    public TimerDirection Direction { get; private set; }
    public bool IsRunning { get; private set; }

    public event Action<int> OnTick; // каждый тик (передаёт текущее значение)
    public event Action OnComplete; // таймер дошёл до предела

    private float elapsedTime;

    public GameTimer(int startSeconds, int maxSeconds, TimerDirection direction)
    {
        Direction = direction;
        MaxSeconds = Mathf.Max(0, maxSeconds);

        if (direction == TimerDirection.Down)
            CurrentSeconds = Mathf.Clamp(startSeconds, 0, maxSeconds);
        else
            CurrentSeconds = Mathf.Clamp(startSeconds, 0, maxSeconds);

        IsRunning = false;
    }

    public void Start()
    {
        IsRunning = true;
        elapsedTime = 0;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Reset(int? toValue = null)
    {
        if (Direction == TimerDirection.Up)
            CurrentSeconds = toValue ?? 0;
        else
            CurrentSeconds = toValue ?? MaxSeconds;

        IsRunning = false;
        elapsedTime = 0;
    }

    public void Update(float deltaTime)
    {
        if (!IsRunning) return;

        elapsedTime += deltaTime;
        if (elapsedTime >= 1f)
        {
            int ticks = (int)elapsedTime;
            elapsedTime -= ticks;

            for (int i = 0; i < ticks; i++)
            {
                Tick();
            }
        }
    }

    private void Tick()
    {
        if (Direction == TimerDirection.Up)
        {
            CurrentSeconds++;
            OnTick?.Invoke(CurrentSeconds);

            if (CurrentSeconds >= MaxSeconds)
            {
                CurrentSeconds = MaxSeconds;
                IsRunning = false;
                OnComplete?.Invoke();
            }
        }
        else // Down
        {
            CurrentSeconds--;
            OnTick?.Invoke(CurrentSeconds);

            if (CurrentSeconds <= 0)
            {
                CurrentSeconds = 0;
                IsRunning = false;
                OnComplete?.Invoke();
            }
        }
    }

    public int GetRemainingSeconds()
    {
        return Direction == TimerDirection.Up
            ? Mathf.Max(0, MaxSeconds - CurrentSeconds)
            : Mathf.Max(0, CurrentSeconds);
    }

    public string GetFormattedTime()
    {
        int minutes = CurrentSeconds / 60;
        int seconds = CurrentSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    public string GetFormattedRemainingTime()
    {
        int remaining = GetRemainingSeconds();
        int minutes = remaining / 60;
        int seconds = remaining % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    public static string GetFormattedTime(int currentSeconds)
    {
        int minutes = currentSeconds / 60;
        int seconds = currentSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}