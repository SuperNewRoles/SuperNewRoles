using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.CustomOptions;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Impostor;

public enum CamouflageColorType
{
    Fixed,
    Select,
    Random
}

public enum CamouflageColor
{
    Red = 0,
    Blue = 1,
    Green = 2,
    Pink = 3,
    Orange = 4,
    Yellow = 5,
    Black = 6,
    White = 7,
    Purple = 8,
    Brown = 9,
    Cyan = 10,
    Lime = 11,
    Maroon = 12,
    Rose = 13,
    Banana = 14,
    Gray = 15,
    Tan = 16,
    Coral = 17
}

class Camouflager : RoleBase<Camouflager>
{
    public override RoleId Role { get; } = RoleId.Camouflager;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new CamouflagerAbility(new(
            coolTime: CamouflagerCoolTime,
            durationTime: CamouflagerDurationTime,
            camouflageColor: (int)CamouflagerCamouflageColor,
            changeColorType: (int)CamouflagerCamouflageChangeColor
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("CamouflagerCoolTime", 2.5f, 180f, 2.5f, 30f)]
    public static float CamouflagerCoolTime;
    [CustomOptionFloat("CamouflagerDurationTime", 2.5f, 60f, 2.5f, 15f)]
    public static float CamouflagerDurationTime;
    [CustomOptionSelect("CamouflagerCamouflageChangeColor", typeof(CamouflageColorType), "CamouflagerCamouflageChangeColor.")]
    public static CamouflageColorType CamouflagerCamouflageChangeColor;
    [CustomOptionSelect("CamouflagerCamouflageColor", typeof(CamouflageColor), "CamouflagerCamouflageColor.", parentFieldName: nameof(CamouflagerCamouflageChangeColor), parentActiveValue: CamouflageColorType.Select)]
    public static CamouflageColor CamouflagerCamouflageColor;
}