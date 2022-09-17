using UnityEngine;
using SuperNewRoles.Patch;
using static SuperNewRoles.Modules.CustomOptions;
using System.Collections.Generic;


namespace SuperNewRoles.Roles.Impostor{
    public class Conjurer {
        private  const int Id =992;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;

        public static void SetupCustomOptions(){
            Option = new(Id, false, CustomOptionType.Impostor, "ConjurerName", color, 1);
            PlayerCount = CustomOption.Create(Id+1, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], Option);
        }
        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;
        public static int Count;
        public static void ClearAndReload()
        {
            Player = new();
            Count = 0;
        }


    }
}