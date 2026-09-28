using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModConfigGUI.UI;

namespace ModConfigGUI.Patches
{

[HarmonyPatch(typeof(LayerModConfig))]
public class PatchLayerModConfig
{
    [HarmonyPrefix]
    [HarmonyPatch("Open")]
    public static bool Open_Prefix(ModPackage p)
    {
        if (!ModConfigGUI.ConfigGUI.legacyGUI.Value) return true;
        try
        {
            if (!ModConfigGUI.GetPlugins().TryGetValue(p, out BaseUnityPlugin plugin)) return true;
            string guid = plugin.Info.Metadata.GUID;
            ILayerBuilder builder;
            if (LayerBuilder.GetBuilders().TryGetValue(guid, out Func<ILayerBuilder> value)) builder = value();
            else
            {
                ConfigFile configFile = plugin.Config;
                if (configFile.Keys.Count == 0) return true;
                builder = LayerBuilder.CreateDefault(guid, p.title, configFile);
            }
            UIHelper.AddLayer<UI.LayerModConfig>(builder);
            return false;
        } catch (Exception)
        {
            return true;
        }
    }

}

}