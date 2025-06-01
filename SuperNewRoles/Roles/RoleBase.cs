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

internal abstract class RoleBase<T> : BaseSingleton<T>, IRoleBase where T : RoleBase<T>, new()
{
    protected int _optionidbase = -1;
    protected AudioClip _introsound;

    public abstract RoleId Role { get; }
    public abstract Color32 RoleColor { get; }
    public virtual string RoleName => Role.ToString();
    public virtual bool HiddenOption => OptionTeam == RoleOptionMenuType.Hidden;
    public virtual List<AssignedTeamType> AssignedTeams => [AssignedTeam];
    public virtual CustomOption[] Options => RoleOptionManager.TryGetRoleOption(Role, out var role) ? role.Options : [];
    public virtual int? PercentageOption => RoleOptionManager.TryGetRoleOption(Role, out var role) ? role.Percentage : null;
    public virtual int? NumberOfCrews => RoleOptionManager.TryGetRoleOption(Role, out var role) ? role.NumberOfCrews : null;
    public abstract List<Func<AbilityBase>> Abilities { get; }
    public abstract QuoteMod QuoteMod { get; }
    public abstract AssignedTeamType AssignedTeam { get; }
    public abstract WinnerTeamType WinnerTeam { get; }
    public abstract TeamTag TeamTag { get; }
    public abstract RoleTag[] RoleTags { get; }
    public abstract short IntroNum { get; }
    public virtual RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public virtual AudioClip CustomIntroSound { get; } = null;
    public virtual RoleId[] RelatedRoleIds { get; } = [];
    public AudioClip IntroSound
    {
        get
        {
            if (CustomIntroSound != null) return CustomIntroSound;
            if (_introsound == null) _introsound = RoleManager.Instance.GetRole(IntroSoundType)?.IntroSound;
            return _introsound;
        }
    }
    public int OptionIdBase
    {
        get
        {
            if (_optionidbase < 0) _optionidbase = (int)(AssignedTeam + 1) * 100000 + (int)Role * 100;
            return _optionidbase;
        }
    }

    public abstract RoleOptionMenuType OptionTeam { get; }
    public virtual MapNames[] AvailableMaps { get; } = [];

    // public abstract void CreateCustomOption();
}

public interface IRoleBase : IRoleInformation
{
    public RoleId Role { get; }
    /// <summary>
    /// 追加したいAbilityを[ typeof(HogeAbility), typeof(FugaAbility) ]の形でListとして用意する
    /// 役職選出時(OnSetRole)に自動でnewされます
    /// </summary>
    public List<Func<AbilityBase>> Abilities { get; }
    public QuoteMod QuoteMod { get; }
    public AssignedTeamType AssignedTeam { get; }
    public WinnerTeamType WinnerTeam { get; }
    public TeamTag TeamTag { get; }
    public RoleTag[] RoleTags { get; }
    public RoleOptionMenuType OptionTeam { get; }
    public short IntroNum { get; }
    public bool IsVanillaRole => QuoteMod == QuoteMod.Vanilla;
    /// <summary>
    /// CustomIntroSoundがnullの時に使用される
    /// </summary>
    public RoleTypes IntroSoundType { get; }
    /// <summary>
    /// こちらを設定するとIntroSoundTypeよりも優先される
    /// </summary>
    public AudioClip CustomIntroSound { get; }

    public RoleId[] RelatedRoleIds { get; }
    public MapNames[] AvailableMaps { get; }
    /// <summary>
    /// AbilityはAbilitiesから自動でセットされるが、追加で他の処理を行いたい場合はOverrideすること
    /// </summary>
    public virtual void OnSetRole(PlayerControl player)
    {
        ExPlayerControl exPlayer = player;
        AbilityParentRole parent = new(exPlayer, this);
        foreach (var abilityFactory in Abilities)
        {
            exPlayer.AddAbility(abilityFactory(), parent);
        }
    }

    public static ulong GenerateAbilityId(byte playerId, RoleId role, int abilityIndex)
    {
        return (ulong)(playerId * 1000000) + (ulong)((int)role * 1000) + (ulong)abilityIndex;
    }
}

public interface IRoleInformation
{
    public string RoleName { get; }
    public Color32 RoleColor { get; }
    public bool HiddenOption { get; }
    public List<AssignedTeamType> AssignedTeams { get; }
    public CustomOption[] Options { get; }
    public int? PercentageOption { get; }
    public int? NumberOfCrews { get; }
}