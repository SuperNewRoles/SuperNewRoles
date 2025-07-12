using System;
using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ghost;

class Cantera : GhostRoleBase<Cantera>
{
    public override GhostRoleId Role => GhostRoleId.Cantera;

    public override Color32 RoleColor => new(255, 237, 194, byte.MaxValue);

    public override List<Func<AbilityBase>> Abilities => [() => new CanteraAbility(
        new(
            Cooldown: CanteraCooldown,
            Duration: CanteraDuration,
            ChangedVision: CanteraChangedVision,
            IsLimitUses: CanteraIsLimitUses,
            MaxUses: CanteraMaxUses
        )
    )];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;

    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;

    public override TeamTag TeamTag => TeamTag.Crewmate;

    public override RoleTag[] RoleTags => [RoleTag.GhostRole];

    public override short IntroNum => 1;

    [CustomOptionFloat("CanteraCooldown", 2.5f, 60f, 2.5f, 15f, translationName: "CoolTime")]
    public static float CanteraCooldown;

    [CustomOptionFloat("CanteraDuration", 2.5f, 60f, 2.5f, 5f, translationName: "DurationTime")]
    public static float CanteraDuration;

    [CustomOptionFloat("CanteraChangedVision", 0.25f, 3f, 0.25f, 1.5f)]
    public static float CanteraChangedVision;

    [CustomOptionBool("CanteraIsLimitUses", false)]
    public static bool CanteraIsLimitUses;

    [CustomOptionInt("CanteraMaxUses", 1, 15, 1, 3, parentFieldName: nameof(CanteraIsLimitUses))]
    public static int CanteraMaxUses;
}

public record CanteraAbilityOption(float Cooldown, float Duration, float ChangedVision, bool IsLimitUses, int MaxUses);
public class CanteraAbility : TargetCustomButtonBase, IButtonEffect
{
    public override Color32 OutlineColor => Cantera.Instance.RoleColor;

    public override bool OnlyCrewmates => false;

    public override float DefaultTimer => Data.Cooldown;

    public override string buttonText => ModTranslation.GetString("CanteraButtonText");

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("CanteraButton.png");

    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () => RpcCantera(this, null);

    public float EffectDuration => Data.Duration;

    public float EffectTimer { get; set; }

    protected override KeyType keytype => KeyType.Ability1;

    public CanteraAbilityOption Data { get; }

    public ExPlayerControl CurrentTarget { get; set; }

    public override bool IgnoreWalls => true;

    public override ShowTextType showTextType => Data.IsLimitUses ? ShowTextType.ShowWithCount : ShowTextType.Hidden;

    public DisableOtherPlayerInfoAbility disableOtherPlayerAbility { get; set; }
    public CustomHauntToAbility customHauntToAbility;
    public DisibleHauntAbility disibleHauntAbility;

    private EventListener<ShipStatusLightEventData> _shipStatusLightEvent;
    private EventListener<WrapUpEventData> _wrapUpEvent;

    public CanteraAbility(CanteraAbilityOption data)
    {
        Data = data;
        Count = Data.IsLimitUses ? Data.MaxUses : int.MaxValue;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        disableOtherPlayerAbility = new DisableOtherPlayerInfoAbility(() => HasCount);
        customHauntToAbility = new CustomHauntToAbility(() => CurrentTarget);
        disibleHauntAbility = new DisibleHauntAbility(() => HasCount);

        Player.AttachAbility(disableOtherPlayerAbility, new AbilityParentAbility(this));
        Player.AttachAbility(customHauntToAbility, new AbilityParentAbility(this));
        Player.AttachAbility(disibleHauntAbility, new AbilityParentAbility(this));

        _shipStatusLightEvent = ShipStatusLightEvent.Instance.AddListener(OnShipStatusLight);
        _wrapUpEvent = WrapUpEvent.Instance.AddListener((_) => WrapUp());
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _shipStatusLightEvent?.RemoveListener();
        _wrapUpEvent?.RemoveListener();
    }
    public override bool CheckHasButton()
    {
        return Player.AmOwner && HasCount;
    }

    public override bool CheckIsAvailable()
    {
        return Target != null;
    }

    private void OnShipStatusLight(ShipStatusLightEventData data)
    {
        if (CurrentTarget?.AmOwner == true)
            data.lightRadius = Mathf.Lerp(ShipStatus.Instance.MinLightRadius, ShipStatus.Instance.MaxLightRadius, 1) * Data.ChangedVision;
    }

    public override void OnClick()
    {
        if (CurrentTarget == null)
        {
            RpcCantera(this, Target);
        }
    }
    public void WrapUp()
    {
        CurrentTarget = null;
    }
    [CustomRPC]
    public static void RpcCantera(CanteraAbility ability, ExPlayerControl target)
    {
        ability.CurrentTarget = target;
        if (target != null)
            ability.Count--;
    }
}