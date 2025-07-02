using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;
using SuperNewRoles.Roles.Crewmate;
using AmongUs.GameOptions;
using System.Linq;

namespace SuperNewRoles.Roles.Ability;

public record CelebrityData
{
    public bool EnableGlowEffect;
    public bool GlowOnlyWhileAlive;
    public bool YellowChangedRole;
}
public class CelebrityAbility : AbilityBase
{
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    private EventListener _fixedUpdateEvent;
    private EventListener<MeetingStartEventData> _meetingStartEvent;
    private float _glowTimer = 0f;
    private bool _isAlive = true;
    public CelebrityData Data;
    public CelebrityAbility(CelebrityData data)
    {
        Data = data;
    }
    public override void AttachToLocalPlayer()
    {
        _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }
    public override void AttachToAlls()
    {
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _meetingStartEvent = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _isAlive = ExPlayerControl.LocalPlayer.IsAlive();
        if (Data.YellowChangedRole)
            Player.AttachAbility(new AlwaysCelebrityAbility(), new AbilityParentPlayer(Player));
    }
    private void OnMeetingStart(MeetingStartEventData data)
    {
        _glowTimer = 0f;
        _isAlive = Player.IsAlive();
    }
    // 名前の色を更新するイベントハンドラ
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        // 他のプレイヤーから自分がスターと分かるように、スターの名前の色を変更する
        UpdateCelebrityNameColor(data);
    }

    // 固定更新イベントハンドラ - 発光効果を管理
    private void OnFixedUpdate()
    {
        if (!Data.EnableGlowEffect) return;
        if (MeetingHud.Instance != null || ExileController.Instance != null) return;
        // 生存状態の確認
        if (Data.GlowOnlyWhileAlive && !_isAlive)
        {
            // 死亡して発光を止める設定の場合
            return;
        }

        // キルクールタイム経過ごとに発光
        var killCooldown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);

        // キルクールが5秒以下の場合、5秒周期で発光
        float glowInterval = killCooldown <= 5f ? 5f : killCooldown;

        // タイマー処理
        _glowTimer += Time.fixedDeltaTime;
        if (_glowTimer >= glowInterval)
        {
            _glowTimer = 0f;

            // 全員の画面を光らせる
            FlashHandler.RpcShowFlashAll(new Color32(255, 215, 0, 100), 1f);
        }
    }

    // スターの名前の色を更新
    private void UpdateCelebrityNameColor(NameTextUpdateEventData data)
    {
        if (data.Player.Role == RoleId.Celebrity)
            NameText.SetNameTextColor(data.Player, Celebrity.Instance.RoleColor);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateEvent?.RemoveListener();
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateEvent?.RemoveListener();
        _meetingStartEvent?.RemoveListener();
    }
}
public class AlwaysCelebrityAbility : AbilityBase
{
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateEvent?.RemoveListener();
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player != Player) return;
        NameText.SetNameTextColor(Player, Celebrity.Instance.RoleColor);
    }
}