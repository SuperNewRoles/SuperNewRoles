using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public sealed class RemoteControllerAbility : AbilityBase
{
    private RemoteControllerMarkButton _markButton;
    private RemoteControllerOperationButton _operationButton;
    private CustomKillButtonAbility _killButton;

    private EventListener<MeetingStartEventData> _meetingStartListener;
    private EventListener<MeetingCloseEventData> _meetingCloseListener;
    private EventListener<DieEventData> _dieListener;
    private EventListener _fixedUpdateListener;

    private GameObject _targetUiContainer;
    private PoolablePlayer _targetIcon;
    private GameObject _lightChild;
    private float _syncTimer;
    private float _velocitySyncTimer;

    private static readonly HashSet<byte> _controlledPlayerIds = new();

    public byte TargetPlayerId { get; private set; } = byte.MaxValue;
    public bool UnderOperation { get; private set; }

    public float MarkCooldown { get; }
    public bool OperationInfinite { get; }
    public float OperationDuration { get; }

    public RemoteControllerAbility(float markCooldown, bool operationInfinite, float operationDuration)
    {
        MarkCooldown = markCooldown;
        OperationInfinite = operationInfinite;
        OperationDuration = operationDuration;
    }

    public ExPlayerControl TargetPlayer => TargetPlayerId == byte.MaxValue ? null : ExPlayerControl.ExPlayerControls.FirstOrDefault(p => p.PlayerId == TargetPlayerId);

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        _markButton = new RemoteControllerMarkButton(this);
        _operationButton = new RemoteControllerOperationButton(this);
        _killButton = new RemoteControllerKillButton(this);
        // Operationを先にしないと、Markを押した時に一気にオペレーション状態まで進んでしまう
        Player.AttachAbility(_operationButton, new AbilityParentAbility(this));
        Player.AttachAbility(_markButton, new AbilityParentAbility(this));
        Player.AttachAbility(_killButton, new AbilityParentAbility(this));
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _meetingCloseListener = MeetingCloseEvent.Instance.AddListener(OnMeetingClose);
        _dieListener = DieEvent.Instance.AddListener(OnDie);
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);

        CreateTargetUi();
        CreateLightChild();
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
        _meetingCloseListener?.RemoveListener();
        _dieListener?.RemoveListener();
        _fixedUpdateListener?.RemoveListener();

        StopOperationLocalOnly();

        if (_targetUiContainer != null)
        {
            UnityEngine.Object.Destroy(_targetUiContainer);
            _targetUiContainer = null;
        }
        _targetIcon = null;

        if (_lightChild != null)
        {
            UnityEngine.Object.Destroy(_lightChild);
            _lightChild = null;
        }
    }

    private void OnMeetingStart(MeetingStartEventData _)
    {
        if (!Player.AmOwner) return;
        CancelAndClearTarget();
    }

    private void OnMeetingClose(MeetingCloseEventData _)
    {
        if (!Player.AmOwner) return;
        StopOperation();
    }

    private void OnDie(DieEventData data)
    {
        if (!Player.AmOwner) return;
        if (data.player?.PlayerId != Player.PlayerId) return;
        CancelAndClearTarget();
    }

    private void OnFixedUpdate()
    {
        if (!Player.AmOwner) return;
        UpdateTargetUi();
        UpdateLight();

        if (!UnderOperation) return;
        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null || ExileController.Instance != null)
            return;

        Player.Player.moveable = false;
        var target = TargetPlayer;
        if (target == null || target.IsDead())
        {
            CancelAndClearTarget();
            return;
        }

        UpdateUseAndVentTargets(target);

        if (Minigame.Instance != null) return;
        if (!target.Player.CanMove) return;

        _syncTimer -= Time.fixedDeltaTime;
        _velocitySyncTimer -= Time.fixedDeltaTime;

        var joystick = FastDestroyableSingleton<HudManager>.Instance.joystick;
        var delta = joystick != null ? joystick.DeltaL : Vector2.zero;

        // 常にローカルでSetNormalizedVelocityを実行
        // NOTE: 操作者側でも見た目（自視点）を動かすために、所有者チェックで弾かない。
        // 実際の権限/同期は下のRPC側が担保する（ターゲット本人のクライアントが最終的に動かす）。
        if (target.MyPhysics != null)
        {
            target.MyPhysics.SetNormalizedVelocity(delta);
        }

        // 0.05秒ごとにRPCでネットワーク同期
        if (_velocitySyncTimer <= 0f)
        {
            _velocitySyncTimer = 0f; // 0秒ごとに制限
            RemoteControllerRpc.RpcSetNormalizedVelocity(target.PlayerId, delta.x, delta.y, target.transform.position);
        }
    }

    private void CreateTargetUi()
    {
        _targetUiContainer = new GameObject("RemoteControllerTargetUI");
        _targetUiContainer.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
        _targetUiContainer.transform.localPosition = new(-4.19f, -2.4f, 0f);
        _targetUiContainer.transform.localScale = Vector3.one * 0.3f;
        var aspectPosition = _targetUiContainer.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspectPosition.DistanceFromEdge = new(0.35f, 0.35f);
        aspectPosition.OnEnable();
        _targetUiContainer.SetActive(false);
    }

    private void UpdateTargetUi()
    {
        if (_targetUiContainer == null) return;

        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null)
        {
            if (_targetUiContainer.activeSelf) _targetUiContainer.SetActive(false);
            return;
        }

        if (TargetPlayerId == byte.MaxValue || !Player.AmOwner)
        {
            if (_targetUiContainer.activeSelf) _targetUiContainer.SetActive(false);
            return;
        }

        var target = TargetPlayer;
        if (target == null)
        {
            if (_targetUiContainer.activeSelf) _targetUiContainer.SetActive(false);
            return;
        }

        if (!_targetUiContainer.activeSelf) _targetUiContainer.SetActive(true);
        if (_targetIcon == null)
        {
            var prefab = FastDestroyableSingleton<HudManager>.Instance.IntroPrefab.PlayerPrefab;
            _targetIcon = UnityEngine.Object.Instantiate(prefab, _targetUiContainer.transform);
            _targetIcon.cosmetics.showColorBlindText = false;
            _targetIcon.cosmetics.isNameVisible = true;
            _targetIcon.cosmetics.UpdateNameVisibility();
            if (_targetIcon.cosmetics.colorBlindText != null) _targetIcon.cosmetics.colorBlindText.text = string.Empty;
            _targetIcon.transform.localPosition = new(0f, 0f, -0.3f);
        }

        _targetIcon.UpdateFromEitherPlayerDataOrCache(target.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
        _targetIcon.cosmetics.nameText.text = $"[ {ModTranslation.GetString("RemoteControllerTargetLabel")} ]\n{target.Data.PlayerName}";
    }

    private void CreateLightChild()
    {
        _lightChild = new GameObject("RemoteControllerLightChild") { layer = LayerExpansion.GetShadowLayer() };
        _lightChild.transform.position = Vector3.zero;
        _lightChild.transform.localScale = Vector3.zero;
        LightSource source = PlayerControl.LocalPlayer.LightPrefab;
        _lightChild.AddComponent<MeshFilter>().mesh = source.lightChildMesh;
        _lightChild.AddComponent<MeshRenderer>().material.shader = source.LightCutawayMaterial.shader;
    }

    private void UpdateLight()
    {
        if (_lightChild == null) return;
        if (!Player.AmOwner || !UnderOperation)
        {
            _lightChild.transform.localScale = Vector3.zero;
            return;
        }

        var target = TargetPlayer;
        if (target == null)
        {
            _lightChild.transform.localScale = Vector3.zero;
            return;
        }

        Vector3 position = target.transform.position;
        position.z -= 7f;
        _lightChild.transform.position = position;
        float size = ShipStatus.Instance != null
            ? (ShipStatus.Instance.MaxLightRadius * GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod) * 5.25f)
            : 0f;
        _lightChild.transform.localScale = new(size, size, 1f);
    }

    private void UpdateUseAndVentTargets(ExPlayerControl targetPlayer)
    {
        var local = PlayerControl.LocalPlayer;
        if (local == null) return;

        float closestDistance = float.MaxValue;
        float closestVentDistance = float.MaxValue;
        IUsable closestUsable = null;
        Vent closestVent = null;

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(targetPlayer.GetTruePosition(), local.MaxReportDistance, Constants.Usables))
        {
            if (!local.cache.TryGetValue(collider, out var cached))
            {
                local.cache[collider] = collider.GetComponents<IUsable>().ToArray();
                cached = local.cache[collider];
            }
            if (cached == null) continue;

            foreach (IUsable usable in cached)
            {
                if (usable == null) continue;
                if (usable.TryCast<Console>() &&
                    !usable.Il2CppIs(out DoorConsole _) &&
                    !usable.Il2CppIs(out PlatformConsole _) &&
                    !usable.Il2CppIs(out ZiplineConsole _))
                    continue;

                if (usable.Il2CppIs(out Vent vent))
                {
                    float d = RemoteVentCanUse(targetPlayer, vent, out bool canUse);
                    if (!canUse) continue;
                    if (d < closestVentDistance)
                    {
                        closestVentDistance = d;
                        closestVent = vent;
                    }
                }
                else
                {
                    float d = usable.CanUse(targetPlayer.Data, out bool canUse, out bool _);
                    if (!canUse) continue;
                    if (d < closestDistance)
                    {
                        closestDistance = d;
                        closestUsable = usable;
                    }
                }
            }
        }

        var hud = FastDestroyableSingleton<HudManager>.Instance;
        if (hud == null) return;
        hud.UseButton.SetTarget(closestUsable);
        hud.ImpostorVentButton.SetTarget(closestVent);
        local.closest = closestUsable;

        if (closestVent != null)
        {
            local.Data.Role.SetUsableTarget(closestVent.TryCast<IUsable>());
        }
        else
        {
            local.Data.Role.SetUsableTarget(closestUsable);
        }
    }

    internal static float RemoteVentCanUse(ExPlayerControl targetPlayer, Vent vent, out bool canUse)
    {
        canUse = false;
        if (targetPlayer?.Player == null || vent == null) return float.MaxValue;
        Bounds bounds = targetPlayer.Player.Collider.bounds;
        Vector3 center = bounds.center;
        Vector3 position = vent.transform.position;
        float distance = Vector2.Distance(center, position);
        canUse = distance <= vent.UsableDistance && !PhysicsHelpers.AnythingBetween(targetPlayer.Player.Collider, center, position, Constants.ShipOnlyMask, false);
        return distance;
    }

    public void CancelAndClearTarget()
    {
        StopOperation();
        RpcSetTarget(byte.MaxValue);
    }

    public void StopOperation()
    {
        if (!Player.AmOwner) return;
        if (!UnderOperation) return;

        var target = TargetPlayer;
        if (target != null && target.Player != null && target.Player.inVent && TryGetVentId(target.PlayerId, out int inVentId))
        {
            target.MyPhysics?.RpcExitVent(inVentId);
            ModHelpers.VentById(inVentId)?.SetButtons(false);
        }

        _operationButton?.ForceStopEffectLocal();
        RpcSetUnderOperation(false, TargetPlayerId);
        RpcSetTarget(byte.MaxValue);
        StopOperationLocalOnly();
        Player.ResetKillCooldown();
        Player.SetKillTimerUnchecked(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown),
            GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
        Player.Player.moveable = true;
    }

    private void StopOperationLocalOnly()
    {
        if (!Player.AmOwner) return;
        if (Camera.main != null)
        {
            var cam = Camera.main.GetComponent<FollowerCamera>();
            if (cam != null) cam.SetTarget(ExPlayerControl.LocalPlayer.Player);
        }
        if (_lightChild != null) _lightChild.transform.localScale = Vector3.zero;
    }

    [CustomRPC]
    public void RpcSetTarget(byte targetPlayerId)
    {
        TargetPlayerId = targetPlayerId;
        if (Player.AmOwner && TargetPlayerId == byte.MaxValue)
        {
            if (_targetUiContainer != null) _targetUiContainer.SetActive(false);
        }
    }

    [CustomRPC]
    public void RpcSetUnderOperation(bool underOperation, byte targetPlayerId)
    {
        if (underOperation)
        {
            RegisterControlledPlayer(targetPlayerId);
        }
        else
        {
            UnregisterControlledPlayer(targetPlayerId);
        }

        UnderOperation = underOperation;
        if (!underOperation)
        {
            _syncTimer = 2.5f;
            _velocitySyncTimer = 0f;
        }
    }

    private static void RegisterControlledPlayer(byte playerId)
    {
        if (playerId == byte.MaxValue) return;
        _controlledPlayerIds.Add(playerId);
    }

    private static void UnregisterControlledPlayer(byte playerId)
    {
        if (playerId == byte.MaxValue) return;
        _controlledPlayerIds.Remove(playerId);
    }

    public static bool IsPlayerBeingControlled(byte playerId)
    {
        return playerId != byte.MaxValue && _controlledPlayerIds.Contains(playerId);
    }

    public static bool IsAnyOperatingOn(ExPlayerControl possibleTarget)
    {
        if (possibleTarget == null) return false;
        return IsPlayerBeingControlled(possibleTarget.PlayerId);
    }

    public static bool TryGetLocalOperationTarget(out RemoteControllerAbility ability, out ExPlayerControl target)
    {
        ability = null;
        target = null;
        var local = ExPlayerControl.LocalPlayer;
        if (local == null) return false;
        if (!local.TryGetAbility<RemoteControllerAbility>(out ability)) return false;
        if (!ability.UnderOperation) return false;
        target = ability.TargetPlayer;
        return target != null;
    }

    public static bool TryGetVentId(byte playerId, out int ventId)
    {
        ventId = -1;
        if (ShipStatus.Instance == null) return false;
        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var system)) return false;
        if (!system.Il2CppIs(out VentilationSystem ventilation)) return false;
        if (!ventilation.PlayersInsideVents.TryGetValue(playerId, out byte id)) return false;
        ventId = id;
        return true;
    }
}

internal sealed class RemoteControllerMarkButton : TargetCustomButtonBase
{
    private readonly RemoteControllerAbility _ability;
    public RemoteControllerMarkButton(RemoteControllerAbility ability) { _ability = ability; }

    public override float DefaultTimer => _ability.MarkCooldown;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("RemoteControllerOperationButton.png");
    public override string buttonText => ModTranslation.GetString("RemoteControllerMarkButton");
    protected override KeyType keytype => KeyType.Ability1;

    public override Color32 OutlineColor => Palette.ImpostorRed;
    public override bool OnlyCrewmates => true;
    public override IEnumerable<PlayerControl> UntargetablePlayers =>
        ExPlayerControl.ExPlayerControls
            .Where(p => p != null && p.TryGetAbility<RemoteControllerAbility>(out var a) && a.TargetPlayerId != byte.MaxValue && a.TargetPlayer != null)
            .Select(p => (PlayerControl)p.GetAbility<RemoteControllerAbility>().TargetPlayer);

    public override bool CheckHasButton() => base.CheckHasButton() && _ability.TargetPlayerId == byte.MaxValue;

    public override bool CheckIsAvailable() => Player.IsAlive() && TargetIsExist;

    public override void OnClick()
    {
        if (Target == null) return;
        _ability.RpcSetTarget(Target.PlayerId);
    }
}

internal sealed class RemoteControllerKillButton : CustomKillButtonAbility
{
    private readonly RemoteControllerAbility _ability;

    public RemoteControllerKillButton(RemoteControllerAbility ability) : base(
        canKill: () => true,
        killCooldown: () => GameOptionsManager.Instance?.CurrentGameOptions?.GetFloat(FloatOptionNames.KillCooldown) ?? 0f,
        onlyCrewmates: () => true,
        customKillHandler: (target) =>
        {
            if (!ability.UnderOperation || ability.TargetPlayerId == byte.MaxValue) return false;
            var operatorTarget = ability.TargetPlayer;
            if (operatorTarget == null) return false;
            operatorTarget.Player.RpcCustomMurderPlayer(target, true);
            ability.CancelAndClearTarget();
            return true;
        }
    )
    {
        _ability = ability;
    }

    public override PlayerControl TargetingPlayer
        => (_ability.UnderOperation && _ability.TargetPlayer?.Player != null)
            ? _ability.TargetPlayer.Player
            : PlayerControl.LocalPlayer;

    public override bool CheckDecreaseCoolCount()
    {
        if (DestroyableSingleton<HudManager>.Instance.IsIntroDisplayed)
            return false;

        var targetingPlayer = TargetingPlayer;
        if (targetingPlayer == null)
            return false;

        var moveable = !targetingPlayer.inVent && targetingPlayer.moveable;
        return !targetingPlayer.inVent && moveable;
    }

    public override bool CheckIsAvailable()
    {
        if (!TargetIsExist) return false;
        if (!CanKill()) return false;
        if (ExPlayerControl.LocalPlayer.IsDead()) return false;
        // 操作中はCanMoveをチェックしない（操作中はmoveableがfalseになるため）
        return true;
    }
}

internal sealed class RemoteControllerOperationButton : CustomButtonBase, IButtonEffect
{
    private readonly RemoteControllerAbility _ability;
    public RemoteControllerOperationButton(RemoteControllerAbility ability) { _ability = ability; }

    public override float DefaultTimer => 0f;
    public override bool IsFirstCooldownTenSeconds => false;
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("RemoteControllerOperationButton.png");
    public override string buttonText => ModTranslation.GetString("RemoteControllerOperateButton");
    protected override KeyType keytype => KeyType.Ability1;

    public override bool CheckHasButton() => base.CheckHasButton() && _ability.TargetPlayerId != byte.MaxValue;

    public override bool CheckIsAvailable() => Player.IsAlive() && !_ability.UnderOperation;

    public override void OnClick()
    {
        if (_ability.TargetPlayerId == byte.MaxValue) return;
        if (!_ability.UnderOperation)
        {
            var target = _ability.TargetPlayer;
            if (target == null) return;
            if (target.Player != null && target.Player.inVent && RemoteControllerAbility.TryGetVentId(target.PlayerId, out int inVentId))
            {
                ModHelpers.VentById(inVentId)?.SetButtons(true);
            }
            if (Camera.main != null)
            {
                var cam = Camera.main.GetComponent<FollowerCamera>();
                if (cam != null) cam.SetTarget(target.Player);
            }
            ExPlayerControl.LocalPlayer.MyPhysics.body.velocity = Vector2.zero;
            _ability.RpcSetUnderOperation(true, _ability.TargetPlayerId);
        }
    }

    public Action OnEffectEnds => () =>
    {
        _ability.StopOperation();
    };

    public float EffectDuration => _ability.OperationDuration;
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    public bool IsEffectDurationInfinity => _ability.OperationInfinite;
    public bool effectCancellable => true;

    public void ForceStopEffectLocal()
    {
        isEffectActive = false;
    }
}

public static class RemoteControllerRpc
{
    [CustomRPC(onlyOtherPlayer: true)]
    public static void RpcSetNormalizedVelocity(byte targetPlayerId, float x, float y, Vector3 position)
    {
        var target = ExPlayerControl.ExPlayerControls.FirstOrDefault(p => p != null && p.PlayerId == targetPlayerId);
        if (target == null || !target.AmOwner) return;
        if (MeetingHud.Instance != null || ExileController.Instance != null) return;
        if (target.Player == null || target.MyPhysics == null) return;
        target.MyPhysics.SetNormalizedVelocity(new Vector2(x, y));
    }

    [CustomRPC(onlyOtherPlayer: false)]
    public static void RpcMoveVent(byte targetPlayerId, int ventId)
    {
        var target = (ExPlayerControl)PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == targetPlayerId);
        if (target == null) return;
        if (ShipStatus.Instance == null) return;
        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var system)) return;
        if (!system.Il2CppIs(out VentilationSystem ventilation)) return;
        VentilationSystem.Update(VentilationSystem.Operation.Move, ventId);
        if (target.AmOwner)
        {
            Vent.currentVent = ModHelpers.VentById(ventId);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl))]
public static class RemoteControllerPlayerControlPatch
{
    [HarmonyPatch(nameof(PlayerControl.CanMove), MethodType.Getter), HarmonyPostfix]
    public static void CanMoveGetterPostfix(PlayerControl __instance, ref bool __result)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
        if (HudManager.Instance.IsIntroDisplayed) return;
        if (!__instance.AmOwner) return;
        if (!__result) return;

        var exPlayer = (ExPlayerControl)__instance;
        if (exPlayer.TryGetAbility<RemoteControllerAbility>(out var myAbility) && myAbility.UnderOperation)
        {
            __result = false;
            return;
        }
        if (RemoteControllerAbility.IsAnyOperatingOn(exPlayer))
        {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.Deserialize))]
public static class RemoteControllerNetworkTransformPatch
{
    public static bool Prefix(CustomNetworkTransform __instance)
    {
        var local = ExPlayerControl.LocalPlayer;
        if (local == null) return true;
        if (!local.TryGetAbility<RemoteControllerAbility>(out var ability)) return true;
        if (!ability.UnderOperation) return true;
        if (GeneralSettingOptions.NetworkTransformType != NetworkTransformType.Vanilla) return true;

        var target = ability.TargetPlayer;
        if (target == null) return true;

        // 操作者側のクライアントだけ、ターゲットのネットワーク同期を受信しない
        return __instance.myPlayer != target.Player;
    }
}


[HarmonyPatch(typeof(UseButton), nameof(UseButton.DoClick))]
public static class RemoteControllerUseButtonPatch
{
    public static bool Prefix(UseButton __instance)
    {
        if (!RemoteControllerAbility.TryGetLocalOperationTarget(out _, out var operatorTarget)) return true;
        if (__instance == null || !__instance.isActiveAndEnabled || __instance.isCoolingDown) return false;
        if (__instance.currentTarget == null) return false;

        if (__instance.currentTarget.Il2CppIs(out DoorConsole console))
        {
            try
            {
                Minigame minigame = UnityEngine.Object.Instantiate(console.MinigamePrefab, Camera.main.transform);
                minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
                minigame.Cast<IDoorMinigame>().SetDoor(console.MyDoor);
                minigame.Begin(null);
            }
            catch
            {
                if (Minigame.Instance) UnityEngine.Object.Destroy(Minigame.Instance.gameObject);
            }
            return false;
        }

        if (__instance.currentTarget.Il2CppIs(out Ladder ladder))
        {
            operatorTarget.MyPhysics?.RpcClimbLadder(ladder);
            return false;
        }
        if (__instance.currentTarget.Il2CppIs(out PlatformConsole platformConsole))
        {
            operatorTarget.Player?.RpcUsePlatform();
            return false;
        }
        if (__instance.currentTarget.Il2CppIs(out ZiplineConsole zipline))
        {
            operatorTarget.Player?.RpcUseZipline(operatorTarget.Player, zipline.zipline, zipline.atTop);
            return false;
        }

        return false;
    }
}

[HarmonyPatch(typeof(VentButton), nameof(VentButton.DoClick))]
public static class RemoteControllerVentButtonPatch
{
    public static bool Prefix(VentButton __instance)
    {
        if (!RemoteControllerAbility.TryGetLocalOperationTarget(out _, out var operatorTarget)) return true;
        if (__instance == null || !__instance.isActiveAndEnabled || __instance.isCoolingDown) return false;

        if (operatorTarget.Player == null || operatorTarget.MyPhysics == null) return false;

        if (operatorTarget.Player.inVent)
        {
            if (!RemoteControllerAbility.TryGetVentId(operatorTarget.PlayerId, out int inVentId)) return false;
            operatorTarget.MyPhysics.RpcExitVent(inVentId);
            ModHelpers.VentById(inVentId)?.SetButtons(false);
            return false;
        }

        if (__instance.currentTarget == null) return false;
        var vent = __instance.currentTarget;
        RemoteControllerAbility.RemoteVentCanUse(operatorTarget, vent, out bool canUse);
        if (!canUse) return false;
        operatorTarget.MyPhysics.RpcEnterVent(vent.Id);
        vent.SetButtons(true);
        return false;
    }
}

[HarmonyPatch(typeof(Vent), nameof(Vent.TryMoveToVent))]
public static class RemoteControllerVentMovePatch
{
    public static bool Prefix(Vent __instance, ref bool __result, Vent otherVent, ref string error)
    {
        if (!RemoteControllerAbility.TryGetLocalOperationTarget(out _, out var operatorTarget)) return true;
        if (otherVent == null)
        {
            error = "Vent does not exist";
            __result = false;
            return false;
        }
        if (operatorTarget.Player == null || operatorTarget.MyPhysics == null)
        {
            error = "Target is null";
            __result = false;
            return false;
        }
        if (!operatorTarget.Player.inVent)
        {
            error = "Player is not currently inside a vent";
            __result = false;
            return false;
        }
        if (operatorTarget.Player.walkingToVent || operatorTarget.Player.Visible)
        {
            error = "Player was still in the middle of animating into current vent";
            __result = false;
            return false;
        }

        Vector3 position = otherVent.transform.position;
        position -= (Vector3)operatorTarget.Player.Collider.offset;

        operatorTarget.RpcCustomSnapTo(position);
        operatorTarget.NetTransform?.SnapTo(position);

        if (Constants.ShouldPlaySfx())
        {
            var clip = ShipStatus.Instance?.VentMoveSounds?.ToArray()?.FirstOrDefault();
            if (clip != null) SoundManager.Instance.PlaySound(clip, loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
        }

        __instance.SetButtons(enabled: false);
        otherVent.SetButtons(enabled: true);

        RemoteControllerRpc.RpcMoveVent(operatorTarget.PlayerId, otherVent.Id);

        error = string.Empty;
        __result = true;
        return false;
    }
}
