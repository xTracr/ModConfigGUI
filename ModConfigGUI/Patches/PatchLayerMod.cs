using HarmonyLib;
using ModConfigGUI.Config;

namespace ModConfigGUI.Patches
{

[HarmonyPatch(typeof(LayerMod))]
public class PatchLayerMod
{
    [HarmonyPrefix]
    [HarmonyPatch("OnInit")]
    public static void OnInit_Prefix() => LangConfig.ReLoad();
}

}