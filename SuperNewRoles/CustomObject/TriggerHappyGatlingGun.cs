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
    private AttenuatedAudioSource attenuatedAudioSource;

    // 振動用
    private float shakeTime = 0f;
    private float shakeIntensity = 0.015f;
    private float shakeSpeed = 12f;

    // フェード用
    private bool isFiring = false;
    private float targetVolume = 0f;
    private float currentVolume = 0f;
    private const float FadeInSpeed = 0.6f;  // フェードイン速度（秒）
    private const float FadeOutSpeed = 0.4f; // フェードアウト速度（秒）
    private const float MaxVolume = 1f;   // 最大音量

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

        // 減衰対応オーディオソースの設定（staticユーティリティを使用）
        AudioClip clip = AssetManager.GetAsset<AudioClip>("TriggerHappyShotSound.wav");
        attenuatedAudioSource = AttenuatedAudioSourceUtility.SetupSimple(gameObject, clip, loop: true, maxDistance: 5f, minDistance: 1f);
        audioSource = attenuatedAudioSource.GetComponent<AudioSource>();

        // AttenuatedAudioSourceの自動更新を無効化（手動で音量制御するため）
        attenuatedAudioSource.enabled = false;

        audioSource.Play();

        // 初期音量を0に設定（フェードイン開始）
        currentVolume = 0f;
        targetVolume = 0f;
        attenuatedAudioSource.maxVolume = 0f;
        audioSource.volume = 0f; // 最初のフレームで確実に0に設定
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
        // 発射状態の更新とフェード処理
        UpdateFiringState();
        UpdateFade();

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

    private void UpdateFiringState()
    {
        if (Player == null || !Player.TryGetAbility<TriggerHappyAbility>(out var ability))
        {
            isFiring = false;
            return;
        }

        // 発射可能な状態かチェック
        bool canFire = Player.AmOwner
            && MeetingHud.Instance == null
            && !Player.Player.inVent
            && Player.IsAlive()
            && ability.isEffectActive;

        // 発射状態の変化を検出
        if (canFire && !isFiring)
        {
            // フェードイン開始
            isFiring = true;
            targetVolume = MaxVolume;
        }
        else if (!canFire && isFiring)
        {
            // フェードアウト開始
            isFiring = false;
            targetVolume = 0f;
        }
    }

    private void UpdateFade()
    {
        if (attenuatedAudioSource == null || audioSource == null) return;

        // 現在の音量を目標音量に向かってスムーズに変化させる
        float fadeSpeed = targetVolume > currentVolume ? FadeInSpeed : FadeOutSpeed;
        currentVolume = Mathf.MoveTowards(currentVolume, targetVolume, fadeSpeed * Time.deltaTime);

        // 距離ベースの減衰を計算（AttenuatedAudioSourceのロジックを使用）
        float distanceVolume = 1f;
        if (PlayerControl.LocalPlayer != null)
        {
            Vector2 soundPosition = transform.position;
            Vector2 listenerPosition = PlayerControl.LocalPlayer.GetTruePosition();
            float distance = Vector2.Distance(soundPosition, listenerPosition);

            if (distance > attenuatedAudioSource.maxDistance)
            {
                distanceVolume = 0f;
            }
            else if (distance <= attenuatedAudioSource.minDistance)
            {
                distanceVolume = 1f;
            }
            else
            {
                // 線形減衰
                float normalizedDistance = (distance - attenuatedAudioSource.minDistance) / (attenuatedAudioSource.maxDistance - attenuatedAudioSource.minDistance);
                distanceVolume = 1f - normalizedDistance;
            }
        }

        // フェード制御と距離減衰の両方を適用
        attenuatedAudioSource.maxVolume = currentVolume;
        audioSource.volume = distanceVolume * currentVolume;
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

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        Player = null;
        spriteRenderer = null;
        sprites = null;
        audioSource = null;
        attenuatedAudioSource = null;
    }
}
