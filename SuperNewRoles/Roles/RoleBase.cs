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
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles;

internal abstract class RoleBase<T> : BaseSingleton<T>, IRoleBase where T : RoleBase<T>, new()
{
    protected int _optionidbase = -1;
    protected AudioClip _introsound;

    public abstract RoleId Role { get; }
    public abstract Color32 RoleColor { get; }
    public abstract List<Type> Abilities { get; }
    public abstract QuoteMod QuoteMod { get; }
    public abstract AssignedTeamType AssignedTeam { get; }
    public abstract WinnerTeamType WinnerTeam { get; }
    public abstract TeamTag TeamTag { get; }
    public abstract RoleTag[] RoleTags { get; }
    public abstract short IntroNum { get; }
    public virtual RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public virtual AudioClip CustomIntroSound { get; } = null;
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

    // public abstract void CreateCustomOption();
}

public interface IRoleBase
{
    public RoleId Role { get; }
    public Color32 RoleColor { get; }
    /// <summary>
    /// 追加したいAbilityを[ typeof(HogeAbility), typeof(FugaAbility) ]の形でListとして用意する
    /// 役職選出時(OnSetRole)に自動でnewされます
    /// </summary>
    public List<Type> Abilities { get; }
    public QuoteMod QuoteMod { get; }
    public AssignedTeamType AssignedTeam { get; }
    public WinnerTeamType WinnerTeam { get; }
    public TeamTag TeamTag { get; }
    public RoleTag[] RoleTags { get; }
    public short IntroNum { get; }
    /// <summary>
    /// CustomIntroSoundがnullの時に使用される
    /// </summary>
    public RoleTypes IntroSoundType { get; }
    /// <summary>
    /// こちらを設定するとIntroSoundTypeよりも優先される
    /// </summary>
    public AudioClip CustomIntroSound { get; }

    /// <summary>
    /// AbilityはAbilitiesから自動でセットされるが、追加で他の処理を行いたい場合はOverrideすること
    /// </summary>
    public virtual void OnSetRole(PlayerControl player)
    {
        foreach (Type ability in Abilities.AsSpan())
            if (ability.IsAssignableFrom(typeof(AbilityBase)))
                player.AddAbility((AbilityBase)Activator.CreateInstance(ability));
            else
                Logger.Warning($"{ability.ToString()}はAbilityではないです。", "OnSetRole");
    }
}