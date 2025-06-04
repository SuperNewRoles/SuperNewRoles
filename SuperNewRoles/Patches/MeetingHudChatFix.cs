using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
public static class MeetingHudChatFix
{
    public static void Postfix(MeetingHud __instance)
    {
        DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(false);
        new LateTask(() => DestroyableSingleton<HudManager>.Instance.Chat.SetVisible(ExPlayerControl.LocalPlayer.IsDead()), 1.5f);
    }
}