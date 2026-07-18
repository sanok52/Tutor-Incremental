using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//Data-классы
[Serializable]
public abstract class SettingsDataData
{
    public string VariableName;
}

[Serializable]
public class SettingsDataSliderData : SettingsDataData
{
    public float Value = 1f;
}

[Serializable]
public class SettingsDataToggleData : SettingsDataData
{
    public bool Value = true;
}


//контракт для всех настроек
[Serializable]
public abstract class SettingsDataValue
{
    public string VariableName;

    public abstract void Apply(SettingsData data);
}


[Serializable]
public class SettingsDataSlider : SettingsDataValue
{
    [Range(0f, 1f)]
    public float DefaultValue = 1f;
    public Vector2 MinMaxDb = new(-40f, 0f);

    public override void Apply(SettingsData data)
    {
        // базовый slider ничего не делает
    }

    public virtual float MapValue(float value)
    {
        return value <= 0f ? -80f : Mathf.Lerp(MinMaxDb.x, MinMaxDb.y, value);
    }

    public virtual SettingsDataSliderData CreateDefaultData()
    {
        return new SettingsDataSliderData
        {
            VariableName = VariableName,
            Value = DefaultValue
        };
    }
}

[Serializable]
public class SettingsDataVolume : SettingsDataSlider
{
    public override void Apply(SettingsData data)
    {
        if (GameSettings.GlobalMixer == null) return;

        var sliderData = data.GetSlider(VariableName);
        if (sliderData == null) return;

        float db = MapValue(sliderData.Value);
        GameSettings.GlobalMixer.SetFloat(VariableName, db);
    }
}

[Serializable]
public class SettingsDataToggle : SettingsDataValue
{
    public bool DefaultValue = true;

    public override void Apply(SettingsData data)
    {
        // toggle не делает ничего сам по себе
    }

    public virtual SettingsDataToggleData CreateDefaultData()
    {
        return new SettingsDataToggleData
        {
            VariableName = VariableName,
            Value = DefaultValue
        };
    }
}