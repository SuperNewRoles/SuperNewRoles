using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles;
public static class HideChat
{
    public static bool HideChatEnabled = true;
    private static bool SerializeByHideChat = false;
    public static bool CanSerializeGameData => !HideChatEnabled || SerializeByHideChat || !RoleClass.IsMeeting;
    public static void OnStartMeeting()
    {
        _ = new LateTask(() => DesyncSetDead(), 4.5f);
        _ = new LateTask(() => DesyncSetDead(), 6.5f);
    }
    public static void DesyncSetDead(CustomRpcSender? sender, NetworkedPlayerInfo target)
    {
        AliveState State = new(target);
        target.IsDead = true;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player == null ||
                player.Data.IsDead ||
                player.Data.Disconnected)
                continue;
            if (player.PlayerId == target.PlayerId)
                continue;
            if (player.IsMod()) continue;

            Logger.Info($"HideChat:SetDead: {target.PlayerId} => {player.PlayerId}");

            // シリアライズ
            SerializeByHideChat = true;
            RPCHelper.RpcSyncNetworkedPlayer(sender, target, player.GetClientId());
            SerializeByHideChat = false;
        }
        target.IsDead = State.IsDead;
        target.Disconnected = State.Disconnected;
    }
    private static void DesyncSetDead()
    {
        PlayerData<AliveState> AliveStates = new();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            AliveStates[player.PlayerId] = new(player.Data);
        }
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (AliveStates[player] == null ||
                AliveStates[player].IsDead ||
                AliveStates[player].Disconnected)
                continue;
            if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
            if (player.IsMod()) continue;
            foreach (PlayerControl player2 in PlayerControl.AllPlayerControls)
            {
                player2.Data.IsDead = true;
            }
            player.Data.IsDead = false;

            // シリアライズ
            SerializeByHideChat = true;
            RPCHelper.RpcSyncAllNetworkedPlayer(player.GetClientId());
            SerializeByHideChat = false;
        }
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            player.Data.IsDead = AliveStates[player.PlayerId].IsDead;
            player.Data.Disconnected = AliveStates[player.PlayerId].Disconnected;
        }
    }
    public static void OnAddChat(PlayerControl player, string message, bool isAdd)
    {
        if (!RoleClass.IsMeeting || !HideChatEnabled || player.IsDead() || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            return;
        CustomRpcSender sender = new("HideChatSender", sendOption: Hazel.SendOption.Reliable, false);
        OnAddChat(sender, player, message, isAdd);
        sender.SendMessage();
    }
    public static void OnAddChat(CustomRpcSender sender, PlayerControl player, string message, bool isAdd)
    {
        if (!RoleClass.IsMeeting || !HideChatEnabled || player.IsDead() || AntiBlackOut.GamePlayers != null)
            return;
        if (isAdd)
        {
            bool playerIsDead = player.Data.IsDead;
            player.Data.IsDead = false;
            SerializeByHideChat = true;
            RPCHelper.RpcSyncNetworkedPlayer(sender, player.Data);
            SerializeByHideChat = false;
            foreach (PlayerControl target in PlayerControl.AllPlayerControls)
            {
                if (player == target || target.IsDead())
                    continue;
                if (target.PlayerId == 0)
                    continue;
                if (target.IsMod())
                    continue;
                player.RPCSendChatPrivate(message, target, sender);
                sender.EndMessage();
            }
            player.Data.IsDead = playerIsDead;
        }
        DesyncSetDead(sender, player?.Data);
    }
}
public class AliveState
{
    public bool IsDead { get; }
    public bool Disconnected { get;}
    public AliveState(NetworkedPlayerInfo player)
    {
        IsDead = player.IsDead;
        Disconnected = player.Disconnected;
    }
}