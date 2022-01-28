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
        ShareOptions = 91,
        SetRole,
        RPCClergymanLightOut,
        SheriffKill,
        CustomRPCKill,
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
                    CustomOption.CustomOption option = CustomOption.CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                    option.updateSelection((int)selection);
                }
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }
        public static void SetRole(byte playerid,byte RPCRoleId)
        {
            ModHelpers.playerById(playerid).setRole((RoleId)RPCRoleId);
        }
        public static void SheriffKill(byte SheriffId,byte TargetId,bool MissFire)
        {
            PlayerControl sheriff = ModHelpers.playerById(SheriffId);
            PlayerControl target = ModHelpers.playerById(TargetId);
            SuperNewRolesPlugin.Logger.LogInfo(sheriff.PlayerId);
            SuperNewRolesPlugin.Logger.LogInfo(sheriff.PlayerId);
            if (sheriff == null || target == null) return;

            if (MissFire)
            {
                sheriff.MurderPlayer(sheriff);
            } else
            {
                sheriff.MurderPlayer(target);
            }

        }
        public static void CustomRPCKill(byte notTargetId,byte targetId)
        {
            SuperNewRolesPlugin.Logger.LogInfo(notTargetId);
            SuperNewRolesPlugin.Logger.LogInfo(targetId);
            if (notTargetId == targetId)
            {
                PlayerControl Player = ModHelpers.playerById(targetId);
                Player.MurderPlayer(Player);
            }
            else
            {
                PlayerControl notTargetPlayer = ModHelpers.playerById(notTargetId);
                PlayerControl TargetPlayer = ModHelpers.playerById(targetId);
                notTargetPlayer.MurderPlayer(TargetPlayer);
            }
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
                        case (byte)CustomRPC.SheriffKill:
                            RPCProcedure.SheriffKill(reader.ReadByte(),reader.ReadByte(),reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.CustomRPCKill:
                            RPCProcedure.CustomRPCKill(reader.ReadByte(), reader.ReadByte());
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
