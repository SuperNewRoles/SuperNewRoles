using HarmonyLib;

namespace SuperNewRoles.MapOptions
{
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance)
        {
            // ベントを追加する
            AdditionalVents.AddAdditionalVents();
            // スペシメンにバイタルを移動する
            SpecimenVital.moveVital();
        }
    }
}
