using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Impostor;
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
    private readonly List<DeadBody> _arrowRemoveBuffer = new();
    private readonly HashSet<int> _trackedParentIds = new();
    private DeadBody[] _cachedDeadBodies = System.Array.Empty<DeadBody>();
    private int _scanCounter;
    private int _lastDeadCount = -1;

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
        SubscribeWithAbility(FixedUpdateEvent.Instance, OnFixedUpdate);
        SubscribeWithAbility(WrapUpEvent.Instance, OnWrapUp);
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        // reset
        foreach (var (arrow, color) in _deadBodyArrows.Values)
        {
            if (arrow?.arrow != null)
                UnityEngine.Object.Destroy(arrow.arrow);
        }
        _deadBodyArrows.Clear();
        _trackedParentIds.Clear();
        _lastDeadCount = -1;
        _scanCounter = 0;
        _cachedDeadBodies = System.Array.Empty<DeadBody>();
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
            _trackedParentIds.Clear();
            _lastDeadCount = -1;
            _cachedDeadBodies = System.Array.Empty<DeadBody>();
            return;
        }
        if (!ShowArrows) return;

        // FindObjectsOfTypeは間引くが、死者数が変わったら即スキャンする
        _scanCounter++;
        int deadCount = CountDeadPlayers();
        if (_scanCounter >= 10 || _cachedDeadBodies.Length == 0 || deadCount != _lastDeadCount)
        {
            _scanCounter = 0;
            _lastDeadCount = deadCount;
            _cachedDeadBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
        }

        _trackedParentIds.Clear();
        foreach (DeadBody dead in _cachedDeadBodies)
        {
            if (!IsTrackableDeadBody(dead)) continue;
            _trackedParentIds.Add(dead.ParentId);
        }

        _arrowRemoveBuffer.Clear();
        foreach (var arrowEntry in _deadBodyArrows)
        {
            if (arrowEntry.Key == null)
            {
                if (arrowEntry.Value.arrow?.arrow != null)
                    UnityEngine.Object.Destroy(arrowEntry.Value.arrow.arrow);
                _arrowRemoveBuffer.Add(arrowEntry.Key);
                continue;
            }

            int parentId = arrowEntry.Key.ParentId;
            if (_trackedParentIds.Contains(parentId))
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
                _arrowRemoveBuffer.Add(arrowEntry.Key);
            }
        }
        for (int i = 0; i < _arrowRemoveBuffer.Count; i++)
            _deadBodyArrows.Remove(_arrowRemoveBuffer[i]);

        foreach (DeadBody dead in _cachedDeadBodies)
        {
            if (!IsTrackableDeadBody(dead)) continue;
            bool exists = false;
            foreach (DeadBody key in _deadBodyArrows.Keys)
            {
                if (key != null && key.ParentId == dead.ParentId)
                {
                    exists = true;
                    break;
                }
            }
            if (exists) continue;

            // 新しい死体に対して矢印を追加（既に同じParentIdの矢印が存在しなければ）
            Color arrowColor = _deadBodyColorMode == DeadBodyColorMode.None ? _defaultArrowColor : ResolveDeadBodyArrowColor(dead);
            _deadBodyArrows.Add(dead, (new Arrow(arrowColor), arrowColor));
            _deadBodyArrows[dead].arrow.Update(dead.transform.position, arrowColor);
            _deadBodyArrows[dead].arrow.arrow.SetActive(true);
        }
    }

    private static int CountDeadPlayers()
    {
        int count = 0;
        var players = PlayerControl.AllPlayerControls;
        if (players == null)
            return 0;
        foreach (var player in players)
        {
            if (player != null && player.Data != null && player.Data.IsDead)
                count++;
        }
        return count;
    }

    private static bool IsTrackableDeadBody(DeadBody dead)
    {
        return dead != null
            && !OrpheusMainAbility.IsManagedCorpseBody(dead)
            && IsTrackableDeadBodyPosition(dead.transform.position);
    }

    internal static bool IsTrackableDeadBodyPosition(Vector3 position)
    {
        return IsTrackableDeadBodyPosition(position.x, position.y);
    }

    internal static bool IsTrackableDeadBodyPosition(float x, float y)
    {
        return !DeadBodyVisibility.IsHiddenPosition(x, y);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();

        // 矢印を削除
        foreach (var (arrow, color) in _deadBodyArrows.Values)
        {
            if (arrow?.arrow != null)
                UnityEngine.Object.Destroy(arrow.arrow);
        }
        _deadBodyArrows.Clear();
        _trackedParentIds.Clear();
        _lastDeadCount = -1;
        _scanCounter = 0;
        _cachedDeadBodies = System.Array.Empty<DeadBody>();
    }

    /// <summary>死体用矢印の色をモードに応じて取得する</summary>
    /// <param name="db">色を取得したい死体</param>
    /// <returns>死体用矢印の色</returns>
    private Color ResolveDeadBodyArrowColor(DeadBody db)
    {
        var exp = ExPlayerControl.ById(db.ParentId);
        int deadBodyColorId;

        if (exp == null || exp.Data == null) return _defaultArrowColor;

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
