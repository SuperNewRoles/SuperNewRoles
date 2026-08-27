using HarmonyLib;

namespace SuperNewRoles.Patches;

/// <summary>
/// バニラのファントムは透明化後に Appear で解除する。
/// Among Us は <see cref="Constants.IsVersionModded"/> 時の CmdCheckVanish / CmdCheckAppear に
/// ホスト自身の分岐がなく、ホストが送った RPC を自分では処理しない。
/// SNR は常に Mod 扱いにするため、ホストでは Check* を直接実行して解除できるようにする。
/// </summary>

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
public static class PlayerControlCmdCheckVanishPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
            return true;

        __instance.CheckVanish();
        return false;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
public static class PlayerControlCmdCheckAppearPatch
{
    public static bool Prefix(PlayerControl __instance, bool shouldAnimate)
    {
        if (!AmongUsClient.Instance.AmHost)
            return true;

        __instance.CheckAppear(shouldAnimate);
        return false;
    }
}
