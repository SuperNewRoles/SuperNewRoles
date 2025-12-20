using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.CustomObject;

public class TriggerHappyGatlingGun : MonoBehaviour
{
    private ExPlayerControl Player;
    private SpriteRenderer spriteRenderer;
    private Sprite[] sprites;
    private float time;
    private int index;
    private int frameRate = 60;
    private TriggerHappyData data;
    private float fireTimer;
    private Vector2 lastAimDirection = Vector2.right;
    private const float FireInterval = 0.001f;
    private const float MuzzleOffset = 0.9f;

    // 角度同期用
    private float targetAngle = 0f;
    private float currentAngle = 0f;
    private const float AngleSmoothTime = 0.1f; // 角度スムージング時間

    // マウス方向追従用
    private bool _hasInitialPosition = false;
    private Vector3 _followVelocity = Vector3.zero;

    private AudioSource audioSource;

    // 振動用
    private float shakeTime = 0f;
    private float shakeIntensity = 0.015f;
    private float shakeSpeed = 12f;

    // 角度を設定するメソッド（RPC同期用）
    public void SetAngle(float angle)
    {
        targetAngle = angle;
    }

    public static TriggerHappyGatlingGun Spawn(ExPlayerControl player, Sprite[] gatlingSprites, TriggerHappyData data)
    {
        if (gatlingSprites.Length == 0)
            throw new ArgumentException("Sprites must not be empty");

        GameObject gameObject = new("TriggerHappyGatlingGun");
        TriggerHappyGatlingGun gatlingGun = gameObject.AddComponent<TriggerHappyGatlingGun>();
        gatlingGun.Init(player, gatlingSprites, data);
        gameObject.transform.SetParent(player.transform);
        return gatlingGun;
    }

    private void Init(ExPlayerControl player, Sprite[] gatlingSprites, TriggerHappyData data)
    {
        Player = player;
        sprites = gatlingSprites;
        this.data = data;

        // SpriteRendererの設定
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[0];

        // 初期位置設定
        transform.localPosition = new Vector3(1.3f, 0.35f, -5f);
        transform.localScale = Vector3.one * 0.56f;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = AssetManager.GetAsset<AudioClip>("TriggerHappy_Bullet_Shot.wav");
        audioSource.loop = true;
        audioSource.spatialBlend = 1f; // 3D音源にする
        audioSource.minDistance = 1f;  // 最小距離
        audioSource.maxDistance = 5f; // 最大距離
        audioSource.rolloffMode = AudioRolloffMode.Linear; // 減衰モード
        audioSource.Play();
    }

    private void FixedUpdate()
    {
        if (Player == null || !Player.IsAlive() || MeetingHud.Instance != null || Player.Player.inVent)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            _hasInitialPosition = false;
            _followVelocity = Vector3.zero;
            return;
        }

        // 角度の計算（オーナーのみマウス方向を使用、それ以外は同期された角度を使用）
        if (Player.AmOwner)
        {
            // マウス方向の計算
            Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
            currentAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);
            targetAngle = currentAngle; // オーナーの場合はtargetAngleも更新
        }
        else
        {
            // 非オーナーの場合は同期された角度にスムージングを適用
            float currentAngleDeg = currentAngle * Mathf.Rad2Deg;
            float targetAngleDeg = targetAngle * Mathf.Rad2Deg;
            float smoothedAngleDeg = Mathf.LerpAngle(currentAngleDeg, targetAngleDeg, Time.fixedDeltaTime / AngleSmoothTime);
            currentAngle = smoothedAngleDeg * Mathf.Deg2Rad;
        }

        lastAimDirection = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)).normalized;

        const float handOffset = 0.8f;
        const float heightOffset = 0.35f;
        const float followSmoothTime = 0.05f;

        var targetPosition = Player.Player.GetTruePosition() + new Vector2(handOffset * Mathf.Cos(currentAngle), handOffset * Mathf.Sin(currentAngle));
        var desired = new Vector3(targetPosition.x, targetPosition.y + heightOffset, -5f);

        if (!_hasInitialPosition)
        {
            transform.position = desired;
            _hasInitialPosition = true;
            _followVelocity = Vector3.zero;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref _followVelocity,
                followSmoothTime,
                Mathf.Infinity,
                Time.fixedDeltaTime);
        }

        // 回転設定
        transform.eulerAngles = new Vector3(0f, 0f, currentAngle * Mathf.Rad2Deg);

        // スプライトの反転設定
        if (Mathf.Cos(currentAngle) < 0f)
        {
            spriteRenderer.flipY = true;
        }
        else
        {
            spriteRenderer.flipY = false;
        }

        // 小刻みな振動を追加（銃の反動のような感じ）
        shakeTime += Time.fixedDeltaTime * shakeSpeed;
        float shakeX = Mathf.Sin(shakeTime) * shakeIntensity * 0.3f; // X軸は小さめに
        float shakeY = Mathf.Sin(shakeTime * 1.7f) * shakeIntensity; // Y軸はメインの振動
        Vector3 shakeOffset = new(shakeX, shakeY, 0f);
        transform.position += shakeOffset;

        // 位置/角度を確定してから表示する
        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }

    private void Update()
    {
        TryFire();
        if (Player != null && Player.TryGetAbility<TriggerHappyAbility>(out var ability))
        {
            ability.TickBatch(Time.deltaTime);
        }

        // アニメーションの更新
        if (sprites.Length == 0 || frameRate <= 0) return;

        time += Time.deltaTime;
        if (time >= 1f / frameRate)
        {
            time = 0;
            index = (index + 1) % sprites.Length;
            spriteRenderer.sprite = sprites[index];
        }
    }

    private void TryFire()
    {
        if (Player == null || !Player.AmOwner)
            return;
        if (MeetingHud.Instance != null || Player.Player.inVent || !Player.IsAlive())
            return;
        if (!Player.TryGetAbility<TriggerHappyAbility>(out var ability) || !ability.isEffectActive)
            return;

        fireTimer -= Time.deltaTime;
        if (fireTimer > 0f)
            return;

        fireTimer = FireInterval;
        Vector2 direction = ApplySpread(lastAimDirection, data?.BulletSpreadAngle ?? 0);
        Vector2 spawnPosition = (Vector2)transform.position + direction * MuzzleOffset;
        ability.QueueBullet(spawnPosition, direction);
    }

    private static Vector2 ApplySpread(Vector2 direction, int spreadAngle)
    {
        if (spreadAngle <= 0)
            return direction;

        float half = spreadAngle * 0.5f;
        float offset = UnityEngine.Random.Range(-half, half);
        return Quaternion.Euler(0f, 0f, offset) * direction;
    }

    private void OnDestroy()
    {
        // リソースの解放
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteRenderer.sprite = null;
        }

        Player = null;
        spriteRenderer = null;
        sprites = null;
        audioSource.Stop();
    }
}
