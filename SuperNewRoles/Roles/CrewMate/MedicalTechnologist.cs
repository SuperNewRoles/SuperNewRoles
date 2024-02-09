/* ◯作り方◯
    1.ICrewmateかINeutralかIImpostorのどれかを継承する // [x]
    2.必要なインターフェースを実装する // [x]
    3.Roleinfo,Optioninfo,Introinfoを設定する // [x]
    4.設定を作成する(CreateOptionが必要なければOptioninfoのoptionCreatorをnullにする) // [x]
    5.インターフェースの内容を実装していく // [ ]
*/

using System;
using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class MedicalTechnologist : RoleBase, ICrewmate, ISupportSHR, ICustomButton, INameHandler, IMeetingHandler, ICheckMurderHandler
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
            optionCreator: CreateOption);

    // customOptionData

    /// <summary>
    /// 勝利判定分類の陣営で判定するか
    /// </summary>
    /// <value>true : 勝利判定分類の陣営で判定する, false => アサイン分類及の陣営 及び モデファイラ側の勝利条件で判定する。</value>
    public static CustomOption IsJudgmentTeamType;

    /// <summary>
    /// 第三陣営間の判定を, それぞれの勝利条件毎で個別に判定するか (第三陣営同士は敵と判断するか)
    /// </summary>
    public static CustomOption IsJudgmentDetailedNeutralTeamType;
    private static void CreateOption()
    {
        IsJudgmentTeamType = CustomOption.Create(Optioninfo.OptionId++, true, CustomOptionType.Crewmate, "MedicalTechnologistIsJudgmentTeamType", true, Optioninfo.RoleOption);
        IsJudgmentDetailedNeutralTeamType = CustomOption.Create(Optioninfo.OptionId++, true, CustomOptionType.Crewmate, "MedicalTechnologistIsJudgmentDetailedNeutralTeamType", true, Optioninfo.RoleOption);
    }

    public static new IntroInfo Introinfo = new(RoleId.MedicalTechnologist, introSound: RoleTypes.Scientist);

    // RoleClass

    /// <summary>
    /// 残りアビリティ使用可能回数
    /// </summary>
    public int AbilityRemainingCount;
    /// <summary>
    /// サンプル取得中のクルー
    /// </summary>
    private (byte FirstCrew, byte SecondCrew) SampleCrews;

    /// <summary>
    /// 判定の方法を記録する
    /// </summary>
    private static JudgmentType JudgmentSystem => _JudgmentSystem;
    private static JudgmentType _JudgmentSystem { get; set; }

    [Flags]
    enum JudgmentType
    {
        None = 0b00,                //  (false, false)
        TeamType = 0b01,            //  (false, true)
        DetailedNeutral = 0b10,     //  (true, false)
        //                              (true, true)    0b11
    }

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
            () => OnCouldUse() ? Optioninfo.CoolTime : 0f, // [x]MEMO : 使い切ったらクールを0にする
            new(-2f, 1, 0),
            "MedicalTechnologistButtonName",
            KeyCode.F,
            49,
            baseButton: HudManager.Instance.AbilityButton,
            CouldUse: () => OnCouldUse(),
            SetTargetUntargetPlayer: () => SetTargetUntargetPlayer(),
            isUseSecondButtonInfo: true
        ); // [x]MEMO : 残り回数表示の更新等をできるように追加する

        this.CustomButtonInfos = new CustomButtonInfo[1] { MTButtonInfo };

        this.AbilityRemainingCount = Optioninfo.AbilityMaxCount;
        this.SampleCrews = (byte.MaxValue, byte.MaxValue);

        JudgmentType judgmentSystem = JudgmentType.None;
        judgmentSystem |= IsJudgmentTeamType.GetBool() ? JudgmentType.TeamType : JudgmentType.None;                         // 0b_0x
        judgmentSystem |= IsJudgmentDetailedNeutralTeamType.GetBool() ? JudgmentType.DetailedNeutral : JudgmentType.None;   // 0b_x0

        _JudgmentSystem = judgmentSystem; // 0b_xx
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
    private void ButtonOnClick()
    {
        if (SampleCrews.FirstCrew == byte.MaxValue) SampleCrews.FirstCrew = MTButtonInfo.CurrentTarget.PlayerId;
        else if (SampleCrews.SecondCrew == byte.MaxValue)
        {
            SampleCrews.SecondCrew = MTButtonInfo.CurrentTarget.PlayerId;
            AbilityRemainingCount--;
        }
        else Logger.Error("既に検体を取得済みにもかかわらず, 対象が取得されました。", Roleinfo.NameKey);

        MTButtonInfo.customButton.SecondButtonInfoText.text = MtButtonCountString();
    }
    private void MTButtonReset() // [x]MEMO : 対象のリセット, [ターン中使用回数をリセット => ターン中使用回数と対象は同じ変数で管理に]
    {
        SampleCrews = (byte.MaxValue, byte.MaxValue);
        MTButtonInfo.customButton.SecondButtonInfoText.text = MtButtonCountString();
    }
    private string MtButtonCountString() // [x]MEMO : 残り全体回数\n現在フェイズ残り指定回数 => 選択対象の名前 // [ ]MEMO : SHRではシェリフと同じように名前で表示
    {
        string remainingCountText = $"{ModTranslation.GetString("MedicalTechnologistAbilityRemainingCount")}{AbilityRemainingCount}";
        string targetText = $"{ModTranslation.GetString("MedicalTechnologistSelectTarget")}";

        string mark = ModHelpers.Cs(new Color(179f / 255f, 0f, 0f), " \u00A9"); // © => 赤血球
        string targetInfoText =
            SampleCrews.FirstCrew == byte.MaxValue && SampleCrews.SecondCrew == byte.MaxValue
                ? $"{ModTranslation.GetString("MedicalTechnologistUnselected")}" // 未選択
                : SampleCrews.FirstCrew != byte.MaxValue && SampleCrews.SecondCrew == byte.MaxValue
                    ? $"{mark}" // 一人選択済み
                    : $"{mark}{mark}"; // 対象選択完了

        string infoText = AbilityRemainingCount > 0 || !(SampleCrews.FirstCrew == byte.MaxValue && SampleCrews.SecondCrew == byte.MaxValue)
            ? $"{remainingCountText}\n{targetText}{targetInfoText}"
            : $"{remainingCountText}";

        return infoText;
    }
    private bool OnCouldUse() => AbilityRemainingCount > 0 && (SampleCrews.FirstCrew == byte.MaxValue || SampleCrews.SecondCrew == byte.MaxValue);
    private List<PlayerControl> SetTargetUntargetPlayer()
    {
        List<PlayerControl> untargetPlayer = new();
        if (SampleCrews.FirstCrew != byte.MaxValue) untargetPlayer.Add(ModHelpers.PlayerById(SampleCrews.FirstCrew));
        if (SampleCrews.SecondCrew != byte.MaxValue) untargetPlayer.Add(ModHelpers.PlayerById(SampleCrews.SecondCrew));
        return untargetPlayer;
    }

    // INameHandler
    public void OnHandleName()
    {
        string suffix = ModHelpers.Cs(new Color(179f / 255f, 0f, 0f), " \u00A9"); // © => 赤血球

        if (SampleCrews.FirstCrew != byte.MaxValue)
        {
            PlayerControl first = ModHelpers.PlayerById(SampleCrews.FirstCrew);
            SetNamesClass.SetPlayerNameText(first, $"{first.NameText().text}{suffix}");
        }
        if (SampleCrews.SecondCrew != byte.MaxValue)
        {
            PlayerControl Second = ModHelpers.PlayerById(SampleCrews.SecondCrew);
            SetNamesClass.SetPlayerNameText(Second, $"{Second.NameText().text}{suffix}");
        }
    }

    // IMeetingHandler
    public void StartMeeting() // [x]MEMO : 後で自分のRoleBaseのみに処理を掛ける方法聞く => OK
    {
        string infoName = ModTranslation.GetString("MedicalTechnologistName");
        string infoContents;

        if (AmongUsClient.Instance.AmHost && Mode.ModeHandler.IsMode(Mode.ModeId.SuperHostRoles))
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.IsMod()) continue; // 導入ゲストは自分で処理させる, 自分自身が臨床検査技師の時は導入ゲストと同様以下の分岐で処理をする。

                MedicalTechnologist roleBase = player.GetRoleBase<MedicalTechnologist>();
                if (roleBase == null) continue;
                if (roleBase.SampleCrews.FirstCrew == byte.MaxValue || roleBase.SampleCrews.SecondCrew == byte.MaxValue) continue;

                PlayerControl firstCrew = ModHelpers.PlayerById(roleBase.SampleCrews.FirstCrew);
                PlayerControl secondCrew = ModHelpers.PlayerById(roleBase.SampleCrews.SecondCrew);

                infoContents = SampleTestResultsText(firstCrew, secondCrew);
                AddChatPatch.ChatInformation(player, infoName, infoContents, "#259f94");
            }
        }
        if (PlayerControl.LocalPlayer.IsRole(RoleId.MedicalTechnologist))
        {
            MedicalTechnologist roleBase = PlayerControl.LocalPlayer.GetRoleBase<MedicalTechnologist>();
            if (roleBase == null) return;
            if (roleBase.SampleCrews.FirstCrew == byte.MaxValue || roleBase.SampleCrews.SecondCrew == byte.MaxValue) return;

            PlayerControl firstCrew = ModHelpers.PlayerById(roleBase.SampleCrews.FirstCrew);
            PlayerControl secondCrew = ModHelpers.PlayerById(roleBase.SampleCrews.SecondCrew);

            infoContents = SampleTestResultsText(firstCrew, secondCrew);
            AddChatPatch.ChatInformation(PlayerControl.LocalPlayer, infoName, infoContents, "#259f94", true);
        }
    }
    public void CloseMeeting() { }

    // ICheckMurderHandler
    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        return true; // [ ]MEMO : クールはリセットしたい。~> キルを守護で防ぐ事が必要?
    }

    // Custom

    /// <summary>
    /// 採取対象の検体を解析し, 組織適合検査の検査結果を文字列で取得する。
    /// </summary>
    /// <param name="firstCrew">採取対象 1人目</param>
    /// <param name="secondCrew">採取対象 2人目</param>
    /// <returns>string : 検査結果</returns>
    private static string SampleTestResultsText(PlayerControl firstCrew, PlayerControl secondCrew)
    {
        string samplePresentation;
        string resultText;
        string judgmentSystemText = $"{ModTranslation.GetString("MedicalTechnologistJudgmentSystem")}\n<pos=10%>{GetJudgmentSystemText()}</pos>";

        if (firstCrew != null && secondCrew != null)
        {
            SampleAnalysisData firstSample = new(firstCrew);
            SampleAnalysisData secondSample = new(secondCrew);

            SampleAnalysisData.ResultsType result = GetResultsInfo(firstSample, secondSample);

            samplePresentation = string.Format(ModTranslation.GetString("MedicalTechnologistSamplePresentation"), firstCrew.name, secondCrew.name);

            if (result == SampleAnalysisData.ResultsType.Conform) // 適合
            {
                resultText = $"{ModTranslation.GetString("MedicalTechnologistSampleResult")}\n{ModTranslation.GetString("MedicalTechnologistMatch")}";
            }
            else if (result == SampleAnalysisData.ResultsType.Rejection) // 反発
            {
                resultText = $"{ModTranslation.GetString("MedicalTechnologistSampleResult")}\n{ModTranslation.GetString("MedicalTechnologistMismatch")}";
            }
            else // 呼ばれる事は無い(はずの)フレーバーテキスト
            {
                resultText = $"<pos=10%>Please report to the SuperNewRoles developer.</pos>\n<pos=10%>ErrorCode : 0b_{Convert.ToString((int)JudgmentSystem, 2)}</pos>";
                Logger.Error($"JudgmentSystem に 不正な値が代入されています。 => JudgmentSystem = {JudgmentSystem}", Roleinfo.NameKey);
            }
        }
        else // 呼ばれる事は無い(はずの)フレーバーテキスト
        {
            samplePresentation = string.Format(ModTranslation.GetString("MedicalTechnologistSamplePresentation"), firstCrew != null ? firstCrew.name : "null", secondCrew != null ? secondCrew.name : "null");
            resultText = "<pos=10%>検体不適正 ( 不合格検体 )</pos>\n<pos=10%>検体が適切に提出なされなかった為,</pos>\n<pos=10%>陣営の判別が行えませんでした。</pos>";
            Logger.Error("検体が揃っていないにも関わらず, 提出されました。", Roleinfo.NameKey);
        }

        return $"{judgmentSystemText}\n\n{samplePresentation}\n\n{resultText}";
    }

    /// <summary>
    /// 判定方式を取得し, プレイヤー向けの説明テキスト(フレーバーテキスト)に変換する。
    /// </summary>
    /// <returns></returns>

    private static string GetJudgmentSystemText()
    {
        return JudgmentSystem switch
        {
            JudgmentType.None => ModTranslation.GetString("MedicalTechnologistNoneSystem"),
            JudgmentType.TeamType => ModTranslation.GetString("MedicalTechnologistTeamTypeSystem"),
            JudgmentType.DetailedNeutral => ModTranslation.GetString("MedicalTechnologistDetailedNeutralSystem"),
            JudgmentType.TeamType | JudgmentType.DetailedNeutral => ModTranslation.GetString("MedicalTechnologistDetailedNeutralBothSystem"),
            _ => "System Error",
        };
    }

    /// <summary>
    /// 検体の解析データを使用して 組織適合検査を行い, 検査結果を取得する。
    /// </summary>
    /// <param name="firstSample">検体 1</param>
    /// <param name="secondSample">検体 2</param>
    /// <returns>検体の解析結果 => ResultsType.Conform : 適合(同陣営), ResultsType.Rejection : 反発(別陣営)</returns>
    private static SampleAnalysisData.ResultsType GetResultsInfo(SampleAnalysisData firstSample, SampleAnalysisData secondSample)
    {
        SampleAnalysisData.ResultsType result;
        const SampleAnalysisData.DetailedTeamRoleType defaultNeutral = SampleAnalysisData.DetailedTeamRoleType.DefaultNeutral;

        switch (JudgmentSystem)
        {
            case JudgmentType.None | JudgmentType.None: // アサイン分類のみで判定
                result = firstSample.AssignTeamType == secondSample.AssignTeamType ? SampleAnalysisData.ResultsType.Conform : SampleAnalysisData.ResultsType.Rejection;
                break;
            case JudgmentType.TeamType | JudgmentType.None: // 通常( + モデファイラ)の勝利陣営分類で判定
                result = firstSample.RoleAndModifierTeamType == secondSample.RoleAndModifierTeamType ? SampleAnalysisData.ResultsType.Conform : SampleAnalysisData.ResultsType.Rejection;
                break;
            case JudgmentType.None | JudgmentType.DetailedNeutral:

                SampleAnalysisData.DetailedTeamRoleType firstSampleType = firstSample.AssignTeamType != TeamRoleType.Neutral // 検体 1 のデータを処理する。
                    ? (SampleAnalysisData.DetailedTeamRoleType)firstSample.AssignTeamType   // 陣営(アサイン分類)が 第三陣営でない場合, [ 陣営(アサイン分類) ] をDetailedTeamRoleTypeに変換して検査に用いる。
                    : firstSample.DetailedTeamType;                                         // 陣営(アサイン分類)が 第三陣営の場合, [ 陣営(詳細勝利陣営分類) ] を変換せず検査に用いる。

                SampleAnalysisData.DetailedTeamRoleType secondSampleType = secondSample.AssignTeamType != TeamRoleType.Neutral // 検体 2 のデータを処理する。
                    ? (SampleAnalysisData.DetailedTeamRoleType)secondSample.AssignTeamType  // 陣営(アサイン分類)が 第三陣営でない場合, [ 陣営(アサイン分類) ] をDetailedTeamRoleTypeに変換して検査に用いる。
                    : secondSample.DetailedTeamType;                                        // 陣営(アサイン分類)が 第三陣営の場合, [ 陣営(詳細勝利陣営分類) ] を変換せず検査に用いる。

                result = firstSampleType == defaultNeutral && secondSampleType == defaultNeutral // 第三陣営同士は, 別陣営と判定する。
                    ? SampleAnalysisData.ResultsType.Rejection
                    : firstSampleType == secondSampleType
                        ? SampleAnalysisData.ResultsType.Conform
                        : SampleAnalysisData.ResultsType.Rejection;
                break;
            case JudgmentType.TeamType | JudgmentType.DetailedNeutral: // 詳細な陣営分類で判定
                result = firstSample.DetailedTeamType == defaultNeutral && secondSample.DetailedTeamType == defaultNeutral // 第三陣営同士は, 別陣営と判定する。
                    ? SampleAnalysisData.ResultsType.Rejection
                    : firstSample.DetailedTeamType == secondSample.DetailedTeamType
                        ? SampleAnalysisData.ResultsType.Conform
                        : SampleAnalysisData.ResultsType.Rejection;
                break;
            default:
                result = SampleAnalysisData.ResultsType.Error;
                break;
        }

        return result;
    }

    private class SampleAnalysisData
    {
        /// <summary>
        /// 陣営 (アサイン分類)
        /// </summary>
        public TeamRoleType AssignTeamType;
        /// <summary>
        /// 陣営 (役職 及び モデファイラの 勝利陣営分類)
        /// </summary>
        public TeamType RoleAndModifierTeamType;
        /// <summary>
        /// 詳細な陣営
        /// </summary>
        public DetailedTeamRoleType DetailedTeamType;

        /// <summary>
        /// 勝利陣営の詳細な情報を表す。
        /// クルーメイト, インポスター, (詳細情報無し) 第三陣営は [ TeamType ] からの変換を可能とする。
        /// </summary>
        public enum DetailedTeamRoleType
        {
            Crewmate = TeamType.Crewmate,
            Impostor = TeamType.Impostor,
            DefaultNeutral = TeamType.Neutral, // チームを組まない第三陣営
            Lovers,
            Quarreled,
            Jackal,
            Pavlovsdogs,
            Fox,
            ThreeLittlePig,
        }

        public enum ResultsType
        {
            Error,
            Conform,    // 適合 (反応無し)
            Rejection,  // 拒絶
        }

        /// <summary>
        /// 検体の解析データ
        /// </summary>
        /// <param name="Sample">解析対象の検体</param>
        public SampleAnalysisData(PlayerControl Sample)
        {
            TeamRoleType teamRoleType = CustomRoles.GetRoleTeam(Sample);
            TeamType roleAndModifierTeamType = GetRoleAndModifierTeamType(Sample);
            DetailedTeamRoleType detailedTeamRoleType = GetDetailedTeamType(Sample, roleAndModifierTeamType);

            this.AssignTeamType = teamRoleType;
            this.RoleAndModifierTeamType = roleAndModifierTeamType;
            this.DetailedTeamType = detailedTeamRoleType;
        }

        static TeamType GetRoleAndModifierTeamType(PlayerControl Sample)
        {
            TeamType roleTeamType = CustomRoles.GetRoleTeamType(Sample); // マッドはImpostor, フレンズ及び式神はこの時点ではDefaultNeutral になる
            TeamType ModifierTeamType = roleTeamType;

            // TeamRoleType 及び, TeamType では, 判別できない役職の陣営を取得する。
            if (Sample.IsRole(RoleId.ShermansServant)) { ModifierTeamType = TeamType.Crewmate; };

            // moderator の陣営を取得する。
            if (Sample.IsLovers()) { ModifierTeamType = TeamType.Neutral; } // TODO : ラバーズリワークの影響を受ける。現在強制的に第三陣営判定しているリワーク後は ラバーズの設定 : [第三陣営として配役する] に従うように変更。
            if (Sample.IsQuarreled()) { ModifierTeamType = TeamType.Neutral; }

            TeamType roleAndModifierTeamType = roleTeamType == ModifierTeamType ? roleTeamType : ModifierTeamType;

            return roleAndModifierTeamType;
        }

        static DetailedTeamRoleType GetDetailedTeamType(PlayerControl Sample, TeamType roleAndModifierTeamType) =>
            roleAndModifierTeamType == TeamType.Neutral
                ? GetDetailedNeutralType(Sample) // 勝利陣営分類が第三陣営の時, 第三陣営の詳細な陣営を取得する。
                : (DetailedTeamRoleType)roleAndModifierTeamType; // 勝利陣営分類が第三陣営以外の時, 役職及びモデファイラの勝利陣営分類を詳細な陣営に変換する。

        /// <summary>
        /// 第三陣営の詳細な勝利陣営を取得する。
        /// チームを組む第三陣営が増えた場合追記必須。
        /// </summary>
        /// <param name="Sample">解析対象の検体</param>
        /// <returns>DetailedTeamRoleType : 第三陣営の詳細な勝利陣営</returns>
        static DetailedTeamRoleType GetDetailedNeutralType(PlayerControl Sample)
        {
            // RoleIdで判定できない役を判定する
            if (Sample.IsLovers()) return DetailedTeamRoleType.Lovers; // TODO : ラバーズリワークの影響を受ける。別のラバーズを別陣営判定にするかはリワーク後, 要相談。
            if (Sample.IsQuarreled()) return DetailedTeamRoleType.Quarreled;

            // Jackalは別判定がある為, そちらを使う
            if (Sample.IsJackalTeam()) return DetailedTeamRoleType.Jackal;

            // or 使ってないのは見た目の問題と,"ここに書く役そこまでないのに, orで処理を重くして迄, 省略する意味無くないか"と考えた為。
            //必要ならorに変更して下さい。
            return Sample.GetRole() switch
            {
                RoleId.Pavlovsdogs => DetailedTeamRoleType.Pavlovsdogs,
                RoleId.Pavlovsowner => DetailedTeamRoleType.Pavlovsdogs,
                RoleId.Fox => DetailedTeamRoleType.Fox,
                RoleId.FireFox => DetailedTeamRoleType.Fox,
                RoleId.TheFirstLittlePig => DetailedTeamRoleType.ThreeLittlePig,
                RoleId.TheSecondLittlePig => DetailedTeamRoleType.ThreeLittlePig,
                RoleId.TheThirdLittlePig => DetailedTeamRoleType.ThreeLittlePig,
                _ => DetailedTeamRoleType.DefaultNeutral
            };
        }
    }
}