using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Modifiers;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class DisconnectedResultSaver
{
    public class DisconnectedData
    {
        public RoleId RoleId { get; set; }
        public ModifierRoleId ModifierRoleId { get; set; }
        public GhostRoleId GhostRoleId { get; set; }
        public List<RoleId> RoleHistory { get; set; }
        public List<GhostRoleId> GhostRoleHistory { get; set; }
        public List<ModifierRoleId> ModifierRoleHistory { get; set; }
        public List<string> ModifierMarks { get; set; }
        public (int completed, int total) Tasks { get; set; }
        public bool IsImpostor { get; set; }
        public bool IsDead { get; set; }
        public string Hat2Id { get; set; }
        public string Visor2Id { get; set; }
        public Color32? LoversHeartColor { get; set; }
        public DisconnectedData(ExPlayerControl player)
        {
            RoleId = player.Role;
            Tasks = ModHelpers.TaskCompletedData(player.Data);
            IsImpostor = player.IsImpostor();
            ModifierRoleId = player.ModifierRole;
            GhostRoleId = player.GhostRole;
            IsDead = player.IsDead();
            RoleHistory = new List<RoleId>(player.RoleHistory);
            GhostRoleHistory = new List<GhostRoleId>(player.GhostRoleHistory);
            ModifierRoleHistory = new List<ModifierRoleId>(player.ModifierRoleHistory);
            ModifierMarks = new();
            foreach (var modifier in player.ModifierRoleBases)
                ModifierMarks.Add(modifier.ModifierMark(player));
            CustomCosmeticsLayer customCosmeticsLayer = CustomCosmeticsLayers.ExistsOrInitialize(player.Player.cosmetics);
            Hat2Id = customCosmeticsLayer?.hat2?.Hat?.ProdId ?? "";
            Visor2Id = customCosmeticsLayer?.visor2?.Visor?.ProdId ?? "";
            LoversHeartColor = player.TryGetAbility<LoversAbility>(out var loversAbility) ? loversAbility.HeartColor : null;
        }
    }
    public static DisconnectedResultSaver Instance { get; private set; }
    public static Dictionary<byte, DisconnectedData> Disconnecteds { get; } = new();
    public static void Initialize()
    {
        Disconnecteds.Clear();
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
    public static void Prefix(PlayerControl player)
    {
        if (player == null || AmongUsClient.Instance == null ||
            AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started ||
            DisconnectedResultSaver.Instance == null)
        {
            return;
        }
        DisconnectedResultSaver.Instance.HandleDisconnect(player);
    }
}
