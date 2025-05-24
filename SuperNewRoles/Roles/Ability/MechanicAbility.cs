using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Events.PCEvents;

namespace SuperNewRoles.Roles.Ability;

public class MechanicAbility : VentTargetCustomButtonBase, IAbilityCount, IButtonEffect
{
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>(SpriteName);
    public override string buttonText => ModTranslation.GetString("MechanicButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolTime;
    public override Color32 OutlineColor => new(82, 108, 173, byte.MaxValue);

    private float coolTime;
    private float durationTime;
    private Vent currentVent;
    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    private EventListener<PlayerPhysicsFixedUpdateEventData> _onPlayerPhysicsFixedUpdateEvent;
    private EventListener<DieEventData> _onDie;

    public string SpriteName { get; }
    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () =>
    {
        if (currentVent != null)
        {
            RpcSetVentStatus(ExPlayerControl.LocalPlayer, currentVent, false, moveableVentPosition);
            currentVent = null;
        }
    };

    public float EffectDuration => durationTime;
    public bool effectCancellable => true;

    public float EffectTimer { get; set; }
    private Vector3 moveableVentPosition;

    public MechanicAbility(float coolTime, float durationTime, string sprite)
    {
        this.coolTime = coolTime;
        this.durationTime = durationTime;
        SpriteName = sprite;
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (!CheckIsAvailable()) return;

        Vector3 originalPos = Target.transform.position;
        RpcSetVentStatus(ExPlayerControl.LocalPlayer, Target, true, originalPos);
        currentVent = Target;
    }

    public override bool CheckIsAvailable()
    {
        return Target != null;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _onMeetingStartEvent = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _onDie = DieEvent.Instance.AddListener(OnDie);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _onMeetingStartEvent?.RemoveListener();
        _onDie?.RemoveListener();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _onPlayerPhysicsFixedUpdateEvent = PlayerPhysicsFixedUpdateEvent.Instance.AddListener(OnPlayerPhysicsFixedUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _onPlayerPhysicsFixedUpdateEvent?.RemoveListener();
        if (currentVent != null)
            SetVentStatus(Player, currentVent, false, moveableVentPosition);
        ModHelpers.SetOpacity(Player, 1f, false);
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (currentVent != null)
        {
            RpcSetVentStatus(ExPlayerControl.LocalPlayer, currentVent, false, moveableVentPosition);
            currentVent = null;
        }
    }

    private void OnPlayerPhysicsFixedUpdate(PlayerPhysicsFixedUpdateEventData data)
    {
        if (data.Instance.myPlayer != Player) return;
        if (data.Instance.myPlayer.Data.IsDead) return;
        if (currentVent != null)
        {
            // プレイヤーの位置にベントを移動
            PlayerControl player = Player;
            Vector2 truepos = player.GetTruePosition();
            currentVent.transform.position = new(truepos.x, truepos.y, player.transform.position.z + 0.0025f);
            SetHideStatus(player, true);
            if (data.Instance.myPlayer.moveable)
                moveableVentPosition = currentVent.transform.position;
        }
    }

    private void OnDie(DieEventData data)
    {
        if (data.player == Player && currentVent != null)
            RpcSetVentStatus(ExPlayerControl.LocalPlayer, currentVent, false, moveableVentPosition);
    }

    [CustomRPC]
    public void RpcSetVentStatus(ExPlayerControl source, Vent targetvent, bool isUsing, Vector3 originalPos)
    {
        SetVentStatus(source, targetvent, isUsing, originalPos);
    }

    public void SetVentStatus(ExPlayerControl source, Vent targetvent, bool isUsing, Vector3 originalPos)
    {
        if (isUsing)
        {
            currentVent = targetvent;
            Vector2 truepos = source.GetTruePosition();
            targetvent.transform.position = new(truepos.x, truepos.y, source.transform.position.z + 0.0025f);
            SetHideStatus(source, true);
        }
        else
        {
            currentVent = null;
            targetvent.transform.position = originalPos;
            SetHideStatus(source, false);
        }
    }

    public static void SetHideStatus(PlayerControl target, bool isOn)
    {
        var opacity = 0f;
        if (isOn)
        {
            opacity = 0f;
            target.cosmetics.currentBodySprite.BodySprite.material.SetFloat("_Outline", 0f);
        }
        else
        {
            opacity = 1.5f;
        }
        ModHelpers.SetOpacity(target, opacity, false);
    }
}