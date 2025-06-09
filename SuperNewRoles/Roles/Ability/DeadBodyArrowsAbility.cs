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
    private readonly Color _defaultArrowColor;
    public bool ShowArrows => _showArrows?.Invoke() ?? true;
    /// <summary>死体矢印に反映させるボディカラーのモード</summary>
    private readonly DeadBodyColorMode _deadBodyColorMode;
    private Dictionary<DeadBody, (Arrow arrow, Color color)> _deadBodyArrows = new();
    private EventListener _fixedUpdateEvent;

    /// <param name="showArrows">矢印のを表示できるか</param>
    /// <param name="arrowColor">矢印の色(指定無しの場合Vultureのロールカラー)</param>
    public DeadBodyArrowsAbility(Func<bool> showArrows, Color arrowColor = default, DeadBodyColorMode colorMode = DeadBodyColorMode.None)
    {
        _showArrows = showArrows;
        _defaultArrowColor = arrowColor == default ? Vulture.Instance.RoleColor : arrowColor;
        _deadBodyColorMode = colorMode;
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
            foreach (var (arrow, color) in _deadBodyArrows.Values)
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

        // 既存の矢印を更新または不要な矢印を削除
        foreach (var arrowEntry in _deadBodyArrows.ToList())
        {
            int parentId = arrowEntry.Key.ParentId;
            if (deadBodiesByParentId.ContainsKey(parentId))
            {

                if (arrowEntry.Value.arrow == null)
                {
                    var arrowColor = _deadBodyColorMode == DeadBodyColorMode.None ? _defaultArrowColor : ResolveDeadBodyArrowColor(arrowEntry.Key);
                    _deadBodyArrows[arrowEntry.Key] = (new Arrow(arrowColor), arrowColor);
                }
                _deadBodyArrows[arrowEntry.Key].arrow.Update(arrowEntry.Key.transform.position, arrowEntry.Value.color);
                _deadBodyArrows[arrowEntry.Key].arrow.arrow.SetActive(true);
            }
            else
            {
                if (arrowEntry.Value.arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrowEntry.Value.arrow.arrow);
                _deadBodyArrows.Remove(arrowEntry.Key);
            }
        }

        // 新しい死体に対して矢印を追加（既に同じParentIdの矢印が存在しなければ）
        foreach (var kv in deadBodiesByParentId)
        {
            if (_deadBodyArrows.Keys.Any(db => db.ParentId == kv.Key)) continue;

            Color arrowColor = _deadBodyColorMode == DeadBodyColorMode.None ? _defaultArrowColor : ResolveDeadBodyArrowColor(kv.Value);
            _deadBodyArrows.Add(kv.Value, (new Arrow(arrowColor), arrowColor));
            _deadBodyArrows[kv.Value].arrow.Update(kv.Value.transform.position, arrowColor);
            _deadBodyArrows[kv.Value].arrow.arrow.SetActive(true);
        }
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();

        // イベントリスナーを削除
        if (_fixedUpdateEvent != null)
            FixedUpdateEvent.Instance.RemoveListener(_fixedUpdateEvent);

        // 矢印を削除
        foreach (var (arrow, color) in _deadBodyArrows.Values)
        {
            if (arrow?.arrow != null)
                UnityEngine.Object.Destroy(arrow.arrow);
        }
        _deadBodyArrows.Clear();
    }

    /// <summary>死体用矢印の色をモードに応じて取得する</summary>
    /// <param name="db">色を取得したい死体</param>
    /// <returns>死体用矢印の色</returns>
    private Color ResolveDeadBodyArrowColor(DeadBody db)
    {
        var exp = ExPlayerControl.ById(db.ParentId);
        int deadBodyColorId;

        if (exp == null) return _defaultArrowColor;

        // 明暗表示関連
        const int lightColorId = (int)SuperNewRoles.CustomCosmetics.CustomColors.ColorType.Pitchwhite;
        const int darknessColorId = (int)SuperNewRoles.CustomCosmetics.CustomColors.ColorType.Crasyublue;

        // 有効なプレイヤーカラーの範囲内か
        var isValidColorId = SuperNewRoles.CustomCosmetics.CustomColors.IsValidColorId(exp.Data.DefaultOutfit.ColorId);

        switch (_deadBodyColorMode)
        {
            case DeadBodyColorMode.LightAndDarkness:
            case DeadBodyColorMode.Adaptive when !isValidColorId: // ボディカラー反映時に 不正なColorIdであれば明暗表示で返す
                deadBodyColorId = SuperNewRoles.CustomCosmetics.CustomColors.IsLighter(exp) ? lightColorId : darknessColorId;
                break;
            case DeadBodyColorMode.Adaptive when isValidColorId: // 有効なColorIdであれば 死体の色を返す
                deadBodyColorId = exp.Data.DefaultOutfit.ColorId;
                break;
            default:
                return _defaultArrowColor;
        }

        Color deadBodyColor = Palette.PlayerColors[deadBodyColorId];
        return deadBodyColor;
    }
}

public enum DeadBodyColorMode
{
    /// <summary>死体色を反映しない</summary>
    None,
    /// <summary>死体の明暗のみ反映する</summary>
    LightAndDarkness,
    /// <summary>死体色を反映する</summary>
    Adaptive,
}
