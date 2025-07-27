using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;
class VampireDependent : RoleBase<VampireDependent>
{
    public override RoleId Role => RoleId.VampireDependent;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new VampireDependentAbility(
        data: new VampireDependentData(
            killCooldown: Vampire.VampireDependentKillCooldown,
            canUseVent: Vampire.VampireDependentCanUseVent
        ),
        vampire: new VampireData(
            vampireInvisibleOnAdmin: Vampire.VampireInvisibleOnAdmin,
            vampireCannotFixSabotage: Vampire.VampireCannotFixSabotage,
            vampireCannotUseDevice: Vampire.VampireCannotUseDevice,
            vampireDependentHasReverseVision: Vampire.VampireDependentHasReverseVision,
            vampireDependentHasImpostorVisionInLightsoff: Vampire.VampireDependentHasImpostorVisionInLightsoff
        )
    )];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Shapeshifter;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [RoleTag.ImpostorTeam];
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Hidden;
}
public record VampireDependentData(float killCooldown, bool canUseVent);
public class VampireDependentAbility : AbilityBase
{
    public VampireAbility vampire { get; private set; }
    public VampireData VampireData { get; }
    public CustomVentAbility ventAbility;
    private CustomKillButtonAbility killButtonAbility;
    public VampireDependentData Data { get; }
    private EventListener<MurderEventData> _murderListener;
    private SabotageCanUseAbility sabotageCanUseAbility;
    private DeviceCanUseAbility deviceCanUseAbility;
    private HideInAdminAbility hideInAdminAbility;
    private ReverseVisionAbility reverseVisionAbility;
    public VampireDependentAbility(VampireDependentData data, VampireData vampire)
    {
        this.Data = data;
        this.VampireData = vampire;
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        killButtonAbility = new CustomKillButtonAbility(
            canKill: () => true,
            killCooldown: () => Data.killCooldown,
            onlyCrewmates: () => true,
            isTargetable: (player) => player != vampire?.Player
        );
        ventAbility = new CustomVentAbility(() => Data.canUseVent);
        sabotageCanUseAbility = new SabotageCanUseAbility(
            () => VampireData.vampireCannotFixSabotage ? SabotageType.Lights : SabotageType.None
        );
        deviceCanUseAbility = new DeviceCanUseAbility(
            () => VampireData.vampireCannotUseDevice ? DeviceTypeFlag.All : DeviceTypeFlag.None
        );
        hideInAdminAbility = new HideInAdminAbility(
            () => VampireData.vampireInvisibleOnAdmin
        );
        reverseVisionAbility = new ReverseVisionAbility(
            () => VampireData.vampireDependentHasReverseVision,
            () => VampireData.vampireDependentHasImpostorVisionInLightsoff
        );
        Player.AttachAbility(killButtonAbility, new AbilityParentAbility(this));
        Player.AttachAbility(ventAbility, new AbilityParentAbility(this));
        Player.AttachAbility(sabotageCanUseAbility, new AbilityParentAbility(this));
        Player.AttachAbility(deviceCanUseAbility, new AbilityParentAbility(this));
        Player.AttachAbility(hideInAdminAbility, new AbilityParentAbility(this));
        Player.AttachAbility(reverseVisionAbility, new AbilityParentAbility(this));
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
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
    private void OnMurder(MurderEventData data)
    {
        if (data.target == vampire?.Player && Player.IsAlive())
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.VampireWithDead);
    }
    public void SetVampire(VampireAbility vampire)
    {
        this.vampire = vampire;
    }
}