using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine.PlayerLoop;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.CustomObject;
using HarmonyLib;

namespace SuperNewRoles.Roles.Impostor;
class Vampire : RoleBase<Vampire>
{
    public override RoleId Role => RoleId.Vampire;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new VampireAbility
    (
        nightFall: new VampireNightFallData(
            canCreate: VampireCreateDependents,
            cooldown: VampireNightFallCooldown
        ),
        kill: new VampireKillData(
            killCooldown: VampireAbsorptionCooldown,
            killDelay: VampireKillDelay,
            blackBloodstains: VampireBlackBloodstains,
            bloodStainDurationTurn: VampireBloodstainDurationTurn
        ),
        vampire: new VampireData(
            vampireInvisibleOnAdmin: VampireInvisibleOnAdmin,
            vampireCannotFixSabotage: VampireCannotFixSabotage,
            vampireCannotUseDevice: VampireCannotUseDevice,
            vampireDependentHasReverseVision: VampireDependentHasReverseVision,
            vampireDependentHasImpostorVisionInLightsoff: VampireDependentHasImpostorVisionInLightsoff
        )
    )];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Shapeshifter;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => new RoleTag[] { RoleTag.SpecialKiller };
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Hidden;

    [CustomOptionFloat("VampireAbsorptionCooldown", 2.5f, 60f, 2.5f, 30f)]
    public static float VampireAbsorptionCooldown;

    [CustomOptionFloat("VampireKillDelay", 1f, 30f, 1f, 10f)]
    public static float VampireKillDelay;

    [CustomOptionInt("VampireBloodstainDuration", 1, 5, 1, 1)]
    public static int VampireBloodstainDurationTurn;

    [CustomOptionBool("VampireBlackBloodstains", false)]
    public static bool VampireBlackBloodstains;

    [CustomOptionBool("VampireCreateDependents", false)]
    public static bool VampireCreateDependents;

    [CustomOptionFloat("VampireNightFallCooldown", 2.5f, 60f, 2.5f, 30f, parentFieldName: nameof(VampireCreateDependents))]
    public static float VampireNightFallCooldown;

    [CustomOptionFloat("VampireDependentKillCooldown", 2.5f, 60f, 2.5f, 30f, parentFieldName: nameof(VampireCreateDependents))]
    public static float VampireDependentKillCooldown;

    [CustomOptionBool("VampireDependentCanUseVent", true, parentFieldName: nameof(VampireCreateDependents))]
    public static bool VampireDependentCanUseVent;

    [CustomOptionBool("VampireDependentHasReverseVision", true, parentFieldName: nameof(VampireCreateDependents))]
    public static bool VampireDependentHasReverseVision;

    [CustomOptionBool("VampireDependentHasImpostorVisionInLightsoff", true, parentFieldName: nameof(VampireCreateDependents))]
    public static bool VampireDependentHasImpostorVisionInLightsoff;

    [CustomOptionBool("VampireCannotFixSabotage", true)]
    public static bool VampireCannotFixSabotage;

    [CustomOptionBool("VampireInvisibleOnAdmin", true)]
    public static bool VampireInvisibleOnAdmin;

    [CustomOptionBool("VampireNoDeathOnVitals", true)]
    public static bool VampireNoDeathOnVitals;

    [CustomOptionBool("VampireCannotUseDevice", true)]
    public static bool VampireCannotUseDevice;
}
public record VampireNightFallData(bool canCreate, float cooldown);
public record VampireKillData(float killCooldown, float killDelay, bool blackBloodstains, int bloodStainDurationTurn);
public record VampireData(bool vampireInvisibleOnAdmin, bool vampireCannotFixSabotage, bool vampireCannotUseDevice, bool vampireDependentHasReverseVision, bool vampireDependentHasImpostorVisionInLightsoff);
public class VampireAbility : AbilityBase
{
    private VampireDependentAbility dependent;
    private CustomSidekickButtonAbility sidekickButtonAbility;
    private CustomKillButtonAbility killButtonAbility;
    private bool created = false;
    public VampireNightFallData nightFall { get; }
    public VampireKillData kill { get; }
    public ExPlayerControl TargetingPlayer { get; private set; }
    public VampireData Vampire { get; }
    private EventListener _fixedUpdateListener;
    private EventListener<MurderEventData> _murderListener;
    private EventListener<WrapUpEventData> _wrapUpListener;

    private float delayTimer;
    private float bloodStainTimer;
    private SabotageCanUseAbility sabotageCanUseAbility;
    private DeviceCanUseAbility deviceCanUseAbility;
    private Transform bloodStainsParentTargeting;
    private List<(Transform bloodStainsParent, int limitedTurn)> bloodStainParent = new();
    private HideInAdminAbility hideInAdminAbility;
    public VampireAbility(VampireNightFallData nightFall, VampireKillData kill, VampireData vampire)
    {
        this.nightFall = nightFall;
        this.kill = kill;
        this.Vampire = vampire;
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        sidekickButtonAbility = new CustomSidekickButtonAbility(new(
            canCreateSidekick: (_) => nightFall.canCreate && !created,
            sidekickCooldown: () => nightFall.cooldown,
            sidekickRole: () => RoleId.VampireDependent,
            sidekickRoleVanilla: () => RoleTypes.Crewmate,
            sidekickSprite: AssetManager.GetAsset<Sprite>("VampireSidekickButton.png"),
            sidekickText: ModTranslation.GetString("VampireDependentsButtonText"),
            sidekickCount: () => 1,
            isTargetable: (player) => !player.IsImpostor(),
            onSidekickCreated: (player) =>
            {
                new LateTask(() => RpcSetDependent(player), 0.1f, "VampireSetDependent");
            }
        ));
        killButtonAbility = new CustomKillButtonAbility(
            canKill: () => true,
            killCooldown: () => kill.killCooldown,
            onlyCrewmates: () => true,
            isTargetable: (player) => !player.IsImpostor() && dependent?.Player != player,
            customKillHandler: (target) =>
            {
                Logger.Info("CustomKILLED!!!!!!!!");
                RpcSetTargetingPlayer(target);
                return true;
            }
        );
        hideInAdminAbility = new HideInAdminAbility(
            () => Vampire.vampireInvisibleOnAdmin
        );
        sabotageCanUseAbility = new SabotageCanUseAbility(
            () => Vampire.vampireCannotFixSabotage ? SabotageType.Lights : SabotageType.None
        );
        deviceCanUseAbility = new DeviceCanUseAbility(
            () => Vampire.vampireCannotUseDevice ? DeviceTypeFlag.All : DeviceTypeFlag.None
        );

        Player.AttachAbility(sidekickButtonAbility, new AbilityParentAbility(this));
        Player.AttachAbility(killButtonAbility, new AbilityParentAbility(this));
        Player.AttachAbility(sabotageCanUseAbility, new AbilityParentAbility(this));
        Player.AttachAbility(deviceCanUseAbility, new AbilityParentAbility(this));
        Player.AttachAbility(hideInAdminAbility, new AbilityParentAbility(this));
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _fixedUpdateListener?.RemoveListener();
        _murderListener?.RemoveListener();
        _wrapUpListener?.RemoveListener();
    }
    private void OnWrapUp(WrapUpEventData data)
    {
        for (int i = bloodStainParent.Count - 1; i >= 0; i--)
        {
            var (parent, turn) = bloodStainParent[i];
            if (turn <= 0)
            {
                GameObject.Destroy(parent.gameObject);
                bloodStainParent.RemoveAt(i);
            }
            else
            {
                bloodStainParent[i] = (parent, turn - 1);
            }
        }
        if (bloodStainsParentTargeting == null) return;
        bloodStainsParentTargeting.gameObject.SetActive(true);
        bloodStainParent.Add((bloodStainsParentTargeting, kill.bloodStainDurationTurn - 1));
    }
    private void OnMurder(MurderEventData data)
    {
        if (Player.AmOwner && data.killer == dependent?.Player)
            ExPlayerControl.LocalPlayer.ResetKillCooldown();
    }
    [CustomRPC]
    public void RpcSetDependent(ExPlayerControl player)
    {
        dependent = player.GetAbility<VampireDependentAbility>();
        dependent.SetVampire(this);
        this.created = true;
    }
    [CustomRPC]
    public void RpcSetTargetingPlayer(ExPlayerControl player)
    {
        TargetingPlayer = player;
        delayTimer = kill.killDelay;
        bloodStainsParentTargeting = new GameObject($"BloodStainsTargeting to {player.PlayerId}").transform;
        bloodStainsParentTargeting.gameObject.SetActive(false);
        bloodStainTimer = 0;
        if (dependent?.Player?.AmOwner == false)
            return;
        dependent?.Player.ResetKillCooldown();
    }
    private void OnFixedUpdate()
    {
        if (TargetingPlayer == null || TargetingPlayer.IsDead()) return;
        delayTimer -= Time.fixedDeltaTime;
        if (delayTimer <= 0)
        {
            delayTimer = 0;
            new BloodStain(TargetingPlayer, isBlack: kill.blackBloodstains, parent: bloodStainsParentTargeting).BloodStainObject.transform.localScale *= 3f;
            TargetingPlayer.CustomDeath(CustomDeathType.VampireKill, source: Player);
            TargetingPlayer = null;
            return;
        }
        bloodStainTimer -= Time.fixedDeltaTime;
        if (bloodStainTimer <= 0)
        {
            // 親で一括管理してる
            new BloodStain(TargetingPlayer, isBlack: kill.blackBloodstains, parent: bloodStainsParentTargeting);
            bloodStainTimer = 0.1f;
        }
    }

    private static bool CancelVitalsDead(ExPlayerControl player)
    {
        return player.Role is RoleId.Vampire or RoleId.VampireDependent;
    }


    [HarmonyPatch(typeof(VitalsPanel), nameof(VitalsPanel.SetDead))]
    class VitalsPanelSetDeadPatch
    {
        static bool Prefix(VitalsPanel __instance)
        {
            return __instance.PlayerInfo.Object == null || !CancelVitalsDead(__instance.PlayerInfo.Object);
        }
    }
    [HarmonyPatch(typeof(VitalsPanel), nameof(VitalsPanel.SetDisconnected))]
    class VitalsPanelSetDisconnectPatch
    {
        static bool Prefix(VitalsPanel __instance)
        {
            return __instance.PlayerInfo.Object == null || !CancelVitalsDead(__instance.PlayerInfo.Object);
        }
    }
}