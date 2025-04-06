using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ModConfigGUI.UI
{

public class EntryType<T> : EntryType where T : Component
{
    readonly Func<UIEntry, int, T> _initEntry;
    readonly Action<UIEntry, T, string> _refreshEntry;

    public EntryType(Func<UIEntry, int, T> initEntry, Action<UIEntry, T, string> refreshEntry)
    {
        _initEntry = initEntry;
        _refreshEntry = refreshEntry;
    }

    public EntryType<T> AfterRefresh(Action<UIEntry, T, string> afterRefresh) => new EntryType<T>(_initEntry, _refreshEntry + afterRefresh);

    public override Component InitEntry(UIEntry entry, int width) => _initEntry(entry, width);

    public override void RefreshEntry(UIEntry entry, Component component, string currentValue)
    {
        if (component is T typedComponent) _refreshEntry(entry, typedComponent, currentValue);
    }
}

public abstract class EntryType
{
    public static readonly EntryType<UIButton> Cycling;
    public static readonly EntryType<UIText> Description;
    public static readonly EntryType<Dropdown> Dropdown;
    public static readonly EntryType<UIInputText> InputField;
    public static readonly EntryType<UIButton> Keymap;
    public static readonly EntryType<UISlider> Slider;

    static EntryType()
    {
        Cycling = new EntryType<UIButton>((entry, width) =>
        {
            var cyclingButton = Util.Instantiate<UIButton>("UI/Element/Button/ButtonMain", entry);
            cyclingButton.gameObject.AddComponent<LayoutElement>().preferredWidth = width;
            return cyclingButton;
        }, (entry, cyclingButton, currentValue) =>
        {
            if (entry.Options is null) return;
            int index = Math.Max(0, Array.IndexOf(entry.Options, currentValue));
            cyclingButton.mainText.text = entry.Options[index];
            cyclingButton.SetOnClick(() =>
                entry.SetValue(cyclingButton.mainText.text = entry.Options[index = (index + 1) % entry.Options.Length]));
        });
        Description = new EntryType<UIText>((entry, width) => entry.Text, (entry, uiText, currentValue) =>
        {
            uiText.text = currentValue;
            uiText.fontSize = 15;
            entry.textButton.interactable = false;
            entry.resetButton.SetActive(false);
        });
        Dropdown = new EntryType<Dropdown>((entry, width) =>
        {
            var dropdown = Util.Instantiate<Dropdown>("UI/Element/Input/DropdownDefault", entry);
            dropdown.gameObject.AddComponent<LayoutElement>().preferredWidth = width;
            if (entry.Options is null) return dropdown;
            dropdown.options = entry.Options.Select(option => new Dropdown.OptionData(option)).ToList();
            dropdown.onValueChanged.AddListener(value => entry.SetValue(entry.Options[value]));
            return dropdown;
        }, (entry, dropdown, currentValue) =>
        {
            if (entry.Options is null) return;
            dropdown.SetValueWithoutNotify(Math.Max(0, Array.IndexOf(entry.Options, currentValue)));
        });
        InputField = new EntryType<UIInputText>((entry, width) =>
        {
            Transform inputText = Util.Instantiate("UI/Element/Input/InputText", entry);
            inputText.gameObject.AddComponent<VerticalLayoutGroup>().childForceExpandWidth = false;
            Object.Destroy(inputText.Find("text invalid (1)").gameObject);
            var layout = inputText.GetComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
            var inputField = inputText.Find("InputField").GetComponent<UIInputText>();
            inputField.gameObject.AddComponent<LayoutElement>().preferredWidth = width - 4;
            inputField.Find("Image").SetActive(false);
            var textColor = new Color(0.239f, 0.175f, 0.079f);
            inputField.field.textComponent.color = textColor;
            inputField.field.textComponent.fontSize = 15;
            inputField.field.image.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
            inputField.field.characterLimit = 0;
            inputField.field.characterValidation = UnityEngine.UI.InputField.CharacterValidation.None;
            inputField.field.contentType = UnityEngine.UI.InputField.ContentType.Standard;
            inputField.field.onValueChanged.AddListener(value =>
            {
                try
                {
                    entry.SetValue(value);
                    inputField.field.textComponent.color = textColor;
                } catch (Exception exception)
                {
                    inputField.field.textComponent.color = Color.red;
                    ShowTooltip(exception.Message, inputText);
                }
            });
            return inputField;
        }, (entry, inputField, currentValue) => inputField.Text = currentValue);
        Keymap = new EntryType<UIButton>((entry, width) =>
        {
            var keymapButton = Util.Instantiate<UIButton>("UI/Element/Button/ButtonMain", entry);
            keymapButton.gameObject.AddComponent<LayoutElement>().preferredWidth = width;
            return keymapButton;
        }, (entry, keymapButton, currentValue) =>
        {
            var keymap = new EInput.KeyMap { action = 0, key = currentValue == "None" ? KeyCode.None : (KeyCode)Enum.Parse(typeof(KeyCode), currentValue) };
            keymapButton.mainText.text = keymap.key.ToString();
            keymapButton.SetOnClick(() => Dialog.Keymap(keymap).SetOnKill(() =>
            {
                entry.SetValue(keymap.key.ToString());
                keymapButton.mainText.text = keymap.key.ToString();
            }));
        });
        Slider = new EntryType<UISlider>((entry, width) =>
        {
            Transform transform = Util.Instantiate("UI/Element/Input/Slider");
            transform.SetParent(entry.transform);
            transform.gameObject.AddComponent<ContentSizeFitter>();
            transform.gameObject.AddComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
            var slider = transform.Find("Slider").GetComponent<UISlider>();
            slider.gameObject.AddComponent<LayoutElement>().preferredWidth = width - width / 6f;
            Transform textInfo = slider.transform.Find("UIText");
            textInfo.SetParent(transform);
            textInfo.gameObject.AddComponent<LayoutElement>().preferredWidth = width / 6f;
            slider.textInfo = textInfo.GetComponent<UIText>();
            return slider;
        }, (entry, slider, currentValue) =>
        {
            IList<string>? options = entry.Options;
            if (options is null) return;
            int i = Math.Max(0, options.IndexOf(currentValue));
            slider.minValue = 0.0f;
            slider.maxValue = options.Count - 1;
            slider.SetValueWithoutNotify(i);
            float textWidth = slider.textInfo.GetComponent<LayoutElement>().preferredWidth;
            int maxTextLength = (int)Math.Round(textWidth / 8f);
            slider.SetList(i, options, (index, value) =>
            {
                entry.SetValue(value);
                if (value.Length > maxTextLength) ShowTooltip(value, slider.transform);
                else TooltipManager.Instance.HideTooltips();
            }, info => info.Substring(0, Math.Min(maxTextLength, info.Length)));
        });
    }

    static void ShowTooltip(string text, Transform transform) =>
        TooltipManager.Instance.ShowTooltip(new TooltipData { enable = true, offset = new Vector3(0, -10), id = "", lang = "", text = text }, transform);

    public abstract Component InitEntry(UIEntry entry, int width);

    public abstract void RefreshEntry(UIEntry entry, Component component, string currentValue);
}

}