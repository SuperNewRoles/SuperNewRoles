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
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static GameData;
using static SuperNewRoles.Helpers.DesyncHelpers;
using static SuperNewRoles.ModHelpers;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
public class UsePlatformPlayerControlPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            AmongUsClient.Instance.StartRpc(__instance.NetId, 32, SendOption.Reliable)
                .EndMessage();
            return false;
        }
        AirshipStatus airshipStatus = GameObject.FindObjectOfType<AirshipStatus>();
        if (airshipStatus)
            airshipStatus.GapPlatform.Use(__instance);
        return false;
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
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, isDefaultOnly:false))
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
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
class ReportDeadBodyPatch
{
    public static byte MeetingTurn_Now { get; private set; }
    public static void ClearAndReloads()
    {
        MeetingTurn_Now = 0;
    }

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
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
            if (__instance.IsRole(RoleId.DyingMessenger) &&
                target != null &&
                DeadPlayer.ActualDeathTime.ContainsKey(target.PlayerId))
            {
                bool isGetRole = (float)(DeadPlayer.ActualDeathTime[target.PlayerId].Item1 + new TimeSpan(0, 0, 0, DyingMessenger.DyingMessengerGetRoleTime.GetInt()) - DateTime.Now).TotalSeconds >= 0;
                bool isGetLightAndDarker = (float)(DeadPlayer.ActualDeathTime[target.PlayerId].Item1 + new TimeSpan(0, 0, 0, DyingMessenger.DyingMessengerGetLightAndDarkerTime.GetInt()) - DateTime.Now).TotalSeconds >= 0;
                string firstPerson = IsSucsessChance(9) ? ModTranslation.GetString("DyingMessengerFirstPerson1") : ModTranslation.GetString("DyingMessengerFirstPerson2");
                if (isGetRole)
                {
                    string text = string.Format(ModTranslation.GetString("DyingMessengerGetRoleText"), firstPerson, ModTranslation.GetString($"{DeadPlayer.ActualDeathTime[target.PlayerId].Item2.GetRole()}Name"));
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
                        CustomColors.LighterColors.Contains(DeadPlayer.ActualDeathTime[target.PlayerId].Item2.Data.DefaultOutfit.ColorId) ? ModTranslation.GetString("LightColor") : ModTranslation.GetString("DarkerColor"));
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
        if (ReportDeadBody.ReportDeadBodyPatch(__instance, target) && ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.IsRole(RoleId.Doppelganger))
                {
                    new LateTask(() =>
                    {
                        player.RpcShapeshift(player, false);
                    }, 0.5f);
                    SyncSetting.CustomSyncSettings(player);
                }
            }
        }
        return RoleClass.Assassin.TriggerPlayer == null
        && (Mode.PlusMode.PlusGameOptions.UseDeadBodyReport || target == null)
        && (Mode.PlusMode.PlusGameOptions.UseMeetingButton || target != null)
        && !ModeHandler.IsMode(ModeId.BattleRoyal, ModeId.PantsRoyal)
        && !ModeHandler.IsMode(ModeId.CopsRobbers)
&& (ModeHandler.IsMode(ModeId.SuperHostRoles)
            ? Mode.SuperHostRoles.ReportDeadBody.ReportDeadBodyPatch(__instance, target)
            : !ModeHandler.IsMode(ModeId.Zombie)
            && (!ModeHandler.IsMode(ModeId.Detective) || target != null || !Mode.Detective.Main.IsNotDetectiveMeetingButton || __instance.PlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId));
    }

    public static void Postfix()
    {
        if (!AmongUsClient.Instance.AmHost) return; // ホスト以外此処は読まないが, バニラ側の使用が変更された時に問題が起きないように ホスト以外はreturnする。
        MeetingTurn_Now++;

        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SendMeetingTurnNow);
        writer.Write(MeetingTurn_Now);
        writer.EndRPC();
    }

    public static void SaveMeetingTurnNow(byte nowTurn) => MeetingTurn_Now = nowTurn;
}
public static class PlayerControlFixedUpdatePatch
{
    public static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
    {
        PlayerControl result = null;
        float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)];
        if (!MapUtilities.CachedShipStatus) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            PlayerInfo playerInfo = allPlayers[i];
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
        float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)];
        if (!MapUtilities.CachedShipStatus) return result;
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            PlayerInfo playerInfo = allPlayers[i];
            //下記Jackalがbuttonのターゲットにできない役職の設定
            if (playerInfo.Object.IsDead() ||
                playerInfo.PlayerId == targetingPlayer.PlayerId ||
                playerInfo.Object.IsJackalTeamJackal() ||
                playerInfo.Object.IsJackalTeamSidekick()
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

        float num = GameOptionsData.KillDistances[Mathf.Clamp(GameManager.Instance.LogicOptions.currentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2)];
        if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
        if (!targetingPlayer.Data.IsDead)
        {
            Logger.Info($"{targetingPlayer.name}は, 生存している為 幽霊役職用の対象取得を使用できません。");
            return result; // ボタンの使用者が生きていたらnullを返す
        }

        Vector2 truePosition = targetingPlayer.GetTruePosition();
        Il2CppSystem.Collections.Generic.List<PlayerInfo> allPlayers = Instance.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            PlayerInfo playerInfo = allPlayers[i]; // ボタンの対象判定
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