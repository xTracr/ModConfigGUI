using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using ModConfigGUI.UI;
using UnityEngine;

namespace ModConfigGUI.Config
{

public class SupportedType
{
    static readonly Dictionary<Type, SupportedType> DefaultTypes = new Dictionary<Type, SupportedType>();
    public static readonly SupportedType String = GetOrCreate(typeof(string));
    public static readonly SupportedType Bool;
    public static readonly SupportedType Byte;
    public static readonly SupportedType SByte;
    public static readonly SupportedType Short;
    public static readonly SupportedType UShort;
    public static readonly SupportedType Int;
    public static readonly SupportedType UInt;
    public static readonly SupportedType Long;
    public static readonly SupportedType ULong;
    public static readonly SupportedType Float;
    public static readonly SupportedType Double;
    public static readonly SupportedType Decimal;
    public static readonly SupportedType KeyCode = CreateBuilder(typeof(KeyCode)).SetDefaultEntryType(EntryType.Keymap).Build();
    readonly EntryType _defaultEntryType;
    readonly string[]? _defaultOptions;
    readonly Func<object, string> _toString;
    readonly Func<string, object> _toValue;
    readonly Func<AcceptableValueBase, string[]?> _listToOptions;
    readonly Func<AcceptableValueBase, string[]?>? _rangeToOptions;
    public Type Type { get; }
    public bool SupportsRange => !(_rangeToOptions is null);

    static SupportedType()
    {
        const int count = 100;
        Bool = CreateBuilder(typeof(bool))
            .SetDefaultEntryType(EntryType.Cycling)
            .SetToString(obj => obj.ToString().ToLowerInvariant())
            .SetToValue(str => bool.Parse(str))
            .SetDefaultOptions(new[] { true, false })
            .Build();
        Byte = CreateBuilder(typeof(byte))
            .SetToString(obj => obj.ToString())
            .SetToValue(str => byte.Parse(str))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                return bound is null ? null : Enumerable.Range((byte)bound[0], (byte)bound[1] + 1).Select(i => i.ToString()).ToArray();
            }).Build();
        SByte = CreateBuilder(typeof(sbyte))
            .SetToString(obj => obj.ToString())
            .SetToValue(str => sbyte.Parse(str))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                return bound is null ? null : Enumerable.Range((sbyte)bound[0], (sbyte)bound[1] + 1).Select(i => i.ToString()).ToArray();
            }).Build();
        Short = CreateBuilder(typeof(short))
            .SetToString(obj => obj.ToString())
            .SetToValue(str => short.Parse(str))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                return bound is null ? null : Enumerable.Range((short)bound[0], (short)bound[1] + 1).Select(i => i.ToString()).ToArray();
            }).Build();
        UShort = CreateBuilder(typeof(ushort))
            .SetToString(obj => obj.ToString())
            .SetToValue(str => ushort.Parse(str))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                return bound is null ? null : Enumerable.Range((ushort)bound[0], (ushort)bound[1] + 1).Select(i => i.ToString()).ToArray();
            }).Build();
        Int = CreateBuilder(typeof(int))
            .SetToString(obj => obj.ToString())
            .SetToValue(str => int.Parse(str))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                return bound is null ? null : Enumerable.Range((int)bound[0], (int)bound[1] + 1).Select(i => i.ToString()).ToArray();
            }).Build();
        UInt = CreateBuilder(typeof(uint))
            .SetToString(obj => obj.ToString())
            .SetToValue(str => uint.Parse(str))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                if (bound is null) return null;
                uint lower = (uint)bound[0];
                uint upper = (uint)bound[1];
                return Enumerable.Range(0, (int)(upper - lower + 1)).Select(i => (lower + i).ToString()).ToArray();
            }).Build();
        Long = CreateBuilder(typeof(long))
            .SetToString(obj => obj.ToString())
            .SetToValue(str => long.Parse(str))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                if (bound is null) return null;
                long lower = (long)bound[0];
                long upper = (long)bound[1];
                long step = (upper - lower) / count;
                return Enumerable.Range(0, count + 1).Select(i => (lower + i * step).ToString()).ToArray();
            }).Build();
        ULong = CreateBuilder(typeof(ulong))
            .SetToString(obj => obj.ToString())
            .SetToValue(str => ulong.Parse(str))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                if (bound is null) return null;
                ulong lower = (ulong)bound[0];
                ulong upper = (ulong)bound[1];
                ulong step = (upper - lower) / count;
                return Enumerable.Range(0, count + 1).Select(i => (lower + (ulong)i * step).ToString()).ToArray();
            }).Build();
        Float = CreateBuilder(typeof(float))
            .SetToString(obj => ((float)obj).ToString(NumberFormatInfo.InvariantInfo))
            .SetToValue(str => float.Parse(str, NumberFormatInfo.InvariantInfo))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                if (bound is null) return null;
                float lower = (float)bound[0];
                float upper = (float)bound[1];
                float step = (upper - lower) / count;
                return Enumerable.Range(0, count + 1).Select(i => (lower + i * step).ToString(NumberFormatInfo.InvariantInfo)).ToArray();
            }).Build();
        Double = CreateBuilder(typeof(double))
            .SetToString(obj => ((double)obj).ToString(NumberFormatInfo.InvariantInfo))
            .SetToValue(str => double.Parse(str, NumberFormatInfo.InvariantInfo))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                if (bound is null) return null;
                double lower = (double)bound[0];
                double upper = (double)bound[1];
                double step = (upper - lower) / count;
                return Enumerable.Range(0, count + 1).Select(i => (lower + i * step).ToString(NumberFormatInfo.InvariantInfo)).ToArray();
            }).Build();
        Decimal = CreateBuilder(typeof(decimal))
            .SetToString(obj => ((decimal)obj).ToString(NumberFormatInfo.InvariantInfo))
            .SetToValue(str => decimal.Parse(str, NumberFormatInfo.InvariantInfo))
            .SetRangeToOptions(acceptableValue =>
            {
                object[]? bound = acceptableValue.GetBound();
                if (bound is null) return null;
                decimal lower = (decimal)bound[0];
                decimal upper = (decimal)bound[1];
                decimal step = (upper - lower) / count;
                return Enumerable.Range(0, count + 1).Select(i => (lower + i * step).ToString(NumberFormatInfo.InvariantInfo)).ToArray();
            }).Build();
        foreach (Type type in TomlTypeConverter.GetSupportedTypes()) GetOrCreate(type);
    }

    SupportedType(Type type, EntryType defaultEntryType, Func<object, string> toString, Func<string, object> toValue, string[]? defaultOptions,
        Func<AcceptableValueBase, string[]?> listToOptions, Func<AcceptableValueBase, string[]?>? rangeToOptions)
    {
        Type = type;
        _defaultEntryType = defaultEntryType;
        _toString = toString;
        _toValue = toValue;
        _defaultOptions = defaultOptions;
        _listToOptions = listToOptions;
        _rangeToOptions = rangeToOptions;
    }

    public static Builder CreateBuilder(Type type) => new Builder(type);

    public static IReadOnlyDictionary<Type, SupportedType> GetDefaultTypes() => DefaultTypes;

    public static SupportedType? TryGetDefault(Type type) => DefaultTypes.TryGetValue(type, out SupportedType supportedType) ? supportedType : null;

    public static SupportedType GetOrCreate(Type type) => TryGetDefault(type) ?? CreateBuilder(type).Build();

    public string ToString(object value) => _toString(value);

    public object ToValue(string serializedValue) => _toValue(serializedValue);

    public EntryType GetDefaultEntryType(AcceptableValueBase? acceptableValue = null)
    {
        if (acceptableValue.IsList()) return EntryType.Dropdown;
        if (acceptableValue.IsRange() && SupportsRange) return EntryType.Slider;
        return _defaultEntryType;
    }

    public string[]? GetOptions(AcceptableValueBase? acceptableValue, object[]? shouldContain = null)
    {
        string[]? options = acceptableValue is null ? null : _rangeToOptions?.Invoke(acceptableValue) ?? _listToOptions(acceptableValue);
        if (options is null || shouldContain is null) return _defaultOptions;
        var result = new List<string>();
        List<string> newOptions = shouldContain.Where(value => value.GetType() == Type).Select(_toString).Where(str => !options.Contains(str)).ToList();
        result.AddRange(newOptions);
        result.AddRange(options);
        MethodInfo? compareTo = Type.GetMethod("CompareTo", new[] { Type });
        if (compareTo is null || newOptions.Count == 0) return result.ToArray();
        result.Sort((a, b) => (int)compareTo.Invoke(_toValue(a), new[] { _toValue(b) }));
        return result.ToArray();
    }

    public class Builder
    {
        readonly Type _type;
        EntryType? _defaultEntryType;
        Array? _defaultOptions;
        Func<object, string>? _toString;
        Func<string, object>? _toValue;
        Func<AcceptableValueBase, string[]?>? _listToOptions;
        Func<AcceptableValueBase, string[]?>? _rangeToOptions;
        
        internal Builder(Type type) => _type = type;

        public Builder SetDefaultEntryType(EntryType defaultEntryType)
        {
            _defaultEntryType = defaultEntryType;
            return this;
        }

        public Builder SetDefaultOptions(Array defaultOptions)
        {
            _defaultOptions = defaultOptions;
            return this;
        }

        public Builder SetToString(Func<object, string> toString)
        {
            _toString = toString;
            return this;
        }

        public Builder SetToValue(Func<string, object> toValue)
        {
            _toValue = toValue;
            return this;
        }

        public Builder SetListToOptions(Func<AcceptableValueBase, string[]?> listToOptions)
        {
            _listToOptions = listToOptions;
            return this;
        }

        public Builder SetRangeToOptions(Func<AcceptableValueBase, string[]?> rangeToOptions)
        {
            _rangeToOptions = rangeToOptions;
            return this;
        }

        public SupportedType Build(bool addToDefaultTypes = true, bool overrideDefault = false)
        {
            _defaultEntryType ??= _type.IsEnum ? (EntryType)EntryType.Dropdown : EntryType.InputField;
            _toString ??= value => TomlTypeConverter.ConvertToString(value, _type);
            _toValue ??= str => TomlTypeConverter.ConvertToValue(str, _type);
            string[]? serializedOptions = _defaultOptions?.Cast<object>().Select(_toString).ToArray() ?? (_type.IsEnum ? Enum.GetNames(_type) : null);
            _listToOptions ??= acceptableValue =>
            {
                object[]? listedValues = acceptableValue.GetListedValues();
                return listedValues is null ? null : Array.ConvertAll(listedValues, obj => _toString(obj));
            };
            var supportedType = new SupportedType(_type, _defaultEntryType, _toString, _toValue, serializedOptions, _listToOptions, _rangeToOptions);
            if (overrideDefault || (addToDefaultTypes && !DefaultTypes.ContainsKey(_type))) DefaultTypes[_type] = supportedType;
            return supportedType;
        }
    }
}

}