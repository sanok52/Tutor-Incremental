
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Отвечает за проверку возможности и выполнение использования предмета.
/// </summary>
public static class ItemMediatorInvSys
{

    public static event Action<ItemBehContext> OnItemUse; // Событие для оповещения о том, что предмет был использован
    public static event Action<ItemBehContext> OnItemDrop; // Событие для оповещения о том, что предмет был выброшен

    /// <summary>
    /// Контекст использования. Пока пустой, будет расширен позже.
    /// </summary>
    public static ItemBehContext GetContext(ItemInstanceInvSys itemInstance)
    {
        return new ItemBehContext
        {
            ItemInstance = itemInstance
        };
    }

    /// <summary>
    /// Проверяет, можно ли использовать предмет.
    /// </summary>
    public static bool CanUse(ItemBehContrainerInvSys behaviour, ItemInstanceInvSys itemInstance) =>
        behaviour.CanUse(GetContext(itemInstance));

    /// <summary>
    /// Выполняет использование предмета.
    /// </summary>
    public static void Execute(ItemBehContrainerInvSys behaviour, ItemInstanceInvSys itemInstance)
    {
        if (behaviour == null)
            return;

        var context = GetContext(itemInstance);
        behaviour.ExecuteAll(context);
        OnItemUse?.Invoke(context);
    }

    /// <summary>
    /// Выполняет использование предмета.
    /// </summary>
    public static void ExecuteAllRoutine(ItemBehContrainerInvSys behaviour, ItemInstanceInvSys itemInstance)
    {
        if (behaviour == null)
            return;

        var context = GetContext(itemInstance);
        OnItemUse?.Invoke(context);
        ItemExecuterInvSys.Instance.Execute(behaviour, context);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void ExecuteDrop(ItemBehContrainerInvSys behaviour, ItemInstanceInvSys itemInstance)
    {
        if (behaviour == null)
            return;

        var context = GetContext(itemInstance);
        OnItemDrop?.Invoke(context);
        behaviour.ExecuteAllDrop(context);
    }

    /// <summary>
    /// Выполняет использование предмета.
    /// </summary>
    public static void ExecuteDropAllRoutine(ItemBehContrainerInvSys behaviour, ItemInstanceInvSys itemInstance)
    {
        if (behaviour == null)
            return;

        var context = GetContext(itemInstance);
        OnItemDrop?.Invoke(context);
        ItemExecuterInvSys.Instance.ExecuteDrop(behaviour, context);
    }
}