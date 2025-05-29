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
    public bool IsCustomSoulColor; // イビルシーアかどうかのフラグを追加
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
    // 通常シーア用の固定霊魂カラーID
    private const int DefaultSoulColorId = 0; // 赤色を使用

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
            int colorId;
            if (Data.IsCustomSoulColor)
            {
                // イビルシーアの場合はプレイヤーの色を使用
                colorId = data.player.Data.DefaultOutfit.ColorId;
            }
            else
            {
                // 通常シーアの場合は固定色を使用
                colorId = DefaultSoulColorId;
            }

            deadBodyPositions.Add((data.player.transform.position, colorId));

            // 霊魂を即表示（会議を待たずに表示）
            CreateSoul(data.player.transform.position, colorId);
        }

        // モードが「死の点滅が見える」または「両方」の場合
        if (mode == SeerMode.Both || mode == SeerMode.FlashOnly)
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
            (Vector3 pos, int soulColorId) = posAndColor;
            CreateSoul(pos, soulColorId);
        }

        // 霊魂の位置をリセット
        deadBodyPositions = new();
    }

    // 霊魂を作成する共通メソッド
    private void CreateSoul(Vector3 position, int colorId)
    {
        GameObject soul = new();
        soul.transform.position = position;
        soul.layer = 5;
        var rend = soul.AddComponent<SpriteRenderer>();
        rend.sprite = GetSoulSprite();
        rend.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        rend.maskInteraction = SpriteMaskInteraction.None;
        PlayerMaterial.SetColors(colorId, rend);
        PlayerMaterial.Properties Properties = new()
        {
            MaskLayer = 0,
            MaskType = PlayerMaterial.MaskType.None,
            ColorId = colorId,
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