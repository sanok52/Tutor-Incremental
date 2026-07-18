using System;
using UnityEngine;

[Serializable]
public class FloatContainer : IValueContainer
{
    [SerializeField] private string title;
    public string Title => title;

    [SerializeField] private float _value;
    [SerializeField] private Vector2 _clampRange; // используем Vector2 вместо Vector2Int

    [Space]
    [SerializeField] private ValueContainerUISettings interfaceData;
    ValueContainerUISettings IValueContainer.InterfaceData => interfaceData;

    public event Action<ContainerChangeData> OnChangeValue;
    public event Action<int> OnOverfullValue;  // по условию интерфейса – int, но для float логичнее float
    public event Action<int> OnDownfullValue;  // оставлено как в интерфейсе, хотя лучше float

    public int Value => (int)_value;           // интерфейс требует int, поэтому приведение
    public Vector2Int ClampRange => new Vector2Int((int)_clampRange.x, (int)_clampRange.y); // приведение

    // Вспомогательные свойства для удобства
    public float FloatValue => _value;
    public Vector2 FloatClampRange => _clampRange;

    public void Init(float initialValue, Vector2Int minMaxRange)
    {
        _clampRange = new Vector2(minMaxRange.x, minMaxRange.y);
        SetValue(initialValue);
    }

    public FloatContainer(float initialValue, Vector2 minMaxRange)
    {
        _clampRange = minMaxRange;
        SetValue(initialValue);
    }

    // Основные методы – работают с float
    public void AddValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        if (changeTypeReal == ValueChangeType.None)
            changeTypeReal = ValueChangeType.Add;

        if (value < 0)
        {
            RemoveValue(-value, point, transform, changeTypeReal);
            return;
        }
        if (Mathf.Approximately(value, 0))
        {
            NotChangeValue(changeTypeReal, point, transform);
            return;
        }

        OffsetValue(value, ValueChangeType.Add, changeTypeReal, point, transform);
    }

    public void RemoveValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        if (changeTypeReal == ValueChangeType.None)
            changeTypeReal = ValueChangeType.Remove;

        if (value < 0)
        {
            AddValue(-value, point, transform, changeTypeReal);
            return;
        }
        if (Mathf.Approximately(value, 0))
        {
            NotChangeValue(changeTypeReal, point, transform);
            return;
        }

        OffsetValue(-value, ValueChangeType.Remove, changeTypeReal, point, transform);
    }

    public void SetValue(float value, Vector3? point = null,
        Transform transform = null, ValueChangeType changeTypeReal = ValueChangeType.None)
    {
        if (changeTypeReal == ValueChangeType.None)
            changeTypeReal = ValueChangeType.Set;

        if (Mathf.Approximately(value, _value))
        {
            NotChangeValue(changeTypeReal, point, transform);
            return;
        }

        OffsetValue(value - _value, ValueChangeType.Set, changeTypeReal, point, transform);
    }

    private void NotChangeValue(ValueChangeType changeTypeReal = ValueChangeType.None,
        Vector3? point = null, Transform transform = null)
    {
        OffsetValue(0, ValueChangeType.None, changeTypeReal, point, transform);
    }

    // Метод OffsetValue для float (перегрузка, требуемая интерфейсом, но интерфейс требует int – несоответствие!)
    // Поскольку в IValueContainer объявлен void OffsetValue(int value, ...), мы вынуждены сделать приведение.
    // Если вы измените интерфейс на float, этот метод станет основным.
    public void OffsetValue(float value, ValueChangeType changeType, ValueChangeType changeTypeReal,
        Vector3? point = null, Transform transform = null)
    {
        float valueMemory = _value;
        float valueChange = _value + value;
        ChangeValue(valueChange);

        float delta = _value - valueMemory;

        OnChangeValue?.Invoke(new ContainerChangeData(Title, _value, delta, value,
            changeType, changeTypeReal, interfaceData, transform, point));
    }

    // Интерфейс требует этот метод с int, поэтому реализуем его явно или вызываем float-версию
    public void OffsetValue(int value, ValueChangeType changeType, ValueChangeType changeTypeReal,
        Vector3? point = null, Transform transform = null)
    {
        OffsetValue((float)value, changeType, changeTypeReal, point, transform);
    }

    private void ChangeValue(float value)
    {
        _value = value;

        if (_value > _clampRange.y)
        {
            float overfull = _value - _clampRange.y;
            _value = _clampRange.y;
            OnOverfullValue?.Invoke((int)overfull); // приведение, так как событие требует int
        }
        else if (_value < _clampRange.x)
        {
            float downfull = _clampRange.x - _value;
            _value = _clampRange.x;
            OnDownfullValue?.Invoke((int)downfull);
        }
    }

    public IValueContainer Clone()
    {
        return new FloatContainer(_value, _clampRange) { title = this.title };
    }
}