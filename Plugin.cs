namespace Mod;

using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin instance;
    public static ConfigFile config;

    FileSystemWatcher watcher;

    public static ConfigEntry<Patched.flags> targetType;

    public void Awake()
    {
        instance = this;
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        config = Config;

        targetType = config.Bind("General", "Target type", Patched.flags.coinAndCore);

        new Harmony(PluginInfo.GUID).PatchAll();
    }

    public void Start()
    {
        watcher = new FileSystemWatcher(Path.GetDirectoryName(Config.ConfigFilePath), Path.GetFileName(Config.ConfigFilePath));
        watcher.Changed += (_, __) => Config.Reload();
        watcher.EnableRaisingEvents = true;
    }

    public void Update() => Patched.flag = targetType.Value;

    public static T Ass<T>(string path) => Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
    public static void LogInfo(object msg) => instance.Logger.LogInfo(msg);
    public static void LogWarning(object msg) => instance.Logger.LogWarning(msg);
    public static void LogError(object msg) => instance.Logger.LogError(msg);
}

[HarmonyPatch]
public static class Patched
{
    public enum flags
    {
        coin,
        core,
        coinAndCore,
    }

    const int flagsCount = 3;

    static bool coinOnlyMode;
    static bool coreEjectOnlyMode;
    static bool v;

    public static flags flag = flags.coinAndCore;

    public static void UpdateBitFlags() => flag = (flags)(((int)flag + 1) % flagsCount);

    [HarmonyPatch(typeof(CameraFrustumTargeter), nameof(CameraFrustumTargeter.Update))]
    public static bool Prefix(CameraFrustumTargeter __instance)
    {
        var target = __instance.CurrentTarget;
        if (!target) return true;

        Coin coin = target.GetComponent<Coin>();
        Grenade core = target.GetComponent<Grenade>();

        if ((flag == flags.coin && !coin) || (flag == flags.core && !core))
        {
            ResetTarget(__instance);
            return false;
        }
        if (flag == flags.coin && coin)
        {
            var rb = target.GetComponent<Rigidbody>();
            if (rb.velocity == Vector3.zero)
                ResetTarget(__instance);
            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(CameraFrustumTargeter), nameof(CameraFrustumTargeter.Update))]
    public static void Postfix(CameraFrustumTargeter __instance)
    {
        var target = __instance.CurrentTarget;

        float distToXHair = float.PositiveInfinity;
        Collider endCollider = null;

        if (target != null)
        {
            Coin coin = target.GetComponent<Coin>();
            Grenade core = target.GetComponent<Grenade>();

            if (flag == flags.coin && !coin) ResetTarget(__instance);
            if (flag == flags.core && !core) ResetTarget(__instance);
            if (target != null)
            {
                if (coin || core)
                {
                    Rigidbody rb = target.GetComponent<Rigidbody>();
                    bool isStill = rb.velocity == Vector3.zero;
                    if (isStill) ResetTarget(__instance);
                }
                else
                {
                    Vector3 vec2d = __instance.camera.WorldToViewportPoint(target.bounds.center);
                    if (!(vec2d.x <= 0.5f + __instance.maxHorAim / 2f && vec2d.x >= 0.5f - __instance.maxHorAim / 2f && vec2d.y <= 0.5f + __instance.maxHorAim / 2f && vec2d.y >= 0.5f - __instance.maxHorAim / 2f && vec2d.z >= 0f))
                        v = false;
                    else
                        v = true;
                    if (!v) ResetTarget(__instance);
                }
            }
        }

        int numTargets = Physics.OverlapBoxNonAlloc(__instance.bounds.center, __instance.bounds.extents, __instance.targets, __instance.transform.rotation, __instance.mask.value);

        List<Collider> coinsAndGrenadeColliders = new List<Collider>();
        for (int i = 0; i < numTargets; i++)
        {
            Coin coin = __instance.targets[i].GetComponent<Coin>();
            Grenade core = __instance.targets[i].GetComponent<Grenade>();

            if ((flag == flags.coinAndCore && (coin || core)) || (flag == flags.core && core) || (flag == flags.coin && coin))
                coinsAndGrenadeColliders.Add(__instance.targets[i]);
        }

        if (coinsAndGrenadeColliders.Count == 0) return;

        foreach (Collider targ in coinsAndGrenadeColliders)
        {
            Vector3 vec2d = __instance.camera.WorldToViewportPoint(targ.bounds.center);
            float cdXH = Vector3.Distance(vec2d, new Vector2(0.5f, 0.5f));
            if (vec2d.x <= 0.5f + __instance.maxHorAim / 2f && vec2d.x >= 0.5f - __instance.maxHorAim / 2f && vec2d.y <= 0.5f + __instance.maxHorAim / 2f && vec2d.y >= 0.5f - __instance.maxHorAim / 2f && vec2d.z >= 0f && cdXH < distToXHair)
            {
                distToXHair = cdXH;
                endCollider = targ;
            }
        }

        if (endCollider != null)
        {
            __instance.CurrentTarget = endCollider;
            __instance.IsAutoAimed = true;
        }
    }

    public static void ResetTarget(CameraFrustumTargeter __instance)
    {
        __instance.CurrentTarget = null;
        __instance.IsAutoAimed = false;
    }
}

public class PluginInfo
{
    public const string GUID = "duviz.AutoAimTweaks";
    public const string Name = "AutoAimTweaks";
    public const string Version = "0.1.0";
}