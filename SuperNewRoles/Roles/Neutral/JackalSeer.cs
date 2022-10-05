using Hazel;
using SuperNewRoles.Buttons;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles
{
    class JackalSeer
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.JackalSeer.KillCoolDown;
            HudManagerStartPatch.JackalKillButton.Timer = RoleClass.JackalSeer.KillCoolDown;
            HudManagerStartPatch.JackalSeerSidekickButton.MaxTimer = RoleClass.JackalSeer.KillCoolDown;
            HudManagerStartPatch.JackalSeerSidekickButton.Timer = RoleClass.JackalSeer.KillCoolDown;
        }
        public static void EndMeeting()
        {
            ResetCoolDown();
        }
        public class JackalSeerFixedPatch
        {
            static void JackalSeerPlayerOutLineTarget()
            {
                SetPlayerOutline(JackalSetTarget(), RoleClass.JackalSeer.color);
            }
            public static void Postfix(PlayerControl __instance, RoleId role)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (RoleClass.JackalSeer.SidekickSeerPlayer.Count > 0)
                    {
                        var upflag = true;
                        foreach (PlayerControl p in RoleClass.JackalSeer.JackalSeerPlayer)
                        {
                            if (p.IsAlive())
                            {
                                upflag = false;
                            }
                        }
                        if (upflag)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, SendOption.Reliable, -1);
                            writer.Write(true);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.SidekickPromotes(true);
                        }
                    }
                }
                if (role == RoleId.JackalSeer)
                {
                    JackalSeerPlayerOutLineTarget();
                }
            }
        }
    }
}