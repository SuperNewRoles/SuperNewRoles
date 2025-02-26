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
    private bool isShooting = false;
    private Vector3 startPosition { get; }
    private float timer;
    private bool detached = false;
    public WaveCannonObjectBase(WaveCannonAbility ability)
    {
        this.ability = ability;
        new LateTask(() => this.fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate), 0f);
        isShooting = false;
        startPosition = ability.Player.transform.position;
        OnFixedUpdate();
        Logger.Info("StartPosition:" + startPosition.x + "," + startPosition.y + "," + startPosition.z);
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
        if (ability.Player.AmOwner)
        {
            ability.Player.transform.position = startPosition;
            ability.Player.MyPhysics.body.velocity = Vector2.zero;
            ability.Player.moveable = false;
        }
        if (isShooting)
        {
            OnAnimationUpdateShooting();
            if (!ability.Player.AmOwner) return;
            foreach (var collider in HitColliders)
            {
                foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
                {
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
    public void Detach()
    {
        Logger.Info("Detached");
        new LateTask(() => FixedUpdateEvent.Instance.RemoveListener(fixedUpdateEvent), 0f);
        detached = true;
        if (WaveCannonObject != null)
            GameObject.Destroy(WaveCannonObject);
        ability.Player.moveable = true;
    }
    [CustomRPC]
    public static void RpcSpawnFromType(ExPlayerControl source, WaveCannonType type, ulong abilityId)
    {
        WaveCannonAbility ability = source.GetAbility<WaveCannonAbility>(abilityId);
        if (ability == null) return;
        WaveCannonObjectBase obj = SpawnFromType(type, ability);
        ability.SpawnedWaveCannonObject(obj);
    }
    public static WaveCannonObjectBase SpawnFromType(WaveCannonType type, WaveCannonAbility ability)
    {
        switch (type)
        {
            case WaveCannonType.Cannon:
                return new WaveCannonObjectCannon(ability);
            default:
                throw new Exception($"Invalid wave cannon type: {type}");
        }
    }
}