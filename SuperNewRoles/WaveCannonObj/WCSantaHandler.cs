using System;
using System.Collections.Generic;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Mode;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.WaveCannonObj;

public class WCSantaHandler : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public PolygonCollider2D KillCollider { get; private set; }
    public static readonly float SantaSpeed = 6.5f;
    public static bool IsFlipX;
    public float moveX;
    public static bool reflection = false;
    public static float Angle;
    public static Vector3 WiseManVector;
    public static float Xdiff;

    private WaveCannonAbility _ability;
    private ExPlayerControl _source;
    private bool _friendlyFire;
    private readonly HashSet<byte> _alreadyKilled = new();

    /// <summary>
    /// サンタが撃ち終わり後も死亡判定を継続するために、発射者情報を保持する。
    /// </summary>
    public void Init(WaveCannonAbility ability)
    {
        _ability = ability;
        _source = ability?.Player;
        _friendlyFire = ability?.friendlyFire ?? true;
    }

    public void Start()
    {
        Renderer = gameObject.AddComponent<SpriteRenderer>();
        Renderer.sprite = AssetManager.GetAsset<Sprite>("WaveCannonSanta.png");
        // サンタ本体に死亡判定用のコライダーを付与
        KillCollider = gameObject.AddComponent<PolygonCollider2D>();
        KillCollider.isTrigger = true;
    }

    public void Update()
    {
        int flip = transform.parent == null && IsFlipX ? -1 : 1;
        if (transform.localScale.y < 0.725f)
            transform.localScale += new Vector3(flip * -0.05f, 0.05f, 0.05f);

        transform.localPosition += new Vector3(flip * SantaSpeed * Time.deltaTime, 0, 0);

        // 撃ち終わり(Detach)後もサンタ自体が死亡判定を持ち続ける
        TryKillTouchingPlayers();

        if (Vector2.Distance(transform.position, PlayerControl.LocalPlayer.transform.position) > 30)
            Destroy(gameObject);
    }

    private void TryKillTouchingPlayers()
    {
        if (KillCollider == null) return;
        if (_ability == null || _source == null) return;
        if (!_source.AmOwner) return; // キル判定は発射者視点のみ
        // 波動砲本体が生存している（=発射中/演出中）間は、WaveCannonObjectBase側で判定されるため二重処理を避ける
        if (_ability.WaveCannonObject != null) return;
        if (MeetingHud.Instance != null) return;

        foreach (var target in ExPlayerControl.ExPlayerControls)
        {
            if (target == null) continue;
            if (!target.IsAlive()) continue;
            if (target.PlayerId == _source.PlayerId) continue;
            if (_alreadyKilled.Contains(target.PlayerId)) continue;

            if (!KillCollider.IsTouching(target.Player.Collider)) continue;
            if (!IsValidTargetByFriendlyFire(target)) continue;

            // WaveCannon系のTryKillEvent(賢者等)はRpcCustomDeath内部で処理される
            _source.RpcCustomDeath(target, CustomDeathType.WaveCannonSanta);
            _alreadyKilled.Add(target.PlayerId);
        }
    }

    private bool IsValidTargetByFriendlyFire(ExPlayerControl target)
    {
        if (_friendlyFire) return true;

        if (ModeManager.IsMode(ModeId.WCBattleRoyal))
        {
            if (WCBattleRoyalMode.Instance.IsOnSameTeam(_source.Player, target.Player))
                return false;
        }
        else
        {
            if (_source.IsImpostor() && target.IsImpostor())
                return false;
            if (_source.IsCrewmate() && target.IsCrewmate())
                return false;
            if (_source.IsJackal() && target.IsJackal())
                return false;
        }
        return true;
    }
}

public class SantaKillAnimation : ICustomKillAnimation
{
    private GameObject _santaObject;
    private SpriteRenderer _santaRenderer;
    private PoolablePlayer _victimPlayer;
    private SpriteRenderer[] _victimRenderers;
    private float _timer = 0f;
    private bool _animationFinished = false;
    private Vector3 _initialVictimPosition;
    private Vector3 _initialSantaPosition;

    public void Initialize(OverlayKillAnimation __instance, KillOverlayInitData initData)
    {
        // Santaオブジェクトを作成（左から登場）
        _santaObject = new GameObject("SantaKillSanta");
        _santaObject.transform.parent = __instance.transform;
        _santaRenderer = _santaObject.AddComponent<SpriteRenderer>();
        _santaRenderer.sprite = AssetManager.GetAsset<Sprite>("WaveCannonSanta.png");
        _santaRenderer.gameObject.layer = 5;
        _santaRenderer.flipX = true;
        _santaRenderer.transform.localScale = Vector3.one * 0.6f;
        _santaRenderer.color = Color.white;

        // 初期位置：画面の左側外
        _initialSantaPosition = new Vector3(-8f, 0f, 0f);
        _santaObject.transform.localPosition = _initialSantaPosition;

        // 被害者オブジェクトはIntroPrefab.PlayerPrefabを使う（他のUI表現と同じ）
        var playerUIObjectPrefab = FastDestroyableSingleton<HudManager>.Instance.IntroPrefab.PlayerPrefab;
        _victimPlayer = GameObject.Instantiate(playerUIObjectPrefab, __instance.transform);
        _victimPlayer.gameObject.name = "SantaKillVictim";
        _victimPlayer.gameObject.SetActive(true);

        // このアニメは「被害者視点でのみ」出す設計なので、victim=LocalPlayerとして初期化
        // （initDataの構造に依存しないようにする）
        _victimPlayer.UpdateFromEitherPlayerDataOrCache(PlayerControl.LocalPlayer.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
        if (_victimPlayer.cosmetics?.colorBlindText != null)
            _victimPlayer.cosmetics.colorBlindText.text = "";
        if (_victimPlayer.cosmetics != null)
        {
            _victimPlayer.cosmetics.showColorBlindText = false;
            _victimPlayer.cosmetics.isNameVisible = false;
            _victimPlayer.cosmetics.UpdateNameVisibility();
        }

        // フェード用にスプライトレンダラー群をキャッシュ
        _victimRenderers = _victimPlayer.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < _victimRenderers.Length; i++)
        {
            // サンタより奥に
            _victimRenderers[i].sortingOrder = 5;
        }

        _victimPlayer.transform.localScale = Vector3.one * 0.8f; // Santaより小さめに

        // 初期位置：中央
        _initialVictimPosition = new Vector3(-0.3f, 0.13f, 0);
        _victimPlayer.transform.localPosition = _initialVictimPosition;
    }

    public bool FixedUpdate()
    {
        if (Time.deltaTime == 0) return true;
        if (_animationFinished) return false;

        _timer += Time.fixedDeltaTime;

        // Phase 1-2: Santaが左から登場して右へ移動、victimが飛んでいく (0-3.5秒)
        if (_timer <= 3.5f)
        {
            // Santaの移動（左から右へスムーズに）
            float totalProgress = _timer / 3.5f;
            float santaProgress = Mathf.Pow(totalProgress, 0.7f); // ease-inで加速
            float santaX = Mathf.Lerp(_initialSantaPosition.x, 8f, santaProgress);

            _santaObject.transform.localPosition = new Vector3(santaX, 0, 0f);

            // victimのアニメーション（Santaが中央に到達してからスタート）
            float santaReachCenterProgress = 0.3f; // Santaが中央(x=0)に到達するタイミング
            if (totalProgress >= santaReachCenterProgress)
            {
                float contactProgress = (totalProgress - santaReachCenterProgress) / (1f - santaReachCenterProgress); // 残り時間で飛翔
                float victimProgress = Mathf.Min(contactProgress, 1f);
                float easedVictimProgress = Mathf.Pow(victimProgress, 0.8f); // ease-in

                // 回転しながら上昇し続ける軌道（放物線ではなく直線的に上昇）
                float rotationAngle = easedVictimProgress * 1080f; // 3回転
                float height = easedVictimProgress * 6f; // 直線的に上昇し続ける
                float horizontalMove = easedVictimProgress * 12f; // 右方向に遠くまで飛ぶ

                _victimPlayer.transform.localPosition = new Vector3(
                    _initialVictimPosition.x + horizontalMove,
                    _initialVictimPosition.y + height,
                    0f
                );
                _victimPlayer.transform.localEulerAngles = new Vector3(0f, 0f, rotationAngle);

                // スケールも少し変化させてダイナミックに + 遠ざかる効果
                float scaleMultiplier = 1f + Mathf.Sin(victimProgress * Mathf.PI * 3f) * 0.3f;
                float distanceScale = Mathf.Max(0.3f, 1f - (victimProgress * 0.7f)); // 遠ざかるにつれて小さくなる
                _victimPlayer.transform.localScale = Vector3.one * (0.8f * scaleMultiplier * distanceScale);

                // 途中からフェードアウト開始（着地せずに消える）
                if (victimProgress > 0.7f)
                {
                    float fadeProgress = (victimProgress - 0.7f) / 0.3f;
                    float alpha = 1f - fadeProgress;
                    for (int i = 0; i < _victimRenderers.Length; i++)
                    {
                        if (_victimRenderers[i] == null) continue;
                        var color = _victimRenderers[i].color;
                        color.a = alpha;
                        _victimRenderers[i].color = color;
                    }
                }
            }
        }
        // Phase 3: Santaのみフェードアウト (3.5秒以降)
        else if (_timer <= 4.0f)
        {
            float fadeProgress = (_timer - 3.5f) / 0.5f;

            // Santaのみフェードアウト（victimは既にフェードアウト済み）
            if (_santaRenderer != null)
            {
                Color santaColor = _santaRenderer.color;
                santaColor.a = 1f - fadeProgress;
                _santaRenderer.color = santaColor;
            }
        }
        else
        {
            // アニメーション終了
            _animationFinished = true;

            // オブジェクトを破棄
            if (_santaObject != null)
                GameObject.Destroy(_santaObject);
            if (_victimPlayer != null)
                GameObject.Destroy(_victimPlayer.gameObject);

            return false;
        }

        return true;
    }
}
