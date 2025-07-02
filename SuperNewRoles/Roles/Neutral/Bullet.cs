using System;
using System.Collections.Generic;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.WaveCannonObj;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

class Bullet : RoleBase<Bullet>
{
    public override RoleId Role { get; } = RoleId.Bullet;
    public override Color32 RoleColor { get; } = Jackal.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new JSidekickAbility(canUseVent: WaveCannonJackal.WaveCannonJackalCanUseVent),
        () => new BulletAbility(WaveCannonJackal.WaveCannonJackalBulletLoadBulletCooltime, WaveCannonJackal.WaveCannonJackalBulletLoadedChargeTime)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;

    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;

    public override TeamTag TeamTag { get; } = TeamTag.Neutral;

    public override RoleTag[] RoleTags { get; } = [];

    public override short IntroNum { get; } = 1;

    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Hidden;
}

public class BulletAbility : TargetCustomButtonBase
{
    public float CoolDown { get; }
    public float EffectDuration { get; }

    public override Color32 OutlineColor => Bullet.Instance.RoleColor;

    public override bool OnlyCrewmates => false;

    public override float DefaultTimer => CoolDown;

    public override string buttonText => ModTranslation.GetString("BulletButton");

    private EventListener<PlayerPhysicsFixedUpdateEventData> _playerPhysicsFixedUpdateEvent;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("BulletLoadBulletButton.png");

    protected override KeyType keytype => KeyType.Ability1;
    public override Func<ExPlayerControl, bool> IsTargetable => (player) => player == _parent?.Player;

    private AbilityParentRole _parent;
    private WaveCannonAbility _waveCannonAbility;

    private EventListener<NameTextUpdateEventData> _nameTextUpdateEvent;
    private EventListener<MeetingStartEventData> _meetingStartEvent;
    private EventListener<EmergencyCheckEventData> _emergencyCheckEvent;

    public BulletAbility(float coolDown, float effectDuration)
    {
        CoolDown = coolDown;
        EffectDuration = effectDuration;
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        // PromoteOnParentDeathAbilityのアタッチを待つために1F遅延させる
        new LateTask(() =>
        {
            _parent = Player.GetAbility<PromoteOnParentDeathAbility>()?.Owner;
            _waveCannonAbility = _parent.Player.GetAbility<WaveCannonAbility>();
        }, 0f);
        _playerPhysicsFixedUpdateEvent = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(OnPlayerPhysicsFixedUpdate);
        _nameTextUpdateEvent = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _meetingStartEvent = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _emergencyCheckEvent = EmergencyCheckEvent.Instance.AddListener(OnEmergencyCheck);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _playerPhysicsFixedUpdateEvent?.RemoveListener();
        _nameTextUpdateEvent?.RemoveListener();
        _meetingStartEvent?.RemoveListener();
        _emergencyCheckEvent?.RemoveListener();
        if (_waveCannonAbility != null)
            _waveCannonAbility.bullet = null;
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player == Player || data.Player == _parent?.Player)
        {
            if (Player.AmOwner || _parent?.Player?.AmOwner == true)
            {
                NameText.AddNameText(data.Player, ModHelpers.Cs(Bullet.Instance.RoleColor, "☆"), true);
            }
        }

        // 親の位置がわかる設定
        if (WaveCannonJackal.WaveCannonJackalBulletCanSeeParent && data.Player == _parent?.Player && Player.AmOwner)
        {
            NameText.AddNameText(data.Player, ModHelpers.Cs(Color.yellow, "★"), true);
        }

        // 弾の位置がわかる設定
        if (WaveCannonJackal.WaveCannonJackalCanSeeBullet && data.Player == Player && _parent?.Player?.AmOwner == true)
        {
            NameText.AddNameText(data.Player, ModHelpers.Cs(Color.cyan, "●"), true);
        }
    }
    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (_waveCannonAbility?.bullet == Player)
            _waveCannonAbility.bullet = null;
    }
    private void OnEmergencyCheck(EmergencyCheckEventData data)
    {
        if (_waveCannonAbility?.WaveCannonObject != null && _waveCannonAbility.WaveCannonObject is WaveCannonObjectBullet bulletObject)
        {
            data.RefEmergencyTexts.Add(ModTranslation.GetString("BulletEmergencyText"));
            data.RefEnabledEmergency = false;
        }
    }
    private void OnPlayerPhysicsFixedUpdate(PlayerPhysicsFixedUpdateEventData data)
    {
        if (data.Instance.myPlayer.PlayerId != Player.PlayerId) return;
        if (Player.IsDead() || _parent?.Player?.IsDead() == true) return;
        if (_waveCannonAbility?.bullet != null)
        {
            Player.transform.position = _parent.Player.transform.position;
            Player.NetTransform.SnapTo(_parent.Player.transform.position);
            Player.MyPhysics.body.velocity = Vector2.zero;
        }
    }

    public override bool CheckIsAvailable()
    {
        return Target != null && _waveCannonAbility?.Type != WaveCannonType.Bullet;
    }
    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && _waveCannonAbility?.Type != WaveCannonType.Bullet;
    }

    public override void OnClick()
    {
        RpcBullet();
    }
    [CustomRPC]
    public void RpcBullet()
    {
        _waveCannonAbility.bullet = Player;
        _waveCannonAbility.bulletDuration = EffectDuration;
        _waveCannonAbility.Timer = 0.00001f;
    }
}