using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;

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
    private static Dictionary<AssignedTeamType, List<AssignTickets>> AssignTickets_HundredPercent = new();
    private static Dictionary<AssignedTeamType, List<AssignTickets>> AssignTickets_NotHundredPercent = new();
    public static void AssignCustomRoles()
    {
        CreateTickets();

        // Assign Impostors
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Impostor],
        AssignTickets_NotHundredPercent[AssignedTeamType.Impostor],
        true, 0);

        // Assign Neutral
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Neutral],
        AssignTickets_NotHundredPercent[AssignedTeamType.Neutral],
        false, 0);

        // Assign Crews
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Crewmate],
        AssignTickets_NotHundredPercent[AssignedTeamType.Crewmate],
        false, 0);

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
    }
}
public struct AssignTickets : IComparable<AssignTickets>
{
    public RoleOptionManager.RoleOption RoleOption { get; }
    public AssignTickets(RoleOptionManager.RoleOption roleOption)
    {
        RoleOption = roleOption;
    }
    public int CompareTo(AssignTickets other)
    {
        return RoleOption.RoleId.CompareTo(other.RoleOption.RoleId);
    }
    public override int GetHashCode()
    {
        return RoleOption.RoleId.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj is AssignTickets other)
        {
            return RoleOption.RoleId == other.RoleOption.RoleId;
        }
        return false;
    }
}