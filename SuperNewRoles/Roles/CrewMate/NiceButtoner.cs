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

namespace SuperNewRoles.Roles.Crewmate;

class NiceButtoner : RoleBase<NiceButtoner>
{
    public override RoleId Role => RoleId.NiceButtoner;
    public override Color32 RoleColor => new(0, 255, 255, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities => [() => new ButtonerAbility(new(NiceButtonerCooldown, NiceButtonerUseLimit), AssetManager.GetAsset<Sprite>("NiceButtonerButton.png"))];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [];
    public override short IntroNum => 1; // 適切な値に変更してください
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionInt("NiceButtonerUseLimit", 1, 10, 1, 1, translationName: "UseLimit")]
    public static int NiceButtonerUseLimit;

    [CustomOptionFloat("NiceButtonerCooldown", 0f, 180f, 2.5f, 20f, translationName: "CoolTime")]
    public static float NiceButtonerCooldown;
}