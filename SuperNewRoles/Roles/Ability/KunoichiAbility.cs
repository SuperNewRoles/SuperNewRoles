using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Ability;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Roles.Ability.CustomButton;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SuperNewRoles.Roles.Ability;

/// <summary>
/// Container that wires Kunoichi's buttons and disables the vanilla kill button.
/// </summary>
public class KunoichiAbility : AbilityBase
{
    private readonly float _coolTime;
    private readonly int _killKunai;
    private readonly bool _canThrowWhileInvisible;
    private readonly bool _hideEnabled;
    private readonly float _hideTime;
    private readonly bool _requirePressToHide;

    public KunoichiHideAbility HideAbility { get; private set; }
    public KunoichiKunaiAbility KunaiAbility { get; private set; }
    public KunoichiKunaiDisplayAbility DisplayAbility { get; private set; }
    public KunoichiKunaiHitUIAbility HitUIAbility { get; private set; }

    public KunoichiAbility(float coolTime, int killKunai, bool canThrowWhileInvisible, bool hideEnabled, float hideTime, bool requirePressToHide)
    {
        _coolTime = coolTime;
        _killKunai = killKunai;
        _canThrowWhileInvisible = canThrowWhileInvisible;
        _hideEnabled = hideEnabled;
        _hideTime = hideTime;
        _requirePressToHide = requirePressToHide;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        HideAbility = _hideEnabled && _hideTime > 0
            ? new KunoichiHideAbility(_hideTime, _requirePressToHide)
            : null;

        KunaiAbility = new KunoichiKunaiAbility(
            _coolTime,
            _killKunai,
            () => HideAbility?.IsInvisible == true,
            _canThrowWhileInvisible);
        HitUIAbility = new KunoichiKunaiHitUIAbility(
            () => KunaiAbility?.GetCurrentHits() ?? Enumerable.Empty<(ExPlayerControl target, int count)>(),
            () => KunaiAbility?.KillThreshold ?? 0);
        DisplayAbility = new KunoichiKunaiDisplayAbility(_canThrowWhileInvisible, () => HideAbility?.IsInvisible == true);

        Player.AttachAbility(KunaiAbility, new AbilityParentAbility(this));
        Player.AttachAbility(DisplayAbility, new AbilityParentAbility(this));
        Player.AttachAbility(HitUIAbility, new AbilityParentAbility(this));

        if (HideAbility != null)
        {
            Player.AttachAbility(HideAbility, new AbilityParentAbility(this));
        }

        // Disable the vanilla kill button; Kunoichi relies on kunai hits.
        Player.AttachAbility(new KillableAbility(() => false), new AbilityParentAbility(this));
    }
}

/// <summary>
/// Kunai throw button: a line-based invisible projectile that stacks hits and kills on threshold.
/// </summary>
public class KunoichiKunaiAbility : CustomButtonBase
{
    private readonly float _coolTime;
    private readonly int _killThreshold;
    private readonly Func<bool> _isInvisible;
    private readonly bool _allowWhileInvisible;

    private EventListener<WrapUpEventData> _wrapUpEvent;
    private EventListener<MeetingStartEventData> _meetingStartEvent;
    private EventListener _fixedUpdateEvent;

    private static readonly Dictionary<byte, Dictionary<byte, int>> HitCounts = new();
    public int KillThreshold => _killThreshold;

    private readonly List<KunaiProjectile> _projectiles = new();

    private class KunaiProjectile
    {
        public GameObject Obj;
        public Vector2 Velocity;
        public float SpawnTime;
    }

    public KunoichiKunaiAbility(float coolTime, int killThreshold, Func<bool> isInvisible, bool allowWhileInvisible)
    {
        _coolTime = Mathf.Max(0.001f, coolTime);
        _killThreshold = Mathf.Max(1, killThreshold);
        _isInvisible = isInvisible;
        _allowWhileInvisible = allowWhileInvisible;
    }

    public override Sprite Sprite => FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic.sprite;
    public override string buttonText => ModTranslation.GetString("KunoichiKunaiButtonName");
    protected override KeyType keytype => KeyType.Kill;
    public override float DefaultTimer => _coolTime;
    public override bool IsFirstCooldownTenSeconds => false;

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _wrapUpEvent = WrapUpEvent.Instance.AddListener(_ => ClearDeadHits());
        _meetingStartEvent = MeetingStartEvent.Instance.AddListener(_ => { ClearDeadHits(); DestroyProjectiles(); });
        _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }


    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _wrapUpEvent?.RemoveListener();
        _meetingStartEvent?.RemoveListener();
        _fixedUpdateEvent?.RemoveListener();
        DestroyProjectiles();
        ClearLocalHits();
    }

    public override bool CheckIsAvailable()
    {
        if (Player == null || !Player.IsAlive()) return false;
        if (MeetingHud.Instance != null) return false;
        if (Player.Player == null || Player.Player.inVent || !Player.Player.CanMove) return false;
        if (!_allowWhileInvisible && _isInvisible != null && _isInvisible()) return false;
        return true;
    }

    public override void OnClick()
    {
        if (!CheckIsAvailable()) return;
        SpawnProjectile();
    }

    public IEnumerable<(ExPlayerControl target, int count)> GetCurrentHits()
    {
        if (Player == null) yield break;
        if (!HitCounts.TryGetValue(Player.PlayerId, out var map)) yield break;
        foreach (var kv in map)
        {
            if (kv.Value <= 0) continue;
            var target = ExPlayerControl.ById(kv.Key);
            if (target == null || target.Data == null) continue;
            yield return (target, kv.Value);
        }
    }

    private void ClearLocalHits()
    {
        HitCounts.Remove(Player?.PlayerId ?? byte.MaxValue);
    }

    private void ClearDeadHits()
    {
        if (Player == null) return;
        if (!HitCounts.TryGetValue(Player.PlayerId, out var map)) return;

        List<byte> removeKeys = null;
        foreach (var kv in map)
        {
            if (kv.Value <= 0)
            {
                removeKeys ??= new List<byte>();
                removeKeys.Add(kv.Key);
                continue;
            }

            var target = ExPlayerControl.ById(kv.Key);
            if (target == null || target.Data == null || target.Data.IsDead)
            {
                removeKeys ??= new List<byte>();
                removeKeys.Add(kv.Key);
            }
        }

        if (removeKeys == null) return;

        foreach (var key in removeKeys)
        {
            map.Remove(key);
        }

        if (map.Count == 0)
        {
            HitCounts.Remove(Player.PlayerId);
        }
    }

    private void SpawnProjectile()
    {
        var shooter = Player?.Player;
        if (shooter == null) return;

        var sprite = AssetManager.GetAsset<Sprite>("KunoichiKunai.png");
        var go = new GameObject("KunoichiKunaiProjectile");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 310;

        var start = shooter.GetTruePosition();
        Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f);
        if (mouseDirection == Vector3.zero) mouseDirection = Vector3.right;
        float mouseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);
        Vector2 dir = new(Mathf.Cos(mouseAngle), Mathf.Sin(mouseAngle));
        if (dir == Vector2.zero) dir = Vector2.right;

        const float handOffset = 0.8f;   // 手元の表示位置に合わせる
        const float heightOffset = 0.35f;
        var spawnPos = start + dir * handOffset + new Vector2(0f, heightOffset);
        go.transform.position = new Vector3(spawnPos.x, spawnPos.y, shooter.transform.position.z);

        go.transform.rotation = Quaternion.Euler(0f, 0f, mouseAngle * Mathf.Rad2Deg);
        go.transform.localScale = Vector3.one * 0.5f;

        _projectiles.Add(new KunaiProjectile
        {
            Obj = go,
            Velocity = dir * 10f,
            SpawnTime = Time.time
        });
    }

    private void OnFixedUpdate()
    {
        if (_projectiles.Count == 0) return;
        var shooter = Player;
        if (shooter == null || shooter.Player == null)
        {
            DestroyProjectiles();
            return;
        }

        float maxLife = 1.5f;
        float maxDistance = 7f;
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var p = _projectiles[i];
            if (p.Obj == null)
            {
                _projectiles.RemoveAt(i);
                continue;
            }

            p.Obj.transform.position += (Vector3)(p.Velocity * Time.fixedDeltaTime);

            if (Time.time - p.SpawnTime > maxLife || Vector2.Distance(shooter.GetTruePosition(), (Vector2)p.Obj.transform.position) > maxDistance)
            {
                UnityEngine.Object.Destroy(p.Obj);
                _projectiles.RemoveAt(i);
                continue;
            }

            foreach (var candidate in PlayerControl.AllPlayerControls.ToArray())
            {
                if (candidate == null || candidate.PlayerId == shooter.PlayerId) continue;
                if (candidate.Data == null || candidate.Data.IsDead) continue;
                if (candidate.inVent) continue;

                var dist = Vector2.Distance(candidate.GetTruePosition() + new Vector2(0, 0.4f), (Vector2)p.Obj.transform.position);
                if (dist < 0.35f)
                {
                    HandleHit(candidate);
                    UnityEngine.Object.Destroy(p.Obj);
                    _projectiles.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private void HandleHit(PlayerControl target)
    {
        if (target == null) return;
        var shooterId = Player.PlayerId;
        var targetId = target.PlayerId;

        if (!HitCounts.TryGetValue(shooterId, out var map))
        {
            map = new Dictionary<byte, int>();
            HitCounts[shooterId] = map;
        }

        map.TryGetValue(targetId, out var current);
        current++;
        map[targetId] = current;

        var targetEx = (ExPlayerControl)target;
        if (targetEx == null) return;

        if (current >= _killThreshold)
        {
            Player.RpcCustomDeath(targetEx, CustomDeathType.KnifeKill);
            map[targetId] = 0;
        }
    }

    private void DestroyProjectiles()
    {
        foreach (var p in _projectiles)
        {
            if (p.Obj != null) UnityEngine.Object.Destroy(p.Obj);
        }
        _projectiles.Clear();
    }
}

/// <summary>
/// 命中したプレイヤー一覧を表示するUI。ShowPlayerUIAbility風でカウントを重ねて表示。
/// </summary>
public class KunoichiKunaiHitUIAbility : AbilityBase
{
    private readonly Func<IEnumerable<(ExPlayerControl target, int count)>> _getHits;
    private readonly Func<int> _getThreshold;
    private GameObject _container;
    private readonly List<(byte playerId, int count)> _lastHits = new();
    private readonly List<PoolablePlayer> _uiObjects = new();
    private EventListener _fixedUpdateListener;

    public KunoichiKunaiHitUIAbility(
        Func<IEnumerable<(ExPlayerControl target, int count)>> getHits,
        Func<int> getThreshold)
    {
        _getHits = getHits;
        _getThreshold = getThreshold;
    }

    public override void AttachToLocalPlayer()
    {
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _container = new GameObject("KunoichiHitUIContainer");
        _container.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
        _container.transform.localPosition = new(-4.19f, -2.4f, 0f);
        _container.transform.localScale = Vector3.one * 0.3f;
        var aspectPosition = _container.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspectPosition.DistanceFromEdge = new(0.35f, 0.35f);
        aspectPosition.OnEnable();
    }

    public override void DetachToLocalPlayer()
    {
        _fixedUpdateListener?.RemoveListener();
        if (_container != null)
        {
            GameObject.Destroy(_container);
            _container = null;
        }
        _uiObjects.Clear();
    }

    private void OnFixedUpdate()
    {
        if (_container == null) return;

        bool shouldHide = FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null;
        if (shouldHide)
        {
            if (_container.activeSelf) _container.SetActive(false);
            return;
        }

        int threshold = Mathf.Max(0, _getThreshold?.Invoke() ?? 0);
        if (threshold <= 0)
        {
            if (_container.activeSelf) _container.SetActive(false);
            return;
        }

        var hits = (_getHits?.Invoke() ?? Enumerable.Empty<(ExPlayerControl target, int count)>())
            .Where(h => h.target != null && h.target.Data != null && h.count > 0)
            .Select(h => (h.target, h.count))
            .OrderByDescending(h => h.count)
            .ThenBy(h => h.target.PlayerId)
            .ToList();

        if (hits.Count == 0)
        {
            if (_container.activeSelf) _container.SetActive(false);
            _lastHits.Clear();
            return;
        }

        if (!_container.activeSelf) _container.SetActive(true);

        bool updated = hits.Count != _lastHits.Count;
        if (!updated)
        {
            for (int i = 0; i < hits.Count; i++)
            {
                if (_lastHits[i].playerId != hits[i].target.PlayerId || _lastHits[i].count != hits[i].count)
                {
                    updated = true;
                    break;
                }
            }
        }

        if (!updated) return;

        var prefab = FastDestroyableSingleton<HudManager>.Instance.IntroPrefab.PlayerPrefab;
        for (int i = 0; i < hits.Count; i++)
        {
            var hit = hits[i];
            PoolablePlayer uiObj;
            if (i < _uiObjects.Count)
            {
                uiObj = _uiObjects[i];
                uiObj.gameObject.SetActive(true);
            }
            else
            {
                uiObj = GameObject.Instantiate(prefab, _container.transform);
                _uiObjects.Add(uiObj);
                uiObj.gameObject.SetActive(true);
            }

            uiObj.UpdateFromEitherPlayerDataOrCache(hit.target.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
            uiObj.transform.localPosition = new(i * 1.5f, 0f, -0.3f);

            if (uiObj.cosmetics.nameText != null)
            {
                uiObj.cosmetics.nameText.text = $"{hit.count}";
                uiObj.cosmetics.nameText.enableWordWrapping = false;
                uiObj.cosmetics.nameText.color = hit.count >= threshold ? Palette.EnabledColor : Color.white;
                uiObj.cosmetics.nameText.transform.localScale = Vector3.one * 6f;
                uiObj.cosmetics.nameText.gameObject.SetActive(true);
            }
        }

        for (int i = hits.Count; i < _uiObjects.Count; i++)
        {
            _uiObjects[i].gameObject.SetActive(false);
        }

        _lastHits.Clear();
        foreach (var hit in hits)
        {
            _lastHits.Add((hit.target.PlayerId, hit.count));
        }
    }
}

/// <summary>
/// Handles invisibility based on stand-still time. Supports both auto-hide and button-trigger modes.
/// </summary>
public class KunoichiHideAbility : CustomButtonBase, IButtonEffect
{
    private readonly float _hideWait;
    private readonly bool _requirePress;

    private float _stopTimer;
    private Vector2 _lastPosition;

    private EventListener _fixedUpdateEvent;
    private EventListener<MeetingStartEventData> _meetingStartEvent;
    private readonly OpacityFadeController _opacityFader = new();

    public bool IsInvisible { get; private set; }

    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public Action OnEffectEnds => () => ExitInvisibility();
    public float EffectDuration => _hideWait;
    public override bool CheckHasButton() => _requirePress && base.CheckHasButton();
    public bool effectCancellable => true;
    public bool IsEffectDurationInfinity => true;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("KunoichiHideButton.png");
    public override string buttonText => ModTranslation.GetString("KunoichiHideButtonName");
    protected override KeyType keytype => KeyType.None;
    public override float DefaultTimer => 0f;

    public KunoichiHideAbility(float hideWait, bool requirePress)
    {
        _hideWait = Mathf.Max(0f, hideWait);
        _requirePress = requirePress;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _meetingStartEvent = MeetingStartEvent.Instance.AddListener(_ => ExitInvisibility());
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _fixedUpdateEvent?.RemoveListener();
        _meetingStartEvent?.RemoveListener();
        _opacityFader.StopAll();
        ExitInvisibility();
    }

    public override bool CheckIsAvailable()
    {
        if (!_requirePress) return false;
        if (Player == null || !Player.IsAlive()) return false;
        if (MeetingHud.Instance != null) return false;
        if (Player.Player == null || !Player.Player.CanMove) return false;
        if (_hideWait <= 0f) return false;
        return !IsInvisible && _stopTimer >= _hideWait;
    }

    public override void OnClick()
    {
        if (!CheckIsAvailable()) return;
        RpcEnterInvisibility();
    }

    private void OnFixedUpdate()
    {
        if (Player == null || Player.Player == null) return;

        if (!Player.IsAlive())
        {
            ExitInvisibility();
            _stopTimer = 0f;
            return;
        }

        var current = Player.GetTruePosition();
        if (Vector2.Distance(current, _lastPosition) < 0.01f)
        {
            _stopTimer += Time.fixedDeltaTime;
        }
        else
        {
            _stopTimer = 0f;
            if (IsInvisible) ExitInvisibility();
        }

        _lastPosition = current;

        if (!_requirePress && _hideWait > 0f && !IsInvisible && MeetingHud.Instance == null && Player.Player.CanMove && _stopTimer >= _hideWait)
        {
            EnterInvisibility();
        }
    }

    [CustomRPC]
    public void RpcEnterInvisibility()
    {
        EnterInvisibility();
    }

    private void EnterInvisibility()
    {
        if (IsInvisible) return;
        IsInvisible = true;
        isEffectActive = true;
        EffectTimer = 0f;
        if (Player?.Player != null)
        {
            bool sameTeam = Player.roleBase.AssignedTeam == ExPlayerControl.LocalPlayer.roleBase.AssignedTeam;
            _opacityFader.Apply(Player, sameTeam ? 0.1f : 0f);
        }
    }

    private void ExitInvisibility()
    {
        if (!IsInvisible) return;
        IsInvisible = false;
        isEffectActive = false;
        if (Player?.Player != null)
        {
            _opacityFader.Apply(Player, 1f);
        }
    }
}

/// <summary>
/// Toggle-only button to show/hide the hand-held kunai (visual aid, no hit logic).
/// </summary>
public class KunoichiKunaiDisplayAbility : CustomButtonBase
{
    private readonly bool _canThrowWhileInvisible;
    private readonly Func<bool> _isInvisible;
    private GameObject _displayKunai;
    private EventListener _fixedUpdateEvent;
    private bool _isShown;
    private bool _hasInitialPosition;
    private Vector3 _followVelocity;

    public KunoichiKunaiDisplayAbility(bool canThrowWhileInvisible, Func<bool> isInvisible)
    {
        _canThrowWhileInvisible = canThrowWhileInvisible;
        _isInvisible = isInvisible;
    }

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("KunoichiKunaiButton.png");
    public override string buttonText => ModTranslation.GetString("KunoichiKunaiDisplayButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => 0f;
    public override bool IsFirstCooldownTenSeconds => false;

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _fixedUpdateEvent?.RemoveListener();
        if (_displayKunai != null) UnityEngine.Object.Destroy(_displayKunai);
    }

    public override bool CheckIsAvailable()
    {
        if (Player == null || !Player.IsAlive()) return false;
        if (Player.Player == null || !Player.Player.CanMove) return false;
        if (!_canThrowWhileInvisible && _isInvisible != null && _isInvisible()) return false;
        return true;
    }

    public override void OnClick()
    {
        if (!CheckIsAvailable()) return;
        _isShown = !_isShown;
        EnsureKunai();

        if (_displayKunai != null) _displayKunai.SetActive(false);

        if (!_isShown)
        {
            _hasInitialPosition = false;
            _followVelocity = Vector3.zero;
            return;
        }

        // 再表示時に「前回位置で一瞬表示」されないよう、表示前に位置を確定させる
        if (TryGetDesired(out var mouseAngle, out var desired))
        {
            _displayKunai.transform.position = desired;
            _displayKunai.transform.eulerAngles = new Vector3(0f, 0f, mouseAngle * Mathf.Rad2Deg);
            var sr = _displayKunai.GetComponent<SpriteRenderer>();
            sr.flipY = Mathf.Cos(mouseAngle) < 0f;
            _hasInitialPosition = true;
            _followVelocity = Vector3.zero;
        }

        if (_displayKunai != null) _displayKunai.SetActive(true);
    }

    private bool TryGetDesired(out float mouseAngle, out Vector3 desired)
    {
        mouseAngle = 0f;
        desired = Vector3.zero;

        if (Player == null || Player.Player == null) return false;

        Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
        if (mouseDirection == Vector3.zero) mouseDirection = Vector3.right;
        mouseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);

        const float handOffset = 0.8f;
        const float heightOffset = 0.35f;
        var targetPosition = Player.GetTruePosition() + new Vector2(handOffset * Mathf.Cos(mouseAngle), handOffset * Mathf.Sin(mouseAngle));
        desired = new Vector3(targetPosition.x, targetPosition.y + heightOffset, Player.transform.position.z);
        return true;
    }

    private void EnsureKunai()
    {
        if (_displayKunai != null) return;
        _displayKunai = new GameObject("KunoichiKunaiDisplay");
        _displayKunai.transform.localScale = Vector3.one * 0.5f;
        var sr = _displayKunai.AddComponent<SpriteRenderer>();
        sr.sprite = AssetManager.GetAsset<Sprite>("KunoichiKunai.png");
        sr.sortingOrder = 305;
    }

    private void OnFixedUpdate()
    {
        if (_displayKunai == null || !_isShown)
        {
            if (_displayKunai != null) _displayKunai.SetActive(false);
            _hasInitialPosition = false;
            _followVelocity = Vector3.zero;
            return;
        }

        if (Player == null || Player.Player == null || !Player.IsAlive() || MeetingHud.Instance != null || Player.Player.inVent)
        {
            _displayKunai.SetActive(false);
            _hasInitialPosition = false;
            _followVelocity = Vector3.zero;
            return;
        }

        Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
        var mouseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);
        const float handOffset = 0.8f;
        const float heightOffset = 0.35f;
        const float followSmoothTime = 0.05f;
        var targetPosition = Player.GetTruePosition() + new Vector2(handOffset * Mathf.Cos(mouseAngle), handOffset * Mathf.Sin(mouseAngle));
        var desired = new Vector3(targetPosition.x, targetPosition.y + heightOffset, Player.transform.position.z);
        if (!_hasInitialPosition)
        {
            _displayKunai.transform.position = desired;
            _hasInitialPosition = true;
            _followVelocity = Vector3.zero;
        }
        else
        {
            _displayKunai.transform.position = Vector3.SmoothDamp(
                _displayKunai.transform.position,
                desired,
                ref _followVelocity,
                followSmoothTime,
                Mathf.Infinity,
                Time.fixedDeltaTime);
        }
        _displayKunai.transform.eulerAngles = new Vector3(0f, 0f, mouseAngle * Mathf.Rad2Deg);

        var sr = _displayKunai.GetComponent<SpriteRenderer>();
        if (Mathf.Cos(mouseAngle) < 0f)
        {
            sr.flipY = true;
        }
        else
        {
            sr.flipY = false;
        }

        // 位置/角度を確定してから表示する（前回位置の一瞬表示を防ぐ）
        if (!_displayKunai.activeSelf) _displayKunai.SetActive(true);
    }
}

