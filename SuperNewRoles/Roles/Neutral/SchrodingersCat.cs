using SuperNewRoles.Patch;
using static SuperNewRoles.Modules.CustomOptions;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Roles.Neutral
{
    public static class SchrodingersCat
    {
        private const int OptionId = 999;

        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        private static CustomOption BecomeRoleByImpostor;
        private static CustomOption BecomeRoleByJackal;
        public static void SetupCustomOptions()
        {
            Option = new(985, false, CustomOptionType.Neutral, "SchrodingersCatName", Color, 1);
            PlayerCount = CustomOption.Create(986, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], Option);
            BecomeRoleByImpostor = CustomOption.Create(987, false, CustomOptionType.Neutral, "BecomeRolesByImpostor", new string[] { "ImpostorName", "MadmateName" }, Option);
            BecomeRoleByJackal = CustomOption.Create(988, false, CustomOptionType.Neutral, "BecomeRolesByJackal", new string[] { "JackalName", "JackalFriendsName" }, Option);
        }

        public static List<PlayerControl> Player;
        public static Color32 Color = new(128, 128, 128, byte.MaxValue);
        private static int BeImpRole; // 1ならインポスター、2ならマッドメイト
        private static int BeJacRole;
        public static void ClearAndReload()
        {
            Player = new();
            BeImpRole = BecomeRoleByImpostor.GetSelection();
            BeJacRole = BecomeRoleByJackal.GetSelection();
        }

        public static void CatRoleChange(PlayerControl cat,PlayerControl killer){
            killer.RpcShowGuardEffect(cat);
            cat.RpcShowGuardEffect(killer);
            if (killer.IsImpostor()){
                SetNamesClass.SetPlayerNameColor(cat,RoleClass.ImpostorRed);
                if (BeImpRole == 1){
                    cat.RPCSetRoleUnchecked(RoleTypes.Impostor);
                    cat.SetRole(RoleId.DefaultRole);
                } else {
                    cat.SetRole(RoleId.MadMate);
                }
            } else if (killer.IsJackalTeam()) {
                SetNamesClass.SetPlayerNameColor(cat,RoleClass.Jackal.color);
                if (BeJacRole == 1) {
                    cat.SetRole(RoleId.Jackal);
                } else {
                    cat.SetRole(RoleId.JackalFriends);
                }
            } else {
                switch (killer.GetRole()){
                    case RoleId.Sheriff:
                    case RoleId.RemoteSheriff:
                    case RoleId.MeetingSheriff:
                        cat.RPCSetRoleUnchecked(RoleTypes.Crewmate);
                        cat.SetRole(RoleId.DefaultRole);
                        SetNamesClass.SetPlayerNameColor(cat,Palette.White);
                        break;
                    case RoleId.Egoist:
                        cat.SetRole(RoleId.Egoist);
                        SetNamesClass.SetPlayerNameColor(cat,RoleClass.Egoist.color);
                        break;
                    case RoleId.SchrodingersCat:
                        Logger.SendInGame("自爆");
                        break;
                    default:
                        Logger.Info($"だれですか({killer.GetRole()})", "Change SchrodingersCat Role");
                        break;
                }
            }
        }
    }
}