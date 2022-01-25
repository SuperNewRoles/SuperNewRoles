using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using SuperNewRoles.Patches;
using SuperNewRoles.CustomOption;

namespace SuperNewRoles.CustomRPC
{
    public enum RoleId
    {
        DefaultRole,
        SoothSayer,
        Jester,
        Lighter,
        EvilLighter,
        EvilScientist,
        Sheriff,
        MeetingSheriff,
        AllKiller,
        Teleporter,
        SpiritMedium,
        SpeedBooster,
        EvilSpeedBooster,
        Tasker,
        Doorr,
        EvilDoorr,
        Sealdor,
        Speeder,
        Freezer,
        Guesser,
        EvilGuesser,
        Vulture,
        NiceScientist,
        Clergyman,
        MadMate

    }

    enum CustomRPC
    {
        ShareOptions,
        SetRole,
        sheriffKill,
        RPCClergymanLightOut,
    }
    public static class RPCProcedure
    {

        // Main Controls


        public static void ShareOptions(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption.CustomOption option = CustomOption.CustomOption.options[0];
                    option.updateSelection((int)selection);
                }
            }
            catch (Exception e)
            {
                if (ConfigRoles.DebugMode.Value)
                {
                    SuperNewRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
                }
            }
        }
        public static void SetRole(byte playerid,byte RPCRoleId)
        {
            ModHelpers.playerById(playerid).setRole((RoleId)RPCRoleId);
        }

        public static void sheriffKill(byte sheriffId, byte targetId)
        {
            SuperNewRolesPlugin.Logger.LogInfo(sheriffId);
            SuperNewRolesPlugin.Logger.LogInfo(targetId);
            PlayerControl sheriffplayer = ModHelpers.playerById(sheriffId);
            PlayerControl targetplayer = ModHelpers.playerById(targetId);

            SuperNewRolesPlugin.Logger.LogInfo(sheriffplayer.PlayerId);
            SuperNewRolesPlugin.Logger.LogInfo(targetplayer.PlayerId);
            if (sheriffplayer == null || targetplayer == null) return;
            if (!Roles.Sheriff.IsSheriffKill(targetplayer))
            {
                sheriffplayer.MurderPlayer(sheriffplayer);
                return;
            }

            sheriffplayer.MurderPlayer(targetplayer);
        }
        public static void RPCClergymanLightOut(bool Start)
        {
            SuperNewRolesPlugin.Logger.LogInfo(Start);
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (Start)
            {
                SuperNewRolesPlugin.Logger.LogInfo("start");
                Roles.Clergyman.LightOutStartRPC();
            }
            else
            {
                SuperNewRolesPlugin.Logger.LogInfo("end");
                Roles.Clergyman.LightOutEndRPC();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class RPCHandlerPatch
        {
            static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                try
                {
                    byte packetId = callId;
                    switch (packetId)
                    {

                        // Main Controls


                        case (byte)CustomRPC.ShareOptions:
                            RPCProcedure.ShareOptions((int)reader.ReadPackedUInt32(), reader);
                            break;
                        case (byte)CustomRPC.SetRole:
                            RPCProcedure.SetRole(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.sheriffKill:
                            RPCProcedure.sheriffKill(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.RPCClergymanLightOut:
                            RPCProcedure.RPCClergymanLightOut(reader.ReadBoolean());
                            break;

                    }
                }
                catch (Exception e)
                {
                    if (ConfigRoles.DebugMode.Value)
                    {
                        SuperNewRolesPlugin.Logger.LogError("Error while deserializing RPC: " + e.Message);
                    }
                }
            }
        }
        
    }
}
