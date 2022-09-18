using UnityEngine;
using SuperNewRoles.Patch;
using static SuperNewRoles.Modules.CustomOptions;
using System.Collections.Generic;
using SuperNewRoles.Buttons;

namespace SuperNewRoles.Roles.Impostor
{
    public class Conjurer
    {
        private const int Id = 992;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption CoolDown;

        public static void SetupCustomOptions()
        {
            Option = new(Id, false, CustomOptionType.Impostor, "ConjurerName", color, 1);
            PlayerCount = CustomOption.Create(Id + 1, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], Option);
            CoolDown = CustomOption.Create(Id + 2, false, CustomOptionType.Impostor, "CoolDown", 10f, 1f, 60f, 1f, Option);
        }
        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;
        public static int Count;
        public static Vector2[] Positions;
        public static void ClearAndReload()
        {
            Player = new();
            Count = 0;
            Positions = new Vector2[] { new(), new(), new() };
        }

        private static Sprite AddbuttonSprite;
        private static Sprite StartbuttonSprite;
        public static Sprite GetBeaconButtonSprite()
        {
            if (AddbuttonSprite) return AddbuttonSprite;
            AddbuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ConjurerAddButton.png", 115f);
            return AddbuttonSprite;
        }
        public static Sprite GetStartButtonSprite()
        {
            if (StartbuttonSprite) return StartbuttonSprite;
            StartbuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ConjurerStartButton.png", 115f);
            return StartbuttonSprite;
        }

        public static CustomButton BeaconButton;
        public static CustomButton StartButton;
        public static void SetupCustomButtons(HudManager hm)
        {
            BeaconButton = new(
            () =>
            {
                switch (Count)
                {
                    case 0:
                        Positions[0] = PlayerControl.LocalPlayer.transform.position;
                        break;
                    case 1:
                        Positions[1] = PlayerControl.LocalPlayer.transform.position;
                        break;
                    case 2:
                        Positions[2] = PlayerControl.LocalPlayer.transform.position;
                        break;
                    default:
                        Logger.Warn($"無効な値です:{Count}", "Conjurer Add");
                        break;
                }
                Count++;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Conjurer; },
            () =>
            { return PlayerControl.LocalPlayer.CanMove && Count != 3; },
            () => { ResetCoolDown(); },
            GetBeaconButtonSprite(),
            new Vector3(-1.8f, -0.06f, 0),
            hm,
            hm.AbilityButton,
            KeyCode.Q,
            8,
            () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("ConjurerBeaconName"),
                showButtonText = true
            };

            StartButton = new(
            () =>
            {
                ResetCoolDown();
                Count = 0;

            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Conjurer; },
            () =>
            { return PlayerControl.LocalPlayer.CanMove && Count == 3; },
            () => { ResetCoolDown(); },
            GetStartButtonSprite(),
            new Vector3(-1.8f, -0.06f, 0),
            hm,
            hm.AbilityButton,
            KeyCode.F,
            48,
            () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("ConjurerAddName"),
                showButtonText = true
            };
        }

        public static void ResetCoolDown()
        {
            BeaconButton.MaxTimer = CoolDown.GetFloat();
            BeaconButton.Timer = CoolDown.GetFloat();
        }
    }
}