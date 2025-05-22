using System;
using System.IO;
using System.Linq;
using Agartha;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.BattleRoyal;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(GameManager), nameof(GameManager.Serialize))]
class GameManagerSerializeFix
{
    public static bool Prefix(GameManager __instance, [HarmonyArgument(0)] MessageWriter writer, [HarmonyArgument(1)] bool initialState, ref bool __result)
    {
        bool flag = false;
        for (int index = 0; index < __instance.LogicComponents.Count; ++index)
        {
            GameLogicComponent logicComponent = __instance.LogicComponents[index];
            if (initialState || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
                logicComponent.Pointer != __instance.LogicOptions.Pointer)
            {
                flag = true;
                writer.StartMessage((byte)index);
                var hasBody = logicComponent.Serialize(writer);
                if (hasBody) writer.EndMessage();
                else writer.CancelMessage();
                logicComponent.ClearDirtyFlag();
            }
        }
        __instance.ClearDirtyBits();
        __result = flag;
        return false;
    }
}

[HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
class ControllerManagerUpdatePatch
{
    static readonly (int, int)[] resolutions = { (480, 270), (640, 360), (800, 450), (1280, 720), (1600, 900), (1920, 1080) };
    static int resolutionIndex = 0;
    static AudioSource source;

    static KeyCode[] HaisonKeyCodes = [KeyCode.H, KeyCode.LeftShift, KeyCode.RightShift];
    static KeyCode[] ForceEndMeetingKeyCodes = [KeyCode.M, KeyCode.LeftShift, KeyCode.RightShift];
    static KeyCode[] LogKeyCodes = [KeyCode.S, KeyCode.LeftShift, KeyCode.RightShift];


    public static void Postfix()
    {
        //解像度変更
        if (Input.GetKeyDown(KeyCode.F9))
        {
            resolutionIndex++;
            if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
            ResolutionManager.SetResolution(resolutions[resolutionIndex].Item1, resolutions[resolutionIndex].Item2, false);
        }

        // その時点までのlogを切り出す
        if (ModHelpers.GetManyKeyDown(LogKeyCodes))
        {
            string via = "KeyCmdVia";
            LoggerPlus.SaveLog(via, via);
        }


        //　ゲーム中
        if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && AmongUsClient.Instance.AmHost)
        {
            // 廃村
            if (ModHelpers.GetManyKeyDown(HaisonKeyCodes))
            {
                RPCHelper.StartRPC(CustomRPC.SetHaison).EndRPC();
                RPCProcedure.SetHaison();
                Logger.Info("===================== 廃村 ======================", "End Game");
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    EndGameCheck.CustomEndGame(ShipStatus.Instance, CustomGameOverReason.HAISON, false);
                }
                else
                {
                    GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                    MapUtilities.CachedShipStatus.enabled = false;
                }
            }
            // 会議を強制終了
            if (RoleClass.IsMeeting && MeetingHud.Instance != null && ModHelpers.GetManyKeyDown(ForceEndMeetingKeyCodes))
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    // 会議強制スキップを行うと, CheckForEndVotingを通過しない為, 此処で呼び出し
                    if (Mode.PlusMode.PlusGameOptions.EnableFirstEmergencyCooldown)
                        EmergencyMinigamePatch.FirstEmergencyCooldown.OnCheckForEndVotingNotMod(false);
                }

                if (ModeHandler.IsMode(ModeId.BattleRoyal))
                    SelectRoleSystem.OnEndSetRole();
                else
                    MeetingHud.Instance.RpcClose();
            }
        }

        // デバッグモード　かつ　左コントロール
        if (DebugModeManager.IsDebugMode && Input.GetKey(KeyCode.LeftControl))
        {
            // Spawn dummys
            if (Input.GetKeyDown(KeyCode.G))
            {
                PlayerControl bot = BotManager.Spawn(PlayerControl.LocalPlayer.NameText().text, false);
                Logger.Info(EOSManager.Instance.UserIDToken);

                bot.NetTransform.SnapTo(PlayerControl.LocalPlayer.transform.position);
                //new LateTask(() => bot.NetTransform.RpcSnapTo(new Vector2(0, 15)), 0.2f, "Bot TP Task");
                //new LateTask(() => { foreach (var pc in CachedPlayer.AllPlayers) pc.PlayerControl.RpcMurderPlayer(bot); }, 0.4f, "Bot Kill Task");
                //new LateTask(() => bot.Despawn(), 0.6f, "Bot Despawn Task");
            }

            //ここにデバッグ用のものを書いてね
            if (Input.GetKeyDown(KeyCode.I))
            {
                Vector2 center = ShipStatus.Instance.MapPrefab.HerePoint.transform.parent.localPosition * -1f * ShipStatus.Instance.MapScale;
                File.WriteAllBytes("SpawnableMap.png", MapDatabase.MapDatabase.GetCurrentMapData().OutputMap(center, new Vector2(10f, 7f) * ShipStatus.Instance.MapScale, 40f).EncodeToPNG());
                return;
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                UnityEngine.Object.Instantiate(DestroyableSingleton<RoleManager>.Instance.protectAnim, PlayerControl.LocalPlayer.gameObject.transform).Play(PlayerControl.LocalPlayer, null, PlayerControl.LocalPlayer.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.ImpostorGhost);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                PVCreator.Start();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                PVCreator.End();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                PVCreator.Start2();
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                ModHelpers.PlayerById(1).RpcMurderPlayer(PlayerControl.LocalPlayer, true);//ModHelpers.PlayerById(2));
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Logger.Info("Test Option Set", "Test");
                IGameOptions options = GameOptionsManager.Instance.CurrentGameOptions.DeepCopy();
                options.SetFloat(FloatOptionNames.KillCooldown, 10f);
                options.SetFloat(FloatOptionNames.CrewLightMod, 10f);
                GameManager.Instance.LogicOptions.SetGameOptions(options);
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
                BotManager.Spawn($"bot{(byte)GameData.Instance.GetAvailableId()}");
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                BotManager.AllBotDespawn();
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {
                SuperNewRolesPlugin.Logger.LogInfo("new(" + (PlayerControl.LocalPlayer.transform.position.x - 13.2f) + "f, " + (PlayerControl.LocalPlayer.transform.position.y - 16f) + "f), ");
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                foreach (RoleId role in Enum.GetValues(typeof(RoleId)))
                {
                    Roles.Role.QuoteMod quoteMod = Roles.RoleBases.CustomRoles.GetQuoteMod(role);
                    if (quoteMod != Roles.Role.QuoteMod.SuperNewRoles) Logger.Info($"{role}, 参考元 : {quoteMod}", "QuoteMod Log");
                }
            }
        }
        // 以下フリープレイのみ
        if (AmongUsClient.Instance.NetworkMode != NetworkModes.FreePlay) return;
        // エアーシップのトイレのドアを開ける
        if (Input.GetKeyDown(KeyCode.T))
        {
            RPCHelper.RpcOpenToilet();
        }
    }
}