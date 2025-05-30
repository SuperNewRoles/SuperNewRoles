using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class EvilButtoner : RoleBase<EvilButtoner>
{
    public override RoleId Role => RoleId.EvilButtoner;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => [() => new ButtonerAbility(new(EvilButtonerCooldown, EvilButtonerUseLimit), AssetManager.GetAsset<Sprite>("EvilButtonerButton.png"))];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionInt("EvilButtonerUseLimit", 1, 10, 1, 1, translationName: "UseLimit")]
    public static int EvilButtonerUseLimit;

    [CustomOptionFloat("EvilButtonerCooldown", 0f, 180f, 2.5f, 20f, translationName: "CoolTime")]
    public static float EvilButtonerCooldown;
}