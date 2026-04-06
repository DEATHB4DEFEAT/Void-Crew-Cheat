using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using CG.Client.Player.Interactions;
using CG.Client.Ship.Interactions;
using CG.Client.UserData;
using CG.Game.Player;
using CG.Profile;
using CG.Ship.Hull;
using Gameplay.Cryptic;
using Gameplay.Enhancements;
using Gameplay.Hub;
using HarmonyLib;
using Interactions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace VoidCrew;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private new static ManualLogSource _logger;
    private static Harmony _harmony;

    private void Awake()
    {
        _logger = base.Logger;

        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);
        InputSystem.onEvent += OnInputEvent;

        _logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has loaded");
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
        InputSystem.onEvent -= OnInputEvent;
        _logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has unloaded");
    }

    private bool _highlightInteractables = true;
    private bool _highlightInteractablesPrev = true;
    private void Update()
    {
        foreach (var interactable in FindObjectsOfType<AbstractInteractable>())
        {
            // Highlight everything interactable
            if (_highlightInteractables)
                interactable.SetHighlighted(true);
            else if (_highlightInteractablesPrev)
                interactable.SetHighlighted(false);

            // Larger interaction range, not infinite for whatever reason
            var field = typeof(AbstractInteractable).GetField(nameof(AbstractInteractable.interactDistance), BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
                field.SetValue(interactable, float.MaxValue);
        }
        _highlightInteractablesPrev = _highlightInteractables;

        // Don't need to hold to activate switches
        foreach (var switchClicker in FindObjectsOfType<LongPressSwitchClicker>())
            switchClicker._holdTime = 0;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (!eventPtr.IsA<StateEvent>() || device is not Keyboard keyboard)
            return;

        if (keyboard.f1Key.isPressed)
        {
            _logger.LogInfo("F1 pressed, unlocking all cosmetics (achievements/DLC will not be unlocked permanently and will be removed on restart)");
            RewardHelpers.UnlockAll();
        }
        else if (keyboard.f2Key.isPressed)
        {
            _logger.LogInfo($"F2 pressed, toggling interactable highlighting (was {(_highlightInteractables ? "on" : "off")})");
            _highlightInteractables = !_highlightInteractables;
        }
    }
}


/// Lets you pick a lot of mutators
[HarmonyPatch(typeof(HubMutatorManager), nameof(HubMutatorManager.GetMaxMutatorsCount))]
public class HubMutatorManagerPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref int __result)
    {
        __result = 100;
    }
}

/// Infinite perk points
[HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.UsedPerkPoints), MethodType.Getter)]
public class PlayerProfilePatch
{
    [HarmonyPostfix]
    public static void Postfix(ref int __result)
    {
        __result = 0;
    }
}

/// No cooldown for jetpack dash
[HarmonyPatch(typeof(Player), nameof(Player.RegisterJetpackStats))]
public class PlayerPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Player __instance)
    {
        __instance.JetpackDashCooldown = 0;
    }
}

/// Instant cooldown and infinite duration for abilities, also lets you stop them early
[HarmonyPatch(typeof(ClassAbility), nameof(ClassAbility.ActivationKeyPressed))]
public class ClassAbilityPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref ClassAbility __instance)
    {
        if (__instance is not FiniteDurationClassAbility ability)
            return;

        if (ability.IsOngoing())
        {
            ability.StopAbility();
            ability.RefundCooldown(ability.CooldownSeconds.Value);
        }
        else
        {
            ability.CurrentCooldown = 0;
            if (ability.StartAbility())
                ability.CurrentActiveDuration = float.MaxValue;
        }
    }
}
/// Grapple items off shelves cause it's cool as hell
/// <br />
/// Yes this is taken from an existing mod, but this is a cheat mod, we don't need a modded lobby to use it
[HarmonyPatch(typeof(GrapplingHookProjectile), "ImpactValidTarget")]
public class GrapplingHookProjectilePatch
{
    [HarmonyPrefix]
    public static void Prefix(GrapplingHookProjectile __instance, ref Transform impactedTargetTransform, Vector3 impactPoint)
    {
        var minDistance = float.PositiveInfinity;
        CarryablesSocket closestSocket = null;

        foreach (var socket in Object.FindObjectsOfType<CarryablesSocket>().Where(socket => socket.Payload != null && socket.isOutput))
        {
            var distance = (socket.transform.position - impactPoint).magnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                closestSocket = socket;
            }
        }
        if (closestSocket == null) return;

        if (minDistance > 0.9) return;

        var item = closestSocket.Payload;
        closestSocket.ReleaseCarryable();
        __instance.AttachedToCarryable = item;
        impactedTargetTransform = item.transform;
        if (item.SimulationPlatform != null)
            __instance.parentPlatform.TryUnregisterProjectile(__instance);
    }
}


/// Complete arrow combos instantly
[HarmonyPatch(typeof(CrypticKeyCombo), nameof(CrypticKeyCombo.StartCrypticGameplay))]
public class CrypticKeyComboStartCrypticGameplayPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref CrypticKeyCombo __instance)
    {
        __instance.finishRoutine = __instance.StartCoroutine(__instance.DelayedFinish(true));
    }
}
/// <see cref="CrypticKeyComboStartCrypticGameplayPatch"/>
[HarmonyPatch(typeof(CrypticKeyCombo), nameof(CrypticKeyCombo.DelayedFinish))]
public class CrypticKeyComboDelayedFinishPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref CrypticKeyCombo __instance)
    {
        __instance.finishRoutine = null;
        __instance.Success();
        return false;
    }
}

/// No cooldown for enhancements
[HarmonyPatch(typeof(Enhancement), nameof(Enhancement.Update))]
public class EnhancementPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref Enhancement __instance)
    {
        if (__instance.State is EnhancementState.Cooldown or EnhancementState.Failed)
            __instance.SetState(EnhancementState.Inactive, __instance.AppliedEnhancementEffect.DefaultGrade(), 1f);
    }
}

/// Don't need to hold to activate levers/circles
[HarmonyPatch(typeof(Lever), nameof(Lever.StartClick))]
public class LeverPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref Lever __instance)
    {
        if (__instance.LeverPosition < __instance.triggerThreshold)
            __instance.SetPullLocked(true);
        else if (__instance.LeverPosition >= __instance.triggerThreshold)
            __instance.SetPullLocked(false);
    }
}
