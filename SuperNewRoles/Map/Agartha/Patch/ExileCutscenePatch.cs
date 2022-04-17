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

                        newplayer.transform.localScale *= 1.5f;
                        IsEnd = false;
                        float m = 0;
                        float m2 = 0;
                        float m3 = -1;
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
                                newplayer.VisorSlot.transform.Rotate(new Vector3(0,0,m3));
                                newplayer.HatSlot.transform.Rotate(new Vector3(0, 0, m3));
                                yield return null;
                            }
                            newplayer.gameObject.SetActive(false);
                            yield return null;
                        }
                        AmongUsClient.Instance.StartCoroutine(Coro());
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
        [HarmonyPatch(typeof(MiraExileController), nameof(MiraExileController.Animate))]
        public static class ExileControllerPatch
        {
            static void Prefix(MiraExileController __instance)
            {
                if (Data.IsMap(CustomMapNames.Agartha))
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