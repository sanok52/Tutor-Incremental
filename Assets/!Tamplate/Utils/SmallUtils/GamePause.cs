using System.Collections;
using UnityEngine;
using System;

public static class GamePause
{
    public static bool IsPause { get; private set; }

    public static event Action<bool> OnPauseChange;

    public static void SetPause (bool pause)
    {
        if (pause == IsPause)
            return;

        IsPause = pause;
        OnPauseChange?.Invoke(pause);
    }

    public static void SwitchPause()
    {
        SetPause(!IsPause);
    }
}