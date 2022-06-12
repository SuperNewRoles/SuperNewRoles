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
using SuperNewRoles.Intro;

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
            if (player.isRole(RoleId.Jackal)) return false;
            return player != null && player.Data.Role.IsImpostor;
        }

        public static bool isHauntedWolf(this PlayerControl player)
        {
            if (player.isRole(RoleId.HauntedWolf)) return true;
            return player != null && !player.isImpostor() && !player.isNeutral() && !player.isCrew();
        }

        public static bool IsQuarreled(this PlayerControl player, bool IsChache = true)
        {
            if (player.IsBot()) return false;
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
        public static void SetQuarreled(PlayerControl player1, PlayerControl player2)
        {
            var sets = new List<PlayerControl>() { player1, player2 };
            RoleClass.Quarreled.QuarreledPlayer.Add(sets);
            ChacheManager.ResetQuarreledChache();
        }
        public static void SetQuarreledRPC(PlayerControl player1, PlayerControl player2)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetQuarreled, Hazel.SendOption.Reliable, -1);
            Writer.Write(player1.PlayerId);
            Writer.Write(player2.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static void SetLovers(PlayerControl player1, PlayerControl player2)
        {
            var sets = new List<PlayerControl>() { player1, player2 };
            RoleClass.Lovers.LoversPlayer.Add(sets);
            if (player1.PlayerId == CachedPlayer.LocalPlayer.PlayerId || player2.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            }
            ChacheManager.ResetLoversChache();
        }
        public static void SetLoversRPC(PlayerControl player1, PlayerControl player2)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetLovers, Hazel.SendOption.Reliable, -1);
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
        public static PlayerControl GetOneSideLovers(this PlayerControl player, bool IsChache = true)
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
                    returntext = CustomOptions.JesterIsVent.name + ":" + CustomOptions.JesterIsVent.getString() + "\n";
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
                case RoleId.MadStuntMan:
                    returntext = CustomOptions.MadStuntManIsUseVent.name + ":" + CustomOptions.MadStuntManIsUseVent.getString() + "\n";
                    returntext += CustomOptions.MadStuntManIsCheckImpostor.name + ":" + CustomOptions.MadStuntManIsCheckImpostor.getString() + "\n";
                    break;
                case RoleId.MadJester:
                    returntext = CustomOptions.MadJesterIsUseVent.name + ":" + CustomOptions.MadJesterIsUseVent.getString() + "\n";
                    break;
                case RoleId.MadSeer:
                    returntext = CustomOptions.MadSeerIsUseVent.name + ":" + CustomOptions.MadSeerIsUseVent.getString() + "\n";
                    returntext += CustomOptions.MadSeerIsCheckImpostor.name + ":" + CustomOptions.MadSeerIsCheckImpostor.getString() + "\n";
                    break;
                case RoleId.JackalFriends:
                    returntext = CustomOptions.JackalFriendsIsUseVent.name + ":" + CustomOptions.JackalFriendsIsUseVent.getString() + "\n";
                    returntext += CustomOptions.JackalFriendsIsCheckJackal.name + ":" + CustomOptions.JackalFriendsIsCheckJackal.getString() + "\n";
                    break;
                case RoleId.SeerFriends:
                    returntext = CustomOptions.SeerFriendsIsUseVent.name + ":" + CustomOptions.SeerFriendsIsUseVent.getString() + "\n";
                    returntext += CustomOptions.SeerFriendsIsCheckJackal.name + ":" + CustomOptions.SeerFriendsIsCheckJackal.getString() + "\n";
                    break;
                case RoleId.MayorFriends:
                    returntext = CustomOptions.MayorFriendsIsUseVent.name + ":" + CustomOptions.MayorFriendsIsUseVent.getString() + "\n";
                    returntext += CustomOptions.MayorFriendsIsCheckJackal.name + ":" + CustomOptions.MayorFriendsIsCheckJackal.getString() + "\n";
                    break;
                case RoleId.Fox:
                    returntext = CustomOptions.FoxIsUseVent.name + ":" + CustomOptions.FoxIsUseVent.getString() + "\n";
                    break;
                    //ベント設定可視化
            }
            return returntext;
        }

        public static void ShowFlash(Color color, float duration = 1f)
        //Seerで使用している画面を光らせるコード
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

                if (p < 0.5)
                {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                }
                else
                {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }
                if (p == 1f && renderer != null) renderer.enabled = false;
            })));
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
                case (CustomRPC.RoleId.Shielder):
                    Roles.RoleClass.Shielder.ShielderPlayer.Add(player);
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
                case (CustomRPC.RoleId.MadStuntMan):
                    Roles.RoleClass.MadStuntMan.MadStuntManPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.NiceHawk):
                    Roles.RoleClass.NiceHawk.NiceHawkPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Bakery):
                    Roles.RoleClass.Bakery.BakeryPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MadJester):
                    Roles.RoleClass.MadJester.MadJesterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MadHawk):
                    Roles.RoleClass.MadHawk.MadHawkPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.FalseCharges):
                    Roles.RoleClass.FalseCharges.FalseChargesPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.NiceTeleporter):
                    Roles.RoleClass.NiceTeleporter.NiceTeleporterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Celebrity):
                    Roles.RoleClass.Celebrity.CelebrityPlayer.Add(player);
                    Roles.RoleClass.Celebrity.ViewPlayers.Add(player);
                    break;
                case (CustomRPC.RoleId.Nocturnality):
                    Roles.RoleClass.Nocturnality.NocturnalityPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Observer):
                    Roles.RoleClass.Observer.ObserverPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Vampire):
                    Roles.RoleClass.Vampire.VampirePlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Fox):
                    Roles.RoleClass.Fox.FoxPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.DarkKiller):
                    Roles.RoleClass.DarkKiller.DarkKillerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Seer):
                    Roles.RoleClass.Seer.SeerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MadSeer):
                    Roles.RoleClass.MadSeer.MadSeerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilSeer):
                    Roles.RoleClass.EvilSeer.EvilSeerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.RemoteSheriff):
                    Roles.RoleClass.RemoteSheriff.RemoteSheriffPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.TeleportingJackal):
                    Roles.RoleClass.TeleportingJackal.TeleportingJackalPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MadMaker):
                    Roles.RoleClass.MadMaker.MadMakerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Demon):
                    Roles.RoleClass.Demon.DemonPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.TaskManager):
                    Roles.RoleClass.TaskManager.TaskManagerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.SeerFriends):
                    Roles.RoleClass.SeerFriends.SeerFriendsPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.JackalSeer):
                    Roles.RoleClass.JackalSeer.JackalSeerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Assassin):
                    Roles.RoleClass.Assassin.AssassinPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Marine):
                    Roles.RoleClass.Marine.MarinePlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Arsonist):
                    Roles.RoleClass.Arsonist.ArsonistPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Chief):
                    Roles.RoleClass.Chief.ChiefPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Cleaner):
                    Roles.RoleClass.Cleaner.CleanerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MadCleaner):
                    Roles.RoleClass.MadCleaner.MadCleanerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Samurai):
                    Roles.RoleClass.Samurai.SamuraiPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MayorFriends):
                    Roles.RoleClass.MayorFriends.MayorFriendsPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.VentMaker):
                    Roles.RoleClass.VentMaker.VentMakerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.GhostMechanic):
                    Roles.RoleClass.GhostMechanic.GhostMechanicPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilHacker):
                    Roles.RoleClass.EvilHacker.EvilHackerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.HauntedWolf):
                    Roles.RoleClass.HauntedWolf.HauntedWolfPlayer.Add(player);
                    break;
                //ロールアド
                default:
                    SuperNewRolesPlugin.Logger.LogError("setRole: no method found for role type {role}");
                    return;
            }
            bool flag = player.getRole() != role && player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;
            if (role.isGhostRole())
            {
                ChacheManager.ResetMyGhostRoleChache();
            }
            else
            {
                ChacheManager.ResetMyRoleChache();
            }

            if (flag)
            {
                SuperNewRolesPlugin.Logger.LogInfo("リフレッシュ");
                PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            }
            SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " >= " + role);
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
                case (CustomRPC.RoleId.Shielder):
                    Roles.RoleClass.Shielder.ShielderPlayer.RemoveAll(ClearRemove);
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
                case (CustomRPC.RoleId.MadKiller):
                    Roles.RoleClass.SideKiller.MadKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Survivor):
                    Roles.RoleClass.Survivor.SurvivorPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadMayor):
                    Roles.RoleClass.MadMayor.MadMayorPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadStuntMan):
                    Roles.RoleClass.MadStuntMan.MadStuntManPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadHawk):
                    Roles.RoleClass.MadHawk.MadHawkPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.NiceHawk):
                    Roles.RoleClass.NiceHawk.NiceHawkPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Bakery):
                    Roles.RoleClass.Bakery.BakeryPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadJester):
                    Roles.RoleClass.MadJester.MadJesterPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.FalseCharges):
                    Roles.RoleClass.FalseCharges.FalseChargesPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.NiceTeleporter):
                    Roles.RoleClass.NiceTeleporter.NiceTeleporterPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Celebrity):
                    Roles.RoleClass.Celebrity.CelebrityPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Nocturnality):
                    Roles.RoleClass.Nocturnality.NocturnalityPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Observer):
                    Roles.RoleClass.Observer.ObserverPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Vampire):
                    Roles.RoleClass.Vampire.VampirePlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Fox):
                    Roles.RoleClass.Fox.FoxPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.DarkKiller):
                    Roles.RoleClass.DarkKiller.DarkKillerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Seer):
                    Roles.RoleClass.Seer.SeerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadSeer):
                    Roles.RoleClass.MadSeer.MadSeerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilSeer):
                    Roles.RoleClass.EvilSeer.EvilSeerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.TeleportingJackal):
                    Roles.RoleClass.TeleportingJackal.TeleportingJackalPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.RemoteSheriff):
                    Roles.RoleClass.RemoteSheriff.RemoteSheriffPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadMaker):
                    Roles.RoleClass.MadMaker.MadMakerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Demon):
                    Roles.RoleClass.Demon.DemonPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.TaskManager):
                    Roles.RoleClass.TaskManager.TaskManagerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.SeerFriends):
                    Roles.RoleClass.SeerFriends.SeerFriendsPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.JackalSeer):
                    Roles.RoleClass.JackalSeer.JackalSeerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.SidekickSeer):
                    Roles.RoleClass.JackalSeer.SidekickSeerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Assassin):
                    Roles.RoleClass.Assassin.AssassinPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Marine):
                    Roles.RoleClass.Marine.MarinePlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Arsonist):
                    Roles.RoleClass.Arsonist.ArsonistPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Chief):
                    Roles.RoleClass.Chief.ChiefPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Cleaner):
                    Roles.RoleClass.Cleaner.CleanerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MadCleaner):
                    Roles.RoleClass.MadCleaner.MadCleanerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.Samurai):
                    Roles.RoleClass.Samurai.SamuraiPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.MayorFriends):
                    Roles.RoleClass.MayorFriends.MayorFriendsPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.VentMaker):
                    Roles.RoleClass.VentMaker.VentMakerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.GhostMechanic):
                    Roles.RoleClass.GhostMechanic.GhostMechanicPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.EvilHacker):
                    Roles.RoleClass.EvilHacker.EvilHackerPlayer.RemoveAll(ClearRemove);
                    break;
                case (CustomRPC.RoleId.HauntedWolf):
                    Roles.RoleClass.HauntedWolf.HauntedWolfPlayer.RemoveAll(ClearRemove);
                    break;
                    //ロールリモベ

            }
            ChacheManager.ResetMyRoleChache();
        }
        public static void setRoleRPC(this PlayerControl Player, RoleId SelectRoleDate)
        {
            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetRole, Hazel.SendOption.Reliable, -1);
            killWriter.Write(Player.PlayerId);
            killWriter.Write((byte)SelectRoleDate);
            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
            RPCProcedure.SetRole(Player.PlayerId, (byte)SelectRoleDate);
        }
        public static bool isClearTask(this PlayerControl player)
        {
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
                case (RoleId.MadStuntMan):
                    IsTaskClear = true;
                    break;
                case (RoleId.MadKiller):
                    IsTaskClear = true;
                    break;
                case (RoleId.MadHawk):
                    IsTaskClear = true;
                    break;
                case (RoleId.MadJester):
                    IsTaskClear = true;
                    break;
                case (RoleId.FalseCharges):
                    IsTaskClear = true;
                    break;
                case (RoleId.Fox):
                    IsTaskClear = true;
                    break;
                case (RoleId.TeleportingJackal):
                    IsTaskClear = true;
                    break;
                case (RoleId.Demon):
                    IsTaskClear = true;
                    break;
                case (RoleId.SeerFriends):
                    IsTaskClear = true;
                    break;
                case (RoleId.Arsonist):
                    IsTaskClear = true;
                    break;
                case (RoleId.JackalSeer):
                    IsTaskClear = true;
                    break;
                case (RoleId.SidekickSeer):
                    IsTaskClear = true;
                    break;
                case (RoleId.MadMaker):
                    IsTaskClear = true;
                    break;
                case (RoleId.MadCleaner):
                    IsTaskClear = true;
                    break;
                case (RoleId.MayorFriends):
                    IsTaskClear = true;
                    break;
                    //タスククリアか
            }
            if (!IsTaskClear && ModeHandler.isMode(ModeId.SuperHostRoles) && (player.isRole(RoleId.Sheriff) || player.isRole(RoleId.RemoteSheriff)))
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
            RoleId role = player.getRole();
            if (role == RoleId.Minimalist) return RoleClass.Minimalist.UseVent;
            if (role == RoleId.Samurai) return RoleClass.Samurai.UseVent;
            else if (player.isImpostor()) return true;
            else if (player.isRole(RoleId.Jackal) || player.isRole(RoleId.Sidekick)) return RoleClass.Jackal.IsUseVent;
            else if (ModeHandler.isMode(ModeId.SuperHostRoles) && IsComms()) return false;
            switch (role)
            {
                case RoleId.Jester:
                    return RoleClass.Jester.IsUseVent;
                case RoleId.MadMate:
                    if (CachedPlayer.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel) return false;
                    return RoleClass.MadMate.IsUseVent;
                case RoleId.TeleportingJackal:
                    return RoleClass.TeleportingJackal.IsUseVent;
                case RoleId.JackalFriends:
                    return RoleClass.JackalFriends.IsUseVent;
                case RoleId.Egoist:
                    return RoleClass.Egoist.UseVent;
                case RoleId.Technician:
                    return IsSabotage();
                case RoleId.MadMayor:
                    return RoleClass.MadMayor.IsUseVent;
                case RoleId.MadJester:
                    return RoleClass.MadJester.IsUseVent;
                case RoleId.MadStuntMan:
                    return RoleClass.MadStuntMan.IsUseVent;
                case RoleId.MadHawk:
                    return RoleClass.MadHawk.IsUseVent;
                case RoleId.MadSeer:
                    return RoleClass.MadSeer.IsUseVent;
                case RoleId.MadMaker:
                    return RoleClass.MadMaker.IsUseVent;
                case RoleId.Fox:
                    return RoleClass.Fox.IsUseVent;
                case RoleId.Demon:
                    return RoleClass.Demon.IsUseVent;
                case RoleId.SeerFriends:
                    return RoleClass.SeerFriends.IsUseVent;
                case RoleId.SidekickSeer:
                case RoleId.JackalSeer:
                    return RoleClass.Jackal.IsUseVent;
                case RoleId.MadCleaner:
                    return RoleClass.MadCleaner.IsUseVent;
                /*
                case RoleId.Scavenger:
                    return RoleClass.Scavenger.IsUseVent;
                */
                case RoleId.Arsonist:
                    return RoleClass.Arsonist.IsUseVent;
                case RoleId.Vulture:
                    return RoleClass.Vulture.IsUseVent;
                case RoleId.MayorFriends:
                    return RoleClass.MayorFriends.IsUseVent;
                    //ベントが使える
                    /*
                    case RoleId.Scavenger:
                        return RoleClass.Scavenger.IsUseVent;
                    */
            }
            return false;
        }
        public static bool IsSabotage()
        {
            try
            {
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles)
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
            RoleId role = player.getRole();
            if (role == RoleId.Minimalist) return RoleClass.Minimalist.UseSabo;
            if (role == RoleId.Samurai) return RoleClass.Samurai.UseSabo;
            else if (player.isImpostor()) return true;
            switch (role)
            {
                case RoleId.Jester:
                    return RoleClass.Jester.IsUseSabo;
                case RoleId.Sidekick:
                case RoleId.Jackal:
                    return RoleClass.Jackal.IsUseSabo;
                case RoleId.TeleportingJackal:
                    return RoleClass.TeleportingJackal.IsUseSabo;
                case RoleId.SidekickSeer:
                case RoleId.JackalSeer:
                    return RoleClass.Jackal.IsUseSabo;
                case RoleId.Egoist:
                    return RoleClass.Egoist.UseSabo;
            }
            return false;
        }
        public static bool IsImpostorLight(this PlayerControl player)
        {
            RoleId role = player.getRole();
            if (role == RoleId.Egoist) return RoleClass.Egoist.ImpostorLight;
            if (ModeHandler.isMode(ModeId.SuperHostRoles)) return false;
            switch (role)
            {
                case RoleId.MadMate:
                    return RoleClass.MadMate.IsImpostorLight;
                case RoleId.MadMayor:
                    return RoleClass.MadMayor.IsImpostorLight;
                case RoleId.MadStuntMan:
                    return RoleClass.MadStuntMan.IsImpostorLight;
                case RoleId.MadHawk:
                    return RoleClass.MadHawk.IsImpostorLight;
                case RoleId.MadJester:
                    return RoleClass.MadJester.IsImpostorLight;
                case RoleId.MadSeer:
                    return RoleClass.MadSeer.IsImpostorLight;
                case RoleId.Fox:
                    return RoleClass.Fox.IsImpostorLight;
                case RoleId.TeleportingJackal:
                    return RoleClass.TeleportingJackal.IsImpostorLight;
                case RoleId.MadMaker:
                    return RoleClass.MadMaker.IsImpostorLight;
                case RoleId.Jackal:
                case RoleId.Sidekick:
                    return RoleClass.Jackal.IsImpostorLight;
                case RoleId.JackalFriends:
                    return RoleClass.JackalFriends.IsImpostorLight;
                case RoleId.SeerFriends:
                    return RoleClass.SeerFriends.IsImpostorLight;
                case RoleId.JackalSeer:
                case RoleId.SidekickSeer:
                    return RoleClass.Jackal.IsImpostorLight;
                case RoleId.MadCleaner:
                    return RoleClass.MadCleaner.IsImpostorLight;
                case RoleId.MayorFriends:
                    return RoleClass.MayorFriends.IsImpostorLight;
                    //インポの視界
            }
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
                case (RoleId.FalseCharges):
                    IsNeutral = true;
                    break;
                case (RoleId.Fox):
                    IsNeutral = true;
                    break;
                case (RoleId.TeleportingJackal):
                    IsNeutral = true;
                    break;
                case (RoleId.Demon):
                    IsNeutral = true;
                    break;
                case (RoleId.JackalSeer):
                    IsNeutral = true;
                    break;
                case (RoleId.SidekickSeer):
                    IsNeutral = true;
                    break;
                case (RoleId.Arsonist):
                    IsNeutral = true;
                    break;
                case (RoleId.MayorFriends):
                    IsNeutral = true;
                    break;
                    //第三か
            }
            return IsNeutral;
        }
        public static bool isRole(this PlayerControl p, RoleId role, bool IsChache = true)
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
                MyRole = p.getRole(false);
            }
            if (MyRole == role)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static float getCoolTime(PlayerControl __instance)
        {
            float addition = PlayerControl.GameOptions.killCooldown;
            if (ModeHandler.isMode(ModeId.Default))
            {
                RoleId role = __instance.getRole();
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
                }
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
                case RoleId.Cleaner:
                case RoleId.Samurai:
                    return getCoolTime(p);
            }
            return PlayerControl.GameOptions.killCooldown;
        }
        public static RoleId getGhostRole(this PlayerControl player, bool IsChache = true)
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
                if (Roles.RoleClass.GhostMechanic.GhostMechanicPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.GhostMechanic;
                }
                //ここが幽霊役職
            }
            catch
            {

            }
            return RoleId.DefaultRole;
        }
        public static bool isGhostRole(this RoleId role)
        {
            return IntroDate.GetIntroDate(role).IsGhostRole;
        }
        public static bool isGhostRole(this PlayerControl p, RoleId role, bool IsChache = true)
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
                MyRole = p.getGhostRole(false);
            }
            if (MyRole == role)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static RoleId getRole(this PlayerControl player, bool IsChache = true)
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
                else if (Roles.RoleClass.Shielder.ShielderPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Shielder;
                }
                else if (Roles.RoleClass.Shielder.ShielderPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Shielder;
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
                else if (Roles.RoleClass.MadStuntMan.MadStuntManPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadStuntMan;
                }
                else if (Roles.RoleClass.NiceHawk.NiceHawkPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.NiceHawk;
                }
                else if (Roles.RoleClass.Bakery.BakeryPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Bakery;
                }
                else if (Roles.RoleClass.MadHawk.MadHawkPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadHawk;
                }
                else if (Roles.RoleClass.MadJester.MadJesterPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadJester;
                }
                else if (Roles.RoleClass.FalseCharges.FalseChargesPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.FalseCharges;
                }
                else if (Roles.RoleClass.NiceTeleporter.NiceTeleporterPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.NiceTeleporter;
                }
                else if (Roles.RoleClass.NiceTeleporter.NiceTeleporterPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.NiceTeleporter;
                }
                else if (Roles.RoleClass.Celebrity.CelebrityPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Celebrity;
                }
                else if (Roles.RoleClass.Nocturnality.NocturnalityPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Nocturnality;
                }
                else if (Roles.RoleClass.Observer.ObserverPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Observer;
                }
                else if (Roles.RoleClass.Vampire.VampirePlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Vampire;
                }
                else if (Roles.RoleClass.DarkKiller.DarkKillerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.DarkKiller;
                }
                else if (Roles.RoleClass.Seer.SeerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Seer;
                }
                else if (Roles.RoleClass.MadSeer.MadSeerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadSeer;
                }
                else if (Roles.RoleClass.EvilSeer.EvilSeerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.EvilSeer;
                }
                else if (Roles.RoleClass.RemoteSheriff.RemoteSheriffPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.RemoteSheriff;
                }
                else if (Roles.RoleClass.Vampire.VampirePlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Vampire;
                }
                else if (Roles.RoleClass.DarkKiller.DarkKillerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.DarkKiller;
                }
                else if (Roles.RoleClass.Fox.FoxPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Fox;
                }
                else if (Roles.RoleClass.TeleportingJackal.TeleportingJackalPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.TeleportingJackal;
                }
                else if (Roles.RoleClass.MadMaker.MadMakerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadMaker;
                }
                else if (Roles.RoleClass.DarkKiller.DarkKillerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.DarkKiller;
                }
                else if (Roles.RoleClass.Fox.FoxPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Fox;
                }
                else if (Roles.RoleClass.TeleportingJackal.TeleportingJackalPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.TeleportingJackal;
                }
                else if (Roles.RoleClass.MadMaker.MadMakerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadMaker;
                }
                else if (Roles.RoleClass.Demon.DemonPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Demon;
                }
                else if (Roles.RoleClass.TaskManager.TaskManagerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.TaskManager;
                }
                else if (Roles.RoleClass.SeerFriends.SeerFriendsPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.SeerFriends;
                }
                else if (Roles.RoleClass.JackalSeer.JackalSeerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.JackalSeer;
                }
                else if (Roles.RoleClass.JackalSeer.SidekickSeerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.SidekickSeer;
                }
                else if (Roles.RoleClass.Assassin.AssassinPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Assassin;
                }
                else if (Roles.RoleClass.Marine.MarinePlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Marine;
                }
                else if (Roles.RoleClass.SeerFriends.SeerFriendsPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.SeerFriends;
                }
                else if (Roles.RoleClass.Arsonist.ArsonistPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Arsonist;
                }
                else if (Roles.RoleClass.Chief.ChiefPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Chief;
                }
                else if (Roles.RoleClass.Cleaner.CleanerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Cleaner;
                }
                else if (Roles.RoleClass.Samurai.SamuraiPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.Samurai;
                }
                else if (Roles.RoleClass.MadCleaner.MadCleanerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MadCleaner;
                }
                else if (Roles.RoleClass.MayorFriends.MayorFriendsPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.MayorFriends;
                }
                else if (Roles.RoleClass.VentMaker.VentMakerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.VentMaker;
                }
                else if (Roles.RoleClass.EvilHacker.EvilHackerPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.EvilHacker;
                }
                else if (Roles.RoleClass.HauntedWolf.HauntedWolfPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.HauntedWolf;
                }
                //ロールチェック
            }
            catch (Exception e)
            {

                SuperNewRolesPlugin.Logger.LogInfo("エラー:" + e);
                return RoleId.DefaultRole;
            }
            return RoleId.DefaultRole;

        }
        public static Dictionary<byte, bool> DeadCaches;
        public static bool isDead(this PlayerControl player, bool Cache = true)
        {
            if (Cache)
            {
                try
                {
                    return DeadCaches[player.PlayerId];
                }
                catch { }
            }
            return player == null || player.Data.Disconnected || player.Data.IsDead;
        }

        public static bool isAlive(this PlayerControl player)
        {
            return !isDead(player);
        }
    }
}