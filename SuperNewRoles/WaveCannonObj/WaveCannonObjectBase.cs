using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SuperNewRoles.WaveCannonObj;

public abstract class WaveCannonObjectBase
{
    protected EventListener fixedUpdateEvent;
    protected WaveCannonAbility ability;
    public abstract void OnAnimationUpdateCharging();
    public abstract void OnAnimationUpdateShooting();
    public abstract void OnAnimationShoot();
    public abstract void OnAnimationWiseMan(float distanceX, Vector3 position, float angle);
    public abstract Collider2D[] HitColliders { get; }
    public abstract SpriteRenderer ShootRenderer { get; }
    public abstract float ShootTime { get; }
    public abstract GameObject WaveCannonObject { get; }
    public abstract bool HidePlayer { get; }
    public virtual Vector3 startPositionOffset => Vector3.zero;
    public virtual bool EnabledWiseMan => true;
    private bool isShooting = false;
    private Vector3 startPosition { get; }
    private float timer;
    private bool detached = false;
    private bool isResetKillCooldown;
    private ExPlayerControl _touchedWiseman;
    private bool isShootingStarted = false;
    private float shootDelayTimer = 0f;
    private const float SHOOT_DELAY_TIME = 0.5f;
    public bool checkedWiseman = false;
    public bool willCheckWiseman = false;

    public WaveCannonObjectBase(WaveCannonAbility ability, bool isFlipX, Vector3 startPosition, bool isResetKillCooldown)
    {
        this.ability = ability;
        new LateTask(() => this.fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate), 0f);
        isShooting = false;
        this.startPosition = startPosition;
        ability.Player.NetTransform.RpcSnapTo(startPosition);
        OnFixedUpdate();
        timer = ShootTime;
        this.isResetKillCooldown = isResetKillCooldown;
    }
    private void OnFixedUpdate()
    {
        if (detached) return;
        if (((ExPlayerControl)ability.Player).IsDead() || MeetingHud.Instance != null)
        {
            Detach();
            return;
        }
        if (HidePlayer)
        {
            ability.Player.cosmetics.currentBodySprite.BodySprite.gameObject.SetActive(false);
            ability.Player.cosmetics.gameObject.SetActive(false);
        }
        if (ability.Player.AmOwner)
        {
            ability.Player.transform.position = startPosition;
            ability.Player.MyPhysics.body.velocity = Vector2.zero;
            ability.Player.Player.moveable = false;
        }
        ability.Player.transform.position = startPosition;
        ability.Player.NetTransform.SnapTo(startPosition);
        if (isShooting)
        {
            // 賢者のRpcを待機する(ちらつきを防ぐため)
            if (isShootingStarted)
            {
                OnAnimationUpdateShooting();
            }
            else if (!ability.Player.AmOwner)
            {
                shootDelayTimer += Time.deltaTime;
                if (shootDelayTimer >= SHOOT_DELAY_TIME)
                {
                    isShootingStarted = true;
                    OnAnimationShoot();
                }
                else
                {
                    OnAnimationUpdateCharging();
                    return;
                }
            }
            else if (ability.Player.AmOwner)
            {
                isShootingStarted = true;
                OnAnimationShoot();
            }

            if (!ability.Player.AmOwner) return;
            List<ExPlayerControl> targetPlayersSorted = ExPlayerControl.ExPlayerControls.Where(x => x.IsAlive() && x.PlayerId != ability.Player.PlayerId).OrderBy(x => Mathf.Abs(ability.Player.transform.position.x - x.transform.position.x)).ToList();
            foreach (var collider in HitColliders)
            {
                foreach (ExPlayerControl player in targetPlayersSorted)
                {
                    if (collider == null) continue;
                    if (player.IsDead()) continue;
                    if (player.PlayerId == ability.Player.PlayerId) continue;
                    if (!collider.IsTouching(player.Player.Collider)) continue;
                    if (player == _touchedWiseman) continue;
                    if (EnabledWiseMan && !checkedWiseman && _touchedWiseman == null && player.TryGetAbility<WiseManAbility>(out var wiseManAbility) && wiseManAbility.Active && !wiseManAbility.Guarded)
                    {
                        _touchedWiseman = player;
                        RpcWaveCannonWiseMan(ability, player, GetRandomAngle());
                        continue;
                    }
                    ExPlayerControl.LocalPlayer.RpcCustomDeath(player, CustomDeathType.WaveCannon);
                }
            }
            if (!willCheckWiseman)
            {
                willCheckWiseman = true;
            }
            else if (!checkedWiseman && _touchedWiseman == null)
            {
                RpcWaveCannonNoWiseMan(ability);
            }
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                Detach();
            }
        }
        else
        {
            OnAnimationUpdateCharging();
        }
    }
    public void OnShoot()
    {
        isShooting = true;
    }
    [CustomRPC(true)]
    public static void RpcDetach(ExPlayerControl source, ulong abilityId)
    {
        WaveCannonAbility ability = source.GetAbility<WaveCannonAbility>(abilityId);
        if (ability == null) return;
        WaveCannonObjectBase obj = ability.WaveCannonObject;
        if (obj == null) return;
        obj.Detach();
    }
    private static float GetRandomAngle()
    {
        var angles = new List<float> { 135, 90, 270, 225 };
        return angles[UnityEngine.Random.Range(0, angles.Count)];
    }
    [CustomRPC]
    public static void RpcWaveCannonWiseMan(WaveCannonAbility ability, ExPlayerControl wiseMan, float angle)
    {
        WaveCannonObjectBase obj = ability.WaveCannonObject;
        if (obj == null) return;
        obj._touchedWiseman = wiseMan;
        if (!obj.isShootingStarted)
            obj.OnAnimationShoot();
        obj.isShootingStarted = true;
        obj.checkedWiseman = true;
        obj.OnAnimationWiseMan(Mathf.Abs(ability.Player.transform.position.x - wiseMan.transform.position.x) - 0.8f, wiseMan.transform.position, angle);

        RoleEffectAnimation roleEffectAnimation = GameObject.Instantiate<RoleEffectAnimation>(DestroyableSingleton<RoleManager>.Instance.protectAnim, wiseMan.Player.gameObject.transform);
        roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(shouldBeVisible: true);
        roleEffectAnimation.Play(wiseMan, null, wiseMan.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
        if (ability.Player.AmOwner || wiseMan.AmOwner)
            obj.OnAnimationShoot();
        if (wiseMan.TryGetAbility<WiseManAbility>(out var wiseManAbility))
        {
            wiseManAbility.Guarded = true;
        }
    }
    [CustomRPC]
    public static void RpcWaveCannonNoWiseMan(WaveCannonAbility ability)
    {
        WaveCannonObjectBase obj = ability.WaveCannonObject;
        if (obj == null) return;
        if (!obj.isShootingStarted)
            obj.OnAnimationShoot();
        obj.isShootingStarted = true;
        obj.checkedWiseman = true;
    }
    public virtual void Detach()
    {
        if (ability?.Player?.AmOwner == true)
            RpcDetach(ability.Player, ability.AbilityId);
        new LateTask(() => FixedUpdateEvent.Instance.RemoveListener(fixedUpdateEvent), 0f);
        detached = true;
        if (WaveCannonObject != null)
            GameObject.Destroy(WaveCannonObject);
        if (HidePlayer)
        {
            ability.Player.cosmetics.currentBodySprite.BodySprite.gameObject.SetActive(true);
            ability.Player.cosmetics.gameObject.SetActive(true);
        }
        ability.Player.Player.moveable = true;
        ability.Player.MyPhysics.Animations.PlayIdleAnimation();
        if (ability.Player.AmOwner && isResetKillCooldown)
            ExPlayerControl.LocalPlayer.ResetKillCooldown();
    }
    [CustomRPC]
    public static void RpcSpawnFromType(ExPlayerControl source, WaveCannonType type, ulong abilityId, bool isFlipX, Vector3 startPosition)
    {
        WaveCannonAbility ability = source.GetAbility<WaveCannonAbility>(abilityId);
        if (ability == null) return;
        WaveCannonObjectBase obj = SpawnFromType(type, ability, isFlipX, startPosition, ability.isResetKillCooldown);
        ability.SpawnedWaveCannonObject(obj);
    }
    public static WaveCannonObjectBase SpawnFromType(WaveCannonType type, WaveCannonAbility ability, bool isFlipX, Vector3 startPosition, bool isResetKillCooldown = false)
    {
        switch (type)
        {
            case WaveCannonType.Tank:
                return new WaveCannonObjectTank(ability, isFlipX, startPosition, isResetKillCooldown);
            case WaveCannonType.Bullet:
                return new WaveCannonObjectBullet(ability, isFlipX, startPosition, isResetKillCooldown);
            default:
                throw new Exception($"Invalid wave cannon type: {type}");
        }
    }
}