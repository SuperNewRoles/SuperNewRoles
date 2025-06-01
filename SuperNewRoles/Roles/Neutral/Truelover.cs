using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Modifiers;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Truelover : RoleBase<Truelover>
{
    public override RoleId Role { get; } = RoleId.Truelover;
    public override Color32 RoleColor { get; } = Lovers.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new CreateLoversAbility(TrueLoverCoolTime, ModTranslation.GetString("TrueLoverCreateLovers"), AssetManager.GetAsset<Sprite>("trueloverloveButton.png"), true, enabledTimeLimit: TrueloverEnabledTimeLimit, timeLimit: TrueloverTimeLimit)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];
    [CustomOptionFloat("TrueLoverCoolTime", 0f, 180f, 2.5f, 0f, translationName: "CoolTime")]
    public static float TrueLoverCoolTime;
    [CustomOptionBool("TrueloverEnabledTimeLimit", true)]
    public static bool TrueloverEnabledTimeLimit;
    [CustomOptionFloat("TrueloverTimeLimit", 30f, 600f, 15f, 120f, parentFieldName: nameof(TrueloverEnabledTimeLimit))]
    public static float TrueloverTimeLimit;
}

// TODO: リワーク
public class TrueloverAbility : AbilityBase
{
    private EventListener _fixedUpdateListener;
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
    }

}