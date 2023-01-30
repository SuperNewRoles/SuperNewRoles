using System;
using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class DyingMessenger
{
    private const int OptionId = 1171;
    public static CustomRoleOption DyingMessengerOption;
    public static CustomOption DyingMessengerPlayerCount;
    public static CustomOption DyingMessengerGetRoleTime;
    public static CustomOption DyingMessengerGetLightAndDarkerTime;
    public static void SetupCustomOptions()
    {
        DyingMessengerOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.DyingMessenger);
        DyingMessengerPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], DyingMessengerOption);
        DyingMessengerGetRoleTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "DyingMessengerGetRoleTimeSetting", 20f, 0f, 60f, 1f, DyingMessengerOption);
        DyingMessengerGetLightAndDarkerTime = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "DyingMessengerGetLightAndDarkerTimeSetting", 2f, 0f, 60f, 1f, DyingMessengerOption);
    }

    public static List<PlayerControl> DyingMessengerPlayer;
    public static Color32 color = new(191, 197, 202, byte.MaxValue);
    public static Dictionary<byte, (DateTime, PlayerControl)> KillTime;
    public static void ClearAndReload()
    {
        DyingMessengerPlayer = new();
        KillTime = new();
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public class MeetingHudUpdatePatch
    {
        public static void Postfix()
        {
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea player in MeetingHud.Instance.playerStates)
                {
                    PlayerControl target = null;
                    PlayerControl.AllPlayerControls.ToList().ForEach(x =>
                    {
                        string name = player.NameText.text.Replace(GetLightAndDarkerText(true), "").Replace(GetLightAndDarkerText(false), "");
                        if (name == x.Data.PlayerName) target = x;
                    });
                    if (target != null)
                    {
                        if (ConfigRoles.IsLightAndDarker.Value)
                        {
                            if (player.NameText.text.Contains(GetLightAndDarkerText(true)) ||
                                player.NameText.text.Contains(GetLightAndDarkerText(false))) continue;
                            player.NameText.text += GetLightAndDarkerText(CustomColors.lighterColors.Contains(target.Data.DefaultOutfit.ColorId));
                        }
                        else player.NameText.text = player.NameText.text.Replace(GetLightAndDarkerText(true), "").Replace(GetLightAndDarkerText(false), "");
                    }
                    else Logger.Error($"プレイヤーコントロールを取得できませんでした。 プレイヤー名 : {player.NameText.text}", "LightAndDarkerText");
                }
            }
        }
        public static string GetLightAndDarkerText(bool isLight) => $" ({(isLight ? ModTranslation.GetString("LightColor") : ModTranslation.GetString("DarkerColor"))[0]})";
    }
}
