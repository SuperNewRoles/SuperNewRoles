using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Impostor;
internal class Crab : RoleBase<Crab>
{
    public override RoleId Role => RoleId.Crab;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => new() { () => new CrabAbility(CrabCoolTime, CrabEffectDuration) };

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => Array.Empty<RoleTag>();
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("CrabCoolTime", 2.5f, 180f, 2.5f, 20f, translationName: "CoolTime")]
    public static float CrabCoolTime;
    [CustomOptionFloat("CrabEffectDuration", 0f, 30f, 0.5f, 5f, translationName: "DurationTime")]
    public static float CrabEffectDuration;
}

public class CrabAbility : CustomButtonBase, IButtonEffect
{
    // イベント購読用
    private EventListener<PlayerPhysicsFixedUpdateEventData> _physicsUpdateListener;

    private bool IsCrabActive;
    public bool isEffectActive { get; set; }
    public float EffectTimer { get; set; }
    Action IButtonEffect.OnEffectEnds => () => { RpcSetCrab(false); };
    public float EffectDuration => _effectDuration;
    public bool effectCancellable => true;

    public override float DefaultTimer => _coolTime;
    public override string buttonText => ModTranslation.GetString("CrabButton");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("crabButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    private float _coolTime;
    private float _effectDuration;
    public CrabAbility(float coolTime, float effectDuration)
    {
        _coolTime = coolTime;
        _effectDuration = effectDuration;
    }

    public override bool CheckIsAvailable() => Player.IsAlive();
    public override void OnClick() { RpcSetCrab(true); }

    [CustomRPC]
    public void RpcSetCrab(bool isCrabActive)
    {
        IsCrabActive = isCrabActive;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _physicsUpdateListener = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(OnPhysicsFixedUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _physicsUpdateListener?.RemoveListener();
    }

    private void OnPhysicsFixedUpdate(PlayerPhysicsFixedUpdateEventData data)
    {
        if (!data.Instance.AmOwner) return;
        if (Player.IsDead()) return;

        if (MeetingHud.Instance != null)
        {
            IsCrabActive = false;
            return;
        }
        if (IsCrabActive)
        {
            var vel = data.Instance.body.velocity;
            data.Instance.body.velocity = new Vector2(vel.x, 0f);
        }
    }
}