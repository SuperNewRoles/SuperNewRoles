using System;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Ability;

public record OwlData(
    float KillCooldown,
    bool CanUseVent,
    int CanSpecialBlackoutDeadBodyCount,
    float SpecialBlackoutCool,
    float SpecialBlackoutTime,
    bool CanKillOutsideOfBlackout,
    bool CanTransportOutsideOfBlackout
);

public class OwlAbility : AbilityBase
{
    private readonly OwlData _data;
    private Vent _nestVent;
    private bool _hasNestVent;
    private int _deadBodyCount;

    private CustomKillButtonAbility _customKillButtonAbility;
    private CustomVentAbility _customVentAbility;
    private ImpostorVisionAbility _impostorVisionAbility;
    private OwlNestAbility _nestAbility;
    private OwlDeadBodyTransportAbility _deadBodyTransportAbility;
    private OwlSpecialBlackoutAbility _specialBlackoutAbility;

    public OwlAbility(OwlData data)
    {
        _data = data;
        _hasNestVent = false;
        _deadBodyCount = 0;
    }

    public override void AttachToAlls()
    {
        _customKillButtonAbility = new(
            () => _hasNestVent,
            () => _data.KillCooldown,
            () => false,
            isTargetable: target => _data.CanKillOutsideOfBlackout || ModHelpers.IsElectrical()
        );
        _customVentAbility = new(
            () => _data.CanUseVent
        );
        _impostorVisionAbility = new(
            ModHelpers.IsElectrical
        );
        _nestAbility = new(
            vent => { if (!_hasNestVent) RpcSetNestVent(this, vent.Id); }
        );
        _deadBodyTransportAbility = new(
            () => _nestVent,
            () => RpcSetDeadBodyCount(this, _deadBodyCount + 1),
            () => _data.CanTransportOutsideOfBlackout || ModHelpers.IsElectrical()
        );
        _specialBlackoutAbility = new(
            _data.SpecialBlackoutCool,
            _data.SpecialBlackoutTime,
            () => _hasNestVent && _deadBodyCount >= _data.CanSpecialBlackoutDeadBodyCount
        );

        Player.AddAbility(_customKillButtonAbility, new AbilityParentAbility(this));
        Player.AddAbility(_customVentAbility, new AbilityParentAbility(this));
        Player.AddAbility(_impostorVisionAbility, new AbilityParentAbility(this));
        Player.AddAbility(_nestAbility, new AbilityParentAbility(this));
        Player.AddAbility(_deadBodyTransportAbility, new AbilityParentAbility(this));
        Player.AddAbility(_specialBlackoutAbility, new AbilityParentAbility(this));
    }

    [CustomRPC]
    public static void RpcSetNestVent(OwlAbility ability, int value)
    {
        ability._nestVent = Array.Find<Vent>(ShipStatus.Instance.AllVents, x => x.Id == value);
        ability._hasNestVent = true;
    }

    [CustomRPC]
    public static void RpcSetDeadBodyCount(OwlAbility ability, int value) => ability._deadBodyCount = value;
}

public class OwlNestAbility : VentTargetCustomButtonBase, IAbilityCount
{
    public override float DefaultTimer => 0f;
    public override string buttonText => ModTranslation.GetString("OwlNestButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("OwlNestBuildingButton.png");
    protected override KeyType keytype => KeyType.Ability1;
    public override Color32 OutlineColor => ExPlayerControl.LocalPlayer.roleBase.RoleColor;
    public override bool IsFirstCooldownTenSeconds => false;

    private Action<Vent> _onNestBuilding;

    public OwlNestAbility(Action<Vent> onNestBuilding)
    {
        _onNestBuilding = onNestBuilding;
        Count = 1;
    }

    public override bool CheckIsAvailable() => ExPlayerControl.LocalPlayer.IsAlive() && TargetIsExist;

    public override bool CheckHasButton() => base.CheckHasButton() && HasCount;

    public override void OnClick()
    {
        if (!TargetIsExist) return;
        _onNestBuilding(Target);
        this.UseAbilityCount();
    }
}

public class OwlDeadBodyTransportAbility : CustomButtonBase, IButtonEffect
{
    public override float DefaultTimer => 0;
    public override string buttonText => DeadBodyInTransport != null && CanHideDeadBody() ? ModTranslation.GetString("OwlHideDeadBodyButton") : ModTranslation.GetString("OwlTransportButton");
    public override Sprite Sprite => DeadBodyInTransport != null && CanHideDeadBody() ? _hideDeadBodySprite : _transportSprite;
    protected override KeyType keytype => KeyType.Ability1;
    public override bool IsFirstCooldownTenSeconds => false;
    bool IButtonEffect.isEffectActive { get; set; }
    Action IButtonEffect.OnEffectEnds => OnEffectEnds;
    float IButtonEffect.EffectDuration => 0;
    float IButtonEffect.EffectTimer { get; set; }
    bool IButtonEffect.IsEffectDurationInfinity => true;
    bool IButtonEffect.effectCancellable => true;

    private readonly Sprite _transportSprite = AssetManager.GetAsset<Sprite>("OwlTransportButton.png");
    private readonly Sprite _hideDeadBodySprite = AssetManager.GetAsset<Sprite>("OwlHideDeadBodyButton.png");

    private readonly Func<Vent> _getNestVent;
    private Vent _nestVent = null;
    private bool _nestBuilt = false;
    private readonly Action _deadBodyCountIncrement;
    private readonly Func<bool> _canUse;

    private DeadBody _candidateTarget;
    public DeadBody DeadBodyInTransport { get; private set; }

    private EventListener _fixedUpdateListener;

    public OwlDeadBodyTransportAbility(Func<Vent> nestVent, Action deadBodyCountIncrement, Func<bool> canUse)
    {
        _getNestVent = nestVent;
        _deadBodyCountIncrement = deadBodyCountIncrement;
        _canUse = canUse;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _fixedUpdateListener?.RemoveListener();
    }

    public override bool CheckIsAvailable()
    {
        if (!Player.Player.CanMove) return false;
        if (DeadBodyInTransport == null)
        {
            _candidateTarget = GetDeadBody();
            return _candidateTarget != null && _canUse();
        }
        else return true;
    }

    public override bool CheckHasButton()
    {
        if (!_nestBuilt)
        {
            _nestVent = _getNestVent();
            if (_nestVent == null) return false;
            _nestBuilt = true;
        }
        return base.CheckHasButton() && _canUse();
    }

    public override void OnClick()
    {
        if (_candidateTarget == null) return;
        RpcDeadBodyInTransport(this, _candidateTarget.ParentId);
    }

    public override void OnMeetingEnds() => RpcDeadBodyInTransport(this);

    void IButtonEffect.DoEffect(ActionButton actionButton, float effectStartTime) { }

    private void OnEffectEnds()
    {
        DeadBody body = DeadBodyInTransport;
        RpcDeadBodyInTransport(this);
        if (CanHideDeadBody())
        {
            RpcHideDeadBody(body.ParentId);
            _deadBodyCountIncrement();
        }
    }

    private void OnFixedUpdate()
    {
        if (DeadBodyInTransport != null)
        {
            // 死体を自分の位置に移動
            DeadBodyInTransport.transform.position = Player.transform.position;
        }
    }

    private DeadBody GetDeadBody()
    {
        Vector3 position = Player.transform.position;
        float minDistance = float.MaxValue;
        DeadBody result = null;
        foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
        {
            // 既に誰かが運んでいる死体はスキップ
            if (ExPlayerControl.ExPlayerControls.Any(x =>
                (x.Role == RoleId.Matryoshka && x.GetAbility<MatryoshkaAbility>()?.currentWearingBody == body) ||
                (x.Role == RoleId.Owl && x.GetAbility<OwlDeadBodyTransportAbility>()?.DeadBodyInTransport == body)
            )) continue;

            float distance = Vector2.Distance(position, body.transform.position);
            if (distance <= 2f && distance < minDistance)
            {
                minDistance = distance;
                result = body;
            }
        }
        return result;
    }

    private bool CanHideDeadBody()
    {
        if (!_nestBuilt) return false;
        Vector2 center = Player.Player.Collider.bounds.center;
        Vector3 position = _nestVent.transform.position;
        if (Vector2.Distance(center, position) > _nestVent.UsableDistance) return false;
        if (!PhysicsHelpers.AnythingBetween(Player.Player.Collider, center, position, Constants.ShipOnlyMask, false))
            return true;
        return false;
    }

    [CustomRPC]
    public static void RpcDeadBodyInTransport(OwlDeadBodyTransportAbility ability, byte id = byte.MaxValue)
    {
        if (id == byte.MaxValue)
        {
            ability.DeadBodyInTransport = null;
            return;
        }
        foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body.ParentId == id)
            {
                ability.DeadBodyInTransport = body;
                break;
            }
        }
    }

    [CustomRPC]
    public static void RpcHideDeadBody(byte id)
    {
        foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body.ParentId == id)
            {
                body.transform.position = new(9999f, 9999f);
                break;
            }
        }
    }
}

public class OwlSpecialBlackoutAbility : CustomButtonBase, IButtonEffect
{
    public override float DefaultTimer => _cooldown;
    public override string buttonText => ModTranslation.GetString("OwlSpecialBlackoutButton");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("OwlSpecialBlackoutButton.png");
    protected override KeyType keytype => KeyType.Ability2;
    bool IButtonEffect.isEffectActive { get; set; }
    Action IButtonEffect.OnEffectEnds => OnEffectEnds;
    float IButtonEffect.EffectDuration => _effectDuration;
    float IButtonEffect.EffectTimer { get; set; }

    private readonly float _cooldown;
    private readonly float _effectDuration;
    private Func<bool> _canUse;
    public bool IsSpecialBlackout;
    private float _contractionTimer;

    private EventListener<MeetingStartEventData> _meetingStartListener;
    private EventListener _fixedUpdateListener;
    private EventListener<ShipStatusLightEventData> _shipStatusLightListener;

    public OwlSpecialBlackoutAbility(float cooldown, float effectDuration, Func<bool> canUse)
    {
        _cooldown = cooldown;
        _effectDuration = effectDuration;
        _canUse = canUse;
        IsSpecialBlackout = false;
        _contractionTimer = 1;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
    }

    public override void AttachToOthers()
    {
        base.AttachToOthers();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _shipStatusLightListener = ShipStatusLightEvent.Instance.AddListener(OnShipStatusLight);
    }

    public override void DetachToOthers()
    {
        base.DetachToOthers();
        _fixedUpdateListener?.RemoveListener();
        _shipStatusLightListener?.RemoveListener();
    }

    public override bool CheckIsAvailable() => true;

    public override bool CheckHasButton() => base.CheckHasButton() && _canUse();

    public override void OnClick() => RpcSpecialBlackout(this, true);

    private void OnEffectEnds() => RpcSpecialBlackout(this, false);

    private void OnMeetingStart(MeetingStartEventData data) => OnEffectEnds();

    private void OnFixedUpdate()
    {
        if (IsSpecialBlackout ? _contractionTimer <= 0 : _contractionTimer >= 1) return;
        _contractionTimer = Mathf.Clamp01(_contractionTimer + (Time.fixedDeltaTime * 2 * (IsSpecialBlackout ? -1 : 1)));
    }

    private void OnShipStatusLight(ShipStatusLightEventData data)
    {
        ShipStatus status = ShipStatus.Instance;
        if (status == null) return;
        if (data.player == null || data.player.IsDead) return;
        if (((ExPlayerControl)data.player).HasImpostorVision()) return;
        if (!IsSpecialBlackout && _contractionTimer >= 1) return;
        data.lightRadius = LightPatch.GetNeutralLightRadius(status, false, _contractionTimer);
    }

    [CustomRPC]
    public static void RpcSpecialBlackout(OwlSpecialBlackoutAbility ability, bool isBlackout)
    {
        ability.IsSpecialBlackout = isBlackout;
        ability._contractionTimer = isBlackout ? 1 : 0;
    }
}