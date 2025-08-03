using System;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Events.PCEvents;

namespace SuperNewRoles.Roles.Ability;

public class InvisibleAbility : CustomButtonBase, IButtonEffect
{
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>(SpriteName);
    public override string buttonText => ModTranslation.GetString("ScientistButtonName");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => coolTime;
    private float coolTime;
    private float durationTime;
    private bool canLighterSeeScientist;
    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    private EventListener<DieEventData> _onDie;
    private EventListener _onFixedUpdate;

    public string SpriteName { get; }
    public bool isEffectActive { get; set; }

    public Action OnEffectEnds => () =>
    {
        RpcSetInvisible(ExPlayerControl.LocalPlayer, false);
    };

    public float EffectDuration => durationTime;
    public bool effectCancellable => true;

    public float EffectTimer { get; set; }

    private bool invisible;

    public InvisibleAbility(float coolTime, float durationTime, bool canLighterSeeScientist, string sprite)
    {
        this.coolTime = coolTime;
        this.durationTime = durationTime;
        this.canLighterSeeScientist = canLighterSeeScientist;
        SpriteName = sprite;
    }

    public override void OnClick()
    {
        if (!CheckIsAvailable()) return;
        RpcSetInvisible(ExPlayerControl.LocalPlayer, true);
    }

    public override bool CheckIsAvailable()
    {
        return ExPlayerControl.LocalPlayer.IsAlive();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _onFixedUpdate = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _onFixedUpdate?.RemoveListener();
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

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (isEffectActive)
        {
            RpcSetInvisible(ExPlayerControl.LocalPlayer, false);
        }
    }

    private void OnDie(DieEventData data)
    {
        if (data.player == Player && isEffectActive)
            RpcSetInvisible(ExPlayerControl.LocalPlayer, false);
    }

    [CustomRPC]
    public void RpcSetInvisible(ExPlayerControl player, bool isInvisible)
    {
        SetInvisible(player, isInvisible);
        invisible = isInvisible;
    }

    private void OnFixedUpdate()
    {
        if (invisible)
            SetInvisible(Player, true);
    }

    public void SetInvisible(ExPlayerControl player, bool isInvisible)
    {
        if (isInvisible)
        {
            ModHelpers.SetOpacity(player.Player, CanSeeTranslucentState(player) ? 0.4f : 0f);
        }
        else
        {
            ModHelpers.SetOpacity(player.Player, 1f);
        }
    }

    public bool CanSeeTranslucentState(ExPlayerControl invisibleTarget)
    {
        if (invisibleTarget == ExPlayerControl.LocalPlayer)
            return true;

        if (canLighterSeeScientist && ExPlayerControl.LocalPlayer.TryGetAbility<LighterAbility>(out var lighterAbility) && lighterAbility.isEffectActive)
            return true;

        // インポスター同士で見える場合（EvilScientistの場合）
        if (invisibleTarget.IsImpostor() && ExPlayerControl.LocalPlayer.IsImpostor())
            return true;

        if (ExPlayerControl.LocalPlayer.IsDead())
            return true;

        return false;
    }
}