using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate;

class Impostor : RoleBase<Impostor>
{
    public override RoleId Role { get; } = RoleId.Impostor;

    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [];

    public override QuoteMod QuoteMod { get; } = QuoteMod.Vanilla;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    //TODO:Intro文章の指定だけどこれなんで複数あるの？
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    //これはむしろPlayer毎に設定するべきでは？
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;

    public override TeamTag TeamTag { get; } = TeamTag.Impostor;

    //TODO:要検討
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;
}