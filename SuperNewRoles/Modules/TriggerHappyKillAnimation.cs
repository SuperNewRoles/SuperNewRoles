using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperNewRoles.Modules;

/// <summary>
/// TriggerHappy(HappyGatling)専用のキルカットイン。
/// 被害者視点でのみ再生される想定（CustomDeath側で設定）。
/// </summary>
public sealed class TriggerHappyKillAnimation : ICustomKillAnimation
{
    private readonly byte _killerId;

    private Transform _root;

    private GameObject _weaponObject;
    private SpriteRenderer _weaponRenderer;
    private Sprite[] _weaponSprites;

    private PoolablePlayer _killerPlayer;
    private PoolablePlayer _victimPlayer;
    private SpriteRenderer[] _victimRenderers;

    private AudioClip _shotClip;
    private AudioSource _shotSource;
    private bool _shotStarted;
    private float _shotBaseVolume = 0.55f;

    private Sprite _bulletSprite;
    private readonly List<BulletTracer> _tracers = new();
    private float _nextTracerTime;
    private const float TracerInterval = 0.05f;
    private const float TracerSpeed = 18f;
    private const float TracerLife = 0.4f;
    private const int TracerBurstCount = 4; // 1回の発射で複数本出して“撃ってる感”を出す
    private const int MaxActiveTracers = 28; // 画面が埋まらないように上限

    private float _timer;
    private bool _finished;

    public TriggerHappyKillAnimation(ExPlayerControl killer)
    {
        _killerId = killer?.PlayerId ?? byte.MaxValue;
    }

    public void Initialize(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        _timer = 0f;
        _finished = false;
        _shotStarted = false;
        _shotSource = null;

        _root = __instance.transform;

        _shotClip = AssetManager.GetAsset<AudioClip>("TriggerHappyShotSound.wav");
        _bulletSprite = AssetManager.GetAsset<Sprite>("TriggerHappy_Bullet.png");
        _tracers.Clear();
        _nextTracerTime = 0.40f; // 連射が始まって少ししてから弾を見せる

        _weaponSprites = new[]
        {
            AssetManager.GetAsset<Sprite>("TriggerHappy_Machinegun_1.png"),
            AssetManager.GetAsset<Sprite>("TriggerHappy_Machinegun_2.png")
        }.Where(x => x != null).ToArray();

        var playerUIObjectPrefab = FastDestroyableSingleton<HudManager>.Instance.IntroPrefab.PlayerPrefab;

        // Killer
        _killerPlayer = GameObject.Instantiate(playerUIObjectPrefab, __instance.transform);
        _killerPlayer.gameObject.name = "TriggerHappyKillKiller";
        _killerPlayer.gameObject.SetActive(true);
        // 近すぎないように左右の距離を広げる
        _killerPlayer.transform.localPosition = new Vector3(-2.35f, 0.05f, 0f);
        _killerPlayer.transform.localScale = Vector3.one * 0.85f;

        var killerPc = FindPlayerControl(_killerId);
        if (killerPc != null)
        {
            _killerPlayer.UpdateFromEitherPlayerDataOrCache(killerPc.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
        }
        else
        {
            // 取得できない場合はローカル情報でフォールバック（見た目が多少ズレても演出は成立する）
            _killerPlayer.UpdateFromEitherPlayerDataOrCache(PlayerControl.LocalPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
        }
        if (_killerPlayer.cosmetics != null)
        {
            _killerPlayer.cosmetics.showColorBlindText = false;
            _killerPlayer.cosmetics.isNameVisible = false;
            _killerPlayer.cosmetics.UpdateNameVisibility();
        }

        // Victim (this animation is designed to be victim POV only)
        _victimPlayer = GameObject.Instantiate(playerUIObjectPrefab, __instance.transform);
        _victimPlayer.gameObject.name = "TriggerHappyKillVictim";
        _victimPlayer.gameObject.SetActive(true);
        _victimPlayer.UpdateFromEitherPlayerDataOrCache(PlayerControl.LocalPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
        if (_victimPlayer.cosmetics != null)
        {
            _victimPlayer.cosmetics.showColorBlindText = false;
            _victimPlayer.cosmetics.isNameVisible = false;
            _victimPlayer.cosmetics.UpdateNameVisibility();
        }
        _victimPlayer.transform.localScale = Vector3.one * 0.85f;
        // 近すぎないように右側へ
        _victimPlayer.transform.localPosition = new Vector3(1.35f, 0.0f, 0f);

        _victimRenderers = _victimPlayer.GetComponentsInChildren<SpriteRenderer>(true);

        // Weapon (gatling) attached to killer
        _weaponObject = new GameObject("TriggerHappyKillWeapon");
        _weaponObject.transform.SetParent(_killerPlayer.transform, worldPositionStays: false);
        _weaponObject.transform.localPosition = new Vector3(0.65f, 0.05f, -0.1f);
        _weaponObject.transform.localRotation = Quaternion.identity;
        _weaponObject.transform.localScale = Vector3.one * 0.85f;
        _weaponRenderer = _weaponObject.AddComponent<SpriteRenderer>();
        _weaponRenderer.gameObject.layer = 5;
        _weaponRenderer.sprite = _weaponSprites.Length > 0 ? _weaponSprites[0] : null;
        _weaponRenderer.flipY = false;
        _weaponRenderer.sortingOrder = 20;
    }

    public bool FixedUpdate()
    {
        if (_finished) return false;

        _timer += Time.fixedDeltaTime;

        UpdateTracers();

        // 0.0 - 0.35: small anticipation (少し溜めを増やす)
        if (_timer < 0.35f)
        {
            return true;
        }

        // 0.35 - 1.65: firing + recoil（連射時間を延長）
        if (_timer < 1.65f)
        {
            AnimateWeapon();
            float t = Mathf.InverseLerp(0.35f, 1.65f, _timer);
            float shake = Mathf.Sin(_timer * 55f) * 0.03f;
            if (_weaponObject != null)
                _weaponObject.transform.localPosition = new Vector3(0.65f + shake, 0.05f + (shake * 0.4f), -0.1f);
            if (_victimPlayer != null)
                _victimPlayer.transform.localPosition = new Vector3(1.35f + (t * 0.22f), 0.0f + (Mathf.Sin(_timer * 30f) * 0.02f), 0f);

            // 効果音：TriggerHappyShotSound.wavは長いので「一回だけ」鳴らして後でフェードアウトする
            TryStartShot();

            // 弾（トレーサー）を連射中に見せる
            SpawnTracersIfNeeded();

            return true;
        }

        // 1.65 - 3.05: victim falls down（倒れる時間も延長）
        if (_timer < 3.05f)
        {
            UpdateShotFadeOut();
            float t = Mathf.InverseLerp(1.65f, 3.05f, _timer);
            if (_victimPlayer != null)
            {
                float rot = Mathf.Lerp(0f, -95f, EaseOutCubic(t));
                _victimPlayer.transform.localEulerAngles = new Vector3(0f, 0f, rot);
                _victimPlayer.transform.localPosition = new Vector3(
                    Mathf.Lerp(1.57f, 2.05f, t),
                    Mathf.Lerp(0.0f, -1.35f, EaseInCubic(t)),
                    0f);
            }
            return true;
        }

        // 3.05 - 3.45: fade out（余韻を残す）
        if (_timer < 3.45f)
        {
            UpdateShotFadeOut(force: true);
            float t = Mathf.InverseLerp(3.05f, 3.45f, _timer);
            SetVictimAlpha(1f - t);
            return true;
        }

        Cleanup();
        _finished = true;
        return false;
    }

    private void AnimateWeapon()
    {
        if (_weaponRenderer == null || _weaponSprites == null || _weaponSprites.Length == 0) return;
        int frame = (int)(_timer * 24f) % _weaponSprites.Length;
        _weaponRenderer.sprite = _weaponSprites[frame];
    }

    private void SetVictimAlpha(float alpha)
    {
        if (_victimRenderers == null) return;
        alpha = Mathf.Clamp01(alpha);
        for (int i = 0; i < _victimRenderers.Length; i++)
        {
            var r = _victimRenderers[i];
            if (r == null) continue;
            var c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    private void Cleanup()
    {
        for (int i = 0; i < _tracers.Count; i++)
        {
            var t = _tracers[i];
            if (t?.GameObject != null)
                UnityEngine.Object.Destroy(t.GameObject);
        }
        _tracers.Clear();

        if (_weaponObject != null) UnityEngine.Object.Destroy(_weaponObject);
        if (_killerPlayer != null) UnityEngine.Object.Destroy(_killerPlayer.gameObject);
        if (_victimPlayer != null) UnityEngine.Object.Destroy(_victimPlayer.gameObject);
        if (_shotSource != null)
        {
            _shotSource.Stop();
            _shotSource = null;
        }
        _weaponObject = null;
        _killerPlayer = null;
        _victimPlayer = null;
        _victimRenderers = null;
        _weaponRenderer = null;
        _weaponSprites = null;
        _shotClip = null;
        _bulletSprite = null;
        _root = null;
    }

    private void TryStartShot()
    {
        if (_shotStarted) return;
        _shotStarted = true;
        if (_shotClip == null) return;

        // 既存のStingerとは別に、銃声を足す（音量は控えめ）
        // wavが長いので、後でフェードアウトして止める前提
        _shotSource = SoundManager.Instance.PlaySound(_shotClip, loop: false, _shotBaseVolume);
    }

    private void UpdateShotFadeOut(bool force = false)
    {
        if (_shotSource == null) return;

        // 連射フェーズ(〜1.65)が終わったらフェードアウト開始
        // force=true の場合は必ずフェードアウトさせる（最終フェードの余韻）
        if (!force && _timer < 1.65f) return;

        const float fadeDuration = 0.8f;
        float t = force
            ? 1f
            : Mathf.InverseLerp(1.65f, 1.65f + fadeDuration, _timer);

        float vol = Mathf.Lerp(_shotBaseVolume, 0f, Mathf.Clamp01(t));
        _shotSource.volume = vol;

        if (t >= 1f || vol <= 0.001f)
        {
            _shotSource.Stop();
            _shotSource = null;
        }
    }

    private sealed class BulletTracer
    {
        public GameObject GameObject;
        public SpriteRenderer Renderer;
        public Vector3 Velocity;
        public float Age;
    }

    private void SpawnTracersIfNeeded()
    {
        if (_bulletSprite == null) return;
        if (_weaponObject == null || _victimPlayer == null) return;
        if (_root == null) return;

        while (_timer >= _nextTracerTime && _timer < 1.65f)
        {
            _nextTracerTime += TracerInterval;
            // 高さ(上下)を変えながら複数出す（同じ高さだけだと単調）
            for (int i = 0; i < TracerBurstCount; i++)
            {
                float yJitter = UnityEngine.Random.Range(-0.20f, 0.20f);
                float xJitter = UnityEngine.Random.Range(-0.05f, 0.05f);
                SpawnTracerOnce(new Vector3(xJitter, yJitter, 0f));
            }
        }
    }

    private void SpawnTracerOnce(Vector3 endJitter)
    {
        // weapon tip -> victim torso
        Vector3 startWorld = _weaponObject.transform.position + (_weaponObject.transform.right * 0.45f);
        Vector3 endWorld = _victimPlayer.transform.position + new Vector3(-0.1f, 0.15f, 0f) + endJitter;
        Vector3 dir = (endWorld - startWorld);
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        var go = new GameObject("TriggerHappyKillTracer") { layer = 5 };
        // OverlayKillAnimation配下はUI用レイヤー(5)で描画されるため合わせる
        go.transform.SetParent(_root, worldPositionStays: false);
        // ローカル座標で生成（UIカメラの描画座標系に合わせる）
        Vector3 startLocal = _root.InverseTransformPoint(startWorld);
        go.transform.localPosition = startLocal;
        // 大きすぎるので縮小（弾丸というよりトレーサー/弾線として見せる）
        go.transform.localScale = Vector3.one * 0.35f;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = _bulletSprite;
        sr.sortingOrder = 25;
        sr.color = new Color(1f, 1f, 1f, 0.9f);
        sr.gameObject.layer = 5;

        _tracers.Add(new BulletTracer
        {
            GameObject = go,
            Renderer = sr,
            Velocity = dir * TracerSpeed,
            Age = 0f
        });

        // 多すぎる時は古いものから消す
        while (_tracers.Count > MaxActiveTracers)
        {
            var oldest = _tracers[0];
            if (oldest?.GameObject != null)
                UnityEngine.Object.Destroy(oldest.GameObject);
            _tracers.RemoveAt(0);
        }
    }

    private void UpdateTracers()
    {
        if (_tracers.Count == 0) return;

        for (int i = _tracers.Count - 1; i >= 0; i--)
        {
            var t = _tracers[i];
            if (t == null || t.GameObject == null)
            {
                _tracers.RemoveAt(i);
                continue;
            }

            t.Age += Time.fixedDeltaTime;
            t.GameObject.transform.position += t.Velocity * Time.fixedDeltaTime;

            if (t.Renderer != null)
            {
                float a = 1f - Mathf.Clamp01(t.Age / TracerLife);
                var c = t.Renderer.color;
                c.a = a;
                t.Renderer.color = c;
            }

            if (t.Age >= TracerLife)
            {
                UnityEngine.Object.Destroy(t.GameObject);
                _tracers.RemoveAt(i);
            }
        }
    }

    private static PlayerControl FindPlayerControl(byte playerId)
    {
        if (playerId == byte.MaxValue) return null;
        foreach (var pc in PlayerControl.AllPlayerControls)
        {
            if (pc != null && pc.PlayerId == playerId)
                return pc;
        }
        return null;
    }

    private static float EaseOutCubic(float t)
        => 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);

    private static float EaseInCubic(float t)
        => Mathf.Pow(Mathf.Clamp01(t), 3f);
}


