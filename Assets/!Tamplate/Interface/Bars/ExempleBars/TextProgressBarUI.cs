using UnityEngine;
using TMPro;

public class TextProgressBarUI : BarUI
{
    [Header("Text Bar Settings")]
    [SerializeField] private TMP_Text output;
    [SerializeField] private int barLength = 10; // длина текстовой полоски
    [SerializeField] private Gradient gradient = new Gradient() { alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }, colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.red, 1f) } }; //От белого к красному, например
    [SerializeField] private string prefix = "";
    [SerializeField] private string postfix = "";
    [SerializeField] private bool isShowValue = true;

    private void OnValidate()
    {
        if(!Application.isPlaying)
        {
            UpdateBar(MaxValue, MaxValue);
        }
    }

    public override void ShowValueInInterface(float currentValue)
    {
        UpdateBar(CurrentValue, MaxValue);
    }

    public override void ShowMaxValueInInterface(float maxValue)
    {
        UpdateBar(CurrentValue, maxValue);
    }

    private void UpdateBar(float current, float max)
    {
        if (!output) return;

        float amount = (max <= 0f) ? 0f : Mathf.Clamp01(current / max);

        int filled = Mathf.RoundToInt(amount * barLength);
        int empty = barLength - filled;

        string bar = new string('-', filled) + new string(' ', empty);

        string valueText = isShowValue ? $" {Mathf.RoundToInt(amount * 100)}" : "";

        output.text = $"{prefix}{bar}{valueText}{postfix}";
        output.color = gradient.Evaluate(amount);
    }
}
