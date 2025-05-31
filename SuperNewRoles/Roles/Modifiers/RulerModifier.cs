using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;


class RulerModifier : ModifierBase<RulerModifier>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.RulerModifier;

    public override Color32 RoleColor => new(255, 202, 0, 255);

    public override List<Func<AbilityBase>> Abilities => [() => new CustomSidekickButtonAbility(new(
            canCreateSidekick: (created) => !created,
            sidekickCooldown: () => Cooldown,
            sidekickRole: () => RoleId.Madmate,
            sidekickRoleVanilla: () => RoleTypes.Crewmate,
            sidekickSprite: AssetManager.GetAsset<Sprite>("CreateMadmateButton.png"),
            sidekickText: ModTranslation.GetString("CreateMadmateButtonText"),
            sidekickCount: () => 1,
            isTargetable: (player) => !player.IsImpostor()
        ))];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [AssignedTeamType.Impostor];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override Func<ExPlayerControl, string> ModifierMark => (player) => "{0}" + ModHelpers.Cs(RoleColor, "†");

    public override bool AssignFilter => true;

    public override RoleId[] DoNotAssignRoles => [];
    public override RoleId[] RelatedRoleIds => [RoleId.Madmate];

    [CustomOptionFloat("RulerModifierCooldown", 0f, 180f, 2.5f, 30f, translationName: "CoolTime")]
    public static float Cooldown;

}
