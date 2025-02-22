using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Roles.Ability.CustomButton;

public abstract class CustomMeetingButtonBase : AbilityBase
{
    //エフェクトがある(≒押したらカウントダウンが始まる？)ボタンの場合は追加でIButtonEffectを継承すること
    //奪える能力の場合はIRobableを継承し、Serializer/DeSerializerを実装
    private EventListener<MeetingStartEventData> startMeetingEvent;
    private EventListener closeMeetingEvent;
    public abstract Sprite Sprite { get; }
    private static readonly Color GrayOut = new(1f, 1f, 1f, 0.3f);
    public abstract bool HasButtonLocalPlayer { get; }

    private Dictionary<byte, GameObject> targetButtons;
    private Dictionary<byte, SpriteRenderer> targetButtonSprites;

    public abstract bool CheckIsAvailable(ExPlayerControl player);
    public abstract bool CheckHasButton(ExPlayerControl player);

    public abstract void OnClick();

    public virtual ActionButton textTemplate => HudManager.Instance.AbilityButton;

    public CustomMeetingButtonBase() { }

    public override void AttachToLocalPlayer()
    {
        startMeetingEvent = MeetingStartEvent.Instance.AddListener(OnStartMeeting);
        closeMeetingEvent = MeetingCloseEvent.Instance.AddListener(OnCloseMeeting);
    }
    private void OnStartMeeting(MeetingStartEventData data)
    {
        GenerateButton();
    }
    private void OnCloseMeeting()
    {
        DestroyAllButton();
    }
    private void DestroyAllButton()
    {
        foreach (var button in targetButtons)
        {
            if (button.Value == null) continue;
            UnityEngine.Object.Destroy(button.Value);
        }
    }
    private void GenerateButton()
    {
        if (targetButtons.Count > 0)
        {
            foreach (var button in targetButtons)
            {
                UnityEngine.Object.Destroy(button.Value);
            }
            targetButtons.Clear();
        }
        if (MeetingHud.Instance == null) return;
        foreach (var player in MeetingHud.Instance.playerStates)
        {
            if (player == null || player.AmDead) continue;
            ExPlayerControl exPlayer = (ExPlayerControl)player;
            if (exPlayer == null) continue;
            if (!HasButtonLocalPlayer && exPlayer.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
            if (!CheckHasButton(exPlayer)) continue;

            GameObject template = player.Buttons.transform.Find("CancelButton").gameObject;
            GameObject targetBox = UnityEngine.Object.Instantiate(template, player.transform);
            targetBox.name = "TargetButton";
            targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
            SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
            renderer.sprite = Sprite;
            PassiveButton button = targetBox.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((Action)(() => OnClickEvent(exPlayer, targetBox)));
            targetButtons[exPlayer.PlayerId] = targetBox;
            targetButtonSprites[exPlayer.PlayerId] = renderer;
        }
    }

    public virtual void OnMeetingUpdate()
    {
        if (PlayerControl.LocalPlayer?.Data == null || ExileController.Instance)
        {
            SetActiveAll(false);
            return;
        }
        foreach (var button in targetButtons)
        {
            if (button.Value == null || !button.Value.activeSelf) continue;
            ExPlayerControl exPlayer = ExPlayerControl.ById(button.Key);
            if (exPlayer == null) continue;
            bool hasButton = CheckHasButton(exPlayer);
            SetActive(button.Value, hasButton);
            if (hasButton && targetButtonSprites.TryGetValue(button.Key, out var sprite))
                SetEnabled(sprite, CheckIsAvailable(exPlayer));
        }
    }

    public void OnClickEvent(ExPlayerControl exPlayer, GameObject button)
    {
        if (CheckHasButton(exPlayer) && CheckIsAvailable(exPlayer))
        {
            this.OnClick();
        }
    }
    private void SetEnabled(SpriteRenderer button, bool isEnabled)
    {
        button.material.color = isEnabled ? Color.white : GrayOut;
    }
    private void SetActiveAll(bool isActive)
    {
        if (isActive)
        {
            foreach (var button in targetButtons)
            {
                SetActive(button.Value, isActive);
            }
        }
        else
        {
            foreach (var button in targetButtons)
            {
                SetActive(button.Value, isActive);
            }
        }
    }
    private void SetActive(GameObject button, bool isActive)
    {
        if (button == null && button.activeSelf == isActive) return;
        button.SetActive(isActive);
    }
    public override void Detach()
    {
        base.Detach();
        MeetingStartEvent.Instance.RemoveListener(startMeetingEvent);
    }
}
