using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

public class DisconnectedResultSaver
{
    public class DisconnectedData
    {
        public RoleId RoleId { get; set; }
        public ModifierRoleId ModifierRoleId { get; set; }
        public GhostRoleId GhostRoleId { get; set; }
        public (int completed, int total) Tasks { get; set; }
        public bool IsImpostor { get; set; }
        public bool IsDead { get; set; }
        public string Hat2Id { get; set; }
        public string Visor2Id { get; set; }
        public DisconnectedData(ExPlayerControl player)
        {
            RoleId = player.Role;
            Tasks = ModHelpers.TaskCompletedData(player.Data);
            IsImpostor = player.IsImpostor();
            ModifierRoleId = player.ModifierRole;
            GhostRoleId = player.GhostRole;
            IsDead = player.IsDead();
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.Player.cosmetics);
            Hat2Id = customCosmeticsLayer?.hat2?.Hat?.ProdId ?? "";
            Visor2Id = customCosmeticsLayer?.visor2?.Visor?.ProdId ?? "";
        }
    }
    public static DisconnectedResultSaver Instance { get; private set; }
    public static Dictionary<byte, DisconnectedData> Disconnecteds { get; } = new();
    public static void Initialize()
    {
        Instance = new();
    }
    public void HandleDisconnect(PlayerControl player)
    {
        if (player == null)
        {
            Logger.Error("Player is null");
            return;
        }
        ExPlayerControl exPlayer = ExPlayerControl.ById(player.PlayerId);
        if (exPlayer == null)
        {
            Logger.Error($"Player {player.PlayerId} not found");
            return;
        }
        Disconnecteds[player.PlayerId] = new DisconnectedData(exPlayer);
    }
    public DisconnectedData GetDisconnectedData(byte playerId)
    {
        return Disconnecteds.GetValueOrDefault(playerId);
    }
}
[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
public static class DisconnectedResultSaverCoStartGamePatch
{
    public static void Postfix(AmongUsClient __instance)
    {
        DisconnectedResultSaver.Initialize();
    }
}
[HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), [typeof(PlayerControl), typeof(DisconnectReasons)])]
public static class DisconnectedResultSaverHandleDisconnectPatch
{
    public static void Postfix(GameData __instance, PlayerControl player, DisconnectReasons reason)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        DisconnectedResultSaver.Instance.HandleDisconnect(player);
    }
}