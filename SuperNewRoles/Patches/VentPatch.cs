using HarmonyLib;

namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetOutline))]
    class VentSetOutlinePatch
    {
        static void Postfix(Vent __instance){
            var color = IntroData.GetIntroData(CachedPlayer.LocalPlayer.PlayerControl.GetRole(), PlayerControl.LocalPlayer).color;
            string[] ventColors = new string[] { "_OutlineColor", "_AddColor" };
            foreach (var data in ventColors)
                __instance.myRend.material.SetColor(data, color);
        }
    }
}