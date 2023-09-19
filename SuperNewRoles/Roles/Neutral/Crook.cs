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

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, true, RoleId.Crook);
            PlayerCount = CustomOption.Create(optionId + 1, true, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option);
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = new(96, 161, 189, byte.MaxValue);

        public static void ClearAndReload()
        {
            Player = new();
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