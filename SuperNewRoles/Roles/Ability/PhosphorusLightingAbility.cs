using System;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomObject;

namespace SuperNewRoles.Roles.Ability;

public class PhosphorusLightingAbility : CustomButtonBase, IButtonEffect
{
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("PhosphorusLightingButton.png");
    public override string buttonText => ModTranslation.GetString("PhosphorusLightingButtonName");
    protected override KeyType keytype => KeyType.Ability2;
    public override float DefaultTimer => coolTime;
    private float coolTime;
    private float durationTime;
    private float lightRange;

    public bool isEffectActive { get; set; }
    public Action OnEffectEnds => () =>
    {
        // エフェクト終了時にランタンを消灯
        RpcLightingOff(ExPlayerControl.LocalPlayer.PlayerId);
    };

    public float EffectDuration => durationTime;
    public bool effectCancellable => true;
    public float EffectTimer { get; set; }

    public PhosphorusLightingAbility(float coolTime, float durationTime, float lightRange)
    {
        this.coolTime = coolTime;
        this.durationTime = durationTime;
        this.lightRange = lightRange;
    }

    public override void OnClick()
    {
        // アクティブなランタンがあるかチェック
        var activeLanterns = Lantern.GetLanternsByOwner(ExPlayerControl.LocalPlayer)
            .Where(x => x.IsActivating).ToList();

        if (activeLanterns.Count == 0) return;

        // ランタンを点灯
        RpcLightingOn(ExPlayerControl.LocalPlayer.PlayerId);
    }

    public override bool CheckIsAvailable()
    {
        if (!ExPlayerControl.LocalPlayer.IsAlive()) return false;

        // アクティブなランタンがあるかチェック
        var activeLanterns = Lantern.GetLanternsByOwner(ExPlayerControl.LocalPlayer)
            .Where(x => x.IsActivating).ToList();

        return activeLanterns.Count > 0;
    }

    public override void OnMeetingEnds()
    {
        if (isEffectActive)
        {
            // 会議で効果が中断された時も、見た目だけ点灯したまま残らないよう必ず消灯する
            RpcLightingOff(Player.PlayerId);
        }

        base.OnMeetingEnds();
    }

    [CustomRPC]
    public static void RpcLightingOn(byte playerId)
    {
        var player = ExPlayerControl.ById(playerId);
        if (player == null) return;

        var lanterns = Lantern.GetLanternsByOwner(player);
        foreach (var lantern in lanterns)
        {
            lantern.LightingOn();
        }
    }

    [CustomRPC]
    public static void RpcLightingOff(byte playerId)
    {
        var player = ExPlayerControl.ById(playerId);
        if (player == null) return;

        var lanterns = Lantern.GetLanternsByOwner(player);
        foreach (var lantern in lanterns)
        {
            lantern.LightingOff();
        }
    }
}
