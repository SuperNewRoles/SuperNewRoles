using System;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class AmnesiacAbility : AbilityBase
{
    private EventListener<CalledMeetingEventData> _calledMeetingListener;

    public AmnesiacAbility()
    {
    }

    public override void AttachToLocalPlayer()
    {
        _calledMeetingListener = CalledMeetingEvent.Instance.AddListener(OnCalledMeeting);
    }

    private void OnCalledMeeting(CalledMeetingEventData data)
    {
        // イベントを起こしたプレイヤーが自分でない場合は処理しない
        if (data.reporter.PlayerId != Player.PlayerId) return;

        // ターゲットが存在しない場合（会議ボタンでの呼び出し）は処理しない
        if (data.target == null) return;

        // 死体のプレイヤーIDを取得
        byte deadPlayerId = data.target.PlayerId;

        // 死体のプレイヤーを取得
        ExPlayerControl deadPlayer = ExPlayerControl.ById(deadPlayerId);
        if (deadPlayer == null) return;

        // 役職変更のRPCを呼び出す
        RpcChangeRole(Player, deadPlayer);
    }

    [CustomRPC]
    public static void RpcChangeRole(ExPlayerControl amnesiac, ExPlayerControl deadPlayer)
    {
        // nullチェックを追加
        if (amnesiac == null || deadPlayer == null || amnesiac.Player == null || deadPlayer.Player == null)
        {
            Logger.Error("AmnesiacAbility.RpcChangeRole: プレイヤーまたはそのプレイヤーオブジェクトがnullです");
            return;
        }

        // 死体のプレイヤーの役職とチームを取得
        RoleId targetRoleId = deadPlayer.Role;

        // 忘却者の役職を変更
        amnesiac.ReverseRole(deadPlayer);

        deadPlayer.SetRole(targetRoleId);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (_calledMeetingListener != null)
        {
            CalledMeetingEvent.Instance.RemoveListener(_calledMeetingListener);
            _calledMeetingListener = null;
        }
    }
}