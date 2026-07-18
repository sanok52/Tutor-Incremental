using System;
using System.Collections.Generic;
using UnityEngine;
// ћедиатор условий Ц исправлен с учЄтом вашего пожелани€ возвращать true при ошибке
public static class DialogConditionMediator
{
    public static Dictionary<string, Func<float>> ValuesFl = new Dictionary<string, Func<float>>();
    public static Dictionary<string, Func<string>> ValuesStr = new Dictionary<string, Func<string>>();
    public static Dictionary<string, Func<float, float, bool>> CompareFl = new Dictionary<string, Func<float, float, bool>>();
    public static Dictionary<string, Func<string, string, bool>> CompareStr = new Dictionary<string, Func<string, string, bool>>();

    // ≈сли хотите при ошибке возвращать false, помен€йте этот флаг
    public static bool ReturnTrueOnMissingValue = true;

    public static bool TestCondition(DialogTransitionCondition condition)
    {
        if (condition == null) return true; // нет услови€ Ц всегда истина

        if (ValuesFl.ContainsKey(condition.ValueName))
            return TestConditionFloat(condition.ValueName, condition.ValueFl, condition.CompareCommand);
        if (ValuesStr.ContainsKey(condition.ValueName))
            return TestConditionString(condition.ValueName, condition.ValueStr, condition.CompareCommand);

        Debug.LogError($"Value '{condition.ValueName}' not found in DialogConditionMediator");
        return ReturnTrueOnMissingValue; // по умолчанию true, как в оригинале
    }

    private static bool TestConditionString(string valueName, string valueStr, string compareCommand)
    {
        if (!ValuesStr.ContainsKey(valueName))
        {
            Debug.LogError($"String provider for '{valueName}' not found");
            return ReturnTrueOnMissingValue;
        }
        if (!CompareStr.ContainsKey(compareCommand))
        {
            Debug.LogError($"Comparator '{compareCommand}' not found for string");
            return ReturnTrueOnMissingValue;
        }
        return CompareStr[compareCommand].Invoke(ValuesStr[valueName].Invoke(), valueStr);
    }

    private static bool TestConditionFloat(string valueName, float valueFl, string compareCommand)
    {
        if (!ValuesFl.ContainsKey(valueName))
        {
            Debug.LogError($"Float provider for '{valueName}' not found");
            return ReturnTrueOnMissingValue;
        }
        if (!CompareFl.ContainsKey(compareCommand))
        {
            Debug.LogError($"Comparator '{compareCommand}' not found for float");
            return ReturnTrueOnMissingValue;
        }
        return CompareFl[compareCommand].Invoke(ValuesFl[valueName].Invoke(), valueFl);
    }

    public static void Clear()
    {
        ValuesFl.Clear();
        ValuesStr.Clear();
        CompareFl.Clear();
        CompareStr.Clear();
    }
}
