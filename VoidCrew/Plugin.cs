using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CG;
using CG.Client.Ship.Interactions;
using CG.Client.UserData;
using CG.Cloud;
using CG.Game;
using CG.Game.Player;
using CG.Profile;
using CG.Ship.Hull;
using Gameplay.Cryptic;
using Gameplay.Enhancements;
using Gameplay.Hub;
using Gameplay.Perks;
using Gameplay.Utilities;
using HarmonyLib;
using Interactions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace VoidCrew;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static Harmony _harmony;
    private static GameObject _gameObject;

    private void Awake()
    {
        Logger = base.Logger;

        FindObjectOfType(typeof(Cheat))?.SafeDestroy();
    }

    private void OnDestroy()
    {
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());

        _gameObject = new GameObject("CheatThing", typeof(Cheat));
        DontDestroyOnLoad(_gameObject);

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
}

public class Cheat : MonoBehaviour
{
    ManualLogSource Logger => Plugin.Logger;


    public void Awake()
    {
        Logger.LogInfo("Cheat.Awake called");
        InputSystem.onEvent += OnInputEvent;
    }

    public void OnDestroy()
    {
        Logger.LogInfo("Cheat.OnDestroy called");
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (eventPtr.IsA<StateEvent>() && device is Keyboard keyboard)
        {
            if (keyboard.f1Key.isPressed)
            {
                Logger.LogInfo("F1 pressed, unlocking all cosmetics (achievements will not be unlocked and will be removed on restart)");
                RewardHelpers.UnlockAll();
            }
        }
    }
}


[HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.UsedPerkPoints), MethodType.Getter)]
public class PlayerProfilePatch
{
    [HarmonyPostfix]
    public static void Postfix(ref int __result)
    {
        __result = 0;
    }
}

[HarmonyPatch(typeof(Player), nameof(Player.RegisterJetpackStats))]
public class PlayerPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Player __instance)
    {
        __instance.JetpackDashCooldown = 0;
    }
}

[HarmonyPatch(typeof(CrypticKeyCombo), nameof(CrypticKeyCombo.StartCrypticGameplay))]
public class CrypticKeyComboPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref CrypticKeyCombo __instance)
    {
        __instance.Success();
    }
}
[HarmonyPatch(typeof(Enhancement), nameof(Enhancement.Update))]
public class EnhancementPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Enhancement __instance)
    {
        if (__instance.State is EnhancementState.Cooldown or EnhancementState.Failed)
        {
            __instance.SetState(EnhancementState.Inactive, __instance.AppliedEnhancementEffect.DefaultGrade(), 1f);
        }
    }
}

[HarmonyPatch(typeof(HubMutatorManager), nameof(HubMutatorManager.GetMaxMutatorsCount))]
public class HubMutatorManagerPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref int __result)
    {
        __result = 100;
    }
}

[HarmonyPatch(typeof(LongPressSwitchClicker), nameof(LongPressSwitchClicker.HandleClickStart))]
public class LongPressSwitchClickerPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref LongPressSwitchClicker __instance)
    {
        __instance.stateIndicator.ForceChange(!__instance.stateIndicator.Value);
    }
}
[HarmonyPatch(typeof(Lever), nameof(Lever.StartClick))]
public class LeverPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Lever __instance)
    {
        if (__instance.LeverPosition < __instance.triggerThreshold)
        {
            __instance.LeverPosition = __instance.triggerThreshold + 0.1f;
        }
        else if (__instance.LeverPosition > __instance.triggerThreshold)
        {
            __instance.LeverPosition = __instance.triggerThreshold - 0.1f;
        }
        __instance.EndClick();
    }
}
