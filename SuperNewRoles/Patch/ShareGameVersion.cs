using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patch
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
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareSNRVersion, SendOption.Reliable, -1);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Major);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Minor);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Build);
                    writer.WritePacked(AmongUsClient.Instance.ClientId);
                    writer.Write((byte)(SuperNewRolesPlugin.Version.Revision < 0 ? 0xFF : SuperNewRolesPlugin.Version.Revision));
                    writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ShareSNRversion(SuperNewRolesPlugin.Version.Major, SuperNewRolesPlugin.Version.Minor, SuperNewRolesPlugin.Version.Build, SuperNewRolesPlugin.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
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

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareSNRVersion, SendOption.Reliable, -1);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Major);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Minor);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Build);
                    writer.WritePacked(AmongUsClient.Instance.ClientId);
                    writer.Write((byte)(SuperNewRolesPlugin.Version.Revision < 0 ? 0xFF : SuperNewRolesPlugin.Version.Revision));
                    writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ShareSNRversion(SuperNewRolesPlugin.Version.Major, SuperNewRolesPlugin.Version.Minor, SuperNewRolesPlugin.Version.Build, SuperNewRolesPlugin.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
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
                }
                if (ConfigRoles.IsVersionErrorView.Value)
                {
                    if (!AmongUsClient.Instance.AmHost)
                    {
                        if (!VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId))
                        {
                            message += "\n" + ModTranslation.GetString("ErrorHostNoVersion") + "\n";
                            blockStart = true;
                        }
                        else
                        {
                            var client = AmongUsClient.Instance.GetHost();
                            PlayerVersion PV = VersionPlayers[client.Id];
                            int diff = SuperNewRolesPlugin.Version.CompareTo(PV.version);
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
                                if (!(client.PlatformData.Platform != Platforms.StandaloneEpicPC && client.PlatformData.Platform != Platforms.StandaloneSteamPC && CustomOptions.DisconnectNotPCOption.GetBool()))
                                {
                                    message += string.Format(ModTranslation.GetString("ErrorClientNoVersion"), client.PlayerName) + "\n";
                                    blockStart = true;
                                }
                            }
                            else
                            {
                                PlayerVersion PV = VersionPlayers[client.Id];
                                int diff = SuperNewRolesPlugin.Version.CompareTo(PV.version);
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
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetRoomTimerRPC, SendOption.Reliable, -1);
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
                    //MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.AutoCreateRoom, SendOption.Reliable, -1);
                    //AmongUsClient.Instance.FinishRpcImmediately(writer);
                    //var roomid = InnerNet.GameCode.IntToGameName(AmongUsClient.Instance.GameId);
                    //AmongUsClient.Instance.StartCoroutine(CREATEROOMANDJOIN(roomid, AmongUsClient.Instance.GameId));
                    notcreateroom = true;
                }
            }
            /**
                if (!AmongUsClient.Instance.AmHost)
                {
                    if (!playerVersions.ContainsKey(AmongUsClient.Instance.HostId) || SuperNewRolesPlugin.Version.CompareTo(playerVersions[AmongUsClient.Instance.HostId].version) != 0)
                    {
                        __instance.GameStartText.text = ModTranslation.GetString("ErrorHostNoVersion");
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition + Vector3.up * 2;
                    }
                    else
                    {
                        __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                    }
                }**/
        }
    }
    public class PlayerVersion
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