using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;
public class EvilScientist : InvisibleRoleBase, IImpostor, ICustomButton
{
    public static new RoleInfo Roleinfo = new(
        typeof(EvilScientist),
        (p) => new EvilScientist(p),
        RoleId.EvilScientist,
        "EvilScientist",
        RoleClass.ImpostorRed,
        new(RoleId.EvilScientist, TeamTag.Impostor),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.EvilScientist, 205300, false,
            CoolTimeOption: (30f, 2.5f, 60f, 2.5f, false),
            DurationTimeOption: (10f, 2.5f, 20f, 2.5f, false),
            optionCreator: null);
    public static new IntroInfo Introinfo =
        new(RoleId.EvilScientist, introNum: 2, introSound: RoleTypes.Scientist);
    public EvilScientist(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        ButtonInfo = new(
            null,
            this,
            () => this.EnableInvisible(PlayerControl.LocalPlayer, true),
            (isAlive) => isAlive,
            CustomButtonCouldType.CanMove,
            OnMeetingEnds: null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.EvilScientistButton.png", 115f),
            () => Optioninfo.CoolTime,
            new(-2f, 1, 0),
            "ScientistButtonName",
            KeyCode.F,
            49,
            baseButton: HudManager.Instance.AbilityButton,
            DurationTime: () => {Logger.Info($"Optioninfo.DurationTime = {Optioninfo.DurationTime}"); return Optioninfo.DurationTime;},
            OnEffectEnds: () => this.DisableInvisible(true)
        );

        this.CustomButtonInfos = new CustomButtonInfo[1] { ButtonInfo };
    }

    // ICustomButton
    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo ButtonInfo { get; }
    public override bool CanSeeTranslucentState(PlayerControl invisibleTarget)
    {
        bool result =
            invisibleTarget == PlayerControl.LocalPlayer
                ? true
                : invisibleTarget.IsImpostor() && PlayerControl.LocalPlayer.IsImpostor();

        return result;
    }
}
