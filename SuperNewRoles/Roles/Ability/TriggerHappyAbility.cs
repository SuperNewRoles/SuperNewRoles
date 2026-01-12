using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.CustomObject;
using static SuperNewRoles.CustomObject.CustomPlayerAnimationSimple;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public class TriggerHappyAbility : CustomButtonBase, IAbilityCount, IButtonEffect
{
    private readonly TriggerHappyData _data;
    private readonly Dictionary<byte, int> _hitCounts = new();
    private readonly List<Vector3> _pendingBulletPositionsWithTime = new();
    private readonly List<Vector2> _pendingBulletDirections = new();
    internal const float BatchInterval = 0.2f;
    private const bool EnableMidpointShooting = true; // 中間位置からの追加発射を有効にするかどうか
    private float _batchTimer;
    private SyncKillCoolTimeAbility _syncKillCoolTimeAbility;
    private TriggerHappyBullet _lastBullet;
    private Vector2 _lastFirePosition;

    public TriggerHappyAbility(TriggerHappyData data)
    {
        _data = data;
        Count = data.UseLimit;
    }

    public TriggerHappyData Data => _data;

    public override float DefaultTimer => _data.Cooldown;
    public override string buttonText => ModTranslation.GetString("TriggerHappyButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("TriggerHappy_Button.png");
    public override ShowTextType showTextType => ShowTextType.ShowWithCount;

    protected override KeyType keytype => KeyType.Ability1;

    public bool isEffectActive { get; set; }
    public bool IsGatlingGunActive => ActiveGatlingGun;

    public Action OnEffectEnds => onFinishHappy;

    public float EffectDuration => _data.Duration;

    public float EffectTimer { get; set; }

    private bool ActiveGatlingGun = false;
    private TriggerHappyGatlingGun GatlingGunAnimation = null;

    private EventListener<MeetingStartEventData> _meetingStartEventListener;

    private void onFinishHappy()
    {
        FlushBulletBatch();
        RpcFinishHappy();
    }

    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && (isEffectActive || HasCount);
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove;
    }

    public override void OnClick()
    {
        this.UseAbilityCount();
        RpcStartHappy();
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        ActiveGatlingGun = false;
        _meetingStartEventListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        if (_data.SyncKillCoolTime && _syncKillCoolTimeAbility == null)
        {
            _syncKillCoolTimeAbility = SyncKillCoolTimeAbility.CreateAndAttach(this);
        }
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        ResetState();
        _meetingStartEventListener?.RemoveListener();
    }
    private void OnMeetingStart(MeetingStartEventData data)
    {
        ResetState();
    }
    private void ResetState()
    {
        ActiveGatlingGun = false;
        _hitCounts.Clear();
        _pendingBulletPositionsWithTime.Clear();
        _pendingBulletDirections.Clear();
        _batchTimer = 0f;
        _lastBullet = null;
        _lastFirePosition = Vector2.zero;
        if (GatlingGunAnimation != null)
        {
            GatlingGunAnimation.RequestFadeOutAndDestroy();
            GatlingGunAnimation = null;
        }
    }
    [CustomRPC]
    public void RpcStartHappy()
    {
        ActiveGatlingGun = true;
        _hitCounts.Clear();
        _pendingBulletPositionsWithTime.Clear();
        _pendingBulletDirections.Clear();
        _batchTimer = 0f;
        _lastBullet = null;
        _lastFirePosition = Vector2.zero;
        GatlingGunAnimation = TriggerHappyGatlingGun.Spawn(Player,
            GetSprites("TriggerHappy_Machinegun_{0}.png", 1, 2),
            _data);
    }
    [CustomRPC]
    public void RpcFinishHappy()
    {
        ResetState();
    }

    public void TickBatch(float deltaTime)
    {
        if (!Player.AmOwner || !ActiveGatlingGun)
            return;

        _batchTimer += deltaTime;
        if (_batchTimer < BatchInterval)
            return;

        FlushBulletBatch();
        _batchTimer = 0f;
        // ついでにこのタイミングでキルクールタイム同期もしておく
        Player.ResetKillCooldown();
    }

    public void QueueBullet(Vector2 position, Vector2 direction)
    {
        if (!Player.AmOwner || !ActiveGatlingGun)
            return;

        // 通常の位置から弾を発射
        var bullet1 = TriggerHappyBullet.Spawn(Player, this, position, direction, _data.Range, _data.BulletSize, _data.PierceWalls);
        float timeOffset = Mathf.Clamp(_batchTimer, 0f, BatchInterval);
        _pendingBulletPositionsWithTime.Add(new Vector3(position.x, position.y, timeOffset));
        _pendingBulletDirections.Add(direction);

        // 中間位置からの追加発射が有効な場合、前回の弾が存在する場合、中間位置から追加の弾を発射
        if (EnableMidpointShooting && _lastBullet != null && _lastBullet.IsActive)
        {
            Vector2 lastBulletPos = _lastBullet.transform.position;
            Vector2 midpoint = (lastBulletPos + _lastFirePosition) / 2f;
            var bullet2 = TriggerHappyBullet.Spawn(Player, this, midpoint, direction, _data.Range, _data.BulletSize, _data.PierceWalls);
            _pendingBulletPositionsWithTime.Add(new Vector3(midpoint.x, midpoint.y, timeOffset));
            _pendingBulletDirections.Add(direction);
        }

        // 最新の弾と発射位置を更新
        _lastBullet = bullet1;
        _lastFirePosition = position;
    }

    private void FlushBulletBatch()
    {
        if (!Player.AmOwner)
            return;
        if (_pendingBulletPositionsWithTime.Count == 0)
            return;

        // バッチ送信時点のガトリングガン角度も一緒に送る（非オーナー側の表示角度同期用）
        float currentAngle = 0f;
        if (GatlingGunAnimation != null)
        {
            currentAngle = GatlingGunAnimation.transform.eulerAngles.z * Mathf.Deg2Rad;
        }

        var batchData = new TriggerHappyBulletBatchData(_pendingBulletPositionsWithTime, _pendingBulletDirections, currentAngle);
        RpcFireBulletBatch(batchData);
        _pendingBulletPositionsWithTime.Clear();
        _pendingBulletDirections.Clear();
    }

    [CustomRPC(true)]
    public void RpcFireBulletBatch(TriggerHappyBulletBatchData batchData)
    {
        if (batchData == null || batchData.Count == 0)
            return;

        // 角度同期：オーナーが送ったangleを、非オーナー側のGatlingGunがtargetAngleとして受け取り追従する
        if (GatlingGunAnimation != null)
        {
            GatlingGunAnimation.SetAngle(batchData.AngleRadians);
        }

        for (int i = 0; i < batchData.Count; i++)
        {
            Vector2 position = batchData.GetPosition(i);
            Vector2 direction = batchData.GetDirection(i);
            float delay = batchData.GetDelay(i);
            new LateTask(() =>
            {
                TriggerHappyBullet.Spawn(Player, this, position, direction, _data.Range, _data.BulletSize, _data.PierceWalls);
            }, delay, "TriggerHappyBullet", log: false);
        }
    }


    public void RegisterHit(ExPlayerControl target)
    {
        if (!Player.AmOwner)
            return;
        if (target == null || target.IsDead())
            return;

        if (!_hitCounts.TryGetValue(target.PlayerId, out var hits))
            hits = 0;
        hits++;
        if (hits >= _data.RequiredHits)
        {
            _hitCounts.Remove(target.PlayerId);
            ExPlayerControl.LocalPlayer.RpcCustomDeath(target, CustomDeathType.HappyGatling);
            if (_data.KillSound)
            {
                byte victimId = target.PlayerId;
                new LateTask(() =>
                {
                    // RpcCustomDeath直後はData.IsDeadがまだ反映されない場合があるため、少し遅延してから確認する
                    var victim = ExPlayerControl.ExPlayerControls.Find(p => p != null && p.PlayerId == victimId);
                    if (victim != null && victim.IsDead())
                        SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, loop: false, 0.8f);
                }, 0.1f, "TriggerHappyKillSound");
            }
            return;
        }

        _hitCounts[target.PlayerId] = hits;
    }

}
