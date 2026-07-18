using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public static class GameSettings
{
    private const string SETTINGS_KEY = "SettingsJSON";

    public static AudioMixer GlobalMixer { get; private set; }
    public static GameSettingsSO SettingsSO { get; private set; }
    public static SettingsData Data { get; private set; }

    public static event Action<SettingsData> OnSettingsChange;

    private static bool _initialized;

    public static void Init()
    {
        Debug.Log("Settings Iint");
        if (_initialized) return;
        _initialized = true;

        LoadResources();
        LoadSettings();
        DOVirtual.DelayedCall(Time.deltaTime, InitInternal);
    }

    private static void InitInternal()
    {
        ApplySettings();
    }

    private static void LoadResources()
    {
        GlobalMixer = Resources.Load<AudioMixer>("Settings/GlobalMixer");
        SettingsSO = Resources.Load<GameSettingsSO>("Settings/GameSettingsSO");
    }

    // -------------------- LOAD / SAVE --------------------

    public static void LoadSettings()
    {
        Data = new SettingsData();

        if (PlayerPrefs.HasKey(SETTINGS_KEY))
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(SETTINGS_KEY), Data);

        foreach (var setting in GetAllSettings())
        {
            switch (setting)
            {
                case SettingsDataSlider slider:
                    if (Data.GetSlider(slider.VariableName) == null)
                        Data.Sliders.Add(slider.CreateDefaultData());
                    break;
                case SettingsDataToggle toggle:
                    if (Data.GetToggle(toggle.VariableName) == null)
                        Data.Toggles.Add(toggle.CreateDefaultData());
                    break;
            }
        }
    }

    public static void SaveSettings()
    {
        PlayerPrefs.SetString(SETTINGS_KEY, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    // -------------------- APPLY --------------------

    public static void ApplySettings()
    {
        foreach (var setting in GetAllSettings())
        {
            setting.Apply(Data);
        }

        OnSettingsChange?.Invoke(Data);
    }

    // -------------------- SET / GET --------------------

    public static void SetValue(string key, float value)
    {
        var data = Data.GetSlider(key);
        if (data == null) return;

        data.Value = Mathf.Clamp01(value);
        ApplySettings();
        SaveSettings();
    }

    public static void SetValue(string key, bool value)
    {
        var data = Data.GetToggle(key);
        if (data == null) return;

        data.Value = value;
        ApplySettings();
        SaveSettings();
    }

    public static float GetValue(string key)
    {
        var data = Data.GetSlider(key);
        return data != null ? data.Value : -1f;
    }

    public static bool GetToggle(string key)
    {
        var data = Data.GetToggle(key);
        return data != null && data.Value;
    }

    public static float GetMappedValue(string key)
    {
        var setting = GetAllSettings().FirstOrDefault(s => s.VariableName == key) as SettingsDataSlider;
        if (setting == null) return -1f;

        float value = GetValue(key);
        return value < 0f ? -1f : setting.MapValue(value);
    }

    // -------------------- INTERNAL --------------------

    private static IEnumerable<SettingsDataValue> GetAllSettings()
    {
        if(SettingsSO == null)
        {
            Debug.LogWarning($"SettingsSO == null");
            yield break;
        }

        foreach (var s in SettingsSO.SliderSettingsDatas)
            yield return s;

        foreach (var v in SettingsSO.VolumeSettingsDatas)
            yield return v;

        foreach (var t in SettingsSO.ToggleSettingsDatas)
            yield return t;
    }
}

[Serializable]
public class SettingsData
{
    public List<SettingsDataSliderData> Sliders = new();
    public List<SettingsDataToggleData> Toggles = new();

    public SettingsDataSliderData GetSlider(string name) =>
        Sliders.FirstOrDefault(s => s.VariableName == name);

    public SettingsDataToggleData GetToggle(string name) =>
        Toggles.FirstOrDefault(t => t.VariableName == name);
}