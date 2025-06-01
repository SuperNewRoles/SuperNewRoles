using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

public class DeadBodyArrowsAbility : AbilityBase
{
    private readonly Func<bool> _showArrows;
    private readonly Color _arrowColor;
    public bool ShowArrows => _showArrows?.Invoke() ?? true;
    private Dictionary<DeadBody, Arrow> _deadBodyArrows = new();
    private EventListener _fixedUpdateEvent;

    public DeadBodyArrowsAbility(Func<bool> showArrows, Color arrowColor)
    {
        _showArrows = showArrows;
        _arrowColor = arrowColor;
    }

    public override void AttachToLocalPlayer()
    {
        // 矢印表示のイベントリスナーを設定
        if (ShowArrows)
        {
            _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        }
    }

    private void OnFixedUpdate()
    {
        if (Player.IsDead())
        {
            if (_deadBodyArrows.Count <= 0) return;
            foreach (var arrow in _deadBodyArrows.Values)
            {
                if (arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrow.arrow);
            }
            _deadBodyArrows.Clear();
            return;
        }
        if (!ShowArrows) return;

        // DeadBodyの検索を一度だけ行い、ParentIdでグループ化してキャッシュ
        DeadBody[] allDeadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        Dictionary<int, DeadBody> deadBodiesByParentId = new();
        foreach (DeadBody dead in allDeadBodies)
        {
            if (!deadBodiesByParentId.ContainsKey(dead.ParentId))
                deadBodiesByParentId.Add(dead.ParentId, dead);
        }

        Color roleColor = _arrowColor;

        // 既存の矢印を更新または不要な矢印を削除
        foreach (var arrowEntry in _deadBodyArrows.ToList())
        {
            int parentId = arrowEntry.Key.ParentId;
            if (deadBodiesByParentId.ContainsKey(parentId))
            {
                if (arrowEntry.Value == null)
                    _deadBodyArrows[arrowEntry.Key] = new Arrow(roleColor);
                _deadBodyArrows[arrowEntry.Key].Update(arrowEntry.Key.transform.position, roleColor);
                _deadBodyArrows[arrowEntry.Key].arrow.SetActive(true);
            }
            else
            {
                if (arrowEntry.Value?.arrow != null)
                    UnityEngine.Object.Destroy(arrowEntry.Value.arrow);
                _deadBodyArrows.Remove(arrowEntry.Key);
            }
        }

        // 新しい死体に対して矢印を追加（既に同じParentIdの矢印が存在しなければ）
        foreach (var kv in deadBodiesByParentId)
        {
            if (_deadBodyArrows.Keys.Any(db => db.ParentId == kv.Key))
                continue;
            _deadBodyArrows.Add(kv.Value, new Arrow(roleColor));
            _deadBodyArrows[kv.Value].Update(kv.Value.transform.position, roleColor);
            _deadBodyArrows[kv.Value].arrow.SetActive(true);
        }
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();

        // イベントリスナーを削除
        if (_fixedUpdateEvent != null)
            FixedUpdateEvent.Instance.RemoveListener(_fixedUpdateEvent);

        // 矢印を削除
        foreach (var arrow in _deadBodyArrows.Values)
        {
            if (arrow?.arrow != null)
                UnityEngine.Object.Destroy(arrow.arrow);
        }
        _deadBodyArrows.Clear();
    }
}