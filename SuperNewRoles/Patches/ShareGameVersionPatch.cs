using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Agartha;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Mode;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
public static class PlayerCountChange
{
    public static void Prefix(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;
    }
}
class ShareGameVersion
{
    public static bool IsVersionOK = false;
    public static bool IsChangeVersion = false;
    public static bool IsRPCSend = false;
    public static float timer = 600;
    public static float RPCTimer = 1f;
    private static float kickingTimer = 0f;
    /// <summary> 導入状態のエラーを表示する場所 </summary>
    public static TextMeshPro VersionErrorInfo;
    private static bool notcreateroom;
    private static byte[] ModuleVersion = Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray();
    private static byte ModuleRevision = (byte)(SuperNewRolesPlugin.ThisVersion.Revision < 0 ? 0xFF : SuperNewRolesPlugin.ThisVersion.Revision);

    public static void SendVersionRPC()
    {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareSNRVersion, SendOption.Reliable, -1);
        writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Major);
        writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Minor);
        writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Build);
        writer.WritePacked(AmongUsClient.Instance.ClientId);
        writer.Write(ModuleRevision);
        writer.Write(ModuleVersion);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.ShareSNRversion(SuperNewRolesPlugin.ThisVersion.Major, SuperNewRolesPlugin.ThisVersion.Minor, SuperNewRolesPlugin.ThisVersion.Build, SuperNewRolesPlugin.ThisVersion.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix()
        {
            if (PlayerControl.LocalPlayer != null) SendVersionRPC();
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public class GameStartManagerStartPatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            timer = 600f;
            RPCTimer = 1f;
            notcreateroom = false;
            kickingTimer = 0f;
            ClearVersionErrorInfo();

            RoleClass.ClearAndReloadRoles();
            GameStartManagerUpdatePatch.Proce = 0;
            GameStartManagerUpdatePatch.LastBlockStart = false;
            GameStartManagerUpdatePatch.VersionPlayers = new Dictionary<int, PlayerVersion>();
        }

        static void ClearVersionErrorInfo()
        {
            VersionErrorInfo = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.TaskPanel.taskText, FastDestroyableSingleton<HudManager>.Instance.transform);
            VersionErrorInfo.fontSize = VersionErrorInfo.fontSizeMin = VersionErrorInfo.fontSizeMax = 3f;
            VersionErrorInfo.autoSizeTextContainer = false;
            VersionErrorInfo.enableWordWrapping = false;
            VersionErrorInfo.alignment = TMPro.TextAlignmentOptions.Center;
            VersionErrorInfo.transform.position = Vector3.zero;
            VersionErrorInfo.transform.localPosition = new Vector3(0f, 0f, -40f);
            VersionErrorInfo.transform.localScale = Vector3.one;
            VersionErrorInfo.color = Palette.White;
            VersionErrorInfo.enabled = false;
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class GameStartManagerUpdatePatch
    {
        private static bool update = false;
        public static Dictionary<int, PlayerVersion> VersionPlayers = new();
        public static int Proce;
        private static string currentText = "";
        public static bool LastBlockStart;

        public static void Prefix(GameStartManager __instance)
        {
            if (!GameData.Instance) return; // Not host or no instance
            update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }
        public static void Postfix(GameStartManager __instance)
        {
            if (ReplayManager.IsReplayMode)
            {
                if (FastDestroyableSingleton<GameStartManager>.Instance.startState == GameStartManager.StartingStates.Countdown)
                {
                    FastDestroyableSingleton<GameStartManager>.Instance.countDownTimer = 0;
                }
                else
                {
                    Logger.Info("COMMeDDDDDDDDDDDD!!!!!!!!!");
                    __instance.StartButton.GetComponent<PassiveButton>().OnClick.Invoke();
                    FastDestroyableSingleton<GameStartManager>.Instance.startState = GameStartManager.StartingStates.Countdown;
                    FastDestroyableSingleton<GameStartManager>.Instance.countDownTimer = 0;
                }
            }
            Proce++;
            if (Proce >= 20)
            {
                SendVersionRPC();
                Proce = 0;
            }
            string message = "";
            bool blockStart = false;
            bool hostModeInVanilla = false;
            if (AmongUsClient.Instance.AmHost)
            {
                if (CustomOptionHolder.DisconnectNotPCOption.GetBool())
                {
                    foreach (InnerNet.ClientData p in AmongUsClient.Instance.allClients)
                    {
                        if (p.PlatformData.Platform is not Platforms.StandaloneEpicPC and not Platforms.StandaloneSteamPC)
                        {
                            AmongUsClient.Instance.KickPlayer(p.Id, false);
                        }
                    }
                }
                // アガルタ反映関係の警告文制御
                if ((CustomMapNames)GameManager.Instance.LogicOptions.currentGameOptions.MapId == CustomMapNames.Mira && //マップ設定がMiraである かつ
                    CustomOptionHolder.enableAgartha.GetBool() && //「アガルタ」が有効である かつ
                    !ModeHandler.IsMode(ModeId.Default, false) && //モードがデフォルトでない(特殊モードである) かつ
                    !CustomOptionHolder.DisconnectNotPCOption.GetBool() && //「PC以外キック」が無効(バニラをキックする状態)である かつ
                    !ConfigRoles.DebugMode.Value) //Debugモードでない時
                {
                    // 警告を表示する
                    message += $"\n{ModTranslation.GetString("IsSpecialModeOnAndVanillaKickOff")}\n";
                    blockStart = true;
                }
            }
            if (!AmongUsClient.Instance.AmHost)
            {
                if (!VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId))
                {
                    message += $"\n{ModTranslation.GetString("ErrorHostNoVersion")}\n";
                    blockStart = true;
                }
                else
                {
                    var client = AmongUsClient.Instance.GetHost();
                    PlayerVersion PV = VersionPlayers[client.Id];
                    int diff = SuperNewRolesPlugin.ThisVersion.CompareTo(PV.version);
                    if (diff > 0)
                    {
                        message += $"\n{ModTranslation.GetString("ErrorHostChangeVersion")} (v{VersionPlayers[client.Id].version})\n";
                        blockStart = true;
                    }
                    else if (diff < 0)
                    {
                        message += $"\n{ModTranslation.GetString("ErrorHostChangeVersion")} (v{VersionPlayers[client.Id].version})\n";
                        blockStart = true;
                    }
                    else if (!PV.GuidMatches())
                    { // version presumably matches, check if Guid matches
                        message += $"\n{ModTranslation.GetString("ErrorHostGuidMatches")} (v{VersionPlayers[client.Id].version})\n";
                        blockStart = true;
                    }
                }
                //TheOtherRoles\Patches\GameStartManagerPatch.cs より
                if (!VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId) || SuperNewRolesPlugin.ThisVersion.CompareTo(VersionPlayers[AmongUsClient.Instance.HostId].version) != 0 || !VersionPlayers[AmongUsClient.Instance.HostId].GuidMatches())
                {
                    kickingTimer += Time.deltaTime;
                    if (kickingTimer > 10)
                    {
                        kickingTimer = 0;
                        AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                        SceneChanger.ChangeScene("MainMenu");
                    }

                    message += $"\n{String.Format(ModTranslation.GetString("KickReasonHostNoVersion"), Math.Round(10 - kickingTimer))}\n";
                }
            }
            if (ConfigRoles.IsVersionErrorView.Value || AmongUsClient.Instance.AmHost)
            {
                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients)
                {
                    if (client.Id != AmongUsClient.Instance.HostId)
                    {
                        if (!VersionPlayers.ContainsKey(client.Id))
                        {
                            if (ConfigRoles.IsVersionErrorView.Value || ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.Werewolf, false))
                                message += $"{string.Format(ModTranslation.GetString("ErrorClientNoVersion"), client.PlayerName)} \n";

                            // HostModeでないなら、バニラ参加者がいる場合開始不可能にする。
                            if (ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.Werewolf, false)) blockStart = true;
                            /*  そうではない(HostMode)ならば、
                                vanilla参加者がいて且つエラーを表示する設定が有効である場合、Messageだけを表示できるようにする。*/
                            else hostModeInVanilla = true;
                        }
                        else
                        {
                            PlayerVersion PV = VersionPlayers[client.Id];
                            int diff = SuperNewRolesPlugin.ThisVersion.CompareTo(PV.version);
                            if (diff > 0)
                            {
                                message += $"{string.Format(ModTranslation.GetString("ErrorClientChangeVersion"), client.Character.Data.PlayerName)} (v{VersionPlayers[client.Id].version})\n";
                                blockStart = true;
                            }
                            else if (diff < 0)
                            {
                                message += $"{string.Format(ModTranslation.GetString("ErrorClientChangeVersion"), client.Character.Data.PlayerName)} (v{VersionPlayers[client.Id].version})\n";
                                blockStart = true;
                            }
                            else if (!PV.GuidMatches())
                            { // version presumably matches, check if Guid matches
                                message += $"{string.Format(ModTranslation.GetString("ErrorClientGuidMatches"), client.Character.Data.PlayerName)} \n";
                                blockStart = true;
                            }
                        }
                    }
                }
            }
            if (AmongUsClient.Instance.AmHost)
            {
                if (!blockStart)
                {
                    // 参加者の導入状況に問題が無い時、開始ボタンと開始のテキストを表示する。(アップデート処理の負荷を下げる為、ifを使用)
                    if (__instance.StartButton.enabled != true) __instance.StartButton.SetButtonEnableState(true);
                }
                else
                {
                    message = $"{ModTranslation.GetString("ErrorClientCanNotPley")} \n" + message;
                    //開始ボタンを押せないようにする。
                    __instance.ResetStartState();

                    // 参加者の導入状況に問題がある時、開始ボタンと開始のテキストを非表示にする。(アップデート処理の負荷を下げる為、ifを使用)
                    if (__instance.StartButton.enabled != false) __instance.StartButton.SetButtonEnableState(false);
                }
            }
            if (blockStart || hostModeInVanilla)
            {
                VersionErrorInfo.text = message;
                VersionErrorInfo.enabled = true;

                // ゲーム開始後はエラー表記を非表示にする
                if ((__instance.startState == GameStartManager.StartingStates.Countdown && Mathf.CeilToInt(__instance.countDownTimer) <= 0) || __instance.startState == GameStartManager.StartingStates.Starting)
                    VersionErrorInfo.enabled = false;
            }
            else
            {
                if (LastBlockStart)
                {
                    VersionErrorInfo.text = "";
                    VersionErrorInfo.enabled = false;
                }
            }
            LastBlockStart = blockStart;
            if (update) currentText = __instance.PlayerCounter.text;
            if (AmongUsClient.Instance.AmHost)
            {
                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                RPCTimer -= Time.deltaTime;
                if (RPCTimer <= 0)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetRoomTimerRPC, SendOption.Reliable, -1);
                    int minutes2 = (int)timer / 60;
                    int seconds2 = (int)timer % 60;
                    writer.Write((byte)minutes2);
                    writer.Write((byte)seconds2);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCTimer = 1f;
                }
            }
            else
            {
                timer = Mathf.Max(0f, timer);
            }
            int minutes = (int)timer / 60;
            int seconds = (int)timer % 60;
            string suffix = $" ({minutes:00}:{seconds:00})";
            __instance.StartButton.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>().text = $"{FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.StartLabel)} {suffix}";
            __instance.StartButtonClient.transform.Find("Text_TMP").GetComponent<TextMeshPro>().text = $"{FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.WaitingForHost)} {suffix}";
            if (minutes == 0 && seconds < 5 && !notcreateroom && ConfigRoles.IsAutoRoomCreate.Value) notcreateroom = true;
        }
    }
}
public struct PlayerVersion
{
    public readonly Version version;
    public readonly Guid guid;

    public PlayerVersion(in Version version, in Guid guid)
    {
        this.version = version;
        this.guid = guid;
    }

    public bool GuidMatches()
    {
        return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(guid);
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj == null) return false;
        PlayerVersion other = (PlayerVersion)obj;
        return Equals(other.version, other.guid);
    }

    public bool Equals(Version version, Guid guid)
    {
        if (!this.version.Equals(version)) return false;
        if (!this.guid.Equals(guid)) return false;
        return true;
    }
    public static bool operator ==(PlayerVersion left, PlayerVersion right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PlayerVersion left, PlayerVersion right)
    {
        return !(left == right);
    }
}