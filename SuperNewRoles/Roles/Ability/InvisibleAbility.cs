using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Modules.Events;
using SuperNewRoles.Events.PCEvents;
using HarmonyLib;

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
    private readonly OpacityFadeController _opacityFader = new();

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

    // バニラの PlayerControl.SetHatAndVisorAlpha がドア開閉などで任意に呼ばれ、
    // ハット・バイザーのアルファ値を上書きして点滅して見えるバグの対策。
    // 現在透明化中のプレイヤーを登録しておき、上書きされた直後に再適用する。
    private static readonly Dictionary<byte, InvisibleAbility> _invisiblePlayers = new();

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
        _opacityFader.StopAll();
        _invisiblePlayers.Remove(Player.PlayerId);
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
            _opacityFader.Apply(Player, CanSeeTranslucentState(Player) ? 0.4f : 0f, forceSnap: true);
    }

    public void SetInvisible(ExPlayerControl player, bool isInvisible)
    {
        if (isInvisible)
        {
            _invisiblePlayers[player.PlayerId] = this;
            _opacityFader.Apply(player, CanSeeTranslucentState(player) ? 0.4f : 0f);
        }
        else
        {
            _invisiblePlayers.Remove(player.PlayerId);
            _opacityFader.Apply(player, 1f);
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

    // バニラの SetHatAndVisorAlpha がドア開閉・ラダー使用などで呼ばれた直後に
    // 透明化中のプレイヤーの不透明度を再適用し、点滅して見えるバグを防ぐ。
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetHatAndVisorAlpha))]
    public static class SetHatAndVisorAlphaPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (__instance == null) return;
            if (!_invisiblePlayers.TryGetValue(__instance.PlayerId, out var ability)) return;

            ExPlayerControl target = __instance;
            if (target == null || target.IsDead())
            {
                _invisiblePlayers.Remove(__instance.PlayerId);
                return;
            }

            float opacity = ability.CanSeeTranslucentState(target) ? 0.4f : 0f;
            ModHelpers.SetOpacity(__instance, opacity);
        }
    }

    // ゲーム開始時に静的辞書をリセットする。
    // static フィールドはゲームをまたいで持続するため、前のセッションの
    // エントリが次のゲームで同じ PlayerId の別プレイヤーに誤適用されるのを防ぐ。
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class CoStartGamePatch
    {
        public static void Postfix()
        {
            _invisiblePlayers.Clear();
        }
    }
}
