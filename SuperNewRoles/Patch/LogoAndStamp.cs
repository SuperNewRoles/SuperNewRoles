using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace SuperNewRoles.Patches
{
    [HarmonyPatch]
    public static class CredentialsPatch
    {
        
        public static string baseCredentials = $@"<size=130%><color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color></size> v{SuperNewRolesPlugin.Version.ToString()}";

        private static Task<bool> kari;
        public static string contributorsCredentials = "<size=80%>GitHub Contributors: Alex2911, amsyarasyiq, gendelo3</size>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Prefix(VersionShower __instance)
            {
            }
            static void Postfix(VersionShower __instance)
            {

                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;
                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0f, 0);
                credentials.SetText(ModTranslation.getString("creditsMain"));
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.9f;
                AutoUpdate.checkForUpdate(credentials);

                var version = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(credentials);
                version.transform.position = new Vector3(0, -0.35f, 0);
                version.SetText(string.Format(ModTranslation.getString("creditsVersion"), SuperNewRolesPlugin.Version.ToString()));

                credentials.transform.SetParent(amongUsLogo.transform);
                version.transform.SetParent(amongUsLogo.transform);
                
                

            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
                {
                    __instance.text.text = $"{baseCredentials}\n{__instance.text.text}";
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        __instance.transform.localPosition = new Vector3(3.45f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    }
                    else
                    {
                        __instance.transform.localPosition = new Vector3(4.2f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    }
                }
                else
                {
                    __instance.text.text = $"{baseCredentials}\n{ModTranslation.getString("creditsFull")}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(3.5f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class LogoPatch
        {
            static void Postfix(PingTracker __instance)
            {

                DestroyableSingleton<ModManager>.Instance.ShowModStamp();


                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null)
                {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }
                var snrLogo = new GameObject("bannerLogo_SNR");
                snrLogo.transform.position = Vector3.up;
                var renderer = snrLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = ModTranslation.getImage("banner.png", 150f);
            }
        }
    }
}
