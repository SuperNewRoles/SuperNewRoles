using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
class RoleManagerSelectRolesPatch
{
    public static void Postfix(RoleManager __instance)
    {
        AssignRoles.AssignCustomRoles();
    }
}

public static class AssignRoles
{
    [CustomOptionInt("AssignRoles_MaxImpostors", 0, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
    public static int MaxImpostors;
    [CustomOptionInt("AssignRoles_MaxCrews", 0, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
    public static int MaxCrews;
    [CustomOptionInt("AssignRoles_MaxNeutrals", 0, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
    public static int MaxNeutrals;

    private static Dictionary<AssignedTeamType, List<AssignTickets>> AssignTickets_HundredPercent = new();
    private static Dictionary<AssignedTeamType, List<AssignTickets>> AssignTickets_NotHundredPercent = new();
    private static List<RoleId> AssignedRoleIds = new(); // 既にアサインされた役職のIDを追跡

    public static void AssignCustomRoles()
    {
        CreateTickets();
        AssignedRoleIds.Clear(); // 役職アサイン前にクリア

        // Assign Impostors
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Impostor],
        AssignTickets_NotHundredPercent[AssignedTeamType.Impostor],
        true, MaxImpostors);

        // Assign Neutral
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Neutral],
        AssignTickets_NotHundredPercent[AssignedTeamType.Neutral],
        false, MaxNeutrals);

        if (JackalFriends.JackalFriendsDontAssignIfJackalNotAssigned && !ExPlayerControl.ExPlayerControls.Any(player => player.IsJackalTeam()))
        {
            AssignTickets_HundredPercent[AssignedTeamType.Crewmate].RemoveAll(ticket => ticket.RoleOption.RoleId == RoleId.JackalFriends);
            AssignTickets_NotHundredPercent[AssignedTeamType.Crewmate].RemoveAll(ticket => ticket.RoleOption.RoleId == RoleId.JackalFriends);
        }

        // Assign Crews
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Crewmate],
        AssignTickets_NotHundredPercent[AssignedTeamType.Crewmate],
        false, MaxCrews);

        // Assign Modifiers
        AssignModifiers();
    }
    private static void CreateTickets()
    {
        foreach (var team in Enum.GetValues(typeof(AssignedTeamType)).Cast<AssignedTeamType>())
        {
            AssignTickets_HundredPercent[team] = new();
            AssignTickets_NotHundredPercent[team] = new();
        }
        foreach (var roleOption in RoleOptionManager.RoleOptions)
        {
            if (roleOption.NumberOfCrews <= 0)
                continue;
            if (roleOption.Percentage >= 100)
            {
                AssignTickets_HundredPercent[roleOption.AssignTeam].Add(new AssignTickets(roleOption));
            }
            else if (roleOption.Percentage > 0)
            {
                var ticket = new AssignTickets(roleOption);
                for (int i = 0; i < (roleOption.Percentage / 10); i++)
                {
                    AssignTickets_NotHundredPercent[roleOption.AssignTeam].Add(ticket);
                }
            }
        }
    }
    private static void AssignTickets(List<AssignTickets> tickets_hundred, List<AssignTickets> tickets_not_hundred, bool isImpostor, int maxBeans)
    {
        List<PlayerControl> targetPlayers = new();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Role.IsImpostor == isImpostor && (isImpostor || ((ExPlayerControl)player).Role is RoleId.Crewmate or RoleId.None))
                targetPlayers.Add(player);
        }
        if (targetPlayers.Count <= 0)
            return;

        // 100%チケットの割り当て処理
        while (tickets_hundred.Count > 0 && targetPlayers.Count > 0 && maxBeans > 0)
        {
            int ticketIndex = UnityEngine.Random.Range(0, tickets_hundred.Count);
            AssignTickets selectedTicket = tickets_hundred[ticketIndex];
            selectedTicket.IncrementRemainingAssignBeans();
            if (selectedTicket.RemainingAssignBeans <= 0)
                tickets_hundred.RemoveAt(ticketIndex);

            int playerIndex = UnityEngine.Random.Range(0, targetPlayers.Count);
            PlayerControl targetPlayer = targetPlayers[playerIndex];
            targetPlayers.RemoveAt(playerIndex);

            RoleId roleId = selectedTicket.RoleOption.RoleId;
            AssignRole(targetPlayer, roleId);
            AssignedRoleIds.Add(roleId); // アサインした役職を追跡
            maxBeans--;

            // 排他設定を再度適用（次のループのために）
            RoleOptionManager.ApplyExclusivitySettings(AssignedRoleIds, AssignTickets_NotHundredPercent.Values.ToArray(), AssignTickets_HundredPercent.Values.ToArray());
        }

        // 100%未満のチケットからランダムに選択して割り当てる
        while (tickets_not_hundred.Count > 0 && targetPlayers.Count > 0 && maxBeans > 0)
        {
            // 排他設定を適用
            RoleOptionManager.ApplyExclusivitySettings(AssignedRoleIds, AssignTickets_NotHundredPercent.Values.ToArray(), AssignTickets_HundredPercent.Values.ToArray());
            if (tickets_not_hundred.Count == 0)
                break;

            int ticketIndex = UnityEngine.Random.Range(0, tickets_not_hundred.Count);
            AssignTickets selectedTicket = tickets_not_hundred[ticketIndex];
            selectedTicket.IncrementRemainingAssignBeans();
            if (selectedTicket.RemainingAssignBeans <= 0)
                tickets_not_hundred.RemoveAll(x => x.RoleOption == selectedTicket.RoleOption);

            int playerIndex = targetPlayers.GetRandomIndex();
            PlayerControl targetPlayer = targetPlayers[playerIndex];
            targetPlayers.RemoveAt(playerIndex);

            RoleId roleId = selectedTicket.RoleOption.RoleId;
            AssignRole(targetPlayer, roleId);
            AssignedRoleIds.Add(roleId); // アサインした役職を追跡
            maxBeans--;

            // 排他設定を再度適用（次のループのために）
            RoleOptionManager.ApplyExclusivitySettings(AssignedRoleIds, AssignTickets_NotHundredPercent.Values.ToArray(), AssignTickets_HundredPercent.Values.ToArray());
        }
        foreach (var player in targetPlayers)
        {
            AssignRole(player, isImpostor ? RoleId.Impostor : RoleId.Crewmate);
        }
    }
    private static void AssignRole(PlayerControl player, RoleId roleId)
    {
        Logger.Info($"Assigning role {roleId} to player {player.PlayerId}");
        ((ExPlayerControl)player).RpcCustomSetRole(roleId);
    }
    private static void AssignModifiers()
    {
        Logger.Info("AssignModifiers() 開始: Modifierのアサイン処理を開始します。");
        var allPlayers = ExPlayerControl.ExPlayerControls;
        Logger.Info($"AssignModifiers: 全プレイヤー数 = {allPlayers.Count}");
        var allModifiers = CustomRoleManager.AllModifiers;
        Logger.Info($"AssignModifiers: 全Modifier数 = {allModifiers.Length}");

        foreach (var modifierBase in allModifiers)
        {
            var modifierRoleId = modifierBase.ModifierRole;
            Logger.Info($"AssignModifiers: Modifier処理開始 - ModifierRole = {modifierRoleId}");

            // モディファイアのオプションから確率を取得
            var modifierRoleOption = RoleOptionManager.ModifierRoleOptions.FirstOrDefault(x => x.ModifierRoleId == modifierRoleId);
            if (modifierRoleOption == null)
            {
                Logger.Info($"AssignModifiers: ModifierRoleOptionが見つからないため、ModifierRole {modifierRoleId} をスキップします。");
                continue;
            }
            if (modifierRoleOption.Percentage <= 0)
            {
                Logger.Info($"AssignModifiers: ModifierRoleOptionのパーセンテージが0以下のため、ModifierRole {modifierRoleId} をスキップします。");
                continue;
            }

            List<ExPlayerControl> targetPlayers = ExPlayerControl.ExPlayerControls
                .Where(x => modifierBase.AssignedTeams.Count <= 0 || modifierBase.AssignedTeams.Contains(x.roleBase.AssignedTeam))
                .ToList();
            Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} に適用可能なプレイヤー数 = {targetPlayers.Count}");

            for (int i = 0; i < modifierRoleOption.NumberOfCrews; i++)
            {
                Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - ループ {i + 1}/{modifierRoleOption.NumberOfCrews} 開始");
                int randomRoll = ModHelpers.GetRandomInt(0, 100);
                Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - ループ {i + 1} のランダム値 = {randomRoll} (閾値: {modifierRoleOption.Percentage})");
                if (randomRoll > modifierRoleOption.Percentage)
                {
                    Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - ループ {i + 1} はランダム判定により割り当てをスキップします。");
                    continue;
                }
                if (targetPlayers.Count == 0)
                {
                    Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - ループ {i + 1} で対象プレイヤーが存在しないため、ループを終了します。");
                    break;
                }
                int playerIndex = targetPlayers.GetRandomIndex();
                Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - 選択されたプレイヤーインデックス = {playerIndex}");
                PlayerControl targetPlayer = targetPlayers[playerIndex];
                Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - 選択されたプレイヤーID = {targetPlayer.PlayerId}");
                targetPlayers.RemoveAt(playerIndex);
                Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - プレイヤー {targetPlayer.PlayerId} を対象リストから削除しました。");
                Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - プレイヤー {targetPlayer.PlayerId} に対してModifierの割り当てを試みます。");
                AssignModifier(targetPlayer, modifierRoleId);
            }
            Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} の処理が完了しました。");
        }
        Logger.Info("AssignModifiers() 終了: 全てのModifier処理が完了しました。");
    }

    private static void AssignModifier(PlayerControl player, ModifierRoleId modifierRoleId)
    {
        Logger.Info($"AssignModifier: プレイヤー {player.PlayerId} に ModifierRole {modifierRoleId} の割り当て処理を開始します。");
        ExPlayerControl exPlayer = player;

        // 既存のモディファイアとフラグの状態を確認
        if (exPlayer.ModifierRole.HasFlag(modifierRoleId))
        {
            Logger.Info($"AssignModifier: プレイヤー {player.PlayerId} は既にModifierRole {modifierRoleId} を保持しているため、割り当てをスキップします。");
            return;
        }

        Logger.Info($"AssignModifier: プレイヤー {player.PlayerId} にModifierRole {modifierRoleId} を割り当てます。");
        ModifierRoleId newModifierRole = modifierRoleId;
        exPlayer.RpcCustomSetModifierRole(newModifierRole);
        Logger.Info($"AssignModifier: RPCを使用してプレイヤー {player.PlayerId} にModifierRole {modifierRoleId} を適用しました。");
    }
}
public struct AssignTickets
{
    public RoleOptionManager.RoleOption RoleOption { get; }
    public int RemainingAssignBeans { get; private set; }
    public AssignTickets(RoleOptionManager.RoleOption roleOption)
    {
        RoleOption = roleOption;
        RemainingAssignBeans = roleOption.NumberOfCrews;
    }
    public void IncrementRemainingAssignBeans()
    {
        RemainingAssignBeans--;
    }
}