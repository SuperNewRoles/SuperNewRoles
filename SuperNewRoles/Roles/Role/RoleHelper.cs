using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Impostor;

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
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetQuarreled, SendOption.Reliable, -1);
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
                PlayerControlHepler.RefreshRoleDescription(PlayerControl.LocalPlayer);
            }
            ChacheManager.ResetLoversChache();
        }
        public static void SetLoversRPC(PlayerControl player1, PlayerControl player2)
        {
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetLovers, SendOption.Reliable, -1);
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

        public static Dictionary<RoleId, List<PlayerControl>> RoleData = new() {
            {RoleId.SoothSayer,RoleClass.SoothSayer.SoothSayerPlayer},
            {RoleId.Jester,RoleClass.Jester.JesterPlayer},
            {RoleId.Lighter,RoleClass.Lighter.LighterPlayer},
            {RoleId.EvilLighter,RoleClass.EvilLighter.EvilLighterPlayer},
            {RoleId.EvilScientist,RoleClass.EvilScientist.EvilScientistPlayer},
            {RoleId.Sheriff,RoleClass.Sheriff.SheriffPlayer},
            {RoleId.MeetingSheriff,RoleClass.MeetingSheriff.MeetingSheriffPlayer},
            {RoleId.Jackal,RoleClass.Jackal.JackalPlayer},
            {RoleId.Sidekick,RoleClass.Jackal.SidekickPlayer},
            {RoleId.Teleporter,RoleClass.Teleporter.TeleporterPlayer},
            {RoleId.SpiritMedium,RoleClass.SpiritMedium.SpiritMediumPlayer},
            {RoleId.SpeedBooster,RoleClass.SpeedBooster.SpeedBoosterPlayer},
            {RoleId.EvilSpeedBooster,RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer},
            {RoleId.Tasker,RoleClass.Tasker.TaskerPlayer},
            {RoleId.Doorr,RoleClass.Doorr.DoorrPlayer},
            {RoleId.EvilDoorr,RoleClass.EvilDoorr.EvilDoorrPlayer},
            {RoleId.Shielder,RoleClass.Shielder.ShielderPlayer},
            {RoleId.Speeder,RoleClass.Speeder.SpeederPlayer},
            {RoleId.Freezer,RoleClass.Freezer.FreezerPlayer},
            {RoleId.Guesser,RoleClass.Guesser.GuesserPlayer},
            {RoleId.EvilGuesser,RoleClass.EvilGuesser.EvilGuesserPlayer},
            {RoleId.Vulture,RoleClass.Vulture.VulturePlayer},
            {RoleId.NiceScientist,RoleClass.NiceScientist.NiceScientistPlayer},
            {RoleId.Clergyman,RoleClass.Clergyman.ClergymanPlayer},
            {RoleId.MadMate,RoleClass.MadMate.MadMatePlayer},
            {RoleId.Bait,RoleClass.Bait.BaitPlayer},
            {RoleId.HomeSecurityGuard,RoleClass.HomeSecurityGuard.HomeSecurityGuardPlayer},
            {RoleId.StuntMan,RoleClass.StuntMan.StuntManPlayer},
            {RoleId.Moving,RoleClass.Moving.MovingPlayer},
            {RoleId.Opportunist,RoleClass.Opportunist.OpportunistPlayer},
            {RoleId.NiceGambler,RoleClass.NiceGambler.NiceGamblerPlayer},
            {RoleId.EvilGambler,RoleClass.EvilGambler.EvilGamblerPlayer},
            {RoleId.Bestfalsecharge,RoleClass.Bestfalsecharge.BestfalsechargePlayer},
            {RoleId.Researcher,RoleClass.Researcher.ResearcherPlayer},
            {RoleId.SelfBomber,RoleClass.SelfBomber.SelfBomberPlayer},
            {RoleId.God,RoleClass.God.GodPlayer},
            {RoleId.AllCleaner,RoleClass.AllCleaner.AllCleanerPlayer},
            {RoleId.NiceNekomata,RoleClass.NiceNekomata.NiceNekomataPlayer},
            {RoleId.EvilNekomata,RoleClass.EvilNekomata.EvilNekomataPlayer},
            {RoleId.JackalFriends,RoleClass.JackalFriends.JackalFriendsPlayer},
            {RoleId.Doctor,RoleClass.Doctor.DoctorPlayer},
            {RoleId.CountChanger,RoleClass.CountChanger.CountChangerPlayer},
            {RoleId.Pursuer,RoleClass.Pursuer.PursuerPlayer},
            {RoleId.Minimalist,RoleClass.Minimalist.MinimalistPlayer},
            {RoleId.Hawk,RoleClass.Hawk.HawkPlayer},
            {RoleId.Egoist,RoleClass.Egoist.EgoistPlayer},
            {RoleId.NiceRedRidingHood,RoleClass.NiceRedRidingHood.NiceRedRidingHoodPlayer},
            {RoleId.EvilEraser,RoleClass.EvilEraser.EvilEraserPlayer},
            {RoleId.Workperson,RoleClass.Workperson.WorkpersonPlayer},
            {RoleId.Magaziner,RoleClass.Magaziner.MagazinerPlayer},
            {RoleId.Hunter,Mode.Werewolf.Main.HunterPlayers},
            {RoleId.Mayor,RoleClass.Mayor.MayorPlayer},
            {RoleId.truelover,RoleClass.Truelover.trueloverPlayer},
            {RoleId.Technician,RoleClass.Technician.TechnicianPlayer},
            {RoleId.SerialKiller,RoleClass.SerialKiller.SerialKillerPlayer},
            {RoleId.OverKiller,RoleClass.OverKiller.OverKillerPlayer},
            {RoleId.Levelinger,RoleClass.Levelinger.LevelingerPlayer},
            {RoleId.EvilMoving,RoleClass.EvilMoving.EvilMovingPlayer},
            {RoleId.Amnesiac,RoleClass.Amnesiac.AmnesiacPlayer},
            {RoleId.SideKiller,RoleClass.SideKiller.SideKillerPlayer},
            {RoleId.Survivor,RoleClass.Survivor.SurvivorPlayer},
            {RoleId.MadMayor,RoleClass.MadMayor.MadMayorPlayer},
            {RoleId.MadStuntMan,RoleClass.MadStuntMan.MadStuntManPlayer},
            {RoleId.NiceHawk,RoleClass.NiceHawk.NiceHawkPlayer},
            {RoleId.Bakery,RoleClass.Bakery.BakeryPlayer},
            {RoleId.MadJester,RoleClass.MadJester.MadJesterPlayer},
            {RoleId.MadHawk,RoleClass.MadHawk.MadHawkPlayer},
            {RoleId.FalseCharges,RoleClass.FalseCharges.FalseChargesPlayer},
            {RoleId.NiceTeleporter,RoleClass.NiceTeleporter.NiceTeleporterPlayer},
            {RoleId.Celebrity,RoleClass.Celebrity.CelebrityPlayer},
            {RoleId.Nocturnality,RoleClass.Nocturnality.NocturnalityPlayer},
            {RoleId.Observer,RoleClass.Observer.ObserverPlayer},
            {RoleId.Vampire,RoleClass.Vampire.VampirePlayer},
            {RoleId.Fox,RoleClass.Fox.FoxPlayer},
            {RoleId.DarkKiller,RoleClass.DarkKiller.DarkKillerPlayer},
            {RoleId.Seer,RoleClass.Seer.SeerPlayer},
            {RoleId.MadSeer,RoleClass.MadSeer.MadSeerPlayer},
            {RoleId.EvilSeer,RoleClass.EvilSeer.EvilSeerPlayer},
            {RoleId.RemoteSheriff,RoleClass.RemoteSheriff.RemoteSheriffPlayer},
            {RoleId.TeleportingJackal,RoleClass.TeleportingJackal.TeleportingJackalPlayer},
            {RoleId.MadMaker,RoleClass.MadMaker.MadMakerPlayer},
            {RoleId.Demon,RoleClass.Demon.DemonPlayer},
            {RoleId.TaskManager,RoleClass.TaskManager.TaskManagerPlayer},
            {RoleId.SeerFriends,RoleClass.SeerFriends.SeerFriendsPlayer},
            {RoleId.JackalSeer,RoleClass.JackalSeer.JackalSeerPlayer},
            {RoleId.SidekickSeer,RoleClass.JackalSeer.SidekickSeerPlayer},
            {RoleId.Assassin,RoleClass.Assassin.AssassinPlayer},
            {RoleId.Marine,RoleClass.Marine.MarinePlayer},
            {RoleId.Arsonist,RoleClass.Arsonist.ArsonistPlayer},
            {RoleId.Chief,RoleClass.Chief.ChiefPlayer},
            {RoleId.Cleaner,RoleClass.Cleaner.CleanerPlayer},
            {RoleId.MadCleaner,RoleClass.MadCleaner.MadCleanerPlayer},
            {RoleId.Samurai,RoleClass.Samurai.SamuraiPlayer},
            {RoleId.MayorFriends,RoleClass.MayorFriends.MayorFriendsPlayer},
            {RoleId.VentMaker,RoleClass.VentMaker.VentMakerPlayer},
            {RoleId.GhostMechanic,RoleClass.GhostMechanic.GhostMechanicPlayer},
            {RoleId.EvilHacker,RoleClass.EvilHacker.EvilHackerPlayer},
            {RoleId.HauntedWolf,RoleClass.HauntedWolf.HauntedWolfPlayer},
            {RoleId.PositionSwapper,RoleClass.PositionSwapper.PositionSwapperPlayer},
            {RoleId.Tuna,RoleClass.Tuna.TunaPlayer},
            {RoleId.Mafia,RoleClass.Mafia.MafiaPlayer},
            {RoleId.BlackCat,RoleClass.BlackCat.BlackCatPlayer},
            {RoleId.SecretlyKiller,RoleClass.SecretlyKiller.SecretlyKillerPlayer},
            {RoleId.Spy,RoleClass.Spy.SpyPlayer},
            {RoleId.Kunoichi,RoleClass.Kunoichi.KunoichiPlayer},
            {RoleId.DoubleKiller,RoleClass.DoubleKiller.DoubleKillerPlayer},
            {RoleId.Smasher,RoleClass.Smasher.SmasherPlayer},
            {RoleId.SuicideWisher,RoleClass.SuicideWisher.SuicideWisherPlayer},
            {RoleId.Neet,RoleClass.Neet.NeetPlayer},
            {RoleId.FastMaker,RoleClass.FastMaker.FastMakerPlayer},
            {RoleId.ToiletFan,RoleClass.ToiletFan.ToiletFanPlayer},
            {RoleId.SatsumaAndImo,RoleClass.SatsumaAndImo.SatsumaAndImoPlayer},
            {RoleId.EvilButtoner,RoleClass.EvilButtoner.EvilButtonerPlayer},
            {RoleId.NiceButtoner,RoleClass.NiceButtoner.NiceButtonerPlayer},
            {RoleId.Finder,RoleClass.Finder.FinderPlayer},
            {RoleId.Revolutionist,RoleClass.Revolutionist.RevolutionistPlayer},
            {RoleId.Dictator,RoleClass.Dictator.DictatorPlayer},
            {RoleId.Spelunker,RoleClass.Spelunker.SpelunkerPlayer},
            {RoleId.SuicidalIdeation,RoleClass.SuicidalIdeation.SuicidalIdeationPlayer},
            {RoleId.Hitman,RoleClass.Hitman.HitmanPlayer},
            {RoleId.Matryoshka,RoleClass.Matryoshka.MatryoshkaPlayer},
            {RoleId.Nun,RoleClass.Nun.NunPlayer},
            {RoleId.Psychometrist,RoleClass.Psychometrist.PsychometristPlayer},
            {RoleId.SeeThroughPerson,RoleClass.SeeThroughPerson.SeeThroughPersonPlayer},
            {RoleId.PartTimer,RoleClass.PartTimer.PartTimerPlayer},
            {RoleId.Painter,RoleClass.Painter.PainterPlayer},
            {RoleId.Photographer,RoleClass.Photographer.PhotographerPlayer},
            {RoleId.Stefinder,RoleClass.Stefinder.StefinderPlayer},
            {RoleId.Slugger,RoleClass.Slugger.SluggerPlayer},
            {RoleId.ShiftActor,ShiftActor.Player},
            {RoleId.ConnectKiller,RoleClass.ConnectKiller.ConnectKillerPlayer},
            {RoleId.Doppelganger,RoleClass.Doppelganger.DoppelggerPlayer},
        };

        public static void SetRole(this PlayerControl player, RoleId role)
        {
            if (!Roles.Neutral.Spelunker.CheckSetRole(player, role)) return;
            foreach (var dicItem in RoleData)
            {
                if (role == dicItem.Key)
                {
                    if (dicItem.Key == RoleId.Celebrity)
                    {
                        RoleClass.Celebrity.ViewPlayers.Add(player);
                    }
                    dicItem.Value.Add(player);
                    break;
                }
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
                PlayerControlHepler.RefreshRoleDescription(PlayerControl.LocalPlayer);
            }
            SuperNewRolesPlugin.Logger.LogInfo(player.Data.PlayerName + " >= " + role);
            ShiftActor.ShapeshifterSet();
            PlayerAnimation anim = PlayerAnimation.GetPlayerAnimation(player.PlayerId);
            if (anim != null) anim.HandleAnim(RpcAnimationType.Stop);
        }
        private static PlayerControl ClearTarget;
        public static void ClearRole(this PlayerControl player)
        {
            static bool ClearRemove(PlayerControl p)
            {
                return p.PlayerId == ClearTarget.PlayerId;
            }
            ClearTarget = player;
            foreach (var dicItem in RoleData)
            {
                if (player.GetRole() == dicItem.Key)
                {
                    dicItem.Value.RemoveAll(ClearRemove);
                }
            }
            ChacheManager.ResetMyRoleChache();
        }
        public static void SetRoleRPC(this PlayerControl Player, RoleId SelectRoleDate)
        {
            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRole, SendOption.Reliable, -1);
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
                case RoleId.SatsumaAndImo:
                case RoleId.Revolutionist:
                case RoleId.Spelunker:
                case RoleId.SuicidalIdeation:
                case RoleId.Hitman:
                case RoleId.Stefinder:
                case RoleId.PartTimer:
                case RoleId.Photographer:
                    //タスククリアか
                    IsTaskClear = true;
                    break;
            }
            if (!IsTaskClear
                && ((ModeHandler.IsMode(ModeId.SuperHostRoles) &&
                player.IsRole(RoleId.Sheriff, RoleId.RemoteSheriff, RoleId.ToiletFan, RoleId.NiceButtoner))
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
                RoleId.Stefinder => RoleClass.Stefinder.UseVent,
                _ => false,
            };
        }
        public static bool IsSabotage()
        {
            try
            {
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
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
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixComms)
                        return true;
            }
            catch { }
            return false;
        }
        public static bool IsLightdown()
        {
            try
            {
                foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    if (task.TaskType == TaskTypes.FixLights)
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
                RoleId.Jester => RoleClass.Jester.IsUseSabo && ModeHandler.IsMode(ModeId.Default),
                RoleId.Sidekick or RoleId.Jackal => RoleClass.Jackal.IsUseSabo,
                RoleId.TeleportingJackal => RoleClass.TeleportingJackal.IsUseSabo,
                RoleId.SidekickSeer or RoleId.JackalSeer => RoleClass.JackalSeer.IsUseSabo,
                RoleId.Egoist => RoleClass.Egoist.UseSabo,
                RoleId.Stefinder => RoleClass.Stefinder.UseSabo,
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
                    RoleId.Photographer => RoleClass.Photographer.IsImpostorVision,
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
                case RoleId.Spelunker:
                case RoleId.SuicidalIdeation:
                case RoleId.Hitman:
                case RoleId.Stefinder:
                case RoleId.PartTimer:
                case RoleId.Photographer:
                    //第三か
                    IsNeutral = true;
                    break;
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
                    case RoleId.Matryoshka:
                        addition = RoleClass.Matryoshka.MyKillCoolTime;
                        break;
                    case RoleId.ShiftActor:
                        addition = ShiftActor.KillCool;
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
                foreach (var dicItem in RoleData)
                {
                    if (dicItem.Value.IsCheckListPlayerControl(player)) return dicItem.Key;
                }
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
        public static bool IsDead(this CachedPlayer player)
        {
            return player == null || player.Data.Disconnected || player.Data.IsDead;
        }
        public static bool IsAlive(this CachedPlayer player)
        {
            return !IsDead(player);
        }
    }
}
