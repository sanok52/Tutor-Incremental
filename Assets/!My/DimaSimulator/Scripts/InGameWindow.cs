using System;
using UnityEngine;

public class InGameWindow : MonoBehaviour
{
    [SerializeField] private string id;

    private CanvasGroup canvasGroup;

    public string ID => id;

    public void Show(bool v)
    {
        canvasGroup.alpha = v ? 1f : 0f;
        canvasGroup.interactable = v;
        canvasGroup.blocksRaycasts = v;
    }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        FindFirstObjectByType<InGameWindowManager>().AddInList(this);
    }

    private void OnDestroy()
    {
        FindFirstObjectByType<InGameWindowManager>().RemoveFromList(this);
    }
}