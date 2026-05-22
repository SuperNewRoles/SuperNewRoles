using System;
using SuperNewRoles.CustomObject;
using UnityEngine;
using UnityEngine.Rendering;

namespace SuperNewRoles.Modules;

public sealed class RocketLauncherKillAnimation : ICustomKillAnimation
{
    private const string ProjectileSpriteName = "RocketLauncherProjectile.png";
    private const string ExplosionPrefabName = "RocketLauncherExplosion";
    private const string ShootSoundName = "RocketLauncherShoot.wav";

    private const float LaunchStartTime = 0.42f;
    private const float ExplosionStartTime = 1.45f;
    private const float ExplosionFadeStartTime = 2.12f;
    private const float EndTime = 2.75f;
    private const float ProjectileStartScale = 0.85f * 1.1f;
    private const float ProjectileEndScale = 0.62f * 1.1f;
    private const float ExplosionScale = 1.35f * 1.3f;

    private Transform _root;
    private PoolablePlayer _victimPlayer;
    private GameObject _projectileObject;
    private SpriteRenderer _projectileRenderer;
    private GameObject _explosionObject;
    private SpriteRenderer[] _explosionRenderers;
    private AudioSource _shootSource;
    private float _timer;
    private bool _launched;
    private bool _exploded;
    private bool _finished;

    public void Initialize(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        _root = __instance.transform;
        _timer = 0f;
        _launched = false;
        _exploded = false;
        _finished = false;

        CreateVictimPlayer(__instance);
        CreateProjectile();
    }

    public bool FixedUpdate()
    {
        if (_finished)
            return false;
        if (Time.deltaTime == 0f)
            return true;

        _timer += Time.fixedDeltaTime;

        if (!_launched && _timer >= LaunchStartTime)
            LaunchProjectile();

        if (!_exploded && _timer >= ExplosionStartTime)
            ExplodeProjectile();

        if (!_exploded)
            UpdateBeforeExplosion();
        else
            UpdateAfterExplosion();

        if (_timer < EndTime)
            return true;

        Cleanup();
        _finished = true;
        return false;
    }

    private void CreateVictimPlayer(OverlayKillAnimation overlay)
    {
        var playerPrefab = FastDestroyableSingleton<HudManager>.Instance.IntroPrefab.PlayerPrefab;
        _victimPlayer = GameObject.Instantiate(playerPrefab, overlay.transform);
        _victimPlayer.gameObject.name = "RocketLauncherKillVictim";
        _victimPlayer.gameObject.SetActive(true);
        _victimPlayer.UpdateFromEitherPlayerDataOrCache(PlayerControl.LocalPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
        _victimPlayer.transform.localPosition = new Vector3(-1.7f, -0.05f, 0f);
        _victimPlayer.transform.localScale = Vector3.one * 0.82f;

        if (_victimPlayer.cosmetics != null)
        {
            _victimPlayer.cosmetics.showColorBlindText = false;
            _victimPlayer.cosmetics.isNameVisible = false;
            _victimPlayer.cosmetics.UpdateNameVisibility();
        }

        foreach (var renderer in _victimPlayer.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (renderer == null)
                continue;
            renderer.gameObject.layer = 5;
            renderer.sortingOrder = 10;
        }
    }

    private void CreateProjectile()
    {
        var projectileSprite = AssetManager.GetAsset<Sprite>(ProjectileSpriteName);
        if (projectileSprite == null)
            throw new Exception($"Failed to load Asset: {ProjectileSpriteName}");

        var shootClip = AssetManager.GetAsset<AudioClip>(ShootSoundName);
        if (shootClip == null)
            throw new Exception($"Failed to load Asset: {ShootSoundName}");

        _projectileObject = new GameObject("RocketLauncherKillProjectile") { layer = 5 };
        _projectileObject.transform.SetParent(_root, worldPositionStays: false);
        _projectileObject.transform.localPosition = new Vector3(-1.7f, -0.05f, 0f);
        _projectileObject.transform.localScale = Vector3.one * ProjectileStartScale;
        _projectileObject.transform.localRotation = Quaternion.Euler(0f, 0f, RocketLauncherProjectile.GetSpriteAngle(Vector2.right));
        _projectileObject.SetActive(false);

        _projectileRenderer = _projectileObject.AddComponent<SpriteRenderer>();
        _projectileRenderer.sprite = projectileSprite;
        _projectileRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        _projectileRenderer.maskInteraction = SpriteMaskInteraction.None;
        _projectileRenderer.sortingOrder = 30;
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(_projectileRenderer, false);
        PlayerMaterial.SetColors(PlayerControl.LocalPlayer.Data.DefaultOutfit.ColorId, _projectileRenderer);
    }

    private void LaunchProjectile()
    {
        _launched = true;
        if (_victimPlayer != null)
            _victimPlayer.gameObject.SetActive(false);
        if (_projectileObject != null)
            _projectileObject.SetActive(true);

        _shootSource = SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>(ShootSoundName), loop: false, 0.75f);
    }

    private void UpdateBeforeExplosion()
    {
        if (!_launched)
        {
            if (_victimPlayer != null)
            {
                float anticipationT = Mathf.InverseLerp(0f, LaunchStartTime, _timer);
                float shake = Mathf.Sin(_timer * 78f) * Mathf.Lerp(0.01f, 0.045f, anticipationT);
                float lift = Mathf.Sin(anticipationT * Mathf.PI) * 0.06f;
                float pullback = Mathf.Lerp(0f, -0.16f, EaseInCubic(anticipationT));
                _victimPlayer.transform.localPosition = new Vector3(-1.7f + pullback + shake, -0.05f + lift, 0f);
                _victimPlayer.transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(_timer * 34f) * Mathf.Lerp(0f, 3.5f, anticipationT));
                _victimPlayer.transform.localScale = Vector3.one * Mathf.Lerp(0.82f, 0.88f, anticipationT);
            }
            return;
        }

        if (_projectileObject == null)
            return;

        float t = Mathf.InverseLerp(LaunchStartTime, ExplosionStartTime, _timer);
        float eased = EaseInCubic(t);
        Vector3 position = new(
            Mathf.Lerp(-1.7f, 2.0f, eased),
            -0.05f + (Mathf.Sin(t * Mathf.PI) * 0.65f),
            0f);
        _projectileObject.transform.localPosition = position;
        _projectileObject.transform.localScale = Vector3.one * Mathf.Lerp(ProjectileStartScale, ProjectileEndScale, t);
        _projectileObject.transform.localRotation = Quaternion.Euler(
            0f,
            0f,
            RocketLauncherProjectile.GetSpriteAngle(Vector2.right) + (Mathf.Sin(_timer * 18f) * Mathf.Lerp(6f, 2f, t)));
    }

    private void ExplodeProjectile()
    {
        _exploded = true;
        if (_projectileObject != null)
            _projectileObject.SetActive(false);

        var prefab = AssetManager.GetAsset<GameObject>(ExplosionPrefabName);
        if (prefab == null)
            throw new Exception($"Failed to load Asset: {ExplosionPrefabName}");

        _explosionObject = GameObject.Instantiate(prefab, _root);
        _explosionObject.name = "RocketLauncherKillExplosion";
        _explosionObject.transform.localPosition = new Vector3(2.0f, -0.05f, 0f);
        _explosionObject.transform.localScale = Vector3.one * ExplosionScale;
        _explosionObject.transform.localRotation = Quaternion.identity;

        var animator = _explosionObject.GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            GameObject.Destroy(_explosionObject);
            throw new Exception($"{ExplosionPrefabName} must contain Animator with RuntimeAnimatorController.");
        }
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        _explosionRenderers = _explosionObject.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var renderer in _explosionRenderers)
        {
            if (renderer == null)
                continue;
            renderer.gameObject.layer = 5;
            renderer.sortingOrder = 40;
        }
    }

    private void UpdateAfterExplosion()
    {
        if (_shootSource != null && _timer >= ExplosionStartTime)
        {
            float t = Mathf.InverseLerp(ExplosionStartTime + 0.25f, EndTime, _timer);
            _shootSource.volume = Mathf.Lerp(0.75f, 0f, Mathf.Clamp01(t));
            if (_shootSource.volume <= 0.001f)
            {
                _shootSource.Stop();
                _shootSource = null;
            }
        }

        if (_explosionRenderers == null)
            return;

        float alpha = 1f - Mathf.InverseLerp(ExplosionFadeStartTime, EndTime, _timer);
        alpha = Mathf.Clamp01(alpha);
        foreach (var renderer in _explosionRenderers)
        {
            if (renderer == null)
                continue;
            var color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    private void Cleanup()
    {
        if (_shootSource != null)
        {
            _shootSource.Stop();
            _shootSource = null;
        }
        if (_victimPlayer != null)
            GameObject.Destroy(_victimPlayer.gameObject);
        if (_projectileObject != null)
            GameObject.Destroy(_projectileObject);
        if (_explosionObject != null)
            GameObject.Destroy(_explosionObject);

        _root = null;
        _victimPlayer = null;
        _projectileObject = null;
        _projectileRenderer = null;
        _explosionObject = null;
        _explosionRenderers = null;
    }

    private static float EaseInCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * t;
    }
}
