using System;
using System.Collections.Generic;
using System.Reflection;
using Agartha;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
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
    private static bool notcreateroom;
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public class AmongUsClientOnPlayerJoinedPatch
    {
        public static void Postfix()
        {
            if (PlayerControl.LocalPlayer != null)
            {
                SuperNewRolesPlugin.Logger.LogInfo("[VersionShare]Version Shared!");
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareSNRVersion, SendOption.Reliable, -1);
                writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Major);
                writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Minor);
                writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Build);
                writer.WritePacked(AmongUsClient.Instance.ClientId);
                writer.Write((byte)(SuperNewRolesPlugin.ThisVersion.Revision < 0 ? 0xFF : SuperNewRolesPlugin.ThisVersion.Revision));
                writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.ShareSNRversion(SuperNewRolesPlugin.ThisVersion.Major, SuperNewRolesPlugin.ThisVersion.Minor, SuperNewRolesPlugin.ThisVersion.Build, SuperNewRolesPlugin.ThisVersion.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
            }
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public class GameStartManagerStartPatch
    {
        public static void Postfix()
        {
            timer = 600f;
            RPCTimer = 1f;
            notcreateroom = false;
            kickingTimer = 0f;
            RoleClass.ClearAndReloadRoles();
            GameStartManagerUpdatePatch.Proce = 0;
            GameStartManagerUpdatePatch.LastBlockStart = false;
            GameStartManagerUpdatePatch.VersionPlayers = new Dictionary<int, PlayerVersion>();
            GameStartManagerUpdatePatch.Alllady613ErrorMessage = false;
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
        internal static bool Alllady613ErrorMessage = false;

        public static void Prefix(GameStartManager __instance)
        {
            if (!GameData.Instance) return; // Not host or no instance
            update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }
        public static void Postfix(GameStartManager __instance)
        {
            Proce++;
            if (Proce >= 10)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.ShareSNRVersion, SendOption.Reliable, -1);
                writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Major);
                writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Minor);
                writer.WritePacked(SuperNewRolesPlugin.ThisVersion.Build);
                writer.WritePacked(AmongUsClient.Instance.ClientId);
                writer.Write((byte)(SuperNewRolesPlugin.ThisVersion.Revision < 0 ? 0xFF : SuperNewRolesPlugin.ThisVersion.Revision));
                writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.ShareSNRversion(SuperNewRolesPlugin.ThisVersion.Major, SuperNewRolesPlugin.ThisVersion.Minor, SuperNewRolesPlugin.ThisVersion.Build, SuperNewRolesPlugin.ThisVersion.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
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
            if (AmongUsClient.Instance.AmHost)
            {
                if (!(ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.Werewolf, false) || ModHelpers.IsDebugMode()))
                {
                    message += $"\n{ModTranslation.GetString("Ver20613CanNotPlayHostMode")}\n";
                    if (!Alllady613ErrorMessage)
                    {
                        Alllady613ErrorMessage = true;
                        new LateTask(() =>
                            {
                                FastDestroyableSingleton<HudManager>.Instance?.Chat?.AddChat(PlayerControl.LocalPlayer, $"{ModTranslation.GetString("DowngradeDescription")}");
                                GUIUtility.systemCopyBuffer = "https://github.com/ykundesu/SuperNewRoles/wiki/Home/1be6c082ddb8887c04143d23484459c2aa8b66b2#shr%E3%83%A2%E3%83%BC%E3%83%89%E3%82%92%E4%BD%BF%E7%94%A8%E3%81%99%E3%82%8B%E6%99%82%E3%81%AF-snr-v1800-%E4%BB%A5%E9%99%8D%E3%82%92%E4%BD%BF%E7%94%A8%E3%81%97%E3%81%AA%E3%81%84%E3%81%A7%E4%B8%8B%E3%81%95%E3%81%84";
                            }, 3f, "Ver20613CanNotPlayHostMode");
                    }
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
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                }
            }
            if (ConfigRoles.IsVersionErrorView.Value || AmongUsClient.Instance.AmHost)
            {
                foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
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
                    if (__instance.StartButton.enabled != true) __instance.StartButton.enabled = __instance.startLabelText.enabled = true;
                }
                else
                {
                    // message = $"{ModTranslation.GetString("ErrorClientCanNotPley")} \n" + message; (2023.6.27ではHostModeが利用できない為封印)
                    //開始ボタンを押せないようにする。
                    __instance.ResetStartState();

                    // 参加者の導入状況に問題がある時、開始ボタンと開始のテキストを非表示にする。(アップデート処理の負荷を下げる為、ifを使用)
                    if (__instance.StartButton.enabled != false) __instance.StartButton.enabled = __instance.startLabelText.enabled = false;
                }
            }
            if (blockStart || hostModeInVanilla)
            {
                __instance.GameStartText.text = message;
                __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
            }
            else
            {
                if (LastBlockStart)
                {
                    __instance.GameStartText.text = "";
                }
                __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
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

            __instance.PlayerCounter.text = currentText.Replace("\n", "") + suffix.Replace("\n", "")
            ;
            __instance.PlayerCounter.autoSizeTextContainer = true;
            if (minutes == 0 && seconds < 5 && !notcreateroom && ConfigRoles.IsAutoRoomCreate.Value)
            {
                notcreateroom = true;
            }
        }
    }
}
public struct PlayerVersion
{
    public readonly Version version;
    public readonly Guid guid;

    public PlayerVersion(Version version, Guid guid)
    {
        this.version = version;
        this.guid = guid;
    }

    public bool GuidMatches()
    {
        return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(guid);
    }
}