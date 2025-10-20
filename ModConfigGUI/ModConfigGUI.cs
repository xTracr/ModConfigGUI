using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModConfigGUI.Config;
using ModConfigGUI.UI;

namespace ModConfigGUI
{

[BepInPlugin(GUID, Name, Version)]
public class ModConfigGUI : BaseUnityPlugin
{
    public const string GUID = "me.xtracr.modconfiggui";
    public const string Name = "Mod Config GUI";
    public const string Version = "0.1.16";
    static readonly Dictionary<BaseModPackage, BaseUnityPlugin> Plugins = new Dictionary<BaseModPackage, BaseUnityPlugin>();
    public static string ModDir { get; private set; } = "";
    public static ConfigGUI ConfigGUI { get; private set; }

    public static IReadOnlyDictionary<BaseModPackage, BaseUnityPlugin> GetPlugins() => Plugins;

    void Awake() { new Harmony(GUID).PatchAll(); }

    void Start()
    {
        ModDir = Path.GetDirectoryName(Info.Location) ?? "";
        ConfigGUI = new ConfigGUI(new ConfigFile(Path.Combine(Paths.ConfigPath, "modconfiggui.cfg"), true, Info.Metadata));
        LayerBuilder.RegisterBuilder(GUID, () => ConfigGUI.CreateLayerBuilder(GUID, Name));

        BaseUnityPlugin[] plugins = ModManager.ListPluginObject.OfType<BaseUnityPlugin>().ToArray();
        foreach (BaseModPackage package in EClass.core.mods.packages)
        {
            BaseUnityPlugin? plugin = plugins.FirstOrDefault(p => p.Info.Location.Contains(package.dirInfo.FullName));
            if (plugin is null) continue;
            Plugins[package] = plugin;
        }
        LangConfig.ReLoad();
    }
}

}