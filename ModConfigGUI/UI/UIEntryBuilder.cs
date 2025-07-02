using System;
using BepInEx.Configuration;
using ModConfigGUI.Config;
using UnityEngine;

namespace ModConfigGUI.UI
{

public class UIEntryBuilder
{
    object _value;
    protected readonly SupportedType _supportedType;
    protected Color? _nameColor;
    protected int _width = 180;
    protected EntryType? _entryType;
    protected object? _defaultValue;
    protected TooltipData? _tooltip;
    protected AcceptableValueBase? _acceptableValue;
    protected Action<object>? _onSave;
    protected Func<object>? _onLoad;
    public string Name { get; }
    public object Value
    {
        get => _value;
        set
        {
            if (_acceptableValue != null && !_acceptableValue.IsValid(value))
            {
                object[]? values;
                if ((values = _acceptableValue.GetListedValues()) != null)
                    throw new ArgumentException($"Value not in options: {string.Join(", ", Array.ConvertAll(values, _supportedType.ToString))}");
                if ((values = _acceptableValue.GetBound()) != null)
                    throw new ArgumentException($"Value not in range: [{_supportedType.ToString(values[0])}, {_supportedType.ToString(values[1])}]");
                throw new ArgumentException("Invalid value: " + value);
            }
            if (Equals(_value, value = _acceptableValue?.Clamp(value) ?? value)) return;
            _value = value;
        }
    }

    protected UIEntryBuilder(string name, object value, SupportedType supportedType)
    {
        Name = name;
        _value = value;
        _supportedType = supportedType;
    }

    public static UIEntryBuilder Create(string name, Type type, object value, SupportedType? supportedType = null)
    {
        if (!type.IsInstanceOfType(value)) throw new ArgumentException($"Value is not of type '{type.Name}'");
        if (supportedType is null || supportedType.Type != type) supportedType = SupportedType.TryGetDefault(type);
        if (supportedType != null) return new UIEntryBuilder(name, value, supportedType);
        if (type.IsEnum) supportedType = SupportedType.GetOrCreate(type);
        else throw new ArgumentException($"Type '{type.Name}' is not supported by the Mod Config GUI");
        return new UIEntryBuilder(name, value, supportedType);
    }

    public UIEntryBuilder SetNameColor(Color? nameColor)
    {
        _nameColor = nameColor;
        return this;
    }

    public UIEntryBuilder SetWidth(int width)
    {
        _width = width;
        return this;
    }

    public UIEntryBuilder SetEntryType(EntryType entryType)
    {
        _entryType = entryType;
        return this;
    }

    public UIEntryBuilder SetDefaultValue(object defaultValue)
    {
        _defaultValue = defaultValue;
        return this;
    }

    public UIEntryBuilder SetTooltip(string tooltip) => SetTooltip(new TooltipData
        { enable = !string.IsNullOrEmpty(tooltip), icon = true, offset = new Vector3(0, -10), id = "", lang = "", text = tooltip });

    public UIEntryBuilder SetTooltip(TooltipData tooltip)
    {
        _tooltip = tooltip;
        return this;
    }

    public UIEntryBuilder SetAcceptableValue(AcceptableValueBase acceptableValue)
    {
        _acceptableValue = acceptableValue;
        return this;
    }

    public UIEntryBuilder SetOnSave(Action<object> onSave)
    {
        _onSave = onSave;
        return this;
    }

    public UIEntryBuilder SetOnLoad(Func<object> onLoad)
    {
        _onLoad = onLoad;
        return this;
    }

    public void Save() => _onSave?.Invoke(Value);

    public void Load() => Value = LoadValue();

    public object LoadValue() => _onLoad?.Invoke() ?? Value;

    public UIEntry Build(UIEntry source)
    {
        source.valueGetter = () => _supportedType.ToString(Value);
        source.onValueSet.RemoveAllListeners();
        source.onValueSet.AddListener(str => Value = _supportedType.ToValue(str));
        source.Text.SetText(Name);
        source.Text.color = _nameColor ?? source.Text.color;
        if (!(_tooltip is null)) source.textButton.tooltip = _tooltip;
        _entryType ??= _supportedType.GetDefaultEntryType(_acceptableValue);
        source.SetEntryType(_entryType, _supportedType.GetOptions(_acceptableValue, _defaultValue is null ? null : new[] { _defaultValue }));
        source.Init(_width);
        if (_defaultValue is null) return source;
        source.resetButton.interactable = true;
        source.resetButton.onClick.AddListener(() =>
        {
            source.SetValue(_supportedType.ToString(_defaultValue));
            source.Refresh();
        });
        return source;
    }
}

}