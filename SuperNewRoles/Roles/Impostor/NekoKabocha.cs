using System.Collections.Generic;
using SuperNewRoles.Patches;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptions;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Impostor
{
    public static class NekoKabocha
    {
        private const int OptionId = 992;
        public static CustomRoleOption NekoKabochaOption;
        public static CustomOption NekoKabochaPlayerCount;
        public static CustomOption KillCoolDown;
        private static CustomOption CanRevengeCrewMate;
        private static CustomOption CanRevengeNeutral;
        private static CustomOption CanRevengeImpostor;
        private static CustomOption CanRevengeExiled;
        public static void SetupCustomOptions()
        {
            NekoKabochaOption = new(OptionId, true, CustomOptionType.Impostor, "NekoKabochaName", color, 1);
            NekoKabochaPlayerCount = Create(OptionId + 1, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], NekoKabochaOption);
            KillCoolDown = Create(OptionId + 2, true, CustomOptionType.Impostor, "KillCoolDown", 40f, 0f, 120f, 2.5f, NekoKabochaOption, format: "unitSeconds");
            CanRevengeCrewMate = Create(OptionId + 3, true, CustomOptionType.Impostor, "CanRevengeCrewMate", true, NekoKabochaOption);
            CanRevengeNeutral = Create(OptionId + 4, true, CustomOptionType.Impostor, "CanRevengeNeutral", true, NekoKabochaOption);
            CanRevengeImpostor = Create(OptionId + 5, true, CustomOptionType.Impostor, "CanRevengeImpostor", true, NekoKabochaOption);
            CanRevengeExiled = Create(OptionId + 6, true, CustomOptionType.Impostor, "CanRevengeExiled", true, NekoKabochaOption);
        }

        public static List<PlayerControl> NekoKabochaPlayer;
        public static Color32 color = ImpostorRed;
        public static bool CanRevengeCrew;
        public static bool CanRevengeNeut;
        public static bool CanRevengeImp;
        public static bool CanRevengeExile;
        public static void ClearAndReload()
        {
            NekoKabochaPlayer = new();
            CanRevengeCrew = CanRevengeCrewMate.GetBool();
            CanRevengeNeut = CanRevengeNeutral.GetBool();
            CanRevengeImp = CanRevengeImpostor.GetBool();
            CanRevengeExile = CanRevengeExiled.GetBool();
        }

        public static void OnKill(PlayerControl killer)
        {
            if ((killer.IsCrew() && CanRevengeCrew) ||
                (killer.IsNeutral() && CanRevengeNeut) ||
                (killer.IsImpostor() && CanRevengeImp))
            {
                killer.RpcMurderPlayer(killer);
                killer.RpcSetFinalStatus(FinalStatus.Revenge);
            }
        }
    }
}