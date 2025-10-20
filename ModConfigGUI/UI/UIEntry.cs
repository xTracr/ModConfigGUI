using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModConfigGUI.UI
{

public class UIEntry : EMono
{
    public readonly UnityEvent<string> onValueSet = new UnityEvent<string>();
    public UIButton textButton;
    public UIButton resetButton;
    public Func<string> valueGetter;
    Component _component;
    EntryType _entryType = EntryType.InputField;
    public string[]? Options { get; private set; }
    public UIText Text => textButton.mainText;

    public void SetEntryType(EntryType entryType, string[]? options = null)
    {
        _entryType = entryType;
        Options = options;
    }

    public void Init(int width) { _component = _entryType.InitEntry(this, width); }

    public void Refresh() { _entryType.RefreshEntry(this, _component, valueGetter()); }

    public void SetValue(string value) => onValueSet.Invoke(value);

    public UIEntry OnCreate(bool active = true)
    {
        gameObject.SetActive(active);
        this.Rect().SetSize(new Vector2(100, 34));
        gameObject.AddComponent<HorizontalLayoutGroup>().childForceExpandWidth = false;
        textButton = UIHelper.GetResource<UIButton>("button activate");
        textButton.transform.SetParent(transform);
        textButton.transform.Find("Text (1)")?.gameObject.SetActive(false);
        textButton.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1.0f;
        textButton.Rect().SetPivot(0.5f, 0.5f);
        textButton.interactable = true;
        textButton.mainText.transform.position = new Vector3(50, 0, 0);
        resetButton = UIHelper.GetResource<UIButton>("ButtonToggle");
        resetButton.transform.SetParent(transform);
        resetButton.gameObject.AddComponent<LayoutElement>().preferredWidth = 36;
        resetButton.interactable = false;
        Transform resetImage = resetButton.transform.Find("Image");
        resetImage.DestroyChildren();
        resetImage.GetComponent<Image>().sprite = Sprite.Create(UIHelper.ICONS_48, new Rect(199, 150, 34, 34), new Vector2(0.5f, 0.5f));
        return this;
    }
}

}