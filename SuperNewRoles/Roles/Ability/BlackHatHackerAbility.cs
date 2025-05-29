using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Events;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles.Ability;

public class BlackHatHackerAbility : AbilityBase
{
    public BlackHatHackerData Data { get; private set; }

    // DevicesPatchからアクセスするための静的プロパティ
    public static BlackHatHackerAbility LocalInstance { get; private set; }
    public BlackHatHackerHackButton HackButton { get; private set; }
    public PortableAdminAbility AdminAbility { get; set; }
    private BlackHatHackerVitalsButton VitalsAbility { get; set; }

    public BlackHatHackerTaskPanel TaskPanel { get; private set; }

    // 感染タイマー管理
    public Dictionary<byte, float> InfectionTimer { get; private set; }
    public float NotInfectiousTimer { get; private set; }
    public float SharedTimer { get; private set; } = 1f;
    public List<byte> DeadPlayers { get; private set; }
    public List<byte> _SelfPropagationPlayerId { get; private set; }
    public List<byte> _InfectedPlayerId { get; private set; }

    private EventListener _fixedUpdateEvent;
    private EventListener<WrapUpEventData> _wrapUpEvent;

    public List<byte> SelfPropagationPlayerId
    {
        get
        {
            List<byte> add = InfectionTimer
                .Where(x => !_SelfPropagationPlayerId.Contains(x.Key) && IsSelfPropagation(x.Value))
                .Select(x => x.Key).ToList();
            _SelfPropagationPlayerId.AddRange(add);
            return _SelfPropagationPlayerId;
        }
    }

    public List<byte> InfectedPlayerId
    {
        get
        {
            List<byte> check = InfectionTimer
                .Where(x => !_InfectedPlayerId.Contains(x.Key) && x.Value >= Data.HackInfectiousTime)
                .Select(x => x.Key).ToList();
            _InfectedPlayerId.AddRange(check);
            return _InfectedPlayerId;
        }
    }

    public BlackHatHackerAbility(BlackHatHackerData data)
    {
        Data = data;
        InitializeTimers();
    }

    private void InitializeTimers()
    {
        InfectionTimer = new Dictionary<byte, float>();
        NotInfectiousTimer = Data.NotInfectiousTime;
        SharedTimer = 1f;
        DeadPlayers = new List<byte>();
        _SelfPropagationPlayerId = new List<byte>();
        _InfectedPlayerId = new List<byte>();
    }

    public override void AttachToAlls()
    {
        HackButton = new BlackHatHackerHackButton(this);
        AdminAbility = new PortableAdminAbility(new PortableAdminData(
            CanUseAdmin: () => Data.CanInfectedAdmin,
            canMoveWhileUsingAdmin: () => Data.CanMoveWhenUsesAdmin
        ));
        VitalsAbility = new BlackHatHackerVitalsButton(this);
        TaskPanel = new BlackHatHackerTaskPanel(this);

        AbilityParentAbility parentAbility = new(this);
        Player.AttachAbility(HackButton, parentAbility);
        Player.AttachAbility(AdminAbility, parentAbility);
        Player.AttachAbility(VitalsAbility, parentAbility);
        Player.AttachAbility(TaskPanel, parentAbility);

        // イベントリスナーの登録
        _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _wrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);

        // ローカルプレイヤーの場合、静的参照を設定
        if (Player.AmOwner)
        {
            LocalInstance = this;
            // 他プレイヤーの感染タイマーを初期化
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.AmOwner) continue;
                InfectionTimer[player.PlayerId] = 0f;
            }
        }
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateEvent?.RemoveListener();
        _wrapUpEvent?.RemoveListener();

        // 静的参照をクリア
        if (LocalInstance == this)
            LocalInstance = null;
    }

    private void OnFixedUpdate()
    {
        if (!Player.AmOwner) return;

        // 共有タイマーの更新
        SharedTimer -= Time.fixedDeltaTime;
        if (SharedTimer <= 0)
        {
            RpcSetInfectionTimer(InfectionTimer);
            SharedTimer = 1;
        }

        // 勝利判定
        if (IsAllInfected() && (Data.CanDeadWin || !ExPlayerControl.LocalPlayer.IsDead()))
        {
            // BlackHatHacker勝利
            EndGamer.RpcEndGameWithWinner(
                CustomGameOverReason.BlackHatHackerWin,
                WinType.SingleNeutral,
                [Player],
                BlackHatHacker.Instance.RoleColor,
                "BlackHatHacker"
            );
            return;
        }

        if (ExPlayerControl.LocalPlayer.IsDead() && !Data.CanDeadWin) return;

        // 非感染タイマーの更新
        NotInfectiousTimer -= Time.fixedDeltaTime;
        if (NotInfectiousTimer > 0) return;

        // 会議中は感染進行停止
        if (DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed) return;
        if (MeetingHud.Instance) return;

        // 自己増殖処理
        foreach (byte id in SelfPropagationPlayerId)
        {
            if (InfectionTimer.ContainsKey(id))
                InfectionTimer[id] += Time.fixedDeltaTime / 5;
        }

        // 感染拡散処理
        foreach (byte id in InfectedPlayerId)
        {
            PlayerControl player = GameData.Instance.GetPlayerById(id)?.Object;
            if (!player || player.Data.IsDead) continue;

            float scope = GameManager.Instance.LogicOptions.GetKillDistance();
            var infection = PlayerControl.AllPlayerControls.ToArray()
                .Where(x => !x.AmOwner && Vector3.Distance(player.transform.position, x.transform.position) <= scope);

            foreach (PlayerControl target in infection)
            {
                if (InfectionTimer.ContainsKey(target.PlayerId))
                    InfectionTimer[target.PlayerId] += Time.fixedDeltaTime;
            }
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        NotInfectiousTimer = this.Data.NotInfectiousTime;
        DeadPlayers = ExPlayerControl.ExPlayerControls
            .Where(x => x.IsDead() || data?.exiled?.PlayerId == x.PlayerId)
            .Select(x => x.PlayerId).ToList();

        // 感染者がいない場合、かつ設定が有効な場合にハック回数を補充
        if (Data.IsNotInfectionIncrease &&
            !InfectionTimer.Any(kvp => kvp.Value >= Data.HackInfectiousTime && !DeadPlayers.Contains(kvp.Key)))
        {
            if (HackButton != null) // HackButtonがnullでないことを確認
            {
                HackButton.Count += 1; // 設定されている最大回数に戻す（または1回増やすなど、仕様に応じて変更）
            }
        }
    }

    public bool IsSelfPropagation(float timer)
    {
        return timer >= Data.HackInfectiousTime * ((int)Data.StartSelfPropagation / 100f);
    }

    public bool IsAllInfected()
    {
        return InfectionTimer.All(x =>
        {
            // DeadPlayersリストを使用して生死を判定
            ExPlayerControl player = ExPlayerControl.ById(x.Key);
            return player == null || player.AmOwner || player.IsDead() || x.Value >= Data.HackInfectiousTime;
        });
    }

    [CustomRPC]
    public void RpcSetInfectionTimer(Dictionary<byte, float> infectionData)
    {
        this.InfectionTimer = infectionData;
    }
}

public class BlackHatHackerData
{
    public float HackCooldown { get; }
    public int HackCount { get; }
    public bool IsNotInfectionIncrease { get; }
    public float HackInfectiousTime { get; }
    public float NotInfectiousTime { get; }
    public BlackHatHackerSelfPropagationType StartSelfPropagation { get; }
    public BlackHatHackerInfectionScopeType InfectionScope { get; }
    public bool CanInfectedAdmin { get; }
    public bool CanMoveWhenUsesAdmin { get; }
    public bool IsAdminColor { get; }
    public bool CanInfectedVitals { get; }
    public bool CanDeadWin { get; }

    public BlackHatHackerData(
        float hackCooldown,
        int hackCount,
        bool isNotInfectionIncrease,
        float hackInfectiousTime,
        float notInfectiousTime,
        BlackHatHackerSelfPropagationType startSelfPropagation,
        BlackHatHackerInfectionScopeType infectionScope,
        bool canInfectedAdmin,
        bool canMoveWhenUsesAdmin,
        bool isAdminColor,
        bool canInfectedVitals,
        bool canDeadWin)
    {
        HackCooldown = hackCooldown;
        HackCount = hackCount;
        IsNotInfectionIncrease = isNotInfectionIncrease;
        HackInfectiousTime = hackInfectiousTime;
        NotInfectiousTime = notInfectiousTime;
        StartSelfPropagation = startSelfPropagation;
        InfectionScope = infectionScope;
        CanInfectedAdmin = canInfectedAdmin;
        CanMoveWhenUsesAdmin = canMoveWhenUsesAdmin;
        IsAdminColor = isAdminColor;
        CanInfectedVitals = canInfectedVitals;
        CanDeadWin = canDeadWin;
    }
}