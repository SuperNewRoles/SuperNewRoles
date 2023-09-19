using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class Crook
{
    public static class CustomOptionData
    {
        private const int optionId = 303600;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption NumberOfInsuranceClaimsReceivedRequiredToWin;
        public static CustomOption TimeTheAbilityToInsureOthersIsAvailable;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, true, RoleId.Crook);
            PlayerCount = CustomOption.Create(optionId + 1, true, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option);
            NumberOfInsuranceClaimsReceivedRequiredToWin = CustomOption.Create(optionId + 2, true, CustomOptionType.Neutral, "CrookNumberOfInsuranceClaimsReceivedRequiredToWin", 3f, 1f, 10f, 1f, Option);
            TimeTheAbilityToInsureOthersIsAvailable = CustomOption.Create(optionId + 3, true, CustomOptionType.Neutral, "CrookTimeTheAbilityToInsureOthersIsAvailable", 10f, 5f, 60f, 5f, Option);
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = new(96, 161, 189, byte.MaxValue);
        internal static float TimeForAbilityUse { get; private set; }

        public static void ClearAndReload()
        {
            Player = new();
            TimeForAbilityUse = CustomOptionData.TimeTheAbilityToInsureOthersIsAvailable.GetFloat();
        }
    }

    internal static class Ability
    {
        private static class Button
        {
            private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CrookButton.png", 115f);
        }
    }
    // ここにコードを書きこんでください
}