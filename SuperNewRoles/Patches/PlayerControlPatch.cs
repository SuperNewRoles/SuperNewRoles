using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.BattleRoyal;
using SuperNewRoles.Mode.BattleRoyal.BattleRole;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay.ReplayActions;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.WaveCannonObj;
using UnityEngine;
using static GameData;
using static SuperNewRoles.Helpers.DesyncHelpers;
using static SuperNewRoles.ModHelpers;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
public static class PlayerPhysicsFixedUpdatePatch
{
    public static void Postfix(PlayerPhysics __instance)
    {
        if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started)
            return;
        InvisibleRole.PlayerPhysics_Postfix(__instance);
        if (!ModeHandler.IsMode(ModeId.Default))
            return;
        RoleId PlayerRole = __instance.myPlayer.GetRole();
        if (PlayerRole == RoleId.Kunoichi)
            Kunoichi.PlayerPhysicsScientistPostfix(__instance);
        else if (__instance.AmOwner && GameData.Instance && __instance.myPlayer.CanMove)
        {
            if (PlayerRole is RoleId.SpeedBooster or RoleId.EvilSpeedBooster)
                SpeedBooster.PlayerPhysicsSpeedPatchPostfix(__instance);
            else if (PlayerRole == RoleId.Squid && Squid.Abilitys.IsBoostSpeed)
                __instance.body.velocity *= Squid.SquidBoostSpeed.GetFloat();
        }

        // 移動速度 低下
        if (__instance.AmOwner && RoleClass.Speeder.IsSpeedDown)
        {
            __instance.body.velocity /= 10f;
        }

        // 停止
        if (RoleClass.Freezer.IsSpeedDown ||
            WaveCannonObject.Objects.Contains(__instance.myPlayer) ||
            JumpDancer.JumpingPlayerIds.ContainsKey(__instance.myPlayer.PlayerId) ||
            SpiderTrap.CatchingPlayers.ContainsKey(__instance.myPlayer.PlayerId) ||
            RoleClass.Penguin.PenguinData.Any(x => x.Value != null && x.Value.PlayerId == __instance.myPlayer.PlayerId) ||
            Rocket.RoleData.RocketData.Any(x => x.Value.Any(y => y.PlayerId == __instance.myPlayer.PlayerId))
            )
        {
            __instance.body.velocity = new Vector2(0f, 0f);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
public static class PlayerControlStartMeetingPatch
{
    public static void Postfix(PlayerControl __instance, NetworkedPlayerInfo target)
    {
        Logger.Info($"StartMeeting AmOwner:{__instance.AmOwner} {target == null}");
        if (!__instance.AmOwner && target == null)
            __instance.RemainingEmergencies--;
    }
}
// Allow movement interpolation to use velocities greater than the local player's
/*
[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
public static class NetworkTransformFixedUpdatePatch
{
    public static bool Prefix(CustomNetworkTransform __instance)
    {
        if (__instance.AmOwner)
        {
            if (__instance.HasMoved())
                __instance.SetDirtyBit(3U);
            return false;
        }
        if (__instance.interpolateMovement != 0f)
        {
            Vector2 vector = __instance.targetSyncPosition - __instance.body.position;
            if (vector.sqrMagnitude >= 0.0001f)
            {
                float num = __instance.interpolateMovement / __instance.sendInterval;
                vector.x *= num;
                vector.y *= num;
            }
            else
                vector = Vector2.zero;
            __instance.body.velocity = vector;
        }
        __instance.targetSyncPosition += __instance.targetSyncVelocity * Time.fixedDeltaTime * 0.1f;
        return false;
    }
}
*/

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckUseZipline))]
static class PlayerControlCheckUseZiplinePatch
{
    public static bool Prefix(PlayerControl target, ZiplineBehaviour ziplineBehaviour, bool fromTop)
    {
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, isDefaultOnly: false))
            return true;
        if (!MapCustom.TheFungleZiplineOption.GetBool())
            return true;
        if (!MapCustom.TheFungleCanUseZiplineOption.GetBool())
            return false;
        //上下可能
        int selection = MapCustom.TheFungleZiplineUpOrDown.GetSelection();
        if (selection == 0)
            return true;
        return (!fromTop && selection == 1) ||
               (fromTop && selection == 2);
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
static class PlayerControlSetCooldownPatch
{
    public static bool Prefix(PlayerControl __instance, float time)
    {
        float cool = RoleHelpers.GetEndMeetingKillCoolTime(__instance);
        if (cool <= 0f) return true;
        if (GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown) == time && !RoleClass.IsCoolTimeSetted)
        {
            __instance.SetKillTimerUnchecked(cool, cool);
            RoleClass.IsCoolTimeSetted = true;
            return false;
        }
        time = Mathf.Clamp(time, 0f, cool);
        __instance.SetKillTimerUnchecked(time, cool);
        return false;
    }
}

#region ファントム
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckVanish))]
public static class PlayerControlCheckVanishPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        bool result = CustomRoles.OnCheckVanish(__instance);
        if (result)
            return true;
        __instance.RpcAppear(true);
        new LateTask(() => __instance.HandleServerAppear(true), 0f);
        return true;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckAppear))]
public static class PlayerControlCheckAppearPatch
{
    public static bool Prefix(PlayerControl __instance, bool shouldAnimate)
        => CustomRoles.OnCheckAppear(__instance, shouldAnimate);
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
public static class PlayerControlCmdCheckVanishPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (AmongUsClient.Instance.AmModdedHost || AmongUsClient.Instance.AmLocalHost)
        {
            __instance.CheckVanish();
            return false;
        }
        return true;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
public static class PlayerControlCmdCheckAppearPatch
{
    public static bool Prefix(PlayerControl __instance, bool shouldAnimate)
    {
        if (AmongUsClient.Instance.AmModdedHost || AmongUsClient.Instance.AmLocalHost)
        {
            __instance.CheckAppear(shouldAnimate);
            return false;
        }
        return true;
    }
}
#endregion

[HarmonyPatch(typeof(SwitchMinigame), nameof(SwitchMinigame.Begin))]
public static class SwitchMinigameBeginPatch
{
    public static bool Prefix()
    {
        return !PlayerControl.LocalPlayer.IsRole(RoleId.Vampire, RoleId.Dependents);
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
public static class PlayerControlAwakePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        // バニラ側の当たり判定が60Collider限定なのでとりま180限定にする
        __instance.hitBuffer = new Collider2D[120];
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
class CompleteTask
{
    public static void Postfix(PlayerControl __instance, uint idx)
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == __instance.PlayerId) Roles.Crewmate.Painter.Handle(Roles.Crewmate.Painter.ActionType.TaskComplete);
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
public static class ExilePlayerPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        // Collect dead player info
        DeadPlayer deadPlayer = new(__instance, __instance.PlayerId, DateTime.UtcNow, DeathReason.Exile, null);
        DeadPlayer.deadPlayers.Add(deadPlayer);
        ReplayActionExile.Create(__instance.PlayerId);
        CustomRoles.OnExild(deadPlayer);
        FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.Exiled;
        if (ModeHandler.IsMode(ModeId.Default))
        {
            switch (__instance.GetRole())
            {
                case RoleId.Speeder:
                    if (RoleClass.Speeder.IsSpeedDown)
                        Speeder.SpeedDownEnd();
                    break;
                case RoleId.Assassin:
                    if (RoleClass.Assassin.MeetingEndPlayers.Contains(__instance.PlayerId))
                        __instance.Revive();
                    __instance.Data.IsDead = false;
                    RPCProcedure.CleanBody(__instance.PlayerId);
                    new LateTask(() =>
                    {
                        if (!AmongUsClient.Instance.AmHost)
                            return;
                        MeetingRoomManager.Instance.AssignSelf(__instance, null);
                        FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                        __instance.RpcStartMeeting(null);
                    }, 0.5f, "Exiled Assassin Meeting");
                    RoleClass.Assassin.TriggerPlayer = __instance;
                    return;
                case RoleId.Hitman:
                    if (__instance.PlayerId ==
                        PlayerControl.LocalPlayer.PlayerId
                       )
                        Hitman.Death();
                    break;
                case RoleId.OrientalShaman:
                    if (!OrientalShaman.OrientalShamanCausative.ContainsKey(__instance.PlayerId))
                        break;
                    PlayerControl causativePlayer = PlayerById(OrientalShaman.OrientalShamanCausative[__instance.PlayerId]);
                    if (!causativePlayer.IsAlive())
                        break;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExiledRPC, SendOption.Reliable, -1);
                    writer.Write(causativePlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ExiledRPC(causativePlayer.PlayerId);
                    causativePlayer.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
                    break;
            }
            if (RoleClass.Lovers.SameDie &&
                __instance.IsLovers() &&
                __instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                PlayerControl SideLoverPlayer = __instance.GetOneSideLovers();
                if (SideLoverPlayer.IsAlive())
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExiledRPC, SendOption.Reliable, -1);
                    writer.Write(SideLoverPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ExiledRPC(SideLoverPlayer.PlayerId);
                    SideLoverPlayer.RpcSetFinalStatus(FinalStatus.LoversBomb);
                }
            }
        }
    }
}
[HarmonyPatch(typeof(LongBoiPlayerBody), nameof(LongBoiPlayerBody.SetHeightFromColor))]
public static class LongBoiPlayerBodySetHeightFromColorPatch
{
    // 真っ黄色を最大に設定
    private static Dictionary<int, float> PlayerLongColorSizes = new() { { 28, 9.2f } };
    public static bool Prefix(LongBoiPlayerBody __instance, int colorIndex)
    {
        if (__instance.isPoolablePlayer)
            return true;
        if (!GameManager.Instance.IsHideAndSeek() ||
            AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started ||
            !(__instance.myPlayerControl.Data.Role != null || __instance.myPlayerControl.Data.Role.TeamType != RoleTeamTypes.Impostor))
        {
            if (colorIndex < __instance.heightsPerColor.Length)
                __instance.targetHeight = __instance.heightsPerColor[colorIndex];
            else
            {
                if (PlayerLongColorSizes.TryGetValue(colorIndex, out float value))
                    __instance.targetHeight = value;
                else
                {
                    __instance.targetHeight = new System.Random(colorIndex).Next(9, 92) / 10f;
                    PlayerLongColorSizes[colorIndex] = __instance.targetHeight;
                }
            }
            if (LobbyBehaviour.Instance != null)
            {
                __instance.SetupNeckGrowth(snapNeck: false, resetNeck: false);
            }
            else
            {
                __instance.SetupNeckGrowth(snapNeck: true, resetNeck: false);
            }
        }
        return false;
    }
}
//[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.PetPet))]
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
class PlayerPhysicsPetPetPatch
{
    public static bool Prefix(PlayerPhysics __instance, byte callId)
    {
        Logger.Info($"[PlayerPhysicsRpcCalled]{callId}.{(RpcCalls)callId}");
        if (callId == (byte)RpcCalls.Pet &&
            __instance.myPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId &&
            !CustomRoles.OnPetPet(__instance.myPlayer))
        {
            _ = new LateTask(__instance.RpcCancelPet, 0f);
        }
        return true;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.TryPet))]
class PlayerControlTryPetPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (!CustomRoles.OnPetPet(__instance))
        {
            _ = new LateTask(__instance.MyPhysics.RpcCancelPet, 0.04f);
        }
        return true;
    }
}
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
class ReportDeadBodyPatch
{
    /// <summary>
    /// 会議が開かれた回数を記録する(ReportDeadBodyでカウントしているが, RPCを飛ばしている為 ゲストに共有されている)
    /// </summary>
    /// <param name="allTurn">全体会議回数</param>
    /// <param name="emergency">緊急招集による会議回数</param>
    /// <param name="report">死体通報による会議回数</param>
    public static (byte all, byte emergency, byte report) MeetingCount { get; private set; }
    public static void ClearAndReloads()
    {
        MeetingCount = (0, 0, 0);
    }

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        if (__instance.IsRole(RoleId.GM))
        {
            MeetingRoomManager.Instance.AssignSelf(__instance, target);
            if (!AmongUsClient.Instance.AmHost)
                return false;
            FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
            __instance.RpcStartMeeting(target);
            return false;
        }
        if (RoleClass.Camouflager.IsCamouflage)
            Camouflager.ResetCamouflage();
        ReplayActionReportDeadBody.Create(__instance.PlayerId, target is null ? (byte)255 : target.PlayerId);
        if (!AmongUsClient.Instance.AmHost) return true;
        if (target != null && RoleClass.BlockPlayers.Contains(target.PlayerId)) return false;
        if (ModeHandler.IsMode(ModeId.HideAndSeek)) return false;
        if (ModeHandler.IsMode(ModeId.Default))
        {
            if (__instance.IsRole(RoleId.EvilButtoner, RoleId.NiceButtoner) &&
                target != null &&
                target.PlayerId == __instance.PlayerId)
                return true;
            if (__instance.IsRole(RoleId.Amnesiac) &&
                target != null &&
                !target.Disconnected)
            {
                __instance.RPCSetRoleUnchecked(target.RoleWhenAlive is null ? target.Role.Role : target.RoleWhenAlive.Value);
                __instance.SwapRoleRPC(target.Object);
                target.Object.SetRoleRPC(__instance.GetRole());
            }
            if (__instance.IsRole(RoleId.DyingMessenger) && target != null)
            {
                bool isGetRole = (float)(DeadPlayer.ActualDeathTime[target.PlayerId].DeathTime + new TimeSpan(0, 0, 0, DyingMessenger.DyingMessengerGetRoleTime.GetInt()) - DateTime.Now).TotalSeconds >= 0;
                bool isGetLightAndDarker = (float)(DeadPlayer.ActualDeathTime[target.PlayerId].DeathTime + new TimeSpan(0, 0, 0, DyingMessenger.DyingMessengerGetLightAndDarkerTime.GetInt()) - DateTime.Now).TotalSeconds >= 0;
                string firstPerson = IsSuccessChance(9) ? ModTranslation.GetString("DyingMessengerFirstPerson1") : ModTranslation.GetString("DyingMessengerFirstPerson2");
                if (isGetRole)
                {
                    string text = string.Format(ModTranslation.GetString("DyingMessengerGetRoleText"), firstPerson, ModTranslation.GetString($"{DeadPlayer.ActualDeathTime[target.PlayerId].Killer.GetRole()}Name"));
                    new LateTask(() =>
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.Chat, __instance);
                        writer.Write(target.PlayerId);
                        writer.Write(text);
                        writer.EndRPC();
                    }, 0.5f, "DyingMessengerText");
                }
                if (isGetLightAndDarker)
                {
                    string text = string.Format(ModTranslation.GetString("DyingMessengerGetLightAndDarkerText"), firstPerson,
                        CustomColors.LighterColors.Contains(DeadPlayer.ActualDeathTime[target.PlayerId].Killer.Data.DefaultOutfit.ColorId) ? ModTranslation.GetString("LightColor") : ModTranslation.GetString("DarkerColor"));
                    new LateTask(() =>
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.Chat, __instance);
                        writer.Write(target.PlayerId);
                        writer.Write(text);
                        writer.EndRPC();
                    }, 0.5f, "DyingMessengerText");
                }
            }
            if (OrientalShaman.IsTransformation)
            {
                OrientalShaman.SetOutfit(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.DefaultOutfit);
                OrientalShaman.IsTransformation = false;
            }
        }

        if (RoleClass.Assassin.TriggerPlayer != null)
            return false;
        // 死体通報できないかつ、死体通報の場合
        if (!Mode.PlusMode.PlusGameOptions.UseDeadBodyReport && target != null)
            return false;
        // 緊急招集が無効かつ、通報対象がいない場合
        if (!Mode.PlusMode.PlusGameOptions.EmergencyMeetingsCallstate.enabledSetting && target == null)
            return false;
        // 会議を開けないモードを防ぐ
        if (ModeHandler.IsMode(ModeId.BattleRoyal, ModeId.PantsRoyal, ModeId.CopsRobbers, ModeId.Zombie))
            return false;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) &&
            !ReportDeadBody.ReportDeadBodyPatch(__instance, target))
            return false;
        // 探偵以外が会議を開けないときの処理
        if (ModeHandler.IsMode(ModeId.Detective)
            && target == null
            && Mode.Detective.Main.IsNotDetectiveMeetingButton
            && __instance.PlayerId != Mode.Detective.Main.DetectivePlayer.PlayerId)
            return false;

        return true;
    }

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo target)
    {
        if (!AmongUsClient.Instance.AmHost) return; // ホスト以外此処は読まないが, バニラ側の使用が変更された時に問題が起きないように ホスト以外はreturnする。

        var targetId = target != null ? target.Object.PlayerId : byte.MaxValue;

        // ゲストに通報対象を送信し, ターン情報を共有する
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SendMeetingCount);
        writer.Write(targetId);
        writer.EndRPC();
        SaveMeetingCount(targetId);

        EmergencyMinigamePatch.SHRMeetingStatusAnnounce.EmergencyCount(__instance); // ReportDeadBody.ReportDeadBodyPatchで実行すると 2回読まれてしまう為ここで呼ぶ。

        PoliceSurgeon_AddActualDeathTime.ReportDeadBody_Postfix();
    }

    /// <summary>通報対象の情報を元に, 会議情報を記録する</summary>
    /// <param name="targetId">通報対象のプレイヤーId</param>
    public static void SaveMeetingCount(byte targetId)
    {
        var target = PlayerById(targetId);
        var count = MeetingCount;

        count.all++;
        if (target == null) count.emergency++; // 通報対象がnullなら緊急招集として記録する
        else count.report++; // 通報対象が存在するなら通報として記録する

        MeetingCount = count;
    }
}
public static class PlayerControlFixedUpdatePatch
{
    public static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, IEnumerable<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
    {
        PlayerControl result = null;
        float num = GameManager.Instance.LogicOptions.GetKillDistance();
        if (!MapUtilities.CachedShipStatus) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            NetworkedPlayerInfo playerInfo = allPlayers[i];
            if (playerInfo.Disconnected ||
                playerInfo.PlayerId == targetingPlayer.PlayerId ||
                playerInfo.IsDead ||
                (onlyCrewmates && playerInfo.Role.IsImpostor)
               )
                continue;
            PlayerControl @object = playerInfo.Object;
            if (untargetablePlayers != null &&
                untargetablePlayers.Any(x => x == @object))
            {
                // if that player is not targetable: skip check
                continue;
            }
            if (@object == null ||
                (@object.inVent && !targetPlayersInVents))
                continue;
            Vector2 vector = @object.GetTruePosition() - truePosition;
            float magnitude = vector.magnitude;
            if (magnitude > num ||
                PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)
                )
                continue;
            result = @object;
            num = magnitude;
        }
        return result;
    }
    public static PlayerControl JackalSetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
    {
        PlayerControl result = null;
        float num = GameManager.Instance.LogicOptions.GetKillDistance();
        if (!MapUtilities.CachedShipStatus) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = GameData.Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            NetworkedPlayerInfo playerInfo = allPlayers[i];
            //下記Jackalがbuttonのターゲットにできない役職の設定
            if (playerInfo.Object.IsDead() ||
                playerInfo.PlayerId == targetingPlayer.PlayerId ||
                playerInfo.Object.IsJackalTeamJackal() ||
                playerInfo.Object.IsJackalTeamSidekick() ||
                playerInfo.Object.IsRole(RoleId.Bullet)
               )
                continue;
            PlayerControl @object = playerInfo.Object;
            if (untargetablePlayers != null &&
                untargetablePlayers.Any(x => x == @object)
               )
                continue;

            if (!@object ||
                (@object.inVent && !targetPlayersInVents)
               )
                continue;
            Vector2 vector = @object.GetTruePosition() - truePosition;
            float magnitude = vector.magnitude;
            if (magnitude > num ||
                PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask)
               )
                continue;
            result = @object;
            num = magnitude;
        }
        return result;
    }

    public static PlayerControl GhostRoleSetTarget(bool onlyCrewmates = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
    { // ほとんど通常のSetTargetと同じだが, 事故を防ぐ為に分割
        PlayerControl result = null;
        if (!MapUtilities.CachedShipStatus) return result;

        float num = GameManager.Instance.LogicOptions.GetKillDistance();
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (!targetingPlayer.Data.IsDead)
        {
            Logger.Info($"{targetingPlayer.name}は, 生存している為 幽霊役職用の対象取得を使用できません。");
            return result; // ボタンの使用者が生きていたらnullを返す
        }

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<NetworkedPlayerInfo> allPlayers = Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            NetworkedPlayerInfo playerInfo = allPlayers[i]; // ボタンの対象判定
            if (playerInfo.Disconnected ||
                playerInfo.PlayerId == targetingPlayer.PlayerId ||
                playerInfo.IsDead ||
                (onlyCrewmates && playerInfo.Role.IsImpostor))
            {
                continue;
            }

            PlayerControl @object = playerInfo.Object;
            if (@object == null || (untargetablePlayers != null && untargetablePlayers.Any(x => x == @object)))
            {
                // if that player is not targetable: skip check
                continue;
            }

            Vector2 vector = @object.GetTruePosition() - truePosition;
            float magnitude = vector.magnitude;
            if (magnitude > num ||
                PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
            {
                continue;
            }

            result = @object;
            num = magnitude;
        }
        return result;
    }

    public static void SetPlayerOutline(PlayerControl target, Color color)
    {
        Material material = target?.MyRend()?.material;
        if (material == null) return;
        material.SetFloat("_Outline", 1f);
        material.SetColor("_OutlineColor", color);
    }
}