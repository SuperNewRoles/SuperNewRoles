using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using TMPro;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;
using static SuperNewRoles.Helpers.DesyncHelpers;
using static SuperNewRoles.ModHelpers;
using static GameData;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcUsePlatform))]
    public class UsePlatformPlayerControlPatch
    {
        public static bool Prefix(PlayerControl __instance)
        {
            Logger.Info("来た");
            if (AmongUsClient.Instance.AmHost)
            {
                AirshipStatus airshipStatus = GameObject.FindObjectOfType<AirshipStatus>();
                if (airshipStatus)
                {
                    airshipStatus.GapPlatform.Use(__instance);
                }
            }
            else
            {
                AmongUsClient.Instance.StartRpc(__instance.NetId, 32, (SendOption)1).EndMessage();
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
    class RpcShapeshiftPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            SyncSetting.CustomSyncSettings();
            if (RoleClass.Assassin.TriggerPlayer != null) return false;
            if (target.IsBot()) return true;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles) && !AmongUsClient.Instance.AmHost) return true;
            if (__instance.PlayerId != target.PlayerId)
            {
                if (__instance.IsRole(RoleId.Doppelganger))
                {
                    RoleClass.Doppelganger.Targets.Add(__instance.PlayerId, target);
                    SuperNewRolesPlugin.Logger.LogInfo($"{__instance.Data.PlayerName}のターゲットが{target.Data.PlayerName}に変更");
                }
            }
            if (__instance.PlayerId == target.PlayerId)
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
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
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
                Roles.Impostor.ShiftActor.Shapeshift(__instance, target);
                return true;
            }
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                switch (__instance.GetRole())
                {
                    case RoleId.RemoteSheriff:
                        if (target.IsDead()) return true;
                        if (!RoleClass.RemoteSheriff.KillCount.ContainsKey(__instance.PlayerId) || RoleClass.RemoteSheriff.KillCount[__instance.PlayerId] >= 1)
                        {
                            if (!Sheriff.IsRemoteSheriffKill(target) || target.IsRole(RoleId.RemoteSheriff))
                            {
                                if (!Sheriff.IsRemoteSheriffKill(target) || target.IsRole(RoleId.RemoteSheriff))
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.SheriffMisFire;
                                    __instance.RpcMurderPlayer(__instance);
                                    FinalStatusClass.RpcSetFinalStatus(__instance, FinalStatus.RemoteSheriffMisFire);
                                    return true;
                                }
                                else
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.SheriffKill;
                                    if (RoleClass.RemoteSheriff.KillCount.ContainsKey(__instance.PlayerId))
                                        RoleClass.RemoteSheriff.KillCount[__instance.PlayerId]--;
                                    else
                                        RoleClass.RemoteSheriff.KillCount[__instance.PlayerId] = CustomOptions.RemoteSheriffKillMaxCount.GetInt() - 1;
                                    if (RoleClass.RemoteSheriff.IsKillTeleport)
                                        __instance.RpcMurderPlayerCheck(target);
                                    else
                                    {
                                        target.RpcMurderPlayer(target);
                                        __instance.RpcShowGuardEffect(__instance);
                                    }
                                    FinalStatusClass.RpcSetFinalStatus(target, target.IsRole(RoleId.HauntedWolf) ? FinalStatus.RemoteSheriffHauntedWolfKill : FinalStatus.RemoteSheriffKill);
                                    Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                    return true;
                                }
                            }
                        }
                        return true;
                    case RoleId.SelfBomber:
                        foreach (PlayerControl p in CachedPlayer.AllPlayers)
                        {
                            if (p.IsAlive() && p.PlayerId != __instance.PlayerId)
                            {
                                if (SelfBomber.GetIsBomb(__instance, p, CustomOptions.SelfBomberScope.GetFloat()))
                                {
                                    __instance.RpcMurderPlayerCheck(p);
                                    p.RpcSetFinalStatus(FinalStatus.BySelfBomberBomb);
                                }
                            }
                        }
                        __instance.RpcMurderPlayer(__instance);
                        __instance.RpcSetFinalStatus(FinalStatus.SelfBomberBomb);
                        return false;
                    case RoleId.Samurai:
                        if (!RoleClass.Samurai.SwordedPlayer.Contains(__instance.PlayerId))
                        {
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                if (p.IsAlive() && p.PlayerId != __instance.PlayerId)
                                {
                                    if (SelfBomber.GetIsBomb(__instance, p, CustomOptions.SamuraiScope.GetFloat()))
                                    {
                                        __instance.RpcSetFinalStatus(FinalStatus.SamuraiKill);
                                        __instance.RpcMurderPlayerCheck(p);
                                    }
                                }
                            }
                            RoleClass.Samurai.SwordedPlayer.Add(__instance.PlayerId);
                        }
                        return false;
                    case RoleId.Arsonist:
                        foreach (PlayerControl p in RoleClass.Arsonist.ArsonistPlayer)
                        {
                            if (Arsonist.IsWin(p))
                            {
                                RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                                MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                                Writer.Write(p.PlayerId);
                                AmongUsClient.Instance.FinishRpcImmediately(Writer);

                                Writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                                Writer.Write((byte)CustomGameOverReason.ArsonistWin);
                                Writer.EndRPC();
                                RPCProcedure.SetWinCond((byte)CustomGameOverReason.ArsonistWin);

                                RoleClass.Arsonist.TriggerArsonistWin = true;
                                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
                                __instance.enabled = false;
                                ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.ArsonistWin, false);
                                return true;
                            }
                        }
                        return false;
                    case RoleId.SuicideWisher:
                        __instance.RpcMurderPlayer(__instance);
                        __instance.RpcSetFinalStatus(FinalStatus.SuicideWisherSelfDeath);
                        return false;
                    case RoleId.ToiletFan:
                        RPCHelper.RpcOpenToilet();
                        return false;
                    case RoleId.NiceButtoner:
                        if (RoleClass.NiceButtoner.SkillCountSHR.ContainsKey(__instance.PlayerId))
                            RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId]--;
                        else
                            RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId] = CustomOptions.NiceButtonerCount.GetInt() - 1;
                        if (RoleClass.NiceButtoner.SkillCountSHR[__instance.PlayerId] + 1 >= 1)
                            EvilButtoner.EvilButtonerStartMeetingSHR(__instance);
                        return false;
                    case RoleId.EvilButtoner:
                        if (RoleClass.EvilButtoner.SkillCountSHR.ContainsKey(__instance.PlayerId))
                            RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId]--;
                        else
                            RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId] = CustomOptions.EvilButtonerCount.GetInt() - 1;
                        if (RoleClass.EvilButtoner.SkillCountSHR[__instance.PlayerId] + 1 >= 1)
                            EvilButtoner.EvilButtonerStartMeetingSHR(__instance);
                        return false;
                    case RoleId.Camouflager:
                        if (AmongUsClient.Instance.AmHost)
                        {
                            RoleClass.Camouflager.Duration = RoleClass.Camouflager.DurationTime;
                            RoleClass.Camouflager.IsCamouflage = true;
                            Roles.Impostor.Camouflager.Camouflage();
                            SyncSetting.CustomSyncSettings(__instance);
                        }
                        return true;
                }
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
    class CheckProtectPatch
    {
        public static bool Prefix()
        {
            return !ModeHandler.IsMode(ModeId.SuperHostRoles);
        }
    }
    [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.Begin))]
    class ShapeshifterMinigameBeginPatch
    {
        public static void Postfix(ShapeshifterMinigame __instance, PlayerTask task)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.GM))
            {
                static void NewTask(ShapeshifterMinigame __instance)
                {
                    new LateTask(() =>
                    {
                        if (__instance == null) return;
                        __instance.transform.localScale = FastDestroyableSingleton<HudManager>.Instance.transform.localScale;
                        NewTask(__instance);
                    }, 0.1f);
                }
                NewTask(__instance);
                foreach (ShapeshifterPanel panel in GameObject.FindObjectsOfType<ShapeshifterPanel>()) GameObject.Destroy(panel.gameObject);
                int index = 0;
                foreach (var Data in Roles.Neutral.GM.ActionDictionary)
                {
                    int num = index % 3;
                    int num2 = index / 3;
                    ShapeshifterPanel panel = GameObject.Instantiate(__instance.PanelPrefab, __instance.transform);
                    panel.transform.localPosition = new Vector3(__instance.XStart + (float)num * __instance.XOffset, __instance.YStart + (float)num2 * __instance.YOffset, -1f);
                    static void Create(ShapeshifterPanel panel, int index, Action action)
                    {
                        panel.SetPlayer(index, CachedPlayer.LocalPlayer.Data, (Action)(() =>
                        {
                            if (MeetingHud.Instance != null) MeetingHud.Instance.transform.FindChild("ButtonStuff").gameObject.SetActive(true);
                            action();
                        }));
                    }
                    Create(panel, index, Data.Value);
                    panel.PlayerIcon.gameObject.SetActive(false);
                    panel.LevelNumberText.transform.parent.gameObject.SetActive(false);
                    panel.transform.FindChild("Nameplate").GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
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
    }
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
                                PlayerControl.LocalPlayer.RpcRevertShapeshift(true);
                            }
                        }, 1.5f, "SHR RemoteSheriff Shape Revert");
                        PlayerControl.LocalPlayer.RpcShapeshift(player, true);
                    }
                    else if (ModeHandler.IsMode(ModeId.Default))
                    {
                        if (player.IsAlive())
                        {
                            var Target = player;
                            var misfire = !Sheriff.IsRemoteSheriffKill(Target);
                            var TargetID = Target.PlayerId;
                            var LocalID = CachedPlayer.LocalPlayer.PlayerId;

                            PlayerControl.LocalPlayer.RpcShapeshift(PlayerControl.LocalPlayer, true);

                            RPCProcedure.SheriffKill(LocalID, TargetID, misfire);
                            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SheriffKill, SendOption.Reliable, -1);
                            killWriter.Write(LocalID);
                            killWriter.Write(TargetID);
                            killWriter.Write(misfire);
                            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                            FinalStatusClass.RpcSetFinalStatus(misfire ? CachedPlayer.LocalPlayer : Target, misfire ? FinalStatus.RemoteSheriffMisFire : (Target.IsRole(RoleId.HauntedWolf) ? FinalStatus.RemoteSheriffHauntedWolfKill : FinalStatus.RemoteSheriffKill));
                            RoleClass.RemoteSheriff.KillMaxCount--;
                        }
                        Sheriff.ResetKillCoolDown();
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
    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    class KillButtonDoClickPatch
    {
        public static bool Prefix(KillButton __instance)
        {
            if (!ModeHandler.IsMode(ModeId.Default))
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
                    {
                        if (__instance.isActiveAndEnabled && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove && !__instance.isCoolingDown && RoleClass.RemoteSheriff.KillMaxCount > 0)
                        {
                            DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
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
                            DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);
                            PlayerControl.LocalPlayer.killTimer = 0.001f;
                        }
                        return false;
                    }
                }
                return true;
            }
            if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && PlayerControl.LocalPlayer.IsAlive() && PlayerControl.LocalPlayer.CanMove)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Kunoichi))
                {
                    Kunoichi.KillButtonClick();
                    return false;
                }
                if (!(__instance.currentTarget.IsRole(RoleId.Bait) || __instance.currentTarget.IsRole(RoleId.NiceRedRidingHood)) && PlayerControl.LocalPlayer.IsRole(RoleId.Vampire))
                {
                    PlayerControl.LocalPlayer.killTimer = RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer);
                    RoleClass.Vampire.target = __instance.currentTarget;
                    RoleClass.Vampire.KillTimer = DateTime.Now;
                    RoleClass.Vampire.Timer = RoleClass.Vampire.KillDelay;
                    return false;
                }
                bool showAnimation = true;

                // Use an unchecked kill command, to allow shorter kill cooldowns etc. without getting kicked
                MurderAttemptResult res = CheckMuderAttemptAndKill(PlayerControl.LocalPlayer, __instance.currentTarget, showAnimation: showAnimation);
                // Handle blank kill
                if (res == MurderAttemptResult.BlankKill)
                {
                    PlayerControl.LocalPlayer.killTimer = RoleHelpers.GetCoolTime(PlayerControl.LocalPlayer);
                }
                __instance.SetTarget(null);
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
    static class CheckMurderPatch
    {
        public static bool isKill = false;
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            Logger.Info($"{__instance.Data.PlayerName}=>{target.Data.PlayerName}", "CheckMurder");
            if (__instance.IsBot() || target.IsBot()) return false;
            Logger.Info("Bot通過", "CheckMurder");
            if (__instance.IsDead() || target.IsDead()) return false;
            Logger.Info("死亡通過", "CheckMurder");
            if (!RoleClass.IsStart && AmongUsClient.Instance.GameMode != GameModes.FreePlay) return false;
            Logger.Info("非スタート通過", "CheckMurder");
            if (__instance.PlayerId == target.PlayerId)
            {
                Logger.Info($"自爆:{target.name}", "CheckMurder");
                __instance.RpcMurderPlayer(target);
                return false;
            }
            Logger.Info("自爆通過", "CheckMurder");
            if (!AmongUsClient.Instance.AmHost) return true;
            Logger.Info("非ホスト通過", "CheckMurder");
            switch (ModeHandler.GetMode())
            {
                case ModeId.Zombie:
                    return false;
                case ModeId.BattleRoyal:
                    if (isKill)
                    {
                        return false;
                    }
                    if (Mode.BattleRoyal.Main.StartSeconds <= 0)
                    {
                        if (Mode.BattleRoyal.Main.IsTeamBattle)
                        {
                            foreach (List<PlayerControl> teams in Mode.BattleRoyal.Main.Teams)
                            {
                                if (teams.Count > 0)
                                {
                                    if (teams.IsCheckListPlayerControl(__instance) && teams.IsCheckListPlayerControl(target))
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        SuperNewRolesPlugin.Logger.LogInfo("[CheckMurder]LateTask:" + (AmongUsClient.Instance.Ping / 1000f) * 2f);
                        isKill = true;
                        if (__instance.PlayerId != 0)
                        {
                            target.Data.IsDead = true;
                            __instance.RpcMurderPlayer(target);
                            Mode.BattleRoyal.Main.MurderPlayer(__instance, target);
                            isKill = false;
                        }
                        else
                        {
                            new LateTask(() =>
                            {
                                if (__instance.IsAlive() && target.IsAlive())
                                {
                                    __instance.RpcMurderPlayer(target);
                                    Mode.BattleRoyal.Main.MurderPlayer(__instance, target);
                                }
                                isKill = false;
                            }, AmongUsClient.Instance.Ping / 1000f * 1.1f, "BattleRoyal Murder");
                        }
                    }
                    return false;
                case ModeId.SuperHostRoles:
                    Logger.Info("SHR", "CheckMurder");
                    if (RoleClass.Assassin.TriggerPlayer != null) return false;
                    Logger.Info("SHR-Assassin.TriggerPlayerを通過", "CheckMurder");
                    if (target.IsRole(RoleId.NekoKabocha))
                    {
                        NekoKabocha.OnKill(__instance);
                        return true;
                    }
                    foreach (var p in Seer.Seers)
                    {
                        foreach (var p2 in p)
                        {
                            if (!p2.IsMod())
                                p2.ShowReactorFlash(1.5f);
                        }
                    }
                    switch (__instance.GetRole())
                    {
                        case RoleId.RemoteSheriff:
                        case RoleId.ToiletFan:
                        case RoleId.NiceButtoner:
                            return false;
                        case RoleId.Egoist:
                            if (!RoleClass.Egoist.UseKill) return false;
                            break;
                        case RoleId.FalseCharges:
                            target.RpcMurderPlayer(__instance);
                            target.RpcSetFinalStatus(FinalStatus.FalseChargesFalseCharge);
                            RoleClass.FalseCharges.FalseChargePlayers[__instance.PlayerId] = target.PlayerId;
                            RoleClass.FalseCharges.AllTurns[__instance.PlayerId] = RoleClass.FalseCharges.DefaultTurn;
                            return false;
                        case RoleId.truelover:
                            if (!__instance.IsLovers())
                            {
                                if (target == null || target.IsLovers() || RoleClass.Truelover.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                __instance.RpcShowGuardEffect(target);
                                RoleClass.Truelover.CreatePlayers.Add(__instance.PlayerId);
                                RoleHelpers.SetLovers(__instance, target);
                                RoleHelpers.SetLoversRPC(__instance, target);
                                //__instance.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);
                            }
                            return false;
                        case RoleId.Sheriff:
                            if (!RoleClass.Sheriff.KillCount.ContainsKey(__instance.PlayerId) || RoleClass.Sheriff.KillCount[__instance.PlayerId] >= 1)
                            {
                                if (!Sheriff.IsSheriffKill(target))
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.SheriffMisFire;
                                    __instance.RpcMurderPlayer(__instance);
                                    __instance.RpcSetFinalStatus(FinalStatus.SheriffMisFire);
                                }
                                else
                                {
                                    FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.SheriffKill;
                                    if (RoleClass.Sheriff.KillCount.ContainsKey(__instance.PlayerId))
                                    {
                                        RoleClass.Sheriff.KillCount[__instance.PlayerId]--;
                                    }
                                    else
                                    {
                                        RoleClass.Sheriff.KillCount[__instance.PlayerId] = CustomOptions.SheriffKillMaxCount.GetInt() - 1;
                                    }
                                    __instance.RpcMurderPlayerCheck(target);
                                    if (target.IsRole(RoleId.HauntedWolf)) __instance.RpcSetFinalStatus(FinalStatus.SheriffHauntedWolfKill);
                                    else __instance.RpcSetFinalStatus(FinalStatus.SheriffKill);
                                    Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                }
                            }
                            return false;
                        case RoleId.MadMaker:
                            if (!target.IsImpostor())
                            {
                                if (target == null || RoleClass.MadMaker.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                __instance.RpcShowGuardEffect(target);
                                RoleClass.MadMaker.CreatePlayers.Add(__instance.PlayerId);
                                target.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                                target.SetRoleRPC(RoleId.MadMate);
                                //__instance.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);
                            }
                            else
                            {
                                __instance.RpcMurderPlayer(__instance);
                                __instance.RpcSetFinalStatus(FinalStatus.MadmakerMisSet);
                            }
                            return false;
                        case RoleId.Demon:
                            if (!__instance.IsCursed(target))
                            {
                                Demon.DemonCurse(target, __instance);
                                __instance.RpcShowGuardEffect(target);
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                            }
                            return false;
                        case RoleId.OverKiller:
                            __instance.RpcMurderPlayerCheck(target);
                            foreach (PlayerControl p in CachedPlayer.AllPlayers)
                            {
                                if (!p.Data.Disconnected && p.PlayerId != target.PlayerId && p.IsPlayer())
                                {
                                    if (p.PlayerId != 0)
                                    {
                                        for (int i = 0; i < RoleClass.OverKiller.KillCount - 1; i++)
                                        {
                                            __instance.RPCMurderPlayerPrivate(target, p);
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < RoleClass.OverKiller.KillCount - 1; i++)
                                        {
                                            __instance.MurderPlayer(target);
                                        }
                                    }
                                }
                            }
                            return false;
                        case RoleId.Arsonist:
                            if (!__instance.IsDoused(target))
                            {
                                __instance.RpcShowGuardEffect(target);// 守護エフェクト
                                SyncSetting.OptionData.DeepCopy().RoleOptions.ShapeshifterCooldown = RoleClass.Arsonist.DurationTime;// シェイプクールダウンを塗り時間に
                                new LateTask(() =>
                                {
                                    if (Vector2.Distance(__instance.transform.position, target.transform.position) <= 1.75f)//1.75f以内にターゲットがいるなら
                                    {
                                        Arsonist.ArsonistDouse(target, __instance);
                                        __instance.RpcShowGuardEffect(target);// もう一度エフェクト
                                        Mode.SuperHostRoles.FixedUpdate.SetRoleName(__instance);
                                    }
                                    else
                                    {//塗れなかったらキルクールリセット
                                        SyncSetting.OptionData.DeepCopy().KillCooldown = SyncSetting.KillCoolSet(0f);
                                    }
                                }, RoleClass.Arsonist.DurationTime, "SHR Arsonist Douse");
                            }
                            return false;
                        case RoleId.Mafia:
                            if (!Mafia.IsKillFlag()) return false;
                            break;
                        case RoleId.FastMaker:
                            if (!RoleClass.FastMaker.IsCreatedMadMate)//まだ作ってなくて、設定が有効の時
                            {
                                if (target == null || RoleClass.FastMaker.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                __instance.RpcShowGuardEffect(target);
                                RoleClass.FastMaker.CreatePlayers.Add(__instance.PlayerId);
                                target.RpcSetRoleDesync(RoleTypes.GuardianAngel);//守護天使にして
                                target.SetRoleRPC(RoleId.MadMate);//マッドにする
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);//名前も変える
                                RoleClass.FastMaker.IsCreatedMadMate = true;//作ったことにする
                                SuperNewRolesPlugin.Logger.LogInfo("[FastMakerSHR]マッドを作ったよ");
                            }
                            else
                            {
                                //作ってたら普通のキル(此処にMurderPlayerを使用すると2回キルされる為ログのみ表示)
                                SuperNewRolesPlugin.Logger.LogInfo("[FastMakerSHR]作ったので普通のキル");
                            }
                            break;
                        case RoleId.Jackal:
                            if (!RoleClass.Jackal.CreatePlayers.Contains(__instance.PlayerId) && RoleClass.Jackal.CanCreateFriend)//まだ作ってなくて、設定が有効の時
                            {
                                SuperNewRolesPlugin.Logger.LogInfo("まだ作ってなくて、設定が有効の時なんでフレンズ作成");
                                if (target == null || RoleClass.Jackal.CreatePlayers.Contains(__instance.PlayerId)) return false;
                                __instance.RpcShowGuardEffect(target);
                                RoleClass.Jackal.CreatePlayers.Add(__instance.PlayerId);
                                if (!target.IsImpostor())
                                {
                                    target.RpcSetRoleDesync(RoleTypes.GuardianAngel);//守護天使にして
                                    Jackal.CreateJackalFriends(target);//クルーにして フレンズにする
                                }
                                Mode.SuperHostRoles.FixedUpdate.SetRoleName(target);//名前も変える
                                SuperNewRolesPlugin.Logger.LogInfo("[JackalSHR]フレンズを作ったよ");
                                return false;
                            }
                            else
                            {
                                // キルができた理由のログを表示する(此処にMurderPlayerを使用すると2回キルされる為ログのみ表示)
                                if (!RoleClass.Jackal.CanCreateFriend) SuperNewRolesPlugin.Logger.LogInfo("[JackalSHR] フレンズを作る設定ではない為 普通のキル");
                                else if (RoleClass.Jackal.CanCreateFriend && RoleClass.Jackal.CreatePlayers.Contains(__instance.PlayerId)) SuperNewRolesPlugin.Logger.LogInfo("[JackalSHR] 作ったので 普通のキル");
                                else SuperNewRolesPlugin.Logger.LogInfo("[JackalSHR] 不正なキル");
                            }
                            break;
                        case RoleId.DarkKiller:
                            var ma = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                            if (ma != null && !ma.IsActive) return false;
                            break;
                    }
                    break;
                case ModeId.Detective:
                    if (target.PlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId) return false;
                    break;
            }
            Logger.Info("全モード通過", "CheckMurder");
            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                SyncSetting.CustomSyncSettings(__instance);
                SyncSetting.CustomSyncSettings(target);
                if (target.IsRole(RoleId.StuntMan))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard, __instance))
                    {
                        if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId))
                        {
                            RoleClass.StuntMan.GuardCount[target.PlayerId] = CustomOptions.StuntManMaxGuardCount.GetInt() - 1;
                            __instance.RpcShowGuardEffect(target);
                            return false;
                        }
                        else
                        {
                            if (!(RoleClass.StuntMan.GuardCount[target.PlayerId] <= 0))
                            {
                                RoleClass.StuntMan.GuardCount[target.PlayerId]--;
                                __instance.RpcShowGuardEffect(target);
                                return false;
                            }
                        }
                    }
                }
                else if (target.IsRole(RoleId.MadStuntMan))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.MadStuntmanGuard, __instance))
                    {
                        if (!RoleClass.MadStuntMan.GuardCount.ContainsKey(target.PlayerId))
                        {
                            __instance.RpcShowGuardEffect(target);
                            return false;
                        }
                        else
                        {
                            if (!(RoleClass.MadStuntMan.GuardCount[target.PlayerId] <= 0))
                            {
                                RoleClass.MadStuntMan.GuardCount[target.PlayerId]--;
                                __instance.RpcShowGuardEffect(target);
                                return false;
                            }
                        }
                    }
                }
                else if (target.IsRole(RoleId.Fox))
                {
                    if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, __instance))
                    {
                        if (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId))
                        {
                            __instance.RpcShowGuardEffect(target);
                            return false;
                        }
                        else
                        {
                            if (!(RoleClass.Fox.KillGuard[target.PlayerId] <= 0))
                            {
                                RoleClass.Fox.KillGuard[target.PlayerId]--;
                                __instance.RpcShowGuardEffect(target);
                                return false;
                            }
                        }
                    }
                }
            }
            Logger.Info("全スタントマン系通過", "CheckMurder");
            __instance.RpcMurderPlayerCheck(target);
            Camouflager.ResetCamouflageSHR(target);
            Logger.Info("RpcMurderPlayerCheck(一番下)を通過", "CheckMurder");
            return false;
        }
        public static void RpcCheckExile(this PlayerControl __instance)
        {
            if (__instance.IsRole(RoleId.Assassin) && __instance.IsAlive())
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
                    __instance.RpcSetName($"<size=200%>{CustomOptions.Cs(RoleClass.Marine.color, IntroDate.MarineIntro.NameKey + "Name")}は誰だ？</size>");
                }, 2f, "RpcCheckExile Who Marine Name");
                new LateTask(() =>
                {
                    __instance.RpcSendChat($"\n{ModTranslation.GetString("MarineWhois")}");
                }, 2.5f, "RpcCheckExile Who Marine Chat");
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
            if (ModeHandler.IsMode(ModeId.HideAndSeek) && target.IsImpostor() && !__instance.IsRole(RoleId.Jackal)) return;
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
                    target.RpcSetName($"<size=200%>{CustomOptions.Cs(RoleClass.Marine.color, IntroDate.MarineIntro.NameKey + "Name")}は誰だ？</size>");
                }, 2f, "RpcMurderPlayerCheck Who Marine Name");
                new LateTask(() =>
                {
                    target.RpcSendChat($"\n{ModTranslation.GetString("MarineWhois")}");
                }, 2.5f, "RpcMurderPlayerCheck Who Marine Chat");
                new LateTask(() =>
                {
                    target.RpcSetName(target.GetDefaultName());
                }, 2f, "RpcMurderPlayerCheck Default Name");
                return;
            }
            SuperNewRolesPlugin.Logger.LogInfo("i(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
            __instance.RpcMurderPlayer(target);
            SuperNewRolesPlugin.Logger.LogInfo("j(Murder)" + __instance.Data.PlayerName + " => " + target.Data.PlayerName);
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    static class PlayerControlSetCoolDownPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
        {
            if (PlayerControl.GameOptions.killCooldown == time && !RoleClass.IsCoolTimeSetted)
            {
                __instance.SetKillTimerUnchecked(RoleHelpers.GetEndMeetingKillCoolTime(__instance), RoleHelpers.GetEndMeetingKillCoolTime(__instance));
                RoleClass.IsCoolTimeSetted = true;
                return false;
            }
            if (__instance.Data.Role.CanUseKillButton && PlayerControl.GameOptions.KillCooldown > 0f)
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer = time, RoleHelpers.GetEndMeetingKillCoolTime(__instance));
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    public static class MurderPlayerPatch
    {
        public static bool resetToCrewmate = false;
        public static bool resetToDead = false;
        public static bool Prefix(PlayerControl __instance, PlayerControl target)
        {
            if (Roles.CrewMate.Knight.GuardedPlayers.Contains(target.PlayerId))
            {
                var Writer = RPCHelper.StartRPC(CustomRPC.KnightProtectClear);
                Writer.Write(target.PlayerId);
                Writer.EndRPC();
                RPCProcedure.KnightProtectClear(target.PlayerId);
                target.protectedByGuardian = true;
                return false;
            }
            EvilGambler.MurderPlayerPrefix(__instance, target);
            Doppelganger.KillCoolSetting.SHRMurderPlayer(__instance, target);
            if (ModeHandler.IsMode(ModeId.Default))
            {
                target.resetChange();
                if (RoleClass.Camouflager.IsCamouflage && target.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    __instance.resetChange();
                if (target.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (PlayerControl.LocalPlayer.IsRole(RoleId.SideKiller))
                    {
                        var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
                        if (sideplayer != null)
                        {
                            if (!RoleClass.SideKiller.IsUpMadKiller)
                            {
                                sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                RoleClass.SideKiller.IsUpMadKiller = true;
                            }
                        }
                    }
                }
                else if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    if (__instance.IsRole(RoleId.EvilGambler))
                    {
                        if (RoleClass.EvilGambler.GetSuc())
                        {
                            PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.SucCool);
                        }
                        else
                        {
                            PlayerControl.LocalPlayer.SetKillTimer(RoleClass.EvilGambler.NotSucCool);
                        }
                    }
                }

                var ma = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].CastFast<SwitchSystem>();
                if (__instance.IsRole(RoleId.DarkKiller))
                    if (ma != null && !ma.IsActive) return false;

                if (AmongUsClient.Instance.AmHost && __instance.PlayerId != target.PlayerId)
                {
                    switch (target.GetRole())
                    {
                        case RoleId.Fox:
                            Fox.FoxMurderPatch.Guard(__instance, target);
                            break;
                        case RoleId.NekoKabocha:
                            NekoKabocha.OnKill(__instance);
                            break;
                    }
                }
            }
            return true;
        }
        public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            // SuperNewRolesPlugin.Logger.LogInfo("MurderPlayer発生！元:" + __instance.GetDefaultName() + "、ターゲット:" + target.GetDefaultName());
            // Collect dead player info
            Logger.Info("追加");
            DeadPlayer deadPlayer = new(target, target.PlayerId, DateTime.UtcNow, DeathReason.Kill, __instance);
            DeadPlayer.deadPlayers.Add(deadPlayer);
            FinalStatusPatch.FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.Kill;

            if (CachedPlayer.LocalPlayer.PlayerId == __instance.PlayerId)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.WaveCannon))
                {
                    if (CustomOptions.WaveCannonIsSyncKillCoolTime.GetBool())
                        HudManagerStartPatch.WaveCannonButton.MaxTimer = CustomOptions.WaveCannonCoolTime.GetFloat();
                }
                else
                {
                    if (CustomOptions.WaveCannonJackalIsSyncKillCoolTime.GetBool())
                        HudManagerStartPatch.WaveCannonButton.MaxTimer = CustomOptions.WaveCannonJackalCoolTime.GetFloat();
                }
            }

            SerialKiller.MurderPlayer(__instance, target);
            Seer.ExileControllerWrapUpPatch.MurderPlayerPatch.Postfix(target);
            Roles.CrewMate.KnightProtected_Patch.MurderPlayerPatch.Postfix(target);

            if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    MurderPlayer.Postfix(__instance, target);
                }
            }
            else if (ModeHandler.IsMode(ModeId.Default))
            {
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
                        __instance.setOutfit(outfit, true);
                }
                if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId && PlayerControl.LocalPlayer.IsRole(RoleId.Finder))
                {
                    RoleClass.Finder.KillCount++;
                }
                if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId && PlayerControl.LocalPlayer.IsRole(RoleId.Slugger))
                {
                    if (CustomOptions.SluggerIsKillCoolSync.GetBool())
                    {
                        HudManagerStartPatch.SluggerButton.MaxTimer = CustomOptions.SluggerCoolTime.GetFloat();
                        HudManagerStartPatch.SluggerButton.Timer = HudManagerStartPatch.SluggerButton.MaxTimer;
                    }
                }
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == target.PlayerId) Roles.CrewMate.Painter.Handle(Roles.CrewMate.Painter.ActionType.Death);
                if (target.IsRole(RoleId.Assassin))
                {
                    target.Revive();
                    target.Data.IsDead = false;
                    RPCProcedure.CleanBody(target.PlayerId);
                    new LateTask(() =>
                    {
                        if (AmongUsClient.Instance.AmHost)
                        {
                            MeetingRoomManager.Instance.AssignSelf(target, null);
                            FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(target);
                            target.RpcStartMeeting(null);
                        }
                    }, 0.5f, "MurderPlayer Assassin Meeting");
                    RoleClass.Assassin.TriggerPlayer = target;
                    return;
                }
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Psychometrist))
                {
                    Roles.CrewMate.Psychometrist.MurderPlayer(__instance, target);
                }
                if (target.IsDead())
                {
                    if (target.IsRole(RoleId.Hitman))
                    {
                        Roles.Neutral.Hitman.Death();
                    }
                }
                Levelinger.MurderPlayer(__instance, target);
                if (RoleClass.Lovers.SameDie && target.IsLovers())
                {
                    if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
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
                        }
                    }
                }
                if (target.IsQuarreled())
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        var Side = RoleHelpers.GetOneSideQuarreled(target);
                        if (Side.IsDead())
                        {
                            RPCProcedure.ShareWinner(target.PlayerId);
                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                            Writer.Write(target.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            RoleClass.Quarreled.IsQuarreledWin = true;
                            ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.QuarreledWin, false);
                        }
                    }
                }
                Minimalist.MurderPatch.Postfix(__instance);
            }
            if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId && ModeHandler.IsMode(ModeId.Default))
            {
                EvilGambler.MurderPlayerPostfix(__instance);
                Doppelganger.KillCoolSetting.MurderPlayer(__instance, target);
                if (__instance.IsImpostor())
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleHelpers.GetCoolTime(__instance), RoleHelpers.GetCoolTime(__instance));
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
    class CompleteTask
    {
        public static void Postfix(PlayerControl __instance, uint idx)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Painter) && RoleClass.Painter.CurrentTarget != null && RoleClass.Painter.CurrentTarget.PlayerId == __instance.PlayerId) Roles.CrewMate.Painter.Handle(Roles.CrewMate.Painter.ActionType.TaskComplete);
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
            FinalStatusPatch.FinalStatusData.FinalStatuses[__instance.PlayerId] = FinalStatus.Exiled;
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (__instance.IsRole(RoleId.Assassin) && !RoleClass.Assassin.MeetingEndPlayers.Contains(__instance.PlayerId))
                {
                    __instance.Revive();
                    __instance.Data.IsDead = false;
                    RPCProcedure.CleanBody(__instance.PlayerId);
                    new LateTask(() =>
                    {
                        if (AmongUsClient.Instance.AmHost)
                        {
                            MeetingRoomManager.Instance.AssignSelf(__instance, null);
                            FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                            __instance.RpcStartMeeting(null);
                        }
                    }, 0.5f, "Exiled Assassin Meeting");
                    RoleClass.Assassin.TriggerPlayer = __instance;
                    return;
                }
                if (__instance.IsRole(RoleId.Hitman))
                {
                    Roles.Neutral.Hitman.Death();
                }
                if (RoleClass.Lovers.SameDie && __instance.IsLovers())
                {
                    if (__instance.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                    {
                        PlayerControl SideLoverPlayer = __instance.GetOneSideLovers();
                        if (SideLoverPlayer.IsAlive())
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExiledRPC, SendOption.Reliable, -1);
                            writer.Write(SideLoverPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.ExiledRPC(SideLoverPlayer.PlayerId);
                        }
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
    class ReportDeadBodyPatch
    {
        public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
        {
            if (__instance.IsRole(RoleId.GM))
            {
                MeetingRoomManager.Instance.AssignSelf(__instance, target);
                if (AmongUsClient.Instance.AmHost)
                {
                    DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(__instance);
                    __instance.RpcStartMeeting(target);
                }
                return false;
            }
            if (!AmongUsClient.Instance.AmHost) return true;
            if (target != null && RoleClass.BlockPlayers.Contains(target.PlayerId)) return false;
            if (ModeHandler.IsMode(ModeId.HideAndSeek)) return false;
            if (RoleClass.Camouflager.IsCamouflage)
            {
                Roles.Impostor.Camouflager.ResetCamouflage();
            }
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (__instance.IsRole(RoleId.EvilButtoner, RoleId.NiceButtoner) && target != null && target.PlayerId == __instance.PlayerId)
                {
                    return true;
                }
                if (__instance.IsRole(RoleId.Amnesiac))
                {
                    if (!target.Disconnected)
                    {
                        __instance.RPCSetRoleUnchecked(target.Role.Role);
                        if (target.Role.IsSimpleRole)
                        {
                            __instance.SetRoleRPC(target.Object.GetRole());
                        }
                    }
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
                            player.RpcRevertShapeshift(false);
                        }, 0.5f);
                        SyncSetting.CustomSyncSettings(player);
                    }
                }
            }
            return (RoleClass.Assassin.TriggerPlayer != null)
            || (!MapOptions.MapOption.UseDeadBodyReport && target != null)
            || (!MapOptions.MapOption.UseMeetingButton && target == null)
            || ModeHandler.IsMode(ModeId.BattleRoyal)
            || ModeHandler.IsMode(ModeId.CopsRobbers)
                ? false
                : ModeHandler.IsMode(ModeId.SuperHostRoles)
                ? Mode.SuperHostRoles.ReportDeadBody.ReportDeadBodyPatch(__instance, target)
                : !ModeHandler.IsMode(ModeId.Zombie)
                && (!ModeHandler.IsMode(ModeId.Detective) || target != null || !Mode.Detective.Main.IsNotDetectiveMeetingButton || __instance.PlayerId == Mode.Detective.Main.DetectivePlayer.PlayerId);
        }
    }
    public static class PlayerControlFixedUpdatePatch
    {
        public static PlayerControl SetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

            if (untargetablePlayers == null)
            {
                untargetablePlayers = new();
            }

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.IsDead && (!onlyCrewmates || !playerInfo.Role.IsImpostor))
                {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers.Any(x => x == @object))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }
                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result.IsDead() ? null : result;
        }
        public static PlayerControl JackalSetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            if (!MapUtilities.CachedShipStatus) return result;
            if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
            if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

            if (untargetablePlayers == null)
            {
                untargetablePlayers = new();
            }

            Vector2 truePosition = targetingPlayer.GetTruePosition();
            Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                //下記Jackalがbuttonのターゲットにできない役職の設定
                if (playerInfo.Object.IsAlive() && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.Object.IsJackalTeamJackal() && !playerInfo.Object.IsJackalTeamSidekick())
                {
                    PlayerControl @object = playerInfo.Object;
                    if (untargetablePlayers.Any(x => x == @object))
                    {
                        // if that player is not targetable: skip check
                        continue;
                    }

                    if (@object && (!@object.inVent || targetPlayersInVents))
                    {
                        Vector2 vector = @object.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = @object;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }
        public static void SetPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
    }
}