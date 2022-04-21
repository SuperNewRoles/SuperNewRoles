using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    class ExileCutscenePatch
    {
        public static bool IsEnd;
        public static PoolablePlayer newplayer;
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        public static class ExileControllerBeginPatch
        {
            static void Postfix(ExileController __instance)
            {
                if (Data.IsMap(CustomMapNames.Agartha))
                {
                    if (__instance.exiled != null)
                    {
                        newplayer = MiraExileController.Instantiate(__instance.Player);
                        newplayer.BodySprites[0].BodySprite.sprite = ImageManager.CustomExilePlayer;

                        __instance.Player.gameObject.SetActive(false);

                        newplayer.transform.localScale *= 2.25f;
                        IsEnd = false;
                        float m = 0;
                        float m2 = 0;
                        float m3 = -1;
                        newplayer.transform.position = new Vector3(13f, 30.4f - 2, -70f);
                        IEnumerator Coro()
                        {
                            while (!IsEnd)
                            {
                                newplayer.transform.position = new Vector3(13f, 30.4f - m, -70f);
                                m += 0.1f;
                                newplayer.VisorSlot.transform.position = new Vector3(14f, 40.4f - m2, -70f);
                                newplayer.HatSlot.transform.position = new Vector3(14f, 40.4f - m2, -70f);
                                m2 += 0.13f;
                                m3 += 0.03f;
                                if (m3 > 256 || m3 < -1)
                                {
                                    m3 = 0;
                                }
                                newplayer.VisorSlot.transform.Rotate(new Vector3(0, 0, m3));
                                newplayer.HatSlot.transform.Rotate(new Vector3(0, 0, m3));
                                yield return null;
                            }
                            newplayer.gameObject.SetActive(false);
                            yield return null;
                        }
                        //AmongUsClient.Instance.StartCoroutine(Coro());
                        IsExileReseted = false;
                    }
                }
                else if (Data.IsMap(CustomMapNames.Mira))
                {
                    if (__instance.exiled != null && !IsExileReseted)
                    {
                        __instance.Player.gameObject.SetActive(true);
                        if (newplayer != null)
                        {
                            newplayer.gameObject.SetActive(false);
                        }
                        IsExileReseted = true;
                    }
                }
            }
        }
        public static Sprite Default;
        public static bool IsReseted = true;
        public static bool IsExileReseted = true;

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        class ExileControllerBeginePatch
        {

            private static TMPro.TextMeshPro breadText;

            public static bool Prefix(
                ExileController __instance,
                [HarmonyArgument(0)] GameData.PlayerInfo exiled,
                [HarmonyArgument(1)] bool tie)
            {
                if (!Data.IsMap(CustomMapNames.Agartha)) return true;
                if (__instance.specialInputHandler != null)
                {
                    __instance.specialInputHandler.disableVirtualCursor = true;
                }
                ExileController.Instance = __instance;
                ControllerManager.Instance.CloseAndResetAll();
                __instance.exiled = exiled;
                __instance.Text.gameObject.SetActive(false);
                __instance.Text.text = string.Empty;
                int num = 0;
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    try
                    {
                        if (p.isImpostor() && !p.Data.IsDead && !p.Data.Disconnected)
                        {
                            num++;
                        }
                    }
                    catch(Exception e) { SuperNewRolesPlugin.Logger.LogError(e); }
                }
                if (exiled != null)
                {
                    int num2 = 0;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        try
                        {
                            if (p.isImpostor())
                            {
                                num2++;
                            }
                        }
                        catch (Exception e) { SuperNewRolesPlugin.Logger.LogError(e); }
                    }
                    if (!PlayerControl.GameOptions.ConfirmImpostor)
                    {
                        __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextNonConfirm, exiled.PlayerName);
                    }
                    else if (exiled.Role.IsImpostor)
                    {
                        if (num2 > 1)
                        {
                            __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPP, exiled.PlayerName);
                        }
                        else
                        {
                            __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSP, exiled.PlayerName);
                        }
                    }
                    else if (num2 > 1)
                    {
                        __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextPN, exiled.PlayerName);
                    }
                    else
                    {
                        __instance.completeString = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ExileTextSN, exiled.PlayerName);
                    }
                    __instance.Player.UpdateFromEitherPlayerDataOrCache(exiled, PlayerOutfitType.Default);
                    __instance.Player.PetSlot.enabled = false;
                    __instance.Player.Skin.animator.Stop();
                    SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(exiled.Outfits[PlayerOutfitType.Default].SkinId);
                    __instance.Player.Skin.layer.sprite = skin.EjectFrame;
                    if (exiled.Object.isImpostor())
                    {
                        num--;
                    }
                }
                else
                {
                    if (tie)
                    {
                        __instance.completeString = string.Format(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileTie),num);
                    }
                    else
                    {
                        __instance.completeString = string.Format(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.NoExileSkip), num);
                    }
                    __instance.Player.gameObject.SetActive(false);
                }
                if (num == 1)
                {
                    __instance.ImpostorText.text = string.Format(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainS), num);
                }
                else
                {
                    __instance.ImpostorText.text = string.Format(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ImpostorsRemainP), num);
                }
                Transform ExileCust = GameObject.Find("MiraExileCutscene(Clone)").transform;
                Background = ExileCust.FindChild("Background");
                SpriteRenderer render = Background.GetComponent<SpriteRenderer>();
                render.sprite = ImageManager.ExileBackImage;
                render.color = Color.white;
                Background.localScale *= 0.051f;
                BlackGround = GameObject.Instantiate(Background,Background.transform.parent);
                SpriteRenderer renderblack = BlackGround.GetComponent<SpriteRenderer>();
                renderblack.sprite = ImageManager.ExileBackImage;
                renderblack.color = Color.black;
                renderblack.gameObject.SetActive(true);
                BlackGround.position += new Vector3(10000,10000,-10);
                //__instance.Text.text = __instance.completeString;
                MiraCont = GameObject.Find("MiraExileCutscene(Clone)").GetComponent<MiraExileController>();
                MiraCont.BackgroundClouds.gameObject.SetActive(false);
                __instance.StartCoroutine(Animate(ExileController.Instance));
                return false;
            }
        }
        private static Transform Background;
        private static Transform BlackGround;
        private static MiraExileController MiraCont;
        private static IEnumerator Animate(ExileController __instance)
        {
            yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear);
            yield return Effects.All(MiraCont.HandleText(), Effects.Slide2D(Background, new Vector2(0f, -3f), new Vector2(0f, 0.5f), __instance.Duration), Effects.Sequence(Effects.Wait(2f), Effects.Slide2D(BlackGround, new Vector2(0f, -12f), new Vector2(0f, 2.5f), 1.2f)));
            if (PlayerControl.GameOptions.ConfirmImpostor)
            {
                __instance.ImpostorText.gameObject.SetActive(true);
            }
            yield return Effects.Bloop(0f, __instance.ImpostorText.transform);
            yield return (object)new WaitForSeconds(0.5f);
            yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);
            __instance.WrapUp();
        }
        [HarmonyPatch(typeof(MiraExileController), nameof(MiraExileController.Animate))]
        public static class ExileControllerPatch
        {
            static void Prefix(MiraExileController __instance)
            {
                /*
                if (Data.IsMap(CustomMapNames.Agartha))
                {
                    ExileCust.FindChild("ExilePoolablePlayer").transform.localScale *= 1.25f;
                    //ExileCust.FindChild("ExilePoolablePlayer").gameObject.SetActive(false);
                    //ExileCust.FindChild("StatusText_TMP").transform.localPosition *= 1.5f;
                    //ExileCust.FindChild("ImpostorText_TMP").transform.localPosition *= 1.5f;
                    ExileCust.FindChild("BackgroundClouds").gameObject.SetActive(false);
                    ExileCust.FindChild("ForegroundClouds").gameObject.SetActive(false);
                    if (Default == null)
                    {
                        Default = render.sprite;
                    }
                    render.sprite = ImageManager.ExileBackImage;
                    render.color = Color.white;
                    Background.localScale *= 0.05f;
                    IsReseted = false;
                } else if (Data.IsMap(CustomMapNames.Mira) && !IsReseted)
                {
                    Transform ExileCust = GameObject.Find("MiraExileCutscene(Clone)").transform;
                    ExileCust.FindChild("ExilePoolablePlayer").transform.localScale *= 1.25f;
                    //ExileCust.FindChild("ExilePoolablePlayer").gameObject.SetActive(false);
                    //ExileCust.FindChild("StatusText_TMP").transform.localPosition *= 1.5f;
                    //ExileCust.FindChild("ImpostorText_TMP").transform.localPosition *= 1.5f;
                    ExileCust.FindChild("BackgroundClouds").gameObject.SetActive(false);
                    ExileCust.FindChild("ForegroundClouds").gameObject.SetActive(false);
                    Transform Background = ExileCust.FindChild("Background");
                    SpriteRenderer render = Background.GetComponent<SpriteRenderer>();
                    render.sprite = Default;
                    render.color = Color.white;
                    Background.localScale /= 0.05f;
                }
                */
            }
        }
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        public static class ExileControllerWrapUp
        {
            static void Postfix(ExileController __instance)
            {
                IsEnd = true;
            }
        }
    }
}