using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class PartTimerAbility : TargetCustomButtonBase
{
    public PartTimerData _data { get; private set; }
    private int _deathTurn;
    public ExPlayerControl _employer { get; private set; }
    private bool _isEmployed => _employer != null;

    private EventListener<WrapUpEventData> _wrapUpListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;
    private EventListener<NameTextUpdateVisiableEventData> _nameTextUpdateVisiableListener;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("PartTimerButton.png");
    public override string buttonText => ModTranslation.GetString("PartTimerButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => CalculateDynamicCooldown();
    public override Color32 OutlineColor => new(0, 255, 0, 255);
    public override bool OnlyCrewmates => false;

    public PartTimerAbility(PartTimerData data)
    {
        _data = data;
        _deathTurn = data.deathTurn;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        _nameTextUpdateVisiableListener = NameTextUpdateVisiableEvent.Instance.AddListener(OnNameTextUpdateVisiable);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _wrapUpListener?.RemoveListener();
        _nameTextUpdateVisiableListener?.RemoveListener();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateListener?.RemoveListener();
    }

    private void OnNameTextUpdateVisiable(NameTextUpdateVisiableEventData data)
    {
        if (!_isEmployed) return;
        if (!_data.canSeeTargetRole) return;
        if (data.Player == _employer)
            NameText.UpdateVisiable(data.Player, true);
    }

    /// <summary>
    /// 生存者数に応じて動的にクールタイムを計算する
    /// 計算式: 現在のCT = 最終CT - (最終CT - 初期CT) * (生存者数 / 試合人数)
    /// </summary>
    private float CalculateDynamicCooldown()
    {
        int alivePlayers = ExPlayerControl.ExPlayerControls.Count(p => p.IsAlive());
        int totalPlayers = ExPlayerControl.ExPlayerControls.Count;

        if (totalPlayers == 0) return _data.initialCooldown;

        float ratio = (float)alivePlayers / totalPlayers;
        return _data.finalCooldown - (_data.finalCooldown - _data.initialCooldown) * ratio;
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (_isEmployed) return;

        RpcEmploy(Target);
        ResetTimer();
    }

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (_isEmployed) return false;
        if (!PlayerControl.LocalPlayer.CanMove || ExPlayerControl.LocalPlayer.IsDead()) return false;
        return true;
    }

    public override bool CheckHasButton()
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && !_isEmployed;
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (Player != ExPlayerControl.LocalPlayer) return;
        if (!Player.IsAlive()) return;

        // 雇用主の生死チェック（会議終了後のみ）
        if (_employer != null && _employer.IsDead())
        {
            _employer = null;
            _deathTurn = _data.deathTurn + 1; // DeathTurnをリセット
        }

        // 無職状態の場合、DeathTurnを減らす
        if (!_isEmployed)
        {
            _deathTurn--;
            if (_deathTurn <= 0)
            {
                // 自殺する
                ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.SuicideSecrets);
                return;
            }
        }

        // NameTextを更新
        NameText.UpdateNameInfo(Player);
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        // フリーター本人の表示
        if (data.Player == Player)
        {
            // 雇用状態と残りターン数を表示
            if (!_isEmployed)
            {
                if (!data.Player.Player.Visible) return;
                var turnText = ModHelpers.Cs(Color.red, $"({_deathTurn})");
                Player.PlayerInfoText.text += turnText;
                if (Player.MeetingInfoText != null)
                    Player.MeetingInfoText.text += turnText;
            }
            else if (_employer.AmOwner)
            {
                setMarkTo(Player);
            }
        }
        else if (Player.AmOwner && _employer == data.Player)
        {
            setMarkTo(data.Player);
        }
    }
    private void setMarkTo(ExPlayerControl player)
    {
        var partTimerSymbol = ModHelpers.Cs(new Color(0, 1, 0, 1), " ■");
        NameText.AddNameText(player, partTimerSymbol);
    }
    [CustomRPC]
    public void RpcEmploy(ExPlayerControl employer)
    {
        _employer = employer;
        NameText.UpdateNameInfo(Player);
        NameText.UpdateNameInfo(employer);
    }

    public override Func<ExPlayerControl, bool> IsTargetable => (target) =>
    {
        // 自分自身は選択不可
        if (target == Player) return false;
        // 死んでいるプレイヤーは選択不可
        if (target.IsDead()) return false;
        return true;
    };
}