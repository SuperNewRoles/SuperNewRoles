using AmongUs.GameOptions;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;
public class NiceScientist : InvisibleRoleBase, ICrewmate, ICustomButton
{
    public static new RoleInfo Roleinfo = new(
        typeof(NiceScientist),
        (p) => new NiceScientist(p),
        RoleId.NiceScientist,
        "NiceScientist",
        Palette.CrewmateBlue,
        new(RoleId.NiceScientist, TeamTag.Crewmate),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.NiceScientist, 404200, false,
            CoolTimeOption: (30f, 2.5f, 60f, 2.5f, false),
            DurationTimeOption: (10f, 2.5f, 20f, 2.5f, false),
            optionCreator: CreateOption);
    public static CustomOption CanTheLighterSeeTheScientist;
    private static void CreateOption()
    {
        CanTheLighterSeeTheScientist = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Crewmate, "ScientistCanTheLighterSeeTheScientist", true, Optioninfo.RoleOption);
    }
    public static new IntroInfo Introinfo =
        new(RoleId.NiceScientist, introNum: 2, introSound: RoleTypes.Scientist);
    public NiceScientist(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        ButtonInfo = new(
            null,
            this,
            () => this.EnableInvisible(PlayerControl.LocalPlayer, true),
            (isAlive) => isAlive,
            CustomButtonCouldType.CanMove,
            OnMeetingEnds: null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.NiceScientistButton.png", 115f),
            () => Optioninfo.CoolTime,
            new(-2f, 1, 0),
            "ScientistButtonName",
            KeyCode.F,
            49,
            baseButton: HudManager.Instance.AbilityButton,
            DurationTime: () => Optioninfo.DurationTime,
            OnEffectEnds: () => { this.DisableInvisible(true); ButtonInfo.ResetCoolTime(); }
        );

        this.CustomButtonInfos = new CustomButtonInfo[1] { ButtonInfo };
    }

    // ICustomButton
    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo ButtonInfo { get; }
    public override bool CanSeeTranslucentState(PlayerControl invisibleTarget) =>
        invisibleTarget == PlayerControl.LocalPlayer ||
        (CanTheLighterSeeTheScientist.GetBool() && PlayerControl.LocalPlayer.IsRole(RoleId.Lighter) && RoleClass.Lighter.IsLightOn);
}