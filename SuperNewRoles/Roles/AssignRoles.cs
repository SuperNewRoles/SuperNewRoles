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
    [CustomOptionInt("AssignRoles_MaxImpostors", 1, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
    public static int MaxImpostors;
    [CustomOptionInt("AssignRoles_MaxCrews", 1, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
    public static int MaxCrews;
    [CustomOptionInt("AssignRoles_MaxNeutrals", 1, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
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