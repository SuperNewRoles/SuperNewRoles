using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

public class PhosphorusPutAbility : CustomButtonBase, IAbilityCount
{
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("PhosphorusPutButton.png");
    public override string buttonText => ModTranslation.GetString("PhosphorusPutButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolTime;
    private float coolTime;

    private EventListener<WrapUpEventData> _wrapUpEvent;

    public override ShowTextType showTextType => ShowTextType.ShowWithCount;

    public PhosphorusPutAbility(float coolTime, int maxUseCount)
    {
        this.coolTime = coolTime;
        Count = maxUseCount;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _wrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _wrapUpEvent?.RemoveListener();
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        RpcActivateLanterns(ExPlayerControl.LocalPlayer.PlayerId);
    }

    public override void OnClick()
    {
        if (!HasCount) return;
        this.UseAbilityCount();

        // RPCで他のプレイヤーにも通知
        RpcPutLantern(ExPlayerControl.LocalPlayer.PlayerId, ExPlayerControl.LocalPlayer.GetTruePosition());
    }

    public override bool CheckIsAvailable()
    {
        return ExPlayerControl.LocalPlayer.IsAlive() && HasCount;
    }

    [CustomRPC]
    public static void RpcPutLantern(byte playerId, Vector2 position)
    {
        var player = ExPlayerControl.ById(playerId);
        if (player == null) return;

        var lantern = new GameObject("Lantern").AddComponent<Lantern>();
        lantern.transform.position = position;
        lantern.Init(player);
    }

    [CustomRPC]
    public static void RpcActivateLanterns(byte playerId)
    {
        var player = ExPlayerControl.ById(playerId);
        if (player == null) return;

        var lanterns = Lantern.GetLanternsByOwner(player);
        foreach (var lantern in lanterns)
        {
            lantern.Activate();
        }
    }
}