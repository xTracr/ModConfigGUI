using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ModConfigGUI.UI
{

public static class UIHelper
{
    public static readonly Texture2D ICONS_48;
    static readonly Transform ClonedObjects;
    static readonly Dictionary<string, Type> DeferredResources;
    static bool _deferredLoaded;

    static UIHelper()
    {
        ClonedObjects = new GameObject("Objects Cloned by ModConfigGUI").transform;
        ClonedObjects.SetParent(PoolManager._trans);
        ClonedObjects.SetActive(false);
        DeferredResources = new Dictionary<string, Type>
        {
            ["button activate"] = typeof(UIButton),
            ["ButtonToggle"] = typeof(UIButton)
        };
        CloneResources(new Dictionary<string, Type>
        {
            ["Layers(Float)"] = typeof(Layer)
        });
        Transform helpTopic = Util.Instantiate<UIItem>("UI/Element/Header/HeaderHelpTopic").transform;
        ICONS_48 = helpTopic.Find("Image").GetComponent<Image>().sprite.texture;
        Object.Destroy(helpTopic.gameObject);
    }

    public static void Init() { }

    public static T Create<T>(Transform? parent = null) where T : MonoBehaviour
    {
        var gameObject = new GameObject(typeof(T).Name, typeof(RectTransform));
        if (parent != null) gameObject.transform.SetParent(parent);
        return gameObject.AddComponent<T>();
    }

    public static T GetResource<T>(string? name = null) where T : Component
    {
        if (!_deferredLoaded)
        {
            CloneResources(DeferredResources);
            _deferredLoaded = true;
        }
        return Object.Instantiate(ClonedObjects.Find<T>(name));
    }

    public static void SetSize(this RectTransform self, Vector2 size)
    {
        Vector2 pivot = self.pivot;
        Vector2 diff = size - self.rect.size;
        self.offsetMin -= new Vector2(diff.x * pivot.x, diff.y * pivot.y);
        self.offsetMax += new Vector2(diff.x * (1f - pivot.x), diff.y * (1f - pivot.y));
    }

    public static TDerived ReplaceComponents<TBase, TDerived>(this TBase original)
        where TBase : MonoBehaviour
        where TDerived : TBase
    {
        GameObject gameObject = original.gameObject;
        var derived = gameObject.AddComponent<TDerived>();
        foreach (FieldInfo field in typeof(TBase).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            field.SetValue(derived, field.GetValue(original));
        Object.Destroy(original);
        gameObject.name = typeof(TDerived).Name;
        gameObject.transform.DestroyChildren();
        return derived;
    }

    public static T AddLayer<T>(ILayerBuilder builder) where T : LayerModConfig
    {
        var layer = EMono.ui.layers.Find(l => l.GetType() == typeof(T)) as T;
        if (layer != null) layer.SetActive(true);
        else EMono.ui.AddLayer(layer = builder.Build(GetResource<Layer>().ReplaceComponents<Layer, T>()));
        return layer;
    }

    public static Window AddWindow(this Layer layer, Window.Setting setting)
    {
        setting.tabs ??= new List<Window.Setting.Tab>();
        var sourceLayer = Layer.Create<LayerList>();
        Window window = sourceLayer.windows.First();
        window.transform.SetParent(layer.transform);
        layer.windows.Add(window);
        Object.Destroy(sourceLayer.gameObject);
        window.setting = setting;
        window.Init(layer);
        window.RectTransform.position = setting.bound.position;
        window.RectTransform.sizeDelta = setting.bound.size;
        window.GetType().GetMethod("RecalculatePositionCaches", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(window, null);
        window.RebuildLayout(true);
        return window;
    }

    static void CloneResources(Dictionary<string, Type> resources)
    {
        foreach (KeyValuePair<string, Type> pair in resources)
        {
            Object? original = Resources.FindObjectsOfTypeAll(pair.Value).FirstOrDefault(o => o.name == pair.Key);
            if (original is null) continue;
            Object.Instantiate(original, ClonedObjects).name = original.name;
        }
    }
}

}