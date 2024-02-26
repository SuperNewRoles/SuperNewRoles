using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.PlusMode;

namespace SuperNewRoles.Patches;

class EmergencyMinigamePatch
{
    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))] // 導入者はここで
    class EmergencyUpdatePatch
    {
        public static void Postfix(EmergencyMinigame __instance)
        {
            bool enabledMeeting = MeetingStatus(out string statusText, out string numberText);

            if (statusText != null) __instance.StatusText.text = statusText;
            if (numberText != null) __instance.NumberText.text = numberText;

            if (!enabledMeeting) // 会議が無効なら, ボタンを封じる
            {
                __instance.state = 2;
                __instance.ButtonActive = false;
                __instance.ClosedLid.gameObject.SetActive(true);
                __instance.OpenLid.gameObject.SetActive(false);
            }
        }

        private static bool MeetingStatus(out string statusText, out string numberText)
        {
            statusText = numberText = null;
            bool enabledMeeting = true; // 会議が有効か

            if (!Sabotage.SabotageManager.IsOKMeeting())
            {
                enabledMeeting = false;
                statusText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyDuringCrisis);
                numberText = string.Empty;
            }

            return enabledMeeting;
        }
    }
}