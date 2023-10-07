using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using Hazel;

namespace SuperNewRoles.Roles.Impostor;

public static class Spider
{
    public static class CustomOptionData
    {
        private static int optionId = 205700;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption SpiderButtonCooldown;
        public static CustomOption SpiderButtonActivate;
        public static CustomOption SpiderSpentSetting;
        public static CustomOption SpiderTrapKillTimeSetting;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, false, RoleId.Spider); optionId++;
            PlayerCount = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "SettingPlayerCountName", CustomOptionHolder.ImpostorPlayers[0], CustomOptionHolder.ImpostorPlayers[1], CustomOptionHolder.ImpostorPlayers[2], CustomOptionHolder.ImpostorPlayers[3], Option); optionId++;
            SpiderButtonCooldown = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "SpiderButtonCooldownSetting", 30f, 2.5f, 60f, 2.5f,  Option); optionId++;
            SpiderButtonActivate = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "SpiderButtonActivateSetting", 5f, 0f, 60f, 2.5f, Option); optionId++;
            SpiderSpentSetting = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "SpiderSpentSetting", 10f, 2.5f, 60f, 2.5f, Option); optionId++;
            SpiderTrapKillTimeSetting = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "SpiderTrapKillTimeSetting", 10f, 2.5f, 60f, 2.5f, Option); optionId++;
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;
        public static float SpiderButtonCooldown;

        public static void ClearAndReload()
        {
            Player = new();
            SpiderButtonCooldown  = CustomOptionData.SpiderButtonCooldown.GetFloat();
            SpiderTrap.ClearAndReloads();
        }
    }

    internal static class Button
    {
        private static CustomButton SpiderButton;
        private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpiderButton.png", 115f);

        internal static void SetupCustomButtons(HudManager hm)
        {
            SpiderButton = new(
                () =>
                {
                    Vector2 pos = PlayerControl.LocalPlayer.transform.position;
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetSpiderTrap);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(pos.x);
                    writer.Write(pos.y);
                    writer.Write((ushort)(SpiderTrap.MaxId+1));
                    writer.EndRPC();
                    RPCProcedure.SetSpiderTrap(PlayerControl.LocalPlayer.PlayerId, pos.x, pos.y, (ushort)(SpiderTrap.MaxId + 1));
                    // ここに能力のコードを記載する

                    ResetSpiderButtonCool();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Spider; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { ResetSpiderButtonCool(); },
                GetButtonSprite(),
                new Vector3(-2f, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                49,
                () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("SpiderButtonName"),
                showButtonText = true
            };
        }

        private static void ResetSpiderButtonCool()
        {
            SpiderButton.MaxTimer = RoleData.SpiderButtonCooldown;
            SpiderButton.Timer = RoleData.SpiderButtonCooldown;
        }
    }

    // ここにコードを書きこんでください
}