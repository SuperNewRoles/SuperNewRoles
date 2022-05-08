using HarmonyLib;
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
                if (PlayerControl.LocalPlayer.PlayerId == __instance.PlayerId && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.DarkKiller))
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
                    HudManager.Instance.KillButton.enabled = true;
                }
                var ma = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                if (ma != null && !ma.IsActive)
                {
                    HudManager.Instance.KillButton.enabled = false;
                }
;
            }
        }
        public class FixedUpdate
        {
            public static void Postfix()
            {
                SetDarkKillerButton();
            }
        }
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
        class DarkKillerDestroyPatch
        {
            public static void Prefix(IntroCutscene __instance)
            {

                if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.DarkKiller))
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.DarkKiller.KillCoolTime);
                }
            }
        }
    }
}
