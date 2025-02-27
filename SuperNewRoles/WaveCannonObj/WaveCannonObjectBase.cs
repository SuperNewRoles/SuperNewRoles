using System;
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
    public abstract Collider2D[] HitColliders { get; }
    public abstract float ShootTime { get; }
    public abstract GameObject WaveCannonObject { get; }
    public abstract bool HidePlayer { get; }
    public virtual Vector3 startPositionOffset => Vector3.zero;
    private bool isShooting = false;
    private Vector3 startPosition { get; }
    private float timer;
    private bool detached = false;

    public WaveCannonObjectBase(WaveCannonAbility ability, bool isFlipX, Vector3 startPosition)
    {
        this.ability = ability;
        new LateTask(() => this.fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate), 0f);
        isShooting = false;
        this.startPosition = startPosition;
        ability.Player.NetTransform.RpcSnapTo(startPosition);
        OnFixedUpdate();
        timer = ShootTime;
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
            ability.Player.moveable = false;
        }
        ability.Player.transform.position = startPosition;
        if (isShooting)
        {
            OnAnimationUpdateShooting();
            if (!ability.Player.AmOwner) return;
            Logger.Info("Shooting");
            foreach (var collider in HitColliders)
            {
                Logger.Info("Collider");

                foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
                {
                    Logger.Info("Player:" + player.PlayerId);
                    if (player.IsDead()) continue;
                    if (player.PlayerId == ability.Player.PlayerId) continue;
                    if (!collider.IsTouching(player.Player.Collider)) continue;
                    ExPlayerControl.LocalPlayer.RpcCustomDeath(player, CustomDeathType.WaveCannon);
                }
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
        OnAnimationShoot();
    }
    public virtual void Detach()
    {
        Logger.Info("Detached");
        new LateTask(() => FixedUpdateEvent.Instance.RemoveListener(fixedUpdateEvent), 0f);
        detached = true;
        if (WaveCannonObject != null)
            GameObject.Destroy(WaveCannonObject);
        if (HidePlayer)
        {
            ability.Player.cosmetics.currentBodySprite.BodySprite.gameObject.SetActive(true);
            ability.Player.cosmetics.gameObject.SetActive(true);
        }
        ability.Player.moveable = true;
        ability.Player.MyPhysics.Animations.PlayIdleAnimation();
    }
    [CustomRPC]
    public static void RpcSpawnFromType(ExPlayerControl source, WaveCannonType type, ulong abilityId, bool isFlipX, Vector3 startPosition)
    {
        WaveCannonAbility ability = source.GetAbility<WaveCannonAbility>(abilityId);
        if (ability == null) return;
        WaveCannonObjectBase obj = SpawnFromType(type, ability, isFlipX, startPosition);
        ability.SpawnedWaveCannonObject(obj);
    }
    public static WaveCannonObjectBase SpawnFromType(WaveCannonType type, WaveCannonAbility ability, bool isFlipX, Vector3 startPosition)
    {
        switch (type)
        {
            case WaveCannonType.Tank:
                return new WaveCannonObjectTank(ability, isFlipX, startPosition);
            default:
                throw new Exception($"Invalid wave cannon type: {type}");
        }
    }
}