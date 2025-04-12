using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

class Necromancer : RoleBase<Necromancer>
{
    public override RoleId Role { get; } = RoleId.Necromancer;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new VisibleGhostAbility(() => true),
        () => new NecromancerCurseButtonAbility(new NecromancerCurseButtonData(NecromancerMaxRevenants, NecromancerCurseCooldown)),
        () => new NecromancerRevenantArrowAbility()];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionFloat("NecromancerCurseCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float NecromancerCurseCooldown;

    [CustomOptionInt("NecromancerMaxRevenants", 1, 15, 1, 1)]
    public static int NecromancerMaxRevenants;

    [CustomOptionInt("RevenantRequiredTasks", 0, 15, 1, 3)]
    public static int RevenantRequiredTasks;

    [CustomOptionInt("RevenantHauntDuration", 1, 15, 1, 5)]
    public static int RevenantHauntDuration;

    [CustomOptionFloat("RevenantHauntVision", 0f, 1.0f, 0.1f, 0.1f)]
    public static float RevenantHauntVision;

    [CustomOptionBool("RevenantCannotReportWhileHaunted", true)]
    public static bool RevenantCannotReportWhileHaunted;
}
public record NecromancerCurseButtonData(int MaxCurseCount, float cooldown);
public class NecromancerCurseButtonAbility : TargetCustomButtonBase, IAbilityCount
{
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("NecromancerButton.png");
    public override Color32 OutlineColor => Palette.ImpostorRed;
    public override bool OnlyCrewmates => false;
    public override Func<bool> IsDeadPlayerOnly => () => true;
    public override Func<ExPlayerControl, bool> IsTargetable => (player) => player.Role != RoleId.Revenant;
    public override bool CheckIsAvailable() => Target != null && ExPlayerControl.LocalPlayer.Player.CanMove;
    protected override KeyType keytype => KeyType.Ability1;

    public override float DefaultTimer => Data.cooldown;
    public override string buttonText => ModTranslation.GetString("NecromancerCurseButtonText");

    public NecromancerCurseButtonData Data { get; }

    public override ShowTextType showTextType => ShowTextType.ShowWithCount;

    public NecromancerCurseButtonAbility(NecromancerCurseButtonData data)
    {
        Data = data;
        Count = Data.MaxCurseCount;
    }

    public override bool CheckHasButton()
        => base.CheckHasButton() && HasCount;

    public override void OnClick()
    {
        this.UseAbilityCount();
        RpcAssignRevenant(Target);
    }

    [CustomRPC]
    public static void RpcAssignRevenant(ExPlayerControl player)
    {
        Logger.Info($"AssignRevenant1");
        player.SetRole(RoleId.Revenant);
        Logger.Info($"AssignRevenant2");
        RoleManager.Instance.SetRole(player, RoleTypes.CrewmateGhost);
        Logger.Info($"AssignRevenant3");
        NameText.UpdateNameInfo(player);
        Logger.Info($"AssignRevenant4");
    }
}

public class NecromancerRevenantArrowAbility : AbilityBase
{
    private EventListener _fixedUpdateListener;
    private EventListener<MeetingCloseEventData> _meetingCloseListener;
    private List<(ExPlayerControl player, Arrow arrow)> _arrows = [];
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _meetingCloseListener = MeetingCloseEvent.Instance.AddListener(OnMeetingClose);
        _arrows = [];
    }

    public void OnMeetingClose(MeetingCloseEventData _)
    {
        foreach (var data in _arrows.ToArray())
        {
            if (data.player?.Role != RoleId.Revenant)
            {
                GameObject.Destroy(data.arrow.arrow);
            }
        }
        _arrows.RemoveAll(x => x.player?.Role != RoleId.Revenant);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
    }

    public void OnFixedUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        foreach (ExPlayerControl player in ExPlayerControl.ExPlayerControls)
        {
            if (player.Role != RoleId.Revenant) continue;
            var arrow = _arrows.FirstOrDefault(arrow => arrow.player == player);
            if (arrow == default)
                _arrows.Add((player, new Arrow(Necromancer.Instance.RoleColor)));
            else
                arrow.arrow.Update(player.transform.position);
        }
    }
}