using System;
using UnityEngine;

public class CleanPlayerInteraction : MonoBehaviour
{
    public event Action OnInteraction;

    public void Interact()
    {
        OnInteraction?.Invoke();
    }
}
