using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using Hazel;
using InnerNet;
using Sentry;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapOption;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Sabotage;
using UnityEngine;
using static SuperNewRoles.Patches.FinalStatusPatch;

namespace SuperNewRoles.Modules;

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
    NiceGuesser,
    EvilGuesser,
    Vulture,
    NiceScientist,
    Clergyman,
    Madmate,
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
    Marlin,
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
    SatsumaAndImo,
    EvilButtoner,
    NiceButtoner,
    Finder,
    Revolutionist,
    Dictator,
    Spelunker,
    SuicidalIdeation,
    Hitman,
    Matryoshka,
    Nun,
    Psychometrist,
    SeeThroughPerson,
    PartTimer,
    Painter,
    Photographer,
    Stefinder,
    Stefinder1,
    Slugger,
    ShiftActor,
    ConnectKiller,
    GM,
    Cracker,
    WaveCannon,
    WaveCannonJackal,
    NekoKabocha,
    Doppelganger,
    Werewolf,
    Knight,
    Pavlovsdogs,
    Pavlovsowner,
    Conjurer,
    Camouflager,
    Cupid,
    HamburgerShop,
    Penguin,
    Dependents,
    LoversBreaker,
    Jumbo,
    Worshiper,
    Safecracker,
    FireFox,
    Squid,
    DyingMessenger,
    WiseMan,
    NiceMechanic,
    EvilMechanic,
    TheFirstLittlePig,
    TheSecondLittlePig,
    TheThirdLittlePig,
    OrientalShaman,
    ShermansServant,
    //RoleId
}

public enum CustomRPC
{
    ShareOptions = 145,
    ShareSNRVersion,
    SetRole,
    SetQuarreled,
    RPCClergymanLightOut,
    SheriffKill = 150,
    MeetingSheriffKill,
    CustomRPCKill,
    ReportDeadBody,
    UncheckedMeeting,
    CleanBody,
    ExiledRPC,
    RPCMurderPlayer,
    ShareWinner,
    TeleporterTP,
    SidekickPromotes = 160,
    CreateSidekick,
    CreateSidekickSeer,
    SetSpeedBoost,
    AutoCreateRoom,
    CountChangerSetRPC,
    SetRoomTimerRPC,
    SetScientistRPC,
    ReviveRPC,
    SetHaison = 170,
    SetWinCond,
    SetDetective,
    UseEraserCount,
    StartGameRPC,
    UncheckedSetTasks,
    SetLovers,
    SetDeviceTime,
    UncheckedSetColor,
    UncheckedSetVanillaRole,
    SetMadKiller = 180,
    SetCustomSabotage,
    UseStuntmanCount,
    UseMadStuntmanCount,
    CustomEndGame,
    UncheckedProtect,
    SetBot,
    DemonCurse,
    ArsonistDouse,
    SetSpeedDown,
    ShielderProtect = 190,
    SetShielder,
    SetSpeedFreeze,
    MakeVent,
    PositionSwapperTP,
    FixLights,
    RandomSpawn,
    KunaiKill,
    SetSecretRoomTeleportStatus,
    ChiefSidekick,
    RpcSetDoorway = 200,
    StartRevolutionMeeting,
    UncheckedUsePlatform,
    BlockReportDeadBody,
    PartTimerSet,
    SetMatryoshkaDeadbody,
    StefinderIsKilled,
    PlayPlayerAnimation,
    SluggerExile,
    PainterPaintSet,
    SharePhotograph = 210,
    PainterSetTarget = 220,
    SetFinalStatus,
    MeetingKill,
    KnightProtected,
    KnightProtectClear,
    GuesserShoot,
    WaveCannon,
    ShowFlash,
    PavlovsOwnerCreateDog,
    CrackerCrack,
    Camouflage = 230,
    ShowGuardEffect,
    SetLoversCupid,
    PenguinHikizuri,
    SetVampireStatus,
    SyncDeathMeeting,
    SetDeviceUseStatus,
    SetLoversBreakerWinner,
    RPCTeleport,
    SafecrackerGuardCount,
    SetVigilance = 240,
    Chat,
    SetWiseManStatus,
    SetVentStatusMechanic,
    SetTheThreeLittlePigsTeam,
    UseTheThreeLittlePigsCount,
    SetOutfit,
    CreateShermansServant,
    SetVisible,
}

public static class RPCProcedure
{
    public static void SetWiseManStatus(byte sourceId, float rotate, bool Is)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        WiseMan.SetWiseManStatus(source, rotate, Is);
    }
    public static void SetVentStatusMechanic(byte sourceplayer, byte targetvent, bool Is, byte[] buff)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceplayer);
        Vent vent = ModHelpers.VentById(targetvent);
        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        position.z = BitConverter.ToSingle(buff, 2 * sizeof(float));
        NiceMechanic.SetVentStatusMechanic(source, vent, Is, position);
    }
    public static void SetVisible(byte id, bool visible)
    {
        PlayerControl player = ModHelpers.PlayerById(id);
        if (player == null) return;
        player.Visible = visible;
    }
    public static void CreateShermansServant(byte OrientalShamanId, byte ShermansServantId)
    {
        PlayerControl OrientalShamanPlayer = ModHelpers.PlayerById(OrientalShamanId);
        PlayerControl ShermansServantIPlayer = ModHelpers.PlayerById(ShermansServantId);
        if (!OrientalShamanPlayer && !ShermansServantIPlayer) return;
        OrientalShaman.OrientalShamanCausative.Add(OrientalShamanId, ShermansServantId);
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(ShermansServantIPlayer, RoleTypes.Crewmate);
    }
    public static void SetOutfit(byte id, int color, string hat, string pet, string skin, string visor, string name)
    {
        PlayerControl player = ModHelpers.PlayerById(id);
        if (player == null) return;
        GameData.PlayerOutfit outfit = new()
        {
            ColorId = color,
            HatId = hat,
            PetId = pet,
            SkinId = skin,
            VisorId = visor,
            PlayerName = name
        };
        player.setOutfit(outfit);
    }
    public static void UseTheThreeLittlePigsCount(byte id)
    {
        PlayerControl player = ModHelpers.PlayerById(id);
        if (player == null) return;
        if (player.IsRole(RoleId.TheSecondLittlePig))
        {
            if (!TheThreeLittlePigs.TheSecondLittlePig.GuardCount.ContainsKey(player.PlayerId))
                TheThreeLittlePigs.TheSecondLittlePig.GuardCount[player.PlayerId] = TheThreeLittlePigs.TheSecondLittlePigMaxGuardCount.GetInt() - 1;
            else TheThreeLittlePigs.TheSecondLittlePig.GuardCount[player.PlayerId]--;
        }
        else if (player.IsRole(RoleId.TheThirdLittlePig))
        {
            if (!TheThreeLittlePigs.TheThirdLittlePig.CounterCount.ContainsKey(player.PlayerId))
                TheThreeLittlePigs.TheThirdLittlePig.CounterCount[player.PlayerId] = TheThreeLittlePigs.TheThirdLittlePigMaxCounterCount.GetInt() - 1;
            else TheThreeLittlePigs.TheThirdLittlePig.CounterCount[player.PlayerId]--;
        }
    }
    public static void SetTheThreeLittlePigsTeam(byte first, byte second, byte third)
    {
        PlayerControl firstPlayer = ModHelpers.PlayerById(first);
        PlayerControl secondPlayer = ModHelpers.PlayerById(second);
        PlayerControl thirdPlayer = ModHelpers.PlayerById(third);
        if (firstPlayer == null || secondPlayer == null || thirdPlayer == null) return;
        List<PlayerControl> theThreeLittlePigsPlayer = new()
        {
            firstPlayer,
            secondPlayer,
            thirdPlayer
        };
        TheThreeLittlePigs.TheThreeLittlePigsPlayer.Add(theThreeLittlePigsPlayer);
    }
    public static void Chat(byte id, string text)
    {
        PlayerControl player = ModHelpers.PlayerById(id);
        if (player == null) return;
        bool isDead = player.IsDead();
        Logger.Info($"{player.Data.PlayerName}が発言します。元のIsDead : {isDead}", "RPC Chat");
        player.Data.IsDead = false;
        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(player, text);
        player.Data.IsDead = isDead;
        if (isDead != player.Data.IsDead) Logger.Error($"{player.Data.PlayerName}のIsDeadが正常に戻りませんでした。元のIsDead : {isDead}, 現在のIsDead : {player.Data.IsDead}", "RPC Chat");
    }
    public static void SetVigilance(bool isVigilance, byte id)
    {
        PlayerControl player = ModHelpers.PlayerById(id);
        if (player == null) return;
        if (Squid.IsVigilance.ContainsKey(id) && Squid.IsVigilance[id] && player.AmOwner && !isVigilance)
        {
            Squid.ResetCooldown();
            Logger.Info("イカの警戒が解けたためクールをリセットしました");
        }
        Squid.IsVigilance[id] = isVigilance;
    }
    public static void SafecrackerGuardCount(byte id, bool isKillGuard)
    {
        PlayerControl player = ModHelpers.PlayerById(id);
        if (player == null) return;
        if (isKillGuard)
        {
            if (Safecracker.KillGuardCount.ContainsKey(id))
                Safecracker.KillGuardCount[id] -= 1;
            else Safecracker.KillGuardCount[id] = Safecracker.SafecrackerMaxKillGuardCount.GetInt() - 1;
        }
        else
        {
            if (Safecracker.ExiledGuardCount.ContainsKey(id))
                Safecracker.ExiledGuardCount[id] -= 1;
            else Safecracker.ExiledGuardCount[id] = Safecracker.SafecrackerMaxExiledGuardCount.GetInt() - 1;
        }
    }
    public static void SetDeviceUseStatus(byte devicetype, byte playerId, bool Is, string time)
    {
        DeviceClass.DeviceType type = (DeviceClass.DeviceType)devicetype;
        PlayerControl player = ModHelpers.PlayerById(playerId);
        if (player == null) return;
        if (!DeviceClass.DeviceUsePlayer.ContainsKey(type)) return;
        if (Is)
        {
            DateTime dateTime;

            if (!DateTime.TryParse(time, out dateTime)) return;
            if (DeviceClass.DeviceUserUseTime[type] < dateTime && DeviceClass.DeviceUsePlayer[type] != null) return;
            DeviceClass.DeviceUsePlayer[type] = player;
            DeviceClass.DeviceUserUseTime[type] = dateTime;
        }
        else
        {
            if (DeviceClass.DeviceUsePlayer[type] != null && DeviceClass.DeviceUsePlayer[type].PlayerId != playerId) return;
            DeviceClass.DeviceUsePlayer[type] = null;
            DeviceClass.DeviceUserUseTime[type] = DateTime.UtcNow;
        }
    }
    public static void SetDeviceTime(byte devicetype, float time)
    {
        DeviceClass.DeviceType type = (DeviceClass.DeviceType)devicetype;
        switch (type)
        {
            case DeviceClass.DeviceType.Admin:
                DeviceClass.AdminTimer = time;
                break;
            case DeviceClass.DeviceType.Camera:
                DeviceClass.CameraTimer = time;
                break;
            case DeviceClass.DeviceType.Vital:
                DeviceClass.VitalTimer = time;
                break;
        }
    }
    public static void SetLoversCupid(byte sourceid, byte player1, byte player2)
    {
        RoleClass.Cupid.CupidLoverPair[sourceid] = player1;
        SetLovers(player1, player2);
    }

    public static void SetVampireStatus(byte sourceId, byte targetId, bool IsOn, bool IsKillSuc)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        PlayerControl target = ModHelpers.PlayerById(targetId);
        if (source == null || target == null) return;
        if (IsOn)
        {
            RoleClass.Vampire.Targets.Add(source, target);
        }
        else
        {
            if (RoleClass.Vampire.BloodStains.ContainsKey(target.PlayerId))
            {
                if (IsKillSuc)
                {
                    BloodStain DeadBloodStain = new(target, target.transform.position);
                    DeadBloodStain.BloodStainObject.transform.localScale *= 3f;
                    RoleClass.Vampire.WaitActiveBloodStains.AddRange(RoleClass.Vampire.BloodStains[target.PlayerId]);
                    RoleClass.Vampire.WaitActiveBloodStains.Add(DeadBloodStain);
                }
                RoleClass.Vampire.BloodStains.Remove(target.PlayerId);
                RoleClass.Vampire.Targets.Remove(source);
            }
        }
    }

    public static void PenguinHikizuri(byte sourceId, byte targetId)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        PlayerControl target = ModHelpers.PlayerById(targetId);
        if (source == null || target == null) return;
        RoleClass.Penguin.PenguinData.Add(source, target);
    }

    public static void ShowGuardEffect(byte showerid, byte targetid)
    {
        if (showerid != CachedPlayer.LocalPlayer.PlayerId) return;
        PlayerControl target = ModHelpers.PlayerById(targetid);
        if (target == null) return;
        PlayerControl.LocalPlayer.ProtectPlayer(target, 0);
        PlayerControl.LocalPlayer.MurderPlayer(target);
    }
    public static void KnightProtectClear(byte Target)
    {
        Knight.GuardedPlayers.Remove(Target);
    }
    public static void SyncDeathMeeting(byte TargetId)
    {
        if (!MeetingHud.Instance) return;

        PlayerControl dyingTarget = ModHelpers.PlayerById(TargetId);
        if (dyingTarget == null) return;

        if (dyingTarget.IsAlive())
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == TargetId)
                {
                    pva.SetDead(pva.DidReport, false);
                    pva.Overlay.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == TargetId)
                {
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                }
                if (pva.VotedFor != TargetId) continue;
                pva.UnsetVote();
                var voteAreaPlayer = ModHelpers.PlayerById(pva.TargetPlayerId);
                if (!voteAreaPlayer.AmOwner) continue;
                MeetingHud.Instance.ClearVote();
            }
            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }
    }
    public static void MeetingKill(byte SourceId, byte TargetId)
    {
        PlayerControl source = ModHelpers.PlayerById(SourceId);
        PlayerControl target = ModHelpers.PlayerById(TargetId);
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
        if (source == null || target == null) return;
        target.Exiled();
        FinalStatusData.FinalStatuses[source.PlayerId] = FinalStatus.Kill;
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == TargetId)
                {
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                }
                if (pva.VotedFor != TargetId) continue;
                pva.UnsetVote();
                var voteAreaPlayer = ModHelpers.PlayerById(pva.TargetPlayerId);
                if (!voteAreaPlayer.AmOwner) continue;
                MeetingHud.Instance.ClearVote();
            }
            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }
        if (CachedPlayer.LocalPlayer.PlayerId == target.PlayerId)
        {
            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(target.Data, source.Data);
        }
    }

    public static void PavlovsOwnerCreateDog(byte sourceid, byte targetid, bool IsSelfDeath)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceid);
        PlayerControl target = ModHelpers.PlayerById(targetid);
        if (source == null || target == null) return;
        if (IsSelfDeath)
        {
            source.MurderPlayer(source);
        }
        else
        {
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(target, RoleTypes.Crewmate);
            SetRole(targetid, (byte)RoleId.Pavlovsdogs);
            if (!RoleClass.Pavlovsowner.CountData.ContainsKey(sourceid))
            {
                RoleClass.Pavlovsowner.CountData[sourceid] = CustomOptionHolder.PavlovsownerCreateDogLimit.GetInt();
            }
            RoleClass.Pavlovsowner.CountData[sourceid]--;
        }
    }
    public static void Camouflage(bool Is)
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (Is) Roles.Impostor.Camouflager.CamouflageSHR();
                else Roles.Impostor.Camouflager.ResetCamouflageSHR();
            }
        }
        else
        {
            if (Is) Roles.Impostor.Camouflager.Camouflage();
            else Roles.Impostor.Camouflager.ResetCamouflage();
        }
    }

    public static void GuesserShoot(byte killerId, byte dyingTargetId, byte guessedTargetId, byte guessedRoleId)
    {
        PlayerControl dyingTarget = ModHelpers.PlayerById(dyingTargetId);
        if (dyingTarget == null) return;
        dyingTarget.Exiled();
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(dyingTarget.KillSfx, false, 0.8f);
        if (MeetingHud.Instance)
        {
            foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
            {
                if (pva.TargetPlayerId == dyingTargetId)
                {
                    pva.SetDead(pva.DidReport, true);
                    pva.Overlay.gameObject.SetActive(true);
                }
                if (pva.VotedFor != dyingTargetId) continue;
                pva.UnsetVote();
                var voteAreaPlayer = ModHelpers.PlayerById(pva.TargetPlayerId);
                if (!voteAreaPlayer.AmOwner) continue;
                MeetingHud.Instance.ClearVote();
            }
            if (AmongUsClient.Instance.AmHost)
                MeetingHud.Instance.CheckForEndVoting();
        }
        PlayerControl guesser = ModHelpers.PlayerById(killerId);
        if (FastDestroyableSingleton<HudManager>.Instance != null && guesser != null)
            if (CachedPlayer.LocalPlayer.PlayerControl == dyingTarget)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(guesser.Data, dyingTarget.Data);
                if (Roles.Attribute.Guesser.guesserUI != null) Roles.Attribute.Guesser.ExitButton.OnClick.Invoke();
            }
    }

    public static void CrackerCrack(byte Target)
    {
        if (!RoleClass.Cracker.CrackedPlayers.Contains(Target)) RoleClass.Cracker.CrackedPlayers.Add(Target);
    }

    public static WaveCannonObject WaveCannon(byte Type, byte Id, bool IsFlipX, byte OwnerId, byte[] buff)
    {
        Logger.Info($"{(WaveCannonObject.RpcType)Type} : {Id} : {IsFlipX} : {OwnerId} : {buff.Length} : {(ModHelpers.PlayerById(OwnerId) == null ? -1 : ModHelpers.PlayerById(OwnerId).Data.PlayerName)}", "RpcWaveCannon");
        switch ((WaveCannonObject.RpcType)Type)
        {
            case WaveCannonObject.RpcType.Spawn:
                Vector3 position = Vector3.zero;
                position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
                position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
                return new(position, IsFlipX, ModHelpers.PlayerById(OwnerId));
            case WaveCannonObject.RpcType.Shoot:
                WaveCannonObject.Objects.FirstOrDefault(x => x.Owner != null && x.Owner.PlayerId == OwnerId && x.Id == Id).Shoot();
                break;
        }
        return null;
    }
    public static void SetFinalStatus(byte targetId, FinalStatus Status)
    {
        FinalStatusData.FinalStatuses[targetId] = Status;
    }

    public static void SluggerExile(byte SourceId, List<byte> Targets)
    {
        Logger.Info("～SluggerExile～");
        PlayerControl Source = SourceId.GetPlayerControl();
        if (Source == null) return;
        Logger.Info("Source突破");
        foreach (byte target in Targets)
        {
            PlayerControl Player = target.GetPlayerControl();
            Logger.Info($"{target}はnullか:{Player == null}");
            if (Player == null) continue;
            Player.Exiled();
            new SluggerDeadbody().Start(Source.PlayerId, Player.PlayerId, Source.transform.position - Player.transform.position);
        }
    }
    public static void PlayPlayerAnimation(byte playerid, byte type)
    {
        RpcAnimationType AnimType = (RpcAnimationType)type;
        PlayerAnimation PlayerAnim = PlayerAnimation.GetPlayerAnimation(playerid);
        if (PlayerAnim == null) return;
        PlayerAnim.HandleAnim(AnimType);
    }
    public static void PainterSetTarget(byte target, bool Is)
    {
        if (target == CachedPlayer.LocalPlayer.PlayerId) RoleClass.Painter.IsLocalActionSend = Is;
    }
    public static void PainterPaintSet(byte target, byte ActionTypeId, byte[] buff)
    {
        Painter.ActionType type = (Painter.ActionType)ActionTypeId;
        if (!RoleClass.Painter.ActionData.ContainsKey(type)) return;
        if (!PlayerControl.LocalPlayer.IsRole(RoleId.Painter)) return;
        if (RoleClass.Painter.CurrentTarget == null || RoleClass.Painter.CurrentTarget.PlayerId != target) return;
        Vector2 position = Vector2.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        RoleClass.Painter.ActionData[type].Add(position);
    }

    public static void BlockReportDeadBody(byte TargetId, bool IsChangeReported)
    {
        if (IsChangeReported)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == TargetId)
                {
                    array[i].Reported = true;
                    return;
                }
            }
        }
        else
        {
            RoleClass.BlockPlayers.Add(TargetId);
        }
    }
    public static void SharePhotograph()
    {
        if (!RoleClass.Photographer.IsPhotographerShared)
        {
            ProctedMessager.ScheduleProctedMessage(ModTranslation.GetString("PhotographerPhotograph"));
        }
        RoleClass.Photographer.IsPhotographerShared = true;
    }
    public static void SetMatryoshkaDeadBody(byte sourceid, byte targetid, bool Is)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceid);
        PlayerControl target = ModHelpers.PlayerById(targetid);
        if (source == null) return;
        Roles.Impostor.Matryoshka.Set(source, target, Is);
    }
    public static void PartTimerSet(byte playerid, byte targetid)
    {
        PlayerControl source = ModHelpers.PlayerById(playerid);
        if (source == null) return;
        RoleClass.PartTimer.Data[source.PlayerId] = targetid;
    }
    public static void UncheckedUsePlatform(byte playerid, bool IsMove)
    {
        PlayerControl source = ModHelpers.PlayerById(playerid);
        AirshipStatus airshipStatus = GameObject.FindObjectOfType<AirshipStatus>();
        if (airshipStatus)
        {
            if (IsMove)
            {
                if (source == null) return;
                airshipStatus.GapPlatform.Use(source);
            }
            else
            {
                airshipStatus.GapPlatform.StopAllCoroutines();
                airshipStatus.GapPlatform.StartCoroutine(Roles.Impostor.Nun.NotMoveUsePlatform(airshipStatus.GapPlatform));
            }
        }
    }
    public static void StefinderIsKilled(byte PlayerId)
        => RoleClass.Stefinder.IsKillPlayer.Add(PlayerId);

    public static void StartRevolutionMeeting(byte sourceid)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceid);
        if (source == null) return;
        RoleClass.Revolutionist.MeetingTrigger = source;
        source.ReportDeadBody(null);
    }

    public static void KunaiKill(byte sourceid, byte targetid)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceid);
        PlayerControl target = ModHelpers.PlayerById(targetid);
        if (source == null || target == null) return;
        RPCMurderPlayer(sourceid, targetid, 0);
        SetFinalStatus(targetid, FinalStatus.KunaiKill);

        if (targetid == CachedPlayer.LocalPlayer.PlayerId)
        {
            FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(target.Data, source.Data);
        }
    }

    public static void ChiefSidekick(byte targetid, bool IsTaskClear)
    {
        RoleClass.Chief.SheriffPlayer.Add(targetid);
        if (IsTaskClear)
        {
            RoleClass.Chief.NoTaskSheriffPlayer.Add(targetid);
        }
        SetRole(targetid, (byte)RoleId.Sheriff);
        if (targetid == CachedPlayer.LocalPlayer.PlayerId)
        {
            Sheriff.ResetKillCooldown();
            RoleClass.Sheriff.KillMaxCount = RoleClass.Chief.KillLimit;
        }
        UncheckedSetVanillaRole(targetid, (byte)RoleTypes.Crewmate);
    }
    public static void FixLights()
    {
        SwitchSystem switchSystem = MapUtilities.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
        switchSystem.ActualSwitches = switchSystem.ExpectedSwitches;
    }
    public static void ArsonistDouse(byte source, byte target)
    {
        PlayerControl TargetPlayer = ModHelpers.PlayerById(target);
        PlayerControl SourcePlayer = ModHelpers.PlayerById(source);
        if (TargetPlayer == null || SourcePlayer == null) return;
        if (!RoleClass.Arsonist.DouseData.ContainsKey(source)) RoleClass.Arsonist.DouseData[source] = new();
        if (!Arsonist.IsDoused(SourcePlayer, TargetPlayer))
        {
            RoleClass.Arsonist.DouseData[source].Add(TargetPlayer);
        }
    }
    public static void DemonCurse(byte source, byte target)
    {
        PlayerControl TargetPlayer = ModHelpers.PlayerById(target);
        PlayerControl SourcePlayer = ModHelpers.PlayerById(source);
        if (TargetPlayer == null || SourcePlayer == null) return;
        if (!RoleClass.Demon.CurseData.ContainsKey(source)) RoleClass.Demon.CurseData[source] = new();
        if (!Demon.IsCursed(SourcePlayer, TargetPlayer))
        {
            RoleClass.Demon.CurseData[source].Add(TargetPlayer);
        }
    }
    public static void SetBot(byte playerid)
    {
        SuperNewRolesPlugin.Logger.LogInfo("セットボット！！！！！！！！！");
        PlayerControl player = ModHelpers.PlayerById(playerid);
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
        PlayerControl player = ModHelpers.PlayerById(playerid);
        PlayerControl source = ModHelpers.PlayerById(sourceid);
        if (player == null || source == null) return;
        source.ProtectPlayer(player, colorid);
    }
    public static void CustomEndGame(GameOverReason reason, bool showAd)
        => CheckGameEndPatch.CustomEndGame(reason, showAd);

    public static void UseStuntmanCount(byte playerid)
    {
        var player = ModHelpers.PlayerById(playerid);
        if (player == null) return;
        if (player.IsRole(RoleId.MadStuntMan))
        {
            if (!RoleClass.MadStuntMan.GuardCount.ContainsKey(playerid))
            {
                RoleClass.MadStuntMan.GuardCount[playerid] = CustomOptionHolder.MadStuntManMaxGuardCount.GetInt() - 1;
            }
            else
            {
                RoleClass.MadStuntMan.GuardCount[playerid]--;
            }
        }
        else if (player.IsRole(RoleId.StuntMan))
        {
            if (!RoleClass.StuntMan.GuardCount.ContainsKey(playerid))
            {
                RoleClass.StuntMan.GuardCount[playerid] = CustomOptionHolder.StuntManMaxGuardCount.GetInt() - 1;
            }
            else
            {
                RoleClass.StuntMan.GuardCount[playerid]--;
            }
        }
    }
    public static void SetMadKiller(byte sourceid, byte targetid)
    {
        var source = ModHelpers.PlayerById(sourceid);
        var target = ModHelpers.PlayerById(targetid);
        if (source == null || target == null) return;
        target.ClearRole();
        RoleClass.SideKiller.MadKillerPlayer.Add(target);
        RoleClass.SideKiller.MadKillerPair.Add(source.PlayerId, target.PlayerId);
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(target, RoleTypes.Crewmate);
        ChacheManager.ResetMyRoleChache();
        PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
    }
    public static void UncheckedSetVanillaRole(byte playerid, byte roletype)
    {
        var player = ModHelpers.PlayerById(playerid);
        if (player == null) return;
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, (RoleTypes)roletype);
        player.Data.Role.Role = (RoleTypes)roletype;
    }

    public static void UncheckedSetTasks(byte playerId, byte[] taskTypeIds)
    {
        var player = ModHelpers.PlayerById(playerId);
        player.ClearAllTasks();
        GameData.Instance.SetTasks(playerId, taskTypeIds);
    }
    public static void StartGameRPC()
        => RoleClass.ClearAndReloadRoles();

    public static void UseEraserCount(byte playerid)
    {
        PlayerControl p = ModHelpers.PlayerById(playerid);
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
            catch (Exception ex) { Logger.Info($"{ex}", "AutoCreateRoom"); }
            AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameid);
        }
    }
    public static void SetHaison()
        => EndGameManagerSetUpPatch.IsHaison = true;

    public static void SetRoomTimerRPC(byte min, byte seconds)
        => ShareGameVersion.timer = (min * 60) + seconds;

    public static void CountChangerSetRPC(byte sourceid, byte targetid)
    {
        var source = ModHelpers.PlayerById(sourceid);
        var target = ModHelpers.PlayerById(targetid);
        if (source == null || target == null) return;
        if (CustomOptionHolder.CountChangerNextTurn.GetBool())
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
        var player = ModHelpers.PlayerById(playerid);
        if (player == null) return;
        Mode.Detective.Main.DetectivePlayer = player;
    }
    public static void ShareOptions(int numberOfOptions, MessageReader reader)
    {
        try
        {
            for (int i = 0; i < numberOfOptions; i++)
            {
                uint optionId = reader.ReadPackedUInt32();
                uint selection = reader.ReadPackedUInt32();
                CustomOption option = CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                option.UpdateSelection((int)selection);
            }
        }
        catch (Exception e)
        {
            SuperNewRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
        }
    }
    public static void ShareSNRversion(int major, int minor, int build, int revision, Guid guid, int clientId)
    {
        Version ver;
        if (revision < 0)
            ver = new(major, minor, build);
        else
            ver = new(major, minor, build, revision);
        ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers[clientId] = new PlayerVersion(ver, guid);
    }
    public static void SetRole(byte playerid, byte RPCRoleId)
    {
        var player = ModHelpers.PlayerById(playerid);
        var roleId = (RoleId)RPCRoleId;
        if (!roleId.IsGhostRole())
        {
            player.ClearRole();
        }
        player.SetRole(roleId);
    }

    public static void SetQuarreled(byte playerid1, byte playerid2)
        => RoleHelpers.SetQuarreled(ModHelpers.PlayerById(playerid1), ModHelpers.PlayerById(playerid2));

    public static void SetLovers(byte playerid1, byte playerid2)
        => RoleHelpers.SetLovers(ModHelpers.PlayerById(playerid1), ModHelpers.PlayerById(playerid2));

    /// <summary>
    /// Sheriffのキルを制御
    /// </summary>
    /// <param name="SheriffId">SheriffのPlayerId</param>
    /// <param name="TargetId">Sheriffのターゲットにされた人のPlayerId</param>
    /// <param name="MissFire">誤爆したか</param>
    /// <param name="alwaysKill">誤爆していて尚且つ誤爆時も対象を殺す設定が有効か</param>
    public static void SheriffKill(byte SheriffId, byte TargetId, bool MissFire, bool alwaysKill)
    {
        PlayerControl sheriff = ModHelpers.PlayerById(SheriffId);
        PlayerControl target = ModHelpers.PlayerById(TargetId);
        if (sheriff == null || target == null) return;

        if (alwaysKill)
        {
            sheriff.MurderPlayer(target);
            sheriff.MurderPlayer(sheriff);
        }
        else if (MissFire)
        {
            sheriff.MurderPlayer(sheriff);
        }
        else
        {
            if (sheriff.IsRole(RoleId.RemoteSheriff) && !RoleClass.RemoteSheriff.IsKillTeleport)
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
        }
    }

    /// <summary>
    /// MeetingSheriffのキルを制御
    /// </summary>
    /// <param name="SheriffId">SheriffのPlayerId</param>
    /// <param name="TargetId">Sheriffのターゲットにされた人のPlayerId</param>
    /// <param name="MissFire">誤爆したか</param>
    /// <param name="alwaysKill">誤爆していて尚且つ誤爆時も対象を殺す設定が有効か</param>
    public static void MeetingSheriffKill(byte SheriffId, byte TargetId, bool MissFire, bool alwaysKill)
    {
        PlayerControl sheriff = ModHelpers.PlayerById(SheriffId);
        PlayerControl target = ModHelpers.PlayerById(TargetId);
        if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
        if (sheriff == null || target == null) return;
        if (!PlayerControl.LocalPlayer.IsAlive())
        {
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, string.Format(ModTranslation.GetString("MeetingSheriffkillChat1"), target.name, sheriff.name));
            if (alwaysKill)
            {
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, string.Format(ModTranslation.GetString("MeetingSheriffkillChat2"), target.name, sheriff.name));
            }
            if (MissFire)
            {
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, string.Format(ModTranslation.GetString("MeetingSheriffkillChat3"), sheriff.name));
            }
            else
            {
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, string.Format(ModTranslation.GetString("MeetingSheriffkillChat4"), sheriff.name));
            }
        }
        if (alwaysKill)
        {
            target.Exiled();
            if (PlayerControl.LocalPlayer == target)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(sheriff.Data, target.Data);
            }
            sheriff.Exiled();
            if (PlayerControl.LocalPlayer == sheriff)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(sheriff.Data, sheriff.Data);
            }
        }
        else if (MissFire)
        {
            sheriff.Exiled();
            if (PlayerControl.LocalPlayer == sheriff)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(sheriff.Data, sheriff.Data);
            }
        }
        else
        {
            target.Exiled();
            if (PlayerControl.LocalPlayer == target)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(sheriff.Data, target.Data);
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
                else if (pva.TargetPlayerId == TargetId && alwaysKill)
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

    public static void KnightProtected(byte KnightId, byte TargetId)
    {
        PlayerControl Knight = ModHelpers.PlayerById(KnightId);
        PlayerControl Target = ModHelpers.PlayerById(TargetId);
        Roles.Crewmate.Knight.GuardedPlayers.Add(TargetId); // 守護をかけられたプレイヤーを保存。
        SuperNewRolesPlugin.Logger.LogInfo($"[KnightProtected]{Knight.GetDefaultName()}が{Target.GetDefaultName()}に護衛を使用しました。");
        if (Roles.Crewmate.Knight.KnightCanAnnounceOfProtected.GetBool()) ProctedMessager.ScheduleProctedMessage(ModTranslation.GetString("TheKnightProtected"));
    }
    public static void CustomRPCKill(byte notTargetId, byte targetId)
    {
        if (notTargetId == targetId)
        {
            PlayerControl Player = ModHelpers.PlayerById(targetId);
            Player.MurderPlayer(Player);
        }
        else
        {
            PlayerControl notTargetPlayer = ModHelpers.PlayerById(notTargetId);
            PlayerControl TargetPlayer = ModHelpers.PlayerById(targetId);
            notTargetPlayer.MurderPlayer(TargetPlayer);
        }
    }
    public static void RPCClergymanLightOut(bool Start)
    {
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
        if (Start)
        {
            Clergyman.LightOutStartRPC();
        }
        else
        {
            if (RoleClass.Clergyman.currentMessage?.text != null)
            {
                GameObject.Destroy(RoleClass.Clergyman.currentMessage.text.gameObject);
            }
            RoleClass.Clergyman.IsLightOff = false;
        }
    }

    public static void SetSpeedBoost(bool Is, byte id)
    {
        var player = ModHelpers.PlayerById(id);
        if (player == null) return;
        if (player.Data.Role.IsImpostor) RoleClass.EvilSpeedBooster.IsBoostPlayers[id] = Is;
        else if (player.IsRole(RoleId.Squid))
        {
            Squid.Abilitys.IsBoostSpeed = Is;
            Squid.Abilitys.BoostSpeedTimer = Squid.SquidBoostSpeedTime.GetFloat();
        }
        else RoleClass.SpeedBooster.IsBoostPlayers[id] = Is;
    }
    public static void ReviveRPC(byte playerid)
    {
        var player = ModHelpers.PlayerById(playerid);
        if (player == null) return;
        player.Revive();
        FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, player.IsImpostor() ? RoleTypes.Impostor : RoleTypes.Crewmate);
        DeadPlayer.deadPlayers?.RemoveAll(x => x.player?.PlayerId == playerid);
        FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.Alive;
    }
    public static void SetScientistRPC(bool Is, byte id)
        => RoleClass.NiceScientist.IsScientistPlayers[id] = Is;

    public static void ReportDeadBody(byte sourceId, byte targetId)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        PlayerControl target = ModHelpers.PlayerById(targetId);
        if (source != null && target != null) source.ReportDeadBody(target.Data);
    }
    public static void UncheckedMeeting(byte sourceId)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
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
    public static void SidekickPromotes(bool isJackalSeer)
    {
        if (isJackalSeer)
        {
            foreach (PlayerControl p in RoleClass.JackalSeer.SidekickSeerPlayer.ToArray())
            {
                p.ClearRole();
                p.SetRole(RoleId.JackalSeer);
                //無限サイドキック化の設定の取得(CanCreateSidekickにfalseが代入されると新ジャッカルにSKボタンが表示されなくなる)
                RoleClass.JackalSeer.CanCreateSidekick = CustomOptionHolder.JackalSeerNewJackalCreateSidekick.GetBool();
            }
        }
        else
        {
            foreach (PlayerControl p in RoleClass.Jackal.SidekickPlayer.ToArray())
            {
                p.ClearRole();
                p.SetRole(RoleId.Jackal);
                //無限サイドキック化の設定の取得(CanCreateSidekickにfalseが代入されると新ジャッカルにSKボタンが表示されなくなる)
                RoleClass.Jackal.CanCreateSidekick = CustomOptionHolder.JackalNewJackalCreateSidekick.GetBool();
            }
        }
        PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
        ChacheManager.ResetMyRoleChache();
    }
    public static void CreateSidekick(byte playerid, bool IsFake)
    {
        var player = ModHelpers.PlayerById(playerid);
        if (player == null) return;
        if (IsFake)
        {
            RoleClass.Jackal.FakeSidekickPlayer.Add(player);
        }
        else
        {
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            player.ClearRole();
            RoleClass.Jackal.SidekickPlayer.Add(player);
            PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
            ChacheManager.ResetMyRoleChache();
        }
    }
    public static void CreateSidekickSeer(byte playerid, bool IsFake)
    {
        var player = ModHelpers.PlayerById(playerid);
        if (player == null) return;
        if (IsFake)
        {
            RoleClass.JackalSeer.FakeSidekickSeerPlayer.Add(player);
        }
        else
        {
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            player.ClearRole();
            RoleClass.JackalSeer.SidekickSeerPlayer.Add(player);
            PlayerControlHelper.RefreshRoleDescription(PlayerControl.LocalPlayer);
            ChacheManager.ResetMyRoleChache();
        }
    }
    public static void ExiledRPC(byte playerid)
    {
        var player = ModHelpers.PlayerById(playerid);
        if (player != null)
        {
            player.Data.IsDead = true;
            player.Exiled();
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
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        PlayerControl target = ModHelpers.PlayerById(targetId);
        if (source != null && target != null)
        {
            if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
            source.MurderPlayer(target);
        }
    }
    public static void ShareWinner(byte playerid)
    {
        PlayerControl player = ModHelpers.PlayerById(playerid);
        if (ModeHandler.IsMode(ModeId.BattleRoyal))
        {
            Mode.BattleRoyal.Main.Winners.Add(player);
        }
        else
        {
            OnGameEndPatch.WinnerPlayer = player;
        }
    }
    public static void TeleporterTP(byte playerid)
    {
        var p = ModHelpers.PlayerById(playerid);
        CachedPlayer.LocalPlayer.transform.position = p.transform.position;
        new CustomMessage(string.Format(ModTranslation.GetString("TeleporterTPTextMessage"), p.NameText().text), 3);
    }
    public static void SetWinCond(byte Cond)
        => OnGameEndPatch.EndData = (CustomGameOverReason)Cond;

    public static void SetSpeedDown(bool Is)
        => RoleClass.Speeder.IsSpeedDown = Is;

    public static void SetSpeedFreeze(bool Is)
        => RoleClass.Freezer.IsSpeedDown = Is;

    public static void ShielderProtect(byte sourceId, byte targetId, byte colorid)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        PlayerControl target = ModHelpers.PlayerById(targetId);
        if (target == null || source == null) return;
        source.ProtectPlayer(target, colorid);
        source.MurderPlayer(target);
        source.ProtectPlayer(target, colorid);
        if (targetId == CachedPlayer.LocalPlayer.PlayerId) Buttons.HudManagerStartPatch.ShielderButton.Timer = 0f;
    }
    public static void SetShielder(byte PlayerId, bool Is)
        => RoleClass.Shielder.IsShield[PlayerId] = RoleClass.Shielder.IsShield[PlayerId] = Is;

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

        var SwapPlayer = ModHelpers.PlayerById(SwapPlayerID);
        var SwapperPlayer = ModHelpers.PlayerById(SwapperID);
        var SwapPosition = SwapPlayer.transform.position;
        var SwapperPosition = SwapperPlayer.transform.position;
        //Text
        var rand = new System.Random();
        if (SwapperID == PlayerControl.LocalPlayer.PlayerId)
        {
            CachedPlayer.LocalPlayer.transform.position = SwapPosition;
            SuperNewRolesPlugin.Logger.LogInfo("スワップ本体！");
        }
        else if (SwapPlayerID == PlayerControl.LocalPlayer.PlayerId)
        {
            CachedPlayer.LocalPlayer.transform.position = SwapperPosition;
            SuperNewRolesPlugin.Logger.LogInfo("スワップランダム！");
            if (rand.Next(1, 20) == 1)
            {
                new CustomMessage(string.Format(ModTranslation.GetString("PositionSwapperSwapText2")), 3);
            }
            else
            {
                new CustomMessage(string.Format(ModTranslation.GetString("PositionSwapperSwapText")), 3);
            }
        }
    }

    public static void RPCTeleport(byte sourceId, byte targetId)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        PlayerControl target = ModHelpers.PlayerById(targetId);
        source.transform.localPosition = target.transform.localPosition;
    }

    public static void RandomSpawn(byte playerId, byte locId)
    {
        HudManager.Instance.StartCoroutine(Effects.Lerp(3f, new Action<float>((p) =>
        { // Delayed action
            if (p == 1f)
            {
                //ShipStatus.Instance.InitialSpawnCenter = new(16.64f, -2.46f);
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
                    0 => ShipStatus.Instance.InitialSpawnCenter,
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
                    _ => ShipStatus.Instance.InitialSpawnCenter,
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

    public static void ShowFlash()
    {
        Seer.ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
    }

    public static void SetLoversBreakerWinner(byte playerid) => RoleClass.LoversBreaker.CanEndGamePlayers.Add(playerid);

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class RPCHandlerPatch
    {
        static bool Prefix(PlayerControl __instance, byte callId, MessageWriter reader)
        {
            switch (callId)
            {
                case (byte)RpcCalls.UsePlatform:
                    if (AmongUsClient.Instance.AmHost)
                    {
                        AirshipStatus airshipStatus = GameObject.FindObjectOfType<AirshipStatus>();
                        if (airshipStatus)
                        {
                            airshipStatus.GapPlatform.Use(__instance);
                            __instance.SetDirtyBit(4096u);
                        }
                    }
                    return false;
            }
            return true;
        }

        /// <summary>
        /// LOGに記載しないRPCを設定する
        /// </summary>
        /// <returns>falseで記載するとRPCをlogに記載しなくなる。</returns>
        private static readonly Dictionary<CustomRPC, bool> IsWritingRPCLog = new() {
            {CustomRPC.ShareSNRVersion,false},
            {CustomRPC.SetRoomTimerRPC,false},
            {CustomRPC.SetDeviceTime,false},
        };

        static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
        {
            if (!IsWritingRPCLog.ContainsKey((CustomRPC)callId))
                Logger.Info(ModHelpers.GetRPCNameFromByte(callId), "RPC");
            try
            {
                byte packetId = callId;
                switch ((CustomRPC)packetId)
                {
                    case CustomRPC.ShareOptions:
                        ShareOptions((int)reader.ReadPackedUInt32(), reader);
                        break;
                    case CustomRPC.ShareSNRVersion:
                        int major = reader.ReadPackedInt32();
                        int minor = reader.ReadPackedInt32();
                        int patch = reader.ReadPackedInt32();
                        int versionOwnerId = reader.ReadPackedInt32();
                        byte revision = 0xFF;
                        Guid guid;
                        if (reader.Length - reader.Position >= 17)
                        {
                            revision = reader.ReadByte();
                            byte[] gbytes = reader.ReadBytes(16);
                            guid = new(gbytes);
                        }
                        else
                        {
                            guid = new(new byte[16]);
                        }
                        ShareSNRversion(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                        break;
                    case CustomRPC.SetRole:
                        SetRole(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SheriffKill:
                        SheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean(), reader.ReadBoolean());
                        break;
                    case CustomRPC.MeetingSheriffKill:
                        MeetingSheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean(), reader.ReadBoolean());
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
                        SidekickPromotes(reader.ReadBoolean());
                        break;
                    case CustomRPC.CreateSidekick:
                        CreateSidekick(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.SetSpeedBoost:
                        SetSpeedBoost(reader.ReadBoolean(), reader.ReadByte());
                        break;
                    case CustomRPC.AutoCreateRoom:
                        AutoCreateRoom();
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
                        UncheckedSetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
                        break;
                    case CustomRPC.SetLovers:
                        SetLovers(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.UncheckedSetColor:
                        __instance.SetColor(reader.ReadByte());
                        break;
                    case CustomRPC.UncheckedSetVanillaRole:
                        UncheckedSetVanillaRole(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SetMadKiller:
                        SetMadKiller(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SetCustomSabotage:
                        SabotageManager.SetSabotage(ModHelpers.PlayerById(reader.ReadByte()), (SabotageManager.CustomSabotage)reader.ReadByte(), reader.ReadBoolean());
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
                        PositionSwapperTP(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.FixLights:
                        FixLights();
                        break;
                    case CustomRPC.RandomSpawn:
                        RandomSpawn(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.KunaiKill:
                        KunaiKill(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SetSecretRoomTeleportStatus:
                        MapCustoms.Airship.SecretRoom.SetSecretRoomTeleportStatus((MapCustoms.Airship.SecretRoom.Status)reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.ChiefSidekick:
                        ChiefSidekick(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.RpcSetDoorway:
                        RPCHelper.RpcSetDoorway(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.StartRevolutionMeeting:
                        StartRevolutionMeeting(reader.ReadByte());
                        break;
                    case CustomRPC.SetMatryoshkaDeadbody:
                        SetMatryoshkaDeadBody(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.UncheckedUsePlatform:
                        UncheckedUsePlatform(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.BlockReportDeadBody:
                        BlockReportDeadBody(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.PartTimerSet:
                        PartTimerSet(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.PainterPaintSet:
                        PainterPaintSet(reader.ReadByte(), reader.ReadByte(), reader.ReadBytesAndSize());
                        break;
                    case CustomRPC.PainterSetTarget:
                        PainterSetTarget(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.SharePhotograph:
                        SharePhotograph();
                        break;
                    case CustomRPC.StefinderIsKilled:
                        StefinderIsKilled(reader.ReadByte());
                        break;
                    case CustomRPC.PlayPlayerAnimation:
                        PlayPlayerAnimation(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SluggerExile:
                        source = reader.ReadByte();
                        byte count = reader.ReadByte();
                        List<byte> Targets = new();
                        for (int i = 0; i < count; i++)
                        {
                            Targets.Add(reader.ReadByte());
                        }
                        SluggerExile(source, Targets);
                        break;
                    case CustomRPC.MeetingKill:
                        MeetingKill(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.KnightProtected:
                        KnightProtected(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.KnightProtectClear:
                        KnightProtectClear(reader.ReadByte());
                        break;
                    case CustomRPC.CrackerCrack:
                        CrackerCrack(reader.ReadByte());
                        break;
                    case CustomRPC.WaveCannon:
                        WaveCannon(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean(), reader.ReadByte(), reader.ReadBytesAndSize());
                        break;
                    case CustomRPC.ShowFlash:
                        ShowFlash();
                        break;
                    case CustomRPC.SetFinalStatus:
                        SetFinalStatus(reader.ReadByte(), (FinalStatus)reader.ReadByte());
                        break;
                    case CustomRPC.PavlovsOwnerCreateDog:
                        PavlovsOwnerCreateDog(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.Camouflage:
                        Camouflage(reader.ReadBoolean());
                        break;
                    case CustomRPC.GuesserShoot:
                        GuesserShoot(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.ShowGuardEffect:
                        ShowGuardEffect(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SetLoversCupid:
                        SetLoversCupid(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.PenguinHikizuri:
                        PenguinHikizuri(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SetVampireStatus:
                        SetVampireStatus(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean(), reader.ReadBoolean());
                        break;
                    case CustomRPC.SyncDeathMeeting:
                        SyncDeathMeeting(reader.ReadByte());
                        break;
                    case CustomRPC.SetDeviceTime:
                        SetDeviceTime(reader.ReadByte(), reader.ReadSingle());
                        break;
                    case CustomRPC.SetDeviceUseStatus:
                        SetDeviceUseStatus(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean(), reader.ReadString());
                        break;
                    case CustomRPC.SetLoversBreakerWinner:
                        SetLoversBreakerWinner(reader.ReadByte());
                        break;
                    case CustomRPC.RPCTeleport:
                        RPCTeleport(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SafecrackerGuardCount:
                        SafecrackerGuardCount(reader.ReadByte(), reader.ReadBoolean());
                        break;
                    case CustomRPC.SetVigilance:
                        SetVigilance(reader.ReadBoolean(), reader.ReadByte());
                        break;
                    case CustomRPC.Chat:
                        Chat(reader.ReadByte(), reader.ReadString());
                        break;
                    case CustomRPC.SetWiseManStatus:
                        SetWiseManStatus(reader.ReadByte(), reader.ReadSingle(), reader.ReadBoolean());
                        break;
                    case CustomRPC.SetVentStatusMechanic:
                        SetVentStatusMechanic(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean(), reader.ReadBytesAndSize());
                        break;
                    case CustomRPC.SetTheThreeLittlePigsTeam:
                        SetTheThreeLittlePigsTeam(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.UseTheThreeLittlePigsCount:
                        UseTheThreeLittlePigsCount(reader.ReadByte());
                        break;
                    case CustomRPC.SetOutfit:
                        SetOutfit(reader.ReadByte(), reader.ReadInt32(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString());
                        break;
                    case CustomRPC.CreateShermansServant:
                        CreateShermansServant(reader.ReadByte(), reader.ReadByte());
                        break;
                    case CustomRPC.SetVisible:
                        SetVisible(reader.ReadByte(), reader.ReadBoolean());
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