using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Attribute;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;

namespace SuperNewRoles.Roles.Impostor;

public class EvilSeer : SeerBase, IImpostor, ISupportSHR, ICustomButton
{
    public static new RoleInfo Roleinfo = new(
        typeof(EvilSeer),
        (p) => new EvilSeer(p),
        RoleId.EvilSeer,
        "EvilSeer",
        RoleClass.ImpostorRed,
        new(RoleId.EvilSeer, TeamTag.Impostor,
            RoleTag.Killer, RoleTag.Information),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.EvilSeer, 201900, true,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.EvilSeer, introSound: RoleTypes.Impostor);

    public static CustomOption Mode;
    public static CustomOption LimitSoulDuration;
    public static CustomOption SoulDuration;
    public static CustomOption IsUniqueSetting;
    public static CustomOption FlashColorMode;
    public static CustomOption IsFlashBodyColor;
    public static CustomOption IsReportingBodyColorName;
    public static CustomOption IsCrewSoulColor;
    public static CustomOption IsDeadBodyArrow;
    public static CustomOption IsArrowColorAdaptiveOption;
    public static CustomOption CreateSetting;
    public static CustomOption CreateModeOption;
    public static CustomOption EvilSeerButtonCooldown;
    private static void CreateOption()
    {
        Mode = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, Optioninfo.RoleOption);
        LimitSoulDuration = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "SeerLimitSoulDuration", false, Optioninfo.RoleOption);
        SoulDuration = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "SeerSoulDuration", 15f, 0f, 120f, 5f, LimitSoulDuration, format: "unitCouples");
        IsUniqueSetting = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilSeerIsUniqueSetting", true, Optioninfo.RoleOption);
        FlashColorMode = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilSeerFlashColorMode", new string[] { "EvilSeerColorModeclear", "EvilSeerColorModeLightAndDark" }, IsUniqueSetting);
        IsFlashBodyColor = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilSeerIsFlashColor", true, IsUniqueSetting);
        IsReportingBodyColorName = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilSeerIsReportingBodyColorName", true, IsFlashBodyColor);
        IsCrewSoulColor = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilSeerIsCrewSoulColor", true, IsUniqueSetting);
        IsDeadBodyArrow = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "VultureShowArrowsSetting", true, IsUniqueSetting);
        IsArrowColorAdaptiveOption = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilSeerIsArrowColorAdaptive", true, IsDeadBodyArrow);
        CreateSetting = Create(Optioninfo.OptionId++, true, CustomOptionType.Impostor, "EvilSeerCreateSetting", false, Optioninfo.RoleOption);
        CreateModeOption = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilSeerCreateHauntedWolfMode", new string[] { "optionOff", "EvilSeerCreateHauntedWolfModeBoth", "EvilSeerCreateHauntedWolfModeAbilityOnry", "EvilSeerCreateHauntedWolfModePassiveOnry", "CreateMadmateSetting" }, CreateSetting);
        EvilSeerButtonCooldown = Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "EvilSeerButtonCooldownSetting", 30f, 0f, 60f, 2.5f, CreateSetting);
    }

    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
    }

    public bool IsUnique { get; }
    public bool IsClearColor { get; }
    public bool IsArrow { get; }
    public bool IsArrowColorAdaptive { get; }
    public bool CanCreate { get; private set; }
    /// <summary>
    /// 狼憑き, マッドメイトを作成する能力のモードを取得する
    /// </summary>
    /// <value>0 : オフ, 1 : 取り憑かせる＆憑り付く, 2 : 取り憑かせる, 3 : 取り付く, 4 : マッドメイトを作れる</value>
    public static int CreateMode { get; set; }
    public Dictionary<DeadBody, ArrowAdaptive> DeadPlayerArrows { get; }

    public RoleTypes RealRole => RoleTypes.Shapeshifter;

    public CustomButtonInfo[] CustomButtonInfos { get; }

    public PlayerData<bool> CanCreateSHR;
    public CustomButtonInfo CreateHauntedWolfButtonInfo { get; }
    public CustomButtonInfo CreateMadmateButtonInfo { get; }
    private void CreateHauntedWolfOnClick()
    {
        var target = CreateHauntedWolfButtonInfo.CurrentTarget;
        if (target == null) return;
        CanCreate = false;

        HauntedWolf.Assign.SetHauntedWolf(target);
        HauntedWolf.Assign.SetHauntedWolfRPC(target);
    }
    private void CreateMadmateButtonOnClick()
    {
        if (!CreateMadmateButtonInfo.CurrentTarget)
            return;
        Madmate.CreateMadmate(CreateMadmateButtonInfo.CurrentTarget);
        CanCreate = false;
    }
    public EvilSeer(PlayerControl p) : base(SoulDuration.GetInt(), LimitSoulDuration.GetBool(), Mode.GetSelection(), p, Roleinfo, Optioninfo, Introinfo)
    {
        deadBodyPositions = new();
        CanCreateSHR = new();
        IsUnique = !ModeHandler.IsMode(ModeId.SuperHostRoles) && IsUniqueSetting.GetBool();
        IsClearColor = FlashColorMode.GetSelection() == 0;
        IsArrow = IsUnique && IsDeadBodyArrow.GetBool();
        IsArrowColorAdaptive = IsArrow && IsArrowColorAdaptiveOption.GetBool();
        CreateMode =
            !CreateSetting.GetBool()
                ? 0 // 設定がオフなら, モードもオフにする
                : ModeHandler.IsMode(ModeId.SuperHostRoles)
                    ? 3 // SHRなら[取り憑く]モードに固定する・
                    : CreateModeOption.GetSelection(); // 設定が有効で, SHR出ない時モード設定を取得する。
        CanCreate = CreateMode != 0;
        DeadPlayerArrows = new();
        CreateHauntedWolfButtonInfo = new(null, this,
            CreateHauntedWolfOnClick, (isAlive) => isAlive && CreateMode is not 0 and <= 2 && CanCreate,
            CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            null, ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CreateHauntedWolfButton.png", 115f),
            EvilSeerButtonCooldown.GetFloat,
            new(-2f, 1, 0), "EvilSeerButtonName",
            KeyCode.F, 49);

        CreateMadmateButtonInfo = new(null, this, CreateMadmateButtonOnClick,
        (isAlive) => isAlive && CreateMode == 4 && CanCreate, CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget, null,
        ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CreateMadmateButton.png", 115f),
        EvilSeerButtonCooldown.GetFloat, new(-2.925f, -0.06f, 0), "CreateMadmateButton",
        null, null, CouldUse: () => !Frankenstein.IsMonster(CreateMadmateButtonInfo.CurrentTarget), SetTargetCrewmateOnly: () => true);
        CustomButtonInfos = new CustomButtonInfo[2]
        {
            CreateHauntedWolfButtonInfo,
            CreateMadmateButtonInfo
        };
    }
    public void DeadBodyArrowFixedUpdate()
    {
        if (!IsArrow) return;

        foreach (var arrow in DeadPlayerArrows)
        {
            bool isTarget = false;
            foreach (DeadBody dead in UnityEngine.Object.FindObjectsOfType<DeadBody>())
            {
                if (arrow.Key.ParentId != dead.ParentId) continue;
                isTarget = true;
                break;
            }
            if (isTarget)
            {
                var deadPlayer = ModHelpers.GetPlayerControl(arrow.Key.ParentId);
                var arrowColor = DefaultBodyColorId;

                if (IsArrowColorAdaptive && IsClearColor) arrowColor = deadPlayer.Data.DefaultOutfit.ColorId; // 最高が最高
                else if (IsArrowColorAdaptive) // 明暗
                {
                    var isLight = CustomCosmetics.CustomColors.LighterColors.Contains(deadPlayer.Data.DefaultOutfit.ColorId);
                    arrowColor = isLight ? LightBodyColorId : DarkBodyColorId;
                }

                if (arrow.Value == null)
                {
                    DeadPlayerArrows[arrow.Key] = new(arrowColor);
                }
                arrow.Value.Update(arrow.Key.transform.position, arrowColor);
                arrow.Value.arrow.SetActive(true);
            }
            else
            {
                if (arrow.Value?.arrow != null)
                    UnityEngine.Object.Destroy(arrow.Value.arrow);
                DeadPlayerArrows.Remove(arrow.Key);
            }
        }
        foreach (DeadBody dead in UnityEngine.Object.FindObjectsOfType<DeadBody>())
        {
            if (DeadPlayerArrows.Any(x => x.Key.ParentId == dead.ParentId)) continue;

            var deadPlayer = ModHelpers.GetPlayerControl(dead.ParentId);
            var arrowColor = IsArrowColorAdaptive ? deadPlayer.Data.DefaultOutfit.ColorId : DefaultBodyColorId;

            DeadPlayerArrows.Add(dead, new(arrowColor));
            DeadPlayerArrows[dead].Update(dead.transform.position, arrowColor);
            DeadPlayerArrows[dead].arrow.SetActive(true);
        }
    }
    /// <summary>
    /// 通常モードでの[取り付く]能力の処理
    /// </summary>
    /// <param name="killer">自身をキルしたプレイヤー</param>
    internal void OnKillDefaultMode(PlayerControl killer)
    {
        if (CreateMode is not (1 or 3)) return;
        if (!CanCreate || killer == PlayerControl.LocalPlayer || killer == null) return;

        CanCreate = false;
        HauntedWolf.Assign.SetHauntedWolf(killer);
        HauntedWolf.Assign.SetHauntedWolfRPC(killer);
    }

    /// <summary>
    /// SHRモードでの[取り付く]能力の処理
    /// </summary>
    /// <param name="killer">自身をキルしたプレイヤー</param>
    internal void OnKillSuperHostRolesMode(PlayerControl killer, PlayerControl victim)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (CreateMode is not (1 or 3)) return;
        if (killer == victim || killer == null) return;

        var killerId = killer.PlayerId;
        if (CanCreateSHR.Contains(killerId)) return;

        CanCreateSHR[killerId] = false;
        HauntedWolf.Assign.SetHauntedWolf(killer);
        HauntedWolf.Assign.SetHauntedWolfRPC(killer);
    }
}