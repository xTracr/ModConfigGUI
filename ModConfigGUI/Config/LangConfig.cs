﻿using System.Collections.Generic;
using System.IO;
using BepInEx;

namespace ModConfigGUI.Config
{

public class LangConfig : SourceLang<Row>
{
    static readonly Dictionary<string, LangConfig> LangConfigs = new Dictionary<string, LangConfig>();
    static string _currnetLang = "";
    public static readonly LangConfig General = CreateInstance<LangConfig>();

    public static LangConfig GetInstance(string guid)
    {
        if (LangConfigs.TryGetValue(guid, out LangConfig instance)) return instance;
        return LangConfigs[guid] = CreateInstance<LangConfig>();
    }

    public static void ReLoad()
    {
        if (_currnetLang.Equals(EClass.core.config.lang)) return;
        _currnetLang = EClass.core.config.lang;
        string langFile = GetFullLangPath(ModConfigGUI.ModDir, _currnetLang);
        if (File.Exists(langFile)) ModUtil.ImportExcel(langFile, "General", General);
        foreach (KeyValuePair<BaseModPackage, BaseUnityPlugin> pair in ModConfigGUI.Packages)
        {
            langFile = GetFullLangPath(pair.Key.dirInfo.FullName, _currnetLang);
            if (File.Exists(langFile)) ModUtil.ImportExcel(langFile, "Config", GetInstance(pair.Value.Info.Metadata.GUID));
        }
    }

    public static string GetFullLangPath(string modDir, string lang)
    {
        string langPath = Path.Combine(modDir, "LangConfig", lang + ".xlsx");
        return File.Exists(langPath) ? langPath : Path.Combine(modDir, "LangConfig", "EN.xlsx");
    }

    public string GetText(string id) => map.TryGetValue(id, out Row value) ? value.text : id;

    public string GetDesc(string id) => map.TryGetValue(id, out Row value) ? value.desc : "";

    public override Row CreateRow() => new Row { id = GetString(0), text = GetString(1), desc = GetString(2) };

    public override void SetRow(Row lRow) => map[lRow.id] = lRow;
}

public class Row : LangRow
{
    public string desc = "";
}

}