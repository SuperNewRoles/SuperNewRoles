using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using UnityEngine;

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
        public static CustomOption RocketChargeTime;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, false, RoleId.Rocket); optionId++;
            PlayerCount = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "SettingPlayerCountName", CustomOptionHolder.ImpostorPlayers[0], CustomOptionHolder.ImpostorPlayers[1], CustomOptionHolder.ImpostorPlayers[2], CustomOptionHolder.ImpostorPlayers[3], Option); optionId++;
            RocketButtonCooldown = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "RocketButtonCooldownSetting", 30f, 2.5f, 60f, 2.5f, Option); optionId++;
            RocketButtonAfterCooldown = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "RocketButtonAfterCooldown", 5f, 0f, 60f, 2.5f, Option); optionId++;
            RocketChargeTime = CustomOption.Create(optionId, false, CustomOptionType.Impostor, "RocketChargeTimeSetting", 3, 0.5f, 10f, 0.5f, Option); optionId++;
        }
    }
    public static void WrapUp(PlayerControl exiled)
    {
        //処理するデータがないならパス
        if (RoleData.RocketData.Count <= 0)
            return;
        foreach (KeyValuePair<PlayerControl, List<PlayerControl>> data in (Dictionary<PlayerControl, List<PlayerControl>>)RoleData.RocketData)
        {
            //削除するか判定する
            if (data.Key == null || data.Value == null || data.Value.Count <= 0 || data.Key.IsDead() || data.Value.IsAllDead() ||
                !data.Key.IsRole(RoleId.Rocket) || (exiled != null && exiled.PlayerId == data.Key.PlayerId))
            {
                continue;
            }
            foreach (PlayerControl player in data.Value)
            {
                if (player == null || player.IsDead())
                    continue;
                player.Exiled();
            }
        }
        RoleData.RocketData.Reset();
    }
    public static void FixedUpdate()
    {
        //会議中なら処理しない
        if (RoleClass.IsMeeting)
            return;
        //処理するデータがないならパス
        if (RoleData.RocketData.Count <= 0)
            return;
        foreach (KeyValuePair<PlayerControl, List<PlayerControl>> data in (Dictionary<PlayerControl, List<PlayerControl>>)RoleData.RocketData)
        {
            //削除するか判定する
            if (data.Key == null || data.Value == null || data.Value.Count <= 0 || data.Key.IsDead() || data.Value.IsAllDead() ||
                !data.Key.IsRole(RoleId.Rocket))
            {
                RoleData.RocketData.Remove(data.Key);
                return;
            }
            int index = -1;
            foreach (PlayerControl player in data.Value)
            {
                index++;
                if (player == null || player.IsDead())
                {
                    //死亡している場合にさよなら
                    data.Value.RemoveAt(index);
                    continue;
                }
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
            RocketButtonCooldown = CustomOptionData.RocketButtonCooldown.GetFloat();
            RocketButtonAfterCooldown = CustomOptionData.RocketButtonAfterCooldown.GetFloat();
            RocketData = new(true, new());
        }
    }

    internal static class Button
    {
        private static CustomButton RocketSeizeButton;
        private static CustomButton RocketRocketButton;
        private static Sprite GetButtonSeizeSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.RocketSeizeButton.png", 115f);
        private static Sprite GetButtonRocketSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.RocketRocketButton.png", 115f);

        internal static void SetupCustomButtons(HudManager hm)
        {
            RocketSeizeButton = new(
                () =>
                {
                    // ここに能力のコードを記載する
                    PlayerControl target = HudManagerStartPatch.SetTarget(RoleData.LocalData, Crewmateonly: true);
                    if (target == null)
                        return;
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RocketSeize);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.RocketSeize(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);
                    RocketSeizeButton.MaxTimer = RoleData.RocketButtonAfterCooldown;
                    RocketSeizeButton.Timer = RoleData.RocketButtonAfterCooldown;
                    RocketRocketButton.MaxTimer = CustomOptionData.RocketChargeTime.GetFloat();
                    RocketRocketButton.Timer = RocketRocketButton.MaxTimer;
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Rocket; },
                () => { return PlayerControl.LocalPlayer.CanMove && HudManagerStartPatch.SetTarget(RoleData.LocalData, Crewmateonly: true); },
                () => { ResetRocketButtonCool(); },
                GetButtonSeizeSprite(),
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
                GetButtonRocketSprite(),
                new Vector3(-2f, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.Q,
                8,
                () => { return false; }
            )
            {
                buttonText = ModHelpers.IsSucsessChance(5) ? ModTranslation.GetString("RocketLetsButtonName") : ModTranslation.GetString("RocketLetsButtonName2"),
                showButtonText = true
            };
        }

        private static void ResetRocketButtonCool()
        {
            RocketSeizeButton.MaxTimer = RoleData.RocketButtonCooldown;
            RocketSeizeButton.Timer = RoleData.RocketButtonCooldown;
            RocketRocketButton.MaxTimer = CustomOptionData.RocketChargeTime.GetFloat();
            RocketRocketButton.Timer = 0;
        }
    }

    // ここにコードを書きこんでください
}