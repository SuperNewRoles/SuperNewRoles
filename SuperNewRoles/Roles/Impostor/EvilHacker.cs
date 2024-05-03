using System;
using System.Collections.Generic;

using AmongUs.GameOptions;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles.Roles.Impostor;

public class EvilHacker : RoleBase, IImpostor, ICustomButton
{
    public static new RoleInfo Roleinfo = new(
        typeof(EvilHacker),
        (p) => new EvilHacker(p),
        RoleId.EvilHacker,
        "EvilHacker",
        RoleClass.ImpostorRed,
        new(RoleId.EvilHacker, TeamTag.Impostor,
            RoleTag.Information, RoleTag.Killer),
        TeamRoleType.Impostor,
        TeamType.Impostor,
        QuoteMod.TheOtherRolesGMH
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.EvilHacker, 200300, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.EvilHacker, introNum: 2, introSound: RoleTypes.Impostor);

    public static CustomOption CanMoveWhenUsesAdmin;
    public static CustomOption MadmateSetting;
    public static CustomOption ButtonCooldown;
    public static CustomOption HasEnhancedAdmin;
    public static CustomOption CanSeeImpostorPositions;
    public static CustomOption CanSeeDeadBodyPositions;
    public static CustomOption CanUseAdminDuringMeeting;
    public static CustomOption SabotageMapShowsAdmin;
    public static CustomOption MapShowsDoorState;
    public static CustomOption CanUseAdminDuringCommsSabotaged;

    public CustomButtonInfo[] CustomButtonInfos { get; }

    private static void CreateOption()
    {
        CanMoveWhenUsesAdmin = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "CanMoveWhenUsesAdmin", false, Optioninfo.RoleOption);
        MadmateSetting = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "CreateMadmateSetting", false, Optioninfo.RoleOption);
        ButtonCooldown = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "CreateMadmateButtonCooldownSetting", 30f, 0f, 60f, 2.5f, MadmateSetting);
        HasEnhancedAdmin = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilHackerHasEnhancedAdmin", true, Optioninfo.RoleOption);
        CanSeeImpostorPositions = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilHackerCanSeeImpostorPositions", true, HasEnhancedAdmin);
        CanSeeDeadBodyPositions = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilHackerCanSeeDeadBodyPositions", true, HasEnhancedAdmin);
        CanUseAdminDuringMeeting = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilHackerCanUseAdminDuringMeeting", true, Optioninfo.RoleOption);
        SabotageMapShowsAdmin = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilHackerSabotageMapShowsAdmin", true, Optioninfo.RoleOption);
        MapShowsDoorState = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilHackerMapShowsDoorState", true, Optioninfo.RoleOption);
        CanUseAdminDuringCommsSabotaged = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilHackerCanUseAdminDuringCommsSabotaged", true, Optioninfo.RoleOption);
    }
    public bool IsMyAdmin { get; set; }
    public bool CanCreateMadmate { get; private set; }
    public void EvilHackerAdminButtonOnClick()
    {
        IsMyAdmin = true;
        FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
        {
            Mode = MapOptions.Modes.CountOverlay,
            AllowMovementWhileMapOpen = CanMoveWhenUsesAdmin.GetBool()
        });
    }
    public void CreateMadmateButtonOnClick()
    {
        var target = EvilHackerMadmateButtonInfo.CurrentTarget;
        if (target.IsImpostor())
            return;
        if (!CanCreateMadmate)
            return;
        Madmate.CreateMadmate(target);
        CanCreateMadmate = false;
    }
    public static Sprite GetAdminButtonSprite()
    {
        MapNames mapId = (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
        UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
        if (mapId is MapNames.Skeld or MapNames.Dleks) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
        else if (mapId is MapNames.Mira) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
        else if (mapId == MapNames.Airship) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
        return button.Image;
    }
    public CustomButtonInfo EvilHackerButtonInfo { get; }
    public CustomButtonInfo EvilHackerMadmateButtonInfo { get; }
    public EvilHacker(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        EvilHackerButtonInfo = new(null, this, EvilHackerAdminButtonOnClick,
            null, CustomButtonCouldType.CanMove, () => IsMyAdmin = false,
            GetAdminButtonSprite(), null, new(-2f, 1, 0),
            "ADMINButton", KeyCode.F, 49);

        EvilHackerMadmateButtonInfo = new(null, this, CreateMadmateButtonOnClick,
        (isAlive) => isAlive && CanCreateMadmate, CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget, null,
        ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CreateMadmateButton.png", 115f),
        ButtonCooldown.GetFloat, new(-2.925f, -0.06f, 0), "CreateMadmateButton",
        null, null, CouldUse: () => !Frankenstein.IsMonster(EvilHackerMadmateButtonInfo.CurrentTarget), SetTargetCrewmateOnly: () => true);

        CustomButtonInfos = new CustomButtonInfo[2]
        {
            EvilHackerButtonInfo, EvilHackerMadmateButtonInfo
        };
        CanCreateMadmate = MadmateSetting.GetBool();
    }
}