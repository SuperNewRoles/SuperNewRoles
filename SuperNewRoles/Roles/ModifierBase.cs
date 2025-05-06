using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using UnityEngine.Networking.Types;
using UnityEngine;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.CustomOptions;

namespace SuperNewRoles.Roles;

internal abstract class ModifierBase<T> : BaseSingleton<T>, IModifierBase where T : ModifierBase<T>, new()
{
    protected int _optionidbase = -1;

    public abstract ModifierRoleId ModifierRole { get; }
    public abstract Color32 RoleColor { get; }
    public abstract List<Func<AbilityBase>> Abilities { get; }
    public abstract QuoteMod QuoteMod { get; }
    public abstract List<AssignedTeamType> AssignedTeams { get; }
    public abstract WinnerTeamType WinnerTeam { get; }
    public abstract TeamTag TeamTag { get; }
    public abstract RoleTag[] RoleTags { get; }
    public abstract short IntroNum { get; }
    public abstract Func<ExPlayerControl, string> ModifierMark { get; }
    public virtual RoleId[] RelatedRoleIds { get; } = [];
    public virtual bool HiddenOption { get; } = false;
    public int OptionIdBase
    {
        get
        {
            if (_optionidbase < 0) _optionidbase = (int)(AssignedTeams.FirstOrDefault() + 1) * 100000 + (int)ModifierRole * 100 + 5000000;
            return _optionidbase;
        }
    }
}

public interface IModifierBase
{
    public ModifierRoleId ModifierRole { get; }
    public Color32 RoleColor { get; }
    /// <summary>
    /// 追加したいAbilityを[ typeof(HogeAbility), typeof(FugaAbility) ]の形でListとして用意する
    /// 役職選出時(OnSetRole)に自動でnewされます
    /// </summary>
    public List<Func<AbilityBase>> Abilities { get; }
    public QuoteMod QuoteMod { get; }
    public List<AssignedTeamType> AssignedTeams { get; }
    public WinnerTeamType WinnerTeam { get; }
    public TeamTag TeamTag { get; }
    public RoleTag[] RoleTags { get; }
    public short IntroNum { get; }
    public bool IsVanillaRole => QuoteMod == QuoteMod.Vanilla;
    public Func<ExPlayerControl, string> ModifierMark { get; }
    public bool HiddenOption { get; }

    public RoleId[] RelatedRoleIds { get; }

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