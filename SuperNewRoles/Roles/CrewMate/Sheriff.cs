using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Crewmate;

class Sheriff : RoleBase<Sheriff>
{
    public override RoleId Role { get; } = RoleId.Sheriff;
    public override Color32 RoleColor { get; } = new(255, 255, 0, byte.MaxValue); // 黄色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new SheriffAbility(new SheriffAbilityData(
        killCooldown: SheriffKillCooldown,
        killCount: SheriffMaxKillCount,
        canKillNeutral: SheriffCanKillNeutral,
        canKillImpostor: SheriffCanKillImpostor,
        canKillMadRoles: SheriffCanKillMadRoles,
        canKillFriendRoles: SheriffCanKillFriendRoles,
        canKillLovers: SheriffCanKillLovers))];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
    [CustomOptionFloat("SheriffKillCooldown", 0f, 60f, 2.5f, 25f)]
    public static float SheriffKillCooldown;

    [CustomOptionInt("SheriffMaxKillCount", 1, 10, 1, 1)]
    public static int SheriffMaxKillCount;

    [CustomOptionBool("SheriffCanKillImpostor", true)]
    public static bool SheriffCanKillImpostor;

    [CustomOptionBool("SheriffCanKillMadRoles", true)]
    public static bool SheriffCanKillMadRoles;

    [CustomOptionBool("SheriffCanKillNeutral", true)]
    public static bool SheriffCanKillNeutral;

    [CustomOptionBool("SheriffCanKillFriendRoles", true)]
    public static bool SheriffCanKillFriendRoles;

    [CustomOptionBool("SheriffCanKillLovers", true)]
    public static bool SheriffCanKillLovers;
}
public class SheriffAbilityData
{
    public float KillCooldown { get; set; }
    public int KillCount { get; set; }
    public bool CanKillNeutral { get; set; }
    public bool CanKillImpostor { get; set; }
    public bool CanKillMadRoles { get; set; }
    public bool CanKillFriendRoles { get; set; }
    public bool CanKillLovers { get; set; }
    public SheriffAbilityData(float killCooldown, int killCount, bool canKillNeutral, bool canKillImpostor, bool canKillMadRoles, bool canKillFriendRoles, bool canKillLovers)
    {
        KillCooldown = killCooldown;
        KillCount = killCount;
        CanKillNeutral = canKillNeutral;
        CanKillImpostor = canKillImpostor;
        CanKillMadRoles = canKillMadRoles;
        CanKillFriendRoles = canKillFriendRoles;
        CanKillLovers = canKillLovers;
    }
    public bool CanKill(PlayerControl killer, ExPlayerControl target)
    {
        if (target.IsImpostor())
            return CanKillImpostor;
        else if (target.IsNeutral())
            return CanKillNeutral;
        else if (target.IsMadRoles())
            return CanKillMadRoles;
        else if (target.IsFriendRoles())
            return CanKillFriendRoles;
        else if (target.IsLovers())
            return CanKillLovers;
        return false;
    }
}

public class SheriffAbility : CustomKillButtonAbility, IAbilityCount
{
    public SheriffAbilityData SheriffAbilityData { get; set; }
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SheriffKillButton.png");
    public override float DefaultTimer => SheriffAbilityData.KillCooldown;
    public override ShowTextType showTextType => ShowTextType.ShowWithCount;
    public SheriffAbility(SheriffAbilityData sheriffAbilityData) : base(
        canKill: () => sheriffAbilityData.KillCount > 0,
        killCooldown: () => sheriffAbilityData.KillCooldown,
        onlyCrewmates: () => false,
        targetPlayersInVents: () => false)
    {
        SheriffAbilityData = sheriffAbilityData;
        Count = SheriffAbilityData.KillCount;
    }

    public override bool CheckHasButton() => base.CheckHasButton() && HasCount;

    public override void OnClick()
    {
        if (Target == null) return;
        if (!CanKill()) return;

        this.UseAbilityCount();

        if (SheriffAbilityData.CanKill(PlayerControl.LocalPlayer, Target))
        {
            // 正当なキル
            ExPlayerControl.LocalPlayer.RpcCustomDeath(Target, CustomDeathType.Kill);
            FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.SheriffKill);
        }
        else
        {
            // 誤射の場合は自分が死ぬ
            ExPlayerControl.LocalPlayer.RpcCustomDeath(ExPlayerControl.LocalPlayer, CustomDeathType.Kill);
            FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.SheriffSelfDeath);
        }
        ResetTimer();
    }
}
