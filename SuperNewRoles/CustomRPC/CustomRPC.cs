using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using SuperNewRoles.Patches;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Roles;

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
        MadMate,
        Bait,
        HomeSecurityGuard,
        StuntMan,
        Moving
    }

    enum CustomRPC
    {
        ShareOptions = 91,
        ShareSNRVersion,
        SetRole,
        SetQuarreled,
        RPCClergymanLightOut,
        SheriffKill,
        MeetingSheriffKill,
        CustomRPCKill,
        ReportDeadBody,
        ShareWinner,
        TeleporterTP,
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
        public static void ShareSNRversion(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            System.Version ver;
            if (revision < 0)
                ver = new System.Version(major, minor, build);
            else
                ver = new System.Version(major, minor, build, revision);
            Patch.ShareGameVersion.playerVersions[clientId] = new Patch.PlayerVersion(ver, guid);
        }
        public static void SetRole(byte playerid,byte RPCRoleId)
        {
            ModHelpers.playerById(playerid).setRole((RoleId)RPCRoleId);
        }
        public static void SetQuarreled(byte playerid1,byte playerid2)
        {
            var player1 = ModHelpers.playerById(playerid1);
            var player2 = ModHelpers.playerById(playerid2);
            RoleHelpers.SetQuarreled(player1,player2);
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
        public static void MeetingSheriffKill(byte SheriffId, byte TargetId, bool MissFire)
        {
            PlayerControl sheriff = ModHelpers.playerById(SheriffId);
            PlayerControl target = ModHelpers.playerById(TargetId);
            SuperNewRolesPlugin.Logger.LogInfo("-----");
            SuperNewRolesPlugin.Logger.LogInfo(SheriffId);
            SuperNewRolesPlugin.Logger.LogInfo(TargetId);
            SuperNewRolesPlugin.Logger.LogInfo(MissFire);
            SuperNewRolesPlugin.Logger.LogInfo("-----");
            target.Exiled();
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
            if (sheriff == null || target == null) return;
            DestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff,sheriff.name+"は"+target.name+"をシェリフキルした！");
            if (MissFire)
            {
                sheriff.Data.IsDead = true;
                if (PlayerControl.LocalPlayer == sheriff)
                {
                    HudManager.Instance.KillOverlay.ShowKillAnimation(sheriff.Data, sheriff.Data);
                }
                
            }
            else
            {
                target.Data.IsDead = true;
                if (PlayerControl.LocalPlayer == target)
                {
                    HudManager.Instance.KillOverlay.ShowKillAnimation(target.Data,sheriff.Data);
                }
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
        public static void ReportDeadBody(byte sourceId, byte targetId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (source != null && target != null) source.ReportDeadBody(target.Data);
        }
        public static void ShareWinner(byte playerid)
        {
            PlayerControl player = ModHelpers.playerById(playerid);
            EndGame.OnGameEndPatch.WinnerPlayer.Add(player);
        }
        public static void TeleporterTP(byte playerid)
        {
            var p = ModHelpers.playerById(playerid);
            PlayerControl.LocalPlayer.transform.position = p.transform.position;
            new CustomMessage(string.Format(ModTranslation.getString("TeleporterTPTextMessage"),p.nameText.text), 3);
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
                        case (byte)CustomRPC.ShareSNRVersion:
                            byte major = reader.ReadByte();
                            byte minor = reader.ReadByte();
                            byte patch = reader.ReadByte();
                            int versionOwnerId = reader.ReadPackedInt32();
                            byte revision = 0xFF;
                            Guid guid;
                            if (reader.Length - reader.Position >= 17)
                            { // enough bytes left to read
                                revision = reader.ReadByte();
                                // GUID
                                byte[] gbytes = reader.ReadBytes(16);
                                guid = new Guid(gbytes);
                            }
                            else
                            {
                                guid = new Guid(new byte[16]);
                            }
                            RPCProcedure.ShareSNRversion(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                            break;
                        case (byte)CustomRPC.SetRole:
                            RPCProcedure.SetRole(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SheriffKill:
                            RPCProcedure.SheriffKill(reader.ReadByte(),reader.ReadByte(),reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.MeetingSheriffKill:
                            RPCProcedure.MeetingSheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.CustomRPCKill:
                            RPCProcedure.CustomRPCKill(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.RPCClergymanLightOut:
                            RPCProcedure.RPCClergymanLightOut(reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.ReportDeadBody:
                            RPCProcedure.ReportDeadBody(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.ShareWinner:
                            RPCProcedure.ShareWinner(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.TeleporterTP:
                            RPCProcedure.TeleporterTP(reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SetQuarreled:
                            RPCProcedure.SetQuarreled(reader.ReadByte(),reader.ReadByte());
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
