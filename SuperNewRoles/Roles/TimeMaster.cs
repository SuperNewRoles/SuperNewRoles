using Hazel;
using SuperNewRoles.Buttons;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class TimeMaster
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.TimeMasterTimeMasterShieldButton.MaxTimer = RoleClass.TimeMaster.Cooldown;
            RoleClass.TimeMaster.ButtonTimer = DateTime.Now;
        }
        public static void TimeShieldStart()
        {
            List<PlayerControl> aliveplayers = new List<PlayerControl>();
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.isAlive() && p.CanMove)
                {
                    aliveplayers.Add(p);
                }
            }
            var player = ModHelpers.GetRandom<PlayerControl>(aliveplayers);
            CustomRPC.RPCProcedure.TimeMasterShield();

            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TimeMasterShield, Hazel.SendOption.Reliable, -1);
            Writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static bool IsTimeMaster(PlayerControl Player)
        {
            if (RoleClass.TimeMaster.TimeMasterPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.TimeMasterTimeMasterShieldButton.MaxTimer = RoleClass.TimeMaster.Cooldown;
            RoleClass.TimeMaster.ButtonTimer = DateTime.Now;
        }
    }
}