using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.ShareCosmetics
{
    class SharePatch
    {
        public static Dictionary<int,string> PlayerUrl;
        public static Dictionary<int, string> PlayerDatas;
        public static Dictionary<int, CosmeticsObject> PlayerObjects;
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
        public class AmongUsClientOnPlayerJoinedPatch
        {
            public static void Postfix()
            {
                if (PlayerControl.LocalPlayer != null && ConfigRoles.IsShareCosmetics.Value && ConfigRoles.ShareCosmeticsNamePlatesURL.Value != "")
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareCosmetics, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(ConfigRoles.ShareCosmeticsNamePlatesURL.Value);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    CustomRPC.RPCProcedure.ShareCosmetics(
                        (byte)PlayerControl.LocalPlayer.PlayerId,
                        ConfigRoles.ShareCosmeticsNamePlatesURL.Value
                        );
                }
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public class GameStartManagerUpdatePatch
        {
            public static int Proce;
            public static void Postfix(GameStartManager __instance)
            {
                Proce++;
                if (Proce >= 10)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareCosmetics, Hazel.SendOption.Reliable, -1);
                    writer.Write((byte)PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(ConfigRoles.ShareCosmeticsNamePlatesURL.Value);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    CustomRPC.RPCProcedure.ShareCosmetics(
                        (byte)PlayerControl.LocalPlayer.PlayerId,
                        ConfigRoles.ShareCosmeticsNamePlatesURL.Value
                        );
                    Proce = 0;
                }
                /**
                SuperNewRolesPlugin.Logger.LogInfo("-plates-");
                SuperNewRolesPlugin.Logger.LogInfo(SharePatch.PlayerObjects[PlayerControl.LocalPlayer.PlayerId].GUID);
                SuperNewRolesPlugin.Logger.LogInfo("ALL:"+ PlayerObjects[PlayerControl.LocalPlayer.PlayerId].AllNamePlates);
                foreach (NamePlatesObject a in SharePatch.PlayerObjects[PlayerControl.LocalPlayer.PlayerId].AllNamePlates)
                {
                    SuperNewRolesPlugin.Logger.LogInfo("--");
                    SuperNewRolesPlugin.Logger.LogInfo("NAME"+a.Name);
                    SuperNewRolesPlugin.Logger.LogInfo("AUTHOR"+a.Author);
                    SuperNewRolesPlugin.Logger.LogInfo("URL"+a.Url);
                    //SuperNewRolesPlugin.Logger.LogInfo("--");
                }
                SuperNewRolesPlugin.Logger.LogInfo("--------");
                **/
            }
        }
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
        public class GameStartManagerStartPatch
        {
            public static void Postfix()
            {
                ShareNamePlate.NamePlateSprites = new Dictionary<int, Sprite>();
                PlayerUrl = new Dictionary<int, string>();
                PlayerDatas = new Dictionary<int, string>();
                PlayerObjects = new Dictionary<int, CosmeticsObject>();
            }
        }
        
    }
}
