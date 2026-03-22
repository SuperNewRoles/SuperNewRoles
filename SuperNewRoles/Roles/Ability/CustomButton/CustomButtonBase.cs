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
using SuperNewRoles.CustomOptions.Categories;
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
    /// <summary>非キラー & キラー : ActiveAbility_1 (Fキー動作)</summary>
    Ability1,
    /// <summary>非キラー : ActiveAbility_2 (Qキー動作)</summary>
    Ability2,
    /// <summary>非キラー : ActiveAbility_3 / キラー ActiveAbility_2 (ショートカット無し)</summary>
    Ability3,
    Vent,
    Use,
    Report,
    Sabotage
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
            KeyType.Ability2 => KeyCode.Q,
            KeyType.Ability3 => KeyCode.None,
            KeyType.Vent => KeyCode.V,
            _ => throw new Exception($"keyTypeが{keyType}の場合はGetKeyCodeを実装してください"),
        };
    }

    private static int GetJoystickKey(KeyType keyType)
    {
        return keyType switch
        {
            KeyType.None => -1, // Rewired.Player.GetButtonDownは 0未満が引数として渡された時falseを返す為 -1
            KeyType.Kill => 8, // PS4 □
            KeyType.Ability1 => 49, // PS4 R2
            KeyType.Ability2 => 8,
            KeyType.Ability3 => -1,
            KeyType.Vent => 50, // PS4 R1 (Impostor Vent)
            KeyType.Use => 6,  // PS4 ×
            KeyType.Report => 7, // PS4 △
            KeyType.Sabotage => 4, // PS4 L2
            _ => throw new Exception($"keyTypeが{keyType}の場合はGetJoystickKeyを実装してください"),
        };

        // その他コントローラー - ショートカットキー対応
        // Tab => 5(PS4 L1)
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
        // --- 事前チェック: ボタンを表示すべきでない状況なら即座に処理を中断 ---
        if (PlayerControl.LocalPlayer?.Data == null || MeetingHud.Instance || ExileController.Instance || !CheckHasButton())
        {
            SetActive(false);
            return;
        }
        // 他のUIが表示されているかどうかに基づいてボタンの表示/非表示を切り替え
        bool active = HudManager.Instance.UseButton.isActiveAndEnabled || HudManager.Instance.PetButton.isActiveAndEnabled;
        SetActive(active);

        // --- タイマー更新 ---
        // クールダウン中で、かつ特殊なエフェクトが作動していない場合にのみタイマーを進める
        if (Timer > 0f && (Timer == MaybeZero || CheckDecreaseCoolCount()) && buttonEffect?.isEffectActive != true)
        {
            DecreaseTimer();
        }

        // --- UIの基本設定 ---
        actionButton.graphic.sprite = Sprite; // スプライトを設定
        actionButton.OverrideText(buttonText); // ボタン下部のテキストを設定
        // クールダウンシェーダーがパイチャートを正しく描画するために不可欠なヘルパー
        CooldownHelpers.SetCooldownNormalizedUvs(actionButton.graphic);

        // --- ボタンの状態を判定 ---
        bool isCoolingDown = Timer > 0f;
        bool canUseByCondition = CheckIsAvailable(); // クールダウン以外の使用条件をチェック
        bool canUseNow = !isCoolingDown && canUseByCondition;
        bool isEffectActive = buttonEffect?.isEffectActive ?? false;

        // --- パイチャートと数字の表示更新 ---
        // この処理は常に呼び出す。不要な場合はSetCoolDownメソッド内部で非表示になる。
        actionButton.SetCoolDown(Timer, DefaultTimerAdjusted);

        // --- 状態に基づいたボタンの見た目と入力処理 ---
        if (isEffectActive)
        {
            // ケース1: IButtonEffectが作動中の場合 (例: 透明化の効果時間中など)
            if (buttonEffect.effectCancellable && buttonEffect.IsEffectAvailable())
            {
                // キャンセル可能な状態 (カラー表示)
                actionButton.graphic.color = Palette.EnabledColor;
                actionButton.graphic.material.SetFloat("_Desat", 0f); // 彩度を最大 (カラー)

                // キー入力によるキャンセル処理
                int joyKey = GetJoystickKey(keytype);
                KeyCode kCode = GetKeyCode(keytype);
                if ((kCode != KeyCode.None && Input.GetKeyDown(kCode)) || (joyKey >= 0 && ConsoleJoystick.player.GetButtonDown(joyKey)))
                {
                    buttonEffect.OnCancel(actionButton);
                    ResetTimer();
                }
            }
        }
        else if (canUseNow)
        {
            // ケース2: 使用可能な状態 (クールダウン完了 & 条件OK)
            // ボタンを鮮やかなカラーで表示
            actionButton.graphic.color = Palette.EnabledColor;
            actionButton.buttonLabelText.color = Palette.EnabledColor;
            actionButton.graphic.material.SetFloat("_Desat", 0f); // 彩度を最大 (カラー)

            // キー入力による使用処理
            int joyKey = GetJoystickKey(keytype);
            KeyCode kCode = GetKeyCode(keytype);
            if ((kCode != KeyCode.None && Input.GetKeyDown(kCode)) || (joyKey >= 0 && ConsoleJoystick.player.GetButtonDown(joyKey)))
            {
                OnClickEvent();
            }
        }
        else
        {
            // ケース3: 使用不可能な状態 (クールダウン中、または条件NG)
            // テキストの色を無効状態の色に設定
            actionButton.buttonLabelText.color = Palette.DisabledClear;

            if (isCoolingDown)
            {
                // サブケース3-1: クールダウン中の場合
                if (canUseByCondition)
                {
                    // 条件を満たしている場合 (例: ターゲットが範囲内)
                    // ボタンを【不透明なカラー】にして、パイチャートを表示する。
                    actionButton.graphic.color = Palette.EnabledColor;
                    actionButton.graphic.material.SetFloat("_Desat", 0f); // カラー
                }
                else
                {
                    // 条件を満たしていない場合 (例: ターゲットが範囲外)
                    // ボタン全体を【半透明のモノクロ】にする。
                    actionButton.graphic.color = GrayOut; // 半透明
                    actionButton.graphic.material.SetFloat("_Desat", 1f); // モノクロ
                }
            }
            else // クールダウンは完了したが、canUseByConditionがfalseの場合
            {
                // サブケース3-2: クールダウンは完了したが、使用条件を満たさない場合
                // ボタン全体を【半透明のモノクロ】にする。
                actionButton.graphic.color = GrayOut;
                actionButton.graphic.material.SetFloat("_Desat", 1f);
            }
        }

        // --- その他のUI更新 ---
        UpdateText(); // 残り回数などのテキストを更新

        // IButtonEffectが作動中なら、その更新処理を呼び出す
        if (isEffectActive)
        {
            buttonEffect.OnFixedUpdate(actionButton);
        }
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
        if (this is TargetCustomButtonBase targetButton && targetButton.ShouldCancelClickForTarget())
            return;
        if (this.Timer <= 0f && CheckIsAvailable() && (buttonEffect == null || !buttonEffect.isEffectActive))
        {
            actionButton.graphic.color = GrayOut;
            this.OnClick();
            this.OnClickEventAction();
            ResetTimer();
            if (buttonEffect != null) buttonEffect.OnClick(actionButton);
        }
        else if (buttonEffect != null && buttonEffect.isEffectActive && buttonEffect.effectCancellable && buttonEffect.IsEffectAvailable())
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
        if (actionButton != null)
        { // タイマーテキストの色をデフォルトに戻し、会議後などに色が緑のまま残ることを防ぐ。
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
        }
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        HudUpdateEvent.Instance.RemoveListener(hudUpdateEvent);
        WrapUpEvent.Instance.RemoveListener(wrapUpEvent);
        GameObject.Destroy(actionButton.gameObject);
    }
    public void SetInitialCooldown()
    {
        switch (GameSettingOptions.InitialCooldown)
        {
            case InitialCooldownType.TenSeconds:
                Timer = 10f;
                break;
            case InitialCooldownType.OneThird:
                Timer = DefaultTimer / 3f;
                break;
            case InitialCooldownType.Immediate:
                Timer = DefaultTimer;
                break;
        }
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
