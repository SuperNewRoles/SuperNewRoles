using HarmonyLib;
using InnerNet;

namespace SuperNewRoles.Patches;

/// <summary>
/// AU MCI の Mod GUID は公式オンライン向けでローカルに参加できなくなるため、
/// OnlineGame のときだけ登録し、それ以外は外す。
/// </summary>
public static class ModRegistrationPatch
{
    public static bool ShouldUseModGuid()
    {
        return AmongUsClient.Instance != null
            && AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;
    }

    public static void ApplyForCurrentNetworkMode()
    {
        CurrentModRegistration.ModRegistrationGuidString = ShouldUseModGuid()
            ? PluginConfig.ModGuid
            : string.Empty;
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HostGame))]
    public static class InnerNetClientHostGamePatch
    {
        public static void Prefix()
        {
            ApplyForCurrentNetworkMode();
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
    public static class InnerNetClientJoinGamePatch
    {
        public static void Prefix()
        {
            ApplyForCurrentNetworkMode();
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.RequestGameList))]
    public static class InnerNetClientRequestGameListPatch
    {
        public static void Prefix()
        {
            ApplyForCurrentNetworkMode();
        }
    }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.GetConnectionData))]
    public static class InnerNetClientGetConnectionDataPatch
    {
        public static void Prefix()
        {
            ApplyForCurrentNetworkMode();
        }
    }

    [HarmonyPatch(typeof(InnerNetServer), nameof(InnerNetServer.StartAsLocalServer))]
    public static class InnerNetServerStartAsLocalServerPatch
    {
        public static void Prefix()
        {
            CurrentModRegistration.ModRegistrationGuidString = string.Empty;
        }
    }
}
