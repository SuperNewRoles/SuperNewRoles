using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using UnhollowerBaseLib;
using UnityEngine;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;

namespace SuperNewRoles
{


    public static class RoleHelpers
    {
        public static bool isCrew(this PlayerControl player)
        {
            return player != null && !player.isImpostor() && !player.isNeutral();
        }

        public static bool isImpostor(this PlayerControl player)
        {
            return player != null && player.Data.Role.IsImpostor;
        }
        public static bool IsQuarreled(this PlayerControl player)
        {
            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer) {
                SuperNewRolesPlugin.Logger.LogInfo(players);
                foreach (PlayerControl p in players)
                {
                    SuperNewRolesPlugin.Logger.LogInfo(player.nameText.text);
                    SuperNewRolesPlugin.Logger.LogInfo(p.nameText.text);
                    if (p == player)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static void SetQuarreled(PlayerControl player1,PlayerControl player2)
        {
            var sets = new List<PlayerControl>() { player1, player2 };
            RoleClass.Quarreled.QuarreledPlayer.Add(sets);
        }
        public static void SetQuarreledRPC(PlayerControl player1, PlayerControl player2)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetQuarreled, Hazel.SendOption.Reliable, -1);
            Writer.Write(player1.PlayerId);
            Writer.Write(player2.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static void RemoveQuarreled(this PlayerControl player)
        {
            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer)
            {
                foreach (PlayerControl p in players)
                {
                    if (p == player)
                    {
                        RoleClass.Quarreled.QuarreledPlayer.Remove(players);
                        return;
                    }
                }
            }
        }
        public static PlayerControl GetOneSideQuarreled(this PlayerControl player)
        {
            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer)
            {
                foreach (PlayerControl p in players)
                {
                    if (p == player)
                    {
                        if (p == players[0])
                        {
                            return players[1];
                        } else
                        {
                            return players[0];
                        }
                    }
                }
            }
            return null;
        }
        public static void setRole(this PlayerControl player, RoleId role)
        {
            SuperNewRolesPlugin.Logger.LogInfo("SetRole");
            switch (role)
            {
                case (CustomRPC.RoleId.SoothSayer):
                    Roles.RoleClass.SoothSayer.SoothSayerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Jester):
                    Roles.RoleClass.Jester.JesterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Lighter):
                    Roles.RoleClass.Lighter.LighterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilLighter):
                    Roles.RoleClass.EvilLighter.EvilLighterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilScientist):
                    Roles.RoleClass.EvilScientist.EvilScientistPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Sheriff):
                    Roles.RoleClass.Sheriff.SheriffPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MeetingSheriff):
                    Roles.RoleClass.MeetingSheriff.MeetingSheriffPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Jackal):
                    Roles.RoleClass.Jackal.JackalPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Sidekick):
                    Roles.RoleClass.Jackal.SidekickPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Teleporter):
                    Roles.RoleClass.Teleporter.TeleporterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.SpiritMedium):
                    Roles.RoleClass.SpiritMedium.SpiritMediumPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.SpeedBooster):
                    Roles.RoleClass.SpeedBooster.SpeedBoosterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilSpeedBooster):
                    Roles.RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Tasker):
                    Roles.RoleClass.Tasker.TaskerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Doorr):
                    Roles.RoleClass.Doorr.DoorrPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilDoorr):
                    Roles.RoleClass.EvilDoorr.EvilDoorrPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Sealdor):
                    Roles.RoleClass.Sealdor.SealdorPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Speeder):
                    Roles.RoleClass.Speeder.SpeederPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Freezer):
                    Roles.RoleClass.Freezer.FreezerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Guesser):
                    Roles.RoleClass.Guesser.GuesserPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilGuesser):
                    Roles.RoleClass.EvilGuesser.EvilGuesserPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Vulture):
                    Roles.RoleClass.Vulture.VulturePlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.NiceScientist):
                    Roles.RoleClass.NiceScientist.NiceScientistPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Clergyman):
                    Roles.RoleClass.Clergyman.ClergymanPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MadMate):
                    Roles.RoleClass.MadMate.MadMatePlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Bait):
                    Roles.RoleClass.Bait.BaitPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.HomeSecurityGuard):
                    Roles.RoleClass.HomeSecurityGuard.HomeSecurityGuardPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.StuntMan):
                    Roles.RoleClass.StuntMan.StuntManPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Moving):
                    Roles.RoleClass.Moving.MovingPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Opportunist):
                    Roles.RoleClass.Opportunist.OpportunistPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.NiceGambler):
                    Roles.RoleClass.NiceGambler.NiceGamblerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilGambler):
                    Roles.RoleClass.EvilGambler.EvilGamblerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Bestfalsecharge):
                    Roles.RoleClass.Bestfalsecharge.BestfalsechargePlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Researcher):
                    Roles.RoleClass.Researcher.ResearcherPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.SelfBomber):
                    Roles.RoleClass.SelfBomber.SelfBomberPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.God):
                    Roles.RoleClass.God.GodPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.AllCleaner):
                    Roles.RoleClass.AllCleaner.AllCleanerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.NiceNekomata):
                    Roles.RoleClass.NiceNekomata.NiceNekomataPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilNekomata):
                    Roles.RoleClass.EvilNekomata.EvilNekomataPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.JackalFriends):
                    Roles.RoleClass.JackalFriends.JackalFriendsPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Doctor):
                    Roles.RoleClass.Doctor.DoctorPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.CountChanger):
                    Roles.RoleClass.CountChanger.CountChangerPlayer.Add(player);
                    break;
                //ロールアド
                default:
                    SuperNewRolesPlugin.Logger.LogError($"setRole: no method found for role type {role}");
                    return;
            }
        }
        private static PlayerControl ClearTarget;
        public static void ClearRole(this PlayerControl player)
        {
            static bool ClearRemove(PlayerControl p)
            {
                SuperNewRolesPlugin.Logger.LogInfo("-ClearRole-");
                SuperNewRolesPlugin.Logger.LogInfo(p.PlayerId);
                SuperNewRolesPlugin.Logger.LogInfo(ClearTarget.PlayerId);
                SuperNewRolesPlugin.Logger.LogInfo("-----------");
                if (p.PlayerId == ClearTarget.PlayerId) return true;
                return false;
            }
            ClearTarget = player;
            SuperNewRolesPlugin.Logger.LogInfo("ClearRole");
            switch (player.getRole())
            {
                case (CustomRPC.RoleId.SoothSayer):
                    Roles.RoleClass.SoothSayer.SoothSayerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Jester):
                    Roles.RoleClass.Jester.JesterPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Lighter):
                    Roles.RoleClass.Lighter.LighterPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilLighter):
                    Roles.RoleClass.EvilLighter.EvilLighterPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilScientist):
                    Roles.RoleClass.EvilScientist.EvilScientistPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Sheriff):
                    Roles.RoleClass.Sheriff.SheriffPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MeetingSheriff):
                    Roles.RoleClass.MeetingSheriff.MeetingSheriffPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Jackal):
                    Roles.RoleClass.Jackal.JackalPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Sidekick):
                    Roles.RoleClass.Jackal.SidekickPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Teleporter):
                    Roles.RoleClass.Teleporter.TeleporterPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.SpiritMedium):
                    Roles.RoleClass.SpiritMedium.SpiritMediumPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.SpeedBooster):
                    Roles.RoleClass.SpeedBooster.SpeedBoosterPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilSpeedBooster):
                    Roles.RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Tasker):
                    Roles.RoleClass.Tasker.TaskerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Doorr):
                    Roles.RoleClass.Doorr.DoorrPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilDoorr):
                    Roles.RoleClass.EvilDoorr.EvilDoorrPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Sealdor):
                    Roles.RoleClass.Sealdor.SealdorPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Speeder):
                    Roles.RoleClass.Speeder.SpeederPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Freezer):
                    Roles.RoleClass.Freezer.FreezerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Guesser):
                    Roles.RoleClass.Guesser.GuesserPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilGuesser):
                    Roles.RoleClass.EvilGuesser.EvilGuesserPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Vulture):
                    Roles.RoleClass.Vulture.VulturePlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.NiceScientist):
                    Roles.RoleClass.NiceScientist.NiceScientistPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Clergyman):
                    Roles.RoleClass.Clergyman.ClergymanPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadMate):
                    Roles.RoleClass.MadMate.MadMatePlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Bait):
                    Roles.RoleClass.Bait.BaitPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.HomeSecurityGuard):
                    Roles.RoleClass.HomeSecurityGuard.HomeSecurityGuardPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.StuntMan):
                    Roles.RoleClass.StuntMan.StuntManPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Moving):
                    Roles.RoleClass.Moving.MovingPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Opportunist):
                    Roles.RoleClass.Opportunist.OpportunistPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.NiceGambler):
                    Roles.RoleClass.NiceGambler.NiceGamblerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilGambler):
                    Roles.RoleClass.EvilGambler.EvilGamblerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Bestfalsecharge):
                    Roles.RoleClass.Bestfalsecharge.BestfalsechargePlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Researcher):
                    Roles.RoleClass.Researcher.ResearcherPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.SelfBomber):
                    Roles.RoleClass.SelfBomber.SelfBomberPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.God):
                    Roles.RoleClass.God.GodPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.AllCleaner):
                    Roles.RoleClass.AllCleaner.AllCleanerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.NiceNekomata):
                    Roles.RoleClass.NiceNekomata.NiceNekomataPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilNekomata):
                    Roles.RoleClass.EvilNekomata.EvilNekomataPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.JackalFriends):
                    Roles.RoleClass.JackalFriends.JackalFriendsPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Doctor):
                    Roles.RoleClass.Doctor.DoctorPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.CountChanger):
                    Roles.RoleClass.CountChanger.CountChangerPlayer.RemoveAll(ClearRemove);
                    break;
                //ロールリモベ
            }
        }
        public static void setRoleRPC(this PlayerControl Player,RoleId SelectRoleDate)
        {
            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            killWriter.Write(Player.PlayerId);
            killWriter.Write((byte)SelectRoleDate);
            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
            RPCProcedure.SetRole(Player.PlayerId, (byte)SelectRoleDate);
        }
        public static bool isClearTask(this PlayerControl player) {
            var IsTaskClear = false;
            switch (player.getRole())
            {
                case (RoleId.Jester):
                    IsTaskClear = true;
                    break;
                case (RoleId.Jackal):
                    IsTaskClear = true;
                    break;
                case (RoleId.Sidekick):
                    IsTaskClear = true;
                    break;
                case (RoleId.Vulture):
                    IsTaskClear = true;
                    break;
                case (RoleId.HomeSecurityGuard):
                    IsTaskClear = true;
                    break;
                case (RoleId.MadMate):
                    IsTaskClear = true;
                    break;
                case (RoleId.JackalFriends):
                    IsTaskClear = true;
                    break;
                case (RoleId.Opportunist):
                    IsTaskClear = true;
                    break; 
                case (RoleId.Researcher):
                    IsTaskClear = true;
                    break; 
                case (RoleId.God):
                    IsTaskClear = true;
                    break; 
                //タスククリアか
            }
            return IsTaskClear;
        }
        public static bool IsUseVent(this PlayerControl player)
        {
            if (player.Data.Role.IsImpostor) return true;
            if (RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(player) && Roles.RoleClass.Jester.IsUseVent) return true;
            if (RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(player) && Roles.RoleClass.MadMate.IsUseVent) return true;
            if ((RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(player) || 
                RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(player)) && Roles.RoleClass.Jackal.IsUseVent) return true;
            if (player.isRole(RoleId.JackalFriends) && RoleClass.JackalFriends.IsUseVent) return true;
            return false;
        }
        public static bool IsUseSabo(this PlayerControl player)
        {
            if (player.Data.Role.IsImpostor) return true;
            if (Roles.RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(player) && Roles.RoleClass.Jester.IsUseSabo) return true;
            if ((RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(player) ||
                RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(player)) && Roles.RoleClass.Jackal.IsUseSabo) return true;
            return false;
        }
        public static bool isNeutral(this PlayerControl player)
        {
            var IsNeutral = false;
            switch (player.getRole())
            {
                case (RoleId.Jester):
                    IsNeutral = true;
                    break;
                case (RoleId.Jackal):
                    IsNeutral = true;
                    break;
                case (RoleId.Sidekick):
                    IsNeutral = true;
                    break;
                case (RoleId.Vulture):
                    IsNeutral = true;
                    break;
                case (RoleId.Opportunist):
                    IsNeutral = true;
                    break;
                case (RoleId.Researcher):
                    IsNeutral = true;
                    break;
                case (RoleId.God):
                    IsNeutral = true;
                    break;
                //第三か
            }
            return IsNeutral;
        }
        public static bool isRole(this PlayerControl p,RoleId role) {
            if (p.getRole() == role)
            {
                return true;
            }
            else {
                return false;
            }
            return false;
        }
        public static SuperNewRoles.CustomRPC.RoleId getRole(this PlayerControl player)
        {
            if (SuperNewRoles.Roles.RoleClass.SoothSayer.SoothSayerPlayer.IsCheckListPlayerControl(player))
            {
                return SuperNewRoles.CustomRPC.RoleId.SoothSayer;
            } else if (Roles.RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(player)) {
                return CustomRPC.RoleId.Jester;
            } else if (Roles.RoleClass.Lighter.LighterPlayer.IsCheckListPlayerControl(player)) {
                return CustomRPC.RoleId.Lighter;
            } else if (Roles.RoleClass.EvilLighter.EvilLighterPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilLighter;
            }
            else if (Roles.RoleClass.EvilScientist.EvilScientistPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilScientist;
            }
            else if (Roles.RoleClass.Sheriff.SheriffPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Sheriff;
            }
            else if (Roles.RoleClass.MeetingSheriff.MeetingSheriffPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.MeetingSheriff;
            }
            else if (Roles.RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Jackal;
            }
            else if (Roles.RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Sidekick;
            }
            else if (Roles.RoleClass.Teleporter.TeleporterPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Teleporter;
            }
            else if (Roles.RoleClass.SpiritMedium.SpiritMediumPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.SpiritMedium;
            }
            else if (Roles.RoleClass.SpeedBooster.SpeedBoosterPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.SpeedBooster;
            }
            else if (Roles.RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilSpeedBooster;
            }
            else if (Roles.RoleClass.Tasker.TaskerPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Tasker;
            }
            else if (Roles.RoleClass.Doorr.DoorrPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Doorr;
            }
            else if (Roles.RoleClass.EvilDoorr.EvilDoorrPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilDoorr;
            }
            else if (Roles.RoleClass.Sealdor.SealdorPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Sealdor;
            }
            else if (Roles.RoleClass.Sealdor.SealdorPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Sealdor;
            }
            else if (Roles.RoleClass.Speeder.SpeederPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Speeder;
            }
            else if (Roles.RoleClass.Freezer.FreezerPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Freezer;
            }
            else if (Roles.RoleClass.Guesser.GuesserPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Guesser;
            }
            else if (Roles.RoleClass.EvilGuesser.EvilGuesserPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilGuesser;
            }
            else if (Roles.RoleClass.Vulture.VulturePlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Vulture;
            }
            else if (Roles.RoleClass.NiceScientist.NiceScientistPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.NiceScientist;
            }
            else if (Roles.RoleClass.Clergyman.ClergymanPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Clergyman;
            }
            else if (Roles.RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.MadMate;
            }
            else if (Roles.RoleClass.Bait.BaitPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Bait;
            }
            else if (Roles.RoleClass.HomeSecurityGuard.HomeSecurityGuardPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.HomeSecurityGuard;
            }
            else if (Roles.RoleClass.StuntMan.StuntManPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.StuntMan;
            }
            else if (Roles.RoleClass.Moving.MovingPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Moving;
            }
            else if (Roles.RoleClass.Opportunist.OpportunistPlayer.IsCheckListPlayerControl(player)) 
            {
                return CustomRPC.RoleId.Opportunist;
            }
            else if (Roles.RoleClass.NiceGambler.NiceGamblerPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.NiceGambler;
            }
            else if (Roles.RoleClass.EvilGambler.EvilGamblerPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilGambler;
            }
            else if (Roles.RoleClass.Bestfalsecharge.BestfalsechargePlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Bestfalsecharge;
            }
            else if (Roles.RoleClass.Researcher.ResearcherPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Researcher;
            }
            else if (Roles.RoleClass.SelfBomber.SelfBomberPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.SelfBomber;
            }
            else if (Roles.RoleClass.God.GodPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.God;
            }
            else if (Roles.RoleClass.AllCleaner.AllCleanerPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.AllCleaner;
            }
            else if (Roles.RoleClass.NiceNekomata.NiceNekomataPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.NiceNekomata;
            }
            else if (Roles.RoleClass.EvilNekomata.EvilNekomataPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilNekomata;
            }
            else if (Roles.RoleClass.JackalFriends.JackalFriendsPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.JackalFriends;
            }
            else if (Roles.RoleClass.Doctor.DoctorPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Doctor;
            }
            else if (Roles.RoleClass.CountChanger.CountChangerPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.CountChanger;
            }
            //ロールチェック
            return SuperNewRoles.CustomRPC.RoleId.DefaultRole;

        }
        public static bool isDead(this PlayerControl player)
        {
            return player.Data.IsDead || player.Data.Disconnected;
        }

        public static bool isAlive(this PlayerControl player)
        {
            return !isDead(player);
        }
    }
}
