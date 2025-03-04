using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

public record SeerData
{
    public SeerMode Mode;
    public bool LimitSoulDuration;
    public float SoulDuration;
}
/// <summary>
/// シーア役職の能力クラス
/// </summary>
public class SeerAbility : AbilityBase
{
    private List<(Vector3, int)> deadBodyPositions = new();
    private EventListener<DieEventData> dieEventListener;
    private EventListener<WrapUpEventData> wrapUpEventListener;
    public SeerData Data;
    public SeerAbility(SeerData data)
    {
        Data = data;
    }
    public override void AttachToLocalPlayer()
    {
        dieEventListener = DieEvent.Instance.AddListener(OnPlayerDead);
        wrapUpEventListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    private void OnPlayerDead(DieEventData data)
    {
        if (ExileController.Instance != null) return;
        // モードが「霊魂が見える」または「両方」の場合
        var mode = Data.Mode;
        if (mode == SeerMode.Both || mode == SeerMode.SoulOnly)
        {
            // 死亡位置を記録
            deadBodyPositions.Add((data.player.transform.position, data.player.Data.DefaultOutfit.ColorId));
        }

        // モードが「死の点滅が見える」または「両方」の場合
        if (mode == Data.Mode || mode == SeerMode.FlashOnly)
        {
            // 死亡時に画面を青く光らせる
            FlashHandler.ShowFlash(Seer.Instance.RoleColor);
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        // モードが「霊魂が見える」または「両方」の場合のみ処理
        var mode = Data.Mode;
        if (mode != SeerMode.Both && mode != SeerMode.SoulOnly) return;

        // 霊魂を表示
        foreach ((Vector3, int) posAndColor in deadBodyPositions)
        {
            GameObject soul = new();
            (Vector3 pos, int soulColorId) = posAndColor;
            soul.transform.position = pos;
            soul.layer = 5;
            var rend = soul.AddComponent<SpriteRenderer>();
            rend.sprite = GetSoulSprite();
            rend.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
            rend.maskInteraction = SpriteMaskInteraction.None;
            PlayerMaterial.SetColors(soulColorId, rend);
            PlayerMaterial.Properties Properties = new()
            {
                MaskLayer = 0,
                MaskType = PlayerMaterial.MaskType.None,
                ColorId = soulColorId,
            };
            rend.material.SetInt(PlayerMaterial.MaskLayer, Properties.MaskLayer);

            // 時間経過で霊魂が消える設定の場合
            if (Data.LimitSoulDuration)
            {
                float soulDuration = Data.SoulDuration;
                FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(soulDuration, new Action<float>((p) =>
                {
                    if (rend != null)
                    {
                        var tmp = rend.color;
                        tmp.a = Mathf.Clamp01(1 - p);
                        rend.color = tmp;
                    }
                    if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                })));
            }
        }

        // 霊魂の位置をリセット
        deadBodyPositions = new();
    }

    private static Sprite soulSprite;
    private static Sprite GetSoulSprite()
    {
        if (soulSprite == null)
        {
            soulSprite = AssetManager.GetAsset<Sprite>("Soul.png");
        }
        return soulSprite;
    }

    public override void DetachToLocalPlayer()
    {
        DieEvent.Instance.RemoveListener(dieEventListener);
        WrapUpEvent.Instance.RemoveListener(wrapUpEventListener);
    }
}