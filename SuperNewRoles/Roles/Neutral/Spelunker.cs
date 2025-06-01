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
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new SpelunkerAbility(new SpelunkerData(
            VentDeathChance: SpelunkerVentDeathChance,
            CommsDeathTime: SpelunkerIsDeathCommsOrPowerdown ? SpelunkerDeathCommsTime : -1f,
            PowerdownDeathTime: SpelunkerIsDeathCommsOrPowerdown ? SpelunkerDeathPowerdownTime : -1f,
            LiftDeathChance: SpelunkerLiftDeathChance,
            DoorOpenChance: SpelunkerDoorOpenChance,
            LadderDeathChance: SpelunkerLadderDeathChance
        )),
        () => new LadderDeathAbility(SpelunkerLadderDeathChance)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    [CustomOptionBool("SpelunkerIsDeathCommsOrPowerdown", true, translationName: "SpelunkerIsDeathCommsOrPowerdown")]
    public static bool SpelunkerIsDeathCommsOrPowerdown;

    [CustomOptionFloat("SpelunkerDeathCommsTime", 5f, 120f, 2.5f, 20f, parentFieldName: nameof(SpelunkerIsDeathCommsOrPowerdown), suffix: "s")]
    public static float SpelunkerDeathCommsTime;

    [CustomOptionFloat("SpelunkerDeathPowerdownTime", 5f, 120f, 2.5f, 20f, parentFieldName: nameof(SpelunkerIsDeathCommsOrPowerdown), suffix: "s")]
    public static float SpelunkerDeathPowerdownTime;

    [CustomOptionInt("SpelunkerVentDeathChance", 0, 100, 5, 20, translationName: "SpelunkerVentDeathChance", suffix: "%")]
    public static int SpelunkerVentDeathChance;

    [CustomOptionInt("SpelunkerLiftDeathChance", 0, 100, 5, 20, translationName: "SpelunkerLiftDeathChance", suffix: "%")]
    public static int SpelunkerLiftDeathChance;

    [CustomOptionInt("SpelunkerDoorOpenChance", 0, 100, 5, 20, translationName: "SpelunkerDoorOpenChance", suffix: "%")]
    public static int SpelunkerDoorOpenChance;

    [CustomOptionInt("SpelunkerLadderDeathChance", 0, 100, 5, 20, translationName: "LadderDeadChance", suffix: "%")]
    public static int SpelunkerLadderDeathChance;

    [CustomOptionBool("SpelunkerIsAdditionalWin", false, translationName: "AdditionalWin")]
    public static bool SpelunkerIsAdditionalWin;
}

public record SpelunkerData(
    int VentDeathChance,
    float CommsDeathTime,
    float PowerdownDeathTime,
    int LiftDeathChance,
    int DoorOpenChance,
    int LadderDeathChance
);

public class SpelunkerAbility : AbilityBase
{
    public SpelunkerData Data { get; set; }

    private EventListener _fixedUpdateListener;
    private EventListener<ExileEventData> _exileListener;
    private EventListener<UsePlatformEventData> _usePlatformListener;
    private EventListener<DoorConsoleUseEventData> _doorConsoleUseListener;

    // Spelunker specific variables
    private bool _isVentChecked;
    private float _commsDeathTimer;
    private float _powerdownDeathTimer;
    private Vector2? _deathPosition;

    public SpelunkerAbility(SpelunkerData data)
    {
        Data = data;
        _commsDeathTimer = data.CommsDeathTime;
        _powerdownDeathTimer = data.PowerdownDeathTime;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _exileListener = ExileEvent.Instance.AddListener(OnExile);
        _usePlatformListener = UsePlatformEvent.Instance.AddListener(OnUsePlatform);
        _doorConsoleUseListener = DoorConsoleUseEvent.Instance.AddListener(OnDoorConsoleUse);
    }

    public override void DetachToLocalPlayer()
    {
        _fixedUpdateListener?.RemoveListener();
        _exileListener?.RemoveListener();
        _usePlatformListener?.RemoveListener();
        _doorConsoleUseListener?.RemoveListener();
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
        if (Data.CommsDeathTime != -1)
        {
            CheckCommsDeath();
        }
        if (Data.PowerdownDeathTime != -1)
        {
            CheckPowerdownDeath();
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
                if (Vector2.Distance(vent.transform.position, ExPlayerControl.LocalPlayer.transform.position) < vent.UsableDistance)
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

    private void CheckCommsDeath()
    {
        if (ModHelpers.IsComms())
        {
            _commsDeathTimer -= Time.fixedDeltaTime;
            if (_commsDeathTimer <= 0)
            {
                ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
                FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.SpelunkerCommsElecDeath);
            }
        }
        else
        {
            _commsDeathTimer = Data.CommsDeathTime;
        }
    }

    private void CheckPowerdownDeath()
    {
        if (ModHelpers.IsElectrical())
        {
            _powerdownDeathTimer -= Time.fixedDeltaTime;
            if (_powerdownDeathTimer <= 0)
            {
                ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
                FinalStatusManager.RpcSetFinalStatus(ExPlayerControl.LocalPlayer, FinalStatus.SpelunkerCommsElecDeath);
            }
        }
        else
        {
            _powerdownDeathTimer = Data.PowerdownDeathTime;
        }
    }
    private void OnExile(ExileEventData data)
    {
        // 会議終了時のリセット
        _deathPosition = null;
    }

    private void OnUsePlatform(UsePlatformEventData data)
    {
        if (data.player == ExPlayerControl.LocalPlayer)
        {
            OnMovingPlatformUsed(data.platform);
        }
    }

    private void OnDoorConsoleUse(DoorConsoleUseEventData data)
    {
        if (data.player == ExPlayerControl.LocalPlayer)
        {
            OnDoorOpen();
        }
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