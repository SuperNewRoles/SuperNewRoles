using HarmonyLib;
using InnerNet;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    class BanBlockedPlayerPatch
    {
        //TOHより、ありがとうございます
        public static void Postfix([HarmonyArgument(0)] ClientData client)
        {
            SuperNewRolesPlugin.Logger.LogInfo($"{client.PlayerName}(ClientID:{client.Id})が参加");
            if (DestroyableSingleton<FriendsListManager>.Instance.IsPlayerBlockedUsername(client.FriendCode) && AmongUsClient.Instance.AmHost)
            {
                AmongUsClient.Instance.KickPlayer(client.Id, true);
                SuperNewRolesPlugin.Logger.LogInfo($"ブロックされているプレイヤー{client?.PlayerName}({client.FriendCode})をBANしました");
            }
        }
    }
}