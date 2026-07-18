using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public interface IValueContainer
{
    public string Title { get; }

    public ValueContainerUISettings InterfaceData { get; }

    public event Action<ContainerChangeData> OnChangeValue;
    public event Action<int> OnOverfullValue;
    public event Action<int> OnDownfullValue;

    public int Value { get; }
    public Vector2Int ClampRange { get; }

    public abstract void Init(float initialValue, Vector2Int minMaxRange);
    public abstract void AddValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None);

    public abstract void RemoveValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None);

    public abstract void SetValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None);

    public abstract void OffsetValue(int value, ValueChangeType changeType, ValueChangeType changeTypeReal,
        Vector3? point = null, Transform transform = null);

    IValueContainer Clone();
}


public enum ValueChangeType { None, Add, Remove, Set }
public struct ContainerChangeData
{
    public string Title;
    public float CurrentValue;

    [Space]
    public float Delta;
    public float DeltaNotClamp;

    [Space]
    public ValueChangeType ChangeTypeReal;
    public ValueChangeType ChangeTypeStart;

    [Space]
    public Transform Transform;
    public Vector3? Point;
    public ValueContainerUISettings InterfaceSettings;

    public ContainerChangeData(ContainerChangeData data) : this()
    {
        Title = data.Title;
        CurrentValue = data.CurrentValue;
        Delta = data.Delta;
        DeltaNotClamp = data.DeltaNotClamp;
        ChangeTypeReal = data.ChangeTypeReal;
        ChangeTypeStart = data.ChangeTypeReal;
        Transform = data.Transform;
        Point = data.Point;
        InterfaceSettings = data.InterfaceSettings;
    }

    public ContainerChangeData(string title, float currentValue, float delta, float deltaNotClamp,
     ValueChangeType changeType, ValueChangeType changeTypeStart, ValueContainerUISettings interfaceSettings,
     Transform transform = null, Vector3? point = null)
    {
        Title = title;
        CurrentValue = currentValue;
        Delta = delta;
        DeltaNotClamp = deltaNotClamp;
        ChangeTypeReal = changeType;
        ChangeTypeStart = changeTypeStart;
        Transform = transform;
        Point = point;
        InterfaceSettings = interfaceSettings;
    }

    public Vector3? GetPoint()
    {
        return Point != null ? Point : (Transform != null ? Transform.position : null);
    }
}

[Serializable]
public struct ValueContainerUISettings
{
    public string Title;
    public ColorBetweenScore Color;
    public float SizeKof;
    public string Prefix;
    public string Postfix;

    public ValueContainerUISettings(string title, ColorBetweenScore color, float sizeKof, string prefix, string postfix)
    {
        Title = title;
        Color = color;
        SizeKof = sizeKof;
        Prefix = prefix;
        Postfix = postfix;
    }
}