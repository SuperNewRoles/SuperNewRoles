using System;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.BattleRoyal.BattleRole;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay.ReplayActions;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Patches;

#region CheckShapeshiftPatch
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckShapeshift))]
class CheckShapeshiftPatch
{
    public static bool Prefix(PlayerControl __instance, PlayerControl target, bool shouldAnimate)
    {
        __instance.logger.Debug($"Checking if {__instance.PlayerId} can shapeshift into {(target == null ? "null player" : target.PlayerId.ToString())}");
        if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
            return false;
        if (target == null ||
            target.Data == null ||
            __instance.Data.IsDead || __instance.Data.Disconnected)
        {
            int num = target != null ? target.PlayerId : -1;
            __instance.logger.Warning($"Bad shapeshift from {__instance.PlayerId} to {num}");
            __instance.RpcRejectShapeshift();
            return false;
        }
        if (target.IsMushroomMixupActive() && shouldAnimate)
        {
            __instance.logger.Warning("Tried to shapeshift while mushroom mixup was active");
            __instance.RpcRejectShapeshift();
            return false;
        }
        if (MeetingHud.Instance && shouldAnimate)
        {
            __instance.logger.Warning("Tried to shapeshift while a meeting was starting");
            __instance.RpcRejectShapeshift();
            return false;
        }
        //ここからMod側のチェック
        if (!HandleShapeshiftCheck(__instance, target, shouldAnimate, out bool reject))
        {
            //まあでも解除する場合はそのまま解除で通した方がいいよね
            if (__instance.PlayerId == target.PlayerId && !reject)
                __instance.RpcShapeshift(__instance, shouldAnimate);
            else
                __instance.RpcRejectShapeshift();
            return false;
        }
        __instance.RpcShapeshift(target, shouldAnimate);
        return false;
    }
    static bool HandleSHRShapeshiftCheck(PlayerControl __instance, PlayerControl target, bool shouldAnimate, out bool reject)
    {
        reject = false;
        //以下解除ならワンクリックボタンor処理しない
        if (__instance == target)
        {
            if (MeetingHud.Instance != null)
                return true;
            if (__instance.GetRoleBase() is ISHROneClickShape oneClickShape)
            {
                oneClickShape.OnOneClickShape();
                return !(reject = true);
            }
            else
            {
                switch (__instance.GetRole())
                {
                    case RoleId.SelfBomber:
                        reject = true;
                        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                        {
                            if (p.IsAlive() && p.PlayerId != __instance.PlayerId && SelfBomber.GetIsBomb(__instance, p, CustomOptionHolder.SelfBomberScope.GetFloat()))
                            {
                                __instance.RpcMurderPlayerCheck(p);
                                p.RpcSetFinalStatus(FinalStatus.BySelfBomberBomb);
                            }
                        }
                        __instance.RpcMurderPlayer(__instance, true);
                        __instance.RpcSetFinalStatus(FinalStatus.SelfBomberBomb);
                        return false;
                    default:
                        break;
                }
            }
            return true;
        }
        switch (__instance.GetRole())
        {
            case RoleId.Samurai:
                if (RoleClass.Samurai.SwordedPlayer.Contains(__instance.PlayerId))
                    return false;
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (p.IsDead() ||
                        p.PlayerId == __instance.PlayerId ||
                        !SelfBomber.GetIsBomb(__instance, p, CustomOptionHolder.SamuraiScope.GetFloat()))
                        continue;
                    p.RpcSetFinalStatus(FinalStatus.SamuraiKill);
                    __instance.RpcMurderPlayerCheck(p);
                }
                RoleClass.Samurai.SwordedPlayer.Add(__instance.PlayerId);
                return false;
            case RoleId.Arsonist:
                foreach (PlayerControl p in RoleClass.Arsonist.ArsonistPlayer.AsSpan())
                {
                    if (!Arsonist.IsWin(p)) continue;
                    RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    Writer.Write(p.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);

                    Arsonist.SettingAfire();

                    Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                    Writer.Write((byte)CustomGameOverReason.ArsonistWin);
                    Writer.EndRPC();
                    RPCProcedure.SetWinCond((byte)CustomGameOverReason.ArsonistWin);

                    RoleClass.Arsonist.TriggerArsonistWin = true;
                    AdditionalTempData.winCondition = WinCondition.ArsonistWin;
                    __instance.enabled = false;
                    GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                    return true;
                }
                return false;
            case RoleId.SuicideWisher:
                __instance.RpcMurderPlayer(__instance, true);
                __instance.RpcSetFinalStatus(FinalStatus.SuicideWisherSelfDeath);
                return false;
            case RoleId.ToiletFan:
                RPCHelper.RpcOpenToilet();
                return false;
            case RoleId.NiceButtoner:
                if (!RoleClass.NiceButtoner.SkillCountSHR.ContainsKey(__instance.PlayerId))
                    RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId] = CustomOptionHolder.NiceButtonerCount.GetInt();
                RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId]--;
                if (RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId] >= 0)
                    EvilButtoner.EvilButtonerStartMeetingSHR(__instance);
                return false;
            case RoleId.EvilButtoner:
                if (!RoleClass.EvilButtoner.SkillCountSHR.ContainsKey(__instance.PlayerId))
                    RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId] = CustomOptionHolder.EvilButtonerCount.GetInt();
                RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId]--;
                if (RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId] >= 0)
                    EvilButtoner.EvilButtonerStartMeetingSHR(__instance);
                return false;
            case RoleId.EvilSeer:
                return false;//shapeとしての能力は持たせない為、誤爆封じで導入者のみ使用不可にする。
            case RoleId.RemoteSheriff:
                //対象が死んでいる場合はシェイプシフトできない
                if (target.IsDead())
                    return false;
                //もう残り回数がない場合はシェイプシフトできない
                if (RoleClass.RemoteSheriff.KillCount.ContainsKey(__instance.PlayerId) &&
                    RoleClass.RemoteSheriff.KillCount[__instance.PlayerId] <= 0)
                    return false;
                break;
            case RoleId.Pavlovsdogs:
                return false;
        }
        return true;
    }
    static bool HandleShapeshiftCheck(PlayerControl __instance, PlayerControl target, bool shouldAnimate, out bool reject)
    {
        reject = false;
        if (RoleClass.Assassin.TriggerPlayer != null)
            return false;
        if (target.IsBot())
            return false;
        // バトルロイヤルモードで、
        // 対象が対象以外のプレイヤーに
        // シェイプシフトしようとした時
        if (ModeHandler.IsMode(ModeId.BattleRoyal)
            && __instance.PlayerId != target.PlayerId
            && Mode.BattleRoyal.Main.StartSeconds <= 0)
        {
            BattleRoyalRole.GetObject(__instance).UseAbility(target);
            return false;
        }

        bool isActivationShapeshift = __instance.PlayerId != target.PlayerId; // true : シェイプシフトする時 / false : シェイプシフトを解除する時

        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            bool ShapeshiftResultSHR = HandleSHRShapeshiftCheck(__instance, target, shouldAnimate, out reject);
            //falseが返ってきたらシェイプシフトできない
            if (!ShapeshiftResultSHR)
                return false;
        }
        //おめでとう！無事にシェイプシフトできました！
        return true;
    }
}
#endregion
#region ShapeshiftPatch
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
class RpcShapeshiftPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        SyncSetting.CustomSyncSettings();

        bool isActivationShapeshift = __instance.PlayerId != target.PlayerId; // true : シェイプシフトする時 / false : シェイプシフトを解除する時

        if (__instance.IsRole(RoleId.MadRaccoon) &&
            __instance == PlayerControl.LocalPlayer) // 導入者が個人で行う処理 (SHR, SNR共通)
        {
            if (isActivationShapeshift)
                MadRaccoon.Button.SetShapeDurationTimer();
            else
                MadRaccoon.Button.ResetShapeDuration(false);
        }
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && !AmongUsClient.Instance.AmHost)
            return true;
        if (isActivationShapeshift)
        {
            if (__instance.IsRole(RoleId.Doppelganger))
            {
                RoleClass.Doppelganger.Targets.Add(__instance.PlayerId, target);
                SuperNewRolesPlugin.Logger.LogInfo($"{__instance.Data.PlayerName}のターゲットが{target.Data.PlayerName}に変更");
            }
        }
        else
        {
            if (__instance.IsRole(RoleId.Doppelganger))
            {
                RoleClass.Doppelganger.Targets.Remove(__instance.PlayerId);
                SuperNewRolesPlugin.Logger.LogInfo($"{__instance.Data.PlayerName}のターゲット、{target.Data.PlayerName}を削除");
            }
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (__instance.IsRole(RoleId.Doppelganger))
                {
                    Doppelganger.ResetShapeCool();
                }
            }
            else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                if (__instance.IsRole(RoleId.RemoteSheriff))
                {
                    __instance.RpcShowGuardEffect(target);
                }
            }
            return true;
        }
        if (__instance.IsRole(RoleId.ShiftActor))
        {
            ShiftActor.Shapeshift(__instance, target);
            return true;
        }
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            switch (__instance.GetRole())
            {
                case RoleId.RemoteSheriff:
                    (var killResult, var suicideResult) = Sheriff.SheriffKillResult(__instance, target);

                    if (killResult.Item1)
                    {
                        FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = killResult.Item2;
                        __instance.RpcMurderPlayerCheck(target);
                        target.RpcSetFinalStatus(killResult.Item2);

                        if (RoleClass.RemoteSheriff.KillCount.ContainsKey(__instance.PlayerId))
                            RoleClass.RemoteSheriff.KillCount[__instance.PlayerId]--;
                        else
                            RoleClass.RemoteSheriff.KillCount[__instance.PlayerId] = CustomOptionHolder.RemoteSheriffKillMaxCount.GetInt() - 1;
                        if (RoleClass.RemoteSheriff.IsKillTeleport)
                            __instance.RpcMurderPlayerCheck(target);
                        else
                        {
                            target.RpcMurderPlayer(target, true);
                            __instance.RpcShowGuardEffect(__instance);
                        }
                        ChangeName.SetRoleName(__instance);
                    }

                    if (suicideResult.Item1)
                    {
                        FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = suicideResult.Item2;
                        __instance.RpcMurderPlayer(__instance, true);
                        __instance.RpcSetFinalStatus(suicideResult.Item2);
                    }
                    return true;
                case RoleId.Camouflager:
                    RoleClass.Camouflager.Duration = RoleClass.Camouflager.DurationTime;
                    RoleClass.Camouflager.ButtonTimer = DateTime.Now;
                    RoleClass.Camouflager.IsCamouflage = true;
                    Camouflager.CamouflageSHR();
                    SyncSetting.CustomSyncSettings(__instance);
                    return true;
                case RoleId.Worshiper:
                    __instance.RpcMurderPlayer(__instance, true);
                    __instance.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
                    return true;
            }
        }
        return true;
    }
    public static void Postfix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
    {
        ReplayActionShapeshift.Create(__instance.PlayerId, targetPlayer.PlayerId, animate);
    }
}
#endregion

#region ShapeshiftMinigameBeginPatch
[HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
class ShapeshifterMinigameBeginPatch
{
    private static void NewTask(ShapeshifterMinigame __instance)
    {
        new LateTask(() =>
        {
            if (__instance == null) return;
            __instance.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.transform.localScale;
            NewTask(__instance);
        }, 0.1f);
    }
    private static void Create(ShapeshifterPanel panel, int index, Action action)
    {
        panel.SetPlayer(index, CachedPlayer.LocalPlayer.Data, (Action)(() =>
        {
            if (MeetingHud.Instance != null) MeetingHud.Instance.transform.FindChild("ButtonStuff").gameObject.SetActive(true);
            action();
        }));
    }
    public static void Postfix(ShapeshifterMinigame __instance, PlayerTask task)
    {
        //** Debuggerの処理 **//
        if (RoleClass.Debugger.AmDebugger)
        {
            Roles.Attribute.Debugger.CreateDebugMenu(__instance);
        }

        //デバッガー + GMだと問題起こりそうですがそうそうないと思うのでﾖｼｯ!
        //** GMの処理 **//
        if (!PlayerControl.LocalPlayer.IsRole(RoleId.GM)) return;

        NewTask(__instance);
        foreach (ShapeshifterPanel panel in GameObject.FindObjectsOfType<ShapeshifterPanel>()) GameObject.Destroy(panel.gameObject);
        int index = 0;
        foreach (var Data in Roles.Neutral.GM.ActionDictionary)
        {
            int num = index % 3;
            int num2 = index / 3;
            ShapeshifterPanel panel = GameObject.Instantiate(__instance.PanelPrefab, __instance.transform);
            panel.transform.localPosition = new Vector3(__instance.XStart + (float)num * __instance.XOffset, __instance.YStart + (float)num2 * __instance.YOffset, -1f);

            Create(panel, index, Data.Value);
            panel.PlayerIcon.gameObject.SetActive(false);
            panel.LevelNumberText.transform.parent.gameObject.SetActive(false);
            // panel.transform.FindChild("Nameplate").GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
            panel.NameText.text = ModTranslation.GetString(Data.Key);
            panel.NameText.transform.localPosition = new(0, 0, -0.1f);
            index++;
        }
        if (MeetingHud.Instance != null)
        {
            MeetingHud.Instance.transform.FindChild("ButtonStuff").gameObject.SetActive(false);
            __instance.transform.FindChild("CloseButton").GetComponent<PassiveButton>().OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                MeetingHud.Instance.transform.FindChild("ButtonStuff").gameObject.SetActive(true);
            }));
        }
    }
}
#endregion

#region ShapeshiftMinigameShapeshiftPatch
[HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Shapeshift))]
class ShapeshifterMinigameShapeshiftPatch
{
    public static bool Prefix(ShapeshifterMinigame __instance, [HarmonyArgument(0)] PlayerControl player)
    {
        if (player.IsBot()) return false;
        if (PlayerControl.LocalPlayer.inVent)
        {
            __instance.Close();
            return false;
        }
        if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
        {
            if (RoleClass.RemoteSheriff.KillMaxCount > 0)
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    new LateTask(() =>
                    {
                        if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
                        {
                            PlayerControl.LocalPlayer.RpcShapeshift(PlayerControl.LocalPlayer, true);
                        }
                    }, 1.5f, "SHR RemoteSheriff Shape Revert");
                    PlayerControl.LocalPlayer.RpcShapeshift(player, true);
                }
                else if (ModeHandler.IsMode(ModeId.Default))
                {
                    if (player.IsAlive())
                    {
                        var target = player;
                        (var killResult, var suicideResult) = Sheriff.SheriffKillResult(CachedPlayer.LocalPlayer, target);

                        PlayerControl.LocalPlayer.RpcShapeshift(PlayerControl.LocalPlayer, true);

                        var localId = CachedPlayer.LocalPlayer.PlayerId;
                        var targetId = target.PlayerId;

                        RPCProcedure.SheriffKill(localId, targetId, killResult.Item1, suicideResult.Item1);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SheriffKill, SendOption.Reliable, -1);
                        killWriter.Write(localId);
                        killWriter.Write(targetId);
                        killWriter.Write(killResult.Item1);
                        killWriter.Write(suicideResult.Item1);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);

                        if (killResult.Item1) FinalStatusClass.RpcSetFinalStatus(target, killResult.Item2);
                        if (suicideResult.Item1) FinalStatusClass.RpcSetFinalStatus(CachedPlayer.LocalPlayer, suicideResult.Item2);

                        RoleClass.RemoteSheriff.KillMaxCount--;
                    }
                    Sheriff.ResetKillCooldown();
                };
            }
            __instance.Close();
            return false;
        }
        PlayerControl.LocalPlayer.RpcShapeshift(player, true);
        __instance.Close();
        return false;
    }
}
#endregion