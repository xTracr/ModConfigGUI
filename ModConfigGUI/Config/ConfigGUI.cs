using BepInEx.Configuration;
using ModConfigGUI.UI;
using UnityEngine;

namespace ModConfigGUI.Config
{

public class ConfigGUI
{
    public readonly ConfigEntry<int> guiWidth;
    public readonly ConfigEntry<int> guiHeight;
    public readonly ConfigEntry<float> widthRatio;
    public readonly ConfigEntry<EntryPointStyle> entryPointStyle;
    public readonly ConfigEntry<string> testString;
    public readonly ConfigEntry<int> testInt;
    public readonly ConfigEntry<bool> testBool;
    public readonly ConfigEntry<float> testFloat;
    public readonly ConfigEntry<KeyCode> testKeyCode;
    public readonly ConfigEntry<int> testIntWithList;
    public readonly ConfigEntry<int> testIntWithRange;
    public readonly ConfigEntry<float> testFloatWithRange;
    public readonly ConfigEntry<decimal> testDecimalWithRange;
    public ConfigFile Config { get; }

    public ConfigGUI(ConfigFile config)
    {
        Config = config;
        const string guiProperties = "GUIProperties";
        guiWidth = config.Bind(guiProperties, "Width", 480);
        guiHeight = config.Bind(guiProperties, "Height", 640);
        var widthRatioList = new AcceptableValueList<float>(0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f);
        widthRatio = config.Bind(guiProperties, "WidthRatio", 0.4f, new ConfigDescription("The width ratio of the entrys to the window.", widthRatioList));
        entryPointStyle = config.Bind(guiProperties, "EntryPointStyle", EntryPointStyle.MapTool);
        testString = config.Bind("TestSection1", "TestString", "A String");
        testInt = config.Bind("TestSection1", "TestInt", 123, "This is a test integer\nwith a newline. ");
        testBool = config.Bind("TestSection2", "TestBool", true);
        testFloat = config.Bind("TestSection2", "TestFloat", 1.23f);
        testKeyCode = config.Bind("TestSection2", "TestKeyCode", KeyCode.A);
        testIntWithList = config.Bind("TestSection3", "TestIntWithList", 2,
            new ConfigDescription("This is a test integer with a list.", new AcceptableValueList<int>(-1, 1, 2, 3, 4, 5, 8)));
        testIntWithRange = config.Bind("TestSection3", "TestIntWithRange", 5, new ConfigDescription("", new AcceptableValueRange<int>(1, 10)));
        testDecimalWithRange = config.Bind("TestSection3", "TestDecimalWithRange", 3.234567000000008974583e14m,
            new ConfigDescription("This is a test decimal with a range.", new AcceptableValueRange<decimal>(-6.786612391286783217242e14m, 1e15m)));
        testFloatWithRange = config.Bind("TestSection3", "TestFloatWithRange", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-10.0f, 10.0f)));
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