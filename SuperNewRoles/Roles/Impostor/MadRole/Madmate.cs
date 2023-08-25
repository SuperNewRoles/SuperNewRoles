using System.Collections.Generic;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Impostor.MadRole;
using static SuperNewRoles.Helpers.RPCHelper;

namespace SuperNewRoles.Roles;

class Madmate
{
    public static List<byte> CheckedImpostor;
    public static bool CheckImpostor(PlayerControl p)
    {
        if (CheckedImpostor.Contains(p.PlayerId)) return true;
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
            case RoleId.MadStuntMan:
                if (!RoleClass.MadStuntMan.IsImpostorCheck) return false;
                CheckTask = RoleClass.MadStuntMan.ImpostorCheckTask;
                break;
            case RoleId.MadHawk:
                if (!RoleClass.MadHawk.IsImpostorCheck) return false;
                CheckTask = RoleClass.MadHawk.ImpostorCheckTask;
                break;
            case RoleId.MadJester:
                if (!RoleClass.MadJester.IsImpostorCheck) return false;
                CheckTask = RoleClass.MadJester.ImpostorCheckTask;
                break;
            case RoleId.MadSeer:
                if (!RoleClass.MadSeer.IsImpostorCheck) return false;
                CheckTask = RoleClass.MadSeer.ImpostorCheckTask;
                break;
            case RoleId.MadCleaner:
                if (!RoleClass.MadCleaner.IsImpostorCheck) return false;
                CheckTask = RoleClass.MadCleaner.ImpostorCheckTask;
                break;
            case RoleId.BlackCat:
                if (!RoleClass.BlackCat.IsImpostorCheck) return false;
                CheckTask = RoleClass.BlackCat.ImpostorCheckTask;
                break;
            case RoleId.Worshiper:
                if (!Worshiper.RoleData.IsImpostorCheck) return false;
                CheckTask = Worshiper.RoleData.ImpostorCheckTask;
                break;
            case RoleId.MadRaccoon:
                if (!MadRaccoon.RoleData.IsImpostorCheck) return false;
                CheckTask = MadRaccoon.RoleData.ImpostorCheckTask;
                break;
            default:
                return false;
        }
        var taskdata = TaskCount.TaskDate(p.Data).Item1;
        if (CheckTask <= taskdata)
        {
            CheckedImpostor.Add(p.PlayerId);
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