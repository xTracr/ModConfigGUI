using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModConfigGUI.UI
{

public class EntryCategory
{
    protected readonly List<UIEntry> _builtEntries = new List<UIEntry>();
    protected readonly Dictionary<string, UIEntryBuilder> _entries = new Dictionary<string, UIEntryBuilder>();
    public string Name { get; }
    public IReadOnlyList<UIEntry> BuiltEntries => _builtEntries;
    public IReadOnlyDictionary<string, UIEntryBuilder> Entries => _entries;

    public EntryCategory(string name) => Name = name;

    public UIEntryBuilder? TryGetEntry(string id) => _entries.TryGetValue(id, out UIEntryBuilder entry) ? entry : null;

    public UIEntryBuilder GetOrCreateEntry(string id, Type type, object value, string name = "")
    {
        if (_entries.TryGetValue(id, out UIEntryBuilder entry)) return entry;
        return _entries[id] = UIEntryBuilder.Create(string.IsNullOrEmpty(name) ? id : name, type, value);
    }

    public void AddDescription(string description, Color? color = null)
    {
        foreach (string line in description.Split('\n'))
            if (line.Length > 0)
                GetOrCreateEntry(line, typeof(string), line).SetEntryType(EntryType.Description).SetNameColor(color);
    }

    public UIList Build()
    {
        _builtEntries.Clear();
        var uiList = UIHelper.Create<UIList>();
        var layout = uiList.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        uiList.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        uiList.bgType = UIList.BGType.stripe;
        uiList.callbacks = new UIList.Callback<UIEntryBuilder, UIEntry>
        {
            mold = UIHelper.Create<UIEntry>(uiList.transform).OnCreate(false),
            onInstantiate = (entry, uiEntry) => _builtEntries.Add(entry.Build(uiEntry)),
            onList = sort => uiList.AddCollection(_entries.Values.ToList())
        };
        uiList.List();
        return uiList;
    }
}

}