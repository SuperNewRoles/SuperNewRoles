using System;
using System.IO;
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
                logicComponent.TryCast<LogicOptions>() == null)
            {
                flag = true;
                writer.StartMessage((byte)index);
                var hasBody = logicComponent.Serialize(writer, initialState);
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
    public static void Postfix()
    {
        if (source != null)
        {
            Logger.Info(source.time.ToString(), "a");
            Logger.Info(source.timeSamples.ToString(), "b");
        }
        //解像度変更
        if (Input.GetKeyDown(KeyCode.F9))
        {
            resolutionIndex++;
            if (resolutionIndex >= resolutions.Length) resolutionIndex = 0;
            ResolutionManager.SetResolution(resolutions[resolutionIndex].Item1, resolutions[resolutionIndex].Item2, false);
        }

        // その時点までのlogを切り出す
        if (ModHelpers.GetManyKeyDown(new[] { KeyCode.S, KeyCode.LeftShift, KeyCode.RightShift }))
        {
            string via = "KeyCommandVia";
            Logger.SaveLog(via, via);
        }


        //　ゲーム中
        if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started && AmongUsClient.Instance.AmHost)
        {
            // 廃村
            if (ModHelpers.GetManyKeyDown(new[] { KeyCode.H, KeyCode.LeftShift, KeyCode.RightShift }))
            {
                RPCHelper.StartRPC(CustomRPC.SetHaison).EndRPC();
                RPCProcedure.SetHaison();
                Logger.Info("===================== 廃村 ======================", "End Game");
                if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                {
                    EndGameCheck.CustomEndGame(ShipStatus.Instance, GameOverReason.ImpostorDisconnect, false);
                }
                else
                {
                    GameManager.Instance.RpcEndGame(GameOverReason.ImpostorDisconnect, false);
                    MapUtilities.CachedShipStatus.enabled = false;
                }
            }
            // 会議を強制終了
            if (ModHelpers.GetManyKeyDown(new[] { KeyCode.M, KeyCode.LeftShift, KeyCode.RightShift }) && RoleClass.IsMeeting)
            {
                if (MeetingHud.Instance != null)
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
        }

        // デバッグモード　かつ　左コントロール
        if (ConfigRoles.DebugMode.Value && Input.GetKey(KeyCode.LeftControl))
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
                CustomSpores.AddMushroom(PlayerControl.LocalPlayer.transform.position);
                return;
                source = SoundManager.Instance.PlaySound(ContentManager.GetContent<AudioClip>("Sauner_SaunaBGM.wav"), true);
                return;
                HudManager.Instance.ShowPopUp("スマソ。無理やわ。");
                return;
                string filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\Replay\";
                DirectoryInfo d = new(filePath);
                Logger.Info("FileName:" + d.GetFiles()[0].Name);
                (ReplayData replay, bool IsSuc) = ReplayReader.ReadReplayDataFirst(d.GetFiles()[0].Name);
                ReplayManager.IsReplayMode = true;
                Logger.Info($"IsSuc:{IsSuc}");
                if (IsSuc)
                {
                    Logger.Info($"PlayerCount:{replay.AllPlayersCount}");
                    Logger.Info($"Mode:{replay.CustomMode}");
                    Logger.Info($"Time:{replay.RecordTime.ToString()}");
                }
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