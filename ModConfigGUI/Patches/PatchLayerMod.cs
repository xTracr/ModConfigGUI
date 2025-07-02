using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ModConfigGUI.Config;
using ModConfigGUI.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ModConfigGUI.Patches
{

[HarmonyPatch(typeof(LayerMod))]
public class PatchLayerMod
{
    static readonly Dictionary<BaseModPackage, Exception> Exceptions = new Dictionary<BaseModPackage, Exception>();
    static bool _hideExceptionDialog;

    [HarmonyTranspiler]
    [HarmonyPatch("OnInit")]
    public static IEnumerable<CodeInstruction> OnInit_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        bool inserted = false;
        MethodInfo? combineOnInstantiate = typeof(PatchLayerMod).GetMethod(nameof(CombineOnInstantiate), BindingFlags.Static | BindingFlags.Public);
        foreach (CodeInstruction code in instructions)
        {
            if (!inserted && code.opcode == OpCodes.Stfld && code.operand is FieldInfo { Name: "onInstantiate" })
            {
                inserted = true;
                yield return new CodeInstruction(OpCodes.Call, combineOnInstantiate);
            }
            yield return code;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("OnInit")]
    public static void OnInit_Prefix() => LangConfig.ReLoad();

    [HarmonyPostfix]
    [HarmonyPatch("OnInit")]
    public static void OnInit_Postfix()
    {
        if (_hideExceptionDialog || Exceptions.Count <= 0) return;
        Dialog.YesNo(Exceptions.Aggregate(LangConfig.General.GetText("genEntryPointFailed"),
                (current, pair) => current + "\n[" + pair.Key.title + "] " + pair.Value.GetType().Name + ": " + pair.Value.Message),
            () => _hideExceptionDialog = true, langYes: LangConfig.General.GetText("hideHint"), langNo: "ok");
        Exceptions.Clear();
    }

    public static Action<BaseModPackage, ItemMod> CombineOnInstantiate(Action<BaseModPackage, ItemMod> original) => original + OnInit_OnInstantiate_Postfix;

    static void OnInit_OnInstantiate_Postfix(BaseModPackage package, ItemMod item)
    {
        try
        {
            if (!ModConfigGUI.Plugins.TryGetValue(package, out BaseUnityPlugin plugin)) return;
            string guid = plugin.Info.Metadata.GUID;
            ILayerBuilder builder;
            if (LayerBuilder.Builders.TryGetValue(guid, out Func<ILayerBuilder> value)) builder = value();
            else
            {
                ConfigFile configFile = plugin.Config;
                if (configFile.Keys.Count == 0) return;
                builder = LayerBuilder.CreateDefault(package.title, LangConfig.GetInstance(guid), configFile);
            }
            item.buttonActivate.onClick.AddListener(() =>
            {
                UIContextMenu contextMenu = ELayer.ui.contextMenu.currentMenu;
                contextMenu.AddButton("config", () => UIHelper.AddLayer<LayerModConfig>(builder));
                contextMenu.Show();
            });
            var spriteRect = new Rect(250, 58, 28, 28);
            switch (ModConfigGUI.ConfigGUI.entryPointStyle.Value)
            {
                case ConfigGUI.EntryPointStyle.NoIcon: return;
                case ConfigGUI.EntryPointStyle.Gear:
                    spriteRect = new Rect(151, 56, 34, 34);
                    break;
                case ConfigGUI.EntryPointStyle.SelectBox:
                    spriteRect = new Rect(582, 440, 33, 32);
                    break;
                case ConfigGUI.EntryPointStyle.System:
                    spriteRect = new Rect(151, 150, 34, 34);
                    break;
                case ConfigGUI.EntryPointStyle.ToggleLog:
                    spriteRect = new Rect(151, 103, 34, 34);
                    break;
            }
            var configButton = UIHelper.GetResource<UIButton>("ButtonToggle");
            configButton.name = "ButtonConfig";
            Transform text1 = item.buttonActivate.Find("Text (1)");
            configButton.transform.SetParent(text1);
            configButton.transform.localPosition = new Vector3(110, 0, 0);
            configButton.transform.localScale = new Vector3(0.0325f * ELayer.config.ui.scale, 0.0325f * ELayer.config.ui.scale, 1);
            Transform image = configButton.transform.Find("Image");
            image.DestroyChildren();
            image.GetComponent<Image>().sprite = Sprite.Create(UIHelper.ICONS_48, spriteRect, new Vector2(0.5f, 0.5f));
            configButton.tooltip = new TooltipData { enable = true, offset = new Vector3(0, -10), id = "", lang = "config" };
            configButton.onClick.AddListener(() => UIHelper.AddLayer<LayerModConfig>(builder));
        } catch (Exception exception)
        {
            Exceptions[package] = exception;
        }
    }
}

}