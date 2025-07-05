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
            vampireDependentHasImpostorVisionInLightsoff: VampireDependentHasImpostorVisionInLightsoff,
            vampireNoDeathOnVitals: VampireNoDeathOnVitals
        )
    )];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Shapeshifter;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => new RoleTag[] { RoleTag.SpecialKiller };
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

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
public record VampireData(bool vampireInvisibleOnAdmin, bool vampireCannotFixSabotage, bool vampireCannotUseDevice, bool vampireDependentHasReverseVision, bool vampireDependentHasImpostorVisionInLightsoff, bool vampireNoDeathOnVitals);
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
    private EventListener<MeetingCalledAnimationInitializeEventData> _meetingCalledListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;

    private float delayTimer;
    private float bloodStainTimer;
    private bool isKillDelayed = false;
    private Vector3 killPosition;
    private SabotageCanUseAbility sabotageCanUseAbility;
    private DeviceCanUseAbility deviceCanUseAbility;
    private Dictionary<ExPlayerControl, Transform> bloodStainsParents = new();
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
            sidekickSprite: AssetManager.GetAsset<Sprite>("VampireNightFallButton.png"),
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
        _meetingCalledListener = MeetingCalledAnimationInitializeEvent.Instance.AddListener(OnMeetingCalled);
    }
    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _fixedUpdateListener?.RemoveListener();
        _murderListener?.RemoveListener();
        _wrapUpListener?.RemoveListener();
        _meetingCalledListener?.RemoveListener();
    }
    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _nameTextUpdateListener?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        // 眷属の名前に印を付ける
        if (dependent?.Player == data.Player)
        {
            NameText.SetNameTextColor(data.Player, Palette.ImpostorRed, true);
        }
    }

    private void OnMeetingCalled(MeetingCalledAnimationInitializeEventData data)
    {
        // 会議が呼ばれた時、遅延キル中なら即座にキルを実行
        if (TargetingPlayer != null && TargetingPlayer.IsAlive())
        {
            ExecuteDelayedKill();
        }
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

        // 各プレイヤーの血痕を有効化
        foreach (var kvp in bloodStainsParents)
        {
            if (kvp.Value != null)
            {
                kvp.Value.gameObject.SetActive(true);
                bloodStainParent.Add((kvp.Value, kill.bloodStainDurationTurn - 1));
            }
        }
        bloodStainsParents.Clear();
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
        isKillDelayed = true;
        killPosition = Player.transform.position;

        // プレイヤーごとの血痕親オブジェクトを作成
        if (!bloodStainsParents.ContainsKey(player))
        {
            var parent = new GameObject($"BloodStains_{player.PlayerId}").transform;
            parent.gameObject.SetActive(false);
            bloodStainsParents[player] = parent;
        }

        bloodStainTimer = 0;

        // 眷属のキルクールをリセット（ヴァンパイアがキルボタンを押した瞬間）
        if (dependent?.Player?.AmOwner == true)
            dependent.Player.ResetKillCooldown();
    }

    private void ExecuteDelayedKill()
    {
        if (TargetingPlayer == null || TargetingPlayer.IsDead()) return;

        var targetParent = bloodStainsParents.GetValueOrDefault(TargetingPlayer);
        if (targetParent != null)
        {
            new BloodStain(TargetingPlayer, isBlack: kill.blackBloodstains, parent: targetParent).BloodStainObject.transform.localScale *= 3f;
        }

        TargetingPlayer.CustomDeath(CustomDeathType.VampireKill, source: Player);

        // 遅延キルが完了したらキルクールを開始
        if (Player.AmOwner)
            Player.ResetKillCooldown();

        TargetingPlayer = null;
        isKillDelayed = false;
    }

    private void OnFixedUpdate()
    {
        if (TargetingPlayer == null || TargetingPlayer.IsDead()) return;

        delayTimer -= Time.fixedDeltaTime;
        if (delayTimer <= 0)
        {
            ExecuteDelayedKill();
            return;
        }

        bloodStainTimer -= Time.fixedDeltaTime;
        if (bloodStainTimer <= 0)
        {
            var targetParent = bloodStainsParents.GetValueOrDefault(TargetingPlayer);
            if (targetParent != null)
            {
                new BloodStain(TargetingPlayer, isBlack: kill.blackBloodstains, parent: targetParent);
            }
            bloodStainTimer = 0.1f;
        }
    }

    private static bool CancelVitalsDead(ExPlayerControl player)
    {
        if (player.Role is RoleId.Vampire or RoleId.VampireDependent)
        {
            var vampireAbility = player.GetAbility<VampireAbility>();
            var dependentAbility = player.GetAbility<VampireDependentAbility>();

            // VampireNoDeathOnVitals設定がtrueの場合のみバイタル表示を隠す
            return (vampireAbility?.Vampire.vampireNoDeathOnVitals ?? false) ||
                   (dependentAbility?.VampireData.vampireNoDeathOnVitals ?? false);
        }
        return false;
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