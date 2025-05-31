using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Impostor;

class EvilHacker : RoleBase<EvilHacker>
{
    public override RoleId Role { get; } = RoleId.EvilHacker;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new EvilHackerAbility(new EvilHackerData(
        canMoveWhileUsingAdmin: EvilHackerCanMoveWhileUsingAdmin,
        canUseAdminDuringComms: EvilHackerCanUseAdminDuringComms,
        hasEnhancedAdmin: EvilHackerHasEnhancedAdmin,
        canUseAdminInMeeting: EvilHackerCanUseAdminInMeeting,
        showAdminOnSabotageMap: EvilHackerShowAdminOnSabotageMap,
        canSeeImpostorsOnAdmin: EvilHackerCanSeeImpostorsOnAdmin,
        canSeeDeadBodiesOnAdmin: EvilHackerCanSeeDeadBodiesOnAdmin,
        showDoorInfoOnMap: EvilHackerShowDoorInfoOnMap,
        canCreateMadmate: EvilHackerCanCreateMadmate,
        madmateCooldown: EvilHackerCreateMadmateCooldown
    ))];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRolesGMH;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;
    public override RoleId[] RelatedRoleIds { get; } = [RoleId.Madmate];
    // アドミン使用中に移動出来るか
    [CustomOptionBool("EvilHackerCanMoveWhileUsingAdmin", false)]
    public static bool EvilHackerCanMoveWhileUsingAdmin;

    // コミュサボ中もアドミンを使用出来るか
    [CustomOptionBool("EvilHackerCanUseAdminDuringComms", false)]
    public static bool EvilHackerCanUseAdminDuringComms;

    // 強化版アドミンを持つか
    [CustomOptionBool("EvilHackerHasEnhancedAdmin", false)]
    public static bool EvilHackerHasEnhancedAdmin;

    // 会議中にアドミンを使用出来るか (強化版アドミンが必要)
    [CustomOptionBool("EvilHackerCanUseAdminInMeeting", true, parentFieldName: nameof(EvilHackerHasEnhancedAdmin))]
    public static bool EvilHackerCanUseAdminInMeeting;

    // サボタージュマップにもアドミンを表示するか (強化版アドミンが必要)
    [CustomOptionBool("EvilHackerShowAdminOnSabotageMap", true, parentFieldName: nameof(EvilHackerHasEnhancedAdmin))]
    public static bool EvilHackerShowAdminOnSabotageMap;

    // インポスターの位置がわかるか (強化版アドミンが必要)
    [CustomOptionBool("EvilHackerCanSeeImpostorsOnAdmin", true, parentFieldName: nameof(EvilHackerHasEnhancedAdmin))]
    public static bool EvilHackerCanSeeImpostorsOnAdmin;

    // 死体の位置がわかるか (強化版アドミンが必要)
    [CustomOptionBool("EvilHackerCanSeeDeadBodiesOnAdmin", true, parentFieldName: nameof(EvilHackerHasEnhancedAdmin))]
    public static bool EvilHackerCanSeeDeadBodiesOnAdmin;

    // マップにドアの開閉情報を表示するか (強化版アドミンが必要)
    [CustomOptionBool("EvilHackerShowDoorInfoOnMap", true, parentFieldName: nameof(EvilHackerHasEnhancedAdmin))]
    public static bool EvilHackerShowDoorInfoOnMap;

    // マッドメイトを指名出来るか
    [CustomOptionBool("EvilHackerCanCreateMadmate", false)]
    public static bool EvilHackerCanCreateMadmate;

    // 「狂わせる」クールタイム (マッドメイトを作成できる場合のみ)
    [CustomOptionFloat("EvilHackerMadmateCooldown", 2.5f, 60f, 2.5f, 30f, parentFieldName: nameof(EvilHackerCanCreateMadmate))]
    public static float EvilHackerCreateMadmateCooldown;
}

// イビルハッカーの設定データ
public record EvilHackerData(
    bool canMoveWhileUsingAdmin,
    bool canUseAdminDuringComms,
    bool hasEnhancedAdmin,
    bool canUseAdminInMeeting,
    bool showAdminOnSabotageMap,
    bool canSeeImpostorsOnAdmin,
    bool canSeeDeadBodiesOnAdmin,
    bool showDoorInfoOnMap,
    bool canCreateMadmate,
    float madmateCooldown
);

/// <summary>
/// イビルハッカーの能力クラス
/// </summary>
public class EvilHackerAbility : AbilityBase
{
    private readonly EvilHackerData Data;
    private PortableAdminAbility _portableAdminAbility;
    private AdvancedAdminAbility _advancedAdminAbility;
    private CustomSidekickButtonAbility _sidekickButtonAbility;
    public EvilHackerAbility(EvilHackerData hackerData) : base()
    {
        Data = hackerData;
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);

        _portableAdminAbility = new PortableAdminAbility(new PortableAdminData(
            CanUseAdmin: () => true,
            canUseAdminDuringComms: () => Data.canUseAdminDuringComms,
            canMoveWhileUsingAdmin: () => Data.canMoveWhileUsingAdmin
        ));

        _advancedAdminAbility = new AdvancedAdminAbility(new AdvancedAdminData(
            AdvancedAdmin: () => Data.hasEnhancedAdmin,
            CanUseAdminDuringComms: () => Data.canUseAdminDuringComms,
            CanUseAdminDuringMeeting: () => Data.canUseAdminInMeeting,
            SabotageMapShowsAdmin: () => Data.showAdminOnSabotageMap,
            DistinctionImpostor: () => Data.canSeeImpostorsOnAdmin,
            DistinctionDead: () => Data.canSeeDeadBodiesOnAdmin,
            ShowDoorClosedMarks: () => Data.showDoorInfoOnMap
        ));

        _sidekickButtonAbility = new CustomSidekickButtonAbility(new(
            canCreateSidekick: (created) => Data.canCreateMadmate && !created,
            sidekickCooldown: () => Data.madmateCooldown,
            sidekickRole: () => RoleId.Madmate,
            sidekickRoleVanilla: () => RoleTypes.Crewmate,
            sidekickSprite: AssetManager.GetAsset<Sprite>("CreateMadmateButton.png"),
            sidekickText: ModTranslation.GetString("CreateMadmateButtonText"),
            sidekickCount: () => 1,
            isTargetable: (player) => !player.IsImpostor()
        ));

        ((ExPlayerControl)player).AttachAbility(_portableAdminAbility, new AbilityParentAbility(this));
        ((ExPlayerControl)player).AttachAbility(_advancedAdminAbility, new AbilityParentAbility(this));
        ((ExPlayerControl)player).AttachAbility(_sidekickButtonAbility, new AbilityParentAbility(this));
    }
    public override void AttachToLocalPlayer()
    {
    }
}