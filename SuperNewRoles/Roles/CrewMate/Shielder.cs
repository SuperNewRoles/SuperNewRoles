using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class Shielder
    {
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class HudManagerUpdatePatch
        {
            public static void Postfix()
            {
                if (HudManagerStartPatch.ShielderButton.Timer <= 0.1f && RoleClass.Shielder.IsShield[CachedPlayer.LocalPlayer.PlayerId] && PlayerControl.LocalPlayer.IsRole(RoleId.Shielder))
                {
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetShielder, SendOption.Reliable, -1);
                    Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    Writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    RPCProcedure.SetShielder(CachedPlayer.LocalPlayer.PlayerId, false);
                    HudManagerStartPatch.ShielderButton.actionButton.cooldownTimerText.color = Color.white;
                    HudManagerStartPatch.ShielderButton.MaxTimer = RoleClass.Shielder.CoolTime;
                    HudManagerStartPatch.ShielderButton.Timer = HudManagerStartPatch.ShielderButton.MaxTimer;
                }
            }
        }
    }
}