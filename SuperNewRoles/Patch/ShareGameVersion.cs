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
        public static bool IsVersionOK = false;
        public static bool IsChangeVersion = false;
        public static bool IsRPCSend = false;
        public static float timer = 600;
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
                timer = 600f;
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            private static bool update = false;
            public static Dictionary<int, PlayerVersion> VersionPlayers = new Dictionary<int, PlayerVersion>();
            public static int Proce;
            private static string currentText = "";

            public static void Prefix(GameStartManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }
            public static void Postfix(GameStartManager __instance)
            {
                SuperNewRolesPlugin.Logger.LogInfo("-Version-");
                SuperNewRolesPlugin.Logger.LogInfo("My:"+AmongUsClient.Instance.ClientId);
                foreach (var pv in VersionPlayers) {
                    SuperNewRolesPlugin.Logger.LogInfo(pv.Key+","+pv.Value); 
                }
                SuperNewRolesPlugin.Logger.LogInfo("---------");
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
                if (ConfigRoles.IsVersionErrorView.Value)
                {
                    if (!AmongUsClient.Instance.AmClient)
                    {

                        if (!VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId))
                        {
                            message += "\n" + ModTranslation.getString("ErrorHostNoVersion");
                            blockStart = true;
                        }
                        else
                        {
                            var client = AmongUsClient.Instance.GetHost();
                            PlayerVersion PV = VersionPlayers[client.Id];
                            int diff = SuperNewRolesPlugin.Version.CompareTo(PV.version);
                            if (diff > 0)
                            {
                                message += $"{ModTranslation.getString("ErrorHostChangeVersion")} (v{VersionPlayers[client.Id].version.ToString()})\n";
                                blockStart = true;
                            }
                            else if (diff < 0)
                            {
                                message += $"{ModTranslation.getString("ErrorHostChangeVersion")} (v{VersionPlayers[client.Id].version.ToString()})\n";
                                blockStart = true;
                            }
                            else if (!PV.GuidMatches())
                            { // version presumably matches, check if Guid matches
                                message += $"{ModTranslation.getString("ErrorHostChangeVersion")} (v{VersionPlayers[client.Id].version.ToString()})\n";
                                blockStart = true;
                            }
                        }

                    }
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                    {
                        if (client.Id != AmongUsClient.Instance.HostId) {
                            if (!VersionPlayers.ContainsKey(client.Id))
                            {
                                message += string.Format(ModTranslation.getString("ErrorClientNoVersion"), client.PlayerName) + "\n";
                                blockStart = true;
                            }
                            else {
                                PlayerVersion PV = VersionPlayers[client.Id];
                                int diff = SuperNewRolesPlugin.Version.CompareTo(PV.version);
                                if (diff > 0)
                                {
                                    message += $"{string.Format(ModTranslation.getString("ErrorClientChangeVersion"),client.Character.Data.PlayerName)} (v{VersionPlayers[client.Id].version.ToString()})\n";
                                    blockStart = true;
                                }
                                else if (diff < 0)
                                {
                                    message += $"{string.Format(ModTranslation.getString("ErrorClientChangeVersion"), client.Character.Data.PlayerName)} (v{VersionPlayers[client.Id].version.ToString()})\n";
                                    blockStart = true;
                                }
                                else if (!PV.GuidMatches())
                                { // version presumably matches, check if Guid matches
                                    message += $"{string.Format(ModTranslation.getString("ErrorClientChangeVersion"), client.Character.Data.PlayerName)} (v{VersionPlayers[client.Id].version.ToString()})\n";
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
                    __instance.GameStartText.text = "";
                    __instance.GameStartText.transform.localPosition = __instance.StartButton.transform.localPosition;
                }

                if (update) currentText = __instance.PlayerCounter.text;

                timer = Mathf.Max(0f, timer -= Time.deltaTime);
                int minutes = (int)timer / 60;
                int seconds = (int)timer % 60;
                string suffix = $" ({minutes:00}:{seconds:00})";

                __instance.PlayerCounter.text = currentText + suffix;
                __instance.PlayerCounter.autoSizeTextContainer = true;
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