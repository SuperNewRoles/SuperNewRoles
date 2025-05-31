using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Revenant : GhostRoleBase<Revenant>
{
    public override GhostRoleId Role { get; } = GhostRoleId.Revenant;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new RevenantAbility(
        new(
            RequiredTasks: Necromancer.RevenantRequiredTasks,
            HauntDuration: Necromancer.RevenantHauntDuration,
            HauntVision: Necromancer.RevenantHauntVision,
            CannotReportWhileHaunted: Necromancer.RevenantCannotReportWhileHaunted
        )),
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.GhostRole];
    public override RoleId[] RelatedRoleIds => [RoleId.Necromancer];
    public override bool HiddenOption => true;
}

public record RevenantAbilityData(int RequiredTasks, int HauntDuration, float HauntVision, bool CannotReportWhileHaunted);
class RevenantAbility : TargetCustomButtonBase
{
    public RevenantAbilityData Data { get; }
    private List<(ExPlayerControl player, Arrow arrow)> Arrows { get; } = [];

    public override Color32 OutlineColor => Palette.ImpostorRed;

    public override bool OnlyCrewmates => true;

    public override float DefaultTimer => 0f;

    public override string buttonText => ModTranslation.GetString("RevenantHauntButtonText");

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("RevenantButton.png");

    protected override KeyType keytype => KeyType.Ability1;

    public override ShowTextType showTextType => ShowTextType.ShowWithCount;

    private byte checkcount;

    private CustomTaskAbility customTaskAbility;
    private TaskOptionData taskOptionData;

    private EventListener _fixedUpdateListener;
    private EventListener<TaskCompleteEventData> _taskCompleteListener;
    private EventListener<ShipStatusLightEventData> _shipStatusLightListener;
    private EventListener<EmergencyCheckEventData> _emergencyCheckListener;
    private EventListener<WrapUpEventData> _wrapUpListener;
    private EventListener _hudManagerUpdateListener;

    public List<(float time, ExPlayerControl player)> HauntedPlayers { get; } = [];
    public bool AmHaunted { get; private set; } = false;

    public override bool CheckHasButton()
        => Player == ExPlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data.IsDead;

    public override Func<ExPlayerControl, bool> IsTargetable => (player) => !player.IsImpostor();

    public Dictionary<ExPlayerControl, GameObject> NecromancerHitodamas = [];

    private CustomHauntToAbility customHauntToAbility;

    public RevenantAbility(RevenantAbilityData data)
    {
        Data = data;
        taskOptionData = new TaskOptionData(100, 100, 100);
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();

        customTaskAbility = new CustomTaskAbility(
            () => (true, false, Data.RequiredTasks),
            taskOptionData
        );
        customHauntToAbility = new CustomHauntToAbility(() => HauntedPlayers?.FirstOrDefault().player);
        Player.AttachAbility(customTaskAbility, new AbilityParentAbility(this));
        Player.AttachAbility(customHauntToAbility, new AbilityParentAbility(this));
        Player.AttachAbility(new DisibleHauntAbility(() => Player.GhostRole == GhostRoleId.Revenant), new AbilityParentAbility(this));

        if (Player.AmOwner)
            ReassignTasks();

        _shipStatusLightListener = ShipStatusLightEvent.Instance.AddListener(OnShipStatusLight);
        _emergencyCheckListener = EmergencyCheckEvent.Instance.AddListener(OnEmergencyCheck);
        _hudManagerUpdateListener = HudUpdateEvent.Instance.AddListener(OnHudManagerUpdate);
        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        NecromancerHitodamas = new();
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _shipStatusLightListener?.RemoveListener();
        _emergencyCheckListener?.RemoveListener();
        _hudManagerUpdateListener?.RemoveListener();
        HauntedPlayers.Clear();
        _wrapUpListener?.RemoveListener();
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        Arrows.Clear();
        RecheckArrows();
        UpdateArrows();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(FixedUpdate);
        _taskCompleteListener = TaskCompleteEvent.Instance.AddListener(OnTaskComplete);
        Count = 0;
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
        _taskCompleteListener?.RemoveListener();
        foreach (var arrow in Arrows)
        {
            if (arrow.arrow != null)
            {
                GameObject.Destroy(arrow.arrow.arrow);
            }
        }
        Arrows.Clear();
    }
    private void OnWrapUp(WrapUpEventData data)
    {
        if (!Player.AmOwner) return;
        foreach (var haunted in HauntedPlayers)
        {
            RpcSetRevenantStatus(this, haunted.player, false);
        }
    }
    private void OnTaskComplete(TaskCompleteEventData data)
    {
        if (!data.player.AmOwner) return;
        if (data.player.AllTasksCompleted())
        {
            ReassignTasks();
            Count++;
        }
    }
    public void FixedUpdate()
    {
        checkcount++;
        if (checkcount % 20 == 0)
        {
            RecheckArrows();
            checkcount = 0;
        }
        UpdateArrows();
        List<(float time, ExPlayerControl player)> hauntedPlayersFinished = [];
        foreach (var haunted in HauntedPlayers.ToArray())
        {
            if (haunted.time <= Time.time)
                hauntedPlayersFinished.Add(haunted);
        }
        foreach (var haunted in hauntedPlayersFinished)
            RpcSetRevenantStatus(this, haunted.player, false);
    }
    private void UpdateArrows()
    {
        if (Arrows.Count <= 0) return;
        foreach (var arrow in Arrows)
        {
            arrow.arrow.Update(arrow.player.transform.position);
        }
    }
    private void RecheckArrows()
    {
        Arrows.RemoveAll(arrow => arrow.arrow == null || arrow.player == null || arrow.player.Role != RoleId.Necromancer);
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.Role == RoleId.Necromancer && !Arrows.Any(arrow => arrow.player == player))
                Arrows.Add((player, new Arrow(Necromancer.Instance.RoleColor)));
        }
    }
    private void ReassignTasks()
    {
        Logger.Info("ReassignTasks");
        int vanillaLong = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
        int vanillaShort = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks);
        int vanillaCommon = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks);
        List<string> taskTypeList = [];

        for (int i = 0; i < vanillaCommon; i++)
            taskTypeList.Add("Common");
        for (int i = 0; i < vanillaShort; i++)
            taskTypeList.Add("Short");
        for (int i = 0; i < vanillaLong; i++)
            taskTypeList.Add("Long");

        taskOptionData.Short = taskOptionData.Common = taskOptionData.Long = 0;
        taskTypeList.Shuffle();
        for (int i = 0; i < Data.RequiredTasks; i++)
        {
            if (taskTypeList.Count <= i)
                taskOptionData.Short++;
            else if (taskTypeList[i] == "Common")
                taskOptionData.Common++;
            else if (taskTypeList[i] == "Short")
                taskOptionData.Short++;
            else if (taskTypeList[i] == "Long")
                taskOptionData.Long++;
        }

        customTaskAbility.AssignTasks();
    }

    public override bool CheckIsAvailable()
    {
        return Count > 0 && Target != null;
    }

    public override void OnClick()
    {
        if (Count <= 0) return;
        Logger.Info("Haunt to " + Target.Player.CurrentOutfit.PlayerName);
        Count--;
        RpcSetRevenantStatus(this, Target.Player, true);
    }
    private void OnShipStatusLight(ShipStatusLightEventData data)
    {
        if (data.player?.Object?.AmOwner == false) return;
        if (!AmHaunted) return;
        // 呪われたプレイヤーの光量を減らす
        data.lightRadius = Data.HauntVision;
    }
    private void OnEmergencyCheck(EmergencyCheckEventData data)
    {
        if (!AmHaunted) return;
        if (Data.CannotReportWhileHaunted)
        {
            data.RefEnabledEmergency = false;
            data.RefEmergencyTexts.Add(ModTranslation.GetString("RevenantCannotReportWhileHaunted"));
        }
    }
    [CustomRPC]
    public static void RpcSetRevenantStatus(RevenantAbility ability, ExPlayerControl player, bool isHaunted)
    {
        if (isHaunted)
        {
            if (player.AmOwner)
                ability.AmHaunted = true;
            ability.HauntedPlayers.Add((Time.time + ability.Data.HauntDuration, player));
            if (ExPlayerControl.LocalPlayer.Role == RoleId.Necromancer || ExPlayerControl.LocalPlayer.GhostRole == GhostRoleId.Revenant)
            {
                ability.NecromancerHitodamas[player] = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("NecromancerHitodama"));
                ability.NecromancerHitodamas[player].transform.SetParent(player.transform);
                ability.NecromancerHitodamas[player].transform.localPosition = new(0, 0.6f, -0.0001f);
                ability.NecromancerHitodamas[player].transform.localScale = Vector3.one * 0.8f;
            }
        }
        else
        {
            if (player.AmOwner)
                ability.AmHaunted = false;
            ability.HauntedPlayers.RemoveAll(haunted => haunted.player == player);
            if (ability.NecromancerHitodamas.ContainsKey(player) && (ExPlayerControl.LocalPlayer.Role == RoleId.Necromancer || ExPlayerControl.LocalPlayer.GhostRole == GhostRoleId.Revenant))
            {
                GameObject.Destroy(ability.NecromancerHitodamas[player]);
                ability.NecromancerHitodamas.Remove(player);
            }
        }
    }
    private void OnHudManagerUpdate()
    {
        if (AmHaunted && Data.CannotReportWhileHaunted)
            HudManager.Instance.ReportButton.SetDisabled();
    }
}