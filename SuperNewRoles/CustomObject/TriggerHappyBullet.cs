using System.Collections.Generic;
using SuperNewRoles.Modules;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class TriggerHappyBullet : MonoBehaviour
{
    private const float BulletSpeed = 10f;
    private const float BounceExcludeAngle = 15f;
    private const float BounceOppositeAngle = 180f;
    private const float BounceAllowedRange = BounceOppositeAngle - (BounceExcludeAngle * 2f);
    private const float BounceTotalRange = BounceAllowedRange * 2f;
    private const float BounceIgnoreDuration = 0.1f;
    private const float GuardEffectCooldown = 0.2f;
    private const int HitBufferSize = 32;
    private const int MaxPoolSize = 256;
    private const float BaseColliderRadius = 0.15f;
    private static readonly Dictionary<byte, GuardEffectState> GuardEffectStates = new();
    private static readonly Stack<TriggerHappyBullet> BulletPool = new();
    private static Sprite BulletSprite;
    private ExPlayerControl owner;
    private TriggerHappyAbility ability;
    private Vector2 direction;
    private float range;
    private bool pierceWalls;
    private float traveled;
    private CircleCollider2D collider2D;
    private SpriteRenderer spriteRenderer;
    private float scaledColliderRadius;
    private Collider2D[] hitBuffer;
    private float ignoreWiseManTimer;
    private byte ignoreWiseManId = byte.MaxValue;
    private bool isActive;

    public bool IsActive => isActive;

    private sealed class GuardEffectState
    {
        public RoleEffectAnimation Effect;
        public float NextAllowedTime;
    }

    public static TriggerHappyBullet Spawn(
        ExPlayerControl owner,
        TriggerHappyAbility ability,
        Vector2 position,
        Vector2 direction,
        float range,
        float size,
        bool pierceWalls)
    {
        var bullet = GetFromPool();
        if (bullet == null)
        {
            GameObject gameObject = new("TriggerHappyBullet");
            bullet = gameObject.AddComponent<TriggerHappyBullet>();
            bullet.SetupComponents();
        }
        else
        {
            bullet.gameObject.SetActive(true);
        }
        bullet.Init(owner, ability, position, direction, range, size, pierceWalls);
        return bullet;
    }

    private static TriggerHappyBullet GetFromPool()
    {
        while (BulletPool.Count > 0)
        {
            var bullet = BulletPool.Pop();
            if (bullet != null && bullet.gameObject != null)
                return bullet;
        }
        return null;
    }

    private void SetupComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        if (collider2D == null)
        {
            collider2D = gameObject.AddComponent<CircleCollider2D>();
            collider2D.isTrigger = true;
            collider2D.radius = BaseColliderRadius;
        }
    }

    private void Init(
        ExPlayerControl owner,
        TriggerHappyAbility ability,
        Vector2 position,
        Vector2 direction,
        float range,
        float size,
        bool pierceWalls)
    {
        this.owner = owner;
        this.ability = ability;
        this.direction = direction.normalized;
        this.range = Mathf.Max(0.1f, range);
        this.pierceWalls = pierceWalls;
        traveled = 0f;
        ignoreWiseManTimer = 0f;
        ignoreWiseManId = byte.MaxValue;
        isActive = true;

        transform.position = new Vector3(position.x, position.y, -4.5f);
        transform.localScale = Vector3.one * Mathf.Max(0.1f, size) * 0.2f;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg);

        SetupComponents();
        if (BulletSprite == null)
            BulletSprite = AssetManager.GetAsset<Sprite>("TriggerHappy_Bullet.png");
        spriteRenderer.sprite = BulletSprite;
        scaledColliderRadius = collider2D.radius * transform.lossyScale.x;
        hitBuffer ??= new Collider2D[HitBufferSize];
    }

    private void Update()
    {
        if (owner == null || !owner.IsAlive() || MeetingHud.Instance != null)
        {
            Despawn();
            return;
        }

        if (ignoreWiseManTimer > 0f)
        {
            ignoreWiseManTimer -= Time.deltaTime;
            if (ignoreWiseManTimer <= 0f)
            {
                ignoreWiseManId = byte.MaxValue;
            }
        }

        var currentPosition = transform.position;
        var step = (Vector3)(direction * BulletSpeed * Time.deltaTime);
        var nextPosition = currentPosition + step;
        if (!pierceWalls && step.sqrMagnitude > 0f)
        {
            float distance = step.magnitude;
            Vector2 currentPosition2D = currentPosition;
            Vector2 stepDirection2D = step.normalized;
            if (PhysicsHelpers.AnyNonTriggersBetween(currentPosition2D, stepDirection2D, distance, Constants.ShipAndObjectsMask))
            {
                Despawn();
                return;
            }
        }

        transform.position = nextPosition;
        traveled += step.magnitude;
        if (traveled >= range)
        {
            Despawn();
            return;
        }

        if (TryHitPlayer(nextPosition))
        {
            Despawn();
        }
    }

    private bool TryHitPlayer(Vector2 position)
    {
        float radius = scaledColliderRadius;
        int hitCount = Physics2D.OverlapCircleNonAlloc(position, radius, hitBuffer, Constants.PlayersOnlyMask);
        if (hitCount == 0)
            return false;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hitBuffer[i];
            if (hit == null)
                continue;
            var playerControl = hit.GetComponentInParent<PlayerControl>();
            if (playerControl == null || playerControl.PlayerId == owner?.PlayerId)
                continue;

            var target = (ExPlayerControl)playerControl;
            if (target == null || target.IsDead() || playerControl.inVent)
                continue;

            var data = ability?.Data;
            if (data != null && !data.FriendlyFire && !IsValidTargetByFriendlyFire(owner, target))
                continue;

            if (!pierceWalls)
            {
                Vector2 targetPosition = target.GetTruePosition();
                Vector2 toTarget = targetPosition - position;
                float distance = toTarget.magnitude;
                if (distance > 0.01f && PhysicsHelpers.AnyNonTriggersBetween(position, toTarget.normalized, distance, Constants.ShipAndObjectsMask))
                    continue;
            }

            if (TryHandleWiseManGuard(target, position))
            {
                return false;
            }

            ability?.RegisterHit(target);
            return true;
        }

        return false;
    }

    private static bool IsValidTargetByFriendlyFire(ExPlayerControl source, ExPlayerControl target)
    {
        if (source == null || target == null) return false;

        // WaveCannonと同様の同陣営/同チーム無効化ルール
        if (ModeManager.IsMode(ModeId.WCBattleRoyal))
        {
            if (WCBattleRoyalMode.Instance.IsOnSameTeam(source.Player, target.Player))
                return false;
        }
        else
        {
            if (source.IsImpostor() && target.IsImpostor())
                return false;
            if (source.IsCrewmate() && target.IsCrewmate())
                return false;
            if (source.IsJackal() && target.IsJackal())
                return false;
        }
        return true;
    }

    private bool TryHandleWiseManGuard(ExPlayerControl target, Vector2 position)
    {
        if (target == null)
            return false;

        if (ignoreWiseManTimer > 0f && ignoreWiseManId == target.PlayerId)
            return true;

        if (!target.TryGetAbility<WiseManAbility>(out var wiseManAbility))
            return false;

        if (!wiseManAbility.Active)
            return false;

        ApplyWiseManBounce(position);
        PlayWiseManGuardEffect(target);

        ignoreWiseManId = target.PlayerId;
        ignoreWiseManTimer = BounceIgnoreDuration;
        return true;
    }

    private void ApplyWiseManBounce(Vector2 position)
    {
        float offset = GetRandomBounceOffset();
        direction = (Quaternion.Euler(0f, 0f, offset) * direction).normalized;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        traveled = 0f;

        float push = 0.05f;
        if (collider2D != null)
        {
            push += collider2D.radius * transform.lossyScale.x;
        }
        transform.position = new Vector3(position.x, position.y, transform.position.z) + (Vector3)(direction * push);
    }

    private static float GetRandomBounceOffset()
    {
        float t = UnityEngine.Random.Range(0f, BounceTotalRange);
        if (t <= BounceAllowedRange)
            return BounceExcludeAngle + t;
        return (BounceOppositeAngle + BounceExcludeAngle) + (t - BounceAllowedRange);
    }

    private static void PlayWiseManGuardEffect(ExPlayerControl wiseMan)
    {
        if (wiseMan?.Player == null)
            return;

        GuardEffectState state = GetGuardEffectState(wiseMan.PlayerId);
        if (IsEffectVisible(state.Effect))
            return;

        state.Effect = null;

        float now = Time.time;
        if (now < state.NextAllowedTime)
            return;

        if (TryFindVisibleProtectEffect(wiseMan, out var existingEffect))
        {
            state.Effect = existingEffect;
            state.NextAllowedTime = now + GuardEffectCooldown;
            return;
        }

        RoleEffectAnimation roleEffectAnimation = GameObject.Instantiate<RoleEffectAnimation>(
            DestroyableSingleton<RoleManager>.Instance.protectAnim,
            wiseMan.Player.gameObject.transform);
        roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(shouldBeVisible: true);
        roleEffectAnimation.Play(wiseMan, null, wiseMan.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
        state.Effect = roleEffectAnimation;
        state.NextAllowedTime = now + GuardEffectCooldown;
    }

    private static GuardEffectState GetGuardEffectState(byte playerId)
    {
        if (!GuardEffectStates.TryGetValue(playerId, out var state))
        {
            state = new GuardEffectState();
            GuardEffectStates[playerId] = state;
        }

        return state;
    }

    private static bool IsEffectVisible(RoleEffectAnimation effect)
    {
        if (effect == null)
            return false;
        if (!effect.gameObject.activeInHierarchy || !effect.isActiveAndEnabled)
            return false;
        if (effect.Renderer != null && !effect.Renderer.enabled)
            return false;
        return true;
    }

    private static bool TryFindVisibleProtectEffect(ExPlayerControl wiseMan, out RoleEffectAnimation effect)
    {
        effect = null;
        if (wiseMan?.Player == null)
            return false;

        var protectAnim = DestroyableSingleton<RoleManager>.Instance.protectAnim;
        if (protectAnim == null || protectAnim.gameObject == null)
            return false;

        string protectName = protectAnim.gameObject.name;
        if (protectName == null || protectName.Length == 0)
            return false;

        var effects = wiseMan.Player.GetComponentsInChildren<RoleEffectAnimation>(true);
        for (int i = 0; i < effects.Length; i++)
        {
            var candidate = effects[i];
            if (candidate == null)
                continue;
            if (!candidate.gameObject.name.StartsWith(protectName))
                continue;
            if (!IsEffectVisible(candidate))
                continue;
            effect = candidate;
            return true;
        }

        return false;
    }

    private void Despawn()
    {
        if (!isActive)
            return;

        isActive = false;
        owner = null;
        ability = null;
        ignoreWiseManTimer = 0f;
        ignoreWiseManId = byte.MaxValue;
        traveled = 0f;

        if (BulletPool.Count < MaxPoolSize)
        {
            gameObject.SetActive(false);
            BulletPool.Push(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
