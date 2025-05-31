using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Tuna : RoleBase<Tuna>
{
    public override RoleId Role { get; } = RoleId.Tuna;
    public override Color32 RoleColor { get; } = Color.cyan;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new DeathIfIdlingAbility(new TunaData(
            stopTimeLimit: TunaStopTimeLimit
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    [CustomOptionFloat("TunaStopTimeLimit", 1f, 3f, 0.5f, 2f)]
    public static float TunaStopTimeLimit;
    [CustomOptionBool("EnableTunaSoloWin", false)]
    public static bool EnableTunaSoloWin;

    public class DeathIfIdlingAbility : AbilityBase
    {
        private readonly TunaData _data;
        private float _stopTimer = 0f;
        private Vector2 _lastPosition;
        private bool _isMoving = true;
        private EventListener _updateListener;
        private EventListener<WrapUpEventData> _wrapUpListener;
        private bool isMeeting = false;
        private bool isMoveStarted = false;

        public DeathIfIdlingAbility(TunaData data)
        {
            _data = data;
            isMoveStarted = false;
        }

        public override void AttachToLocalPlayer()
        {
            // プレイヤーの初期位置を記録
            _lastPosition = Player.transform.position;

            // イベントリスナーを設定
            _updateListener = HudUpdateEvent.Instance.AddListener(OnUpdate);
            _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        }

        private void OnUpdate()
        {
            // 死亡している場合は何もしない
            if (Player.Data.IsDead)
                return;

            // 会議中は何もしない
            if (MeetingHud.Instance != null)
            {
                isMeeting = true;
                return;
            }
            if (isMeeting)
                return;
            if (Player.IsWaitingSpawn())
                return;

            // 現在の位置を取得
            Vector2 currentPosition = Player.transform.position;

            // イントロ中は何もしない
            if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed)
            {
                _lastPosition = currentPosition;
                return;
            }

            // 位置が変わっているかチェック
            bool isCurrentlyMoving = Vector2.Distance(_lastPosition, currentPosition) > 0.0001f;

            // isMoveStartedが無効かつ動いた場合は有効にする
            if (!isMoveStarted && isCurrentlyMoving)
            {
                isMoveStarted = true;
            }

            // isMoveStartedが無効な場合はタイマーを進めない
            if (!isMoveStarted)
                return;

            // 移動状態が変わった場合
            if (isCurrentlyMoving != _isMoving)
            {
                if (isCurrentlyMoving)
                {
                    // 動き始めた場合はタイマーをリセット
                    _stopTimer = 0f;
                }
                _isMoving = isCurrentlyMoving;
            }

            // 止まっている場合はタイマーを進める
            if (!_isMoving)
            {
                _stopTimer += Time.deltaTime;

                // 設定時間を超えたら死亡
                if (_stopTimer >= _data.StopTimeLimit)
                {
                    // 死亡処理
                    KillPlayer();
                }
            }

            // 現在の位置を記録
            _lastPosition = currentPosition;
        }

        private void OnWrapUp(WrapUpEventData data)
        {
            // 会議終了時にタイマーをリセット
            _stopTimer = 0f;
            // 位置を更新
            _lastPosition = Player.transform.position;
            _isMoving = true;
            isMeeting = false;
        }

        private void KillPlayer()
        {
            if (Player.Data.IsDead) return;

            // プレイヤーを殺す
            if (Player.AmOwner)
            {
                // 自分自身の場合は直接死亡処理
                ((ExPlayerControl)Player).RpcCustomDeath(CustomDeathType.Tuna);
            }
        }

        public override void DetachToLocalPlayer()
        {
            // イベントリスナーを削除
            HudUpdateEvent.Instance.RemoveListener(_updateListener);
            WrapUpEvent.Instance.RemoveListener(_wrapUpListener);
        }
    }
}

public class TunaData
{
    public float StopTimeLimit { get; }

    public TunaData(float stopTimeLimit)
    {
        StopTimeLimit = stopTimeLimit;
    }
}