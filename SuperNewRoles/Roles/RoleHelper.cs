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
using SuperNewRoles.Mode;
using SuperNewRoles.CustomOption;

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
            if (player.isRole(RoleId.Sheriff)) return false;
            return player != null && player.Data.Role.IsImpostor;
        }
        public static bool IsQuarreled(this PlayerControl player,bool IsChache = true)
        {
            if (IsChache)
            {
                try
                {
                    if (ChacheManager.QuarreledChache[player.PlayerId] == null)
                        return false;
                    else
                        return true;
                }
                catch
                {
                    return false;
                }
            }
            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer) {
                foreach (PlayerControl p in players)
                {
                    if (p == player)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool IsLovers(this PlayerControl player,bool IsChache = true)
        {
            if (IsChache)
            {
                try
                {
                    if (ChacheManager.LoversChache[player.PlayerId] == null)
                        return false;
                    else
                        return true;
                }
                catch
                {
                    return false;
                }
            }
            foreach (List<PlayerControl> players in RoleClass.Lovers.LoversPlayer)
            {
                foreach (PlayerControl p in players)
                {
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
            ChacheManager.ResetQuarreledChache();
        }
        public static void SetQuarreledRPC(PlayerControl player1, PlayerControl player2)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetQuarreled, Hazel.SendOption.Reliable, -1);
            Writer.Write(player1.PlayerId);
            Writer.Write(player2.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static void SetLovers(PlayerControl player1, PlayerControl player2)
        {
            var sets = new List<PlayerControl>() { player1, player2 };
            RoleClass.Lovers.LoversPlayer.Add(sets);
            if (player1.PlayerId == PlayerControl.LocalPlayer.PlayerId || player2.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            }
            ChacheManager.ResetLoversChache();
        }
        public static void SetLoversRPC(PlayerControl player1, PlayerControl player2)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetLovers, Hazel.SendOption.Reliable, -1);
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
        public static PlayerControl GetOneSideQuarreled(this PlayerControl player,bool IsChache = true)
        {
            if (IsChache)
            {
                if (ChacheManager.QuarreledChache[player.PlayerId] == null) return null;
                return ChacheManager.QuarreledChache[player.PlayerId];
            }
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
        public static PlayerControl GetOneSideLovers(this PlayerControl player,bool IsChache = true)
        {
            if (IsChache)
            {
                if (ChacheManager.LoversChache[player.PlayerId] == null) return null;
                return ChacheManager.LoversChache[player.PlayerId];
            }
            foreach (List<PlayerControl> players in RoleClass.Lovers.LoversPlayer)
            {
                foreach (PlayerControl p in players)
                {
                    if (p == player)
                    {
                        if (p == players[0])
                        {
                            return players[1];
                        }
                        else
                        {
                            return players[0];
                        }
                    }
                }
            }
            return null;
        }
        public static string GetOptionsText(RoleId role, bool IsSHR = true)
        {
            string returntext = "なし";
            switch (role)
            {
                case RoleId.Jester:
                    returntext = CustomOptions.JesterIsVent.name + ":" + CustomOptions.JesterIsVent.getString()+"\n";
                    if (!IsSHR)
                    {
                        returntext += CustomOptions.JesterIsSabotage.name + ":" + CustomOptions.JesterIsSabotage.getString() + "\n";
                    }
                    returntext += CustomOptions.JesterIsWinCleartask.name + ":" + CustomOptions.JesterIsWinCleartask.getString() + "\n";
                    break;
                case RoleId.NiceNekomata:
                    returntext = CustomOptions.NiceNekomataIsChain.name + ":" + CustomOptions.NiceNekomataIsChain.getString() + "\n";
                    break;
                case RoleId.Bait:
                    returntext = CustomOptions.BaitReportTime.name + ":" + CustomOptions.BaitReportTime.getString() + "\n";
                    break;
                case RoleId.MadMate:
                    returntext = CustomOptions.MadMateIsUseVent.name + ":" + CustomOptions.MadMateIsUseVent.getString() + "\n";
                    returntext += CustomOptions.MadMateIsCheckImpostor.name + ":" + CustomOptions.MadMateIsCheckImpostor.getString() + "\n";
                    break;
                case RoleId.Egoist:
                    returntext = CustomOptions.EgoistUseVent.name + ":" + CustomOptions.EgoistUseVent.getString() + "\n";
                    returntext += CustomOptions.EgoistUseSabo.name + ":" + CustomOptions.EgoistUseSabo.getString() + "\n";
                    break;
                case RoleId.MadMayor:
                    returntext = CustomOptions.MadMayorIsUseVent.name + ":" + CustomOptions.MadMayorIsUseVent.getString() + "\n";
                    returntext += CustomOptions.MadMayorIsCheckImpostor.name + ":" + CustomOptions.MadMayorIsCheckImpostor.getString() + "\n";
                    break;
            }
            return returntext;
        }

        public static void setRole(this PlayerControl player, RoleId role)
        {
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
                case (CustomRPC.RoleId.Pursuer):
                    Roles.RoleClass.Pursuer.PursuerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Minimalist):
                    Roles.RoleClass.Minimalist.MinimalistPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Hawk):
                    Roles.RoleClass.Hawk.HawkPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Egoist):
                    Roles.RoleClass.Egoist.EgoistPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.NiceRedRidingHood):
                    Roles.RoleClass.NiceRedRidingHood.NiceRedRidingHoodPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilEraser):
                    Roles.RoleClass.EvilEraser.EvilEraserPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Workperson):
                    Roles.RoleClass.Workperson.WorkpersonPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Magaziner):
                    Roles.RoleClass.Magaziner.MagazinerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Hunter):
                    Mode.Werewolf.main.HunterPlayers.Add(player);
                    break;
                case (CustomRPC.RoleId.Mayor):
                    Roles.RoleClass.Mayor.MayorPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.truelover):
                    Roles.RoleClass.truelover.trueloverPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Technician):
                    Roles.RoleClass.Technician.TechnicianPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.SerialKiller):
                    Roles.RoleClass.SerialKiller.SerialKillerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.OverKiller):
                    Roles.RoleClass.OverKiller.OverKillerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Levelinger):
                    Roles.RoleClass.Levelinger.LevelingerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilMoving):
                    Roles.RoleClass.EvilMoving.EvilMovingPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Amnesiac):
                    Roles.RoleClass.Amnesiac.AmnesiacPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.SideKiller):
                    Roles.RoleClass.SideKiller.SideKillerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Survivor):
                    Roles.RoleClass.Survivor.SurvivorPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MadMayor):
                    Roles.RoleClass.MadMayor.MadMayorPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilMayor):
                    Roles.RoleClass.EvilMayor.EvilMayorPlayer.Add(player);
                    break;
                //ロールアド
                default:
                    SuperNewRolesPlugin.Logger.LogError("setRole: no method found for role type {role}");
                    return;
            }
            ChacheManager.ResetMyRoleChache();
        }
        private static PlayerControl ClearTarget;
        public static void ClearRole(this PlayerControl player)
        {
            static bool ClearRemove(PlayerControl p)
            {
                if (p.PlayerId == ClearTarget.PlayerId) return true;
                return false;
            }
            ClearTarget = player;
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
                case (CustomRPC.RoleId.Pursuer):
                    Roles.RoleClass.Pursuer.PursuerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Minimalist):
                    Roles.RoleClass.Minimalist.MinimalistPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Hawk):
                    Roles.RoleClass.Hawk.HawkPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Egoist):
                    Roles.RoleClass.Egoist.EgoistPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.NiceRedRidingHood):
                    Roles.RoleClass.NiceRedRidingHood.NiceRedRidingHoodPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilEraser):
                    Roles.RoleClass.EvilEraser.EvilEraserPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Workperson):
                    Roles.RoleClass.Workperson.WorkpersonPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Magaziner):
                    Roles.RoleClass.Magaziner.MagazinerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Mayor):
                    Roles.RoleClass.Mayor.MayorPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.truelover):
                    Roles.RoleClass.truelover.trueloverPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Technician):
                    Roles.RoleClass.Technician.TechnicianPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.SerialKiller):
                    Roles.RoleClass.SerialKiller.SerialKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.OverKiller):
                    Roles.RoleClass.OverKiller.OverKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Levelinger):
                    Roles.RoleClass.Levelinger.LevelingerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilMoving):
                    Roles.RoleClass.EvilMoving.EvilMovingPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Amnesiac):
                    Roles.RoleClass.Amnesiac.AmnesiacPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.SideKiller):
                    Roles.RoleClass.SideKiller.SideKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Survivor):
                    Roles.RoleClass.Survivor.SurvivorPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadMayor):
                    Roles.RoleClass.MadMayor.MadMayorPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilMayor):
                    Roles.RoleClass.EvilMayor.EvilMayorPlayer.RemoveAll(ClearRemove);
                    break;
                    //ロールリモベ
            }
            ChacheManager.ResetMyRoleChache();
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
                case (RoleId.Egoist):
                    IsTaskClear = true;
                    break; 
                case (RoleId.Workperson):
                    IsTaskClear = true;
                    break;
                case (RoleId.truelover):
                    IsTaskClear = true;
                    break; 
                case (RoleId.Amnesiac):
                    IsTaskClear = true;
                    break;
                case (RoleId.MadMayor):
                    IsTaskClear = true;
                    break;
                    //タスククリアか
            }
            if (!IsTaskClear && ModeHandler.isMode(ModeId.SuperHostRoles) && player.isRole(RoleId.Sheriff))
            {
                IsTaskClear = true;
            }
            if (!IsTaskClear && player.IsQuarreled())
            {
                IsTaskClear = true;
            }
            if (!IsTaskClear && !RoleClass.Lovers.AliveTaskCount && player.IsLovers())
            {
                IsTaskClear = true;
            }
            return IsTaskClear;
        }
        public static bool IsUseVent(this PlayerControl player)
        {
            if (!RoleClass.Minimalist.UseVent && player.isRole(RoleId.Minimalist)) return false;
            if (player.Data.Role.IsImpostor) return true;
            if (RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(player) && Roles.RoleClass.Jester.IsUseVent) return true;
            if (RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(player) && Roles.RoleClass.MadMate.IsUseVent) return true;
            if ((RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(player) || 
                RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(player)) && Roles.RoleClass.Jackal.IsUseVent) return true;
            if (player.isRole(RoleId.JackalFriends) && RoleClass.JackalFriends.IsUseVent) return true;
            if (player.isRole(RoleId.Egoist) && RoleClass.Egoist.UseVent) return true;
            if (player.isRole(RoleId.Technician) && IsSabotage()) return true;
            if (RoleClass.MadMayor.MadMayorPlayer.IsCheckListPlayerControl(player) && Roles.RoleClass.MadMayor.IsUseVent) return true;
            return false;
        }
        public static bool IsSabotage()
        {
            try
            {
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
                        return true;
            }
            catch { }
            return false;
        }
        public static bool IsUseSabo(this PlayerControl player)
        {
            if (!RoleClass.Minimalist.UseSabo && player.isRole(RoleId.Minimalist)) return false;
            if (player.Data.Role.IsImpostor) return true;
            if (Roles.RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(player) && Roles.RoleClass.Jester.IsUseSabo && !ModeHandler.isMode(ModeId.SuperHostRoles)) return true;
            if ((RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(player) ||
                RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(player)) && Roles.RoleClass.Jackal.IsUseSabo) return true;
            if (player.isRole(RoleId.Egoist) && RoleClass.Egoist.UseSabo) return true;
            return false;
        }
        public static bool IsImpostorLight(this PlayerControl player)
        {
            if (player.isRole(RoleId.Egoist) && RoleClass.Egoist.ImpostorLight) return true;
            if (ModeHandler.isMode(ModeId.SuperHostRoles)) return false;
            if (player.isRole(RoleId.MadMate) && RoleClass.MadMate.IsImpostorLight) return true;
            if (player.isRole(RoleId.MadMayor) && RoleClass.MadMayor.IsImpostorLight) return true;
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
                case (RoleId.Egoist):
                    IsNeutral = true;
                    break;
                case (RoleId.Workperson):
                    IsNeutral = true;
                    break;
                case (RoleId.truelover):
                    IsNeutral = true;
                    break;
                case (RoleId.Amnesiac):
                    IsNeutral = true;
                    break;
                //第三か
            }
            return IsNeutral;
        }
        public static bool isRole(this PlayerControl p,RoleId role,bool IsChache = true) {
            RoleId MyRole;
            if (IsChache)
            {
                try
                {
                   MyRole = ChacheManager.MyRoleChache[p.PlayerId];
                }
                catch
                {
                    MyRole = RoleId.DefaultRole;
                }
            } else
            {
                MyRole = p.getRole(false);
            }
            if ( MyRole == role)
            {
                return true;
            }
            else {
                return false;
            }
            return false;
        }
        public static float getCoolTime(PlayerControl __instance)
        {
            float addition = PlayerControl.GameOptions.killCooldown;
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (__instance.isRole(RoleId.SerialKiller)) addition = RoleClass.SerialKiller.KillTime;
                else if (__instance.isRole(RoleId.OverKiller)) addition = RoleClass.OverKiller.KillCoolTime;
                else if (__instance.isRole(RoleId.SideKiller)) addition = RoleClass.SideKiller.KillCoolTime;
                else if (__instance.isRole(RoleId.MadKiller)) addition = RoleClass.SideKiller.MadKillerCoolTime;
                else if (__instance.isRole(RoleId.Minimalist)) addition = RoleClass.Minimalist.KillCoolTime;
                else if (__instance.isRole(RoleId.Survivor)) addition = RoleClass.Survivor.KillCoolTime;
            }
            return addition;
        }
        public static float GetEndMeetingKillCoolTime(PlayerControl p)
        {
            var role = p.getRole();
            switch (role)
            {
                case RoleId.Minimalist:
                case RoleId.Survivor:
                case RoleId.SideKiller:
                case RoleId.MadKiller:
                case RoleId.OverKiller:
                case RoleId.SerialKiller:
                    return getCoolTime(p);
            }
            return PlayerControl.GameOptions.killCooldown;
        }
        public static RoleId getRole(this PlayerControl player,bool IsChache = true)
        {
            if (IsChache)
            {
                try
                {
                    return ChacheManager.MyRoleChache[player.PlayerId];
                }
                catch
                {
                    return RoleId.DefaultRole;
                }
            }
            try
            {
                if (SuperNewRoles.Roles.RoleClass.SoothSayer.SoothSayerPlayer.IsCheckListPlayerControl(player))
                {
                    return SuperNewRoles.CustomRPC.RoleId.SoothSayer;
                }
                else if (Roles.RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Jester;
                }
                else if (Roles.RoleClass.Lighter.LighterPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Lighter;
                }
                else if (Roles.RoleClass.EvilLighter.EvilLighterPlayer.IsCheckListPlayerControl(player))
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
                else if (Roles.RoleClass.Pursuer.PursuerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Pursuer;
                }
                else if (Roles.RoleClass.Minimalist.MinimalistPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Minimalist;
                }
                else if (Roles.RoleClass.Hawk.HawkPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Hawk;
                }
                else if (Roles.RoleClass.Egoist.EgoistPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Egoist;
                }
                else if (Roles.RoleClass.NiceRedRidingHood.NiceRedRidingHoodPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.NiceRedRidingHood;
                }
                else if (Roles.RoleClass.EvilEraser.EvilEraserPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.EvilEraser;
                }
                else if (Roles.RoleClass.Workperson.WorkpersonPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Workperson;
                }
                else if (Roles.RoleClass.Magaziner.MagazinerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Magaziner;
                }
                else if (Roles.RoleClass.Mayor.MayorPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Mayor;
                }
                else if (Roles.RoleClass.truelover.trueloverPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.truelover;
                }
                else if (Roles.RoleClass.Technician.TechnicianPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Technician;
                }
                else if (Roles.RoleClass.SerialKiller.SerialKillerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.SerialKiller;
                }
                else if (Roles.RoleClass.OverKiller.OverKillerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.OverKiller;
                }
                else if (Roles.RoleClass.Levelinger.LevelingerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Levelinger;
                }
                else if (Roles.RoleClass.EvilMoving.EvilMovingPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.EvilMoving;
                }
                else if (Roles.RoleClass.Amnesiac.AmnesiacPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Amnesiac;
                }
                else if (Roles.RoleClass.SideKiller.SideKillerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.SideKiller;
                }
                else if (Roles.RoleClass.SideKiller.MadKillerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadKiller;
                }
                else if (Roles.RoleClass.Survivor.SurvivorPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Survivor;
                }
                else if (Roles.RoleClass.MadMayor.MadMayorPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadMayor;
                }
                else if (Roles.RoleClass.EvilMayor.EvilMayorPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.EvilMayor;
                }
                //ロールチェック

            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("エラー:" + e);
                return RoleId.DefaultRole;
            }
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
