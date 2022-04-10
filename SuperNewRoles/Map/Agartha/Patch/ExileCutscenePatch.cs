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
                        __instance.Player.gameObject.SetActive(false);
                        newplayer.BodySprites[0].BodySprite.sprite = ImageManager.CustomExilePlayer;
                        newplayer.transform.position = new Vector3(14f, 30.4f, -70f);
                        newplayer.transform.localScale *= 1.5f;
                        IsEnd = false;
                        IEnumerator Coro()
                        {
                            while (!IsEnd)
                            {
                                newplayer.transform.localPosition -= new Vector3(0, 0.025f, 0);
                                yield return null;
                            }
                            yield return null;
                        }
                        AmongUsClient.Instance.StartCoroutine(Coro());
                    }
                }
            }
        }

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
                    render.sprite = ImageManager.ExileBackImage;
                    render.color = Color.white;
                    Background.localScale *= 0.15f;
                }
            }
        }
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
        public static class ExileControllerWrapUp
        {
            static void Postfix(ExileController __instance)
            {
                IsEnd = true;
                GameObject.Destroy(newplayer);
            }
        }
    }
}