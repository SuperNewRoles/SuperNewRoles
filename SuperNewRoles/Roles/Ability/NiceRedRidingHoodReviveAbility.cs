using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Crewmate;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Ability;

public class NiceRedRidingHoodReviveAbility : AbilityBase, IAbilityCount
{
    // イベントリスナー
    private EventListener<MurderEventData> _murderEventListener;
    private EventListener<WrapUpEventData> _wrapUpEventListener;

    // 内部状態
    public ExPlayerControl Killer { get; set; }
    public bool IsRevivable { get; set; }
    public int RemainingReviveCount { get; set; }
    public int NiceRedRidingHoodCount { get; set; }
    public bool NiceRedRidingHoodIsKillerDeathRevive { get; set; }

    public NiceRedRidingHoodReviveAbility(int niceRedRidingHoodCount, bool niceRedRidingHoodIsKillerDeathRevive)
    {
        NiceRedRidingHoodCount = niceRedRidingHoodCount;
        NiceRedRidingHoodIsKillerDeathRevive = niceRedRidingHoodIsKillerDeathRevive;
    }

    // IAbilityCount実装
    public int MaxCount => NiceRedRidingHoodCount;

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        RemainingReviveCount = NiceRedRidingHoodCount;

        // イベントリスナーの登録
        _murderEventListener = MurderEvent.Instance.AddListener(OnMurderEvent);
        _wrapUpEventListener = WrapUpEvent.Instance.AddListener(OnWrapUpEvent);
    }

    public override void DetachToAlls()
    {
        _murderEventListener?.RemoveListener();
        _wrapUpEventListener?.RemoveListener();
        base.DetachToAlls();
    }

    private void OnMurderEvent(MurderEventData data)
    {
        if (data.target != Player || RemainingReviveCount <= 0 || !data.resultFlags.HasFlag(MurderResultFlags.Succeeded))
            return;

        if (TrySetKiller(data.killer))
            return;

        // 一部のカスタム死亡は MurderEvent 上では自殺扱いになるため、
        // CustomDeath 側の MurderData 登録後に実キラーを解決する。
        new LateTask(TrySetKillerFromMurderData, 0f, "NiceRedRidingHoodResolveKiller", log: false);
    }

    private void OnWrapUpEvent(WrapUpEventData data)
    {
        if (!IsRevivable || Player.IsAlive() || Killer == null) return;
        // 追放者がキラーの場合、復活判定
        if (data.exiled != null && data.exiled == Killer)
        {
            Logger.Info($"復活判定(キル者追放) : 可", "NiceRedRidingHood");
            Revive();
        }
        else if (NiceRedRidingHoodIsKillerDeathRevive && Killer.IsDead())
        {
            Logger.Info($"復活判定(キル者死亡) : 可", "NiceRedRidingHood");
            Revive();
        }
    }

    [CustomRPC]
    private void Revive()
    {
        if (!IsRevivable || RemainingReviveCount <= 0) return;

        Player.Player.Revive();
        RoleManager.Instance.SetRole(Player, RoleTypes.Crewmate);

        // FinalStatusを更新
        FinalStatusManager.SetFinalStatus(Player, FinalStatus.Alive);

        RemainingReviveCount--;
        IsRevivable = false;
        MurderDataManager.RevivedMurderData(Player);
        Killer = null;
        Player.Data.IsDead = false;

        Logger.Info($"復活完了 残り復活回数: {RemainingReviveCount}", "NiceRedRidingHood");
    }

    private bool TrySetKiller(ExPlayerControl killer)
    {
        if (killer == null || killer == Player)
            return false;

        Killer = killer;
        IsRevivable = true;
        Logger.Info($"ナイス赤ずきん {Player.Player.name} がキラー {Killer.Player.name} に殺されました", "NiceRedRidingHood");
        return true;
    }

    private void TrySetKillerFromMurderData()
    {
        if (IsRevivable || RemainingReviveCount <= 0 || Player.IsAlive())
            return;

        if (MurderDataManager.TryGetMurderData(Player, out var murderData))
        {
            TrySetKiller(murderData.Killer);
        }
    }
}

// 幽霊の役職表示とHaunt能力の制御
public class NiceRedRidingHoodGhostAbility : AbilityBase
{
    private NiceRedRidingHoodReviveAbility _reviveAbility;
    private DisibleHauntAbility _disableHauntAbility;
    private HideRoleOnGhostAbility _hideRoleOnGhostAbility;

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        // 復活能力への参照を取得
        _reviveAbility = Player.GetAbility<NiceRedRidingHoodReviveAbility>();

        if (_reviveAbility != null)
        {
            // 復活可能な場合は役職表示とHaunt能力を無効化
            _disableHauntAbility = new DisibleHauntAbility(() => _reviveAbility.IsRevivable);
            _hideRoleOnGhostAbility = new HideRoleOnGhostAbility((player) => _reviveAbility.IsRevivable);

            Player.AttachAbility(_disableHauntAbility, new AbilityParentAbility(this));
            Player.AttachAbility(_hideRoleOnGhostAbility, new AbilityParentAbility(this));
        }
    }
}
