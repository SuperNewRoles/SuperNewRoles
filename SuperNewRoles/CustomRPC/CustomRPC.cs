using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.CustomOption;
using SuperNewRoles.EndGame;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using SuperNewRoles.Sabotage;
using UnityEngine;
using static SuperNewRoles.EndGame.FinalStatusPatch;

namespace SuperNewRoles.CustomRPC
{
    public enum RoleId
    {
        DefaultRole,
        SoothSayer,
        Jester,
        Lighter,
        EvilLighter,
        EvilScientist,
        Sheriff,
        MeetingSheriff,
        Jackal,
        Sidekick,
        Teleporter,
        SpiritMedium,
        SpeedBooster,
        EvilSpeedBooster,
        Tasker,
        Doorr,
        EvilDoorr,
        Shielder,
        Speeder,
        Freezer,
        Guesser,
        EvilGuesser,
        Vulture,
        NiceScientist,
        Clergyman,
        MadMate,
        Bait,
        HomeSecurityGuard,
        StuntMan,
        Moving,
        Opportunist,
        NiceGambler,
        EvilGambler,
        Bestfalsecharge,
        Researcher,
        SelfBomber,
        God,
        AllCleaner,
        NiceNekomata,
        EvilNekomata,
        JackalFriends,
        Doctor,
        CountChanger,
        Pursuer,
        Minimalist,
        Hawk,
        Egoist,
        NiceRedRidingHood,
        EvilEraser,
        Workperson,
        Magaziner,
        Hunter,
        Mayor,
        truelover,
        Technician,
        SerialKiller,
        OverKiller,
        Levelinger,
        EvilMoving,
        Amnesiac,
        SideKiller,
        MadKiller,
        Survivor,
        MadMayor,
        NiceHawk,
        Bakery,
        Neta,
        MadJester,
        MadStuntMan,
        MadHawk,
        FalseCharges,
        NiceTeleporter,
        Celebrity,
        Nocturnality,
        Observer,
        Vampire,
        DarkKiller,
        Fox,
        Seer,
        MadSeer,
        EvilSeer,
        RemoteSheriff,
        TeleportingJackal,
        MadMaker,
        Demon,
        TaskManager,
        SeerFriends,
        JackalSeer,
        SidekickSeer,
        Assassin,
        Marine,
        Arsonist,
        Chief,
        Cleaner,
        MadCleaner,
        Samurai,
        MayorFriends,
        VentMaker,
        GhostMechanic,
        EvilHacker,
        HauntedWolf,
        PositionSwapper,
        Tuna,
        Mafia,
        BlackCat,
        SecretlyKiller,
        Spy,
        Kunoichi,
        DoubleKiller,
        Smasher,
        SuicideWisher,
        Neet,
        FastMaker,
        ToiletFan,
        SuperVitaler,
        //RoleId
    }

    public enum CustomRPC
    {
        //TORVersionShare = 65,
        ShareOptions = 144,
        ShareSNRVersion,
        SetRole,
        SetQuarreled,
        RPCClergymanLightOut,
        SheriffKill,
        MeetingSheriffKill,
        CustomRPCKill,
        ReportDeadBody,
        UncheckedMeeting,
        CleanBody,
        ExiledRPC,
        RPCMurderPlayer,
        ShareWinner,
        TeleporterTP,
        SidekickPromotes,
        SidekickSeerPromotes,
        CreateSidekick,
        CreateSidekickSeer,
        SetSpeedBoost,
        ShareCosmetics,
        SetShareNamePlate,
        AutoCreateRoom,
        BomKillRPC,
        ByBomKillRPC,
        NekomataExiledRPC,
        CountChangerSetRPC,
        SetRoomTimerRPC,
        SetScientistRPC,
        ReviveRPC,
        SetHaison,
        SetWinCond,
        SetDetective,
        UseEraserCount,
        StartGameRPC,
        UncheckedSetTasks,
        SetLovers,
        SetUseDevice,
        SetDeviceTime,
        UncheckedSetColor,
        UncheckedSetVanilaRole,
        SetMadKiller,
        SetCustomSabotage,
        UseStuntmanCount,
        UseMadStuntmanCount,
        CustomEndGame,
        UncheckedProtect,
        SetBot,
        DemonCurse,
        ArsonistDouse,
        SetSpeedDown,
        ShielderProtect,
        SetShielder,
        SetSpeedFreeze,
        BySamuraiKillRPC,
        MakeVent,
        PositionSwapperTP,
        UseAdminTime,
        UseCameraTime,
        UseVitalsTime,
        FixLights,
        RandomSpawn,
        KunaiKill,
        SetSecretRoomTeleportStatus,
        ChiefSidekick
    }
    public static class RPCProcedure
    {
        public static void KunaiKill(byte sourceid, byte targetid)
        {
            PlayerControl source = ModHelpers.playerById(sourceid);
            PlayerControl target = ModHelpers.playerById(targetid);
            if (source == null || target == null) return;
            RPCMurderPlayer(sourceid, targetid, 0);
            FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.Kill;

            if (targetid == CachedPlayer.LocalPlayer.PlayerId)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(target.Data, source.Data);
            }
        }

        public static void ChiefSidekick(byte targetid)
        {
            RoleClass.Chief.SheriffPlayer.Add(targetid);
            SetRole(targetid, (byte)RoleId.Sheriff);
            if (targetid == CachedPlayer.LocalPlayer.PlayerId)
            {
                Sheriff.ResetKillCoolDown();
                RoleClass.Sheriff.KillMaxCount = RoleClass.Chief.KillLimit;
            }
            UncheckedSetVanilaRole(targetid, 0);
        }
        public static void FixLights()
        {
            SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
            switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
        }
        public static void ArsonistDouse(byte source, byte target)
        {
            PlayerControl TargetPlayer = ModHelpers.playerById(target);
            PlayerControl SourcePlayer = ModHelpers.playerById(source);
            if (TargetPlayer == null || SourcePlayer == null) return;
            if (!RoleClass.Arsonist.DouseDatas.ContainsKey(source)) RoleClass.Arsonist.DouseDatas[source] = new();
            if (!Arsonist.IsDoused(SourcePlayer, TargetPlayer))
            {
                RoleClass.Arsonist.DouseDatas[source].Add(TargetPlayer);
            }
        }
        public static void DemonCurse(byte source, byte target)
        {
            PlayerControl TargetPlayer = ModHelpers.playerById(target);
            PlayerControl SourcePlayer = ModHelpers.playerById(source);
            if (TargetPlayer == null || SourcePlayer == null) return;
            if (!RoleClass.Demon.CurseDatas.ContainsKey(source)) RoleClass.Demon.CurseDatas[source] = new();
            if (!Demon.IsCursed(SourcePlayer, TargetPlayer))
            {
                RoleClass.Demon.CurseDatas[source].Add(TargetPlayer);
            }
        }
        public static void SetBot(byte playerid)
        {
            SuperNewRolesPlugin.Logger.LogInfo("セットボット！！！！！！！！！");
            PlayerControl player = ModHelpers.playerById(playerid);
            if (player == null)
            {
                SuperNewRolesPlugin.Logger.LogInfo("nullなのでreturn");
                return;
            }
            SuperNewRolesPlugin.Logger.LogInfo("通過:" + player.name);
            if (BotManager.AllBots == null) BotManager.AllBots = new();
            BotManager.AllBots.Add(player);

        }
        public static void UncheckedProtect(byte sourceid, byte playerid, byte colorid)
        {
            PlayerControl player = ModHelpers.playerById(playerid);
            PlayerControl source = ModHelpers.playerById(sourceid);
            if (player == null || source == null) return;
            source.ProtectPlayer(player, colorid);
        }
        public static void CustomEndGame(GameOverReason reason, bool showAd)
        {
            CheckGameEndPatch.CustomEndGame(reason, showAd);
        }
        public static void UseStuntmanCount(byte playerid)
        {
            var player = ModHelpers.playerById(playerid);
            if (player == null) return;
            if (player.isRole(RoleId.MadStuntMan))
            {
                if (!RoleClass.MadStuntMan.GuardCount.ContainsKey(playerid))
                {
                    RoleClass.MadStuntMan.GuardCount[playerid] = ((int)CustomOptions.MadStuntManMaxGuardCount.getFloat()) - 1;
                }
                else
                {
                    RoleClass.MadStuntMan.GuardCount[playerid]--;
                }
            }
            else if (player.isRole(RoleId.StuntMan))
            {
                if (!RoleClass.StuntMan.GuardCount.ContainsKey(playerid))
                {
                    RoleClass.StuntMan.GuardCount[playerid] = ((int)CustomOptions.StuntManMaxGuardCount.getFloat()) - 1;
                }
                else
                {
                    RoleClass.StuntMan.GuardCount[playerid]--;
                }
            }
        }
        public static void SetMadKiller(byte sourceid, byte targetid)
        {
            var source = ModHelpers.playerById(sourceid);
            var target = ModHelpers.playerById(targetid);
            if (source == null || target == null) return;
            target.ClearRole();
            RoleClass.SideKiller.MadKillerPlayer.Add(target);
            RoleClass.SideKiller.MadKillerPair.Add(source.PlayerId, target.PlayerId);
            DestroyableSingleton<RoleManager>.Instance.SetRole(target, RoleTypes.Crewmate);
            ChacheManager.ResetMyRoleChache();
            PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
        }
        public static void UncheckedSetVanilaRole(byte playerid, byte roletype)
        {
            var player = ModHelpers.playerById(playerid);
            if (player == null) return;
            DestroyableSingleton<RoleManager>.Instance.SetRole(player, (RoleTypes)roletype);
            player.Data.Role.Role = (RoleTypes)roletype;
        }
        public static void TORVersionShare(int major, int minor, int build, int revision, byte[] guid, int clientId)
        {
            /*
            SuperNewRolesPlugin.Logger.LogInfo("TORGMシェアあああ！");
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TORVersionShare, Hazel.SendOption.Reliable, clientId);
            writer.WritePacked(major);
            writer.WritePacked(minor);
            writer.WritePacked(build);
            writer.WritePacked(AmongUsClient.Instance.ClientId);
            writer.Write(revision);
            writer.Write(guid);
            AmongUsClient.Instance.FinishRpcImmediately(writer);*/
        }
        public static void SetDeviceTime(float time, byte systemtype)
        {
            var stype = (SystemTypes)systemtype;
            if (stype == SystemTypes.Security)
            {
                BlockTool.CameraTime = time;
            }
            else if (stype == SystemTypes.Admin)
            {
                BlockTool.AdminTime = time;
            }
            else if (stype == SystemTypes.Medical)
            {
                BlockTool.VitalTime = time;
            }
        }
        public static void SetUseDevice(byte playerid, byte systemtype, bool Is)
        {/*
            var stype = (SystemTypes)systemtype;
            var player = ModHelpers.playerById(playerid);
            if (stype == SystemTypes.Security)
            {
                if (Is)
                {
                    if (!BlockTool.CameraPlayers.Contains(player.PlayerId))
                    {
                        BlockTool.CameraPlayers.Add(player.PlayerId);
                    }
                }
                else
                {
                    if (BlockTool.CameraPlayers.Contains(player.PlayerId))
                    {
                        BlockTool.CameraPlayers.Remove(player.PlayerId);
                    }
                }
            } else if (stype == SystemTypes.Admin)
            {
                if (Is)
                {
                    if (!BlockTool.AdminPlayers.Contains(player.PlayerId))
                    {
                        BlockTool.AdminPlayers.Add(player.PlayerId);
                    }
                }
                else
                {
                    if (BlockTool.AdminPlayers.Contains(player.PlayerId))
                    {
                        BlockTool.AdminPlayers.Remove(player.PlayerId);
                    }
                }
            } else if (stype == SystemTypes.Medical)
            {
                if (Is)
                {
                    if (!BlockTool.VitalPlayers.Contains(player.PlayerId))
                    {
                        BlockTool.VitalPlayers.Add(player.PlayerId);
                    }
                }
                else
                {
                    if (BlockTool.VitalPlayers.Contains(player.PlayerId))
                    {
                        BlockTool.VitalPlayers.Remove(player.PlayerId);
                    }
                }
            }*/
        }
        public static void uncheckedSetTasks(byte playerId, byte[] taskTypeIds)
        {
            var player = ModHelpers.playerById(playerId);
            player.clearAllTasks();

            GameData.Instance.SetTasks(playerId, taskTypeIds);
        }
        public static void StartGameRPC()
        {
            RoleClass.ClearAndReloadRoles();
        }
        public static void UseEraserCount(byte playerid)
        {
            PlayerControl p = ModHelpers.playerById(playerid);
            if (p == null) return;
            if (!RoleClass.EvilEraser.Counts.ContainsKey(playerid))
            {
                RoleClass.EvilEraser.Counts[playerid] = RoleClass.EvilEraser.Count;
            }
            else
            {
                RoleClass.EvilEraser.Counts[playerid]--;
            }
        }
        // Main Controls
        public static void AutoCreateRoom()
        {
            if (!ConfigRoles.IsAutoRoomCreate.Value) return;
            AmongUsClient.Instance.StartCoroutine(nameof(CREATEROOMANDJOIN));
            static IEnumerator CREATEROOMANDJOIN()
            {
                var gameid = AmongUsClient.Instance.GameId;
                yield return new WaitForSeconds(8);
                try
                {
                    AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                    SceneChanger.ChangeScene("MainMenu");
                }
                catch
                {
                }
                AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameid);
            }
        }
        public static void SetHaison()
        {
            EndGameManagerSetUpPatch.IsHaison = true;
        }
        public static void ShareCosmetics(byte id, string url)
        {/**

            if (ModHelpers.playerById(id) == null) return;
            if (!SharePatch.PlayerUrl.ContainsKey(id))
            {
                SharePatch.PlayerUrl[id] = url;
                HttpConnect.ShareCosmeticDateDownload(id,url);
            }
            **/
        }
        public static void SetRoomTimerRPC(byte min, byte seconds)
        {
            Patch.ShareGameVersion.timer = (min * 60) + seconds;
        }
        public static void CountChangerSetRPC(byte sourceid, byte targetid)
        {
            var source = ModHelpers.playerById(sourceid);
            var target = ModHelpers.playerById(targetid);
            if (source == null || target == null) return;
            if (CustomOptions.CountChangerNextTurn.getBool())
            {
                RoleClass.CountChanger.Setdata[source.PlayerId] = target.PlayerId;
            }
            else
            {
                RoleClass.CountChanger.ChangeData[source.PlayerId] = target.PlayerId;
            }
        }
        public static void SetDetective(byte playerid)
        {
            var player = ModHelpers.playerById(playerid);
            if (player == null) return;
            Mode.Detective.main.DetectivePlayer = player;
        }
        public static void SetShareNamePlate(byte playerid, byte id)
        {
        }
        public static void ShareOptions(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption.CustomOption option = CustomOption.CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                    option.updateSelection((int)selection);
                }
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }
        public static void ShareSNRversion(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            System.Version ver;
            if (revision < 0)
                ver = new System.Version(major, minor, build);
            else
                ver = new System.Version(major, minor, build, revision);
            Patch.ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers[clientId] = new Patch.PlayerVersion(ver, guid);
            //SuperNewRolesPlugin.Logger.LogInfo("PATCHES:"+ Patch.ShareGameVersion.playerVersions);
        }
        public static void SetRole(byte playerid, byte RPCRoleId)
        {
            var player = ModHelpers.playerById(playerid);
            var roleId = (RoleId)RPCRoleId;
            if (!roleId.isGhostRole())
            {
                player.ClearRole();
            }
            player.setRole(roleId);
        }
        public static void SetQuarreled(byte playerid1, byte playerid2)
        {
            var player1 = ModHelpers.playerById(playerid1);
            var player2 = ModHelpers.playerById(playerid2);
            RoleHelpers.SetQuarreled(player1, player2);
        }
        public static void SetLovers(byte playerid1, byte playerid2)
        {
            var player1 = ModHelpers.playerById(playerid1);
            var player2 = ModHelpers.playerById(playerid2);
            RoleHelpers.SetLovers(player1, player2);
        }
        public static void SheriffKill(byte SheriffId, byte TargetId, bool MissFire)
        {
            SuperNewRolesPlugin.Logger.LogInfo("シェリフ");
            PlayerControl sheriff = ModHelpers.playerById(SheriffId);
            PlayerControl target = ModHelpers.playerById(TargetId);
            if (sheriff == null || target == null) return;
            SuperNewRolesPlugin.Logger.LogInfo("通過");

            if (MissFire)
            {
                sheriff.MurderPlayer(sheriff);
                FinalStatusData.FinalStatuses[sheriff.PlayerId] = FinalStatus.SheriffMisFire;
            }
            else
            {
                if (sheriff.isRole(RoleId.RemoteSheriff) && !RoleClass.RemoteSheriff.IsKillTeleport)
                {
                    if (CachedPlayer.LocalPlayer.PlayerId == SheriffId)
                    {
                        target.MurderPlayer(target);
                    }
                    else
                    {
                        sheriff.MurderPlayer(target);
                    }
                }
                else
                {
                    sheriff.MurderPlayer(target);
                }
                FinalStatusData.FinalStatuses[sheriff.PlayerId] = FinalStatus.SheriffKill;
            }

        }
        public static void MeetingSheriffKill(byte SheriffId, byte TargetId, bool MissFire)
        {
            PlayerControl sheriff = ModHelpers.playerById(SheriffId);
            PlayerControl target = ModHelpers.playerById(TargetId);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
            if (sheriff == null || target == null) return;
            if (!PlayerControl.LocalPlayer.isAlive())
            {
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, sheriff.name + "は" + target.name + "をシェリフキルした！");
                if (MissFire)
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, sheriff.name + "は誤爆した！");
                }
                else
                {
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, sheriff.name + "は成功した！");
                }
            }
            if (MissFire)
            {
                sheriff.Exiled();
                FinalStatusData.FinalStatuses[sheriff.PlayerId] = FinalStatus.MeetingSheriffMisFire;
                if (PlayerControl.LocalPlayer == sheriff)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(sheriff.Data, sheriff.Data);
                }

            }
            else
            {
                target.Exiled();
                FinalStatusData.FinalStatuses[sheriff.PlayerId] = FinalStatus.MeetingSheriffKill;
                if (PlayerControl.LocalPlayer == target)
                {
                    FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(target.Data, sheriff.Data);
                }
            }
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId == SheriffId && MissFire)
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                    }
                    else if (pva.TargetPlayerId == TargetId && !MissFire)
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                    }
                }
                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }

        }
        public static void CustomRPCKill(byte notTargetId, byte targetId)
        {
            if (notTargetId == targetId)
            {
                PlayerControl Player = ModHelpers.playerById(targetId);
                Player.MurderPlayer(Player);
            }
            else
            {
                PlayerControl notTargetPlayer = ModHelpers.playerById(notTargetId);
                PlayerControl TargetPlayer = ModHelpers.playerById(targetId);
                notTargetPlayer.MurderPlayer(TargetPlayer);
            }
        }
        public static void RPCClergymanLightOut(bool Start)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (Start)
            {
                Roles.Clergyman.LightOutStartRPC();
            }
            else
            {
            }
        }
        public static void SetSpeedBoost(bool Is, byte id)
        {
            var player = ModHelpers.playerById(id);
            if (player == null) return;
            if (player.Data.Role.IsImpostor)
            {
                RoleClass.EvilSpeedBooster.IsBoostPlayers[id] = Is;
            }
            else
            {
                RoleClass.SpeedBooster.IsBoostPlayers[id] = Is;
            }
        }
        public static void ReviveRPC(byte playerid)
        {
            var player = ModHelpers.playerById(playerid);
            if (player == null) return;
            player.Revive();
            DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == playerid);
            FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.Alive;
        }
        public static void SetScientistRPC(bool Is, byte id)
        {
            RoleClass.NiceScientist.IsScientistPlayers[id] = Is;
        }
        public static void ReportDeadBody(byte sourceId, byte targetId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (source != null && target != null) source.ReportDeadBody(target.Data);
        }
        public static void UncheckedMeeting(byte sourceId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            if (source != null) source.ReportDeadBody(null);
        }
        public static void CleanBody(byte playerId)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }
            }
        }
        public static void SidekickPromotes()
        {
            foreach (PlayerControl p in RoleClass.Jackal.SidekickPlayer.ToArray())
            {
                p.ClearRole();
                p.setRole(RoleId.Jackal);
            }
            PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            ChacheManager.ResetMyRoleChache();
        }
        public static void SidekickSeerPromotes()
        {
            foreach (PlayerControl p in RoleClass.JackalSeer.SidekickSeerPlayer.ToArray())
            {
                p.ClearRole();
                p.setRole(RoleId.JackalSeer);
            }
            PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
            ChacheManager.ResetMyRoleChache();
        }
        public static void CreateSidekick(byte playerid, bool IsFake)
        {
            var player = ModHelpers.playerById(playerid);
            if (player == null) return;
            if (IsFake)
            {
                RoleClass.Jackal.FakeSidekickPlayer.Add(player);
            }
            else
            {
                DestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                player.ClearRole();
                RoleClass.Jackal.SidekickPlayer.Add(player);
                PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
                ChacheManager.ResetMyRoleChache();
            }
        }
        public static void CreateSidekickSeer(byte playerid, bool IsFake)
        {
            var player = ModHelpers.playerById(playerid);
            if (player == null) return;
            if (IsFake)
            {
                RoleClass.JackalSeer.FakeSidekickSeerPlayer.Add(player);
            }
            else
            {
                DestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
                player.ClearRole();
                RoleClass.JackalSeer.SidekickSeerPlayer.Add(player);
                PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
                ChacheManager.ResetMyRoleChache();
            }
        }
        public static void BomKillRPC(byte sourceId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            if (source != null)
            {
                KillAnimationCoPerformKillPatch.hideNextAnimation = false;
                source.MurderPlayer(source);
                FinalStatusData.FinalStatuses[source.PlayerId] = FinalStatus.SelfBomb;
            }
        }
        public static void ByBomKillRPC(byte sourceId, byte targetId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (source != null && target != null)
            {
                source.MurderPlayer(target);
                FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.BySelfBomb;
            }
        }
        public static void BySamuraiKillRPC(byte sourceId, byte targetId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (source != null && target != null)
            {
                source.MurderPlayer(target);
                FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.Kill;
            }
        }
        public static void ExiledRPC(byte playerid)
        {
            var player = ModHelpers.playerById(playerid);
            if (player != null)
            {
                player.Exiled();
            }
        }
        public static void NekomataExiledRPC(byte playerid)
        {
            var player = ModHelpers.playerById(playerid);
            if (player != null)
            {
                player.Exiled();
                FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.NekomataExiled;
            }
        }
        [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
        class KillAnimationCoPerformKillPatch
        {
            public static bool hideNextAnimation = false;

            public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)] ref PlayerControl source, [HarmonyArgument(1)] ref PlayerControl target)
            {
                if (hideNextAnimation)
                    source = target;
                hideNextAnimation = false;
            }
        }
        public static void RPCMurderPlayer(byte sourceId, byte targetId, byte showAnimation)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (source != null && target != null)
            {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target);
                FinalStatusData.FinalStatuses[source.PlayerId] = FinalStatus.Kill;
            }
        }
        public static void ShareWinner(byte playerid)
        {
            PlayerControl player = ModHelpers.playerById(playerid);
            if (ModeHandler.isMode(ModeId.BattleRoyal))
            {
                Mode.BattleRoyal.main.Winners.Add(player);
            }
            else
            {
                EndGame.OnGameEndPatch.WinnerPlayer = player;
            }
        }
        public static void TeleporterTP(byte playerid)
        {
            var p = ModHelpers.playerById(playerid);
            CachedPlayer.LocalPlayer.transform.position = p.transform.position;
            if (SubmergedCompatibility.isSubmerged())
            {
                SubmergedCompatibility.ChangeFloor(SubmergedCompatibility.GetFloor(p));
            }
            new CustomMessage(string.Format(ModTranslation.getString("TeleporterTPTextMessage"), p.nameText().text), 3);
        }
        public static void SetWinCond(byte Cond)
        {
            OnGameEndPatch.EndData = (CustomGameOverReason)Cond;
        }
        public static void SetSpeedDown(bool Is)
        {
            RoleClass.Speeder.IsSpeedDown = Is;
        }
        public static void SetSpeedFreeze(bool Is)
        {
            RoleClass.Freezer.IsSpeedDown = Is;
        }
        public static void ShielderProtect(byte sourceId, byte targetId, byte colorid)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (target == null || source == null) return;
            source.ProtectPlayer(target, colorid);
            PlayerControl.LocalPlayer.MurderPlayer(target);
            source.ProtectPlayer(target, colorid);
            if (targetId == CachedPlayer.LocalPlayer.PlayerId) Buttons.HudManagerStartPatch.ShielderButton.Timer = 0f;
        }
        public static void SetShielder(byte PlayerId, bool Is)
        {
            RoleClass.Shielder.IsShield[PlayerId] = RoleClass.Shielder.IsShield[PlayerId] = Is;
        }
        public static void MakeVent(float x, float y, float z)
        {
            Vent template = UnityEngine.Object.FindObjectOfType<Vent>();
            Vent VentMakerVent = UnityEngine.Object.Instantiate<Vent>(template);
            if (RoleClass.VentMaker.VentCount == 2)
            {
                RoleClass.VentMaker.Vent.Right = VentMakerVent;
                VentMakerVent.Right = RoleClass.VentMaker.Vent;
                VentMakerVent.Left = null;
                VentMakerVent.Center = null;
            }
            else
            {
                VentMakerVent.Right = null;
                VentMakerVent.Left = null;
                VentMakerVent.Center = null;
            }

            VentMakerVent.transform.position = new Vector3(x, y, z);
            VentMakerVent.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            VentMakerVent.Id = MapUtilities.CachedShipStatus.AllVents.Select(x => x.Id).Max() + 1;
            var allVentsList = MapUtilities.CachedShipStatus.AllVents.ToList();
            allVentsList.Add(VentMakerVent);
            MapUtilities.CachedShipStatus.AllVents = allVentsList.ToArray();
            VentMakerVent.name = "VentMakerVent" + VentMakerVent.Id;
            VentMakerVent.gameObject.SetActive(true);
        }
        public static void PositionSwapperTP(byte SwapPlayerID, byte SwapperID)
        {
            SuperNewRolesPlugin.Logger.LogInfo("スワップ開始！");
            /*if (SubmergedCompatibility.isSubmerged())
            {
                if (PlayerControl.LocalPlayer.PlayerId == SwapPlayerID){
                    SubmergedCompatibility.ChangeFloor(SwapperPlayerFloor);
                }
                else{
                    SubmergedCompatibility.ChangeFloor(SwapPlayerFloor);
                }
            }*/

            var SwapPlayer = ModHelpers.playerById(SwapPlayerID);
            var SwapperPlayer = ModHelpers.playerById(SwapperID);
            var SwapPosition = SwapPlayer.transform.position;
            var SwapperPosition = SwapperPlayer.transform.position;
            //Text
            var rand = new System.Random();
            if (SwapperID == PlayerControl.LocalPlayer.PlayerId /*PlayerControl.LocalPlayer.isRole(RoleId.PositionSwapper)*/)
            {
                CachedPlayer.LocalPlayer.transform.position = SwapPosition;
                SuperNewRolesPlugin.Logger.LogInfo("スワップ本体！");
                return;
            }
            else if (SwapPlayerID == PlayerControl.LocalPlayer.PlayerId)
            {
                CachedPlayer.LocalPlayer.transform.position = SwapperPosition;
                SuperNewRolesPlugin.Logger.LogInfo("スワップランダム！");
                if (rand.Next(1, 20) == 1)
                {
                    new CustomMessage(string.Format(ModTranslation.getString("PositionSwapperSwapText2")), 3);
                }
                else
                {
                    new CustomMessage(string.Format(ModTranslation.getString("PositionSwapperSwapText")), 3);
                }
            }
        }
        /*
        public static void UseAdminTime(float time)
        {
            Patch.AdminPatch.RestrictAdminTime -= time;
        }
        public static void UseCameraTime(float time)
        {
            Patch.CameraPatch.RestrictCameraTime -= time;
        }
        public static void UseVitalTime(float time)
        {
            Patch.VitalsPatch.RestrictVitalsTime -= time;
        }*/
        public static void randomSpawn(byte playerId, byte locId)
        {
            HudManager.Instance.StartCoroutine(Effects.Lerp(3f, new Action<float>((p) =>
            { // Delayed action
                if (p == 1f)
                {
                    Vector2 InitialSpawnCenter = new(16.64f, -2.46f);
                    Vector2 MeetingSpawnCenter = new(17.4f, -16.286f);
                    Vector2 ElectricalSpawn = new(5.53f, -9.84f);
                    Vector2 O2Spawn = new(3.28f, -21.67f);
                    Vector2 SpecimenSpawn = new(36.54f, -20.84f);
                    Vector2 LaboSpawn = new(34.91f, -6.50f);
                    Vector2 CommsSpawn = new(12.24f, -15.9473f);
                    Vector2 StorageSpawn = new(20.9707f, -12.3396f);
                    Vector2 MeetingSpawnUnder = new(22.0948f, -25.1668f);
                    Vector2 LocketSpawn = new(26.6442f, -6.775f);
                    Vector2 LeftReactorSpawn = new(4.6395f, -4.2884f);
                    var loc = locId switch
                    {
                        0 => InitialSpawnCenter,
                        1 => MeetingSpawnCenter,
                        2 => ElectricalSpawn,
                        3 => O2Spawn,
                        4 => SpecimenSpawn,
                        5 => LaboSpawn,
                        6 => CommsSpawn,
                        7 => StorageSpawn,
                        8 => MeetingSpawnUnder,
                        9 => LocketSpawn,
                        10 => LeftReactorSpawn,
                        _ => InitialSpawnCenter,
                    };
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player.Data.PlayerId == playerId)
                        {
                            player.transform.position = loc;
                            break;
                        }
                    }
                }
            })));
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartEndGame))]
        class STARTENDGAME
        {
            static void Postfix()
            {
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class RPCHandlerPatch
        {
            static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                try
                {
                    byte packetId = callId;
                    switch ((CustomRPC)packetId)
                    {

                        // Main Controls
                        /*
                            case CustomRPC.TORVersionShare:
                             int majorTOR = reader.ReadPackedInt32();
                             int minorTOR = reader.ReadPackedInt32();
                             int patchTOR = reader.ReadPackedInt32();
                             int versionOwnerIdTOR = reader.ReadPackedInt32();
                             byte revisionTOR = 0xFF;
                             byte[] guidTOR;
                             revisionTOR = reader.ReadByte();
                             guidTOR = reader.ReadBytes(16);
                             CustomRPC.TORVersionShare(majorTOR, minorTOR, patchTOR, revisionTOR == 0xFF ? -1 : revisionTOR, guidTOR, versionOwnerIdTOR);
                            break;*/
                        case CustomRPC.ShareOptions:
                            ShareOptions((int)reader.ReadPackedUInt32(), reader);
                            break;
                        case CustomRPC.ShareSNRVersion:
                            byte major = reader.ReadByte();
                            byte minor = reader.ReadByte();
                            byte patch = reader.ReadByte();
                            int versionOwnerId = reader.ReadPackedInt32();
                            byte revision = 0xFF;
                            Guid guid;
                            if (reader.Length - reader.Position >= 17)
                            { // enough bytes left to read
                                revision = reader.ReadByte();
                                // GUID
                                byte[] gbytes = reader.ReadBytes(16);
                                guid = new Guid(gbytes);
                            }
                            else
                            {
                                guid = new Guid(new byte[16]);
                            }
                            ShareSNRversion(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                            break;
                        case CustomRPC.SetRole:
                            SetRole(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SheriffKill:
                            SheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case CustomRPC.MeetingSheriffKill:
                            MeetingSheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case CustomRPC.CustomRPCKill:
                            CustomRPCKill(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.RPCClergymanLightOut:
                            RPCClergymanLightOut(reader.ReadBoolean());
                            break;
                        case CustomRPC.ReportDeadBody:
                            ReportDeadBody(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.UncheckedMeeting:
                            UncheckedMeeting(reader.ReadByte());
                            break;
                        case CustomRPC.CleanBody:
                            CleanBody(reader.ReadByte());
                            break;
                        case CustomRPC.RPCMurderPlayer:
                            byte source = reader.ReadByte();
                            byte target = reader.ReadByte();
                            byte showAnimation = reader.ReadByte();
                            RPCMurderPlayer(source, target, showAnimation);
                            break;
                        case CustomRPC.ExiledRPC:
                            ExiledRPC(reader.ReadByte());
                            break;
                        case CustomRPC.ShareWinner:
                            ShareWinner(reader.ReadByte());
                            break;
                        case CustomRPC.TeleporterTP:
                            TeleporterTP(reader.ReadByte());
                            break;
                        case CustomRPC.SetQuarreled:
                            SetQuarreled(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SidekickPromotes:
                            SidekickPromotes();
                            break;
                        case CustomRPC.CreateSidekick:
                            CreateSidekick(reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case CustomRPC.SetSpeedBoost:
                            SetSpeedBoost(reader.ReadBoolean(), reader.ReadByte());
                            break;
                        case CustomRPC.ShareCosmetics:
                            ShareCosmetics(reader.ReadByte(), reader.ReadString());
                            break;
                        case CustomRPC.SetShareNamePlate:
                            SetShareNamePlate(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.AutoCreateRoom:
                            AutoCreateRoom();
                            break;
                        case CustomRPC.BomKillRPC:
                            BomKillRPC(reader.ReadByte());
                            break;
                        case CustomRPC.ByBomKillRPC:
                            ByBomKillRPC(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.NekomataExiledRPC:
                            NekomataExiledRPC(reader.ReadByte());
                            break;
                        case CustomRPC.CountChangerSetRPC:
                            CountChangerSetRPC(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetRoomTimerRPC:
                            SetRoomTimerRPC(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetScientistRPC:
                            SetScientistRPC(reader.ReadBoolean(), reader.ReadByte());
                            break;
                        case CustomRPC.ReviveRPC:
                            ReviveRPC(reader.ReadByte());
                            break;
                        case CustomRPC.SetHaison:
                            SetHaison();
                            break;
                        case CustomRPC.SetWinCond:
                            SetWinCond(reader.ReadByte());
                            break;
                        case CustomRPC.SetDetective:
                            SetDetective(reader.ReadByte());
                            break;
                        case CustomRPC.UseEraserCount:
                            UseEraserCount(reader.ReadByte());
                            break;
                        case CustomRPC.StartGameRPC:
                            StartGameRPC();
                            break;
                        case CustomRPC.UncheckedSetTasks:
                            uncheckedSetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
                            break;
                        case CustomRPC.SetLovers:
                            SetLovers(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetUseDevice:
                            SetUseDevice(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case CustomRPC.SetDeviceTime:
                            SetDeviceTime(reader.ReadSingle(), reader.ReadByte());
                            break;
                        case CustomRPC.UncheckedSetColor:
                            __instance.SetColor(reader.ReadByte());
                            break;
                        case CustomRPC.UncheckedSetVanilaRole:
                            UncheckedSetVanilaRole(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetMadKiller:
                            SetMadKiller(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetCustomSabotage:
                            SabotageManager.SetSabotage(ModHelpers.playerById(reader.ReadByte()), (SabotageManager.CustomSabotage)reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case CustomRPC.CustomEndGame:
                            if (AmongUsClient.Instance.AmHost)
                            {
                                MapUtilities.CachedShipStatus.enabled = false;
                                CustomEndGame((GameOverReason)reader.ReadByte(), reader.ReadBoolean());
                            }
                            break;
                        case CustomRPC.UncheckedProtect:
                            UncheckedProtect(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetBot:
                            SetBot(reader.ReadByte());
                            break;
                        case CustomRPC.DemonCurse:
                            DemonCurse(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SidekickSeerPromotes:
                            SidekickSeerPromotes();
                            break;
                        case CustomRPC.CreateSidekickSeer:
                            CreateSidekickSeer(reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case CustomRPC.ArsonistDouse:
                            ArsonistDouse(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetSpeedDown:
                            SetSpeedDown(reader.ReadBoolean());
                            break;
                        case CustomRPC.ShielderProtect:
                            ShielderProtect(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetShielder:
                            SetShielder(reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case CustomRPC.SetSpeedFreeze:
                            SetSpeedFreeze(reader.ReadBoolean());
                            break;
                        case CustomRPC.MakeVent:
                            MakeVent(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                            break;
                        case CustomRPC.PositionSwapperTP:
                            RPCProcedure.PositionSwapperTP(reader.ReadByte(), reader.ReadByte());
                            break;
                        /*
                    case CustomRPC.UseAdminTime:
                        UseAdminTime(reader.ReadSingle());
                        break;
                    case CustomRPC.UseCameraTime:
                        UseCameraTime(reader.ReadSingle());
                        break;
                    case CustomRPC.UseVitalsTime:
                        UseVitalTime(reader.ReadSingle());
                        break;
                        */
                        case CustomRPC.FixLights:
                            FixLights();
                            break;
                        case CustomRPC.RandomSpawn:
                            byte pId = reader.ReadByte();
                            byte locId = reader.ReadByte();
                            RPCProcedure.randomSpawn(pId, locId);
                            break;
                        case CustomRPC.KunaiKill:
                            KunaiKill(reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.SetSecretRoomTeleportStatus:
                            MapCustoms.Airship.SecretRoom.SetSecretRoomTeleportStatus((MapCustoms.Airship.SecretRoom.Status)reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                            break;
                        case CustomRPC.ChiefSidekick:
                            ChiefSidekick(reader.ReadByte());
                            break;
                    }
                }
                catch (Exception e)
                {
                    SuperNewRolesPlugin.Logger.LogInfo((CustomRPC)callId + "でエラー:" + e);
                }
            }
        }
    }
}
