using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Ability;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record RocketLauncherData(
    float LaunchCooldown,
    bool CollideWithPlayers,
    bool KillImpostors,
    float ExplosionRange,
    bool CanUseNormalKill
);

public class RocketLauncherAbility : AbilityBase
{
    public RocketLauncherData Data { get; }
    public RocketLauncherButtonAbility ButtonAbility { get; private set; }

    public RocketLauncherAbility(RocketLauncherData data)
    {
        Data = data;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        ButtonAbility = new RocketLauncherButtonAbility(Data);
        Player.AttachAbility(ButtonAbility, new AbilityParentAbility(this));
        Player.AttachAbility(new KillableAbility(() => Data.CanUseNormalKill && (ButtonAbility == null || !ButtonAbility.IsBusy)), new AbilityParentAbility(this));
    }
}

public class RocketLauncherButtonAbility : TargetCustomButtonBase
{
    private const string LaunchButtonSprite = "RocketLauncherLaunchButton.png";
    private const string ShotButtonSprite = "RocketLauncherShotButton.png";
    private const string LoadSound = "RocketLauncherLaunch.wav";
    private const string ShootSound = "RocketLauncherShot.wav";
    private const string ExplosionPrefab = "RocketLauncherExplosion";
    private const string ExplosionSound = "RocketLauncherExplosion.wav";
    private const float ExplosionVisualScaleMultiplier = 0.8f;
    private const float ExplosionSoundMaxDistanceMultiplier = 3f;
    private const float ExplosionSoundMinMaxDistance = 3f;
    private const float ExplosionSoundMinDistance = 1.25f;

    private readonly RocketLauncherData _data;
    private ExPlayerControl _heldTarget;
    private RocketLauncherProjectile _activeProjectile;
    private RocketLauncherHeldPlayer _heldVisual;
    private EventListener<MeetingStartEventData> _meetingStartListener;
    private EventListener _fixedUpdateListener;
    private EventListener<DieEventData> _dieListener;
    private EventListener<DisconnectEventData> _disconnectListener;
    private EventListener<MurderEventData> _murderListener;

    private bool HasHeldTarget => _heldTarget != null && _heldTarget.IsAlive();
    private bool HasActiveProjectile => _activeProjectile != null && _activeProjectile.IsActive;
    public bool IsBusy => HasHeldTarget || HasActiveProjectile;

    public override float DefaultTimer => HasHeldTarget ? 0f : _data.LaunchCooldown;
    public override string buttonText => ModTranslation.GetString(HasHeldTarget ? "RocketLauncherShotButtonText" : "RocketLauncherLaunchButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>(HasHeldTarget ? ShotButtonSprite : LaunchButtonSprite);
    protected override KeyType keytype => KeyType.Ability1;
    public override Color32 OutlineColor => RocketLauncher.Instance.RoleColor;
    public override bool OnlyCrewmates => true;
    public override Func<ExPlayerControl, bool> IsTargetable => player => !IsBusy && player != null && player.IsAlive();

    public RocketLauncherButtonAbility(RocketLauncherData data)
    {
        _data = data;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _dieListener = DieEvent.Instance.AddListener(OnDie);
        _disconnectListener = DisconnectEvent.Instance.AddListener(OnDisconnect);
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _murderListener?.RemoveListener();
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _meetingStartListener?.RemoveListener();
        _fixedUpdateListener?.RemoveListener();
        _dieListener?.RemoveListener();
        _disconnectListener?.RemoveListener();
        ClearHeldTargetLocally(restoreMoveable: true);
        DetachProjectileLocally(restoreTarget: true);
    }

    public override bool CheckHasButton()
    {
        return base.CheckHasButton();
    }

    public override bool CheckIsAvailable()
    {
        if (Player == null || !Player.IsAlive())
            return false;
        if (MeetingHud.Instance != null)
            return false;
        if (PlayerControl.LocalPlayer == null || !PlayerControl.LocalPlayer.CanMove)
            return false;
        if (HasActiveProjectile)
            return false;
        if (HasHeldTarget)
            return true;
        return Target != null && Target.IsAlive();
    }

    public override void OnClick()
    {
        if (HasHeldTarget)
        {
            ShootHeldTarget();
            return;
        }

        if (Target == null || !Target.IsAlive())
            return;
        PlayOwnerSound(LoadSound);
        RpcGrabTarget(Target);
    }

    public override void ResetTimer()
    {
        base.ResetTimer();
        if (HasHeldTarget)
            Timer = 0f;
    }

    private void ShootHeldTarget()
    {
        var target = _heldTarget;
        if (target == null || !target.IsAlive())
            return;

        Vector2 startPosition = Player.GetTruePosition();
        Vector2 direction = Player.Player.MyPhysics.FlipX ? Vector2.left : Vector2.right;
        PlayOwnerSound(ShootSound);
        RpcStartShot(target, startPosition, direction);
        if (_data.CanUseNormalKill)
            Player.ResetKillCooldown();
    }

    private void OnFixedUpdate()
    {
        if (_activeProjectile != null && !_activeProjectile.IsActive)
            _activeProjectile = null;

        if (Player == null || !Player.IsAlive())
        {
            ClearHeldTargetLocally(restoreMoveable: true);
            DetachProjectileLocally(restoreTarget: true);
            return;
        }

        if (_heldTarget == null)
            return;

        if (!_heldTarget.IsAlive())
        {
            ClearHeldTargetLocally(restoreMoveable: true);
            return;
        }

        MoveTargetTo(_heldTarget, Player.GetTruePosition());
        SetTargetMoveable(_heldTarget, false);
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (Player == null || !Player.AmOwner)
            return;

        if (HasHeldTarget)
        {
            var target = _heldTarget;
            RpcKillHeldByMeeting(target, target.GetTruePosition());
            return;
        }

        if (HasActiveProjectile)
        {
            var projectile = _activeProjectile;
            RpcKillFlyingTargetOnly(projectile.Target, projectile.Position);
        }
    }

    private void OnDie(DieEventData data)
    {
        if (data.player == null)
            return;

        if (data.player.PlayerId == Player?.PlayerId)
        {
            ClearHeldTargetLocally(restoreMoveable: true);
            DetachProjectileLocally(restoreTarget: true);
            return;
        }

        if (_heldTarget != null && data.player.PlayerId == _heldTarget.PlayerId)
        {
            ClearHeldTargetLocally(restoreMoveable: true);
            return;
        }

        if (_activeProjectile != null && _activeProjectile.Target != null && data.player.PlayerId == _activeProjectile.Target.PlayerId)
        {
            DetachProjectileLocally(restoreTarget: true);
            return;
        }

    }

    private void OnDisconnect(DisconnectEventData data)
    {
        if (data.disconnectedPlayer == null)
            return;

        byte disconnectedPlayerId = data.disconnectedPlayer.PlayerId;
        if (disconnectedPlayerId == Player?.PlayerId)
        {
            ClearHeldTargetLocally(restoreMoveable: true);
            DetachProjectileLocally(restoreTarget: true);
            return;
        }

        if (_heldTarget != null && disconnectedPlayerId == _heldTarget.PlayerId)
        {
            ClearHeldTargetLocally(restoreMoveable: true);
            return;
        }

        if (_activeProjectile != null && _activeProjectile.Target != null && disconnectedPlayerId == _activeProjectile.Target.PlayerId)
            DetachProjectileLocally(restoreTarget: true);
    }

    private void OnMurder(MurderEventData data)
    {
        if (!_data.CanUseNormalKill)
            return;
        if (data.killer != Player)
            return;
        if (IsBusy)
            return;
        ResetTimer();
    }

    [CustomRPC]
    public void RpcGrabTarget(ExPlayerControl target)
    {
        if (target == null || !target.IsAlive())
            return;

        if (_heldTarget != null && _heldTarget != target)
            RestoreTargetControl(_heldTarget);

        DestroyHeldVisual();
        _heldTarget = target;
        MoveTargetTo(target, Player.GetTruePosition());
        SetTargetMoveable(target, false);
        SetTargetVisible(target, false);
        _heldVisual = RocketLauncherHeldPlayer.Spawn(Player, target);
        if (Player.AmOwner)
            Timer = 0f;
    }

    [CustomRPC]
    public void RpcClearHeldTarget(bool restoreMoveable)
    {
        ClearHeldTargetLocally(restoreMoveable);
    }

    [CustomRPC]
    public void RpcStartShot(ExPlayerControl target, Vector2 startPosition, Vector2 direction)
    {
        if (target == null || !target.IsAlive())
            return;

        ClearHeldTargetLocally(restoreMoveable: false);
        DetachProjectileLocally(restoreTarget: true);
        MoveTargetTo(target, startPosition);
        SetTargetMoveable(target, false);
        _activeProjectile = RocketLauncherProjectile.Spawn(this, Player, target, startPosition, direction, _data.CollideWithPlayers);
    }

    [CustomRPC]
    public void RpcReflectProjectile(ExPlayerControl wiseMan, Vector2 position, Vector2 direction)
    {
        if (_activeProjectile == null || !_activeProjectile.IsActive)
            return;

        _activeProjectile.Reflect(wiseMan, position, direction);
        PlayWiseManGuardEffect(wiseMan);
        if (wiseMan != null && wiseMan.TryGetAbility<WiseManAbility>(out var wiseManAbility))
            wiseManAbility.Guarded = true;
    }

    [CustomRPC]
    public void RpcExplodeProjectile(ExPlayerControl launchedTarget, Vector2 position, List<ExPlayerControl> victims)
    {
        if (launchedTarget != null)
            MoveTargetTo(launchedTarget, position);
        DetachProjectileLocally(restoreTarget: true);
        SpawnExplosionAnimation(position);
        PlayExplosionSound(position);

        HashSet<byte> killedPlayerIds = new();
        foreach (var victim in victims ?? [])
        {
            if (victim == null || !victim.IsAlive())
                continue;
            if (!killedPlayerIds.Add(victim.PlayerId))
                continue;

            if (launchedTarget != null && victim.PlayerId == launchedTarget.PlayerId)
                MoveTargetTo(victim, position);
            victim.CustomDeath(CustomDeathType.RocketLauncher, source: Player);
        }

        if (killedPlayerIds.Count > 0 && AmongUsClient.Instance.AmHost && ExPlayerControl.ExPlayerControls.Count(x => x.IsAlive()) == 0)
            EndGamer.RpcEndGameImpostorWin();
    }

    [CustomRPC]
    public void RpcKillHeldByMeeting(ExPlayerControl target, Vector2 position)
    {
        ClearHeldTargetLocally(restoreMoveable: true);
        KillSingleTarget(target, position);
    }

    [CustomRPC]
    public void RpcKillFlyingTargetOnly(ExPlayerControl target, Vector2 position)
    {
        DetachProjectileLocally(restoreTarget: true);
        KillSingleTarget(target, position);
    }

    internal void RequestExplodeFromProjectile(RocketLauncherProjectile projectile, Vector2 position)
    {
        if (!Player.AmOwner || projectile == null || projectile != _activeProjectile)
            return;

        var victims = CollectExplosionVictims(projectile.Target, position);
        RpcExplodeProjectile(projectile.Target, position, victims);
    }

    internal void RequestReflectFromProjectile(RocketLauncherProjectile projectile, ExPlayerControl wiseMan, Vector2 position, Vector2 direction)
    {
        if (!Player.AmOwner || projectile == null || projectile != _activeProjectile)
            return;
        RpcReflectProjectile(wiseMan, position, direction);
    }

    private List<ExPlayerControl> CollectExplosionVictims(ExPlayerControl launchedTarget, Vector2 position)
    {
        List<ExPlayerControl> victims = new();
        if (launchedTarget != null && launchedTarget.IsAlive())
            victims.Add(launchedTarget);

        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            if (player == null || !player.IsAlive())
                continue;
            if (launchedTarget != null && player.PlayerId == launchedTarget.PlayerId)
                continue;
            if (player.Player.inVent)
                continue;
            if (!_data.KillImpostors && player.IsImpostor())
                continue;
            if (Vector2.Distance(position, player.GetTruePosition()) > _data.ExplosionRange)
                continue;

            victims.Add(player);
        }

        return victims;
    }

    private void KillSingleTarget(ExPlayerControl target, Vector2 position)
    {
        if (target == null)
            return;

        MoveTargetTo(target, position);
        RestoreTargetControl(target);
        if (target.IsAlive())
            target.CustomDeath(CustomDeathType.RocketLauncher, source: Player, suppressKillAnimation: true);
    }

    private void ClearHeldTargetLocally(bool restoreMoveable)
    {
        DestroyHeldVisual();
        if (_heldTarget != null && restoreMoveable)
            RestoreTargetControl(_heldTarget);
        _heldTarget = null;
    }

    private void DestroyHeldVisual()
    {
        if (_heldVisual == null)
            return;
        GameObject.Destroy(_heldVisual.gameObject);
        _heldVisual = null;
    }

    private void DetachProjectileLocally(bool restoreTarget)
    {
        if (_activeProjectile == null)
            return;
        _activeProjectile.Detach(restoreTarget);
        _activeProjectile = null;
    }

    private void SpawnExplosionAnimation(Vector2 position)
    {
        var prefab = AssetManager.GetAsset<GameObject>(ExplosionPrefab);
        if (prefab == null)
            throw new Exception($"Failed to load Asset: {ExplosionPrefab}");

        GameObject explosionObject = GameObject.Instantiate(prefab);
        explosionObject.transform.position = new Vector3(position.x, position.y, -4.5f);
        explosionObject.transform.localScale = Vector3.one * Mathf.Max(0.5f, _data.ExplosionRange * 2f) * ExplosionVisualScaleMultiplier;

        var spriteRenderer = explosionObject.GetComponent<SpriteRenderer>();
        var animator = explosionObject.GetComponent<Animator>();
        if (spriteRenderer == null || animator == null || animator.runtimeAnimatorController == null)
        {
            GameObject.Destroy(explosionObject);
            throw new Exception($"{ExplosionPrefab} must contain SpriteRenderer and Animator with RuntimeAnimatorController.");
        }

        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        if (explosionObject.GetComponent<DestroyOnAnimationEndObject>() == null)
            explosionObject.AddComponent<DestroyOnAnimationEndObject>();
    }

    private void PlayExplosionSound(Vector2 position)
    {
        var clip = AssetManager.GetAsset<AudioClip>(ExplosionSound);
        if (clip == null)
            throw new Exception($"Failed to load Asset: {ExplosionSound}");

        var audioObject = new GameObject("RocketLauncherExplosionSound");
        audioObject.transform.position = new Vector3(position.x, position.y, -4.5f);
        float maxDistance = Mathf.Max(ExplosionSoundMinMaxDistance, _data.ExplosionRange * ExplosionSoundMaxDistanceMultiplier);
        var attenuatedAudio = AttenuatedAudioSourceUtility.SetupSimple(audioObject, clip, loop: false, maxDistance: maxDistance, minDistance: ExplosionSoundMinDistance);
        var audioSource = attenuatedAudio.GetComponent<AudioSource>();
        audioSource.Play();
        attenuatedAudio.ManualUpdate();
        GameObject.Destroy(audioObject, clip.length + 0.1f);
    }

    private void PlayOwnerSound(string assetName)
    {
        if (!Player.AmOwner)
            return;
        SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>(assetName), loop: false, 0.8f);
    }

    private static void PlayWiseManGuardEffect(ExPlayerControl wiseMan)
    {
        if (wiseMan?.Player == null)
            return;

        RoleEffectAnimation roleEffectAnimation = GameObject.Instantiate<RoleEffectAnimation>(
            DestroyableSingleton<RoleManager>.Instance.protectAnim,
            wiseMan.Player.gameObject.transform);
        roleEffectAnimation.SetMaskLayerBasedOnWhoShouldSee(shouldBeVisible: true);
        roleEffectAnimation.Play(wiseMan, null, wiseMan.cosmetics.FlipX, RoleEffectAnimation.SoundType.Global);
    }

    internal static void MoveTargetTo(ExPlayerControl target, Vector2 position)
    {
        if (target?.Player == null)
            return;

        target.transform.position = new Vector3(position.x, position.y, target.transform.position.z);
        target.NetTransform?.SnapTo(position);
        if (target.MyPhysics?.body != null)
            target.MyPhysics.body.velocity = Vector2.zero;
    }

    internal static void SetTargetMoveable(ExPlayerControl target, bool moveable)
    {
        if (target?.Player == null || !target.AmOwner)
            return;
        target.Player.moveable = moveable;
    }

    internal static void SetTargetVisible(ExPlayerControl target, bool visible)
    {
        if (target?.cosmetics == null)
            return;

        target.cosmetics.gameObject.SetActive(visible);
        var bodySprite = target.cosmetics.currentBodySprite?.BodySprite;
        if (bodySprite != null)
            bodySprite.gameObject.SetActive(visible);
    }

    internal static void RestoreTargetControl(ExPlayerControl target)
    {
        if (target == null)
            return;
        SetTargetVisible(target, true);
        if (target.IsAlive())
            SetTargetMoveable(target, true);
    }
}
