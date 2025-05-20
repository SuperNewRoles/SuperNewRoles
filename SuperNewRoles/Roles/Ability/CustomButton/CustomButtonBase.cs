using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Roles.Ability.CustomButton;

public enum AvailableType
{
    None = 0, // 0
    Always = 0x001, // 1
    CanMove = 0x002, // 2
    SetTarget = 0x004, // 4
    NotNearDoor = 0x008, // 8
    NotMoving = 0x010, // 16
    SetVent = 0x020, // 32
}
public enum ShowTextType
{
    Hidden,
    Show,
    ShowWithCount,
}
public enum KeyType
{
    None,
    Kill,
    Ability1,
    Ability2,
    Vent,
}
public abstract class CustomButtonBase : AbilityBase
{
    //エフェクトがある(≒押したらカウントダウンが始まる？)ボタンの場合は追加でIButtonEffectを継承すること
    //奪える能力の場合はIRobableを継承し、Serializer/DeSerializerを実装
    private EventListener hudUpdateEvent;
    private EventListener<WrapUpEventData> wrapUpEvent;
    public ActionButton actionButton { get; private set; }
    private IButtonEffect buttonEffect;
    public virtual float Timer { get; set; }
    public abstract float DefaultTimer { get; }
    private float DefaultTimerAdjusted => DefaultTimer <= 0f ? MaybeZero : DefaultTimer;
    public abstract string buttonText { get; }
    private const float MaybeZero = 0.001f;
    public abstract Sprite Sprite { get; }
    public virtual Action OnClickEventAction { get; set; } = () => { };
    private static readonly Color GrayOut = new(1f, 1f, 1f, 0.3f);

    public virtual bool IsFirstCooldownTenSeconds => true;

    //TODO:未実装
    //Updateで感知するよりも、button押したのをトリガーにするべきな気がするけどそれは可能か？
    protected abstract KeyType keytype { get; }
    // protected abstract int joystickkey { get; }

    public abstract bool CheckIsAvailable();
    public virtual bool CheckHasButton() => Player == ExPlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead;

    public abstract void OnClick();
    public virtual void OnMeetingEnds() { ResetTimer(); }

    public virtual ShowTextType showTextType { get; } = ShowTextType.Hidden;
    public virtual string showText { get; } = string.Empty;
    private TextMeshPro _text;

    /// <summary>
    /// カウントを進めるかの判定
    /// デフォルトのままだとベント内もしくは移動不可の時はカウントが進まない
    /// 他の条件を付けたければこれをoverrideすること
    /// </summary>
    /// <returns>trueならカウントが進む</returns>
    public virtual bool CheckDecreaseCoolCount()
    {
        // イントロ中はカウントしない
        if (DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed)
            return false;

        var localPlayer = PlayerControl.LocalPlayer;
        var moveable = !PlayerControl.LocalPlayer.inVent && PlayerControl.LocalPlayer.moveable;

        return !localPlayer.inVent && moveable;
    }

    private static KeyCode GetKeyCode(KeyType keyType)
    {
        return keyType switch
        {
            KeyType.None => KeyCode.None,
            KeyType.Kill => KeyCode.Q,
            KeyType.Ability1 => KeyCode.F,
            KeyType.Ability2 => KeyCode.None,
            KeyType.Vent => KeyCode.V,
            _ => throw new Exception($"keyTypeが{keyType}の場合はGetKeyCodeを実装してください"),
        };
    }

    /// <summary>
    /// カウントの進み方を変えたい場合はここをoverrideして変更すること
    /// </summary>
    public virtual void DecreaseTimer()
    {
        Timer -= Time.deltaTime;
    }

    public virtual ActionButton textTemplate { get { return _template ?? _getTemplate(); } }
    private ActionButton _template;
    private ActionButton _getTemplate()
    {
        //_template = HudManager.Instance.KillButton;
        _template = HudManager.Instance.AbilityButton;
        return _template;
    }

    public CustomButtonBase() { }

    public override void AttachToLocalPlayer()
    {
        actionButton = UnityEngine.Object.Instantiate(textTemplate, textTemplate.transform.parent);
        actionButton.graphic.color = Color.white;
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        button.OnClick = new();
        button.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
        if (actionButton.usesRemainingText != null) actionButton.usesRemainingText.transform.parent.gameObject.SetActive(false);
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => OnClickEvent()));

        GenerateText();

        if (textTemplate)
        {
            UnityEngine.Object.Destroy(actionButton.buttonLabelText);
            actionButton.buttonLabelText = UnityEngine.Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
            actionButton.buttonLabelText.transform.localPosition = new(0, -0.56f, -10);
        }
        SetActive(false);
        hudUpdateEvent = HudUpdateEvent.Instance.AddListener(OnUpdate);
        wrapUpEvent = WrapUpEvent.Instance.AddListener(x => OnMeetingEnds());
        ResetTimer();
    }

    private void GenerateText()
    {
        _text = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, actionButton.transform);
        _text.text = "";
        _text.enableWordWrapping = false;
        _text.transform.localScale = Vector3.one * 0.5f;
        _text.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        _text.gameObject.SetActive(true);
        _text.text = "";
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        buttonEffect = this as IButtonEffect;
    }

    public virtual void OnUpdate()
    {
        if (PlayerControl.LocalPlayer?.Data == null || MeetingHud.Instance || ExileController.Instance || !CheckHasButton())
        {
            SetActive(false);
            return;
        }
        bool active = HudManager.Instance.UseButton.isActiveAndEnabled || HudManager.Instance.PetButton.isActiveAndEnabled;
        SetActive(active);
        if (Timer > 0 && (Timer == MaybeZero || CheckDecreaseCoolCount()) && buttonEffect?.isEffectActive != true) DecreaseTimer();
        actionButton.graphic.sprite = Sprite;
        //エフェクト中は直後のbuttonEffect.Updateで表記が上書きされる……はず
        actionButton.SetCoolDown(Timer, DefaultTimerAdjusted);
        actionButton.OverrideText(buttonText);
        if (CheckIsAvailable() && (buttonEffect == null || !buttonEffect.isEffectActive))
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
            actionButton.graphic.material.SetFloat("_Desat", 0f);
            if (Input.GetKeyDown(GetKeyCode(keytype)))
            {
                OnClickEvent();
            }
        }
        else if (buttonEffect != null && buttonEffect.isEffectActive && buttonEffect.effectCancellable)
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
            actionButton.graphic.material.SetFloat("_Desat", 0f);
            if (Input.GetKeyDown(GetKeyCode(keytype)))
            {
                buttonEffect.OnCancel(actionButton);
                ResetTimer();
            }
        }
        else
        {
            actionButton.graphic.color = GrayOut;
            actionButton.buttonLabelText.color = Palette.DisabledClear;
            actionButton.graphic.material.SetFloat("_Desat", 1f);
        }
        UpdateText();
        //以下はエフェクトがある(≒押したらカウントダウンが始まる)ときだけ呼ばれる
        if (buttonEffect != null) buttonEffect.OnFixedUpdate(actionButton);
    }
    private void UpdateText()
    {
        switch (showTextType)
        {
            case ShowTextType.Hidden:
                _text.text = "";
                break;
            case ShowTextType.Show:
                _text.text = showText;
                break;
            case ShowTextType.ShowWithCount:
                _text.text = string.Format(
                    string.IsNullOrEmpty(showText)
                    ? ModTranslation.GetString("RemainingText")
                    : showText, Count);
                break;
            default:
                throw new Exception($"showTextTypeが{showTextType}の場合はshowTextを設定してください");
        }
    }
    public void OnClickEvent()
    {
        if (this.Timer <= 0f && CheckIsAvailable() && (buttonEffect == null || !buttonEffect.isEffectActive))
        {
            actionButton.graphic.color = GrayOut;
            this.OnClick();
            this.OnClickEventAction();
            ResetTimer();
            if (buttonEffect != null) buttonEffect.OnClick(actionButton);
        }
        else if (buttonEffect != null && buttonEffect.isEffectActive && buttonEffect.effectCancellable)
        {
            actionButton.graphic.color = Palette.EnabledColor;
            buttonEffect.OnCancel(actionButton);
            ResetTimer();
        }
    }
    public void SetActive(bool isActive)
    {
        if (isActive)
        {
            if (actionButton.gameObject.activeSelf) return;
            actionButton.gameObject.SetActive(true);
            actionButton.graphic.enabled = true;
        }
        else
        {
            if (!actionButton.gameObject.activeSelf) return;
            actionButton.gameObject.SetActive(false);
            actionButton.graphic.enabled = false;
        }
    }
    public virtual void ResetTimer()
    {
        Timer = DefaultTimerAdjusted;
        if (buttonEffect != null)
        {
            buttonEffect.EffectTimer = buttonEffect.EffectDuration;
            buttonEffect.isEffectActive = false;
        }
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        HudUpdateEvent.Instance.RemoveListener(hudUpdateEvent);
        WrapUpEvent.Instance.RemoveListener(wrapUpEvent);
        GameObject.Destroy(actionButton.gameObject);
    }
    public void SetCoolTenSeconds()
    {
        Timer = 10f;
    }

    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.Update))]
    public class AbilityUpdate
    {
        public static void Postfix(AbilityButton __instance)
        {
            if (PlayerControl.LocalPlayer.Data.Role.IsSimpleRole && __instance.commsDown.active)
            {
                __instance.commsDown.SetActive(false);
            }
        }
    }
}
