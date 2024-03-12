using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using static SuperNewRoles.Helpers.RPCHelper;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles;

class Jackal
{
    public static void ResetCooldown()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.Jackal.KillCooldown;
        HudManagerStartPatch.JackalKillButton.Timer = RoleClass.Jackal.KillCooldown;
    }
    public static void EndMeetingResetCooldown()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.Jackal.KillCooldown;
        HudManagerStartPatch.JackalKillButton.Timer = RoleClass.Jackal.KillCooldown;
        HudManagerStartPatch.JackalSidekickButton.MaxTimer = CustomOptionHolder.JackalSKCooldown.GetFloat();
        HudManagerStartPatch.JackalSidekickButton.Timer = CustomOptionHolder.JackalSKCooldown.GetFloat();
    }
    public static void EndMeeting() => EndMeetingResetCooldown();
    public static void SetPlayerOutline(PlayerControl target, Color color)
    {
        if (target == null) return;
        SpriteRenderer rend = target.MyRend();
        if (rend == null) return;
        rend.material.SetFloat("_Outline", 1f);
        rend.material.SetColor("_OutlineColor", color);
    }
    public class JackalFixedPatch
    {
        static void JackalPlayerOutLineTarget()
            => SetPlayerOutline(JackalSetTarget(), RoleClass.Jackal.color);
        public static void Postfix(PlayerControl __instance, RoleId role)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (RoleClass.Jackal.SidekickPlayer.Count > 0)
                {
                    var upflag = true;
                    foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                    {
                        if (p.IsAlive())
                        {
                            upflag = false;
                        }
                    }
                    if (upflag)
                    {
                        byte jackalId = (byte)RoleId.Jackal;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, SendOption.Reliable, -1);
                        writer.Write(jackalId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.SidekickPromotes(jackalId);
                    }
                }
            }
            if (role == RoleId.Jackal)
            {
                JackalPlayerOutLineTarget();
            }
        }
    }
    /// <summary>
    /// (役職をリセットし、)ジャッカルフレンズに割り当てます。
    /// </summary>
    /// <param name="target">役職がJackalFriendsに変更される対象</param>
    public static void CreateJackalFriends(PlayerControl target)
    {
        List<RoleTypes> CanNotHaveTaskForRoles = new() { RoleTypes.Impostor, RoleTypes.Shapeshifter, RoleTypes.ImpostorGhost };
        // マッドメイトになる前にタスクを持っていたかを取得
        var canNotHaveTask = CanNotHaveTaskForRoles.Contains(target.Data.Role.Role);
        canNotHaveTask = CanNotHaveTaskForRoles.Contains(RoleSelectHandler.GetDesyncRole(target.GetRole()).RoleType);// Desync役職ならタスクを持っていなかったと見なす ( 個別設定 )
        if (target.GetRoleBase() is ISupportSHR supportSHR) { canNotHaveTask = CanNotHaveTaskForRoles.Contains(supportSHR.DesyncRole); } // Desync役職ならタスクを持っていなかったと見なす ( RoleBace )

        target.ResetAndSetRole(RoleId.JackalFriends);
        if (target.IsRole(RoleId.JackalFriends)) JackalFriends.ChangeJackalFriendsPlayer[target.PlayerId] = !canNotHaveTask;
    }
}