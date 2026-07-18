using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Полоска прогресса: fillAmount меняется пропорционально значению.
/// </summary>
public class ProgressBarUI : BarUI
{
    [SerializeField] private Image progressLine; // у изображения должен быть Fill Method = Horizontal|Vertical

    [Space]
    [SerializeField] private TMP_Text textCurrent;
    [SerializeField] private TMP_Text textRemains;
    [SerializeField] private string postfix;

    public override void ShowValueInInterface(float currentValue)
    {
        UpdateFill(CurrentValue, MaxValue);
    }

    public override void ShowMaxValueInInterface(float maxValue)
    {
        UpdateFill(CurrentValue, maxValue);
    }

    private void UpdateFill(float current, float max)
    {
        if (!progressLine) return;

        float amount = (max <= 0f) ? 0f : current / max;
        progressLine.fillAmount = Mathf.Clamp01(amount);

        if (textCurrent)
            textCurrent.text = $"{current}{postfix}";
        if (textRemains)
        {
            if (max - current > 0f)
                textRemains.text = $"{max - current}{postfix}";
            else
                textRemains.text = $"";
        }
    }
}