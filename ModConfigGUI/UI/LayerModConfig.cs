using System;
using System.Linq;
using ModConfigGUI.Config;
using UnityEngine.Events;

namespace ModConfigGUI.UI
{

public class LayerModConfig : ELayer
{
    readonly UnityAction<string> _strToRefresh;
    public string modName = "";
    public UnityEvent onSave = new UnityEvent();
    public UnityEvent onInit = new UnityEvent();
    public UIButton? saveButton;
    public EntryCategory[] categories = Array.Empty<EntryCategory>();
    public bool Saved => categories.All(category => category.Entries.Values.All(entry => Equals(entry.Value, entry.LoadValue())));

    public LayerModConfig() => _strToRefresh = str => Refresh();

    public void Save()
    {
        foreach (EntryCategory category in categories)
        foreach (UIEntryBuilder entry in category.Entries.Values)
            entry.Save();
        onSave.Invoke();
        Refresh();
    }

    public void Refresh()
    {
        if (saveButton != null) saveButton.mainText.text = Lang.Get("save") + (Saved ? "" : "*");
    }

    public override void OnInit()
    {
        onInit.Invoke();
        foreach (EntryCategory category in categories)
        {
            foreach (UIEntryBuilder entry in category.Entries.Values) entry.Load();
            foreach (UIEntry entry in category.BuiltEntries)
            {
                entry.Refresh();
                entry.onValueSet.RemoveListener(_strToRefresh);
                entry.onValueSet.AddListener(_strToRefresh);
            }
        }
        Refresh();
        base.OnInit();
    }

    public override void OnKill()
    {
        if (!Saved) Dialog.YesNo(LangConfig.General.GetText("notSaved"), Save);
        base.OnKill();
    }
}

}