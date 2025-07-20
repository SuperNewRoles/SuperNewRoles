using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class NiceRedRidingHoodReviveAbility : AbilityBase, IAbilityCount
{
    // イベントリスナー
    private EventListener<MurderEventData> _murderEventListener;
    private EventListener<WrapUpEventData> _wrapUpEventListener;
    private EventListener<DieEventData> _dieEventListener;

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
        _dieEventListener = DieEvent.Instance.AddListener(OnDieEvent);
    }

    public override void DetachToAlls()
    {
        _murderEventListener?.RemoveListener();
        _wrapUpEventListener?.RemoveListener();
        _dieEventListener?.RemoveListener();
        base.DetachToAlls();
    }

    private void OnMurderEvent(MurderEventData data)
    {
        // 自分が殺された場合、キラーを記録
        if (data.target == Player && data.killer != Player && RemainingReviveCount > 0)
        {
            Killer = data.killer;
            IsRevivable = true;
            Logger.Info($"ナイス赤ずきん {Player.Player.name} がキラー {Killer.Player.name} に殺されました", "NiceRedRidingHood");
        }
    }

    private void OnWrapUpEvent(WrapUpEventData data)
    {
        // 追放者がキラーの場合、復活判定
        if (!Player.IsAlive() && IsRevivable && data.exiled != null)
        {
            var exiledPlayer = ExPlayerControl.ExPlayerControls.FirstOrDefault(x => x.Data == data.exiled);
            if (exiledPlayer != null && exiledPlayer == Killer)
            {
                Logger.Info($"復活判定(キル者追放) : 可", "NiceRedRidingHood");
                Revive();
            }
        }
    }

    private void OnDieEvent(DieEventData data)
    {
        // タスクフェイズ中にキラーが死亡した場合の復活判定
        if (!Player.IsAlive() && IsRevivable && NiceRedRidingHoodIsKillerDeathRevive && data.player == Killer)
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
        Killer = null;
        Player.Data.IsDead = false;

        Logger.Info($"復活完了 残り復活回数: {RemainingReviveCount}", "NiceRedRidingHood");
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
            _disableHauntAbility = new DisibleHauntAbility(() => _reviveAbility.RemainingReviveCount > 0 && _reviveAbility.Killer != null);
            _hideRoleOnGhostAbility = new HideRoleOnGhostAbility((player) => _reviveAbility.RemainingReviveCount > 0 && _reviveAbility.Killer != null);

            Player.AttachAbility(_disableHauntAbility, new AbilityParentAbility(this));
            Player.AttachAbility(_hideRoleOnGhostAbility, new AbilityParentAbility(this));
        }
    }
}