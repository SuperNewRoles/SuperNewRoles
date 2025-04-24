using System;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
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
    private int _deadBodyCount;

    private CustomKillButtonAbility _customKillButtonAbility;
    private OwlNestAbility _nestAbility;
    private OwlDeadBodyTransportAbility _deadBodyTransportAbility;
    
    public OwlAbility(OwlData data)
    {
        _data = data;
        _deadBodyCount = 0;
    }

    public override void AttachToAlls()
    {
        _customKillButtonAbility = new(
            () => _data.CanKillOutsideOfBlackout || ModHelpers.IsElectrical(),
            () => _data.KillCooldown,
            () => false
        );
        _nestAbility = new(
            vent => { if (_nestVent == null) RpcNestVent(this, vent.Id); }
        );
        _deadBodyTransportAbility = new(
            () => _nestVent,
            () => RpcNestVent(this, _deadBodyCount + 1)
        );

        Player.AddAbility(_customKillButtonAbility, new AbilityParentAbility(this));
        Player.AddAbility(_nestAbility, new AbilityParentAbility(this));
        Player.AddAbility(_deadBodyTransportAbility, new AbilityParentAbility(this));
    }

    [CustomRPC]
    public static void RpcNestVent(OwlAbility ability, int value) => ability._nestVent = Array.Find<Vent>(ShipStatus.Instance.AllVents, x => x.Id == value);

    [CustomRPC]
    public static void RpcDeadBodyCount(OwlAbility ability, int value)
    {
        ability._deadBodyCount = value;
        Logger.Info($"_deadBodyCount : {ability._deadBodyCount}");
    }
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
    Action IButtonEffect.OnEffectEnds => () => DeadBodyInTransport = null;
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

    private DeadBody _candidateTarget;
    public DeadBody DeadBodyInTransport { get; private set; }
    
    private EventListener _fixedUpdateListener;

    public OwlDeadBodyTransportAbility(Func<Vent> nestVent, Action deadBodyCountIncrement)
    {
        _getNestVent = nestVent;
        _deadBodyCountIncrement = deadBodyCountIncrement;
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
            return _candidateTarget != null;
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
        return base.CheckHasButton();
    }

    public override void OnClick()
    {
        if (DeadBodyInTransport == null)
        {
            if (_candidateTarget == null) return;
            RpcDeadBodyInTransport(this, _candidateTarget.ParentId);
        }
        else
        {
            DeadBody body = DeadBodyInTransport;
            RpcDeadBodyInTransport(this, byte.MaxValue);
            Logger.Info($"CanHideDeadBody() : {CanHideDeadBody()}");
            if (CanHideDeadBody())
            {
                RpcHideDeadBody(body.ParentId);
                _deadBodyCountIncrement();
            }
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
    public static void RpcDeadBodyInTransport(OwlDeadBodyTransportAbility ability, byte id)
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