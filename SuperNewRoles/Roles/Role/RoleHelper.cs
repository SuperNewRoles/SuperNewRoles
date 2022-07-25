﻿using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;

namespace SuperNewRoles
{
    public enum TeamRoleType
    {
        Crewmate,
        Impostor,
        Neutral,
        Error
    }
    public static class RoleHelpers
    {
        public static bool IsCrew(this PlayerControl player)
        {
            return player != null && !player.IsImpostor() && !player.IsNeutral();
        }

        public static bool IsImpostor(this PlayerControl player)
        {
            return !player.IsRole(RoleId.Sheriff, RoleId.Sheriff) && player != null && player.Data.Role.IsImpostor;
        }

        public static bool IsHauntedWolf(this PlayerControl player)
        {
            return player.IsRole(RoleId.HauntedWolf);
        }

        //We are Mad!
        public static bool IsMadRoles(this PlayerControl player)
        {
            RoleId role = player.GetRole();
            return role switch
            {
                RoleId.MadMate => true,
                RoleId.MadMayor => true,
                RoleId.MadStuntMan => true,
                RoleId.MadHawk => true,
                RoleId.MadJester => true,
                RoleId.MadSeer => true,
                RoleId.BlackCat => true,
                RoleId.MadMaker => true,
                //isMad
                _ => false,
            };
        }

        //We are JackalFriends!
        public static bool IsFriendRoles(this PlayerControl player)
        {
            RoleId role = player.GetRole();
            return role switch
            {
                RoleId.JackalFriends => true,
                RoleId.SeerFriends => true,
                RoleId.MayorFriends => true,
                //isFriends
                _ => false,
            };
        }

        public static bool IsQuarreled(this PlayerControl player, bool IsChache = true)
        {
            if (player.IsBot()) return false;
            if (IsChache)
            {
                try
                {
                    return ChacheManager.QuarreledChache[player.PlayerId] != null;
                }
                catch
                {
                    return false;
                }
            }
            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer)
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
        public static bool IsLovers(this PlayerControl player, bool IsChache = true)
        {
            if (player.IsBot()) return false;
            if (IsChache)
            {
                try
                {
                    return ChacheManager.LoversChache[player.PlayerId] != null;
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
        public static void SetQuarreled(PlayerControl player1, PlayerControl player2)
        {
            List<PlayerControl> sets = new() { player1, player2 };
            RoleClass.Quarreled.QuarreledPlayer.Add(sets);
            ChacheManager.ResetQuarreledChache();
        }
        public static void SetQuarreledRPC(PlayerControl player1, PlayerControl player2)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetQuarreled, SendOption.Reliable, -1);
            Writer.Write(player1.PlayerId);
            Writer.Write(player2.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static void SetLovers(PlayerControl player1, PlayerControl player2)
        {
            List<PlayerControl> sets = new() { player1, player2 };
            RoleClass.Lovers.LoversPlayer.Add(sets);
            if (player1.PlayerId == CachedPlayer.LocalPlayer.PlayerId || player2.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            }
            ChacheManager.ResetLoversChache();
        }
        public static void SetLoversRPC(PlayerControl player1, PlayerControl player2)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetLovers, SendOption.Reliable, -1);
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
        public static PlayerControl GetOneSideQuarreled(this PlayerControl player, bool IsChache = true)
        {
            if (IsChache)
            {
                return ChacheManager.QuarreledChache[player.PlayerId] ?? null;
            }
            foreach (List<PlayerControl> players in RoleClass.Quarreled.QuarreledPlayer)
            {
                foreach (PlayerControl p in players)
                {
                    if (p == player)
                    {
                        return p == players[0] ? players[1] : players[0];
                    }
                }
            }
            return null;
        }
        public static PlayerControl GetOneSideLovers(this PlayerControl player, bool IsChache = true)
        {
            if (IsChache)
            {
                return ChacheManager.LoversChache[player.PlayerId] ?? null;
            }
            foreach (List<PlayerControl> players in RoleClass.Lovers.LoversPlayer)
            {
                foreach (PlayerControl p in players)
                {
                    if (p == player)
                    {
                        return p == players[0] ? players[1] : players[0];
                    }
                }
            }
            return null;
        }
        public static bool IsJackalTeam(this PlayerControl player)
        {
            return player.GetRole() is
                RoleId.Jackal or
                RoleId.Sidekick or
                RoleId.JackalFriends or
                RoleId.SeerFriends or
                RoleId.TeleportingJackal or
                RoleId.JackalSeer or
                RoleId.SidekickSeer or
                RoleId.MayorFriends;
        }
        public static bool IsJackalTeamJackal(this PlayerControl player)
        {
            RoleId role = player.GetRole();
            return role is RoleId.Jackal or RoleId.JackalSeer or RoleId.TeleportingJackal;
        }
        public static bool IsJackalTeamSidekick(this PlayerControl player)
        {
            RoleId role = player.GetRole();
            return role is RoleId.Sidekick or RoleId.SidekickSeer;
        }

        public static void SetRole(this PlayerControl player, RoleId role)
        {
            switch (role)
            {
                case RoleId.SoothSayer:
                    RoleClass.SoothSayer.SoothSayerPlayer.Add(player);
                    break;
                case RoleId.Jester:
                    RoleClass.Jester.JesterPlayer.Add(player);
                    break;
                case RoleId.Lighter:
                    RoleClass.Lighter.LighterPlayer.Add(player);
                    break;
                case RoleId.EvilLighter:
                    RoleClass.EvilLighter.EvilLighterPlayer.Add(player);
                    break;
                case RoleId.EvilScientist:
                    RoleClass.EvilScientist.EvilScientistPlayer.Add(player);
                    break;
                case RoleId.Sheriff:
                    RoleClass.Sheriff.SheriffPlayer.Add(player);
                    break;
                case RoleId.MeetingSheriff:
                    RoleClass.MeetingSheriff.MeetingSheriffPlayer.Add(player);
                    break;
                case RoleId.Jackal:
                    RoleClass.Jackal.JackalPlayer.Add(player);
                    break;
                case RoleId.Sidekick:
                    RoleClass.Jackal.SidekickPlayer.Add(player);
                    break;
                case RoleId.Teleporter:
                    RoleClass.Teleporter.TeleporterPlayer.Add(player);
                    break;
                case RoleId.SpiritMedium:
                    RoleClass.SpiritMedium.SpiritMediumPlayer.Add(player);
                    break;
                case RoleId.SpeedBooster:
                    RoleClass.SpeedBooster.SpeedBoosterPlayer.Add(player);
                    break;
                case RoleId.EvilSpeedBooster:
                    RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.Add(player);
                    break;
                case RoleId.Tasker:
                    RoleClass.Tasker.TaskerPlayer.Add(player);
                    break;
                case RoleId.Doorr:
                    RoleClass.Doorr.DoorrPlayer.Add(player);
                    break;
                case RoleId.EvilDoorr:
                    RoleClass.EvilDoorr.EvilDoorrPlayer.Add(player);
                    break;
                case RoleId.Shielder:
                    RoleClass.Shielder.ShielderPlayer.Add(player);
                    break;
                case RoleId.Speeder:
                    RoleClass.Speeder.SpeederPlayer.Add(player);
                    break;
                case RoleId.Freezer:
                    RoleClass.Freezer.FreezerPlayer.Add(player);
                    break;
                case RoleId.Guesser:
                    RoleClass.Guesser.GuesserPlayer.Add(player);
                    break;
                case RoleId.EvilGuesser:
                    RoleClass.EvilGuesser.EvilGuesserPlayer.Add(player);
                    break;
                case RoleId.Vulture:
                    RoleClass.Vulture.VulturePlayer.Add(player);
                    break;
                case RoleId.NiceScientist:
                    RoleClass.NiceScientist.NiceScientistPlayer.Add(player);
                    break;
                case RoleId.Clergyman:
                    RoleClass.Clergyman.ClergymanPlayer.Add(player);
                    break;
                case RoleId.MadMate:
                    RoleClass.MadMate.MadMatePlayer.Add(player);
                    break;
                case RoleId.Bait:
                    RoleClass.Bait.BaitPlayer.Add(player);
                    break;
                case RoleId.HomeSecurityGuard:
                    RoleClass.HomeSecurityGuard.HomeSecurityGuardPlayer.Add(player);
                    break;
                case RoleId.StuntMan:
                    RoleClass.StuntMan.StuntManPlayer.Add(player);
                    break;
                case RoleId.Moving:
                    RoleClass.Moving.MovingPlayer.Add(player);
                    break;
                case RoleId.Opportunist:
                    RoleClass.Opportunist.OpportunistPlayer.Add(player);
                    break;
                case RoleId.NiceGambler:
                    RoleClass.NiceGambler.NiceGamblerPlayer.Add(player);
                    break;
                case RoleId.EvilGambler:
                    RoleClass.EvilGambler.EvilGamblerPlayer.Add(player);
                    break;
                case RoleId.Bestfalsecharge:
                    RoleClass.Bestfalsecharge.BestfalsechargePlayer.Add(player);
                    break;
                case RoleId.Researcher:
                    RoleClass.Researcher.ResearcherPlayer.Add(player);
                    break;
                case RoleId.SelfBomber:
                    RoleClass.SelfBomber.SelfBomberPlayer.Add(player);
                    break;
                case RoleId.God:
                    RoleClass.God.GodPlayer.Add(player);
                    break;
                case RoleId.AllCleaner:
                    RoleClass.AllCleaner.AllCleanerPlayer.Add(player);
                    break;
                case RoleId.NiceNekomata:
                    RoleClass.NiceNekomata.NiceNekomataPlayer.Add(player);
                    break;
                case RoleId.EvilNekomata:
                    RoleClass.EvilNekomata.EvilNekomataPlayer.Add(player);
                    break;
                case RoleId.JackalFriends:
                    RoleClass.JackalFriends.JackalFriendsPlayer.Add(player);
                    break;
                case RoleId.Doctor:
                    RoleClass.Doctor.DoctorPlayer.Add(player);
                    break;
                case RoleId.CountChanger:
                    RoleClass.CountChanger.CountChangerPlayer.Add(player);
                    break;
                case RoleId.Pursuer:
                    RoleClass.Pursuer.PursuerPlayer.Add(player);
                    break;
                case RoleId.Minimalist:
                    RoleClass.Minimalist.MinimalistPlayer.Add(player);
                    break;
                case RoleId.Hawk:
                    RoleClass.Hawk.HawkPlayer.Add(player);
                    break;
                case RoleId.Egoist:
                    RoleClass.Egoist.EgoistPlayer.Add(player);
                    break;
                case RoleId.NiceRedRidingHood:
                    RoleClass.NiceRedRidingHood.NiceRedRidingHoodPlayer.Add(player);
                    break;
                case RoleId.EvilEraser:
                    RoleClass.EvilEraser.EvilEraserPlayer.Add(player);
                    break;
                case RoleId.Workperson:
                    RoleClass.Workperson.WorkpersonPlayer.Add(player);
                    break;
                case RoleId.Magaziner:
                    RoleClass.Magaziner.MagazinerPlayer.Add(player);
                    break;
                case RoleId.Hunter:
                    Mode.Werewolf.Main.HunterPlayers.Add(player);
                    break;
                case RoleId.Mayor:
                    RoleClass.Mayor.MayorPlayer.Add(player);
                    break;
                case RoleId.truelover:
                    RoleClass.Truelover.trueloverPlayer.Add(player);
                    break;
                case RoleId.Technician:
                    RoleClass.Technician.TechnicianPlayer.Add(player);
                    break;
                case RoleId.SerialKiller:
                    RoleClass.SerialKiller.SerialKillerPlayer.Add(player);
                    break;
                case RoleId.OverKiller:
                    RoleClass.OverKiller.OverKillerPlayer.Add(player);
                    break;
                case RoleId.Levelinger:
                    RoleClass.Levelinger.LevelingerPlayer.Add(player);
                    break;
                case RoleId.EvilMoving:
                    RoleClass.EvilMoving.EvilMovingPlayer.Add(player);
                    break;
                case RoleId.Amnesiac:
                    RoleClass.Amnesiac.AmnesiacPlayer.Add(player);
                    break;
                case RoleId.SideKiller:
                    RoleClass.SideKiller.SideKillerPlayer.Add(player);
                    break;
                case RoleId.Survivor:
                    RoleClass.Survivor.SurvivorPlayer.Add(player);
                    break;
                case RoleId.MadMayor:
                    RoleClass.MadMayor.MadMayorPlayer.Add(player);
                    break;
                case RoleId.MadStuntMan:
                    RoleClass.MadStuntMan.MadStuntManPlayer.Add(player);
                    break;
                case RoleId.NiceHawk:
                    RoleClass.NiceHawk.NiceHawkPlayer.Add(player);
                    break;
                case RoleId.Bakery:
                    RoleClass.Bakery.BakeryPlayer.Add(player);
                    break;
                case RoleId.MadJester:
                    RoleClass.MadJester.MadJesterPlayer.Add(player);
                    break;
                case RoleId.MadHawk:
                    RoleClass.MadHawk.MadHawkPlayer.Add(player);
                    break;
                case RoleId.FalseCharges:
                    RoleClass.FalseCharges.FalseChargesPlayer.Add(player);
                    break;
                case RoleId.NiceTeleporter:
                    RoleClass.NiceTeleporter.NiceTeleporterPlayer.Add(player);
                    break;
                case RoleId.Celebrity:
                    RoleClass.Celebrity.CelebrityPlayer.Add(player);
                    RoleClass.Celebrity.ViewPlayers.Add(player);
                    break;
                case RoleId.Nocturnality:
                    RoleClass.Nocturnality.NocturnalityPlayer.Add(player);
                    break;
                case RoleId.Observer:
                    RoleClass.Observer.ObserverPlayer.Add(player);
                    break;
                case RoleId.Vampire:
                    RoleClass.Vampire.VampirePlayer.Add(player);
                    break;
                case RoleId.Fox:
                    RoleClass.Fox.FoxPlayer.Add(player);
                    break;
                case RoleId.DarkKiller:
                    RoleClass.DarkKiller.DarkKillerPlayer.Add(player);
                    break;
                case RoleId.Seer:
                    RoleClass.Seer.SeerPlayer.Add(player);
                    break;
                case RoleId.MadSeer:
                    RoleClass.MadSeer.MadSeerPlayer.Add(player);
                    break;
                case RoleId.EvilSeer:
                    RoleClass.EvilSeer.EvilSeerPlayer.Add(player);
                    break;
                case RoleId.RemoteSheriff:
                    RoleClass.RemoteSheriff.RemoteSheriffPlayer.Add(player);
                    break;
                case RoleId.TeleportingJackal:
                    RoleClass.TeleportingJackal.TeleportingJackalPlayer.Add(player);
                    break;
                case RoleId.MadMaker:
                    RoleClass.MadMaker.MadMakerPlayer.Add(player);
                    break;
                case RoleId.Demon:
                    RoleClass.Demon.DemonPlayer.Add(player);
                    break;
                case RoleId.TaskManager:
                    RoleClass.TaskManager.TaskManagerPlayer.Add(player);
                    break;
                case RoleId.SeerFriends:
                    RoleClass.SeerFriends.SeerFriendsPlayer.Add(player);
                    break;
                case RoleId.JackalSeer:
                    RoleClass.JackalSeer.JackalSeerPlayer.Add(player);
                    break;
                case RoleId.SidekickSeer:
                    RoleClass.JackalSeer.SidekickSeerPlayer.Add(player);
                    break;
                case RoleId.Assassin:
                    RoleClass.Assassin.AssassinPlayer.Add(player);
                    break;
                case RoleId.Marine:
                    RoleClass.Marine.MarinePlayer.Add(player);
                    break;
                case RoleId.Arsonist:
                    RoleClass.Arsonist.ArsonistPlayer.Add(player);
                    break;
                case RoleId.Chief:
                    RoleClass.Chief.ChiefPlayer.Add(player);
                    break;
                case RoleId.Cleaner:
                    RoleClass.Cleaner.CleanerPlayer.Add(player);
                    break;
                case RoleId.MadCleaner:
                    RoleClass.MadCleaner.MadCleanerPlayer.Add(player);
                    break;
                case RoleId.Samurai:
                    RoleClass.Samurai.SamuraiPlayer.Add(player);
                    break;
                case RoleId.MayorFriends:
                    RoleClass.MayorFriends.MayorFriendsPlayer.Add(player);
                    break;
                case RoleId.VentMaker:
                    RoleClass.VentMaker.VentMakerPlayer.Add(player);
                    break;
                case RoleId.GhostMechanic:
                    RoleClass.GhostMechanic.GhostMechanicPlayer.Add(player);
                    break;
                case RoleId.EvilHacker:
                    RoleClass.EvilHacker.EvilHackerPlayer.Add(player);
                    break;
                case RoleId.HauntedWolf:
                    RoleClass.HauntedWolf.HauntedWolfPlayer.Add(player);
                    break;
                case RoleId.PositionSwapper:
                    RoleClass.PositionSwapper.PositionSwapperPlayer.Add(player);
                    break;
                case RoleId.Tuna:
                    RoleClass.Tuna.TunaPlayer.Add(player);
                    break;
                case RoleId.Mafia:
                    RoleClass.Mafia.MafiaPlayer.Add(player);
                    break;
                case RoleId.BlackCat:
                    RoleClass.BlackCat.BlackCatPlayer.Add(player);
                    break;
                case RoleId.SecretlyKiller:
                    RoleClass.SecretlyKiller.SecretlyKillerPlayer.Add(player);
                    break;
                case RoleId.Spy:
                    RoleClass.Spy.SpyPlayer.Add(player);
                    break;
                case RoleId.Kunoichi:
                    RoleClass.Kunoichi.KunoichiPlayer.Add(player);
                    break;
                case RoleId.DoubleKiller:
                    RoleClass.DoubleKiller.DoubleKillerPlayer.Add(player);
                    break;
                case RoleId.Smasher:
                    RoleClass.Smasher.SmasherPlayer.Add(player);
                    break;
                case RoleId.SuicideWisher:
                    RoleClass.SuicideWisher.SuicideWisherPlayer.Add(player);
                    break;
                case RoleId.Neet:
                    RoleClass.Neet.NeetPlayer.Add(player);
                    break;
                case RoleId.FastMaker:
                    RoleClass.FastMaker.FastMakerPlayer.Add(player);
                    break;
                case RoleId.ToiletFan:
                    RoleClass.ToiletFan.ToiletFanPlayer.Add(player);
                    break;
                case RoleId.AllOpener:
                    RoleClass.AllOpener.AllOpenerPlayer.Add(player);
                    break;
                case RoleId.EvilButtoner:
                    RoleClass.EvilButtoner.EvilButtonerPlayer.Add(player);
                    break;
                case RoleId.NiceButtoner:
                    RoleClass.NiceButtoner.NiceButtonerPlayer.Add(player);
                    break;
                case RoleId.Finder:
                    RoleClass.Finder.FinderPlayer.Add(player);
                    break;
                case RoleId.Revolutionist:
                    RoleClass.Revolutionist.RevolutionistPlayer.Add(player);
                    break;
                case RoleId.Dictator:
                    RoleClass.Dictator.DictatorPlayer.Add(player);
                    break;
                //ロールアド
                default:
                    SuperNewRolesPlugin.Logger.LogError($"[SetRole]:No Method Found for Role Type {role}");
                    return;
            }
            bool flag = player.GetRole() != role && player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;
            if (role.IsGhostRole())
            {
                ChacheManager.ResetMyGhostRoleChache();
            }
            else
            {
                ChacheManager.ResetMyRoleChache();
            }
            if (flag)
            {
                PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            }
            SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " >= " + role);
        }
        private static PlayerControl ClearTarget;
        public static void ClearRole(this PlayerControl player)
        {
            static bool ClearRemove(PlayerControl p)
            {
                return p.PlayerId == ClearTarget.PlayerId;
            }
            ClearTarget = player;
            switch (player.GetRole())
            {
                case RoleId.SoothSayer:
                    RoleClass.SoothSayer.SoothSayerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Jester:
                    RoleClass.Jester.JesterPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Lighter:
                    RoleClass.Lighter.LighterPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilLighter:
                    RoleClass.EvilLighter.EvilLighterPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilScientist:
                    RoleClass.EvilScientist.EvilScientistPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Sheriff:
                    RoleClass.Sheriff.SheriffPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MeetingSheriff:
                    RoleClass.MeetingSheriff.MeetingSheriffPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Jackal:
                    RoleClass.Jackal.JackalPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Sidekick:
                    RoleClass.Jackal.SidekickPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Teleporter:
                    RoleClass.Teleporter.TeleporterPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.SpiritMedium:
                    RoleClass.SpiritMedium.SpiritMediumPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.SpeedBooster:
                    RoleClass.SpeedBooster.SpeedBoosterPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilSpeedBooster:
                    RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Tasker:
                    RoleClass.Tasker.TaskerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Doorr:
                    RoleClass.Doorr.DoorrPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilDoorr:
                    RoleClass.EvilDoorr.EvilDoorrPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Shielder:
                    RoleClass.Shielder.ShielderPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Speeder:
                    RoleClass.Speeder.SpeederPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Freezer:
                    RoleClass.Freezer.FreezerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Guesser:
                    RoleClass.Guesser.GuesserPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilGuesser:
                    RoleClass.EvilGuesser.EvilGuesserPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Vulture:
                    RoleClass.Vulture.VulturePlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.NiceScientist:
                    RoleClass.NiceScientist.NiceScientistPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Clergyman:
                    RoleClass.Clergyman.ClergymanPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadMate:
                    RoleClass.MadMate.MadMatePlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Bait:
                    RoleClass.Bait.BaitPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.HomeSecurityGuard:
                    RoleClass.HomeSecurityGuard.HomeSecurityGuardPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.StuntMan:
                    RoleClass.StuntMan.StuntManPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Moving:
                    RoleClass.Moving.MovingPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Opportunist:
                    RoleClass.Opportunist.OpportunistPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.NiceGambler:
                    RoleClass.NiceGambler.NiceGamblerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilGambler:
                    RoleClass.EvilGambler.EvilGamblerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Bestfalsecharge:
                    RoleClass.Bestfalsecharge.BestfalsechargePlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Researcher:
                    RoleClass.Researcher.ResearcherPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.SelfBomber:
                    RoleClass.SelfBomber.SelfBomberPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.God:
                    RoleClass.God.GodPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.AllCleaner:
                    RoleClass.AllCleaner.AllCleanerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.NiceNekomata:
                    RoleClass.NiceNekomata.NiceNekomataPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilNekomata:
                    RoleClass.EvilNekomata.EvilNekomataPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.JackalFriends:
                    RoleClass.JackalFriends.JackalFriendsPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Doctor:
                    RoleClass.Doctor.DoctorPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.CountChanger:
                    RoleClass.CountChanger.CountChangerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Pursuer:
                    RoleClass.Pursuer.PursuerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Minimalist:
                    RoleClass.Minimalist.MinimalistPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Hawk:
                    RoleClass.Hawk.HawkPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Egoist:
                    RoleClass.Egoist.EgoistPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.NiceRedRidingHood:
                    RoleClass.NiceRedRidingHood.NiceRedRidingHoodPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilEraser:
                    RoleClass.EvilEraser.EvilEraserPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Workperson:
                    RoleClass.Workperson.WorkpersonPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Magaziner:
                    RoleClass.Magaziner.MagazinerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Mayor:
                    RoleClass.Mayor.MayorPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.truelover:
                    RoleClass.Truelover.trueloverPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Technician:
                    RoleClass.Technician.TechnicianPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.SerialKiller:
                    RoleClass.SerialKiller.SerialKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.OverKiller:
                    RoleClass.OverKiller.OverKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Levelinger:
                    RoleClass.Levelinger.LevelingerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilMoving:
                    RoleClass.EvilMoving.EvilMovingPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Amnesiac:
                    RoleClass.Amnesiac.AmnesiacPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.SideKiller:
                    RoleClass.SideKiller.SideKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadKiller:
                    RoleClass.SideKiller.MadKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Survivor:
                    RoleClass.Survivor.SurvivorPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadMayor:
                    RoleClass.MadMayor.MadMayorPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadStuntMan:
                    RoleClass.MadStuntMan.MadStuntManPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadHawk:
                    RoleClass.MadHawk.MadHawkPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.NiceHawk:
                    RoleClass.NiceHawk.NiceHawkPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Bakery:
                    RoleClass.Bakery.BakeryPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadJester:
                    RoleClass.MadJester.MadJesterPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.FalseCharges:
                    RoleClass.FalseCharges.FalseChargesPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.NiceTeleporter:
                    RoleClass.NiceTeleporter.NiceTeleporterPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Celebrity:
                    RoleClass.Celebrity.CelebrityPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Nocturnality:
                    RoleClass.Nocturnality.NocturnalityPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Observer:
                    RoleClass.Observer.ObserverPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Vampire:
                    RoleClass.Vampire.VampirePlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Fox:
                    RoleClass.Fox.FoxPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.DarkKiller:
                    RoleClass.DarkKiller.DarkKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Seer:
                    RoleClass.Seer.SeerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadSeer:
                    RoleClass.MadSeer.MadSeerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilSeer:
                    RoleClass.EvilSeer.EvilSeerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.TeleportingJackal:
                    RoleClass.TeleportingJackal.TeleportingJackalPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.RemoteSheriff:
                    RoleClass.RemoteSheriff.RemoteSheriffPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadMaker:
                    RoleClass.MadMaker.MadMakerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Demon:
                    RoleClass.Demon.DemonPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.TaskManager:
                    RoleClass.TaskManager.TaskManagerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.SeerFriends:
                    RoleClass.SeerFriends.SeerFriendsPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.JackalSeer:
                    RoleClass.JackalSeer.JackalSeerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.SidekickSeer:
                    RoleClass.JackalSeer.SidekickSeerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Assassin:
                    RoleClass.Assassin.AssassinPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Marine:
                    RoleClass.Marine.MarinePlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Arsonist:
                    RoleClass.Arsonist.ArsonistPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Chief:
                    RoleClass.Chief.ChiefPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Cleaner:
                    RoleClass.Cleaner.CleanerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MadCleaner:
                    RoleClass.MadCleaner.MadCleanerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Samurai:
                    RoleClass.Samurai.SamuraiPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.MayorFriends:
                    RoleClass.MayorFriends.MayorFriendsPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.VentMaker:
                    RoleClass.VentMaker.VentMakerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.GhostMechanic:
                    RoleClass.GhostMechanic.GhostMechanicPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilHacker:
                    RoleClass.EvilHacker.EvilHackerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.HauntedWolf:
                    RoleClass.HauntedWolf.HauntedWolfPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.PositionSwapper:
                    RoleClass.PositionSwapper.PositionSwapperPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Tuna:
                    RoleClass.Tuna.TunaPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Mafia:
                    RoleClass.Mafia.MafiaPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.BlackCat:
                    RoleClass.BlackCat.BlackCatPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Spy:
                    RoleClass.Spy.SpyPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.DoubleKiller:
                    RoleClass.DoubleKiller.DoubleKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Smasher:
                    RoleClass.Smasher.SmasherPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.SuicideWisher:
                    RoleClass.SuicideWisher.SuicideWisherPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Neet:
                    RoleClass.Neet.NeetPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.FastMaker:
                    RoleClass.FastMaker.FastMakerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.ToiletFan:
                    RoleClass.ToiletFan.ToiletFanPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.AllOpener:
                    RoleClass.AllOpener.AllOpenerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.EvilButtoner:
                    RoleClass.EvilButtoner.EvilButtonerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.NiceButtoner:
                    RoleClass.NiceButtoner.NiceButtonerPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Finder:
                    RoleClass.Finder.FinderPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Revolutionist:
                    RoleClass.Revolutionist.RevolutionistPlayer.RemoveAll(ClearRemove);
                    break;
                case RoleId.Dictator:
                    RoleClass.Dictator.DictatorPlayer.RemoveAll(ClearRemove);
                    break;
                    //ロールリモベ
            }
            ChacheManager.ResetMyRoleChache();
        }
        public static void SetRoleRPC(this PlayerControl Player, RoleId SelectRoleDate)
        {
            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetRole, SendOption.Reliable, -1);
            killWriter.Write(Player.PlayerId);
            killWriter.Write((byte)SelectRoleDate);
            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
            RPCProcedure.SetRole(Player.PlayerId, (byte)SelectRoleDate);
        }
        public static bool IsClearTask(this PlayerControl player)
        {
            var IsTaskClear = false;
            switch (player.GetRole())
            {
                case RoleId.Jester:
                case RoleId.Jackal:
                case RoleId.Sidekick:
                case RoleId.Vulture:
                case RoleId.HomeSecurityGuard:
                case RoleId.MadMate:
                case RoleId.JackalFriends:
                case RoleId.Opportunist:
                case RoleId.Researcher:
                case RoleId.God:
                case RoleId.Egoist:
                case RoleId.Workperson:
                case RoleId.truelover:
                case RoleId.Amnesiac:
                case RoleId.MadMayor:
                case RoleId.MadStuntMan:
                case RoleId.MadKiller:
                case RoleId.MadHawk:
                case RoleId.MadJester:
                case RoleId.FalseCharges:
                case RoleId.Fox:
                case RoleId.TeleportingJackal:
                case RoleId.Demon:
                case RoleId.SeerFriends:
                case RoleId.Arsonist:
                case RoleId.JackalSeer:
                case RoleId.SidekickSeer:
                case RoleId.MadMaker:
                case RoleId.MadCleaner:
                case RoleId.MayorFriends:
                case RoleId.Tuna:
                case RoleId.BlackCat:
                case RoleId.Neet:
                case RoleId.Revolutionist:
                    IsTaskClear = true;
                    break;
                    //タスククリアか
            }
            if (!IsTaskClear
                && ((ModeHandler.IsMode(ModeId.SuperHostRoles) &&
                player.IsRole(RoleId.Sheriff, RoleId.RemoteSheriff, RoleId.ToiletFan, RoleId.NiceButtoner, RoleId.AllOpener))
                || player.IsQuarreled()
                || (!RoleClass.Lovers.AliveTaskCount && player.IsLovers())
                || player.IsImpostor()))
            {
                IsTaskClear = true;
            }
            return IsTaskClear;
        }
        public static bool IsUseVent(this PlayerControl player)
        {
            RoleId role = player.GetRole();
            if (player.IsImpostor()) return true;
            else if (ModeHandler.IsMode(ModeId.SuperHostRoles) && IsComms()) return false;
            return role switch
            {
                RoleId.Jackal or RoleId.Sidekick => RoleClass.Jackal.IsUseVent,
                RoleId.Minimalist => RoleClass.Minimalist.UseVent,
                RoleId.Samurai => RoleClass.Samurai.UseVent,
                RoleId.Jester => RoleClass.Jester.IsUseVent,
                RoleId.MadMate => CachedPlayer.LocalPlayer.Data.Role.Role != RoleTypes.GuardianAngel && RoleClass.MadMate.IsUseVent,
                RoleId.TeleportingJackal => RoleClass.TeleportingJackal.IsUseVent,
                RoleId.JackalFriends => RoleClass.JackalFriends.IsUseVent,
                RoleId.Egoist => RoleClass.Egoist.UseVent,
                RoleId.Technician => IsSabotage(),
                RoleId.MadMayor => RoleClass.MadMayor.IsUseVent,
                RoleId.MadJester => RoleClass.MadJester.IsUseVent,
                RoleId.MadStuntMan => RoleClass.MadStuntMan.IsUseVent,
                RoleId.MadHawk => RoleClass.MadHawk.IsUseVent,
                RoleId.MadSeer => RoleClass.MadSeer.IsUseVent,
                RoleId.MadMaker => RoleClass.MadMaker.IsUseVent,
                RoleId.Fox => RoleClass.Fox.IsUseVent,
                RoleId.Demon => RoleClass.Demon.IsUseVent,
                RoleId.SeerFriends => RoleClass.SeerFriends.IsUseVent,
                RoleId.JackalSeer or RoleId.SidekickSeer => RoleClass.JackalSeer.IsUseVent,
                RoleId.MadCleaner => RoleClass.MadCleaner.IsUseVent,
                RoleId.Arsonist => RoleClass.Arsonist.IsUseVent,
                RoleId.Vulture => RoleClass.Vulture.IsUseVent,
                RoleId.MayorFriends => RoleClass.MayorFriends.IsUseVent,
                RoleId.Tuna => RoleClass.Tuna.IsUseVent,
                RoleId.BlackCat => CachedPlayer.LocalPlayer.Data.Role.Role != RoleTypes.GuardianAngel && RoleClass.BlackCat.IsUseVent,
                RoleId.Spy => RoleClass.Spy.CanUseVent,
                _ => false,
            };
        }
        public static bool IsSabotage()
        {
            try
            {
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                        return true;
            }
            catch { }
            return false;
        }
        public static bool IsComms()
        {
            try
            {
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    if (task.TaskType == TaskTypes.FixComms)
                        return true;
            }
            catch { }
            return false;
        }
        public static bool IsUseSabo(this PlayerControl player)
        {
            RoleId role = player.GetRole();
            if (role == RoleId.Minimalist) return RoleClass.Minimalist.UseSabo;
            if (role == RoleId.Samurai) return RoleClass.Samurai.UseSabo;
            else if (player.IsImpostor()) return true;
            return role switch
            {
                RoleId.Jester => RoleClass.Jester.IsUseSabo,
                RoleId.Sidekick or RoleId.Jackal => RoleClass.Jackal.IsUseSabo,
                RoleId.TeleportingJackal => RoleClass.TeleportingJackal.IsUseSabo,
                RoleId.SidekickSeer or RoleId.JackalSeer => RoleClass.JackalSeer.IsUseSabo,
                RoleId.Egoist => RoleClass.Egoist.UseSabo,
                _ => false,
            };
        }
        public static bool IsImpostorLight(this PlayerControl player)
        {
            RoleId role = player.GetRole();
            return role == RoleId.Egoist
                ? RoleClass.Egoist.ImpostorLight
                : !ModeHandler.IsMode(ModeId.SuperHostRoles)
                && role switch
                {
                    RoleId.MadMate => RoleClass.MadMate.IsImpostorLight,
                    RoleId.MadMayor => RoleClass.MadMayor.IsImpostorLight,
                    RoleId.MadStuntMan => RoleClass.MadStuntMan.IsImpostorLight,
                    RoleId.MadHawk => RoleClass.MadHawk.IsImpostorLight,
                    RoleId.MadJester => RoleClass.MadJester.IsImpostorLight,
                    RoleId.MadSeer => RoleClass.MadSeer.IsImpostorLight,
                    RoleId.Fox => RoleClass.Fox.IsImpostorLight,
                    RoleId.TeleportingJackal => RoleClass.TeleportingJackal.IsImpostorLight,
                    RoleId.MadMaker => RoleClass.MadMaker.IsImpostorLight,
                    RoleId.Jackal or RoleId.Sidekick => RoleClass.Jackal.IsImpostorLight,
                    RoleId.JackalFriends => RoleClass.JackalFriends.IsImpostorLight,
                    RoleId.SeerFriends => RoleClass.SeerFriends.IsImpostorLight,
                    RoleId.JackalSeer or RoleId.SidekickSeer => RoleClass.JackalSeer.IsImpostorLight,
                    RoleId.MadCleaner => RoleClass.MadCleaner.IsImpostorLight,
                    RoleId.MayorFriends => RoleClass.MayorFriends.IsImpostorLight,
                    RoleId.BlackCat => RoleClass.BlackCat.IsImpostorLight,
                    _ => false,
                };
        }
        public static bool IsNeutral(this PlayerControl player)
        {
            var IsNeutral = false;
            switch (player.GetRole())
            {
                case RoleId.Jester:
                case RoleId.Jackal:
                case RoleId.Sidekick:
                case RoleId.Vulture:
                case RoleId.Opportunist:
                case RoleId.Researcher:
                case RoleId.God:
                case RoleId.Egoist:
                case RoleId.Workperson:
                case RoleId.truelover:
                case RoleId.Amnesiac:
                case RoleId.FalseCharges:
                case RoleId.Fox:
                case RoleId.TeleportingJackal:
                case RoleId.Demon:
                case RoleId.JackalSeer:
                case RoleId.SidekickSeer:
                case RoleId.Arsonist:
                case RoleId.MayorFriends:
                case RoleId.Tuna:
                case RoleId.Neet:
                case RoleId.Revolutionist:
                    IsNeutral = true;
                    break;
                    //第三か
            }
            return IsNeutral;
        }
        public static bool IsRole(this PlayerControl p, RoleId role, bool IsChache = true)
        {
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
            }
            else
            {
                MyRole = p.GetRole(false);
            }
            return MyRole == role;
        }
        public static bool IsRole(this PlayerControl p, params RoleId[] roles)
        {
            RoleId MyRole;
            try
            {
                MyRole = ChacheManager.MyRoleChache[p.PlayerId];
            }
            catch
            {
                MyRole = RoleId.DefaultRole;
            }
            foreach (RoleId role in roles)
            {
                if (role == MyRole) return true;
            }
            return false;
        }
        public static float GetCoolTime(PlayerControl __instance)
        {
            float addition = PlayerControl.GameOptions.killCooldown;
            if (ModeHandler.IsMode(ModeId.Default))
            {
                RoleId role = __instance.GetRole();
                switch (role)
                {
                    case RoleId.SerialKiller:
                        addition = RoleClass.SerialKiller.KillTime;
                        break;
                    case RoleId.OverKiller:
                        addition = RoleClass.OverKiller.KillCoolTime;
                        break;
                    case RoleId.SideKiller:
                        addition = RoleClass.SideKiller.KillCoolTime;
                        break;
                    case RoleId.MadKiller:
                        addition = RoleClass.SideKiller.MadKillerCoolTime;
                        break;
                    case RoleId.Minimalist:
                        addition = RoleClass.Minimalist.KillCoolTime;
                        break;
                    case RoleId.Survivor:
                        addition = RoleClass.Survivor.KillCoolTime;
                        break;
                    case RoleId.DarkKiller:
                        addition = RoleClass.DarkKiller.KillCoolTime;
                        break;
                    case RoleId.Cleaner:
                        addition = RoleClass.Cleaner.KillCoolTime;
                        break;
                    case RoleId.Samurai:
                        addition = RoleClass.Samurai.KillCoolTime;
                        break;
                    case RoleId.Kunoichi:
                        addition = RoleClass.Kunoichi.KillCoolTime;
                        break;
                }
            }
            return addition;
        }
        public static float GetEndMeetingKillCoolTime(PlayerControl p)
        {
            var role = p.GetRole();
            return GetCoolTime(p);
        }
        public static RoleId GetGhostRole(this PlayerControl player, bool IsChache = true)
        {
            if (IsChache)
            {
                try
                {
                    return ChacheManager.MyGhostRoleChache[player.PlayerId];
                }
                catch
                {
                    return RoleId.DefaultRole;
                }
            }
            try
            {
                if (RoleClass.GhostMechanic.GhostMechanicPlayer.IsCheckListPlayerControl(player)) return RoleId.GhostMechanic;
                //ここが幽霊役職
            }
            catch { }
            return RoleId.DefaultRole;
        }
        public static bool IsGhostRole(this RoleId role)
        {
            return IntroDate.GetIntroDate(role).IsGhostRole;
        }
        public static bool IsGhostRole(this PlayerControl p, RoleId role, bool IsChache = true)
        {
            RoleId MyRole;
            if (IsChache)
            {
                try
                {
                    MyRole = ChacheManager.MyGhostRoleChache[p.PlayerId];
                }
                catch
                {
                    MyRole = RoleId.DefaultRole;
                }
            }
            else
            {
                MyRole = p.GetGhostRole(false);
            }
            return MyRole == role;
        }
        public static RoleId GetRole(this PlayerControl player, bool IsChache = true)
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
                if (RoleClass.SoothSayer.SoothSayerPlayer.IsCheckListPlayerControl(player)) return RoleId.SoothSayer;
                else if (RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(player)) return RoleId.Jester;
                else if (RoleClass.Lighter.LighterPlayer.IsCheckListPlayerControl(player)) return RoleId.Lighter;
                else if (RoleClass.EvilLighter.EvilLighterPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilLighter;
                else if (RoleClass.EvilScientist.EvilScientistPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilScientist;
                else if (RoleClass.Sheriff.SheriffPlayer.IsCheckListPlayerControl(player)) return RoleId.Sheriff;
                else if (RoleClass.MeetingSheriff.MeetingSheriffPlayer.IsCheckListPlayerControl(player)) return RoleId.MeetingSheriff;
                else if (RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(player)) return RoleId.Jackal;
                else if (RoleClass.Jackal.SidekickPlayer.IsCheckListPlayerControl(player)) return RoleId.Sidekick;
                else if (RoleClass.Teleporter.TeleporterPlayer.IsCheckListPlayerControl(player)) return RoleId.Teleporter;
                else if (RoleClass.SpiritMedium.SpiritMediumPlayer.IsCheckListPlayerControl(player)) return RoleId.SpiritMedium;
                else if (RoleClass.SpeedBooster.SpeedBoosterPlayer.IsCheckListPlayerControl(player)) return RoleId.SpeedBooster;
                else if (RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilSpeedBooster;
                else if (RoleClass.Tasker.TaskerPlayer.IsCheckListPlayerControl(player)) return RoleId.Tasker;
                else if (RoleClass.Doorr.DoorrPlayer.IsCheckListPlayerControl(player)) return RoleId.Doorr;
                else if (RoleClass.EvilDoorr.EvilDoorrPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilDoorr;
                else if (RoleClass.Shielder.ShielderPlayer.IsCheckListPlayerControl(player)) return RoleId.Shielder;
                else if (RoleClass.Shielder.ShielderPlayer.IsCheckListPlayerControl(player)) return RoleId.Shielder;
                else if (RoleClass.Speeder.SpeederPlayer.IsCheckListPlayerControl(player)) return RoleId.Speeder;
                else if (RoleClass.Freezer.FreezerPlayer.IsCheckListPlayerControl(player)) return RoleId.Freezer;
                else if (RoleClass.Guesser.GuesserPlayer.IsCheckListPlayerControl(player)) return RoleId.Guesser;
                else if (RoleClass.EvilGuesser.EvilGuesserPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilGuesser;
                else if (RoleClass.Vulture.VulturePlayer.IsCheckListPlayerControl(player)) return RoleId.Vulture;
                else if (RoleClass.NiceScientist.NiceScientistPlayer.IsCheckListPlayerControl(player)) return RoleId.NiceScientist;
                else if (RoleClass.Clergyman.ClergymanPlayer.IsCheckListPlayerControl(player)) return RoleId.Clergyman;
                else if (RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(player)) return RoleId.MadMate;
                else if (RoleClass.Bait.BaitPlayer.IsCheckListPlayerControl(player)) return RoleId.Bait;
                else if (RoleClass.HomeSecurityGuard.HomeSecurityGuardPlayer.IsCheckListPlayerControl(player)) return RoleId.HomeSecurityGuard;
                else if (RoleClass.StuntMan.StuntManPlayer.IsCheckListPlayerControl(player)) return RoleId.StuntMan;
                else if (RoleClass.Moving.MovingPlayer.IsCheckListPlayerControl(player)) return RoleId.Moving;
                else if (RoleClass.Opportunist.OpportunistPlayer.IsCheckListPlayerControl(player)) return RoleId.Opportunist;
                else if (RoleClass.NiceGambler.NiceGamblerPlayer.IsCheckListPlayerControl(player)) return RoleId.NiceGambler;
                else if (RoleClass.EvilGambler.EvilGamblerPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilGambler;
                else if (RoleClass.Bestfalsecharge.BestfalsechargePlayer.IsCheckListPlayerControl(player)) return RoleId.Bestfalsecharge;
                else if (RoleClass.Researcher.ResearcherPlayer.IsCheckListPlayerControl(player)) return RoleId.Researcher;
                else if (RoleClass.SelfBomber.SelfBomberPlayer.IsCheckListPlayerControl(player)) return RoleId.SelfBomber;
                else if (RoleClass.God.GodPlayer.IsCheckListPlayerControl(player)) return RoleId.God;
                else if (RoleClass.AllCleaner.AllCleanerPlayer.IsCheckListPlayerControl(player)) return RoleId.AllCleaner;
                else if (RoleClass.NiceNekomata.NiceNekomataPlayer.IsCheckListPlayerControl(player)) return RoleId.NiceNekomata;
                else if (RoleClass.EvilNekomata.EvilNekomataPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilNekomata;
                else if (RoleClass.JackalFriends.JackalFriendsPlayer.IsCheckListPlayerControl(player)) return RoleId.JackalFriends;
                else if (RoleClass.Doctor.DoctorPlayer.IsCheckListPlayerControl(player)) return RoleId.Doctor;
                else if (RoleClass.CountChanger.CountChangerPlayer.IsCheckListPlayerControl(player)) return RoleId.CountChanger;
                else if (RoleClass.Pursuer.PursuerPlayer.IsCheckListPlayerControl(player)) return RoleId.Pursuer;
                else if (RoleClass.Minimalist.MinimalistPlayer.IsCheckListPlayerControl(player)) return RoleId.Minimalist;
                else if (RoleClass.Hawk.HawkPlayer.IsCheckListPlayerControl(player)) return RoleId.Hawk;
                else if (RoleClass.Egoist.EgoistPlayer.IsCheckListPlayerControl(player)) return RoleId.Egoist;
                else if (RoleClass.NiceRedRidingHood.NiceRedRidingHoodPlayer.IsCheckListPlayerControl(player)) return RoleId.NiceRedRidingHood;
                else if (RoleClass.EvilEraser.EvilEraserPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilEraser;
                else if (RoleClass.Workperson.WorkpersonPlayer.IsCheckListPlayerControl(player)) return RoleId.Workperson;
                else if (RoleClass.Magaziner.MagazinerPlayer.IsCheckListPlayerControl(player)) return RoleId.Magaziner;
                else if (RoleClass.Mayor.MayorPlayer.IsCheckListPlayerControl(player)) return RoleId.Mayor;
                else if (RoleClass.Truelover.trueloverPlayer.IsCheckListPlayerControl(player)) return RoleId.truelover;
                else if (RoleClass.Technician.TechnicianPlayer.IsCheckListPlayerControl(player)) return RoleId.Technician;
                else if (RoleClass.SerialKiller.SerialKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.SerialKiller;
                else if (RoleClass.OverKiller.OverKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.OverKiller;
                else if (RoleClass.Levelinger.LevelingerPlayer.IsCheckListPlayerControl(player)) return RoleId.Levelinger;
                else if (RoleClass.EvilMoving.EvilMovingPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilMoving;
                else if (RoleClass.Amnesiac.AmnesiacPlayer.IsCheckListPlayerControl(player)) return RoleId.Amnesiac;
                else if (RoleClass.SideKiller.SideKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.SideKiller;
                else if (RoleClass.SideKiller.MadKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.MadKiller;
                else if (RoleClass.Survivor.SurvivorPlayer.IsCheckListPlayerControl(player)) return RoleId.Survivor;
                else if (RoleClass.MadMayor.MadMayorPlayer.IsCheckListPlayerControl(player)) return RoleId.MadMayor;
                else if (RoleClass.MadStuntMan.MadStuntManPlayer.IsCheckListPlayerControl(player)) return RoleId.MadStuntMan;
                else if (RoleClass.NiceHawk.NiceHawkPlayer.IsCheckListPlayerControl(player)) return RoleId.NiceHawk;
                else if (RoleClass.Bakery.BakeryPlayer.IsCheckListPlayerControl(player)) return RoleId.Bakery;
                else if (RoleClass.MadHawk.MadHawkPlayer.IsCheckListPlayerControl(player)) return RoleId.MadHawk;
                else if (RoleClass.MadJester.MadJesterPlayer.IsCheckListPlayerControl(player)) return RoleId.MadJester;
                else if (RoleClass.FalseCharges.FalseChargesPlayer.IsCheckListPlayerControl(player)) return RoleId.FalseCharges;
                else if (RoleClass.NiceTeleporter.NiceTeleporterPlayer.IsCheckListPlayerControl(player)) return RoleId.NiceTeleporter;
                else if (RoleClass.NiceTeleporter.NiceTeleporterPlayer.IsCheckListPlayerControl(player)) return RoleId.NiceTeleporter;
                else if (RoleClass.Celebrity.CelebrityPlayer.IsCheckListPlayerControl(player)) return RoleId.Celebrity;
                else if (RoleClass.Nocturnality.NocturnalityPlayer.IsCheckListPlayerControl(player)) return RoleId.Nocturnality;
                else if (RoleClass.Observer.ObserverPlayer.IsCheckListPlayerControl(player)) return RoleId.Observer;
                else if (RoleClass.Vampire.VampirePlayer.IsCheckListPlayerControl(player)) return RoleId.Vampire;
                else if (RoleClass.DarkKiller.DarkKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.DarkKiller;
                else if (RoleClass.Seer.SeerPlayer.IsCheckListPlayerControl(player)) return RoleId.Seer;
                else if (RoleClass.MadSeer.MadSeerPlayer.IsCheckListPlayerControl(player)) return RoleId.MadSeer;
                else if (RoleClass.EvilSeer.EvilSeerPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilSeer;
                else if (RoleClass.RemoteSheriff.RemoteSheriffPlayer.IsCheckListPlayerControl(player)) return RoleId.RemoteSheriff;
                else if (RoleClass.Vampire.VampirePlayer.IsCheckListPlayerControl(player)) return RoleId.Vampire;
                else if (RoleClass.DarkKiller.DarkKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.DarkKiller;
                else if (RoleClass.Fox.FoxPlayer.IsCheckListPlayerControl(player)) return RoleId.Fox;
                else if (RoleClass.TeleportingJackal.TeleportingJackalPlayer.IsCheckListPlayerControl(player)) return RoleId.TeleportingJackal;
                else if (RoleClass.MadMaker.MadMakerPlayer.IsCheckListPlayerControl(player)) return RoleId.MadMaker;
                else if (RoleClass.DarkKiller.DarkKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.DarkKiller;
                else if (RoleClass.Fox.FoxPlayer.IsCheckListPlayerControl(player)) return RoleId.Fox;
                else if (RoleClass.TeleportingJackal.TeleportingJackalPlayer.IsCheckListPlayerControl(player)) return RoleId.TeleportingJackal;
                else if (RoleClass.MadMaker.MadMakerPlayer.IsCheckListPlayerControl(player)) return RoleId.MadMaker;
                else if (RoleClass.Demon.DemonPlayer.IsCheckListPlayerControl(player)) return RoleId.Demon;
                else if (RoleClass.TaskManager.TaskManagerPlayer.IsCheckListPlayerControl(player)) return RoleId.TaskManager;
                else if (RoleClass.SeerFriends.SeerFriendsPlayer.IsCheckListPlayerControl(player)) return RoleId.SeerFriends;
                else if (RoleClass.JackalSeer.JackalSeerPlayer.IsCheckListPlayerControl(player)) return RoleId.JackalSeer;
                else if (RoleClass.JackalSeer.SidekickSeerPlayer.IsCheckListPlayerControl(player)) return RoleId.SidekickSeer;
                else if (RoleClass.Assassin.AssassinPlayer.IsCheckListPlayerControl(player)) return RoleId.Assassin;
                else if (RoleClass.Marine.MarinePlayer.IsCheckListPlayerControl(player)) return RoleId.Marine;
                else if (RoleClass.SeerFriends.SeerFriendsPlayer.IsCheckListPlayerControl(player)) return RoleId.SeerFriends;
                else if (RoleClass.Arsonist.ArsonistPlayer.IsCheckListPlayerControl(player)) return RoleId.Arsonist;
                else if (RoleClass.Chief.ChiefPlayer.IsCheckListPlayerControl(player)) return RoleId.Chief;
                else if (RoleClass.Cleaner.CleanerPlayer.IsCheckListPlayerControl(player)) return RoleId.Cleaner;
                else if (RoleClass.Samurai.SamuraiPlayer.IsCheckListPlayerControl(player)) return RoleId.Samurai;
                else if (RoleClass.MadCleaner.MadCleanerPlayer.IsCheckListPlayerControl(player)) return RoleId.MadCleaner;
                else if (RoleClass.MayorFriends.MayorFriendsPlayer.IsCheckListPlayerControl(player)) return RoleId.MayorFriends;
                else if (RoleClass.VentMaker.VentMakerPlayer.IsCheckListPlayerControl(player)) return RoleId.VentMaker;
                else if (RoleClass.EvilHacker.EvilHackerPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilHacker;
                else if (RoleClass.HauntedWolf.HauntedWolfPlayer.IsCheckListPlayerControl(player)) return RoleId.HauntedWolf;
                else if (RoleClass.PositionSwapper.PositionSwapperPlayer.IsCheckListPlayerControl(player)) return RoleId.PositionSwapper;
                else if (RoleClass.Tuna.TunaPlayer.IsCheckListPlayerControl(player)) return RoleId.Tuna;
                else if (RoleClass.Mafia.MafiaPlayer.IsCheckListPlayerControl(player)) return RoleId.Mafia;
                else if (RoleClass.BlackCat.BlackCatPlayer.IsCheckListPlayerControl(player)) return RoleId.BlackCat;
                else if (RoleClass.SecretlyKiller.SecretlyKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.SecretlyKiller;
                else if (RoleClass.Spy.SpyPlayer.IsCheckListPlayerControl(player)) return RoleId.Spy;
                else if (RoleClass.Kunoichi.KunoichiPlayer.IsCheckListPlayerControl(player)) return RoleId.Kunoichi;
                else if (RoleClass.DoubleKiller.DoubleKillerPlayer.IsCheckListPlayerControl(player)) return RoleId.DoubleKiller;
                else if (RoleClass.Smasher.SmasherPlayer.IsCheckListPlayerControl(player)) return RoleId.Smasher;
                else if (RoleClass.SuicideWisher.SuicideWisherPlayer.IsCheckListPlayerControl(player)) return RoleId.SuicideWisher;
                else if (RoleClass.Neet.NeetPlayer.IsCheckListPlayerControl(player)) return RoleId.Neet;
                else if (RoleClass.FastMaker.FastMakerPlayer.IsCheckListPlayerControl(player)) return RoleId.FastMaker;
                else if (RoleClass.ToiletFan.ToiletFanPlayer.IsCheckListPlayerControl(player)) return RoleId.ToiletFan;
                else if (RoleClass.AllOpener.AllOpenerPlayer.IsCheckListPlayerControl(player)) return RoleId.AllOpener;
                else if (RoleClass.EvilButtoner.EvilButtonerPlayer.IsCheckListPlayerControl(player)) return RoleId.EvilButtoner;
                else if (RoleClass.NiceButtoner.NiceButtonerPlayer.IsCheckListPlayerControl(player)) return RoleId.NiceButtoner;
                else if (RoleClass.Finder.FinderPlayer.IsCheckListPlayerControl(player)) return RoleId.Finder;
                else if (RoleClass.Revolutionist.RevolutionistPlayer.IsCheckListPlayerControl(player)) return RoleId.Revolutionist;
                else if (RoleClass.Dictator.DictatorPlayer.IsCheckListPlayerControl(player)) return RoleId.Dictator;
                //ロールチェック
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[RoleHelper]Error:" + e);
                return RoleId.DefaultRole;
            }
            return RoleId.DefaultRole;
        }
        public static bool IsDead(this PlayerControl player)
        {
            return player == null || player.Data.Disconnected || player.Data.IsDead;
        }
        public static bool IsAlive(this PlayerControl player)
        {
            return !IsDead(player);
        }
    }
}