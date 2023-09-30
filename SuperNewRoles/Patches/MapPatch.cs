using HarmonyLib;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(MapBehaviour), "get_IsOpenStopped")]
class MapBehaviorGetIsOpenStoppedPatch
{
    static bool Prefix(ref bool __result)
    { // イビルハッカーがアドミン使用中に移動できる
        if (CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.EvilHacker) && CustomOptionHolder.EvilHackerCanMoveWhenUsesAdmin.GetBool())
        {
            __result = false;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Show))]
public static class MapBehaviourShowPatch
{
    public static void Prefix([HarmonyArgument(0)] MapOptions opts)
    {
        // 会議中にマップを開くとアドミンを見ることができる
        if (CachedPlayer.LocalPlayer.PlayerControl.IsRole(RoleId.EvilHacker) && RoleClass.EvilHacker.CanUseAdminDuringMeeting && MeetingHud.Instance && opts.Mode == MapOptions.Modes.Normal)
        {
            RoleClass.EvilHacker.IsMyAdmin = true;
            opts.Mode = MapOptions.Modes.CountOverlay;
        }
    }
}
