using HarmonyLib;
using Hazel;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    class ShareGameVersion
    {
        public static Dictionary<int, PlayerVersion> playerVersions = new Dictionary<int, PlayerVersion>();
        public static bool IsVersionOK = false;
        public static bool IsChangeVersion = false;
        public static bool IsRPCSend = false;
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix()
            {
                if (PlayerControl.LocalPlayer != null)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("バージョンシェア！");
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareSNRVersion, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Major);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Minor);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Build);
                    writer.WritePacked(AmongUsClient.Instance.ClientId);
                    writer.Write((byte)(SuperNewRolesPlugin.Version.Revision < 0 ? 0xFF : SuperNewRolesPlugin.Version.Revision));
                    writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    CustomRPC.RPCProcedure.ShareSNRversion(SuperNewRolesPlugin.Version.Major, SuperNewRolesPlugin.Version.Minor, SuperNewRolesPlugin.Version.Build, SuperNewRolesPlugin.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
                }
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix() {
                GameStartManagerUpdatePatch.Proce = 0;
                GameStartManagerUpdatePatch.VersionPlayers = new Dictionary<int, PlayerVersion>();
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            private static bool update = false;
            private static string currentText = "";
            public static Dictionary<int, PlayerVersion> VersionPlayers = new Dictionary<int, PlayerVersion>();
            public static int Proce;

            public static void Postfix(GameStartManager __instance)
            {
                Proce++;
                if (Proce >= 10) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareSNRVersion, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Major);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Minor);
                    writer.Write((byte)SuperNewRolesPlugin.Version.Build);
                    writer.WritePacked(AmongUsClient.Instance.ClientId);
                    writer.Write((byte)(SuperNewRolesPlugin.Version.Revision < 0 ? 0xFF : SuperNewRolesPlugin.Version.Revision));
                    writer.Write(Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToByteArray());
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    CustomRPC.RPCProcedure.ShareSNRversion(SuperNewRolesPlugin.Version.Major, SuperNewRolesPlugin.Version.Minor, SuperNewRolesPlugin.Version.Build, SuperNewRolesPlugin.Version.Revision, Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId, AmongUsClient.Instance.ClientId);
                    Proce = 0;
                } 
                
                string message = "";
                bool blockStart = false;
                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                    {
                        if (client.Id != AmongUsClient.Instance.HostId) {
                            try
                            {
                                var a = VersionPlayers[client.Id];
                                message += "\n" + string.Format(ModTranslation.getString("ErrorClientNoVersion"), client.PlayerName);
                                blockStart = true;
                            }
                            catch { 
                            }
                        }
                    }
                } else {
                    foreach (InnerNet.ClientData data in AmongUsClient.Instance.allClients) {
                        if (data.Id == AmongUsClient.Instance.HostId) {
                            try
                            {
                                var a = VersionPlayers[data.Id];
                                message += "\n" + ModTranslation.getString("ErrorHostNoVersion");
                                blockStart = true;
                            }
                            catch { 

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
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                }
            }
            /**
                if (!AmongUsClient.Instance.AmHost)
                {
                    if (!playerVersions.ContainsKey(AmongUsClient.Instance.HostId) || SuperNewRolesPlugin.Version.CompareTo(playerVersions[AmongUsClient.Instance.HostId].version) != 0)
                    {
                        __instance.GameStartText.text = ModTranslation.getString("ErrorHostNoVersion");
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
            return Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.Equals(this.guid);
        }
    }
}