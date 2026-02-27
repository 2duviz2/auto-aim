namespace Mod;

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Mod.Helpers.Attributes;
using UnityEngine;
using UnityEngine.AddressableAssets;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin instance;
    public static ConfigFile config;

    public void Awake()
    {
        instance = this;
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        config = Config;

        new Harmony(PluginInfo.GUID).PatchAll();
    }

    public void Start()
    {
        foreach (var (type, attr) in AttributeHelper.GetTypesWithAttribute<CreateOnStart>())
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                GameObject obj = new GameObject(type.Name);
                DontDestroyOnLoad(obj);
                obj.AddComponent(type);
            }
        }
    }

    public static T Ass<T>(string path) => Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
    public static void LogInfo(object msg) => instance.Logger.LogInfo(msg);
    public static void LogWarning(object msg) => instance.Logger.LogWarning(msg);
    public static void LogError(object msg) => instance.Logger.LogError(msg);
}

public class PluginInfo
{
    public const string GUID = "AuthorName.ModName";
    public const string Name = "ModName";
    public const string Version = "1.0.0";
}