using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    public class DarkKiller
    {
        public class MurderPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (CachedPlayer.LocalPlayer.PlayerId == __instance.PlayerId && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.DarkKiller))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.DarkKiller.KillCoolTime);
                }
            }
        }
        public static void SetDarkKillerButton()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.DarkKiller))
            {
                if (!RoleClass.DarkKiller.KillButtonDisable)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillButton.enabled = true;

                    var ma = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                    if (ma != null && !ma.IsActive)
                    {
                        FastDestroyableSingleton<HudManager>.Instance.KillButton.enabled = false;
                    }
                }
            }
        }
        public class FixedUpdate
        {
            public static void Postfix()
            {
                SetDarkKillerButton();
            }
        }
    }
}
