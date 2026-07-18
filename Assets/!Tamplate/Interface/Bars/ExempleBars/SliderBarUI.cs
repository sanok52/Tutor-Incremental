using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderBarUI : BarUI
{
    [SerializeField] private Slider slider;

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
        if (!slider) return;

        float amount = (max <= 0f) ? 0f : current / max;
        slider.value = Mathf.Clamp01(amount);

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