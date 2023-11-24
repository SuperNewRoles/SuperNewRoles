using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Buttons;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases;
public enum CustomButtonCouldType
{
    Always =    0x001, //1
    CanMove =   0x002, //2
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
    public bool HasAbility { get; }
    public int AbilityCount { get; private set; }
    //InfoText

    /// <summary>
    /// CustomButtonInfo
    /// </summary>
    /// <param name="OnClick">クリックした時の処理</param>
    /// <param name="HasButton">ボタンを表示するか</param>
    /// <param name=""></param>
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
        Action OnEffectEnds = null)
    {
        this.HasAbility = false;
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
        this.ButtonText = buttonText;
        this.showButtonText = showButtonText;
        this.GetCoolTimeFunc = CoolTime;
        this.GetDurationTimeFunc = DurationTime;
        this.OnEffectEndsFunc = OnEffectEnds;
        if (this.BaseButton == null)
            this.BaseButton = FastDestroyableSingleton<HudManager>.Instance.AbilityButton;
        this.HotKey = HotKey;
        if (joystickKey.HasValue) {
            this.joystickKey = joystickKey.Value;
        }
        else if (this.HotKey.HasValue) {
            this.joystickKey = JoystickKeys.TryGetValue(HotKey.Value, out int joykey) ? joykey : -1;
        }
        GetOrCreateButton();
    }
    public void OnClick()
    {
        if (HasAbility)
            AbilityCount--;
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
        return roleBase?.Player?.PlayerId == PlayerControl.LocalPlayer.PlayerId &&
            (HasButtonFunc?.Invoke(IsAlive) ?? true);
    }
    public bool CouldUse()
    {
        //AbilityButtonかつ残り回数が0なら
        if (HasAbility && AbilityCount <= 0)
            return false;
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
        return CurrentTarget = HudManagerStartPatch.SetTarget(UntargetPlayer?.Invoke(), TargetCrewmateOnly?.Invoke() ?? false);
    }
}
