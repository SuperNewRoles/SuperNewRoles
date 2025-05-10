using HarmonyLib;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Modules;

public class SaboAndVent
{
    public static EventListener updateEventListener;
    public static void RegisterListener()
    {
        updateEventListener = HudUpdateEvent.Instance.AddListener(SaboAndVentUpdate);
    }
    public static void SaboAndVentUpdate()
    {
        bool canUseVent = ExPlayerControl.LocalPlayer.CanUseVent();
        bool showVentButtonVanilla = ExPlayerControl.LocalPlayer.ShowVanillaVentButton();
        bool canSabotage = ExPlayerControl.LocalPlayer.CanSabotage();
        bool showSaboButtonVanilla = ExPlayerControl.LocalPlayer.ShowVanillaSabotageButton();
        bool isShowKillButton = ExPlayerControl.LocalPlayer.showKillButtonVanilla();
        bool killDisabled = ExPlayerControl.LocalPlayer.HasCustomKillButton() || !isShowKillButton;
        SetVentActive(canUseVent, showVentButtonVanilla);
        SetSaboActive(canSabotage, showSaboButtonVanilla);
        SetKillActive(killDisabled, isShowKillButton);
    }
    private static void SetVentActive(bool canUseVent, bool showVentButtonVanilla)
    {
        if (!canUseVent || !showVentButtonVanilla)
            HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
        else
            HudManager.Instance.ImpostorVentButton.gameObject.SetActive(true);
    }
    private static void SetSaboActive(bool canSabotage, bool showSaboButtonVanilla)
    {
        if (!canSabotage || !showSaboButtonVanilla)
            HudManager.Instance.SabotageButton.gameObject.SetActive(false);
        else
            HudManager.Instance.SabotageButton.gameObject.SetActive(true);
    }
    private static void SetKillActive(bool killDisabled, bool isShowKillButton)
    {
        if (killDisabled)
            HudManager.Instance.KillButton.gameObject.SetActive(false);
        else if (isShowKillButton && (HudManager.Instance.UseButton.gameObject.activeSelf || HudManager.Instance.PetButton.gameObject.activeSelf))
            HudManager.Instance.KillButton.gameObject.SetActive(true);
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
}

