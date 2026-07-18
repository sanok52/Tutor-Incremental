using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : MonoBehaviour
{
    [SerializeField] private List<SliderToSettings> sliders = new();
    [SerializeField] private List<ToggleToSettings> toggles = new();

    private void Awake()
    {
        Validate();
    }

    private void OnEnable()
    {
        Subscribe();
        SyncFromSettings();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    // -------------------- SUBSCRIBE --------------------

    private void Subscribe()
    {
        foreach (var entry in sliders)
        {
            var e = entry; // защита от замыкания
            e.Slider.onValueChanged.AddListener(
                value => OnSliderChanged(e.VariableName, value)
            );
        }

        foreach (var entry in toggles)
        {
            var e = entry; // защита от замыкания
            e.Slider.onValueChanged.AddListener(
                value => OnToggleChange(e.VariableName, value)
            );
        }

        GameSettings.OnSettingsChange += OnSettingsChanged;
    }

    private void Unsubscribe()
    {
        foreach (var entry in sliders)
        {
            entry.Slider.onValueChanged.RemoveAllListeners();
        }

        GameSettings.OnSettingsChange -= OnSettingsChanged;
    }

    // -------------------- CALLBACKS --------------------

    private void OnSliderChanged(string variableName, float value)
    {
        GameSettings.SetValue(variableName, value);
    }

    private void OnToggleChange(string variableName, bool value)
    {
        GameSettings.SetValue(variableName, value);
    }

    private void OnSettingsChanged(SettingsData data)
    {
        SyncFromSettings();
    }

    // -------------------- SYNC --------------------

    private void SyncFromSettings()
    {
        foreach (var entry in sliders)
        {
            float value = GameSettings.GetValue(entry.VariableName);
            if (value < 0f) continue;

            entry.Slider.SetValueWithoutNotify(value);
        }
    }

    // -------------------- VALIDATION --------------------

    private void Validate()
    {
        foreach (var entry in sliders)
        {
            if (entry.Slider == null)
            {
                Debug.LogError(
                    $"SettingsWindow: Slider is missing for setting '{entry.VariableName}'",
                    this
                );
            }
        }
    }
}


[Serializable]
public class SliderToSettings
{
    public string VariableName;
    public Slider Slider;
}


[Serializable]
public class ToggleToSettings
{
    public string VariableName;
    public Toggle Slider;
}