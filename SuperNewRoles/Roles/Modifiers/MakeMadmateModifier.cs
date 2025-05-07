using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Modifiers;


class MakeMadmateModifier : ModifierBase<MakeMadmateModifier>
{
    public override ModifierRoleId ModifierRole => ModifierRoleId.MakeMadmateModifier;

    public override Color32 RoleColor => Palette.ImpostorRed;

    public override List<Func<AbilityBase>> Abilities => [() => new CustomSidekickButtonAbility(
            canCreateSidekick: (created) => !created,
            sidekickCooldown: () => Cooldown,
            sidekickRole: () => RoleId.Madmate,
            sidekickRoleVanilla: () => RoleTypes.Crewmate,
            sidekickSprite: AssetManager.GetAsset<Sprite>("CreateMadmateButton.png"),
            sidekickText: ModTranslation.GetString("CreateMadmateButtonText"),
            sidekickCount: () => 1,
            isTargetable: (player) => !player.IsImpostor()
        )];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override List<AssignedTeamType> AssignedTeams => [AssignedTeamType.Impostor];

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override TeamTag TeamTag => TeamTag.Crewmate;

    public override RoleTag[] RoleTags => [];

    public override short IntroNum => 1;
    public override Func<ExPlayerControl, string> ModifierMark => (player) => ModHelpers.Cs(RoleColor, "狂わせる{0}");

    [CustomOptionFloat("MakeMadmateModifierCooldown", 0f, 180f, 2.5f, 30f, translationName: "CoolTime")]
    public static float Cooldown;

}
