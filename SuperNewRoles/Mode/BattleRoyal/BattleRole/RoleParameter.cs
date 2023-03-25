using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole
{
    public static class RoleParameter
    {
        public const float ReviverPlayerStuckTime = 12;
        public const float ReviverRevivePlayerStuckTime = 3;
        public const float ReviverShowNotificationDurationTime = 3;
        public const float GuardrawerAbilityTime = 6;
        public const float GuardrawerShowNotificationDurationTime = 1.5f;
        public const float KingPosterPlayerStuckTimeStart = 5;
        public const float KingPosterPlayerStuckTimeEnd = 5;
        public const float KingPosterPlayerAbilityTime = 10;
        public const float KingPosterShowNotificationDurationTime = 3;
        //KingPosterAbilityCosmetics
        public const string KingPosterAbilityCosmeticHat = "hat_pk02_Crown";
        public static readonly string[] KingPosterAbilityCosmeticSpecialHats = new string[3] { "hat_crownDouble", "hat_crownBean", "hat_crownTall" };
        public const string KingPosterAbilityCosmeticVisor = "visor_hl_marine";
        public static readonly string[] KingPosterAbilityCosmeticSkins = new string[3] { "skin_D2Hunter", "skin_D2Osiris", "skin_D2Saint14" };

        public const float RevengerReviveTime = 7;

        public const float CrystalMagicianCrystalTime = 10;
        public const float CrystalMagicianSuperPowerTime = 5;
    }
}
