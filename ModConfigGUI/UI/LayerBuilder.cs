using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using ModConfigGUI.Config;
using UnityEngine;

namespace ModConfigGUI.UI
{

public class LayerBuilder : ILayerBuilder
{
    protected readonly Dictionary<string, EntryCategory> _categories = new Dictionary<string, EntryCategory>();
    protected string _title = "config";
    protected string? _filePath;
    protected bool _allowMove = true;
    protected bool _transparent;
    protected int _width = 480;
    protected int _height = 640;
    protected Action? _onSave;
    protected Action? _onLoad;
    static readonly Dictionary<string, Func<ILayerBuilder>> Builders = new Dictionary<string, Func<ILayerBuilder>>();
    static readonly EntryType ErrorEntryType = EntryType.Description.AfterRefresh((uiEntry, uiText, currentValue) =>
    {
        uiText.fontSize = 16;
        uiEntry.textButton.interactable = true;
    });

    protected LayerBuilder() { }

    public static IReadOnlyDictionary<string, Func<ILayerBuilder>> GetBuilders() => Builders;

    public static void RegisterBuilder(string guid, Func<ILayerBuilder> builder) => Builders[guid] = builder;

    public static void RegisterDefaultBuilder(string guid, string name, ConfigFile configFile) =>
        RegisterBuilder(guid, () => CreateDefault(name, LangConfig.GetInstance(guid), configFile));

    public static ILayerBuilder Create() => new LayerBuilder();

    public static ILayerBuilder CreateDefault(string title, LangConfig langConfig, ConfigFile configFile)
    {
        int width = ModConfigGUI.ConfigGUI.guiWidth.Value;
        ILayerBuilder builder = Create()
            .SetTitle(title)
            .SetFilePath(configFile.ConfigFilePath)
            .SetSize(width, ModConfigGUI.ConfigGUI.guiHeight.Value)
            .SetOnSave(configFile.Save)
            .SetOnLoad(() =>
            {
                if (File.Exists(configFile.ConfigFilePath)) configFile.Reload();
            });
        int entryWidth = (int)(width * ModConfigGUI.ConfigGUI.widthRatio.Value);
        foreach (ConfigDefinition definition in configFile.Keys)
        {
            string categoryId = definition.Section;
            EntryCategory category = builder.GetOrCreateCategory(categoryId, langConfig.GetText(categoryId), langConfig.GetDesc(categoryId));
            ConfigEntryBase? entry = configFile.TryGetValue(definition);
            if (entry is null) continue;
            string entryId = definition.Key;
            try
            {
                category.GetOrCreateEntry(entryId, entry.SettingType, entry.BoxedValue, langConfig.GetText(entryId))
                    .SetTooltip(string.IsNullOrEmpty(langConfig.GetDesc(entryId)) ? entry.Description.Description : langConfig.GetDesc(entryId))
                    .SetWidth(entryWidth)
                    .SetDefaultValue(entry.DefaultValue)
                    .SetAcceptableValue(entry.Description.AcceptableValues)
                    .SetOnSave(o => entry.BoxedValue = o)
                    .SetOnLoad(() => entry.BoxedValue);
            } catch (Exception exception)
            {
                category.GetOrCreateEntry(entryId, typeof(string), langConfig.GetText(entryId) + " (Error!)")
                    .SetNameColor(Color.red)
                    .SetTooltip(new TooltipData
                        { enable = true, offset = new Vector3(0, -10), id = "", lang = "", text = exception.GetType().Name + ": " + exception.Message })
                    .SetEntryType(ErrorEntryType);
            }
        }
        return builder;
    }

    public EntryCategory GetOrCreateCategory(string id, string name = "", string desc = "")
    {
        if (_categories.TryGetValue(id, out EntryCategory category)) return category;
        category = new EntryCategory(string.IsNullOrEmpty(name) ? id : name);
        if (!string.IsNullOrEmpty(desc)) category.AddDescription(desc);
        return _categories[id] = category;
    }

    public ILayerBuilder SetTitle(string title)
    {
        _title = title;
        return this;
    }

    public ILayerBuilder SetFilePath(string filePath)
    {
        _filePath = filePath;
        return this;
    }

    public ILayerBuilder AllowMove(bool allowMove = true)
    {
        _allowMove = allowMove;
        return this;
    }

    public ILayerBuilder Transparent(bool transparent = true)
    {
        _transparent = transparent;
        return this;
    }

    public ILayerBuilder SetSize(int width, int height)
    {
        _width = width;
        _height = height;
        return this;
    }

    public ILayerBuilder SetOnSave(Action onSave)
    {
        _onSave = onSave;
        return this;
    }

    public ILayerBuilder SetOnLoad(Action onLoad)
    {
        _onLoad = onLoad;
        return this;
    }

    public T Build<T>(T baseLayer) where T : LayerModConfig
    {
        baseLayer.modName = _title;
        baseLayer.categories = _categories.Values.ToArray();
        if (_onSave != null) baseLayer.onSave.AddListener(() => _onSave());
        if (_onLoad != null) baseLayer.onInit.AddListener(() => _onLoad());
        Window window = baseLayer.AddWindow(new Window.Setting
        {
            textCaption = _title,
            bound = new Rect(0.0f, 0.0f, _width, _height),
            allowMove = _allowMove,
            transparent = _transparent
        });
        window.rectHeader.GetComponentsInChildren<UIHeader>(true).FirstOrDefault()?.SetText("config");
        Transform content = window.Find("Content View").Find("Inner Simple Scroll").Find("Scrollview default").Find("Viewport").Find("Content");
        content.DestroyChildren();
        foreach (EntryCategory category in _categories.Values)
        {
            var sectionRect = new GameObject("Section Name").AddComponent<RectTransform>();
            sectionRect.SetParent(content);
            sectionRect.SetSize(new Vector2(_width, 51));
            UIText text = UIHelper.Create<UIText>(sectionRect).SetSize(-1);
            RectTransform textRect = text.Rect();
            textRect.anchorMax = Vector2.zero;
            textRect.anchorMin = Vector2.zero;
            textRect.anchoredPosition = new Vector2(_width * 0.5f + 25, 20);
            textRect.sizeDelta = new Vector2(_width, 20);
            text.SetColor(FontColor.Topic);
            text.SetText(category.Name);
            category.Build().transform.SetParent(content);
        }
        if (!string.IsNullOrEmpty(_filePath)) window.AddBottomButton("mod_info", () => Util.ShowExplorer(_filePath));
        if (_onSave != null) baseLayer.saveButton = window.AddBottomButton("", baseLayer.Save);
        return baseLayer;
    }
}

public interface ILayerBuilder
{
    EntryCategory GetOrCreateCategory(string id, string name = "", string desc = "");

    ILayerBuilder SetTitle(string title);

    ILayerBuilder SetFilePath(string filePath);

    ILayerBuilder AllowMove(bool allowMove = true);

    ILayerBuilder Transparent(bool transparent = true);

    ILayerBuilder SetSize(int width, int height);

    ILayerBuilder SetOnSave(Action onSave);

    ILayerBuilder SetOnLoad(Action onLoad);

    T Build<T>(T baseLayer) where T : LayerModConfig;
}

}