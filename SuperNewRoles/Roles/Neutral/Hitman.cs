using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Hitman : RoleBase<Hitman>
{
    public override RoleId Role { get; } = RoleId.Hitman;
    public override Color32 RoleColor { get; } = new Color32(86, 41, 18, 255); // 茶色
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new HitmanAbility(new HitmanData(
            KillCooldown: HitmanKillCooldown,
            ChangeTargetTime: HitmanChangeTargetTime,
            WinKillCount: HitmanWinKillCount,
            IsOutMission: HitmanIsOutMission,
            OutMissionLimit: HitmanOutMissionLimit,
            CanUseVent: HitmanCanUseVent,
            HasImpostorVision: HitmanHasImpostorVision
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;
    public override RoleId[] RelatedRoleIds { get; } = [];

    // 殺し屋の設定
    [CustomOptionFloat("HitmanKillCooldown", 2.5f, 60f, 2.5f, 25f, translationName: "CoolTime")]
    public static float HitmanKillCooldown;

    [CustomOptionFloat("HitmanChangeTargetTime", 10f, 60f, 2.5f, 35f)]
    public static float HitmanChangeTargetTime;

    [CustomOptionInt("HitmanWinKillCount", 1, 10, 1, 3)]
    public static int HitmanWinKillCount;

    [CustomOptionBool("HitmanIsOutMission", false)]
    public static bool HitmanIsOutMission;

    [CustomOptionInt("HitmanOutMissionLimit", 0, 10, 1, 3, parentFieldName: nameof(HitmanIsOutMission))]
    public static int HitmanOutMissionLimit;

    [CustomOptionBool("HitmanCanUseVent", false, translationName: "CanUseVent")]
    public static bool HitmanCanUseVent;
    [CustomOptionBool("HitmanHasImpostorVision", false, translationName: "HasImpostorVision")]
    public static bool HitmanHasImpostorVision;

}

public record HitmanData(
    float KillCooldown,
    float ChangeTargetTime,
    int WinKillCount,
    int OutMissionLimit,
    bool CanUseVent,
    bool IsOutMission,
    bool HasImpostorVision
);

public class HitmanAbility : AbilityBase
{
    private CustomKillButtonAbility _killButtonAbility;
    private CustomVentAbility _ventAbility;
    private ShowPlayerUIAbility _showPlayerUIAbility;
    private ImpostorVisionAbility _impostorVisionAbility;

    public HitmanData Data { get; set; }

    private ExPlayerControl _currentTarget;

    private int _failedCount;
    private int _successCount;
    private float _timer;

    private EventListener _fixedUpdateListener;
    private EventListener<MurderEventData> _murderListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;

    private Arrow ArrowToTarget;

    public HitmanAbility(HitmanData data)
    {
        Data = data;
        _timer = Data.ChangeTargetTime;
    }
    public override void AttachToAlls()
    {
        _killButtonAbility = new CustomKillButtonAbility(
            canKill: () => true,
            killCooldown: () => Data.KillCooldown,
            onlyCrewmates: () => false,
            showTextType: () => ShowTextType.Show,
            showText: () => ModTranslation.GetString("HitmanFaildTimer", _timer.ToString("F1"))
        );
        _ventAbility = new CustomVentAbility(
            canUseVent: () => Data.CanUseVent
        );
        _showPlayerUIAbility = new ShowPlayerUIAbility(
            getPlayerList: () => [_currentTarget]
        );
        _impostorVisionAbility = new ImpostorVisionAbility(
            hasImpostorVision: () => Data.HasImpostorVision
        );

        Player.AttachAbility(_killButtonAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_ventAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_showPlayerUIAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_impostorVisionAbility, new AbilityParentAbility(this));

        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
        reSelect();
        ArrowToTarget = new Arrow(Color.red);
    }
    public override void DetachToLocalPlayer()
    {
        _fixedUpdateListener?.RemoveListener();
        _murderListener?.RemoveListener();
        GameObject.Destroy(ArrowToTarget?.arrow.gameObject);
        ArrowToTarget = null;
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateListener?.RemoveListener();
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player != Player) return;
        if (!data.Player.Player.Visible) return;
        string hitmanText = ModHelpers.Cs(Color.cyan, $"({_successCount}/{Data.WinKillCount})");
        if (Data.IsOutMission)
            hitmanText += ModHelpers.Cs(Color.red, $"({_failedCount}/{Data.OutMissionLimit})");
        Player.PlayerInfoText.text += hitmanText;
        if (Player.MeetingInfoText != null)
            Player.MeetingInfoText.text += hitmanText;
    }
    private void OnFixedUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null || ExileController.Instance != null) return;
        _timer -= Time.fixedDeltaTime;
        if (_currentTarget == null || _currentTarget.IsDead())
        {
            reSelect();
            return;
        }
        ArrowToTarget?.Update(_currentTarget.transform.position);
        if (_timer <= 0)
        {
            IncreaseFailedCount();
            NameText.UpdateNameInfo(ExPlayerControl.LocalPlayer);
            reSelect();
        }
    }
    private void reSelect()
    {
        _currentTarget = ModHelpers.GetRandom(ExPlayerControl.ExPlayerControls.Where(p => !p.AmOwner && !p.IsImpostor() && p.IsAlive()).ToList());
    }
    private void IncreaseFailedCount()
    {
        _timer = Data.ChangeTargetTime;
        _failedCount++;
        if (Data.IsOutMission && _failedCount >= Data.OutMissionLimit)
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
        RpcSyncCount();
    }
    public void OnMurder(MurderEventData data)
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (data.killer != Player) return;
        if (_currentTarget == data.target)
            SuccessfulKill();
        else
            IncreaseFailedCount();
    }

    public void SuccessfulKill()
    {
        _timer = Data.ChangeTargetTime;
        _successCount++;
        if (_successCount >= Data.WinKillCount)
        {
            EndGamer.RpcEndGameWithWinner(CustomGameOverReason.HitmanWin, WinType.Default, [Player], Hitman.Instance.RoleColor, "Hitman", string.Empty);
        }
        reSelect();
        RpcSyncCount();
    }
    private void RpcSyncCount()
    {
        RpcSyncCount(this, _successCount, _failedCount);
    }

    [CustomRPC]
    public static void RpcSyncCount(HitmanAbility ability, int successCount, int failedCount)
    {
        if (ability != null)
            ability.SetCount(successCount, failedCount);
        else
            Logger.Error("HitmanAbility is null");
    }
    private void SetCount(int successCount, int failedCount)
    {
        _successCount = successCount;
        _failedCount = failedCount;
    }
}