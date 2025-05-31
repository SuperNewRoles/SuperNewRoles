using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.CrewMate;

class Pteranodon : RoleBase<Pteranodon>
{
    public override RoleId Role { get; } = RoleId.Pteranodon;
    public override Color32 RoleColor { get; } = new Color32(17, 128, 45, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new PteranodonAbility(new PteranodonData(
            coolTime: PteranodonCoolTime
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
    public override RoleId[] RelatedRoleIds { get; } = [];
    public override MapNames[] AvailableMaps { get; } = [MapNames.Airship];

    [CustomOptionFloat("PteranodonCoolTime", 0, 120f, 2.5f, 5f, translationName: "CoolTime")]
    public static float PteranodonCoolTime;
}

public class PteranodonAbility : CustomButtonBase
{
    private readonly PteranodonData _data;
    private EventListener<WrapUpEventData> _wrapUpListener;
    private EventListener _fixedUpdateListener;

    // 飛行状態の管理
    private bool _isFlyingNow;
    private Vector2 _startPosition;
    private Vector2 _targetPosition;
    private Vector2 _currentPosition;
    private float _timer;
    private const float StartTime = 2f;
    private FollowerCamera _camera;

    public PteranodonAbility(PteranodonData data)
    {
        _data = data;
    }

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("PteranodonButton.png");
    public override string buttonText => ModTranslation.GetString("Pteranodon.ButtonTitle");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => _data.CoolTime;

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        _isFlyingNow = false;
        _startPosition = new Vector2();
        _targetPosition = new Vector2();
        _timer = 0;
        _camera = Camera.main.GetComponent<FollowerCamera>();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        // 会議時には飛行状態を解除
        _isFlyingNow = false;
        if (PlayerControl.LocalPlayer != null)
        {
            PlayerControl.LocalPlayer.Collider.enabled = true;
        }
    }

    public override void DetachToLocalPlayer()
    {
        // イベントリスナーを削除
        _wrapUpListener?.RemoveListener();
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _fixedUpdateListener?.RemoveListener();
    }

    public override void OnClick()
    {
        // 飛行機能の実装
        AirshipStatus status = ShipStatus.Instance.TryCast<AirshipStatus>();
        if (status == null) return;

        // 現在位置と目標位置を設定
        _startPosition = PlayerControl.LocalPlayer.transform.position;
        bool isRight = Vector3.Distance(status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.LeftUsePosition), PlayerControl.LocalPlayer.transform.position) <= 0.9f;
        Vector3 targetPosition;
        if (isRight)
            targetPosition = status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.RightUsePosition);
        else
            targetPosition = status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.LeftUsePosition);

        _targetPosition = targetPosition;
        _currentPosition = _startPosition;
        _timer = StartTime;
        _isFlyingNow = true;

        // プレイヤーの移動を無効化
        PlayerControl.LocalPlayer.Collider.enabled = false;
        PlayerControl.LocalPlayer.moveable = false;

        SetStatusRPC(this, true, _targetPosition, _startPosition, StartTime);
    }

    public override bool CheckIsAvailable()
    {
        if (!PlayerControl.LocalPlayer.CanMove) return false;
        AirshipStatus status = ShipStatus.Instance.TryCast<AirshipStatus>();
        if (status == null)
            return false;
        if (status.GapPlatform.Target != null && status.GapPlatform.Target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            return false;
        return Vector3.Distance(status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.LeftUsePosition), PlayerControl.LocalPlayer.transform.position) <= 0.9f || Vector3.Distance(status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.RightUsePosition), PlayerControl.LocalPlayer.transform.position) <= 0.9f;
    }

    public void OnFixedUpdate()
    {
        // 自分自身の飛行の処理
        if (!_isFlyingNow) return;

        _timer -= Time.fixedDeltaTime;
        Vector3 pos = _currentPosition;
        Player.MyPhysics.Animations.PlayIdleAnimation();
        Player.cosmetics.AnimateSkinIdle();
        float tarpos = (_targetPosition.x - _startPosition.x);

        if ((_timer + 0.025f) > (StartTime / 2f))
        {
            pos.y += (((_timer + 0.025f) - (StartTime / 2)) * 4f) * Time.fixedDeltaTime;
        }
        else
        {
            pos.y -= (((StartTime / 2) - (_timer + 0.025f)) * 4f) * Time.fixedDeltaTime;
        }

        pos.x += tarpos * Time.fixedDeltaTime * 0.5f;
        Player.MyPhysics.body.velocity = Vector2.zero;

        if (_timer <= 0)
        {
            _isFlyingNow = false;
            Player.Player.Collider.enabled = true;
            Player.Player.moveable = true;

            Vector3 position = Player.Player.transform.position;

            if (Player.AmOwner)
            {
                SetStatusRPC(this, false, position, _startPosition, 0f);
                Player.NetTransform.RpcSnapTo(position);
            }
            else
                Player.NetTransform.SnapTo(position);
            return;
        }

        _currentPosition = pos;
        Player.transform.position = pos;

        if (Player.AmOwner && _camera != null)
        {
            _camera.transform.position = Vector3.Lerp(_camera.centerPosition, (Vector2)_camera.Target.transform.position + _camera.Offset, 5f * Time.deltaTime);
        }

    }

    [CustomRPC]
    public static void SetStatusRPC(PteranodonAbility ability, bool status, Vector3 targetPosition, Vector3 startPosition, float timer)
    {
        // プレイヤーの飛行状態を設定
        if (status)
        {

            ability._targetPosition = targetPosition;
            ability._currentPosition = ability._startPosition = startPosition;
            ability._timer = timer;
            ability._isFlyingNow = true;

            ability.Player.Player.NetTransform.enabled = false;
            ability.Player.Player.Collider.enabled = false;
            ability.Player.Player.moveable = false;
        }
        else
        {
            ability.Player.Player.NetTransform.enabled = true;
            ability.Player.Player.Collider.enabled = true;
            ability.Player.Player.moveable = true;
            ability._isFlyingNow = false;
        }
    }
}

public class PteranodonData
{
    public float CoolTime { get; }

    public PteranodonData(float coolTime)
    {
        CoolTime = coolTime;
    }
}