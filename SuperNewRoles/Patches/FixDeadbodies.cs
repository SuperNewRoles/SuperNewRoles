using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;
public static class FixDeadbodies
{
    // (プレイヤー, 死体ID) → DeadBody を格納
    public static readonly Dictionary<(byte, int), DeadBody> Deadbodies = new();
    // 登録済み死体の高速チェック用
    private static readonly HashSet<int> _deadBodySet = new();
    // プレイヤー毎の次の死体ID
    private static readonly Dictionary<byte, int> _nextIds = new();
    // 次回に移動する予定の情報
    public static readonly Dictionary<(byte, int), Vector3> WillMoves = new();

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class CoStartGamePatch
    {
        public static void Postfix()
        {
            Deadbodies.Clear();
            _deadBodySet.Clear();
            _nextIds.Clear();
            WillMoves.Clear();
        }
    }

    [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
    class KillAnimationPatch
    {
        public static void Postfix(KillAnimation __instance, PlayerControl target)
        {
            new LateTask(() =>
            {
                int targetId = target.PlayerId;
                var bodies = Object.FindObjectsOfType<DeadBody>();
                if (!_nextIds.TryGetValue(target.PlayerId, out int nextId))
                    nextId = 0;

                foreach (var body in bodies)
                {
                    if (body.ParentId != targetId) continue;
                    // 新規死体のみ追加
                    if (!_deadBodySet.Add(body.GetInstanceID())) continue;

                    Deadbodies[(target.PlayerId, nextId)] = body;
                    // LadderのDestinationを使ってLadderペアと距離を計算し、条件を満たす場合に死体を通報できる場所に移動
                    Ladder nearest = null;
                    float minDist = float.MaxValue;
                    foreach (var ladder in ShipStatus.Instance.Ladders)
                    {
                        float distToBody = Vector2.Distance(body.transform.position, ladder.transform.position);
                        if (distToBody < minDist)
                        {
                            minDist = distToBody;
                            nearest = ladder;
                        }
                    }
                    if (nearest != null && nearest.Destination != null)
                    {
                        var other = nearest.Destination;
                        float pairDistance = Vector2.Distance(nearest.transform.position, other.transform.position);
                        float dx = Mathf.Abs(body.transform.position.x - nearest.transform.position.x);
                        float dy = Mathf.Abs(body.transform.position.y - nearest.transform.position.y);
                        if (dx <= 0.5f)
                        {
                            if (nearest.IsTop)
                            {
                                float dyDown = nearest.transform.position.y - body.transform.position.y;
                                if (dyDown >= 0f && dyDown <= pairDistance / 2f)
                                    body.transform.position = nearest.transform.position + new Vector3(0.15f, 0.01f, -0.15f);
                            }
                            else
                            {
                                float dyUp = body.transform.position.y - nearest.transform.position.y;
                                if (dyUp >= 0f && dyUp <= pairDistance / 2f)
                                    body.transform.position = nearest.transform.position + new Vector3(0.15f, -0.01f, -0.15f);
                            }
                        }
                    }

                    // MovingPlatform に同じ判定ロジックを適用（x軸方向）
                    var platforms = Object.FindObjectsOfType<MovingPlatformBehaviour>();
                    MovingPlatformBehaviour nearestPlatform = null;
                    float minPlatDist = float.MaxValue;
                    foreach (var platform in platforms)
                    {
                        float dist = Vector2.Distance(body.transform.position, platform.transform.position);
                        if (dist < minPlatDist)
                        {
                            minPlatDist = dist;
                            nearestPlatform = platform;
                        }
                    }
                    if (nearestPlatform != null && nearestPlatform.Target != null && nearestPlatform.Target.PlayerId == body.ParentId)
                    {
                        var leftPos = nearestPlatform.transform.parent.TransformPoint(nearestPlatform.LeftUsePosition) + new Vector3(0.3f, 0.2f, 0f);
                        var rightPos = nearestPlatform.transform.parent.TransformPoint(nearestPlatform.RightUsePosition) + new Vector3(-0.3f, 0.2f, 0f);
                        float pairXDist = Mathf.Abs(leftPos.x - rightPos.x);
                        // 近い方にdeadbodyテレポート
                        if (Mathf.Abs(body.transform.position.x - leftPos.x) < Mathf.Abs(body.transform.position.x - rightPos.x))
                        {
                            body.transform.position = leftPos;
                        }
                        else
                        {
                            body.transform.position = rightPos;
                        }
                    }

                    if (AmongUsClient.Instance.AmHost)
                    {
                        RpcSyncDeadbody(target, nextId, body.transform.position);
                    }
                    if (WillMoves.TryGetValue((target.PlayerId, nextId), out var position))
                    {
                        body.transform.position = position;
                        WillMoves.Remove((target.PlayerId, nextId));
                    }
                    nextId++;
                }

                _nextIds[target.PlayerId] = nextId;
                WillMoves.Clear();
            }, 0.1f);
        }
    }
    [CustomRPC]
    public static void RpcSyncDeadbody(ExPlayerControl player, int id, Vector3 position)
    {
        if (Deadbodies.TryGetValue((player.PlayerId, id), out var body))
        {
            body.transform.position = position;
        }
        else
        {
            WillMoves[(player.PlayerId, id)] = position;
        }
    }
}