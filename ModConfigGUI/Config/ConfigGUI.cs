using BepInEx.Configuration;
using ModConfigGUI.UI;
using UnityEngine;

namespace ModConfigGUI.Config
{

public class ConfigGUI
{
    static bool DebugEnv => false;
    public readonly ConfigEntry<int> guiWidth;
    public readonly ConfigEntry<int> guiHeight;
    public readonly ConfigEntry<float> widthRatio;
    public readonly ConfigEntry<EntryPointStyle> entryPointStyle;
    ConfigFile Config { get; }

    public ConfigGUI(ConfigFile config)
    {
        Config = config;
        const string guiProperties = "GUIProperties";
        guiWidth = config.Bind(guiProperties, "Width", 480);
        guiHeight = config.Bind(guiProperties, "Height", 640);
        var widthRatioList = new AcceptableValueList<float>(0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f);
        widthRatio = config.Bind(guiProperties, "WidthRatio", 0.4f, new ConfigDescription("The width ratio of the entrys to the window.", widthRatioList));
        entryPointStyle = config.Bind(guiProperties, "EntryPointStyle", EntryPointStyle.MapTool);
        if (!DebugEnv) return;
        config.Bind("Test1", "String", "A String");
        config.Bind("Test1", "Int", 123, "This is a integer\nwith a newline.");
        config.Bind("Test2", "Bool", true);
        config.Bind("Test2", "Float", 1.23f);
        config.Bind("Test2", "KeyCode", KeyCode.A);
        config.Bind("Test3", "Int_List", 2, new ConfigDescription("A integer with a list.", new AcceptableValueList<int>(-1, 1, 2, 3, 4, 5, 8)));
        config.Bind("Test3", "Int_Range", 5, new ConfigDescription("A integer with a range.", new AcceptableValueRange<int>(1, 10)));
        config.Bind("Test3", "Decimal_Range", 3.234000583e14m, new ConfigDescription("", new AcceptableValueRange<decimal>(-6.786142e14m, 1e15m)));
        config.Bind("Test3", "Float_Range", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-10.0f, 10.0f)));
    }

    public ILayerBuilder CreateLayerBuilder(string guid, string name)
    {
        ILayerBuilder builder = LayerBuilder.CreateDefault(name, LangConfig.GetInstance(guid), Config);
        builder.GetOrCreateCategory(widthRatio.Definition.Section).TryGetEntry(widthRatio.Definition.Key)?.SetEntryType(EntryType.Slider);
        return builder;
    }

    public enum EntryPointStyle
    {
        NoIcon,
        Gear,
        MapTool,
        SelectBox,
        System,
        ToggleLog
    }
}

}