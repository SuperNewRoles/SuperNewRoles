using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases;
public enum CustomButtonCouldType
{
    Always = 0x001, //1
    CanMove = 0x002, //2
    SetTarget = 0x004, //4
}
public class CustomButtonInfo
{
    public static readonly Dictionary<KeyCode, int> JoystickKeys = new()
    {
        { KeyCode.F, 49 },
        { KeyCode.Q, 8 },
    };

    public PlayerControl CurrentTarget { get; private set; }
    public CustomButton customButton { get; private set; }
    private Action OnClickFunc { get; }
    private Func<bool, bool> HasButtonFunc { get; }
    private CustomButtonCouldType CouldUseType { get; }
    private Func<bool> CouldUseFunc { get; }
    private Action OnMeetingEndsFunc { get; }
    private Func<List<PlayerControl>> UntargetPlayer { get; }
    private Func<bool> TargetCrewmateOnly { get; }
    private Func<bool> StopCountCoolFunc { get; }
    private Func<float> GetCoolTimeFunc { get; }
    private Func<float> GetDurationTimeFunc { get; }
    private Action OnEffectEndsFunc { get; }
    private ActionButton BaseButton { get; }
    private ICustomButton icustomButton { get; set; }
    private RoleBase roleBase { get; set; }
    private Sprite buttonSprite { get; }
    private KeyCode? HotKey { get; }
    private int joystickKey { get; }
    private bool showButtonText { get; }
    private Vector3 positionOffset { get; }
    private string ButtonText { get; }
    private bool HasAbilityCountText { get; }
    private TextMeshPro AbilityCountText { get; set; }
    public TextMeshPro SecondButtonInfoText { get; set; }
    private string AbilityCountTextFormat { get; }
    private int _lastAbilityCount { get; set; }
    public bool HasAbility { get; }
    public int AbilityCount { get; set; }
    //InfoText
    /// <summary>
    /// CustomButtonInfo
    /// </summary>
    /// <param name="AbilityCount">使用可能回数(無限ならnull)</param>
    /// <param name="roleBase">関係しているロルベ(thisでOK)</param>
    /// <param name="OnClick">クリック時の処理</param>
    /// <param name="HasButton">ボタンを表示するか(nullなら自分のときにtrue)</param>
    /// <param name="CouldUseType">使用できるか(一括判定、ビット演算方式)</param>
    /// <param name="OnMeetingEnds">会議終了時の処理(なければnull、クールタイムは自動リセット)</param>
    /// <param name="Sprite">見た目</param>
    /// <param name="CoolTime">クールタイム</param>
    /// <param name="positionOffset">positionOffset</param>
    /// <param name="buttonText">ボタンの表示テキスト(未翻訳のものでもOK)</param>
    /// <param name="HotKey">押す時のキー</param>
    /// <param name="joystickKey">コントローラーのキー</param>
    /// <param name="showButtonText">ボタンの表示テキストを表示するか</param>
    /// <param name="StopCountCoolFunc">クールタイムを止めるか(なければnull)</param>
    /// <param name="baseButton">もとにするActionButton(キルボなど、なければAbilityButton)</param>
    /// <param name="DurationTime">継続時間(継続時間を使わなければnull)</param>
    /// <param name="CouldUse">使用するかのAction(不必要ならnull)</param>
    /// <param name="OnEffectEnds">継続時間が終わった時の処理(なければnull)</param>
    /// <param name="hasSecondButtonInfo">ボタンの情報用テキストを表示するか</param>
    /// <param name="HasAbilityCountText">使用可能回数のテキストを表示するか</param>
    /// <param name="AbilityCountTextFormat">使用可能回数のテキストのフォーマット文(なければ自動)</param>
    /// <param name="SetTargetUntargetPlayer">アビリティの対象に取れないプレイヤー達</param>
    /// <param name="SetTargetCrewmateOnly">アビリティの対象に取れるのはクルーメイト及び第三陣営のみか。 (インポスターを対象に取るのが不可能か。)</param>
    public CustomButtonInfo(
        int? AbilityCount,
        RoleBase roleBase,
        Action OnClick,
        Func<bool, bool> HasButton,
        CustomButtonCouldType CouldUseType,
        Action OnMeetingEnds, Sprite Sprite,
        Func<float> CoolTime,
        Vector3 positionOffset,
        string buttonText,
        KeyCode? HotKey = null,
        int? joystickKey = null,
        bool showButtonText = true,
        Func<bool> StopCountCoolFunc = null,
        ActionButton baseButton = null,
        Func<float> DurationTime = null,
        Func<bool> CouldUse = null,
        Action OnEffectEnds = null,
        bool HasAbilityCountText = false,
        string AbilityCountTextFormat = null,
        Func<List<PlayerControl>> SetTargetUntargetPlayer = null,
        Func<bool> SetTargetCrewmateOnly = null,
        bool hasSecondButtonInfo = false)
    {
        this.HasAbility = AbilityCount != null;
        this.AbilityCount = AbilityCount ?? 334;
        this.OnClickFunc = OnClick;
        this.HasButtonFunc = HasButton;
        this.CouldUseFunc = CouldUse;
        this.roleBase = roleBase;
        this.icustomButton = roleBase as ICustomButton;
        this.CouldUseType = CouldUseType;
        this.OnMeetingEndsFunc = OnMeetingEnds;
        this.buttonSprite = Sprite;
        this.BaseButton = BaseButton;
        this.positionOffset = positionOffset;
        this.StopCountCoolFunc = StopCountCoolFunc;
        this.ButtonText = ModTranslation.GetString(buttonText);
        this.showButtonText = showButtonText;
        this.GetCoolTimeFunc = CoolTime;
        this.GetDurationTimeFunc = DurationTime;
        this.OnEffectEndsFunc = OnEffectEnds;
        this.HasAbilityCountText = HasAbilityCountText;
        if (this.BaseButton == null)
            this.BaseButton = FastDestroyableSingleton<HudManager>.Instance.AbilityButton;
        this.HotKey = HotKey;
        if (joystickKey.HasValue)
        {
            this.joystickKey = joystickKey.Value;
        }
        else if (this.HotKey.HasValue)
        {
            this.joystickKey = JoystickKeys.TryGetValue(HotKey.Value, out int joykey) ? joykey : -1;
        }
        this.TargetCrewmateOnly = SetTargetCrewmateOnly;
        this.UntargetPlayer = SetTargetUntargetPlayer;
        GetOrCreateButton();
        if (HasAbilityCountText)
        {
            if (AbilityCountTextFormat == null)
                this.AbilityCountTextFormat = ModTranslation.GetString("AbilityButtonCountTextFormater");
            else
                this.AbilityCountTextFormat = ModTranslation.GetString(AbilityCountTextFormat);
            AbilityCountText = GameObject.Instantiate(customButton.actionButton.cooldownTimerText, customButton.actionButton.cooldownTimerText.transform.parent);
            AbilityCountText.text = "";
            AbilityCountText.enableWordWrapping = false;
            AbilityCountText.transform.localScale = Vector3.one * 0.5f;
            AbilityCountText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        }
        if (hasSecondButtonInfo)
        {
            SecondButtonInfoText = GameObject.Instantiate(customButton.actionButton.cooldownTimerText, customButton.actionButton.cooldownTimerText.transform.parent);
            SecondButtonInfoText.text = "";
            SecondButtonInfoText.enableWordWrapping = false;
            SecondButtonInfoText.transform.localScale = Vector3.one * 0.5f;
            SecondButtonInfoText.transform.localPosition += !HasAbilityCountText
                ? new Vector3(-0.05f, 0.7f, 0)
                : new Vector3(-0.05f, 1.4f, 0);
        }
    }
    public void UpdateAbilityCountText()
    {
        if (AbilityCountText == null)
            return;
        AbilityCountText.text = string.Format(AbilityCountTextFormat,
            _lastAbilityCount = AbilityCount);
    }
    public void OnClick()
    {
        if (HasAbility)
            AbilityCount--;
        UpdateAbilityCountText();
        OnClickFunc?.Invoke();
        ResetCoolTime();
    }
    public CustomButton GetOrCreateButton()
    {
        if (customButton != null)
            return customButton;
        return customButton = new
            (OnClick, HasButton, CouldUse, OnMeetingEnds,
            buttonSprite, positionOffset,
            FastDestroyableSingleton<HudManager>.Instance,
            BaseButton, HotKey, joystickKey, StopCountCool,
            GetDurationTimeFunc != null, GetDurationTimeFunc?.Invoke() ?? 5f, OnEffectEnds)
        {
            buttonText = ButtonText,
            showButtonText = showButtonText
        };
    }
    public void OnEffectEnds()
    {
        OnEffectEndsFunc?.Invoke();
    }
    public bool StopCountCool()
    {
        return StopCountCoolFunc?.Invoke() ?? false;
    }
    public void ResetCoolTime()
    {
        float CoolTime = GetCoolTimeFunc?.Invoke() ?? 0;
        customButton.MaxTimer = CoolTime;
        customButton.Timer = CoolTime;
        if (GetDurationTimeFunc != null)
        {
            customButton.EffectDuration = GetDurationTimeFunc?.Invoke() ?? 0;
            customButton.HasEffect = true;
        }
    }
    public void OnMeetingEnds()
    {
        OnMeetingEndsFunc?.Invoke();
        ResetCoolTime();
        customButton.isEffectActive = false;
    }
    public bool HasButton(bool IsAlive, RoleId _)
    {
        return roleBase?.Player != null && roleBase.Player.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
            (HasButtonFunc?.Invoke(IsAlive) ?? true);
    }
    public bool CouldUse()
    {
        //AbilityButtonかつ残り回数が0なら
        if (HasAbility && AbilityCount <= 0)
            return false;
        if (_lastAbilityCount != AbilityCount)
            UpdateAbilityCountText();
        //CanMoveを判定するかつCanMoveがfalseなら
        if (CouldUseType.HasFlag(CustomButtonCouldType.CanMove) &&
            !PlayerControl.LocalPlayer.CanMove)
            return false;
        //SetTargetを判定するかつSetTargetがfalseなら
        if (CouldUseType.HasFlag(CustomButtonCouldType.SetTarget) &&
            !SetTarget())
            return false;
        //自前の判定があるならそれを使い、falseならreturn
        if (!(CouldUseFunc?.Invoke() ?? true))
            return false;
        return true;
    }
    /// <summary>
    /// 設定した情報を元に相手をセットする
    /// </summary>
    /// <returns></returns>
    public PlayerControl SetTarget()
    {
        CurrentTarget = HudManagerStartPatch.SetTarget(UntargetPlayer?.Invoke(), TargetCrewmateOnly?.Invoke() ?? false);
        PlayerControlFixedUpdatePatch.SetPlayerOutline(CurrentTarget, roleBase.Roleinfo.RoleColor);
        return CurrentTarget;
    }
}