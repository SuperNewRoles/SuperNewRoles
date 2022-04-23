using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using SuperNewRoles.Sabotage.CognitiveDeficit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    class ExileCutscenePatch
    {
        public static PoolablePlayer UpObject;
        public static bool IsEnd;
        private static int m;
        static float rotatedata;
        static IEnumerator Coro()
        {
            var Player = ExileController.Instance.Player;
            Player.transform.localPosition = new Vector3(0f, 5.85f, 1001f);
            Player.transform.localScale = new Vector3(0.75f,0.75f,1.5f);

            Player.HatSlot.gameObject.SetActive(false);
            Player.VisorSlot.gameObject.SetActive(false);
            Player.Skin.gameObject.SetActive(false);

            
            UpObject = GameObject.Instantiate(Player,Player.transform.parent);
            var UpGameObject = UpObject.gameObject;
            UpObject.name = "Exile_Up";
            UpObject.transform.localPosition = new Vector3(0f, 14.75f, 1002f);
            UpObject.transform.localScale = new Vector3(0.75f, 0.75f, 1.5f);
            UpGameObject.GetComponent<SpriteRenderer>().sprite = ImageManager.AgarthagetSprite("CustomExilePlayer_Up");
            if (ExileController.Instance.exiled != null)
            {
                while (UpGameObject.transform.localPosition.y > 8.7f)
                {
                    UpGameObject.transform.localPosition += new Vector3(0, -0.06f, 0);
                    Player.transform.localPosition += new Vector3(0, -0.06f, 0);
                    yield return null;
                }
                while (!IsEnd)
                {
                    if (Time.deltaTime > 0)
                    {
                        UpGameObject.transform.position += new Vector3(0, 0.08f, 0);
                        Background.localPosition += new Vector3(0, 0.04f, 0);
                        rotatedata += 0.025f;
                        if (rotatedata >= 360)
                        {
                            rotatedata = 0;
                        }
                        Player.transform.Rotate(new Vector3(0, 0, rotatedata));
                    }
                    yield return null;
                }
            }else
            {
                yield return Effects.Slide2D(Background, new Vector2(0f, -3f), new Vector2(0f, 1.5f), 4.5f);
            }
            ExileController.Instance.Player.gameObject.SetActive(false);
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
                Background.localScale *= 0.051f;
                Background.localPosition += new Vector3(0,0,1000);
                SpriteRenderer render = Background.GetComponent<SpriteRenderer>();
                render.sprite = ImageManager.ExileBackImage;
                render.color = Color.white;
                BlackGround = GameObject.Instantiate(Background,Background.transform.parent);
                SpriteRenderer renderblack = BlackGround.GetComponent<SpriteRenderer>();
                renderblack.sprite = ImageManager.AgarthagetSprite("Exile_BlackGra");
                renderblack.color = Color.white;
                renderblack.gameObject.SetActive(true);
                BlackGround.position += new Vector3(10000,10000,-10);
                BlackGround.localScale *= 0.9f;
                //__instance.Text.text = __instance.completeString;
                MiraCont = GameObject.FindObjectOfType<MiraExileController>();
                MiraCont.BackgroundClouds.gameObject.SetActive(false);

                //__instance.Duration -= 1f;

                SuperNewRolesPlugin.Logger.LogInfo(__instance.Duration);
                __instance.Player.BodySprites[0].BodySprite.sprite = ImageManager.CustomExilePlayer;

                if (__instance.exiled == null)
                {
                    BackUpTime = 2f;
                    TextPlusTime = 1f;
                } else
                {
                    BackUpTime = 2.75f;
                    TextPlusTime = 1.75f;
                }
                rotatedata = 0;
                __instance.StartCoroutine(Animate(ExileController.Instance));
                __instance.StartCoroutine(Coro());
                return false;
            }
        }
        private static Transform Background;
        private static Transform BlackGround;
        private static MiraExileController MiraCont;
        static float BackUpTime;
        static float TextPlusTime;
        private static IEnumerator Animate(ExileController __instance)
        {
            IsEnd = false;
            yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear);
            TaskBar.Instance.gameObject.SetActive(false);
            yield return Effects.All(Effects.Sequence(Effects.Wait(__instance.Duration - 4+TextPlusTime),MiraCont.HandleText()),Effects.Slide2D(Background, new Vector2(0f, -3f), new Vector2(0f, -1.5f), 1.5f), Effects.Sequence(Effects.Wait(BackUpTime), Effects.Slide2D(BlackGround, new Vector2(0f, -12f), new Vector2(0f, 2.5f), 1.25f)));
            if (PlayerControl.GameOptions.ConfirmImpostor)
            {
                __instance.ImpostorText.gameObject.SetActive(true);
            }
            yield return Effects.Bloop(0f, __instance.ImpostorText.transform);
            yield return new WaitForSeconds(0.5f);
            yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);
            __instance.WrapUp();
            if (PlayerControl.GameOptions.TaskBarMode != TaskBarMode.Invisible)
            {
                TaskBar.Instance.gameObject.SetActive(true);
            }
            IsEnd = true;
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