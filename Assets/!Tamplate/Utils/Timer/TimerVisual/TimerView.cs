using System;
using System.Collections;
using UnityEngine;

public enum TimerViewType { None, Scale };
public class TimerView : MonoBehaviour
{
    [SerializeField] private TimerViewType timerViewType;
    [SerializeField] private Vector2 minMaxScale = new Vector2(0f, 1f);
    [SerializeField] private SpriteRenderer view;
    [SerializeField] private Gradient gradient;

    private Vector2 minMaxValue;
    private float currentValue;

    private void Start()
    {
        Stop();
    }

    public void Init (float min, float max, float? value = null)
    {
        view.gameObject.SetActive(true);
        minMaxValue = new Vector2(min, max);
        currentValue = value == null ? max : value.Value;
    }

    public void UpdateValue(float value)
    {
        currentValue = value;
        float unlerpValue = 1f - Mathf.InverseLerp(minMaxValue.x, minMaxValue.y, value);
        float valueLocal = Mathf.Lerp(minMaxScale.x, minMaxScale.y, unlerpValue);

        switch (timerViewType)
        {
            case TimerViewType.Scale:
                transform.localScale = Vector3.one * valueLocal;
                break;
        }

        view.color = gradient.Evaluate(unlerpValue);
    }

    public void Stop()
    {
        view.gameObject.SetActive(false);
        currentValue = minMaxValue.y;
    }
}