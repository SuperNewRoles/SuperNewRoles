using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using UnityEngine.Rendering;

namespace SuperNewRoles.CustomObject;

public class RocketLauncherProjectile : MonoBehaviour
{
    private const float Speed = 7.5f;
    private const float MaxLifetime = 8f;
    private const float BaseColliderRadius = 0.28f;
    private const float ProjectileCollisionScale = 0.65f;
    private const float ProjectileVisualScale = 0.6f;
    private const float ReflectPushDistance = 0.12f;
    private const float IgnoreWiseManDuration = 0.15f;
    private const float IgnoreSourceDuration = 0.25f;
    private const float WallHitInset = 0.03f;
    private const float WallDeadBodyBackoff = 0.55f;
    private const float WallDeadBodyClearanceRadius = 0.5f;
    private const float WallOverlapResolveStep = 0.08f;
    private const int WallOverlapResolveMaxCount = 18;
    private const string ProjectileSpriteName = "RocketLauncherProjectile.png";
    private const float ProjectileSpriteForwardAngleOffset = 180f;
    private static readonly float[] ReflectAngles = [135f, 90f, 270f, 225f];

    private RocketLauncherButtonAbility _ability;
    private ExPlayerControl _source;
    private ExPlayerControl _target;
    private Vector2 _direction;
    private bool _collideWithPlayers;
    private bool _isActive;
    private float _lifetime;
    private float _ignoreWiseManTimer;
    private float _ignoreSourceTimer;
    private byte _ignoreWiseManId = byte.MaxValue;
    private SpriteRenderer _spriteRenderer;
    private CircleCollider2D _collider;

    public bool IsActive => _isActive;
    public ExPlayerControl Target => _target;
    public Vector2 Position => transform.position;

    public static RocketLauncherProjectile Spawn(
        RocketLauncherButtonAbility ability,
        ExPlayerControl source,
        ExPlayerControl target,
        Vector2 position,
        Vector2 direction,
        bool collideWithPlayers)
    {
        GameObject gameObject = new("RocketLauncherProjectile");
        var projectile = gameObject.AddComponent<RocketLauncherProjectile>();
        projectile.Init(ability, source, target, position, direction, collideWithPlayers);
        return projectile;
    }

    private void Init(
        RocketLauncherButtonAbility ability,
        ExPlayerControl source,
        ExPlayerControl target,
        Vector2 position,
        Vector2 direction,
        bool collideWithPlayers)
    {
        _ability = ability;
        _source = source;
        _target = target;
        _direction = direction.sqrMagnitude <= 0f ? Vector2.right : direction.normalized;
        _collideWithPlayers = collideWithPlayers;
        _isActive = true;
        _lifetime = 0f;
        _ignoreWiseManTimer = 0f;
        _ignoreSourceTimer = IgnoreSourceDuration;
        _ignoreWiseManId = byte.MaxValue;

        SetupComponents();
        transform.position = new Vector3(position.x, position.y, -4.5f);
        transform.localScale = Vector3.one;
        UpdateRotation();

        _spriteRenderer.sprite = AssetManager.GetAsset<Sprite>(ProjectileSpriteName);
        ApplyTargetColor();
        RocketLauncherButtonAbility.SetTargetVisible(_target, false);
        RocketLauncherButtonAbility.SetTargetMoveable(_target, false);
        MoveTo(position);
    }

    private void SetupComponents()
    {
        var spriteObject = new GameObject("Sprite");
        spriteObject.transform.SetParent(transform, false);
        spriteObject.transform.localScale = Vector3.one * ProjectileVisualScale;
        spriteObject.layer = gameObject.layer;

        _spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        _spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(_spriteRenderer, false);

        _collider = gameObject.AddComponent<CircleCollider2D>();
        _collider.isTrigger = true;
        _collider.radius = BaseColliderRadius * ProjectileCollisionScale;
    }

    private void ApplyTargetColor()
    {
        if (_target?.Data == null || _spriteRenderer == null)
            return;
        PlayerMaterial.SetColors(_target.Data.DefaultOutfit.ColorId, _spriteRenderer);
    }

    private void Update()
    {
        if (!_isActive)
            return;
        if (_source == null || !_source.IsAlive())
        {
            Detach(restoreTarget: true);
            return;
        }
        if (_target == null || !_target.IsAlive())
        {
            Detach(restoreTarget: true);
            return;
        }
        if (MeetingHud.Instance != null)
            return;

        UpdateIgnoreWiseManTimer();
        UpdateIgnoreSourceTimer();

        Vector2 currentPosition = transform.position;
        Vector2 step = _direction * Speed * Time.deltaTime;
        if (step.sqrMagnitude <= 0f)
            return;

        Vector2 nextPosition = currentPosition + step;
        if (TryGetWallHitPosition(currentPosition, step, out var wallHitPosition))
        {
            _ability?.RequestExplodeFromProjectile(this, wallHitPosition);
            return;
        }

        MoveTo(nextPosition);
        _lifetime += Time.deltaTime;
        if (_lifetime >= MaxLifetime)
        {
            _ability?.RequestExplodeFromProjectile(this, nextPosition);
            return;
        }

        if (_collideWithPlayers)
            TryHandlePlayerCollision(nextPosition);
    }

    public void Reflect(ExPlayerControl wiseMan, Vector2 position, Vector2 direction)
    {
        if (!_isActive)
            return;

        _direction = direction.sqrMagnitude <= 0f ? -_direction : direction.normalized;
        _lifetime = 0f;
        _ignoreWiseManId = wiseMan?.PlayerId ?? byte.MaxValue;
        _ignoreWiseManTimer = IgnoreWiseManDuration;
        UpdateRotation();
        MoveTo(position + (_direction * ReflectPushDistance));
    }

    public void Detach(bool restoreTarget)
    {
        if (!_isActive)
            return;

        _isActive = false;
        if (restoreTarget)
            RocketLauncherButtonAbility.RestoreTargetControl(_target);

        _ability = null;
        _source = null;
        _target = null;
        Destroy(gameObject);
    }

    private void MoveTo(Vector2 position)
    {
        transform.position = new Vector3(position.x, position.y, -4.5f);
        RocketLauncherButtonAbility.MoveTargetTo(_target, position);
        RocketLauncherButtonAbility.SetTargetVisible(_target, false);
    }

    private void UpdateRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, GetSpriteAngle(_direction));
    }

    internal static float GetSpriteAngle(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0f)
            direction = Vector2.right;
        return (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + ProjectileSpriteForwardAngleOffset;
    }

    internal static bool TryGetLaunchPathHitPosition(Vector2 sourcePosition, Vector2 launchPosition, out Vector2 hitPosition)
    {
        hitPosition = default;
        Vector2 step = launchPosition - sourcePosition;
        float distance = step.magnitude;
        if (distance <= 0f)
            return false;

        Vector2 direction = step / distance;
        float radius = BaseColliderRadius * ProjectileCollisionScale;
        float castDistance = distance + radius;
        bool hitByLine = PhysicsHelpers.AnyNonTriggersBetween(sourcePosition, direction, castDistance, Constants.ShipAndAllObjectsMask);
        bool hitByOverlap = IsOverlappingWall(launchPosition, radius);
        if (!hitByLine && !hitByOverlap)
            return false;

        float nearestDistance = GetNearestWallHitDistance(sourcePosition, direction, castDistance);
        if (nearestDistance < float.MaxValue)
            hitPosition = GetWallSafeExplosionPosition(sourcePosition, direction, nearestDistance, radius);
        else
            hitPosition = ResolveWallOverlap(launchPosition, -direction);

        return true;
    }

    private void UpdateIgnoreWiseManTimer()
    {
        if (_ignoreWiseManTimer <= 0f)
            return;

        _ignoreWiseManTimer -= Time.deltaTime;
        if (_ignoreWiseManTimer <= 0f)
            _ignoreWiseManId = byte.MaxValue;
    }

    private void UpdateIgnoreSourceTimer()
    {
        if (_ignoreSourceTimer <= 0f)
            return;

        _ignoreSourceTimer -= Time.deltaTime;
    }

    private bool TryHandlePlayerCollision(Vector2 position)
    {
        var hits = Physics2D.OverlapCircleAll(position, GetCollisionRadius(), Constants.PlayersOnlyMask);
        foreach (var hit in hits)
        {
            var target = GetCollisionTarget(hit);
            if (target == null)
                continue;
            if (TryHandleWiseManGuard(target, position))
                return true;
        }

        foreach (var hit in hits)
        {
            var target = GetCollisionTarget(hit);
            if (target == null)
                continue;

            _ability?.RequestExplodeFromProjectile(this, position);
            return true;
        }

        return false;
    }

    private bool TryGetWallHitPosition(Vector2 currentPosition, Vector2 step, out Vector2 hitPosition)
    {
        hitPosition = default;
        float distance = step.magnitude;
        if (distance <= 0f)
            return false;

        Vector2 direction = step / distance;
        float radius = GetCollisionRadius();
        float castDistance = distance + radius;
        bool hitByLine = PhysicsHelpers.AnyNonTriggersBetween(currentPosition, direction, castDistance, Constants.ShipAndAllObjectsMask);
        bool hitByOverlap = IsOverlappingWall(currentPosition + step, radius);
        if (!hitByLine && !hitByOverlap)
            return false;

        float nearestDistance = GetNearestWallHitDistance(currentPosition, direction, castDistance);
        if (nearestDistance < float.MaxValue)
            hitPosition = GetWallSafeExplosionPosition(currentPosition, direction, nearestDistance, radius);
        else
            hitPosition = ResolveWallOverlap(currentPosition + step, -direction);

        return true;
    }

    private static Vector2 GetWallSafeExplosionPosition(Vector2 currentPosition, Vector2 direction, float nearestDistance, float radius)
    {
        float backoff = Mathf.Max(WallDeadBodyBackoff, radius + WallHitInset);
        Vector2 position = currentPosition + (direction * (nearestDistance - backoff));
        return ResolveWallOverlap(position, -direction);
    }

    private static float GetNearestWallHitDistance(Vector2 currentPosition, Vector2 direction, float distance)
    {
        float nearestDistance = float.MaxValue;
        var hits = Physics2D.RaycastAll(currentPosition, direction, distance, Constants.ShipAndAllObjectsMask);
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.isTrigger)
                continue;
            if (hit.distance >= nearestDistance)
                continue;

            nearestDistance = hit.distance;
        }

        return nearestDistance;
    }

    private static bool IsOverlappingWall(Vector2 position, float radius)
    {
        var hits = Physics2D.OverlapCircleAll(position, radius, Constants.ShipAndAllObjectsMask);
        foreach (var hit in hits)
        {
            if (hit != null && !hit.isTrigger)
                return true;
        }

        return false;
    }

    private static Vector2 ResolveWallOverlap(Vector2 position, Vector2 escapeDirection)
    {
        if (escapeDirection.sqrMagnitude <= 0f)
            return position;

        Vector2 normalizedEscapeDirection = escapeDirection.normalized;
        Vector2 resolvedPosition = position;
        for (int i = 0; i < WallOverlapResolveMaxCount && IsOverlappingWall(resolvedPosition, WallDeadBodyClearanceRadius); i++)
        {
            resolvedPosition += normalizedEscapeDirection * WallOverlapResolveStep;
        }

        if (!IsOverlappingWall(resolvedPosition, WallDeadBodyClearanceRadius))
            return resolvedPosition;

        foreach (var candidate in EnumerateClearanceCandidates(position, normalizedEscapeDirection))
        {
            if (!IsOverlappingWall(candidate, WallDeadBodyClearanceRadius))
                return candidate;
        }

        return resolvedPosition;
    }

    private static IEnumerable<Vector2> EnumerateClearanceCandidates(Vector2 position, Vector2 preferredDirection)
    {
        Vector2[] directions =
        [
            preferredDirection,
            (preferredDirection + Vector2.up).normalized,
            Vector2.up,
            (-preferredDirection + Vector2.up).normalized,
            -preferredDirection,
            (preferredDirection + Vector2.down).normalized,
            Vector2.down,
            (-preferredDirection + Vector2.down).normalized
        ];

        for (int i = 1; i <= WallOverlapResolveMaxCount; i++)
        {
            float distance = i * WallOverlapResolveStep;
            foreach (var direction in directions)
            {
                if (direction.sqrMagnitude <= 0f)
                    continue;
                yield return position + (direction.normalized * distance);
            }
        }
    }

    private ExPlayerControl GetCollisionTarget(Collider2D hit)
    {
        if (hit == null)
            return null;

        var playerControl = hit.GetComponentInParent<PlayerControl>();
        if (playerControl == null)
            return null;
        if (_source != null && playerControl.PlayerId == _source.PlayerId && _ignoreSourceTimer > 0f)
            return null;
        if (_ignoreWiseManTimer > 0f && playerControl.PlayerId == _ignoreWiseManId)
            return null;
        if (_target != null && playerControl.PlayerId == _target.PlayerId)
            return null;

        var target = (ExPlayerControl)playerControl;
        if (target == null || !target.IsAlive() || playerControl.inVent)
            return null;

        return target;
    }

    private bool TryHandleWiseManGuard(ExPlayerControl target, Vector2 position)
    {
        if (target == null)
            return false;
        if (_ignoreWiseManTimer > 0f && _ignoreWiseManId == target.PlayerId)
            return false;
        if (!target.TryGetAbility<WiseManAbility>(out var wiseManAbility))
            return false;
        if (!wiseManAbility.Active || wiseManAbility.Guarded)
            return false;

        _ability?.RequestReflectFromProjectile(this, target, position, GetReflectDirection());
        return true;
    }

    private Vector2 GetReflectDirection()
    {
        float angle = ReflectAngles[Random.Range(0, ReflectAngles.Length)];
        if (_direction.x < -0.01f)
            angle = 180f - angle;

        float radian = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
    }

    private float GetCollisionRadius()
    {
        if (_collider == null)
            return BaseColliderRadius;
        return _collider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
    }
}
