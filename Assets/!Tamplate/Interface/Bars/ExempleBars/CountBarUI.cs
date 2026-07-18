using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Показывает счётчик (например, «3/10» или «3»).
/// </summary>
public class CountBarUI : BarUI
{
    [Header("UI")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private bool isUseMax = true;               // true → "value/max", false → "value" только

    [Header("Animated Change (DOTween)")]
    [SerializeField] private bool animateValueChange = false;    // вкл / выкл анимацию
    [SerializeField] private float animationDuration = 0.35f;    // длительность
    [SerializeField] private Ease animationEase = Ease.OutQuad;  // тип кривой

    [Space]
    [SerializeField, TextArea(2, 1)] private string prefix;
    [SerializeField, TextArea(2, 1)] private string postfix;

    private int maxCount;
    private float displayedValue;                               // текущее показанное значение (для анимации)
    private Tween valueTween;                                   // активный Tween (если есть)

    /* ────────────────────────────── BarUI overrides ────────────────────────────── */

    public override void ShowValueInInterface(float currentValue)
    {
        if (!text) return;

        if (animateValueChange && Application.isPlaying)
        {
            // останавливаем прошлый твин, если ещё работает
            valueTween?.Kill();
            float start = displayedValue;
            float end = currentValue;

            valueTween = DOTween.To(() => start, x =>
            {
                displayedValue = x;
                UpdateText(Mathf.RoundToInt(x));
            }, end, animationDuration)
            .SetEase(animationEase)
            .OnKill(() => valueTween = null);
        }
        else
        {
            displayedValue = currentValue;
            UpdateText(Mathf.RoundToInt(currentValue));
        }
    }

    public override void ShowMaxValueInInterface(float maxValue)
    {
        maxCount = Mathf.RoundToInt(maxValue);
        // сразу обновляем отображение текущего значения (без анимации, чтобы не сбивать tween)
        UpdateText(Mathf.RoundToInt(displayedValue));
    }

    /* ───────────────────────────────────────────────────────────────────────────── */

    private void UpdateText(int cur)
    {
        text.text = prefix + (isUseMax ? $"{cur}/{maxCount}" : cur.ToString()) + postfix;
    }

    private void OnDisable()
    {
        valueTween?.Kill(); // чистим tween при выключении объекта
    }
}