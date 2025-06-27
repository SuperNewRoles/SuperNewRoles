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
        mode: SheriffSuicideMode,
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

    [CustomOptionSelect("Sheriff.SuicideMode", typeof(SheriffSuicideMode), "Sheriff.SuicideMode.")]
    public static SheriffSuicideMode SheriffSuicideMode = SheriffSuicideMode.Default;

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
    public SheriffSuicideMode Mode { get; set; }
    public bool CanKillNeutral { get; set; }
    public bool CanKillImpostor { get; set; }
    public bool CanKillMadRoles { get; set; }
    public bool CanKillFriendRoles { get; set; }
    public bool CanKillLovers { get; set; }
    public SheriffAbilityData(float killCooldown, int killCount, SheriffSuicideMode mode, bool canKillNeutral, bool canKillImpostor, bool canKillMadRoles, bool canKillFriendRoles, bool canKillLovers)
    {
        KillCooldown = killCooldown;
        KillCount = killCount;
        Mode = mode;

        CanKillNeutral = canKillNeutral;
        CanKillImpostor = canKillImpostor;
        CanKillMadRoles = canKillMadRoles;
        CanKillFriendRoles = canKillFriendRoles;
        CanKillLovers = canKillLovers;
    }
    public bool CanKill(PlayerControl killer, ExPlayerControl target)
    {
        bool canKill = false;

        if (target.IsImpostor())
            canKill = CanKillImpostor;
        else if (target.IsNeutral())
            canKill = CanKillNeutral;
        else if (target.Role == RoleId.HauntedWolf || target.ModifierRole.HasFlag(ModifierRoleId.ModifierHauntedWolf)) // 第三陣営に付与される事はEvilSeerのAbilityによる付与時のみの為、第三陣営としての判定を優先
            canKill = CanKillImpostor;
        else if (target.IsMadRoles())
            canKill = CanKillMadRoles;
        else if (target.IsFriendRoles())
            canKill = CanKillFriendRoles;
        else if (target.IsLovers())
            canKill = CanKillLovers;

        // 狼憑きシェリフの判定反転
        if (Modifiers.ModifierHauntedWolf.ModifierHauntedWolfIsReverseSheriffDecision)
        {
            var killExP = ExPlayerControl.ById(killer.PlayerId);
            if (killExP.ModifierRole.HasFlag(ModifierRoleId.ModifierHauntedWolf))
                canKill = !canKill;
        }

        return canKill;
    }

    /// <summary>
    /// シェリフは自決するか
    /// </summary>
    /// <param name="canKill">キルが可能か</param>
    /// <param name="suicideReason">シェリフの死因</param>
    /// <returns>true => 自決する / false => 自決しない</returns>
    public bool IsSuicide(bool canKill, out FinalStatus suicideReason)
    {
        // 常に自決する場合
        if (Mode == SheriffSuicideMode.AlwaysSuicide)
        {
            suicideReason = FinalStatus.SheriffSuicide;
            return true;
        }

        // 自決判定は通常の場合 ("通常" & "誤射時も対象をキルする")
        suicideReason = canKill ? FinalStatus.Alive : FinalStatus.SheriffMisFire;
        return !canKill;
    }

    /// <summary>誤射時も対象をキルする設定が有効か</summary>
    public bool IsAlwaysKilling => Mode == SheriffSuicideMode.AlwaysKill;
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

        var canKill = SheriffAbilityData.CanKill(PlayerControl.LocalPlayer, Target);
        var isSuicide = SheriffAbilityData.IsSuicide(canKill, out FinalStatus suicideReason);

        if (canKill || SheriffAbilityData.IsAlwaysKilling) // 殺害処理
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(Target, CustomDeathType.Kill);
            FinalStatusManager.RpcSetFinalStatus(Target, canKill ? FinalStatus.SheriffKill : FinalStatus.SheriffWrongfulMurder);
        }

        if (isSuicide && suicideReason != FinalStatus.Alive) // 自害処理
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(ExPlayerControl.LocalPlayer, CustomDeathType.Suicide);
            FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, suicideReason);
        }
        ResetTimer();
    }
}

public enum SheriffSuicideMode
{
    Default, // 通常 (成功時のみ対象を殺害, 誤射時のみ自殺)
    AlwaysSuicide, // 常に自殺する
    AlwaysKill // 誤射時も対象を殺す
}