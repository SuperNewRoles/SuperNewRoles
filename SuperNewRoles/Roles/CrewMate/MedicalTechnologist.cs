/* ◯作り方◯
    1.ICrewmateかINeutralかIImpostorのどれかを継承する // [x]
    2.必要なインターフェースを実装する // [x]
    3.Roleinfo,Optioninfo,Introinfoを設定する // [x]
    4.設定を作成する(CreateOptionが必要なければOptioninfoのoptionCreatorをnullにする) // [x]
    5.インターフェースの内容を実装していく // [ ]
*/

using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class MedicalTechnologist : RoleBase, ICrewmate, ISupportSHR, ICustomButton, IMeetingHandler, ICheckMurderHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(MedicalTechnologist),
        (p) => new MedicalTechnologist(p),
        RoleId.MedicalTechnologist,
        "MedicalTechnologist",
        new(37, 159, 148, byte.MaxValue),
        new(RoleId.MedicalTechnologist, TeamTag.Crewmate, RoleTag.Information),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );

    public static new OptionInfo Optioninfo =
        new(RoleId.MedicalTechnologist, 406700, true,
            CoolTimeOption: (15f, 2.5f, 60f, 2.5f, true),
            AbilityCountOption: (1, 1, 15, 1, true),
            optionCreator: null);

    public static new IntroInfo Introinfo = new(RoleId.MedicalTechnologist, introSound: RoleTypes.Scientist);

    // RoleClass

    /// <summary>
    /// 残りアビリティ使用可能回数
    /// </summary>
    public int AbilityRemainingCount;
    /// <summary>
    /// サンプル取得中のクルー
    /// </summary>
    private (PlayerControl FirstCrew, PlayerControl SecondCrew) SampleCrews;

    public MedicalTechnologist(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        MTButtonInfo = new(
            null,
            this,
            () => ButtonOnClick(),
            (isAlive) => isAlive,
            CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            OnMeetingEnds: MTButtonReset,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MedicalTechnologistButton.png", 115f),
            () => Optioninfo.CoolTime,
            new(-2f, 1, 0),
            "MedicalTechnologistnName",
            KeyCode.F,
            49,
            baseButton: HudManager.Instance.AbilityButton,
            CouldUse: OnCouldUse,
            isUseSecondButtonInfo: true
        ); // [x]MEMO : 残り回数表示の更新等をできるように追加する

        CustomButtonInfos = new CustomButtonInfo[1] { MTButtonInfo };

        AbilityRemainingCount = Optioninfo.AbilityMaxCount;
    }

    // ISupportSHR
    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole => RoleTypes.Impostor;
    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
        ChangePlayers[this.Player.PlayerId] = $"{ChangeName.GetNowName(ChangePlayers, this.Player)}\n{MtButtonCountString()}";
    }

    // ICustomButton
    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo MTButtonInfo { get; }
    private void ButtonOnClick() { }
    private void MTButtonReset() { } // [ ]MEMO : 対象のリセット, ターン中使用回数をリセット
    private string MtButtonCountString() // [ ]MEMO : 残り全体回数\n現在フェイズ残り指定回数 (SHRではシェリフと同じように名前で表示)
    {
        return $"";
    }
    private bool OnCouldUse() =>  AbilityRemainingCount > 0 && (SampleCrews.FirstCrew == null || SampleCrews.SecondCrew == null);

    // IMeetingHandler
    public void StartMeeting() { }
    public void CloseMeeting() { }

    // ICheckMurderHandler
    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        return true; // [ ]MEMO : クールはリセットしたい。~> キルを守護で防ぐ事が必要?
    }
}