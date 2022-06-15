using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    class SplashLogoAnimatorPatch
    {
        public static void Prefix(SplashManager __instance)
        {
            if (ConfigRoles.DebugMode.Value)
            {
                __instance.sceneChanger.AllowFinishLoadingScene();
                __instance.startedSceneLoad = true;
            }
        }
    }
}