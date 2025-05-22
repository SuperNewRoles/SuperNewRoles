using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;
using HarmonyLib;

namespace SuperNewRoles.Roles.Neutral;

class Spelunker : RoleBase<Spelunker>
{
    public override RoleId Role { get; } = RoleId.Spelunker;
    public override Color32 RoleColor { get; } = new(255, 255, 0, byte.MaxValue); // 黄色
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new SpelunkerAbility(new SpelunkerData(
        VentDeathChance: SpelunkerVentDeathChance,
        CommsOrLightdownDeathTime: SpelunkerIsDeathCommsOrPowerdown ? SpelunkerDeathCommsOrPowerdownTime : -1f,
        LiftDeathChance: SpelunkerLiftDeathChance,
        DoorOpenChance: SpelunkerDoorOpenChance
    ))];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    [CustomOptionInt("SpelunkerVentDeathChance", 0, 100, 5, 20, translationName: "SpelunkerVentDeathChance")]
    public static int SpelunkerVentDeathChance;

    [CustomOptionBool("SpelunkerIsDeathCommsOrPowerdown", true, translationName: "SpelunkerIsDeathCommsOrPowerdown")]
    public static bool SpelunkerIsDeathCommsOrPowerdown;

    [CustomOptionFloat("SpelunkerDeathCommsOrPowerdownTime", 5f, 120f, 2.5f, 20f, translationName: "SpelunkerDeathCommsOrPowerdownTime", parentFieldName: nameof(SpelunkerIsDeathCommsOrPowerdown))]
    public static float SpelunkerDeathCommsOrPowerdownTime;

    [CustomOptionInt("SpelunkerLiftDeathChance", 0, 100, 5, 20, translationName: "SpelunkerLiftDeathChance")]
    public static int SpelunkerLiftDeathChance;

    [CustomOptionInt("SpelunkerDoorOpenChance", 0, 100, 5, 20, translationName: "SpelunkerDoorOpenChance")]
    public static int SpelunkerDoorOpenChance;
}

public record SpelunkerData(
    int VentDeathChance,
    float CommsOrLightdownDeathTime,
    int LiftDeathChance,
    int DoorOpenChance
);

public class SpelunkerAbility : AbilityBase
{
    public SpelunkerData Data { get; set; }

    private EventListener _fixedUpdateListener;
    private EventListener<MurderEventData> _murderListener;
    private EventListener<ExileEventData> _exileListener;

    // Spelunker specific variables
    private bool _isVentChecked;
    private float _commsOrLightdownTime;
    private Vector2? _deathPosition;

    private const float VentDistance = 0.35f;

    public SpelunkerAbility(SpelunkerData data)
    {
        Data = data;
        _commsOrLightdownTime = data.CommsOrLightdownDeathTime;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
        _exileListener = ExileEvent.Instance.AddListener(OnExile);
    }

    public override void DetachToLocalPlayer()
    {
        _fixedUpdateListener?.RemoveListener();
        _murderListener?.RemoveListener();
        _exileListener?.RemoveListener();
    }

    private void OnFixedUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null || ExileController.Instance != null) return;

        // ベント判定
        if (Data.VentDeathChance > 0)
        {
            CheckVentDeath();
        }

        // コミュと停電の不安死
        if (Data.CommsOrLightdownDeathTime != -1)
        {
            CheckCommsOrLightdownDeath();
        }

        // 転落死判定
        if (_deathPosition != null && Vector2.Distance((Vector2)_deathPosition, ExPlayerControl.LocalPlayer.transform.position) < 0.5f)
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
            FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.NunDeath);
            _deathPosition = null;
        }
    }

    private void CheckVentDeath()
    {
        Vent currentVent = null;
        bool nearVent = false;

        if (ShipStatus.Instance?.AllVents != null)
        {
            foreach (var vent in ShipStatus.Instance.AllVents)
            {
                if (Vector2.Distance(vent.transform.position + new Vector3(0, 0.15f, 0), ExPlayerControl.LocalPlayer.transform.position) < VentDistance)
                {
                    currentVent = vent;
                    nearVent = true;
                    break;
                }
            }
        }

        if (nearVent)
        {
            if (!_isVentChecked)
            {
                _isVentChecked = true;
                if (ModHelpers.IsSuccessChance(Data.VentDeathChance))
                {
                    ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
                    FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.SpelunkerVentDeath);
                }
            }
        }
        else
        {
            _isVentChecked = false;
        }
    }

    private void CheckCommsOrLightdownDeath()
    {
        if (ModHelpers.IsComms() || ModHelpers.IsElectrical())
        {
            _commsOrLightdownTime -= Time.fixedDeltaTime;
            if (_commsOrLightdownTime <= 0)
            {
                ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
                FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.SpelunkerCommsElecDeath);
            }
        }
        else
        {
            _commsOrLightdownTime = Data.CommsOrLightdownDeathTime;
        }
    }

    private void OnMurder(MurderEventData data)
    {
        // 他のロールに変更されると死ぬ (CheckSetRole equivalent)
        // このロジックは別途 RoleChangeEvent で実装する必要がある
    }

    private void OnExile(ExileEventData data)
    {
        // 会議終了時のリセット
        _deathPosition = null;
    }

    // MovingPlatformの使用時の転落死判定
    public void OnMovingPlatformUsed(MovingPlatformBehaviour platform)
    {
        if (ModHelpers.IsSuccessChance(Data.LiftDeathChance))
        {
            _deathPosition = platform.transform.parent.TransformPoint((!platform.IsLeft) ? platform.LeftUsePosition : platform.RightUsePosition);
            Logger.Info($"Spelunker lift death position set: {_deathPosition}");
        }
    }

    // ドア開放時の指挟み死
    public void OnDoorOpen()
    {
        if (ModHelpers.IsSuccessChance(Data.DoorOpenChance))
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
            FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.SpelunkerOpenDoor);
        }
    }
}

// Harmonyパッチをここに追加
[HarmonyPatch(typeof(MovingPlatformBehaviour), nameof(MovingPlatformBehaviour.UsePlatform))]
public static class MovingPlatformUsePlatformPatch
{
    public static void Postfix(MovingPlatformBehaviour __instance, PlayerControl target)
    {
        if (target.PlayerId == ExPlayerControl.LocalPlayer.PlayerId &&
            ExPlayerControl.LocalPlayer.Role == RoleId.Spelunker)
        {
            var spelunkerAbility = ExPlayerControl.LocalPlayer.GetAbility<SpelunkerAbility>();
            spelunkerAbility?.OnMovingPlatformUsed(__instance);
        }
    }
}

[HarmonyPatch(typeof(DoorConsole), nameof(DoorConsole.Use))]
public static class DoorConsoleOpenPatch
{
    public static void Postfix(DoorConsole __instance)
    {
        __instance.CanUse(ExPlayerControl.LocalPlayer.Data, out var canUse, out var _);
        if (canUse && ExPlayerControl.LocalPlayer.Role == RoleId.Spelunker)
        {
            var spelunkerAbility = ExPlayerControl.LocalPlayer.GetAbility<SpelunkerAbility>();
            spelunkerAbility?.OnDoorOpen();
        }
    }
}