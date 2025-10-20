using HarmonyLib;
using ModConfigGUI.UI;

namespace ModConfigGUI.Patches
{

[HarmonyPatch(typeof(LayerTitle))]
public class PatchLayerTitle
{
    [HarmonyPatch("OnInit")]
    [HarmonyPostfix]
    public static void OnInit_Postfix() => UIHelper.Init();
}

}