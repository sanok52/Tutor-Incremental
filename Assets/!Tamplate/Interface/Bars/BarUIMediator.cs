using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BarUIMediator
{
    private readonly Dictionary<string, List<BarUI>> barUIDictionary = new();

    /// <summary>
    /// Пересобрать словарь: найти все BarUI в сцене (включая неактивные).
    /// </summary>
    public void UpdateLinks()
    {
        barUIDictionary.Clear();

        var bars = GameObject.FindObjectsByType<BarUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var bar in bars)
        {
            if (!barUIDictionary.TryGetValue(bar.ID, out var list))
            {
                list = new List<BarUI>();
                barUIDictionary[bar.ID] = list;
            }
            list.Add(bar);
        }
    }

    /// <summary>
    /// Выполнить общую ActionAtTaste&lt;BarUI&gt; по всем BarUI с данным TargetID.
    /// </summary>
    public void ActionForID(string id, Action<BarUI> handler)
        => ActionForID<BarUI>(id, handler);

    public void ActionForID<T>(string id, Action<T> handler) where T : BarUI
    {
        if (!barUIDictionary.TryGetValue(id, out var list)) return;
        foreach (var bar in list)
            if (bar is T typed)
                handler?.Invoke(typed);
    }

    /// <summary>
    /// Вызвать у всех BarUI с этим TargetID метод Show(currentValue).
    /// </summary>
    public void ShowForID(string id, float currentValue)
    {
        ActionForID(id, (BarUI bar) => bar.Show(currentValue));
    }

    /// <summary>
    /// Вызвать у всех BarUI с этим TargetID метод SetMaxValue(maxValue).
    /// </summary>
    public void SetMaxForID(string id, float maxValue)
    {
        ActionForID(id, (BarUI bar) => bar.SetMaxValue(maxValue));
    }

    public void ShowAll()
    {
        foreach (var barUIList in barUIDictionary.Values)
        {
            foreach (var barUI in barUIList)
            {
                //barUI.Show();
            }
        }
    }

    public BarUI[] GetBarsID (string id)
    {
        return barUIDictionary[id].ToArray();
    }

    internal void SetMaxForID(string v, object jumpsCountMax)
    {
        throw new NotImplementedException();
    }
}