using AmongUs.GameOptions;
using System.Collections.Generic;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using static SuperNewRoles.Helpers.RPCHelper;

namespace SuperNewRoles.Roles;

class Madmate
{
    public static HashSet<byte> CheckedImpostor;
    /// <summary>
    /// MKされたマッドを保存する
    /// </summary>
    /// <value>
    /// true : 元がタスクをできる役職 / false : 元がタスクができない役職
    /// </value>
    public static PlayerData<bool> ChangeMadmatePlayer;

    public static void ClearAndReload()
    {
        CheckedImpostor = new();
        ChangeMadmatePlayer = new(defaultvalue: true);
    }

    public static bool CheckImpostor(PlayerControl p)
    {
        if (CheckedImpostor.Contains(p.PlayerId)) return true;
        if (p.GetRoleBase() is IMadmate imadmate)
        {
            bool canSee = imadmate.CanSeeImpostor(p);
            if (canSee)
                CheckedImpostor.Add(p.PlayerId);
            return canSee;
        }
        int CheckTask = 0;
        switch (p.GetRole())
        {
            case RoleId.Madmate:
                if (!RoleClass.Madmate.IsImpostorCheck) return false;
                bool haveTask = !(Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles) && !ChangeMadmatePlayer[p.PlayerId]);
                CheckTask = haveTask ? RoleClass.Madmate.ImpostorCheckTask : 0;
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
        List<RoleTypes> CanNotHaveTaskForRoles = new() { RoleTypes.Impostor, RoleTypes.Shapeshifter, RoleTypes.ImpostorGhost };
        // マッドメイトになる前にタスクを持っていたかを取得
        var canNotHaveTask = CanNotHaveTaskForRoles.Contains(target.Data.Role.Role);
        canNotHaveTask = CanNotHaveTaskForRoles.Contains(RoleSelectHandler.GetDesyncRole(target.GetRole()).RoleType);// Desync役職ならタスクを持っていなかったと見なす ( 個別設定 )
        if (target.GetRoleBase() is ISupportSHR supportSHR) { canNotHaveTask = CanNotHaveTaskForRoles.Contains(supportSHR.DesyncRole); } // Desync役職ならタスクを持っていなかったと見なす ( RoleBace )

        target.ResetAndSetRole(RoleId.Madmate);
        if (target.IsRole(RoleId.Madmate)) ChangeMadmatePlayer[target.PlayerId] = !canNotHaveTask;
    }
}