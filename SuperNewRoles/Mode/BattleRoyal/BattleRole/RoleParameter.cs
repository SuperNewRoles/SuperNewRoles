using System;
using System.Collections.Generic;
using System.Text;
using Agartha;

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
        public const float GrimReaperEndStuckTime = 4;
        public const float GrimReaperFirstStuckTimeSkeld = 10;
        public const float GrimReaperFirstStuckTimeMira = 13;
        public const float GrimReaperFirstStuckTimeAgartha = 14.5f;
        public const float GrimReaperFirstStuckTimePolus = 16;
        public const float GrimReaperFirstStuckTimeAirship = 23;
        public const float GrimReaperShowNotificationDurationTime = 4.5f;


        public static float GrimReaperFirstStuckTime
        {
            get
            {
                switch (MapData.ThisMap)
                {
                    case CustomMapNames.Skeld:
                        return GrimReaperFirstStuckTimeSkeld;
                    case CustomMapNames.Mira:
                        return GrimReaperFirstStuckTimeMira;
                    case CustomMapNames.Agartha:
                        return GrimReaperFirstStuckTimeAgartha;
                    case CustomMapNames.Polus:
                        return GrimReaperFirstStuckTimePolus;
                    case CustomMapNames.Airship:
                        return GrimReaperFirstStuckTimeAirship;
                }
                return 0f;
            }
        }
        public static float GrimReaperAbilityTime => GrimReaperFirstStuckTime * 0.75f;
    }
}
