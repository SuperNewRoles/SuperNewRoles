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
    bool LimitLoadedTime,
    float LoadedTimeLimit,
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

public class RocketLauncherButtonAbility : TargetCustomButtonBase, IButtonEffect
{
    private const string LaunchButtonSprite = "RocketLauncherLaunchButton.png";
    private const string ShotButtonSprite = "RocketLauncherShotButton.png";
    private const string LoadSound = "RocketLauncherLaunch.wav";
    private const string ShootSound = "RocketLauncherShot.wav";
    private const string ExplosionPrefab = "RocketLauncherExplosion";
    private const string ExplosionSound = "RocketLauncherExplosion.wav";
    private const float ExplosionRangeInternalMultiplier = 2f;
    private const float ExplosionVisualScaleMultiplier = 0.8f;
    private const float ExplosionSoundMaxDistanceMultiplier = 3f;
    private const float ExplosionSoundMinMaxDistance = 3f;
    private const float ExplosionSoundMinDistance = 1.25f;
    private static readonly Dictionary<byte, TargetVisibilityState> HiddenTargetVisibilityStates = new();

    private readonly RocketLauncherData _data;
    private ExPlayerControl _heldTarget;
    private RocketLauncherProjectile _activeProjectile;
    private RocketLauncherHeldPlayer _heldVisual;
    private bool _isHoldingGuardedWiseMan;
    private Vector2 _guardedWiseManHoldPosition;
    private EventListener<MeetingStartEventData> _meetingStartListener;
    private EventListener _fixedUpdateListener;
    private EventListener _hudUpdateListener;
    private EventListener<DieEventData> _dieListener;
    private EventListener<DisconnectEventData> _disconnectListener;
    private EventListener<MurderEventData> _murderListener;

    private bool HasHeldTarget => _heldTarget != null && _heldTarget.IsAlive();
    private bool HasActiveProjectile => _activeProjectile != null && _activeProjectile.IsActive;
    private bool ShouldLimitLoadedTime => _data.LimitLoadedTime && _data.LoadedTimeLimit > 0f;
    public bool IsBusy => HasHeldTarget || HasActiveProjectile;

    public override float DefaultTimer => HasHeldTarget ? 0f : _data.LaunchCooldown;
    public override string buttonText => ModTranslation.GetString(HasHeldTarget ? "RocketLauncherShotButtonText" : "RocketLauncherLaunchButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>(HasHeldTarget ? ShotButtonSprite : LaunchButtonSprite);
    protected override KeyType keytype => KeyType.Ability1;
    public override Color32 OutlineColor => RocketLauncher.Instance.RoleColor;
    public override bool OnlyCrewmates => true;
    public override Func<ExPlayerControl, bool> IsTargetable => player => !IsBusy && player != null && player.IsAlive();
    public bool isEffectActive { get; set; }
    public Action OnEffectEnds => ExpireHeldTargetByEffect;
    public float EffectDuration => ShouldLimitLoadedTime ? _data.LoadedTimeLimit : 0f;
    public bool IsEffectDurationInfinity => !ShouldLimitLoadedTime;
    public bool effectCancellable => true;
    public bool doAdditionalEffect => false;
    public float EffectTimer { get; set; }

    public RocketLauncherButtonAbility(RocketLauncherData data)
    {
        _data = data;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _hudUpdateListener = HudUpdateEvent.Instance.AddListener(OnHudUpdate);
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
        _hudUpdateListener?.RemoveListener();
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

        if (_isHoldingGuardedWiseMan)
        {
            if (!IsWiseManGuardActive(target))
            {
                ReleaseGuardedWiseManHold(resetCooldown: true);
                return;
            }

            Vector2 explosionPosition = _guardedWiseManHoldPosition;
            PlayOwnerSound(ShootSound);
            var victims = CollectExplosionVictims(null, explosionPosition, target);
            RpcExplodeGuardedWiseManHold(target, explosionPosition, victims);
            if (_data.CanUseNormalKill)
                Player.ResetKillCooldown();
            return;
        }

        Vector2 startPosition = GetShotStartPosition(GetHeldTargetFollowPosition());
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

        UpdateHeldTargetFollow();
    }

    private void OnHudUpdate()
    {
        if (Player == null || !Player.IsAlive() || MeetingHud.Instance != null)
            return;

        UpdateHeldTargetFollow();
    }

    private void UpdateHeldTargetFollow()
    {
        if (_heldTarget == null)
            return;

        if (!_heldTarget.IsAlive())
        {
            ClearHeldTargetLocally(restoreMoveable: true);
            return;
        }

        if (_isHoldingGuardedWiseMan)
        {
            if (IsWiseManGuardActive(_heldTarget))
            {
                MoveTargetTo(_heldTarget, _guardedWiseManHoldPosition);
                SetTargetMoveable(_heldTarget, false);
                SetTargetVisible(_heldTarget, true);
                return;
            }

            ReleaseGuardedWiseManHold(resetCooldown: true);
            return;
        }

        MoveTargetTo(_heldTarget, GetHeldTargetFollowPosition());
        SetTargetMoveable(_heldTarget, false);
        SetTargetVisible(_heldTarget, false);
    }

    public bool IsEffectAvailable()
    {
        if (!HasHeldTarget)
            return false;
        if (Player == null || !Player.IsAlive())
            return false;
        if (MeetingHud.Instance != null)
            return false;
        if (PlayerControl.LocalPlayer == null || !PlayerControl.LocalPlayer.CanMove)
            return false;
        return !HasActiveProjectile;
    }

    public void OnCancel(ActionButton actionButton)
    {
        if (!isEffectActive)
            return;

        isEffectActive = false;
        if (actionButton != null)
            actionButton.cooldownTimerText.color = Palette.EnabledColor;
        ShootHeldTarget();
    }

    private void ExpireHeldTargetByEffect()
    {
        if (!ShouldLimitLoadedTime || !HasHeldTarget)
            return;
        if (Player.AmOwner)
        {
            ShootHeldTarget();
            ResetTimer();
        }
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (Player == null || !Player.AmOwner)
            return;

        if (HasHeldTarget)
        {
            var target = _heldTarget;
            RpcKillHeldByMeeting(target, GetPlayerTransformPosition(target));
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
            ClearDeadHeldTargetLocally(data.player);
            return;
        }

        if (_activeProjectile != null && _activeProjectile.Target != null && data.player.PlayerId == _activeProjectile.Target.PlayerId)
        {
            DetachDeadProjectileTargetLocally(data.player);
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
        if (IsWiseManGuardActive(target))
        {
            _isHoldingGuardedWiseMan = true;
            _guardedWiseManHoldPosition = GetPlayerTransformPosition(target);
            MoveTargetTo(target, _guardedWiseManHoldPosition);
            SetTargetMoveable(target, false);
            SetTargetVisible(target, true);
        }
        else
        {
            _isHoldingGuardedWiseMan = false;
            MoveTargetTo(target, GetHeldTargetFollowPosition());
            SetTargetMoveable(target, false);
            SetTargetVisible(target, false);
            _heldVisual = RocketLauncherHeldPlayer.Spawn(Player, target);
        }
        if (Player.AmOwner)
            Timer = 0f;
    }

    [CustomRPC]
    public void RpcClearHeldTarget(bool restoreMoveable)
    {
        ClearHeldTargetLocally(restoreMoveable);
    }

    [CustomRPC]
    public void RpcExpireHeldTarget(ExPlayerControl target, Vector2 returnPosition)
    {
        if (_heldTarget == null || target == null || _heldTarget.PlayerId != target.PlayerId)
            return;

        MoveTargetTo(target, returnPosition);
        ForceTargetTransformTo(target, returnPosition);
        ClearHeldTargetLocally(restoreMoveable: true);
        if (Player.AmOwner)
            ResetTimer();
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
        if (Player.AmOwner)
        {
            Vector2 sourcePosition = GetPlayerTransformPosition(Player);
            if (RocketLauncherProjectile.HasLaunchPathHit(sourcePosition, startPosition))
                RequestExplodeFromProjectile(_activeProjectile, sourcePosition);
        }
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
        bool restoreLaunchedTargetOnDetach = !ContainsPlayer(victims, launchedTarget);
        DetachProjectileLocally(restoreLaunchedTargetOnDetach);
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
            {
                MoveTargetTo(victim, position);
                ForceTargetTransformTo(victim, position);
            }
            victim.CustomDeath(CustomDeathType.RocketLauncher, source: Player);
            if (launchedTarget != null && victim.PlayerId == launchedTarget.PlayerId)
            {
                if (victim.IsAlive())
                    RestoreTargetControl(victim);
                else
                    ReleaseTargetVisibilityAfterDeath(victim);
                PinDeadBodyToPosition(victim.PlayerId, position);
            }
        }

        if (killedPlayerIds.Count > 0 && AmongUsClient.Instance.AmHost && ExPlayerControl.ExPlayerControls.Count(x => x.IsAlive()) == 0)
            EndGamer.RpcEndGameImpostorWin();
    }

    [CustomRPC]
    public void RpcExplodeGuardedWiseManHold(ExPlayerControl wiseMan, Vector2 position, List<ExPlayerControl> victims)
    {
        if (wiseMan == null || !wiseMan.IsAlive())
            return;

        ClearHeldTargetLocally(restoreMoveable: true);
        MoveTargetTo(wiseMan, position);
        ForceTargetTransformTo(wiseMan, position);
        RestoreTargetControl(wiseMan);
        PlayWiseManGuardEffect(wiseMan);
        if (wiseMan.TryGetAbility<WiseManAbility>(out var wiseManAbility))
            wiseManAbility.Guarded = true;

        SpawnExplosionAnimation(position);
        PlayExplosionSound(position);

        HashSet<byte> killedPlayerIds = new();
        foreach (var victim in victims ?? [])
        {
            if (victim == null || !victim.IsAlive())
                continue;
            if (victim.PlayerId == wiseMan.PlayerId)
                continue;
            if (!killedPlayerIds.Add(victim.PlayerId))
                continue;

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

    private List<ExPlayerControl> CollectExplosionVictims(ExPlayerControl launchedTarget, Vector2 position, ExPlayerControl excludedPlayer = null)
    {
        List<ExPlayerControl> victims = new();
        if (launchedTarget != null && launchedTarget.IsAlive() && !IsSamePlayer(launchedTarget, excludedPlayer))
            victims.Add(launchedTarget);

        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            if (player == null || !player.IsAlive())
                continue;
            if (IsSamePlayer(player, excludedPlayer))
                continue;
            if (launchedTarget != null && player.PlayerId == launchedTarget.PlayerId)
                continue;
            if (player.Player.inVent)
                continue;
            if (!_data.KillImpostors && player.IsImpostor())
                continue;
            if (Vector2.Distance(position, player.GetTruePosition()) > GetEffectiveExplosionRange())
                continue;

            victims.Add(player);
        }

        return victims;
    }

    private static bool ContainsPlayer(List<ExPlayerControl> players, ExPlayerControl target)
    {
        return target != null && players != null && players.Any(player => player != null && player.PlayerId == target.PlayerId);
    }

    private static bool IsSamePlayer(ExPlayerControl player, ExPlayerControl target)
    {
        return player != null && target != null && player.PlayerId == target.PlayerId;
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
        _isHoldingGuardedWiseMan = false;
        _guardedWiseManHoldPosition = Vector2.zero;
        isEffectActive = false;
        EffectTimer = 0f;
    }

    private void ClearDeadHeldTargetLocally(ExPlayerControl target)
    {
        DestroyHeldVisual();
        ReleaseTargetVisibilityAfterDeath(target);
        _heldTarget = null;
        _isHoldingGuardedWiseMan = false;
        _guardedWiseManHoldPosition = Vector2.zero;
        isEffectActive = false;
        EffectTimer = 0f;
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

    private void DetachDeadProjectileTargetLocally(ExPlayerControl target)
    {
        DetachProjectileLocally(restoreTarget: false);
        ReleaseTargetVisibilityAfterDeath(target);
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

    private Vector2 GetHeldTargetFollowPosition()
    {
        return RocketLauncherHeldPlayer.GetHeldTargetPosition(Player);
    }

    private static Vector2 GetShotStartPosition(Vector2 visualStartPosition)
    {
        return visualStartPosition + new Vector2(0f, -0.4f);
    }

    private float GetEffectiveExplosionRange()
    {
        return _data.ExplosionRange * ExplosionRangeInternalMultiplier;
    }

    private static bool IsWiseManGuardActive(ExPlayerControl target)
    {
        return target != null &&
            target.TryGetAbility<WiseManAbility>(out var wiseManAbility) &&
            wiseManAbility.Active &&
            !wiseManAbility.Guarded;
    }

    private void ReleaseGuardedWiseManHold(bool resetCooldown)
    {
        var target = _heldTarget;
        Vector2 position = _guardedWiseManHoldPosition;
        ClearHeldTargetLocally(restoreMoveable: true);
        if (target != null)
        {
            MoveTargetTo(target, position);
            ForceTargetTransformTo(target, position);
            RestoreTargetControl(target);
        }
        if (resetCooldown && Player.AmOwner)
            ResetTimer();
    }

    private static Vector2 GetPlayerTransformPosition(ExPlayerControl player)
    {
        if (player?.Player == null)
            return Vector2.zero;

        return player.Player.transform.position;
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

    private static void ForceTargetTransformTo(ExPlayerControl target, Vector2 position)
    {
        if (target?.Player == null)
            return;

        target.Player.transform.position = new Vector3(position.x, position.y, target.Player.transform.position.z);
    }

    private static void PinDeadBodyToPosition(byte playerId, Vector2 position)
    {
        MoveDeadBodyToPosition(playerId, position);
        new LateTask(() => MoveDeadBodyToPosition(playerId, position), 0.05f, "RocketLauncherPinDeadBody", log: false);
    }

    private static void MoveDeadBodyToPosition(byte playerId, Vector2 position)
    {
        foreach (var deadBody in GameObject.FindObjectsOfType<DeadBody>())
        {
            if (deadBody == null || deadBody.ParentId != playerId)
                continue;

            deadBody.transform.position = new Vector3(position.x, position.y, position.y / 1000f);
        }
    }

    internal static void MoveTargetTo(ExPlayerControl target, Vector2 position)
    {
        if (target?.Player == null)
            return;

        target.Player.transform.position = new Vector3(position.x, position.y, target.Player.transform.position.z);
        var body = target.MyPhysics?.body;
        if (body != null)
        {
            body.position = position;
            body.velocity = Vector2.zero;
        }
    }

    internal static void SetTargetMoveable(ExPlayerControl target, bool moveable)
    {
        if (target?.Player == null || !target.AmOwner)
            return;
        target.Player.moveable = moveable;
    }

    internal static void SetTargetVisible(ExPlayerControl target, bool visible)
    {
        if (target == null)
            return;

        if (visible)
        {
            RestoreTargetRenderers(target);
            return;
        }

        HideTargetRenderers(target);
    }

    private static void HideTargetRenderers(ExPlayerControl target)
    {
        if (!HiddenTargetVisibilityStates.TryGetValue(target.PlayerId, out var state))
        {
            var renderers = CollectTargetRenderers(target);
            var gameObjects = CollectTargetVisibilityObjects(target, renderers);
            state = new TargetVisibilityState(target, renderers, gameObjects);
            HiddenTargetVisibilityStates[target.PlayerId] = state;
        }

        state.Hide();
    }

    private static void RestoreTargetRenderers(ExPlayerControl target)
    {
        if (!HiddenTargetVisibilityStates.TryGetValue(target.PlayerId, out var state))
            return;

        state.Restore();
        HiddenTargetVisibilityStates.Remove(target.PlayerId);
    }

    private static void ReleaseTargetVisibilityAfterDeath(ExPlayerControl target)
    {
        if (target == null || !HiddenTargetVisibilityStates.TryGetValue(target.PlayerId, out var state))
            return;

        state.ReleaseHidden();
        HiddenTargetVisibilityStates.Remove(target.PlayerId);
    }

    private static Renderer[] CollectTargetRenderers(ExPlayerControl target)
    {
        List<Renderer> renderers = new();
        if (target.Player != null)
            renderers.AddRange(target.Player.GetComponentsInChildren<Renderer>(true));
        if (target.cosmetics != null)
            renderers.AddRange(target.cosmetics.GetComponentsInChildren<Renderer>(true));

        var bodySprite = target.cosmetics?.currentBodySprite?.BodySprite;
        if (bodySprite != null)
            renderers.Add(bodySprite);

        return renderers
            .Where(renderer => renderer != null)
            .Distinct()
            .ToArray();
    }

    private static GameObject[] CollectTargetVisibilityObjects(ExPlayerControl target, Renderer[] renderers)
    {
        List<GameObject> gameObjects = new();
        if (target.cosmetics != null)
            gameObjects.Add(target.cosmetics.gameObject);

        var bodySpriteObject = target.cosmetics?.currentBodySprite?.BodySprite?.gameObject;
        if (bodySpriteObject != null)
            gameObjects.Add(bodySpriteObject);

        foreach (var renderer in renderers)
        {
            if (renderer?.gameObject != null)
                gameObjects.Add(renderer.gameObject);
        }

        return gameObjects
            .Where(gameObject => gameObject != null)
            .Distinct()
            .ToArray();
    }

    internal static void RestoreTargetControl(ExPlayerControl target)
    {
        if (target == null)
            return;
        SetTargetVisible(target, true);
        if (target.IsAlive())
            SetTargetMoveable(target, true);
    }

    private sealed class TargetVisibilityState
    {
        private readonly ExPlayerControl _target;
        private readonly List<Renderer> _renderers = new();
        private readonly List<bool> _enabledStates = new();
        private readonly List<GameObject> _gameObjects = new();
        private readonly List<bool> _activeStates = new();
        private readonly bool _hasPlayerVisibleState;
        private readonly bool _playerVisibleState;
        private readonly bool _hasCosmeticsVisibleState;
        private readonly bool _cosmeticsVisibleState;
        private readonly bool _hasBodyVisibleState;
        private readonly bool _bodyVisibleState;

        public TargetVisibilityState(ExPlayerControl target, Renderer[] renderers, GameObject[] gameObjects)
        {
            _target = target;
            if (target?.Player != null)
            {
                _hasPlayerVisibleState = true;
                _playerVisibleState = target.Player.Visible;
            }
            if (target?.cosmetics != null)
            {
                _hasCosmeticsVisibleState = true;
                _cosmeticsVisibleState = target.cosmetics.Visible;
            }
            if (target?.cosmetics?.currentBodySprite != null)
            {
                _hasBodyVisibleState = true;
                _bodyVisibleState = target.cosmetics.currentBodySprite.Visible;
            }

            AddRenderers(renderers);
            AddGameObjects(gameObjects);
        }

        private void AddRenderers(Renderer[] renderers)
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null || _renderers.Contains(renderer))
                    continue;

                _renderers.Add(renderer);
                _enabledStates.Add(renderer.enabled);
            }
        }

        private void AddGameObjects(GameObject[] gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject == null || _gameObjects.Contains(gameObject))
                    continue;

                _gameObjects.Add(gameObject);
                _activeStates.Add(gameObject.activeSelf);
            }
        }

        public void Hide()
        {
            ApplyHighLevelVisibility(visible: false);

            foreach (var renderer in _renderers)
            {
                if (renderer != null && renderer.enabled)
                    renderer.enabled = false;
            }

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject != null && gameObject.activeSelf)
                    gameObject.SetActive(false);
            }
        }

        public void Restore()
        {
            RestoreGameObjects();
            RestoreRenderers();
            RestoreHighLevelVisibility(force: true);
        }

        public void ReleaseHidden()
        {
            RestoreGameObjects();
            RestoreRenderers();
            ApplyHighLevelVisibility(visible: false, force: true);
        }

        private void ApplyHighLevelVisibility(bool visible, bool force = false)
        {
            if (_target?.Player != null && (force || _target.Player.Visible != visible))
                _target.Player.Visible = visible;
            if (_target?.cosmetics != null && (force || _target.cosmetics.Visible != visible))
                _target.cosmetics.Visible = visible;
            if (_target?.cosmetics?.currentBodySprite != null
                && (force || _target.cosmetics.currentBodySprite.Visible != visible))
                _target.cosmetics.currentBodySprite.Visible = visible;
        }

        private void RestoreHighLevelVisibility(bool force = false)
        {
            if (_hasPlayerVisibleState
                && _target?.Player != null
                && (force || _target.Player.Visible != _playerVisibleState))
                _target.Player.Visible = _playerVisibleState;
            if (_hasCosmeticsVisibleState
                && _target?.cosmetics != null
                && (force || _target.cosmetics.Visible != _cosmeticsVisibleState))
                _target.cosmetics.Visible = _cosmeticsVisibleState;
            if (_hasBodyVisibleState
                && _target?.cosmetics?.currentBodySprite != null
                && (force || _target.cosmetics.currentBodySprite.Visible != _bodyVisibleState))
                _target.cosmetics.currentBodySprite.Visible = _bodyVisibleState;
        }

        private void RestoreGameObjects()
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                var gameObject = _gameObjects[i];
                if (gameObject != null && gameObject.activeSelf != _activeStates[i])
                    gameObject.SetActive(_activeStates[i]);
            }
        }

        private void RestoreRenderers()
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                var renderer = _renderers[i];
                if (renderer != null && renderer.enabled != _enabledStates[i])
                    renderer.enabled = _enabledStates[i];
            }
        }
    }
}
