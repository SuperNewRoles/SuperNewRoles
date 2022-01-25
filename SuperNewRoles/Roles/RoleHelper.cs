using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using UnhollowerBaseLib;
using UnityEngine;
using System.Linq;
using HarmonyLib;
using Hazel;

namespace SuperNewRoles
{


    public static class RoleHelpers
    {

        public static void setRole(this PlayerControl player, SuperNewRoles.CustomRPC.RoleId role)
        {
            SuperNewRolesPlugin.Logger.LogInfo("SetRole");
            switch (role)
            {
                case (CustomRPC.RoleId.SoothSayer):
                    Roles.RoleClass.SoothSayer.SoothSayerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Jester):
                    Roles.RoleClass.Jester.JesterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Lighter):
                    Roles.RoleClass.Lighter.LighterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilLighter):
                    Roles.RoleClass.EvilLighter.EvilLighterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilScientist):
                    Roles.RoleClass.EvilScientist.EvilScientistPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Sheriff):
                    Roles.RoleClass.Sheriff.SheriffPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.MeetingSheriff):
                    Roles.RoleClass.MeetingSheriff.MeetingSheriffPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.AllKiller):
                    Roles.RoleClass.AllKiller.AllKillerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Teleporter):
                    Roles.RoleClass.Teleporter.TeleporterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.SpiritMedium):
                    Roles.RoleClass.SpiritMedium.SpiritMediumPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.SpeedBooster):
                    Roles.RoleClass.SpeedBooster.SpeedBoosterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilSpeedBooster):
                    Roles.RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Tasker):
                    Roles.RoleClass.Tasker.TaskerPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Doorr):
                    Roles.RoleClass.Doorr.DoorrPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.EvilDoorr):
                    Roles.RoleClass.EvilDoorr.EvilDoorrPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Sealdor):
                    Roles.RoleClass.Sealdor.SealdorPlayer.Add(player);
                    break;
                case (CustomRPC.RoleId.Clergyman):
                    Roles.RoleClass.Clergyman.ClergymanPlayer.Add(player);
                    break;
                default:
                    SuperNewRolesPlugin.Logger.LogError($"setRole: no method found for role type {role}");
                    return;
            }
        }
        public static SuperNewRoles.CustomRPC.RoleId getRole(this PlayerControl player)
        {
            SuperNewRolesPlugin.Logger.LogInfo(Roles.RoleClass.MeetingSheriff.MeetingSheriffPlayer);
            SuperNewRolesPlugin.Logger.LogInfo(Roles.RoleClass.MeetingSheriff.MeetingSheriffPlayer.IsCheckListPlayerControl(player));
            if (SuperNewRoles.Roles.RoleClass.SoothSayer.SoothSayerPlayer.IsCheckListPlayerControl(player))
            {
                return SuperNewRoles.CustomRPC.RoleId.SoothSayer;
            } else if (Roles.RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(player)) {
                return CustomRPC.RoleId.Jester;
            } else if (Roles.RoleClass.Lighter.LighterPlayer.IsCheckListPlayerControl(player)) {
                return CustomRPC.RoleId.Lighter;
            } else if (Roles.RoleClass.EvilLighter.EvilLighterPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilLighter;
            }
            else if (Roles.RoleClass.EvilScientist.EvilScientistPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilScientist;
            }
            else if (Roles.RoleClass.Sheriff.SheriffPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Sheriff;
            }
            else if (Roles.RoleClass.MeetingSheriff.MeetingSheriffPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.MeetingSheriff;
            }
            else if (Roles.RoleClass.AllKiller.AllKillerPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.AllKiller;
            }
            else if (Roles.RoleClass.Teleporter.TeleporterPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Teleporter;
            }
            else if (Roles.RoleClass.SpiritMedium.SpiritMediumPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.SpiritMedium;
            }
            else if (Roles.RoleClass.SpeedBooster.SpeedBoosterPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.SpeedBooster;
            }
            else if (Roles.RoleClass.EvilSpeedBooster.EvilSpeedBoosterPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilSpeedBooster;
            }
            else if (Roles.RoleClass.Tasker.TaskerPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Tasker;
            }
            else if (Roles.RoleClass.Doorr.DoorrPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Doorr;
            }
            else if (Roles.RoleClass.EvilDoorr.EvilDoorrPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.EvilDoorr;
            }
            else if (Roles.RoleClass.Sealdor.SealdorPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Sealdor;
            }
            else if (Roles.RoleClass.Clergyman.ClergymanPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.Clergyman;
            }
            return SuperNewRoles.CustomRPC.RoleId.DefaultRole;

        }
        public static bool isDead(this PlayerControl player)
        {
            return player.Data.IsDead || player.Data.Disconnected;
        }

        public static bool isAlive(this PlayerControl player)
        {
            return !isDead(player);
        }
    }
}
