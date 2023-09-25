using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Buttons;
using System.Linq;
using Hazel;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles.Impostor;

public static class Rocket
{
    public static class CustomOptionData
    {
        private static int optionId = 205600;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption RocketButtonCooldown;
        public static CustomOption RocketButtonAfterCooldown;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, false, RoleId.Rocket); optionId++;
            PlayerCount = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "SettingPlayerCountName", CustomOptionHolder.ImpostorPlayers[0], CustomOptionHolder.ImpostorPlayers[1], CustomOptionHolder.ImpostorPlayers[2], CustomOptionHolder.ImpostorPlayers[3], Option); optionId++;
            RocketButtonCooldown = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "RocketButtonCooldownSetting", 30f, 2.5f, 60f, 2.5f,  Option); optionId++;
            RocketButtonAfterCooldown = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "RocketButtonAfterCooldown", 5f, 0.5f, 15f, 0.5f, Option); optionId++;
        }
    }
    public static void FixedUpdate()
    {
        //処理するデータがないならパス
        if (RoleData.RocketData.Count <= 0)
            return;
        foreach (KeyValuePair<PlayerControl, List<PlayerControl>> data in (Dictionary<PlayerControl,List<PlayerControl>>)RoleData.RocketData)
        {
            //削除するか判定する
            if (data.Key == null || data.Value == null || data.Value.Count <= 0 || data.Key.IsDead() || data.Value.IsAllDead() ||
                !data.Key.IsRole(RoleId.Rocket))
            {
                RoleData.RocketData.Remove(data.Key);
                return;
            }
            //死亡している場合にさよなら
            data.Value.RemoveAll(x => x == null || x.IsDead());
            foreach (PlayerControl player in data.Value)
            {
                player.transform.position = data.Key.transform.position;
            }
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;
        public static float RocketButtonCooldown;
        public static float RocketButtonAfterCooldown;
        public static PlayerData<List<PlayerControl>> RocketData;
        public static List<PlayerControl> LocalData => RocketData[PlayerControl.LocalPlayer];

        public static void ClearAndReload()
        {
            Player = new();
            RocketButtonCooldown  = CustomOptionData.RocketButtonCooldown.GetFloat();
            RocketButtonAfterCooldown = CustomOptionData.RocketButtonAfterCooldown.GetFloat();
            RocketData = new(true, new());
        }
    }

    internal static class Button
    {
        private static CustomButton RocketSeizeButton;
        private static CustomButton RocketRocketButton;
        private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.RocketButton.png", 115f);

        internal static void SetupCustomButtons(HudManager hm)
        {
            RocketSeizeButton = new(
                () =>
                {
                    // ここに能力のコードを記載する
                    PlayerControl target = HudManagerStartPatch.SetTarget(RoleData.LocalData, Crewmateonly:true);
                    if (target == null)
                        return;
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RocketSeize);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.RocketSeize(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
                    RocketSeizeButton.MaxTimer = RoleData.RocketButtonCooldown;
                    RocketSeizeButton.Timer = RoleData.RocketButtonCooldown;
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Rocket; },
                () => { return PlayerControl.LocalPlayer.CanMove && HudManagerStartPatch.SetTarget(RoleData.LocalData, Crewmateonly: true); },
                () => { ResetRocketButtonCool(); },
                GetButtonSprite(),
                new Vector3(-2f, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                49,
                () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("RocketSeizeButtonName"),
                showButtonText = true
            };

            RocketRocketButton = new(
                () =>
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RocketLetsRocket);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.RocketLetsRocket(PlayerControl.LocalPlayer.PlayerId);
                    ResetRocketButtonCool();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Rocket && RoleData.LocalData.Count > 0; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { ResetRocketButtonCool(); },
                GetButtonSprite(),
                new Vector3(-2f, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                49,
                () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("RocketSeizeButtonName"),
                showButtonText = true
            };
        }

        private static void ResetRocketButtonCool()
        {
            RocketSeizeButton.MaxTimer = RoleData.RocketButtonCooldown;
            RocketSeizeButton.Timer = RoleData.RocketButtonCooldown;
            RocketRocketButton.MaxTimer = 0;
            RocketRocketButton.Timer = 0;
        }
    }

    // ここにコードを書きこんでください
}