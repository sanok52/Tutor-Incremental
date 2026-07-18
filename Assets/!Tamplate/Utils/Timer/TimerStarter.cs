
using System;
using UnityEngine;

public class TimerStarter : MonoBehaviour
{
    [SerializeField] private bool playOnStart;
    public GameTimer Timer;

    void Start()
    {
        if (playOnStart)
            Play();
    }

    public void Play(int seconds, TimerDirection timerDirection = TimerDirection.None)
    {
        Timer = new GameTimer(seconds, int.MaxValue, 
            timerDirection == TimerDirection.None ? TimerDirection.Down : timerDirection);
        Play();
    }

    public void Play()
    {
        Timer.Start();
    }

    void Update()
    {
        if(Timer != null)
            Timer.Update(Time.deltaTime);
    }
}