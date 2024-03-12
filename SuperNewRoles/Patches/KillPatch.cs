using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
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
using static SuperNewRoles.ModHelpers;

namespace SuperNewRoles.Patches;

#region KillButtonDoClickPatch

[HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
class KillButtonDoClickPatch
{
    public static bool Prefix(KillButton __instance)
    {
        if (!ModeHandler.IsMode(ModeId.Default))
        {
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles))
                return true;
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
                return true;
            if (__instance.isActiveAndEnabled && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove && !__instance.isCoolingDown && RoleClass.RemoteSheriff.KillMaxCount > 0)
            {
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    p.Data.Role.NameColor = Color.white;
                }
                CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.IsImpostor())
                    {
                        p.Data.Role.NameColor = RoleClass.ImpostorRed;
                    }
                }
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);
                PlayerControl.LocalPlayer.killTimer = 0.001f;
            }
            return false;
        }
        if (!(__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove))
            return false;
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Kunoichi))
        {
            Kunoichi.KillButtonClick();
            return false;
        }
        if (!(__instance.currentTarget.IsRole(RoleId.Bait) || __instance.currentTarget.IsRole(RoleId.NiceRedRidingHood)) && PlayerControl.LocalPlayer.IsRole(RoleId.Vampire))
        {
            PlayerControl.LocalPlayer.killTimer =
                RoleHelpers.GetCoolTime(
                    PlayerControl.LocalPlayer,
                    __instance.currentTarget
                );
            RoleClass.Vampire.target = __instance.currentTarget;
            RoleClass.Vampire.KillTimer = DateTime.Now;
            RoleClass.Vampire.Timer = RoleClass.Vampire.KillDelay;

            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetVampireStatus);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write(RoleClass.Vampire.target.PlayerId);
            writer.Write(true);
            writer.EndRPC();
            RPCProcedure.SetVampireStatus(CachedPlayer.LocalPlayer.PlayerId, RoleClass.Vampire.target.PlayerId, true, false);
            return false;
        }
        bool showAnimation = true;

        //会議中ならさよなら
        if (MeetingHud.Instance != null)
            return false;

        // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
        MurderAttemptResult res = CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget, showAnimation: showAnimation);
        // Handle blank kill
        if (res == MurderAttemptResult.BlankKill)
        {
            PlayerControl.LocalPlayer.killTimer =
                RoleHelpers.GetCoolTime(
                    PlayerControl.LocalPlayer,
                    __instance.currentTarget
                );
        }
        __instance.SetTarget(null);
        return false;
    }
}
#endregion

#region CheckMurderPatch

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
static class CheckMurderPatch
{
    public static bool isKill = false;

    public static bool IsKillSuc = false;

    public static bool HandleSHR(PlayerControl __instance, PlayerControl target)
    {
        if (RoleClass.Assassin.TriggerPlayer != null) return false;
        switch (__instance.GetRole())
        {
            case RoleId.RemoteSheriff:
            case RoleId.ToiletFan:
            case RoleId.NiceButtoner:
            case RoleId.Madmate:
            case RoleId.JackalFriends:
            case RoleId.MadRaccoon:
            case RoleId.Egoist when !RoleClass.Egoist.UseKill:
                return false;
            case RoleId.FalseCharges:
                target.RpcMurderPlayer(__instance, true);
                target.RpcSetFinalStatus(FinalStatus.FalseChargesFalseCharge);
                RoleClass.FalseCharges.FalseChargePlayers[__instance.PlayerId] = target.PlayerId;
                RoleClass.FalseCharges.AllTurns[__instance.PlayerId] = RoleClass.FalseCharges.DefaultTurn;
                return false;
            case RoleId.truelover:
                if (__instance.IsLovers()) return false;
                if (target == null || target.IsLovers() || RoleClass.Truelover.CreatePlayers.Contains(__instance.PlayerId)) return false;
                __instance.RpcShowGuardEffect(target);
                RoleClass.Truelover.CreatePlayers.Add(__instance.PlayerId);
                RoleHelpers.SetLovers(__instance, target);
                RoleHelpers.SetLoversRPC(__instance, target);
                Mode.SuperHostRoles.ChangeName.SetRoleName(__instance);
                Mode.SuperHostRoles.ChangeName.SetRoleName(target);
                return false;
            case RoleId.Sheriff:
                //もうキルできる回数がないならreturn
                if (RoleClass.Sheriff.KillCount.ContainsKey(__instance.PlayerId) && RoleClass.Sheriff.KillCount[__instance.PlayerId] <= 0) return false;

                (var killResult, var suicideResult) = Sheriff.SheriffKillResult(__instance, target);

                if (killResult.Item1)
                {
                    FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = killResult.Item2;
                    __instance.RpcMurderPlayerCheck(target);
                    target.RpcSetFinalStatus(killResult.Item2);

                    if (!RoleClass.Sheriff.KillCount.ContainsKey(__instance.PlayerId))
                        RoleClass.Sheriff.KillCount[__instance.PlayerId] = CustomOptionHolder.SheriffKillMaxCount.GetInt();
                    RoleClass.Sheriff.KillCount[__instance.PlayerId]--;
                    Mode.SuperHostRoles.ChangeName.SetRoleName(__instance);
                }

                if (suicideResult.Item1)
                {
                    FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = suicideResult.Item2;
                    __instance.RpcMurderPlayer(__instance, true);
                    __instance.RpcSetFinalStatus(suicideResult.Item2);
                }
                return false;
            case RoleId.MadMaker:
                if (!target.IsImpostor())
                {
                    if (target == null ||
                        RoleClass.MadMaker.CreatePlayers.Contains(__instance.PlayerId))
                        return false;
                    __instance.RpcShowGuardEffect(target);
                    RoleClass.MadMaker.CreatePlayers.Add(__instance.PlayerId);
                    Madmate.CreateMadmate(target);
                    Mode.SuperHostRoles.ChangeName.SetRoleName(target);
                }
                else
                {
                    __instance.RpcMurderPlayer(__instance, true);
                    __instance.RpcSetFinalStatus(FinalStatus.MadmakerMisSet);
                }
                return false;
            case RoleId.Demon:
                if (__instance.IsCursed(target))
                    return false;
                Demon.DemonCurse(target, __instance);
                __instance.RpcShowGuardEffect(target);
                Mode.SuperHostRoles.ChangeName.SetRoleName(__instance);
                return false;
            case RoleId.OverKiller:
                __instance.RpcMurderPlayerCheck(target);
                target.RpcSetFinalStatus(FinalStatus.OverKillerOverKill);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.Data.Disconnected ||
                        p.PlayerId == target.PlayerId ||
                        p.IsBot())
                        continue;
                    for (int i = 0; i < RoleClass.OverKiller.KillCount - 1; i++)
                    {
                        if (p.PlayerId != 0)
                            __instance.RPCMurderPlayerPrivate(target, p);
                        else
                            __instance.MurderPlayer(target, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
                    }
                }
                return false;
            case RoleId.Arsonist:
                if (__instance.IsDoused(target))
                    return false;
                __instance.RpcShowGuardEffect(target);// 守護エフェクト
                SyncSetting.DefaultOption.DeepCopy().SetFloat(FloatOptionNames.ShapeshifterCooldown, RoleClass.Arsonist.DurationTime);// シェイプクールダウンを塗り時間に
                new LateTask(() =>
                {
                    if (Vector2.Distance(__instance.transform.position, target.transform.position) <= 1.75f)//1.75f以内にターゲットがいるなら
                    {
                        Arsonist.ArsonistDouse(target, __instance);
                        __instance.RpcShowGuardEffect(target);// もう一度エフェクト
                        Mode.SuperHostRoles.ChangeName.SetRoleName(__instance);
                    }
                    else
                    {//塗れなかったらキルクールリセット
                        SyncSetting.DefaultOption.DeepCopy().SetFloat(FloatOptionNames.KillCooldown, SyncSetting.KillCoolSet(0f));
                    }
                }, RoleClass.Arsonist.DurationTime, "SHR Arsonist Douse");
                return false;
            case RoleId.Mafia:
                if (!Mafia.IsKillFlag()) return false;
                break;
            case RoleId.FastMaker:
                if (RoleClass.FastMaker.IsCreatedMadmate)//まだ作ってなくて、設定が有効の時
                {
                    //作ってたら普通のキル(此処にMurderPlayerを使用すると2回キルされる為ログのみ表示)
                    Logger.Info("マッドメイトを作成済みの為 普通のキル", "FastMakerSHR");
                    break;
                }
                if (target == null || RoleClass.FastMaker.CreatePlayers.Contains(__instance.PlayerId)) return false;
                __instance.RpcShowGuardEffect(target);
                RoleClass.FastMaker.CreatePlayers.Add(__instance.PlayerId);
                Madmate.CreateMadmate(target);//クルーにして、マッドにする
                Mode.SuperHostRoles.ChangeName.SetRoleName(target);//名前も変える
                RoleClass.FastMaker.IsCreatedMadmate = true;//作ったことにする
                Logger.Info("マッドメイトを作成しました", "FastMakerSHR");
                return false;
            case RoleId.Jackal:
                //まだ作ってなくて、設定が有効の時
                if (RoleClass.Jackal.CreatePlayers.Contains(__instance.PlayerId) ||
                    !RoleClass.Jackal.CanCreateFriend)
                {
                    // キルができた理由のログを表示する(此処にMurderPlayerを使用すると2回キルされる為ログのみ表示)
                    if (!RoleClass.Jackal.CanCreateFriend) Logger.Info("ジャッカルフレンズを作る設定ではない為 普通のキル", "JackalSHR");
                    else if (RoleClass.Jackal.CanCreateFriend && RoleClass.Jackal.CreatePlayers.Contains(__instance.PlayerId)) Logger.Info("ジャッカルフレンズ作成済みの為 普通のキル", "JackalSHR");
                    else Logger.Info("不正なキル", "JackalSHR");
                    break;
                }
                SuperNewRolesPlugin.Logger.LogInfo("まだ作ってなくて、設定が有効の時なんでフレンズ作成");
                if (target == null || RoleClass.Jackal.CreatePlayers.Contains(__instance.PlayerId)) return false;
                __instance.RpcShowGuardEffect(target);
                RoleClass.Jackal.CreatePlayers.Add(__instance.PlayerId);
                if (!target.IsImpostor())
                {
                    Jackal.CreateJackalFriends(target);//クルーにして フレンズにする
                }
                Mode.SuperHostRoles.ChangeName.SetRoleName(target);//名前も変える
                Logger.Info("ジャッカルフレンズを作成しました。", "JackalSHR");
                return false;
            case RoleId.JackalSeer:
                if (RoleClass.JackalSeer.CreatePlayers.Contains(__instance.PlayerId) && RoleClass.JackalSeer.CanCreateFriend)//まだ作ってなくて、設定が有効の時
                {
                    // キルができた理由のログを表示する(此処にMurderPlayerを使用すると2回キルされる為ログのみ表示)
                    if (!RoleClass.JackalSeer.CanCreateFriend) Logger.Info("ジャッカルフレンズを作る設定ではない為 普通のキル", "JackalSeerSHR");
                    else if (RoleClass.JackalSeer.CanCreateFriend && RoleClass.JackalSeer.CreatePlayers.Contains(__instance.PlayerId)) Logger.Info("ジャッカルフレンズ作成済みの為 普通のキル", "JackalSeerSHR");
                    else Logger.Info("不正なキル", "JackalSeerSHR");
                    break;
                }
                Logger.Info("未作成 且つ 設定が有効である為 フレンズを作成", "JackalSeerSHR");
                if (target == null || RoleClass.JackalSeer.CreatePlayers.Contains(__instance.PlayerId)) return false;
                __instance.RpcShowGuardEffect(target);
                RoleClass.JackalSeer.CreatePlayers.Add(__instance.PlayerId);
                if (!target.IsImpostor())
                {
                    Jackal.CreateJackalFriends(target);//クルーにして フレンズにする
                }
                Mode.SuperHostRoles.ChangeName.SetRoleName(target);//名前も変える
                Logger.Info("ジャッカルフレンズを作成しました。", "JackalSeerSHR");
                return false;
            case RoleId.DarkKiller:
                if (!MapUtilities.CachedShipStatus.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType elec))
                    return false;
                var ma = elec.CastFast<SwitchSystem>();
                if (ma != null && !ma.IsActive) return false;
                break;
            case RoleId.Worshiper:
                __instance.RpcMurderPlayer(__instance, true);
                __instance.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
                return false;
            case RoleId.Penguin:
                PlayerControl currentTarget = RoleClass.Penguin.PenguinData[__instance];
                if (currentTarget != null)
                    break;
                RoleClass.Penguin.PenguinData[__instance] = target;
                RoleClass.Penguin.PenguinTimer.TryAdd(__instance.PlayerId, CustomOptionHolder.PenguinDurationTime.GetFloat());
                target.RpcSnapTo(__instance.transform.position);
                return false;
        }
        SyncSetting.CustomSyncSettings(__instance);
        SyncSetting.CustomSyncSettings(target);
        switch (target.GetRole())
        {
            case RoleId.StuntMan:
                if (!EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard, __instance))
                    break;
                if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId))
                    RoleClass.StuntMan.GuardCount[target.PlayerId] = CustomOptionHolder.StuntManMaxGuardCount.GetInt();
                if (RoleClass.StuntMan.GuardCount[target.PlayerId] <= 0)
                    break;
                RoleClass.StuntMan.GuardCount[target.PlayerId]--;
                __instance.RpcShowGuardEffect(target);
                return false;
            case RoleId.Fox:
                if (!EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, __instance))
                    break;
                __instance.RpcShowGuardEffect(target);
                return false;
        }
        return true;
    }

    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        IsKillSuc = true;
        Logger.Info($"{__instance.Data.PlayerName}=>{target.Data.PlayerName}", "CheckMurder");
        if (
                __instance.IsBot() ||
                __instance.IsDead() ||
                target.IsBot() ||
                target.IsDead() ||
                target.inVent ||
                target.MyPhysics.Animations.IsPlayingEnterVentAnimation() ||
                target.MyPhysics.Animations.IsPlayingAnyLadderAnimation() ||
                target.inMovingPlat ||
                MeetingHud.Instance != null ||
                (!RoleClass.IsStart && AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) ||
                AirShipRandomSpawn.IsLoading
           )
        {
            MurderHelpers.RpcMurderPlayerFailed(__instance, target);
            return IsKillSuc = false;
        }
        Logger.Info("キル可能かを通過しました。", "CheckMurder");
        if (GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek) return true;
        if (__instance.PlayerId == target.PlayerId)
        {
            Logger.Info($"自爆:{target.name}", "CheckMurder");
            __instance.RpcMurderPlayer(target, true);
            return false;
        }

        if (!AmongUsClient.Instance.AmHost) return true;
        if (!CustomRoles.OnCheckMurderPlayer(__instance, target))
            return IsKillSuc = false;
        switch (ModeHandler.GetMode())
        {
            case ModeId.Zombie:
                return IsKillSuc = false;
            case ModeId.PantsRoyal:
                Mode.PantsRoyal.main.OnMurderClick(__instance, target);
                return IsKillSuc = false;
            case ModeId.BattleRoyal:
                if (__instance.PlayerId == PlayerControl.LocalPlayer.PlayerId && isKill)
                    return IsKillSuc = false;
                if (Mode.BattleRoyal.Main.IsTeamBattle)
                {
                    foreach (BattleTeam teams in BattleTeam.BattleTeams)
                    {
                        if (teams.IsTeam(__instance) && teams.IsTeam(target))
                            return IsKillSuc = false;
                    }
                }
                PlayerAbility targetAbility = PlayerAbility.GetPlayerAbility(target);
                if (target.IsRole(RoleId.Guardrawer) && Guardrawer.guardrawers.FirstOrDefault(x => x.CurrentPlayer == target).IsAbilityUsingNow)
                {
                    Mode.BattleRoyal.Main.MurderPlayer(target, __instance, targetAbility);
                    return IsKillSuc = false;
                }
                if (target.IsBot())
                {
                    if (target == CrystalMagician.Bot)
                        CrystalMagician.UseWater(__instance);
                    return IsKillSuc = false;
                }
                if (!PlayerAbility.GetPlayerAbility(__instance).CanUseKill)
                    return IsKillSuc = false;
                KingPoster kp = KingPoster.GetKingPoster(__instance);
                if (!targetAbility.CanKill)
                    return IsKillSuc = false;
                if (__instance.IsRole(RoleId.KingPoster) && kp.IsAbilityUsingNow)
                {
                    kp.OnKillClick(target);
                    return IsKillSuc = false;
                }
                if (Mode.BattleRoyal.Main.StartSeconds <= 0)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("[CheckMurder]LateTask:" + (AmongUsClient.Instance.Ping / 1000f) * 2f);
                    if (__instance.PlayerId != 0)
                    {
                        Mode.BattleRoyal.Main.MurderPlayer(__instance, target, targetAbility);
                        isKill = false;
                    }
                    else
                    {
                        isKill = true;
                        new LateTask(() =>
                        {
                            if (__instance.IsAlive() && target.IsAlive())
                            {
                                Mode.BattleRoyal.Main.MurderPlayer(__instance, target, targetAbility);
                            }
                            isKill = false;
                        }, AmongUsClient.Instance.Ping / 1000f * 1.1f, "BattleRoyal Murder");
                    }
                }
                return false;
            case ModeId.SuperHostRoles:
                bool SHRresult = HandleSHR(__instance, target);
                if (!SHRresult)
                    return IsKillSuc = false;
                break;
            case ModeId.Detective:
                if (target.PlayerId ==
                    Mode.Detective.Main.DetectivePlayer.PlayerId)
                    return IsKillSuc = false;
                break;
        }
        Logger.Info("全モード通過", "CheckMurder");
        Logger.Info("全スタントマン系通過", "CheckMurder");
        __instance.RpcMurderPlayerCheck(target);
        Camouflager.ResetCamouflageSHR(target);
        Logger.Info("RpcMurderPlayerCheck(一番下)を通過", "CheckMurder");
        return false;
    }
    public static void Postfix(PlayerControl __instance, PlayerControl target)
    {
        if (!IsKillSuc)
        {
            MurderHelpers.RpcMurderPlayerFailed(__instance, target);
        }
    }
    public static void RpcCheckExile(this PlayerControl __instance)
    {
        if (__instance.IsRole(RoleId.Assassin) &&
            __instance.IsAlive())
        {
            new LateTask(() =>
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    MeetingRoomManager.Instance.AssignSelf(__instance, null);
                    FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                    __instance.RpcStartMeeting(null);
                }
            }, 0.5f, "RpcCheckExile Assassin Start Meeting");
            new LateTask(() =>
            {
                __instance.RpcSetName($"<size=200%>{CustomOptionHolder.Cs(RoleClass.Marlin.color, IntroData.MarlinIntro.NameKey + "Name")}は誰だ？</size>");
            }, 2f, "RpcCheckExile Who Marlin Name");
            new LateTask(() =>
            {
                __instance.RpcSendChat($"\n{ModTranslation.GetString("MarlinWhois")}");
            }, 2.5f, "RpcCheckExile Who Marlin Chat");
            new LateTask(() =>
            {
                __instance.RpcSetName(__instance.GetDefaultName());
            }, 2f, "RpcCheckExile Default Name");
            RoleClass.Assassin.TriggerPlayer = __instance;
            return;
        }
        __instance.RpcInnerExiled();
    }
    public static void RpcMurderPlayerCheck(this PlayerControl __instance, PlayerControl target)
    {
        if (ModeHandler.IsMode(ModeId.HideAndSeek) &&
            target.IsImpostor() &&
            !__instance.IsRole(RoleId.Jackal))
            return;
        if (target.IsRole(RoleId.Assassin) && target.IsAlive())
        {
            new LateTask(() =>
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    MeetingRoomManager.Instance.AssignSelf(target, null);
                    FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(target);
                    target.RpcStartMeeting(null);
                }
                RoleClass.Assassin.TriggerPlayer = target;
            }, 0.5f, "RpcMurderPlayerCheck Assassin Meeting");
            new LateTask(() =>
            {
                target.RpcSetName($"<size=200%>{CustomOptionHolder.Cs(RoleClass.Marlin.color, IntroData.MarlinIntro.NameKey + "Name")}は誰だ？</size>");
            }, 2f, "RpcMurderPlayerCheck Who Marlin Name");
            new LateTask(() =>
            {
                target.RpcSendChat($"\n{ModTranslation.GetString("MarlinWhois")}");
            }, 2.5f, "RpcMurderPlayerCheck Who Marlin Chat");
            new LateTask(() =>
            {
                target.RpcSetName(target.GetDefaultName());
            }, 2f, "RpcMurderPlayerCheck Default Name");
            return;
        }
        SuperNewRolesPlugin.Logger.LogInfo("i(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
        __instance.RpcMurderPlayerOnCheck(target);
        switch (target.GetRole())
        {
            case RoleId.EvilSeer:
                target.GetRoleBase<EvilSeer>().OnKillSuperHostRolesMode(__instance, target);
                break;
            case RoleId.NekoKabocha:
                NekoKabocha.OnKill(__instance);
                break;
        }
        SuperNewRolesPlugin.Logger.LogInfo("j(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
    }
}
#endregion

#region MurderPlayerPatch

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
public static class MurderPlayerPatch
{
    public static bool resetToCrewmate = false;
    public static bool resetToDead = false;
    public static bool Prefix(PlayerControl __instance, ref PlayerControl target, MurderResultFlags resultFlags)
    {
        __instance.isKilling = false;
        if (resultFlags.HasFlag(MurderResultFlags.FailedError))
        {
            return false;
        }
        if (Knight.GuardedPlayers.Contains(target.PlayerId))
        {
            var Writer = RPCHelper.StartRPC(CustomRPC.KnightProtectClear);
            Writer.Write(target.PlayerId);
            Writer.EndRPC();
            RPCProcedure.KnightProtectClear(target.PlayerId);
            target.protectedByGuardianId = -1;
            return false;
        }
        if (target.IsRole(RoleId.Frankenstein) && Frankenstein.IsMonster(target))
        {
            //相手がフランケン(怪物)で、自分がキルした場合
            if (__instance.AmOwner)
            {
                //音を出して、キルテレポートさせる
                if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.KillSfx, false, 0.8f, null);
                __instance.NetTransform.RpcSnapTo(target.transform.position);
            }
            //自分がフランケン(怪物)で、相手にキルされた場合
            if (target.AmOwner)
            {
                Frankenstein.OnMurderMonster(target);
            }
            return false;
        }
        if (target.IsRole(RoleId.WiseMan) && WiseMan.WiseManData.ContainsKey(target.PlayerId) && WiseMan.WiseManData[target.PlayerId] is not null)
        {
            WiseMan.WiseManData[target.PlayerId] = null;
            PlayerControl targ = target;
            var wisemandata = WiseMan.WiseManPosData.FirstOrDefault(x => x.Key is not null && x.Key.PlayerId == targ.PlayerId);
            if (wisemandata.Key is not null)
                WiseMan.WiseManPosData[wisemandata.Key] = null;
            if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                HudManagerStartPatch.WiseManButton.isEffectActive = false;
                HudManagerStartPatch.WiseManButton.MaxTimer = WiseMan.WiseManCoolTime.GetFloat();
                HudManagerStartPatch.WiseManButton.Timer = HudManagerStartPatch.WiseManButton.MaxTimer;
                Camera.main.GetComponent<FollowerCamera>().Locked = false;
                PlayerControl.LocalPlayer.moveable = true;
            }
            target = __instance;
        }
        EvilGambler.MurderPlayerPrefix(__instance, target);
        Doppelganger.KillCoolSetting.SHRMurderPlayer(__instance, target);
        if (target.IsRole(RoleId.MadRaccoon) &&
            target == PlayerControl.LocalPlayer)
            MadRaccoon.Button.ResetShapeDuration(false);
        //これより下デフォルトのみ実行
        if (!ModeHandler.IsMode(ModeId.Default))
            return true;
        target.resetChange();
        if (RoleClass.Camouflager.IsCamouflage &&
            target.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            __instance.resetChange();
        if (target.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            switch (PlayerControl.LocalPlayer.GetRole())
            {
                case RoleId.SideKiller:
                    var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
                    if (sideplayer == null || sideplayer.IsDead())
                        break;
                    if (!RoleClass.SideKiller.IsUpMadKiller)
                    {
                        sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                        RoleClass.SideKiller.IsUpMadKiller = true;
                    }
                    break;
                case RoleId.EvilSeer:
                    RoleBaseManager.GetLocalRoleBase<EvilSeer>().OnKillDefaultMode(__instance);
                    break;
            }

            if (target.IsRole(RoleId.ShermansServant) &&
                OrientalShaman.IsTransformation)
            {
                OrientalShaman.SetOutfit(target, target.Data.DefaultOutfit);
                OrientalShaman.IsTransformation = false;
            }
        }
        else if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            switch (__instance.GetRole())
            {
                case RoleId.EvilGambler:
                    PlayerControl.LocalPlayer.SetKillTimer(
                        RoleClass.EvilGambler.GetSuc() ?
                        RoleClass.EvilGambler.SucCool :
                        RoleClass.EvilGambler.NotSucCool
                    );
                    break;
            }
        }

        //ダークキラーがキルできるか判定
        if (MapUtilities.CachedShipStatus.Systems.TryGetValue(SystemTypes.Electrical, out ISystemType elecsystem))
        {
            var ma = elecsystem.CastFast<SwitchSystem>();
            if (__instance.IsRole(RoleId.DarkKiller) &&
                ma != null &&
                !ma.IsActive)
                return false;
        }
        if (!AmongUsClient.Instance.AmHost ||
            __instance.PlayerId == target.PlayerId)
            return true;
        switch (target.GetRole())
        {
            case RoleId.Fox:
                Fox.FoxMurderPatch.Guard(__instance, target);
                break;
            case RoleId.NekoKabocha:
                NekoKabocha.OnKill(__instance);
                break;
        }
        return true;
    }
    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        // |:===== targetが生存している場合にも発生させる処理 =====:|
        // SuperNewRolesPlugin.Logger.LogInfo("MurderPlayer発生！元:" + __instance.GetDefaultName() + "、ターゲット:" + target.GetDefaultName());
        // Collect dead player info
        DeadPlayer deadPlayer = new(target, target.PlayerId, DateTime.UtcNow, DeathReason.Kill, __instance);
        DeadPlayer.deadPlayers.Add(deadPlayer);
        FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.Kill;
        ReplayActionMurder.Create(__instance.PlayerId, target.PlayerId);
        CustomRoles.OnKill(deadPlayer);

        // FIXME:狐キル時にはキルクールリセットが発生しないようにして, この処理は死体が発生した時の処理にしたい。
        SerialKiller.MurderPlayer(__instance, target);
        if (target.IsRole(RoleId.Fox) &&
            !RoleClass.Fox.Killer.Contains(__instance.PlayerId))
            RoleClass.Fox.Killer[__instance.PlayerId] = true;


        if (IsDebugMode() &&
            CustomOptionHolder.IsMurderPlayerAnnounce.GetBool())
        {
            new CustomMessage("MurderPlayerが発生しました", 5f);
            Logger.Info("MurderPlayerが発生しました", "DebugMode");
        }

        KnightProtected_Patch.MurderPlayerPatch.Postfix(target);

        if (ModeHandler.IsMode(ModeId.Default))
        {
            Minimalist.MurderPatch.Postfix(__instance);

            Vampire.OnMurderPlayer(__instance, target); // ヴァンパイアと眷属のキルクール同期の為 対象の死亡状態にかかわらず呼び出す

            if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
            {
                if (Squid.Abilitys.IsKillGuard)
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(Squid.SquidNotKillTime.GetFloat(), Squid.SquidNotKillTime.GetFloat());
                    Squid.SetKillTimer(Squid.SquidNotKillTime.GetFloat());
                    Squid.Abilitys.IsKillGuard = false;
                }

                if (__instance.IsImpostor())
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(
                        RoleHelpers.GetCoolTime(__instance, target), RoleHelpers.GetCoolTime(__instance, target)
                        );

                if (PlayerControl.LocalPlayer.IsRole(RoleId.Slugger)) // キルクリセット処理
                {
                    if (Slugger.IsKillCoolSync.GetBool())
                    {
                        PlayerControl.LocalPlayer.GetRoleBase<Slugger>().CustomButtonInfos.FirstOrDefault().ResetCoolTime();
                    }
                }

                EvilGambler.MurderPlayerPostfix(__instance); // キルクリセット処理

                Doppelganger.KillCoolSetting.MurderPlayer(__instance, target); // キルクリセット処理

                if (WaveCannon.IsSyncKillCoolTime.GetBool() &&
                    PlayerControl.LocalPlayer.GetRoleBase() is WaveCannon wavecannon
                    )
                {
                    wavecannon?
                        .CustomButtonInfos?
                        .FirstOrDefault()?
                        .ResetCoolTime();
                }
            }
        }

        // |:===== 以下targetが生存している場合には発生させない処理 =====:|
        if (target.IsAlive()) return;

        Logger.Info("死亡者リストに追加");

        DeadPlayer.ActualDeathTime[target.PlayerId] = (DateTime.Now, __instance);

        if (IsDebugMode() && CustomOptionHolder.IsMurderPlayerAnnounce.GetBool())
        {
            new CustomMessage("\n死者が発生しました", 5f);
            Logger.Info("死者が発生しました", "DebugMode");
        }

        SeerHandler.WrapUpPatch.MurderPlayerPatch.Postfix(target);

        switch (ModeHandler.GetMode())
        {
            case ModeId.SuperHostRoles:
                MurderPlayer.Postfix(__instance, target);
                break;
            case ModeId.Default:
                Levelinger.MurderPlayer(__instance, target);

                if (RoleClass.Camouflager.IsCamouflage)
                {
                    PlayerOutfit outfit = new()
                    {
                        PlayerName = "　",
                        ColorId = RoleClass.Camouflager.Color,
                        SkinId = "",
                        HatId = "",
                        VisorId = "",
                        PetId = "",
                    };
                    target.setOutfit(outfit, true);
                    if (target.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        __instance.setOutfit(outfit, true);
                        if (PlayerControl.LocalPlayer.IsRole(RoleId.Camouflager) && RoleClass.Camouflager.IsCamouflage)
                            Camouflager.RpcResetCamouflage();
                    }
                }

                switch (target.GetRole())
                {
                    case RoleId.NiceMechanic:
                    case RoleId.EvilMechanic:
                        if (target.PlayerId != PlayerControl.LocalPlayer.PlayerId ||
                            !NiceMechanic.TargetVent.TryGetValue(target.PlayerId, out Vent mechanicvent) ||
                            mechanicvent is null)
                            break;
                        Vector3 truepos = target.transform.position;
                        NiceMechanic.RpcSetVentStatusMechanic(
                            PlayerControl.LocalPlayer,
                            mechanicvent,
                            false,
                            new(truepos.x, truepos.y, truepos.z + 0.0025f)
                        );
                        break;
                    case RoleId.Speeder when RoleClass.Speeder.IsSpeedDown:
                        Speeder.SpeedDownEnd();
                        break;
                    case RoleId.Clergyman:
                        RPCProcedure.RPCClergymanLightOut(false);
                        break;
                    case RoleId.OverKiller:
                        FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.OverKillerOverKill;
                        DeadBody deadBodyPrefab = GameManager.Instance.DeadBodyPrefab;
                        Vector3 BodyOffset = target.KillAnimations[0].BodyOffset;
                        for (int i = 0; i < RoleClass.OverKiller.KillCount - 1; i++)
                        {
                            DeadBody deadBody = GameObject.Instantiate(deadBodyPrefab);
                            deadBody.enabled = false;
                            deadBody.ParentId = target.PlayerId;
                            Vector3 position = target.transform.position + BodyOffset;
                            position.z = position.y / 1000f;
                            deadBody.transform.position = position;
                        }
                        break;
                    case RoleId.Jumbo:
                        if (!RoleClass.Jumbo.JumboSize.ContainsKey(target.PlayerId))
                            break;
                        DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i].ParentId == target.PlayerId)
                                array[i].transform.localScale = Vector3.one * (RoleClass.Jumbo.JumboSize[target.PlayerId] + 1f);
                        }
                        break;
                    case RoleId.Assassin:
                        target.Revive();
                        target.Data.IsDead = false;
                        RPCProcedure.CleanBody(target.PlayerId);
                        new LateTask(() =>
                        {
                            if (!AmongUsClient.Instance.AmHost)
                                return;
                            MeetingRoomManager.Instance.AssignSelf(target, null);
                            FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(target);
                            target.RpcStartMeeting(null);
                        }, 0.5f, "MurderPlayer Assassin Meeting");
                        RoleClass.Assassin.TriggerPlayer = target;
                        return;
                    case RoleId.Hitman:
                        if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                            Hitman.Death();
                        break;
                    case RoleId.OrientalShaman:
                        if (!OrientalShaman.OrientalShamanCausative.ContainsKey(target.PlayerId))
                            return;
                        PlayerControl causativePlayer = PlayerById(OrientalShaman.OrientalShamanCausative[target.PlayerId]);
                        if (causativePlayer.IsDead())
                            break;
                        RPCProcedure.RPCMurderPlayer(causativePlayer.PlayerId, causativePlayer.PlayerId, 0);
                        causativePlayer.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
                        break;
                }
                switch (PlayerControl.LocalPlayer.GetRole())
                {
                    case RoleId.Finder:
                        if (PlayerControl.LocalPlayer.PlayerId ==
                            __instance.PlayerId)
                            RoleClass.Finder.KillCount++;
                        break;
                    case RoleId.Painter:
                        if (RoleClass.Painter.CurrentTarget != null &&
                            RoleClass.Painter.CurrentTarget.PlayerId == target.PlayerId)
                            Painter.Handle(Painter.ActionType.Death);
                        break;
                    case RoleId.Psychometrist:
                        Psychometrist.MurderPlayer(__instance, target);
                        break;
                }
                if (__instance.IsRole(RoleId.Frankenstein) && Frankenstein.IsMonster(__instance))
                {
                    //一応会議中に死んだ時とかは蘇らないようにしておく
                    if (!RoleClass.IsMeeting)
                    {
                        new LateTask(() =>
                        {
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ReviveRPC);
                            writer.Write(__instance.PlayerId);
                            writer.EndRPC();
                            RPCProcedure.ReviveRPC(__instance.PlayerId);
                        }, 0.1f, "Revive Frankenstein");
                    }
                    if (__instance.AmOwner)
                        Frankenstein.OnMurderMonster(__instance);
                }
                if (RoleClass.Lovers.SameDie &&
                    target.IsLovers() &&
                    __instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    PlayerControl SideLoverPlayer = target.GetOneSideLovers();
                    if (SideLoverPlayer.IsAlive())
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                        writer.Write(SideLoverPlayer.PlayerId);
                        writer.Write(SideLoverPlayer.PlayerId);
                        writer.Write(byte.MaxValue);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.RPCMurderPlayer(SideLoverPlayer.PlayerId, SideLoverPlayer.PlayerId, byte.MaxValue);
                        SideLoverPlayer.RpcSetFinalStatus(FinalStatus.LoversBomb);
                    }
                }
                if (target.IsQuarreled() && AmongUsClient.Instance.AmHost)
                {
                    if (__instance.IsQuarreled()) RoleClass.Quarreled.IsQuarreledSuicide = true;
                    var Side = RoleHelpers.GetOneSideQuarreled(target);
                    if (Side.IsDead())
                    {
                        if (RoleClass.Quarreled.IsQuarreledSuicide) return;
                        RPCProcedure.ShareWinner(target.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RoleClass.Quarreled.IsQuarreledWin = true;
                        GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                    }
                }
                break;
        }
    }
}
#endregion