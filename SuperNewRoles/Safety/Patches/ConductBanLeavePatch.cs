using HarmonyLib;

namespace SuperNewRoles.Safety.Patches;

[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), [typeof(PlayerControl), typeof(DisconnectReasons)])]
public static class ConductBanLeaveReasonPatch
{
    public static void Prefix(PlayerControl player)
    {
        if (player == null)
            return;
        InnerNet.ClientData client = AmongUsClient.Instance?.GetClientFromCharacter(player);
        int ownerId = player.OwnerId;
        int clientId = client != null ? client.Id : ownerId;
        bool conductLeave = ConductBanLeaveNotice.TryConsume(clientId) || ConductBanLeaveNotice.TryConsume(ownerId);
        bool hostBlockLeave = HostBlockLeaveNotice.TryConsume(clientId) || HostBlockLeaveNotice.TryConsume(ownerId);
        if (!conductLeave && !hostBlockLeave)
            return;
        string name = player.Data?.PlayerName;
        if (string.IsNullOrEmpty(name))
            name = ConductBanLeaveNotice.FindPlayerName(clientId);
        if (conductLeave)
            ConductBanLeaveNotice.RememberName(name);
        if (hostBlockLeave)
            HostBlockLeaveNotice.RememberName(name);
    }
}

[HarmonyPatch(typeof(NotificationPopper), nameof(NotificationPopper.AddDisconnectMessage))]
public static class ConductBanLeaveSuppressPatch
{
    public static bool Prefix(string item)
    {
        return !ConductBanLeaveNotice.ShouldSuppressVanilla(item)
            && !HostBlockLeaveNotice.ShouldSuppressVanilla(item);
    }
}
