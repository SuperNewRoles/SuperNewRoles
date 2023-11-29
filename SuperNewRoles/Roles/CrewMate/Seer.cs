using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

public class SeerBase : RoleBase
{
    public List<(Vector3, int)> deadBodyPositions { get; set; }
    public float soulDuration { get; }
    public bool limitSoulDuration { get; }
    public int SeerMode { get; }
    internal const int DefaultBodyColorId = (int)CustomCosmetics.CustomColors.ColorType.Crasyublue;
    internal const int LightBodyColorId = (int)CustomCosmetics.CustomColors.ColorType.Pitchwhite;
    internal const int DarkBodyColorId = (int)CustomCosmetics.CustomColors.ColorType.Crasyublue;
    public SeerBase(int soulDuration, bool limitSoulDuration, int Mode, PlayerControl player, RoleInfo Roleinfo, OptionInfo Optioninfo, IntroInfo Introinfo) : base(player, Roleinfo, Optioninfo, Introinfo)
    {
        deadBodyPositions = new();
        this.soulDuration = soulDuration;
        this.limitSoulDuration = limitSoulDuration;
        this.SeerMode = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 1 : Mode;
    }
}
class SeerHandler
//マッド・イビル・フレンズ・ジャッカル・サイドキック　シーア
{
    private static SpriteRenderer FullScreenRenderer;
    private static HudManager Renderer;
    private static Coroutine FlashCoroutine;
    public static void ShowFlash_ClearAndReload()
    {
        FullScreenRenderer = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.FullScreen, FastDestroyableSingleton<HudManager>.Instance.transform);
        Renderer = FastDestroyableSingleton<HudManager>.Instance;
        FlashCoroutine = null;
    }
    /** <summary>
        画面を光らせる
        </summary>
        <param name="color">
        (new Color("r値" / 255f, "g値" / 255f, "b値" / 255f))
        あるいはUnityのcolorコード指定で色を選択
        </param>
        <param name="duration">
        color色に画面を光らせはじめ、終わるまでの時間(duration/2秒時に指定色に光る)
        </param>
    **/
    public static void ShowFlash(Color color, float duration = 1f, Action OnFlashEnd = null)
    {
        if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started || Renderer == null || FullScreenRenderer == null) return;

        FullScreenRenderer.gameObject.SetActive(true);
        FullScreenRenderer.enabled = true;
        FlashCoroutine = Renderer.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
        {
            if (p < 0.5f)
            {
                if (FullScreenRenderer != null)
                {
                    FullScreenRenderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f) * color.a);
                }
            }
            else
            {
                if (FullScreenRenderer != null)
                {
                    FullScreenRenderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f) * color.a);
                }
            }
            p *= duration;
            if (p >= duration && FullScreenRenderer != null)
            {
                FullScreenRenderer.enabled = true;
                FullScreenRenderer.gameObject.SetActive(false);
                FlashCoroutine = null;
                Logger.Info("発動待機状態に戻しました。", "SetActive(false)");
                OnFlashEnd?.Invoke();
            }
        })));
    }
    public static void HideFlash()
    {
        FullScreenRenderer.enabled = true;
        FullScreenRenderer.gameObject.SetActive(false);
        if (FlashCoroutine != null)
            Renderer.StopCoroutine(FlashCoroutine);
    }

    private static Sprite GetSoulSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Soul.png", 500f);

    public static class WrapUpPatch
    {
        public static void WrapUpPostfix()
        {
            var role = PlayerControl.LocalPlayer.GetRole();
            var seerBase = PlayerControl.LocalPlayer.GetRoleBase<SeerBase>();
            if (seerBase != null || role is RoleId.Seer or RoleId.MadSeer or RoleId.EvilSeer or RoleId.SeerFriends or RoleId.JackalSeer or RoleId.SidekickSeer)
            {
                List<(Vector3, int)> DeadBodyPositions = seerBase?.deadBodyPositions ?? new();
                bool limitSoulDuration = seerBase?.limitSoulDuration ?? false;
                float soulDuration = seerBase?.soulDuration ?? 0f;
                if (seerBase != null)
                {
                    seerBase.deadBodyPositions = new();
                    if (seerBase.SeerMode is not 0 and not 2)
                        return;
                }
                switch (role)
                {
                    case RoleId.Seer:
                        DeadBodyPositions = RoleClass.Seer.deadBodyPositions;
                        RoleClass.Seer.deadBodyPositions = new List<(Vector3, int)>();
                        limitSoulDuration = RoleClass.Seer.limitSoulDuration;
                        soulDuration = RoleClass.Seer.soulDuration;
                        if (RoleClass.Seer.mode is not 0 and not 2) return;
                        break;
                    case RoleId.MadSeer:
                        DeadBodyPositions = RoleClass.MadSeer.deadBodyPositions;
                        RoleClass.MadSeer.deadBodyPositions = new List<(Vector3, int)>();
                        limitSoulDuration = RoleClass.MadSeer.limitSoulDuration;
                        soulDuration = RoleClass.MadSeer.soulDuration;
                        if (RoleClass.MadSeer.mode is not 0 and not 2) return;
                        break;
                    case RoleId.SeerFriends:
                        DeadBodyPositions = RoleClass.SeerFriends.deadBodyPositions;
                        RoleClass.SeerFriends.deadBodyPositions = new List<(Vector3, int)>();
                        limitSoulDuration = RoleClass.SeerFriends.limitSoulDuration;
                        soulDuration = RoleClass.SeerFriends.soulDuration;
                        if (RoleClass.SeerFriends.mode is not 0 and not 2) return;
                        break;
                    case RoleId.JackalSeer:
                    case RoleId.SidekickSeer:
                        DeadBodyPositions = RoleClass.JackalSeer.deadBodyPositions;
                        RoleClass.JackalSeer.deadBodyPositions = new List<(Vector3, int)>();
                        limitSoulDuration = RoleClass.JackalSeer.limitSoulDuration;
                        soulDuration = RoleClass.JackalSeer.soulDuration;
                        if (RoleClass.JackalSeer.mode is not 0 and not 2) return;
                        break;
                }
                foreach ((Vector3, int) posAndColor in DeadBodyPositions)
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

                    if (limitSoulDuration)
                    {
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
            }
        }

        public static class MurderPlayerPatch
        {
            public static void Postfix([HarmonyArgument(0)] PlayerControl target)
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                SeerBase seerBase = PlayerControl.LocalPlayer.GetRoleBase<SeerBase>();
                if (seerBase != null || role is RoleId.Seer or RoleId.MadSeer or RoleId.EvilSeer or RoleId.SeerFriends or RoleId.JackalSeer or RoleId.SidekickSeer)
                {
                    var bodyColorId = target.Data.DefaultOutfit.ColorId;

                    // 自分が死んだ後は, どのシーアも霊魂の色にクルーのボディカラーを反映させる。
                    var soulColorId = PlayerControl.LocalPlayer.IsDead()
                                    ? bodyColorId
                                    : EvilSeer.DefaultBodyColorId;

                    bool flashModeFlag = false;
                    Color flashColor = new(42f / 255f, 187f / 255f, 245f / 255f); // 基本の発光カラー
                    if (seerBase != null)
                    {
                        if (seerBase.deadBodyPositions != null)
                            seerBase.deadBodyPositions.Add((target.transform.position, soulColorId));
                    }

                    switch (role)
                    {
                        case RoleId.Seer:
                            if (RoleClass.Seer.deadBodyPositions != null) RoleClass.Seer.deadBodyPositions.Add((target.transform.position, soulColorId));
                            flashModeFlag = RoleClass.Seer.mode <= 1;
                            break;
                        case RoleId.MadSeer:
                            if (RoleClass.MadSeer.deadBodyPositions != null) RoleClass.MadSeer.deadBodyPositions.Add((target.transform.position, soulColorId));
                            flashModeFlag = RoleClass.MadSeer.mode <= 1;
                            break;
                        case RoleId.EvilSeer:
                            // |:===== 共通の処理 =====:|
                            var isLight = CustomCosmetics.CustomColors.LighterColors.Contains(target.Data.DefaultOutfit.ColorId);

                            // |:===== 霊魂関連の処理 =====:|
                            var indistinctBodyColorId = isLight
                                ? SeerBase.LightBodyColorId
                                : SeerBase.DarkBodyColorId;

                            EvilSeer evilSeer = seerBase as EvilSeer;

                            var isBodyColor = evilSeer.IsUnique && EvilSeer.IsCrewSoulColor.GetBool();

                            if (PlayerControl.LocalPlayer.IsDead() || (isBodyColor && evilSeer.IsClearColor)) soulColorId = bodyColorId; // 彩光が最高
                            else if (isBodyColor) soulColorId = indistinctBodyColorId; // 明暗
                            else soulColorId = EvilSeer.DefaultBodyColorId; // デフォルト

                            // |:===== 死の点滅関連の処理 =====:|
                            flashModeFlag = seerBase.SeerMode <= 1;
                            if (evilSeer != null && evilSeer.IsUnique && EvilSeer.IsFlashBodyColor.GetBool()) // SHRModeの場合このif文は読まれない
                            {
                                string showtext = "";
                                if (evilSeer.IsClearColor) // 彩光が最高
                                {
                                    flashColor = Palette.PlayerColors[bodyColorId];
                                    var crewColorText = $"[ <color=#89c3eb>{OutfitManager.GetColorTranslation(Palette.ColorNames[bodyColorId])}</color> : {ModHelpers.Cs(flashColor, "■")} ]";
                                    showtext = string.Format(ModTranslation.GetString("EvilSeerClearColorDeadText"), crewColorText);
                                }
                                else // 明暗
                                {
                                    flashColor = isLight
                                        ? new(137f / 255f, 195f / 255f, 235f / 255f)    // 明
                                        : new(116f / 255f, 50f / 255f, 92f / 255f);     // 暗

                                    showtext = isLight
                                        ? ModTranslation.GetString("EvilSeerLightPlayerDeadText")
                                        : ModTranslation.GetString("EvilSeerDarkPlayerDeadText");
                                }
                                if (EvilSeer.IsReportingBodyColorName.GetBool() && flashModeFlag)
                                    new CustomMessage(showtext, 5, true, RoleClass.Seer.color, new(42f / 255f, 187f / 255f, 245f / 255f));
                            }
                            break;
                        case RoleId.SeerFriends:
                            if (RoleClass.SeerFriends.deadBodyPositions != null) RoleClass.SeerFriends.deadBodyPositions.Add((target.transform.position, soulColorId));
                            flashModeFlag = RoleClass.SeerFriends.mode <= 1;
                            break;
                        case RoleId.JackalSeer:
                        case RoleId.SidekickSeer:
                            if (RoleClass.JackalSeer.deadBodyPositions != null) RoleClass.JackalSeer.deadBodyPositions.Add((target.transform.position, soulColorId));
                            flashModeFlag = RoleClass.JackalSeer.mode <= 1;
                            break;
                    }
                    if (flashModeFlag) ShowFlash(flashColor);
                }
            }
            public static void ShowFlash_SHR(PlayerControl target)
            {
                List<List<PlayerControl>> seers = new() {
                    RoleClass.Seer.SeerPlayer,
                    CustomRoles.GetRolePlayers<EvilSeer>().ToList(),
                    RoleClass.MadSeer.MadSeerPlayer,
                    RoleClass.JackalSeer.JackalSeerPlayer,
                    RoleClass.SeerFriends.SeerFriendsPlayer
                };
                foreach (var p in seers)
                {
                    if (p == null) continue;
                    foreach (var p2 in p)
                    {
                        if (p2 == null) continue;
                        if (!p2.IsMod())
                        {
                            p2.ShowReactorFlash(1.5f);
                            Logger.Info($"非導入者で尚且つ[ {p2.GetRole()} ]である{p2.GetDefaultName()}に死の点滅を発生させました。", "MurderPlayer");
                        }
                    }
                }
            }
        }
    }
}