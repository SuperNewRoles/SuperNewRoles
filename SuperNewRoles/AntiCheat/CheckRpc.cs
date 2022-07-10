using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Roles;

namespace SuperNewRoles.AntiCheat
{
    class CheckRpc
    {
        public static bool CheckSetRole(PlayerControl player, RoleId role)
        {
            if (!role.isGhostRole() && RoleClass.AssignedPlayer.Contains(player.PlayerId) &&
                !RoleHelpers.IsInGameAssignRole(role) && !player.isRole(RoleId.Amnesiac))
            {
                Logger.Error($"SetRoleでAssignedPlayerのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            if (!RoleClass.AssignedPlayer.Contains(player.PlayerId) && role == RoleId.MadKiller)
            {
                Logger.Error($"SetRoleでMadKillerCheckのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            return true;
        }
        public static bool CheckSetRoomTimerRPC(byte min, byte seconds)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                Logger.Error($"SetRoomTimerでAmHostのアンチチートが発生しました。min:{min}、seconds:{seconds}", "AntiCheat");
                return false;
            }
            if (Patch.ShareGameVersion.timer < ((min * 60) + seconds))
            {
                Logger.Error($"SetRoomTimerでTimerCheckのアンチチートが発生しました。min:{min}、seconds{seconds}", "AntiCheat");
                return false;
            }
            return true;
        }
        public static bool CheckCustomEndGame(CustomGameOverReason reason, bool showAd)
        {
            switch (reason)
            {
                case (CustomGameOverReason)GameOverReason.ImpostorByKill:
                case CustomGameOverReason.ArsonistWin:
                case CustomGameOverReason.VultureWin:
                case CustomGameOverReason.FalseChargesWin:
                    return true;
            }
            Logger.Error($"CustomEndGameでreasonのアンチチートが発生しました。reason:{reason}, showAd:{showAd}", "AntiCheat");
            return false;
        }
        public static bool CheckMeetingSheriffKill(PlayerControl player, PlayerControl target)
        {
            if (!player.isRole(RoleId.MeetingSheriff))
            {
                Logger.Error($"MeetingSheriffKillでisRoleのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            if (!RoleClass.IsMeeting)
            {
                Logger.Error($"MeetingSheriffKillでIsMeetingのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            if (target.isDead())
            {
                Logger.Error($"MeetingSheriffKillでtargetisDeadのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            if (player.isDead())
            {
                Logger.Error($"MeetingSheriffKillでplayerisAliveのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            return true;
        }
        public static bool CheckSheriffKill(PlayerControl player, PlayerControl target)
        {
            if (!player.isRole(RoleId.Sheriff, RoleId.RemoteSheriff))
            {
                Logger.Error($"CheckSheriffKillでisRoleのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            if (RoleClass.IsMeeting)
            {
                Logger.Error($"CheckSheriffKillでIsMeetingのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            if (target.isDead())
            {
                Logger.Error($"CheckSheriffKillでtargetisDeadのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            if (player.isDead())
            {
                Logger.Error($"CheckSheriffKillでplayerisDeadのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            return true;
        }
        public static bool CheckCustomRPCKill(PlayerControl player)
        {
            if (RoleClass.IsMeeting) {
                Logger.Error($"CustomRPCKillでIsMeetingのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}" ,"AntiCheat");
                return false;
            }
            return true;
        }
        public static bool CheckRevive(PlayerControl player)
        {
            return true;
        }
        public static bool CheckCreateSidekick(PlayerControl player)
        {
            if (player.IsJackalTeam())
            {
                Logger.Error($"CreateSidekickでIsJackalTeamのアンチチートが発生しました。ID:{player.PlayerId}、プレイヤー:{player.Data.PlayerName}、役職{player.getRole()}", "AntiCheat");
                return false;
            }
            return true;
        }
        public static bool CheckSidekickPromotes(RoleId role)
        {
            switch (role)
            {
                case RoleId.Sidekick:
                    if (RoleClass.Jackal.SidekickPlayer.Count <= 0)
                    {
                        Logger.Error($"SidekickPromotesでSidekickCountのチェックアンチチートが発生しました。", "AntiCheat");
                        return false;
                    }
                    foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                    {
                        if (p.isAlive())
                        {
                            Logger.Error($"SidekickPromotesでSidekickIsAliveのアンチチートが発生しました。ID:{p.PlayerId}、プレイヤー:{p.Data.PlayerName}、役職{p.getRole()}", "AntiCheat");
                            return false;
                        }
                    }
                    break;
                case RoleId.SidekickSeer:
                    if (RoleClass.JackalSeer.SidekickSeerPlayer.Count <= 0) return false;
                    foreach (PlayerControl p in RoleClass.JackalSeer.JackalSeerPlayer)
                    {
                        if (p.isAlive())
                        {
                            Logger.Error($"SidekickPromotesでSidekickSeerIsAliveのアンチチートが発生しました。ID:{p.PlayerId}、プレイヤー:{p.Data.PlayerName}、役職{p.getRole()}", "AntiCheat");
                            return false;
                        }
                    }
                    break;
                default:
                    Logger.Error("CheckSidekickPromotesで不明なRoleIdを受け取りました:" + role, "AntiCheat");
                    return false;
            }
            return true;
        }
    }
}
