using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    private ActionButton actionButton;
    public abstract Vector3 PositionOffset { get; }
    public abstract Vector3 LocalScale { get; }
    public abstract float Timer { get; set; }

    public abstract string buttonText { get; }

    public abstract Sprite Sprite { get; }
    public abstract Color? color { get; }
    private static readonly Color GrayOut = new(1f, 1f, 1f, 0.3f);

    //TODO:未実装
    //Updateで感知するよりも、button押したのをトリガーにするべきな気がするけどそれは可能か？
    protected abstract KeyCode? hotkey { get; }
    protected abstract int joystickkey { get; }

    public abstract bool CheckIsAvailable();
    public abstract bool CheckHasButton();

    public abstract void OnClick();
    public abstract void OnMeetingEnds();

    /// <summary>
    /// カウントを進めるかの判定
    /// デフォルトのままだとベント内もしくは移動不可の時はカウントが進まない
    /// 他の条件を付けたければこれをoverrideすること
    /// </summary>
    /// <returns>trueならカウントが進む</returns>
    public virtual bool CheckDecreaseCoolCount()
    {
        var localPlayer = PlayerControl.LocalPlayer;
        var moveable = localPlayer.moveable;

        return !localPlayer.inVent && moveable;
    }

    /// <summary>
    /// カウントの進み方を変えたい場合はここをoverrideして変更すること
    /// </summary>
    public virtual void DecreaseTimer()
    {
        Timer -= Time.deltaTime;
    }

    public abstract ActionButton textTemplate { get; }

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

        //FixedUpdateEvent.AddListener(OnFixedUpdate)
    }

    public void OnFixedUpdate()
    {
        if (Timer > 0 && CheckDecreaseCoolCount()) DecreaseTimer();

        //エフェクト中は直後のbuttonEffect.Updateで表記が上書きされる……はず
        actionButton.SetCoolDown(Timer, float.MaxValue);

        //以下はエフェクトがある(≒押したらカウントダウンが始まる)ときだけ呼ばれる
        IButtonEffect buttonEffect = this as IButtonEffect;
        if (buttonEffect != null) buttonEffect.OnFixedUpdate(actionButton);
    }

    public void OnClickEvent()
    {
        if (this.Timer <= 0f && CheckIsAvailable())
        {
            actionButton.graphic.color = GrayOut;
            this.OnClick();

            IButtonEffect buttonEffect = this as IButtonEffect;
            if (buttonEffect != null) buttonEffect.OnClick(actionButton);
        }
    }
    public void SetActive(bool isActive)
    {
        if (isActive)
        {
            actionButton.gameObject.SetActive(true);
            actionButton.graphic.enabled = true;
        }
        else
        {
            actionButton.gameObject.SetActive(false);
            actionButton.graphic.enabled = false;
        }
    }
}
