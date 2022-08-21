using HarmonyLib;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
    class SplashLogoAnimatorPatch
    {
        static bool IsAgarthaLoaded = false;
        public static void Prefix(SplashManager __instance)
        {
            if (!IsAgarthaLoaded)
            {
                CustomMap.LoadedHandle.Load();
                IsAgarthaLoaded = true;
            }
            if (ConfigRoles.DebugMode.Value)
            {
                __instance.sceneChanger.AllowFinishLoadingScene();
                __instance.startedSceneLoad = true;
            }
        }
    }
}
