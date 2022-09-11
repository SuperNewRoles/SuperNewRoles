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
            Option = new(985, true, CustomOptionType.Neutral, "SchrodingersCatName", Color, 1);
            PlayerCount = CustomOption.Create(986, true, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], Option);
            BecomeRoleByImpostor = CustomOption.Create(987, true, CustomOptionType.Neutral, "BecomeRolesByImpostor", new string[] { "ImpostorName", "MadmateName" }, Option);
            BecomeRoleByJackal = CustomOption.Create(988, true, CustomOptionType.Neutral, "BecomeRolesByJackal", new string[] { "JackalName", "JackalFriendsName" }, Option);
        }

        public static List<PlayerControl> Player;
        public static Color32 Color = new(128, 128, 128, byte.MaxValue);
        private static int BeImpRole; // 0ならインポスター、1ならマッドメイト
        private static int BeJacRole;
        public static void ClearAndReload()
        {
            Player = new();
            BeImpRole = BecomeRoleByImpostor.GetSelection();
            BeJacRole = BecomeRoleByJackal.GetSelection();
        }

        public static void CatRoleChange(PlayerControl cat,PlayerControl killer){
            killer.RpcShowGuardEffect(cat);
            SetNamesClass.SetPlayerNameColor(cat,killer.NameText().color);
            if (killer.IsImpostor()){
                if (BeImpRole == 0){
                    DestroyableSingleton<RoleManager>.Instance.SetRole(cat, RoleTypes.Impostor);
                } else {
                    cat.SetRoleRPC(RoleId.MadMate);
                    if (!cat.IsMod())
                        cat.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                }
            } else if (killer.IsJackalTeam()) {
                if (BeJacRole == 0) {
                    cat.SetRoleRPC(RoleId.Jackal);
                } else {
                    cat.SetRoleRPC(RoleId.JackalFriends);
                    if (!cat.IsMod())
                        cat.RpcSetRoleDesync(RoleTypes.GuardianAngel);
                }
            } else {
                switch (killer.GetRole()){
                    case RoleId.Sheriff:
                    case RoleId.RemoteSheriff:
                    case RoleId.MeetingSheriff:
                        DestroyableSingleton<RoleManager>.Instance.SetRole(cat, RoleTypes.Crewmate);
                        cat.SetRoleRPC(RoleId.DefaultRole);
                        if (!cat.IsMod())
                            cat.SetRole(RoleTypes.GuardianAngel);
                        break;
                    case RoleId.Egoist:
                        cat.SetRoleRPC(RoleId.Egoist);
                        break;
                    case RoleId.SchrodingersCat:
                        Logger.Info("自爆", "Change SchrodingersCat Role");
                        break;
                    default:
                        Logger.Info($"だれですか({killer.GetRole()})", "Change SchrodingersCat Role");
                        break;
                }
            }
            Mode.SuperHostRoles.FixedUpdate.SetRoleName(cat);
        }
    }
}