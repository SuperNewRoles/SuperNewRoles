using HarmonyLib;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class SaboAndVent
{
    public static EventListener updateEventListener;
    private static int _lastUpdateFrame = -1;
    private static bool _cachedCanUseVent;
    private static bool _cachedShowVentButtonVanilla;
    private static bool _cachedCanSabotage;
    private static bool _cachedShowSaboButtonVanilla;
    private static bool _cachedIsShowKillButton;
    private static bool _cachedKillDisabled;

    public static void RegisterListener()
    {
        updateEventListener = HudUpdateEvent.Instance.AddListener(SaboAndVentUpdate);
    }
    public static void SaboAndVentUpdate()
    {
        int frame = Time.frameCount;
        if (frame != _lastUpdateFrame)
        {
            _lastUpdateFrame = frame;
            _cachedCanUseVent = ExPlayerControl.LocalPlayer.CanUseVent();
            _cachedShowVentButtonVanilla = ExPlayerControl.LocalPlayer.ShowVanillaVentButton();
            _cachedCanSabotage = ExPlayerControl.LocalPlayer.CanSabotage();
            _cachedShowSaboButtonVanilla = ExPlayerControl.LocalPlayer.ShowVanillaSabotageButton();
            _cachedIsShowKillButton = ExPlayerControl.LocalPlayer.showKillButtonVanilla();
            _cachedKillDisabled = ExPlayerControl.LocalPlayer.HasCustomKillButton() || !_cachedIsShowKillButton;
        }

        HudManager.Instance.ImpostorVentButton.gameObject.SetActive(_cachedCanUseVent && _cachedShowVentButtonVanilla);
        HudManager.Instance.SabotageButton.gameObject.SetActive(_cachedCanSabotage && _cachedShowSaboButtonVanilla);
        if (_cachedKillDisabled)
            HudManager.Instance.KillButton.gameObject.SetActive(false);
        else if (_cachedIsShowKillButton && (HudManager.Instance.UseButton.gameObject.activeSelf || HudManager.Instance.PetButton.gameObject.activeSelf))
            HudManager.Instance.KillButton.gameObject.SetActive(true);
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    class HudManagerUpdatePatch
    {
        public static void Postfix(HudManager __instance)
        {
            if (Vent.currentVent != null && __instance.ImpostorVentButton != null)
                __instance.ImpostorVentButton.SetTarget(Vent.currentVent);
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    class GameStartManagerStartPatch
    {
        public static void Postfix()
        {
            if (DestroyableSingleton<HudManager>.InstanceExists && HudManager.Instance.SabotageButton != null)
                HudManager.Instance.SabotageButton.gameObject.SetActive(false);
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), new[] { typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool) })]
    class HudManagerSetHudActivePatch
    {
        public static void Postfix()
        {
            if (PlayerControl.LocalPlayer == null || ExPlayerControl.LocalPlayer == null)
                return;

            SaboAndVentUpdate();
        }
    }
    [HarmonyPatch(typeof(NormalGameManager), nameof(NormalGameManager.GetMapOptions))]
    class NormalGameManagerGetMapOptionsPatch
    {
        public static void Postfix(ref MapOptions __result)
        {
            if (__result == null)
                return;

            if (MeetingHud.Instance)
            {
                __result.Mode = MapOptions.Modes.Normal;
                return;
            }

            if (ExPlayerControl.LocalPlayer == null)
                return;

            __result.Mode = ExPlayerControl.LocalPlayer.CanSabotage() && GameManager.Instance.SabotagesEnabled()
                ? MapOptions.Modes.Sabotage
                : MapOptions.Modes.Normal;
        }
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowNormalMap))]
    class MapBehaviourShowNormalMapPatch
    {
        public static bool Prefix(MapBehaviour __instance)
        {
            if (MeetingHud.Instance)
                return true;
            if (ExPlayerControl.LocalPlayer.CanSabotage() && !__instance.IsOpen)
            {
                __instance.Close();
                FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
                {
                    Mode = MapOptions.Modes.Sabotage,
                    AllowMovementWhileMapOpen = true
                });
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
    class MapBehaviourShowSabotageMapPatch
    {
        public static bool Prefix(MapBehaviour __instance)
        {
            if (MeetingHud.Instance)
                return true;
            if (!ExPlayerControl.LocalPlayer.CanSabotage() && !__instance.IsOpen)
            {
                __instance.Close();
                FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
                {
                    Mode = MapOptions.Modes.Normal,
                    AllowMovementWhileMapOpen = true
                });
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    class SabotageButtonDoClickPatch
    {
        public static bool Prefix()
        {
            if (ExPlayerControl.LocalPlayer == null)
                return true;

            if (!ExPlayerControl.LocalPlayer.CanSabotage())
                return false;

            if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
                return true;

            if (!PlayerControl.LocalPlayer.inVent && GameManager.Instance.SabotagesEnabled())
            {
                DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
                {
                    Mode = MapOptions.Modes.Sabotage,
                    AllowMovementWhileMapOpen = true
                });
            }
            return false;
        }
    }
}

