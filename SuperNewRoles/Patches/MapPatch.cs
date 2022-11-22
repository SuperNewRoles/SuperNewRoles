using HarmonyLib;

namespace SuperNewRoles.Patches
{
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
}