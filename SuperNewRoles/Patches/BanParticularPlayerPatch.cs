using HarmonyLib;
using InnerNet;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.Patches;

public static class PlayerKickHelper
{
    public static bool KickPlayerIfNeeded(ClientData client, bool kickPC, bool kickAndroid, bool kickOther)
    {
        if (client == null || client.PlatformData == null) return false;
        if (AmongUsClient.Instance.ClientId == client.Id) return false;

        var pf = client.PlatformData.Platform;

        if (kickPC && (pf == Platforms.StandaloneSteamPC || pf == Platforms.StandaloneEpicPC || pf == Platforms.StandaloneWin10))
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);
            SuperNewRolesPlugin.Logger.LogInfo($"PCプレイヤー {client.PlayerName} をキックしました");
            return true;
        }
        if (kickAndroid && pf == Platforms.Android)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);
            SuperNewRolesPlugin.Logger.LogInfo($"Androidプレイヤー {client.PlayerName} をキックしました");
            return true;
        }
        if (kickOther && pf != Platforms.StandaloneSteamPC && pf != Platforms.StandaloneEpicPC && pf != Platforms.StandaloneWin10 && pf != Platforms.Android)
        {
            AmongUsClient.Instance.KickPlayer(client.Id, false);
            SuperNewRolesPlugin.Logger.LogInfo($"その他プラットフォームのプレイヤー {client.PlayerName} をキックしました");
            return true;
        }
        return false;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
class BanParticularPlayerPatch
{
    public static void Postfix([HarmonyArgument(0)] ClientData client)
    {
        SuperNewRolesPlugin.Logger.LogInfo($"{client.PlayerName}(ClientID:{client.Id})が参加");

        if (!AmongUsClient.Instance.AmHost)
            return;

        // プラットフォームチェック
        if (GeneralSettingOptions.KickPlatformPlayers && PlayerKickHelper.KickPlayerIfNeeded(client,
                                                   GeneralSettingOptions.KickPCPlayers,
                                                   GeneralSettingOptions.KickAndroidPlayers,
                                                   GeneralSettingOptions.KickOtherPlayers))
            return; // キックされた場合は以降の処理をスキップ

        // フレンドコードのチェック（フレンドコードを持っていない人をBANする）
        if (GeneralSettingOptions.BanNoFriendCodePlayers && (string.IsNullOrEmpty(client.FriendCode) || !client.FriendCode.Contains("#")))
        {
            AmongUsClient.Instance.KickPlayer(client.Id, true);
            SuperNewRolesPlugin.Logger.LogInfo($"フレンドコードを持っていないプレイヤー {client?.PlayerName} をBANしました");
            return;
        }
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
class BanPlatformPlayersOnGameStartPatch
{
    private static bool lastKickPlatformPlayersEnabled;
    private static bool lastKickPCPlayers;
    private static bool lastKickAndroidPlayers;
    private static bool lastKickOtherPlayers;

    public static void Postfix(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;

        bool currentKickPlatformPlayersEnabled = GeneralSettingOptions.KickPlatformPlayers;
        bool currentKickPCPlayers = GeneralSettingOptions.KickPCPlayers;
        bool currentKickAndroidPlayers = GeneralSettingOptions.KickAndroidPlayers;
        bool currentKickOtherPlayers = GeneralSettingOptions.KickOtherPlayers;

        if (currentKickPlatformPlayersEnabled != lastKickPlatformPlayersEnabled ||
            currentKickPCPlayers != lastKickPCPlayers ||
            currentKickAndroidPlayers != lastKickAndroidPlayers ||
            currentKickOtherPlayers != lastKickOtherPlayers)
        {
            lastKickPlatformPlayersEnabled = currentKickPlatformPlayersEnabled;
            lastKickPCPlayers = currentKickPCPlayers;
            lastKickAndroidPlayers = currentKickAndroidPlayers;
            lastKickOtherPlayers = currentKickOtherPlayers;

            if (!currentKickPlatformPlayersEnabled) return;

            var clients = AmongUsClient.Instance.allClients;

            foreach (var client in clients)
            {
                PlayerKickHelper.KickPlayerIfNeeded(client, currentKickPCPlayers, currentKickAndroidPlayers, currentKickOtherPlayers);
            }
        }
    }
}