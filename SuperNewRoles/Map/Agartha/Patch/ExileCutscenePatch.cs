using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    class ExileCutscenePatch
    {

        [HarmonyPatch(typeof(MiraExileController), nameof(MiraExileController.Animate))]
        public static class ExileController
        {
            static void Prefix(MiraExileController __instance)
            {
                if (Data.IsMap(CustomMapNames.Agartha))
                {
                    Transform ExileCust = GameObject.Find("MiraExileCutscene(Clone)").transform;
                    ExileCust.FindChild("ExilePoolablePlayer").transform.localScale *= 1.25f;
                    //ExileCust.FindChild("StatusText_TMP").transform.localPosition *= 1.5f;
                    //ExileCust.FindChild("ImpostorText_TMP").transform.localPosition *= 1.5f;
                    ExileCust.FindChild("BackgroundClouds").gameObject.SetActive(false);
                    ExileCust.FindChild("ForegroundClouds").gameObject.SetActive(false);
                    Transform Background = ExileCust.FindChild("Background");
                    SpriteRenderer render = Background.GetComponent<SpriteRenderer>();
                    render.sprite = ImageManager.ExileBackImage;
                    render.color = Color.white;
                    Background.localScale *= 0.13f;
                }
            }
        }
    }
}
