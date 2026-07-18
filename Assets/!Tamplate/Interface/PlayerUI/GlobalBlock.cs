using System;
using UnityEngine;

public static class GlobalBlock
{
    public static Action<bool> OnBlockChanged;
    private static bool isBlock;


    public static void Init()
    {
        OnBlockChanged = null;
    }

public static void SetBlock(bool block)
    {
        if (isBlock != block)
        {
            isBlock = block;
            OnBlockChanged?.Invoke(isBlock);
        }
    }
}
