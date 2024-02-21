using AmongUs.GameOptions;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

// 提案者：gamerkun さん
public class Jammer : InvisibleRoleBase, IImpostor, ICustomButton
{
    public static new RoleInfo Roleinfo = new(
        typeof(Jammer),
        (p) => new Jammer(p),
        RoleId.Jammer,
        "Jammer",
        RoleClass.ImpostorRed,
        new(RoleId.Jammer, TeamTag.Impostor, RoleTag.Information),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Jammer, 206300, false,
            CoolTimeOption: (25f, 2.5f, 60f, 2.5f, false),
            DurationTimeOption: (10f, 2.5f, 120f, 2.5f, false),
            AbilityCountOption: (3, 1, 15, 1, false),
            optionCreator: CreateOption);
    public static CustomOption CanUseAbilitiesAgainstImposter;
    private static void CreateOption()
    {
        CanUseAbilitiesAgainstImposter = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "JammerCanUseAbilitiesAgainstImposter", false, Optioninfo.RoleOption);
    }
    public static new IntroInfo Introinfo =
        new(RoleId.Jammer, introSound: RoleTypes.Shapeshifter);
    public Jammer(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        ButtonInfo = new(
            Optioninfo.AbilityMaxCount,
            this,
            () => this.EnableInvisible(ButtonInfo.CurrentTarget, true),
            (isAlive) => isAlive,
            CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            OnMeetingEnds: null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.JammerButton.png", 115f),
            () => Optioninfo.CoolTime,
            new(-2f, 1, 0),
            "JammerButtonName",
            KeyCode.F,
            49,
            baseButton: HudManager.Instance.AbilityButton,
            DurationTime: () => Optioninfo.DurationTime,
            OnEffectEnds: () => { this.DisableInvisible(true); ButtonInfo.ResetCoolTime(); },
            HasAbilityCountText: true,
            SetTargetCrewmateOnly: () => !CanUseAbilitiesAgainstImposter.GetBool()
        );

        this.CustomButtonInfos = new CustomButtonInfo[1] { ButtonInfo };
    }

    // ICustomButton
    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo ButtonInfo { get; }

    // InvisibleRoleBase
    public override bool CanTransparencyStateReflected(PlayerControl invisibleTarget) => invisibleTarget != PlayerControl.LocalPlayer || (CanUseAbilitiesAgainstImposter.GetBool() && PlayerControl.LocalPlayer.IsImpostor()); // 本人視点除外
    public override bool CanSeeTranslucentState(PlayerControl invisibleTarget) => PlayerControl.LocalPlayer.IsImpostor(); // インポスター視点半透明化
}