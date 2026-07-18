using System;
using UnityEngine;

[Serializable]
public class IntContainer : IValueContainer
{
    [SerializeField] private string title;
    public string Title => title;

    [SerializeField] private int _value;
    [SerializeField] private Vector2Int _clampRange;

    [Space]
    [SerializeField] private ValueContainerUISettings interfaceData;
    ValueContainerUISettings IValueContainer.InterfaceData => interfaceData;

    public event Action<ContainerChangeData> OnChangeValue;
    public event Action<int> OnOverfullValue;
    public event Action<int> OnDownfullValue;

    public int Value => _value;
    public Vector2Int ClampRange => _clampRange;

    public void Init(float initialValue, Vector2Int minMaxRange)
    {
        _clampRange = minMaxRange;
        SetValue(initialValue);
    }

    public IntContainer(int initialValue, Vector2Int minMaxRange)
    {
        _clampRange = minMaxRange;
        SetValue(initialValue);
    }

    public void AddValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        AddValue((int)value, point, transform, changeTypeReal);
    }

    public void AddValue(int value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        if (changeTypeReal == ValueChangeType.None)
            changeTypeReal = ValueChangeType.Add;

        if (value < 0)
        {
            RemoveValue(-value, point, transform, changeTypeReal);
            return;
        }
        else if (value == 0)
        {
            NotChangeValue(changeTypeReal, point, transform);
            return;
        }

        OffsetValue(value, ValueChangeType.Add, changeTypeReal, point, transform);
    }

    public void RemoveValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        RemoveValue((int)value, point, transform, changeTypeReal);
    }

    public void RemoveValue(int value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        if (changeTypeReal == ValueChangeType.None)
            changeTypeReal = ValueChangeType.Add;

        if (value < 0)
        {
            AddValue(-value, point, transform, changeTypeReal);
            return;
        }
        else if (value == 0)
        {
            NotChangeValue(changeTypeReal, point, transform);
            return;
        }

        OffsetValue(-value, ValueChangeType.Remove, changeTypeReal, point, transform);
    }

    public void SetValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        SetValue((int)value, point, transform, changeTypeReal);
    }

    public void SetValue(int value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        if (changeTypeReal == ValueChangeType.None)
            changeTypeReal = ValueChangeType.Set;

        if (value == _value)
        {
            NotChangeValue(changeTypeReal, point, transform);
            return;
        }

        OffsetValue(value - _value, ValueChangeType.Set, changeTypeReal, point, transform);
    }

    private void NotChangeValue(ValueChangeType changeTypeReal = ValueChangeType.None, Vector3? point = null, Transform transform = null)
    {
        OffsetValue(0, ValueChangeType.None, changeTypeReal, point, transform);
    }

    public void OffsetValue(float value, ValueChangeType changeType, ValueChangeType changeTypeReal,
        Vector3? point = null, Transform transform = null)
    {
        OffsetValue((int)value, changeType, changeTypeReal, point, transform);
    }

    public void OffsetValue (int value, ValueChangeType changeType, ValueChangeType changeTypeReal, 
        Vector3? point = null, Transform transform = null)
    {
        int valueMemory = _value;

        int valueChange = _value + value;

        ChangeValue(valueChange);

        int delta = _value - valueMemory;

        OnChangeValue?.Invoke(new ContainerChangeData(Title, _value, delta, value, changeType, changeTypeReal, interfaceData, transform, point));
    }

    private void ChangeValue(int value)
    {
        _value = value;

        if (_value > _clampRange.y)
        {
            int overfull = _clampRange.y - _value;
            _value = _clampRange.y;
            OnOverfullValue?.Invoke(overfull);
        }
        else if (_value < 0)
        {
            int downfull = _clampRange.x - _value;
            _value = _clampRange.x;
            OnOverfullValue?.Invoke(downfull);
        }
    }

    public IValueContainer Clone()
    {
        return new IntContainer(_value, _clampRange) { title = this.title, interfaceData = interfaceData };
    }
}