using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Experimental.GlobalIllumination;

[CreateAssetMenu(fileName = "GameSettingsSO", menuName = "GameSettings")]
public class GameSettingsSO : ScriptableObject
{
    [SerializeField] private List<SettingsDataVolume> volumeSettingsDatas = new();
    [SerializeField] private List<SettingsDataSlider> sliderSettingsDatas = new();
    [SerializeField] private List<SettingsDataToggle> toggleSettingsDatas = new();

    public IReadOnlyList<SettingsDataVolume> VolumeSettingsDatas => volumeSettingsDatas;
    public IReadOnlyList<SettingsDataSlider> SliderSettingsDatas => sliderSettingsDatas;
    public IReadOnlyList<SettingsDataToggle> ToggleSettingsDatas => toggleSettingsDatas;
}