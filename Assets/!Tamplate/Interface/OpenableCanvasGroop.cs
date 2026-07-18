using DG.Tweening;
using UnityEngine;

public class OpenableCanvasGroop : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    private bool isOpen;

    public void ClickButton()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    private void Close()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.DOFade(0f, 0.5f);
        isOpen = false;
    }

    public void Open()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOFade(1f, 0.5f);
        isOpen = true;
    }
}
