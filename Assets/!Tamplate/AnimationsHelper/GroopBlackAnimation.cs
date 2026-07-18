using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GroopBlackAnimation : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;

    [Header("Durations")]
    [SerializeField] private float fadeInDuration = 1f;     // Появление
    [SerializeField] private float waitDuration = 1f;       // Ожидание
    [SerializeField] private float fadeOutDuration = 1f;    // Исчезновение

    public void StartAnim()
    {
        group.alpha = 0f;
        group.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();

        // Появление
        seq.Append(group.DOFade(1f, fadeInDuration));

        // Ожидание
        seq.AppendInterval(waitDuration);

        // Исчезновение
        seq.Append(group.DOFade(0f, fadeOutDuration));

        // По завершению — можно выключить объект
        seq.OnComplete(() =>
        {
            group.gameObject.SetActive(false);
        });
    }
}