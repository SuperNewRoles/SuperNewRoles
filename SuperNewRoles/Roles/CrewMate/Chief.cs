using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine.AddressableAssets;

namespace SuperNewRoles.Roles.Crewmate;

class Chief : RoleBase<Chief>
{
    public override RoleId Role { get; } = RoleId.Chief;
    public override Color32 RoleColor { get; } = Sheriff.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new ChiefAbility(
        new(ChiefSheriffKillCooldown, ChiefSheriffMaxKillCount, ChiefSheriffCanKillNeutral, ChiefSheriffCanKillImpostor, ChiefSheriffCanKillMadRoles, ChiefSheriffCanKillFriendRoles, ChiefSheriffCanKillLovers),
        ChiefCanSeeCreatedSheriff
    )];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
    public override RoleId[] RelatedRoleIds { get; } = [RoleId.Sheriff];

    [CustomOptionFloat("ChiefAppointCooldown", 0f, 60f, 2.5f, 30f)]
    public static float ChiefAppointCooldown;

    [CustomOptionBool("ChiefCanSeeCreatedSheriff", false)]
    public static bool ChiefCanSeeCreatedSheriff;

    [CustomOptionFloat("ChiefSheriffKillCooldown", 0f, 60f, 2.5f, 25f)]
    public static float ChiefSheriffKillCooldown;

    [CustomOptionInt("ChiefSheriffMaxKillCount", 1, 10, 1, 1)]
    public static int ChiefSheriffMaxKillCount;

    [CustomOptionBool("ChiefSheriffCanKillImpostor", true)]
    public static bool ChiefSheriffCanKillImpostor;

    [CustomOptionBool("ChiefSheriffCanKillMadRoles", true)]
    public static bool ChiefSheriffCanKillMadRoles;

    [CustomOptionBool("ChiefSheriffCanKillNeutral", true)]
    public static bool ChiefSheriffCanKillNeutral;

    [CustomOptionBool("ChiefSheriffCanKillFriendRoles", true)]
    public static bool ChiefSheriffCanKillFriendRoles;

    [CustomOptionBool("ChiefSheriffCanKillLovers", true)]
    public static bool ChiefSheriffCanKillLovers;
}

public class ChiefAbility : AbilityBase
{
    private CustomSidekickButtonAbility _sidekickButton;
    private bool _canAppointSheriff = true;
    private SheriffAbilityData _sheriffAbilityData;
    private SheriffAbility _createdSheriff = null;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEventListener;
    private bool _canSeeCreatedSheriff;
    public ChiefAbility(SheriffAbilityData sheriffAbilityData, bool canSeeCreatedSheriff)
    {
        _sheriffAbilityData = sheriffAbilityData;
        _canSeeCreatedSheriff = canSeeCreatedSheriff;
    }
    private bool _hasOldTask = false;
    public override void AttachToAlls()
    {
        // 任命ボタンの作成
        _sidekickButton = new CustomSidekickButtonAbility(new(
            canCreateSidekick: created => _canAppointSheriff && !created,
            sidekickCooldown: () => Chief.ChiefAppointCooldown, // クールダウンを設定値に変更
            sidekickRole: () => RoleId.Sheriff,
            sidekickRoleVanilla: () => RoleTypes.Crewmate,
            sidekickSprite: AssetManager.GetAsset<Sprite>("ChiefSidekickButton.png"), // 既存のスプライトを利用
            sidekickText: ModTranslation.GetString("ChiefAppoint"),
            sidekickCount: () => 1,
            isTargetable: IsTargetable,
            sidekickSuccess: target =>
            {
                _hasOldTask = target.IsTaskTriggerRole();
                return !target.IsImpostor();
            },
            onSidekickCreated: OnSheriffAppointed
        ));

        ExPlayerControl exPlayer = Player;
        AbilityParentAbility abilityParentAbility = new(this);
        exPlayer.AttachAbility(_sidekickButton, abilityParentAbility);

        _nameTextUpdateEventListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    public override void AttachToLocalPlayer()
    {
    }
    public override void DetachToLocalPlayer()
    {
        NameTextUpdateEvent.Instance.RemoveListener(_nameTextUpdateEventListener);
    }

    // 対象を任命可能かどうかの判定
    private bool IsTargetable(ExPlayerControl target)
    {
        // 死亡者は対象外
        if (target.IsDead())
            return false;

        // 自分自身は対象外
        if (target.PlayerId == Player.PlayerId)
            return false;

        return true;
    }

    // シェリフ任命後の処理
    private void OnSheriffAppointed(ExPlayerControl target)
    {
        _canAppointSheriff = false;

        // インポスター判定
        if (target.IsImpostor())
        {
            // インポスターを任命した場合は自身が死亡
            new LateTask(() =>
            {
                ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
            }, 0f);
        }
        else
        {
            SheriffAbility sheriffAbility = target.PlayerAbilities.FirstOrDefault(ability => ability is SheriffAbility) as SheriffAbility;
            if (sheriffAbility == null)
                throw new Exception("SheriffAbilityが見つかりません");
            _createdSheriff = sheriffAbility;
            RpcChiefAppointSheriff(target, sheriffAbility, _sheriffAbilityData.KillCooldown, _sheriffAbilityData.KillCount, _sheriffAbilityData.CanKillNeutral, _sheriffAbilityData.CanKillImpostor, _sheriffAbilityData.CanKillMadRoles, _sheriffAbilityData.CanKillFriendRoles, _sheriffAbilityData.CanKillLovers, _hasOldTask);
        }
    }
    [CustomRPC]
    public static void RpcChiefAppointSheriff(ExPlayerControl target, SheriffAbility sheriffAbility, float killCooldown, int maxKillCount, bool canKillNeutral, bool canKillImpostor, bool canKillMadRoles, bool canKillFriendRoles, bool canKillLovers, bool isOldHasTak)
    {
        sheriffAbility.SheriffAbilityData = new(killCooldown, maxKillCount, canKillNeutral, canKillImpostor, canKillMadRoles, canKillFriendRoles, canKillLovers);
        if (sheriffAbility.Player.AmOwner)
            sheriffAbility.ResetTimer();
        sheriffAbility.Count = maxKillCount;
        if (!isOldHasTak)
        {
            CustomTaskAbility customTaskAbility = new(() => (false, false, 0));
            target.AttachAbility(customTaskAbility, new AbilityParentAbility(sheriffAbility));
        }
        NameText.UpdateAllNameInfo();
    }

    // 作成したシェリフを表示するための処理を追加
    public void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (_createdSheriff?.Player != null &&
            _canSeeCreatedSheriff &&
            data.Player.PlayerId == _createdSheriff.Player.PlayerId &&
            _createdSheriff.Player.IsAlive())
            NameText.SetNameTextColor(_createdSheriff.Player, Sheriff.Instance.RoleColor);
    }
}