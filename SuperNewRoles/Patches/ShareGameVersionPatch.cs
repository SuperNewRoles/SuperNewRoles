using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agartha;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patches
{
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
                RoleClass.ClearAndReloadRoles();
                GameStartManagerUpdatePatch.Proce = 0;
                GameStartManagerUpdatePatch.LastBlockStart = false;
                GameStartManagerUpdatePatch.VersionPlayers = new Dictionary<int, PlayerVersion>();
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
                if (AmongUsClient.Instance.AmHost)
                {
                    if (CustomOptions.DisconnectNotPCOption.GetBool())
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
                    if ((CustomMapNames)PlayerControl.GameOptions.MapId == CustomMapNames.Mira && //マップ設定がMiraである かつ
                        CustomOptions.enableAgartha.GetBool() && //「アガルタ」が有効である かつ
                        !ModeHandler.IsMode(ModeId.Default, false) && //モードがデフォルトでない(特殊モードである) かつ
                        !CustomOptions.DisconnectNotPCOption.GetBool() && //「PC以外キック」が無効(バニラをキックする状態)である かつ
                        !CustomOptions.IsDebugMode.GetBool()) //Debugモードでない時
                    {
                        // 警告を表示する
                        message += $"\n{ModTranslation.GetString("IsSpecialModeOnAndVanillaKickOff")}\n";
                        blockStart = true;
                    }
                }
                if (ConfigRoles.IsVersionErrorView.Value)
                {
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
                                message += $"{ModTranslation.GetString("ErrorHostChangeVersion")} (v{VersionPlayers[client.Id].version})\n";
                                blockStart = true;
                            }
                            else if (diff < 0)
                            {
                                message += $"{ModTranslation.GetString("ErrorHostChangeVersion")} (v{VersionPlayers[client.Id].version})\n";
                                blockStart = true;
                            }
                            else if (!PV.GuidMatches())
                            { // version presumably matches, check if Guid matches
                                message += $"{ModTranslation.GetString("ErrorHostChangeVersion")} (v{VersionPlayers[client.Id].version})\n";
                                blockStart = true;
                            }
                        }
                    }
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                    {
                        if (client.Id != AmongUsClient.Instance.HostId)
                        {
                            if (!VersionPlayers.ContainsKey(client.Id))
                            {
                                message += string.Format(ModTranslation.GetString("ErrorClientNoVersion"), client.PlayerName) + "\n";
                                blockStart = true;
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
                                    message += $"{string.Format(ModTranslation.GetString("ErrorClientChangeVersion"), client.Character.Data.PlayerName)} (v{VersionPlayers[client.Id].version})\n";
                                    blockStart = true;
                                }
                            }
                        }
                    }
                }
                if (blockStart)
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
}