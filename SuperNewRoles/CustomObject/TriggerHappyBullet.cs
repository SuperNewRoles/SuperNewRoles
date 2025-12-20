using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class TriggerHappyBullet : MonoBehaviour
{
    private const float BulletSpeed = 10f;
    private ExPlayerControl owner;
    private TriggerHappyAbility ability;
    private Vector2 direction;
    private float range;
    private bool pierceWalls;
    private float traveled;
    private CircleCollider2D collider2D;

    public static TriggerHappyBullet Spawn(
        ExPlayerControl owner,
        TriggerHappyAbility ability,
        Vector2 position,
        Vector2 direction,
        float range,
        float size,
        bool pierceWalls)
    {
        GameObject gameObject = new("TriggerHappyBullet");
        var bullet = gameObject.AddComponent<TriggerHappyBullet>();
        bullet.Init(owner, ability, position, direction, range, size, pierceWalls);
        return bullet;
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

        transform.position = new Vector3(position.x, position.y, -4.5f);
        transform.localScale = Vector3.one * Mathf.Max(0.1f, size) * 0.2f;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(this.direction.y, this.direction.x) * Mathf.Rad2Deg);

        var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetManager.GetAsset<Sprite>("TriggerHappy_Bullet.png");

        collider2D = gameObject.AddComponent<CircleCollider2D>();
        collider2D.isTrigger = true;
        collider2D.radius = 0.15f;
    }

    private void Update()
    {
        if (owner == null || !owner.IsAlive() || MeetingHud.Instance != null)
        {
            Destroy(gameObject);
            return;
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
                Destroy(gameObject);
                return;
            }
        }

        transform.position = nextPosition;
        traveled += step.magnitude;
        if (traveled >= range)
        {
            Destroy(gameObject);
            return;
        }

        if (TryHitPlayer(nextPosition))
        {
            Destroy(gameObject);
        }
    }

    private bool TryHitPlayer(Vector2 position)
    {
        float radius = collider2D.radius * transform.lossyScale.x;
        foreach (var hit in Physics2D.OverlapCircleAll(position, radius, Constants.PlayersOnlyMask))
        {
            var playerControl = hit.GetComponentInParent<PlayerControl>();
            if (playerControl == null || playerControl.PlayerId == owner?.PlayerId)
                continue;

            var target = (ExPlayerControl)playerControl;
            if (target == null || target.IsDead() || playerControl.inVent)
                continue;

            if (!pierceWalls)
            {
                Vector2 targetPosition = target.GetTruePosition();
                Vector2 toTarget = targetPosition - position;
                float distance = toTarget.magnitude;
                if (distance > 0.01f && PhysicsHelpers.AnyNonTriggersBetween(position, toTarget.normalized, distance, Constants.ShipAndObjectsMask))
                    continue;
            }

            ability?.RegisterHit(target);
            return true;
        }

        return false;
    }
}
