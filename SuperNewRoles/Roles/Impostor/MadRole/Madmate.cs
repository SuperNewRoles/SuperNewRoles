using System.Collections.Generic;
using SuperNewRoles.Patches;
using static SuperNewRoles.Helpers.RPCHelper;

namespace SuperNewRoles.Roles;

class Madmate
{
    public static Dictionary<byte, RoleId> CheckedImpostor;
    public static bool CheckImpostor(PlayerControl p)
    {
        if (CheckedImpostor.ContainsKey(p.PlayerId) && p.GetRole() == CheckedImpostor[p.PlayerId]) return true;
        if (CheckedImpostor.ContainsKey(p.PlayerId) && p.GetRole() != CheckedImpostor[p.PlayerId])
            CheckedImpostor.Remove(p.PlayerId);

        int CheckTask = 0;
        switch (p.GetRole())
        {
            case RoleId.Madmate:
                if (!RoleClass.Madmate.IsImpostorCheck) return false;
                CheckTask = RoleClass.Madmate.ImpostorCheckTask;
                break;
            case RoleId.MadMayor:
                if (!RoleClass.MadMayor.IsImpostorCheck) return false;
                CheckTask = RoleClass.MadMayor.ImpostorCheckTask;
                break;
            case RoleId.MadJester:
                if (!RoleClass.MadJester.IsImpostorCheck) return false;
                CheckTask = RoleClass.MadJester.ImpostorCheckTask;
                break;
            case RoleId.MadSeer:
                if (!RoleClass.MadSeer.IsImpostorCheck) return false;
                CheckTask = RoleClass.MadSeer.ImpostorCheckTask;
                break;
            case RoleId.BlackCat:
                if (!RoleClass.BlackCat.IsImpostorCheck) return false;
                CheckTask = RoleClass.BlackCat.ImpostorCheckTask;
                break;
            case RoleId.Worshiper:
                if (!Roles.Impostor.MadRole.Worshiper.IsImpostorCheck) return false;
                CheckTask = Roles.Impostor.MadRole.Worshiper.ImpostorCheckTask;
                break;
            default:
                return false;
        }
        var taskdata = TaskCount.TaskDate(p.Data).Item1;
        if (CheckTask <= taskdata)
        {
            CheckedImpostor.Add(p.PlayerId, p.GetRole());
            return true;
        }
        return false;
    }
    /// <summary>
    /// (役職をリセットし、)マッドメイトに割り当てます。
    /// </summary>
    /// <param name="target">役職がMadmateに変更される対象</param>
    public static void CreateMadmate(PlayerControl target)
    {
        target.ResetAndSetRole(RoleId.Madmate);
    }
}