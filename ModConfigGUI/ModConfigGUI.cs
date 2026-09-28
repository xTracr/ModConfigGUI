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
    public const string ModId = "xtracr_modconfiggui";
    public const string Name = "Mod Config GUI";
    public const string Version = "0.1.19";
    static readonly Dictionary<BaseModPackage, BaseUnityPlugin> Plugins = new Dictionary<BaseModPackage, BaseUnityPlugin>();
    public static ConfigGUI ConfigGUI { get; private set; }

    public ModConfigGUI() => ConfigGUI = new ConfigGUI(Config);

    public static IReadOnlyDictionary<BaseModPackage, BaseUnityPlugin> GetPlugins() => Plugins;

    public static string GetLangId(string name) => ModId + "." + name;

    void Awake() { new Harmony(GUID).PatchAll(); }

    void Start()
    {
        LayerBuilder.RegisterBuilder(GUID, () => ConfigGUI.CreateLayerBuilder(GUID, Name));

        BaseUnityPlugin[] plugins = ModManager.ListPluginObject.OfType<BaseUnityPlugin>().ToArray();
        foreach (BaseModPackage package in ModManager.Instance.packages)
        {
            BaseUnityPlugin? plugin = plugins.FirstOrDefault(p => p.Info.Location.Contains(package.dirInfo.FullName));
            if (plugin is null) continue;
            Plugins[package] = plugin;
        }
        LangConfig.ReLoad();
    }
}

}