using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
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
public abstract class CustomButtonBase : AbilityBase
{
    //エフェクトがある(≒押したらカウントダウンが始まる？)ボタンの場合は追加でIButtonEffectを継承すること
    //奪える能力の場合はIRobableを継承し、Serializer/DeSerializerを実装
    private EventListener hudUpdateEvent;
    private EventListener<WrapUpEventData> wrapUpEvent;
    private ActionButton actionButton;
    private IButtonEffect buttonEffect;
    public virtual float Timer { get; set; }
    public abstract float DefaultTimer { get; }
    public abstract string buttonText { get; }

    public abstract Sprite Sprite { get; }
    private static readonly Color GrayOut = new(1f, 1f, 1f, 0.3f);

    //TODO:未実装
    //Updateで感知するよりも、button押したのをトリガーにするべきな気がするけどそれは可能か？
    protected abstract KeyCode? hotkey { get; }
    // protected abstract int joystickkey { get; }

    public abstract bool CheckIsAvailable();
    public virtual bool CheckHasButton() => !PlayerControl.LocalPlayer.Data.IsDead;

    public abstract void OnClick();
    public virtual void OnMeetingEnds() { ResetTimer(); }

    /// <summary>
    /// カウントを進めるかの判定
    /// デフォルトのままだとベント内もしくは移動不可の時はカウントが進まない
    /// 他の条件を付けたければこれをoverrideすること
    /// </summary>
    /// <returns>trueならカウントが進む</returns>
    public virtual bool CheckDecreaseCoolCount()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var moveable = localPlayer.CanMove;

        return !localPlayer.inVent && moveable;
    }

    /// <summary>
    /// カウントの進み方を変えたい場合はここをoverrideして変更すること
    /// </summary>
    public virtual void DecreaseTimer()
    {
        Timer -= Time.deltaTime;
    }

    public virtual ActionButton textTemplate => HudManager.Instance.AbilityButton;

    public CustomButtonBase() { }

    public override void AttachToLocalPlayer()
    {
        actionButton = UnityEngine.Object.Instantiate(textTemplate, textTemplate.transform.parent);
        PassiveButton button = actionButton.GetComponent<PassiveButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.Colliders = new Collider2D[] { button.GetComponent<BoxCollider2D>() };
        if (actionButton.usesRemainingText != null) actionButton.usesRemainingText.transform.parent.gameObject.SetActive(false);
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => OnClickEvent()));

        if (textTemplate)
        {
            UnityEngine.Object.Destroy(actionButton.buttonLabelText);
            actionButton.buttonLabelText = UnityEngine.Object.Instantiate(textTemplate.buttonLabelText, actionButton.transform);
        }
        SetActive(false);
        hudUpdateEvent = HudUpdateEvent.Instance.AddListener(OnUpdate);
        wrapUpEvent = WrapUpEvent.Instance.AddListener(x => OnMeetingEnds());
        ResetTimer();
    }
    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);
        buttonEffect = this as IButtonEffect;
    }

    public virtual void OnUpdate()
    {
        if (PlayerControl.LocalPlayer?.Data == null || MeetingHud.Instance || ExileController.Instance || !CheckHasButton())
        {
            SetActive(false);
            return;
        }
        SetActive(HudManager.Instance.UseButton.isActiveAndEnabled || HudManager.Instance.PetButton.isActiveAndEnabled);
        if (Timer > 0 && CheckDecreaseCoolCount()) DecreaseTimer();
        actionButton.graphic.sprite = Sprite;
        //エフェクト中は直後のbuttonEffect.Updateで表記が上書きされる……はず
        actionButton.SetCoolDown(Timer, float.MaxValue);
        actionButton.OverrideText(buttonText);
        if (CheckIsAvailable() && (buttonEffect == null || !buttonEffect.isEffectActive))
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
            actionButton.graphic.material.SetFloat("_Desat", 0f);
            if (Input.GetKeyDown(hotkey ?? KeyCode.None))
            {
                OnClickEvent();
            }
        }
        else if (buttonEffect != null && buttonEffect.isEffectActive && buttonEffect.effectCancellable)
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.EnabledColor;
            actionButton.graphic.material.SetFloat("_Desat", 0f);
            if (Input.GetKeyDown(hotkey ?? KeyCode.None))
            {
                buttonEffect.OnCancel(actionButton);
                ResetTimer();
            }
        }
        else
        {
            actionButton.graphic.color = actionButton.buttonLabelText.color = Palette.DisabledClear;
            actionButton.graphic.material.SetFloat("_Desat", 1f);
        }
        //以下はエフェクトがある(≒押したらカウントダウンが始まる)ときだけ呼ばれる
        if (buttonEffect != null) buttonEffect.OnFixedUpdate(actionButton);
    }

    public void OnClickEvent()
    {
        if (this.Timer <= 0f && CheckIsAvailable() && (buttonEffect == null || !buttonEffect.isEffectActive))
        {
            actionButton.graphic.color = GrayOut;
            this.OnClick();
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
        Timer = DefaultTimer;
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        HudUpdateEvent.Instance.RemoveListener(hudUpdateEvent);
        WrapUpEvent.Instance.RemoveListener(wrapUpEvent);
        GameObject.Destroy(actionButton.gameObject);
    }
}
