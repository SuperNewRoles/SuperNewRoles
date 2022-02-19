using HarmonyLib;
using Hazel;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
/**
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
                GameStartManagerUpdatePatch.VersionPlayers = new List<PlayerControl>();
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            private static bool update = false;
            private static string currentText = "";
            public static List<PlayerControl> VersionPlayers = new List<PlayerControl>();

            public static void Prefix(GameStartManager __instance)
            {
                if (!AmongUsClient.Instance.AmHost || !GameData.Instance) return; // Not host or no instance
                update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
            }

            public static void Postfix(GameStartManager __instance)
            {
                return;
                if (AmongUsClient.Instance.AmHost)
                {
                    bool blockStart = false;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls) {
                        if (!VersionPlayers.Contains(p)) {
                            message += string.Format(ModTranslation.getString("ErrorClientNoVersion"), client.PlayerName);
                        }
                    }
                } else { 

                }
                    string message = "";
                    foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients.ToArray())
                    {
                        if (client.Character == null) continue;
                        var dummyComponent = client.Character.GetComponent<DummyBehaviour>();
                        if (dummyComponent != null && dummyComponent.enabled)
                            continue;
                        else if (!playerVersions.ContainsKey(client.Id))
                        {
                            message += string.Format(ModTranslation.getString("ErrorClientNoVersion"), client.PlayerName);
                            blockStart = true;
                        }
                        else
                        {
                            PlayerVersion PV = playerVersions[client.Id];
                            int diff = SuperNewRolesPlugin.Version.CompareTo(PV.version);

                            message += string.Format(ModTranslation.getString("ErrorClientChangeVersion"),client.PlayerName);
                            blockStart = true;
                            
                        }
                    }
                }
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
                }
            }
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
**/