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
        bool ventActive = ExPlayerControl.LocalPlayer.CanUseVent();
        bool saboActive = ExPlayerControl.LocalPlayer.CanSabotage();
        bool killDisabled = ExPlayerControl.LocalPlayer.HasCustomKillButton() || !ExPlayerControl.LocalPlayer.showKillButtonVanilla();
        SetVentActive(ventActive);
        SetSaboActive(saboActive);
        SetKillActive(killDisabled);
    }
    private static void SetVentActive(bool ventActive)
    {
        if (!ventActive)
            HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
    }
    private static void SetSaboActive(bool saboActive)
    {
        if (!saboActive)
            HudManager.Instance.SabotageButton.gameObject.SetActive(false);
    }
    private static void SetKillActive(bool killDisabled)
    {
        if (killDisabled)
            HudManager.Instance.KillButton.gameObject.SetActive(false);
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

