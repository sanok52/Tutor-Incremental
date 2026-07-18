using System.Collections.Generic;
using UnityEngine;

public class InGameWindowManager : MonoBehaviour
{
    private List<InGameWindow> windows = new List<InGameWindow>();

    public void AddInList (InGameWindow window)
    {
        if (!windows.Contains(window))
        {
            windows.Add(window);
        }
    }

    public void RemoveFromList (InGameWindow window)
    {
        if (windows.Contains(window))
        {
            windows.Remove(window);
        }
    }

    public void ShowWindow(string id)
    {
        ShowWindow(id, false);
    }

    public void ShowWindow(string id, bool hideOthers)
    {
        foreach (var window in windows)
        {
            if (window.ID == id)
            {
                window.Show(true);
            }
            else if (hideOthers)
            {
                window.Show(false);
            }
        }
    }
}