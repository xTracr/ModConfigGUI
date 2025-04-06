using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

namespace ModConfigGUI.Config
{

public static class AcceptableValueHelper
{
    public static bool IsList(this AcceptableValueBase? acceptableValue)
    {
        if (acceptableValue is null) return false;
        Type acceptableValueType = acceptableValue.GetType();
        return acceptableValueType.IsGenericType && acceptableValueType.GetGenericTypeDefinition() == typeof(AcceptableValueList<>);
    }

    public static bool IsRange(this AcceptableValueBase? acceptableValue)
    {
        if (acceptableValue is null) return false;
        Type acceptableValueType = acceptableValue.GetType();
        return acceptableValueType.IsGenericType && acceptableValueType.GetGenericTypeDefinition() == typeof(AcceptableValueRange<>);
    }

    public static object[]? GetListedValues(this AcceptableValueBase? acceptableValue)
    {
        PropertyInfo? valuesProperty = acceptableValue?.GetType().GetProperty("AcceptableValues", BindingFlags.Public | BindingFlags.Instance);
        if (!(valuesProperty?.GetValue(acceptableValue) is Array array)) return null;
        return array.Cast<object>().ToArray();
    }

    public static object[]? GetBound(this AcceptableValueBase? acceptableValue)
    {
        if (acceptableValue is null) return null;
        Type acceptableValueType = acceptableValue.GetType();
        PropertyInfo? minValueProperty = acceptableValueType.GetProperty("MinValue", BindingFlags.Public | BindingFlags.Instance);
        PropertyInfo? maxValueProperty = acceptableValueType.GetProperty("MaxValue", BindingFlags.Public | BindingFlags.Instance);
        if (minValueProperty is null || maxValueProperty is null) return null;
        return new[] { minValueProperty.GetValue(acceptableValue), maxValueProperty.GetValue(acceptableValue) };
    }
}

}