using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using UnityEngine;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.Roles;

internal abstract class ModifierBase<T> : BaseSingleton<T>, IModifierBase where T : ModifierBase<T>, new()
{
    protected int _optionidbase = -1;

    public abstract ModifierRoleId ModifierRole { get; }
    public string RoleName => ModifierRole.ToString();
    public virtual CustomOption[] Options => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.Options : [];
    public virtual int? PercentageOption => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.Percentage : null;
    public virtual int? NumberOfCrews => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.NumberOfCrews : null;
    public abstract Color32 RoleColor { get; }
    public abstract List<Func<AbilityBase>> Abilities { get; }
    public abstract QuoteMod QuoteMod { get; }
    public abstract List<AssignedTeamType> AssignedTeams { get; }
    public abstract WinnerTeamType WinnerTeam { get; }
    public abstract RoleTag[] RoleTags { get; }
    public abstract short IntroNum { get; }
    public abstract Func<ExPlayerControl, string> ModifierMark { get; }
    public virtual RoleId[] RelatedRoleIds { get; } = [];
    public virtual bool HiddenOption { get; } = false;
    public virtual bool AssignFilter { get; } = false;
    public virtual RoleId[] DoNotAssignRoles { get; } = [];
    public virtual bool UseTeamSpecificAssignment { get; } = false;
    public virtual int MaxImpostors => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.MaxImpostors : 0;
    public virtual int ImpostorChance => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.ImpostorChance : 100;
    public virtual int MaxNeutrals => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.MaxNeutrals : 0;
    public virtual int NeutralChance => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.NeutralChance : 100;
    public virtual int MaxCrewmates => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.MaxCrewmates : 0;
    public virtual int CrewmateChance => RoleOptionManager.TryGetModifierRoleOption(ModifierRole, out var role) ? role.CrewmateChance : 100;
}

public interface IModifierBase : IRoleInformation
{
    public ModifierRoleId ModifierRole { get; }
    /// <summary>
    /// 追加したいAbilityを[ typeof(HogeAbility), typeof(FugaAbility) ]の形でListとして用意する
    /// 役職選出時(OnSetRole)に自動でnewされます
    /// </summary>
    public List<Func<AbilityBase>> Abilities { get; }
    public QuoteMod QuoteMod { get; }
    public List<AssignedTeamType> AssignedTeams { get; }
    public WinnerTeamType WinnerTeam { get; }
    public RoleTag[] RoleTags { get; }
    public short IntroNum { get; }
    public bool IsVanillaRole => QuoteMod == QuoteMod.Vanilla;
    public Func<ExPlayerControl, string> ModifierMark { get; }
    public bool HiddenOption { get; }
    public bool AssignFilter { get; }
    public RoleId[] DoNotAssignRoles { get; }
    public RoleId[] RelatedRoleIds { get; }
    public bool UseTeamSpecificAssignment { get; }
    public int MaxImpostors { get; }
    public int ImpostorChance { get; }
    public int MaxNeutrals { get; }
    public int NeutralChance { get; }
    public int MaxCrewmates { get; }
    public int CrewmateChance { get; }

    /// <summary>
    /// AbilityはAbilitiesから自動でセットされるが、追加で他の処理を行いたい場合はOverrideすること
    /// </summary>
    public virtual void OnSetRole(PlayerControl player)
    {
        ExPlayerControl exPlayer = player;
        AbilityParentModifier parent = new(exPlayer, this);
        foreach (var abilityFactory in Abilities)
        {
            exPlayer.AddAbility(abilityFactory(), parent);
        }
    }
}