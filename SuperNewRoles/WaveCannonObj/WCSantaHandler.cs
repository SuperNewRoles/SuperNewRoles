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
    public BoxCollider2D KillCollider { get; private set; }
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
        KillCollider = gameObject.AddComponent<BoxCollider2D>();
        KillCollider.isTrigger = true;
        if (Renderer.sprite != null)
        {
            KillCollider.size = Renderer.sprite.bounds.size;
            KillCollider.offset = Renderer.sprite.bounds.center;
        }
        else
        {
            // 万が一スプライトが無い場合でも判定が消えないように最低限のサイズを設定
            KillCollider.size = new Vector2(0.6f, 0.6f);
            KillCollider.offset = Vector2.zero;
        }
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
            _source.RpcCustomDeath(target, CustomDeathType.WaveCannon);
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
