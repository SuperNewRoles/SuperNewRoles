using HarmonyLib;
using InnerNet;
using SuperNewRoles.Modules;
using System.Linq;
using System;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
class BanParticularPlayerPatch
{
    public static void Postfix([HarmonyArgument(0)] ClientData client)
    {
        SuperNewRolesPlugin.Logger.LogInfo($"{client.PlayerName}(ClientID:{client.Id})が参加");

        if (!AmongUsClient.Instance.AmHost)
            return;

        // プラットフォームのチェック（PC以外をキックする）
        if (GeneralSettingOptions.KickNonPCPlayers && client.PlatformData.Platform != Platforms.StandaloneEpicPC && client.PlatformData.Platform != Platforms.StandaloneSteamPC)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);
            SuperNewRolesPlugin.Logger.LogInfo($"PC以外のプレイヤー {client?.PlayerName} をキックしました");
            return;
        }

        // フレンドコードのチェック（フレンドコードを持っていない人をBANする）
        if (GeneralSettingOptions.BanNoFriendCodePlayers && (string.IsNullOrEmpty(client.FriendCode) || !client.FriendCode.Contains("#")))
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            SuperNewRolesPlugin.Logger.LogInfo($"フレンドコードを持っていないプレイヤー {client?.PlayerName} をBANしました");
            return;
        }
    }
}