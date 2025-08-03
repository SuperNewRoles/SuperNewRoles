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
    /// <summary>イビルシーアの死の点滅のカラーモード</summary>
    public DeadBodyColorMode FlashColorMode = DeadBodyColorMode.None;
    /// <summary>イビルシーアの霊魂のカラーモード</summary>
    public DeadBodyColorMode SoulColorMode = DeadBodyColorMode.None;
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

    // 通常霊魂カラーID
    private const int DefaultSoulColorId = (int)CustomCosmetics.CustomColors.ColorType.Crasyublue;

    // 明暗表示対応用の霊魂カラーID
    private const int LightSoulColorId = (int)CustomCosmetics.CustomColors.ColorType.Pitchwhite;
    private const int DarknessSoulColorId = (int)CustomCosmetics.CustomColors.ColorType.Crasyublue;

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
        if (mode is SeerMode.Both or SeerMode.SoulOnly)
        {
            // 死亡位置を記録

            var colorId = Data.SoulColorMode switch
            {
                DeadBodyColorMode.None => DefaultSoulColorId, // シーア 或いは EvilSeerで設定が無効な場合
                // イビルシーアで明暗表示が有効, 又は死体色表示が有効でColorIdが不正な場合
                DeadBodyColorMode.LightAndDarkness or DeadBodyColorMode.Adaptive when !CustomCosmetics.CustomColors.IsValidColorId(data.player.Data.DefaultOutfit.ColorId) => CustomCosmetics.CustomColors.IsLighter(data.player)
                                        ? LightSoulColorId // 明るい色を反映
                                        : DarknessSoulColorId, // 暗い色を反映
                DeadBodyColorMode.Adaptive => data.player.Data.DefaultOutfit.ColorId, // イビルシーアで設定が有効な場合は、プレイヤーの色を使用
                _ => DefaultSoulColorId, // その他
            };
            deadBodyPositions.Add((data.player.transform.position, colorId));

            // 霊魂を即表示（会議を待たずに表示）
            CreateSoul(data.player.transform.position, colorId);
        }

        // モードが「死の点滅が見える」または「両方」の場合
        if (mode is SeerMode.Both or SeerMode.FlashOnly)
        {
            // 死亡時に画面を光らせる
            FlashHandler.ShowFlash(FlashColor(data.player));

            if (Data.FlashColorMode is DeadBodyColorMode.LightAndDarkness or DeadBodyColorMode.Adaptive)
            {
                var colorText = FlashColorText(data.player);
                if (colorText != null) new CustomMessage(colorText, 3f);
            }
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

    private Color FlashColor(ExPlayerControl exp)
    {
        Color flashColor;

        // 明暗表示関連
        Color lightColor = Palette.PlayerColors[(int)CustomCosmetics.CustomColors.ColorType.Pitchwhite];
        Color darknessColor = Palette.PlayerColors[(int)CustomCosmetics.CustomColors.ColorType.Crasyublue];

        // 有効なプレイヤーカラーの範囲内か
        var isValidColorId = CustomCosmetics.CustomColors.IsValidColorId(exp.Data.DefaultOutfit.ColorId);

        switch (Data.FlashColorMode)
        {
            case DeadBodyColorMode.LightAndDarkness:
            case DeadBodyColorMode.Adaptive when !isValidColorId: // ボディカラー反映時に 不正なColorIdであれば明暗表示で返す
                flashColor = CustomCosmetics.CustomColors.IsLighter(exp) ? lightColor : darknessColor;
                break;
            case DeadBodyColorMode.Adaptive when isValidColorId: // 有効なColorIdであれば 死体の色を返す
                flashColor = Palette.PlayerColors[exp.Data.DefaultOutfit.ColorId];
                break;
            default:
                flashColor = Seer.Instance.RoleColor;
                break;
        }

        return flashColor;
    }

    private string FlashColorText(ExPlayerControl exp)
    {
        string colorText;

        // 有効なプレイヤーカラーの範囲内か
        var isValidColorId = CustomCosmetics.CustomColors.IsValidColorId(exp.Data.DefaultOutfit.ColorId);

        switch (Data.FlashColorMode)
        {
            case DeadBodyColorMode.LightAndDarkness:
            case DeadBodyColorMode.Adaptive when !isValidColorId: // ボディカラー反映時に 不正なColorIdであれば明暗表示で返す
                colorText = CustomCosmetics.CustomColors.IsLighter(exp) ? ModTranslation.GetString("EvilSeer.LightColor") : ModTranslation.GetString("EvilSeer.DarkColor");
                break;
            case DeadBodyColorMode.Adaptive when isValidColorId: // 有効なColorIdであれば 死体の色を返す
                colorText = Palette.GetColorName(exp.Data.DefaultOutfit.ColorId);
                break;
            default:
                return null;
        }

        return string.Format(ModTranslation.GetString("EvilSeer.FlashColorText"), colorText);
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