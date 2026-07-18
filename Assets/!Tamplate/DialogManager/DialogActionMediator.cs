using System;
using System.Collections.Generic;
using UnityEngine;

public static class DialogActionMediator
{
    public static Dictionary<string, Action<DialogActionInfo>> CommandBehaviours = new Dictionary<string, Action<DialogActionInfo>>();

    public static void InvokeCommand(DialogActionInfo actionInfo)
    {
        InvokeCommand(actionInfo.CommandName, actionInfo);
    }

    public static void InvokeCommand(string commandName, DialogActionInfo actionInfo)
    {
        if(!CommandBehaviours.ContainsKey(commandName))
        {
            Debug.LogError($"Dialog Command '{commandName}' not found");
            return;
        }

        CommandBehaviours[commandName].Invoke(actionInfo);
    }

    public static void Clear()
    {
        CommandBehaviours.Clear();
    }
}