using HarmonyLib;
using Il2CppSystem.Linq;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.PlusMode;

namespace SuperNewRoles.Patches;

class EmergencyMinigamePatch
{
    /// <summary>
    /// 緊急招集の状態
    /// </summary>
    enum Status
    {
        Vanilla, // バニラの判定及び, StatusTextを使用する
        EnabledForMod, // モッド側の有効判定及び, StatusTextを使用する
        DisabledForMod // モッド側の無効判定及び, StatusTextを使用する
    }

    [HarmonyPatch(typeof(EmergencyMinigame), nameof(EmergencyMinigame.Update))]
    class EmergencyUpdatePatch
    {
        public static void Postfix(EmergencyMinigame __instance)
        {
            Status status = CheckMeetingStatus(out string statusText, out string numberText);

            if (status == Status.Vanilla) return; // バニラ判定を使用するなら 離脱

            if (statusText != null) __instance.StatusText.text = statusText;
            if (numberText != null) __instance.NumberText.text = numberText;

            bool buttonActive = status == Status.EnabledForMod;

            __instance.state = buttonActive ? 1 : 2;
            __instance.ButtonActive = buttonActive;
            __instance.ClosedLid.gameObject.SetActive(!buttonActive);
            __instance.OpenLid.gameObject.SetActive(buttonActive);
        }
    }

    /// <summary>
    /// 緊急招集を行えるかを判定し, ボタンに表示する情報を取得する。
    /// </summary>
    /// <param name="statusText">ボタンの状態の情報</param>
    /// <param name="numberText">残り緊急招集 使用可能回数</param>
    /// <returns>Status : 緊急招集の現在の状態</returns>
    private static Status CheckMeetingStatus(out string statusText, out string numberText)
    {
        statusText = numberText = null;

        // 緊急招集を使用できない設定なら
        if (!PlusGameOptions.EmergencyMeetingsCallstate.enabledSetting || GameManager.Instance.LogicOptions.GetNumEmergencyMeetings() < 0)
        {
            statusText = ModTranslation.GetString("MeetingStatusCanNotCallEmergencyMeeting");
            numberText = string.Empty;
            return Status.DisabledForMod;
        }

        // サボタージュ中なら
        if (!Sabotage.SabotageManager.IsOKMeeting())
        {
            statusText = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EmergencyDuringCrisis);
            numberText = string.Empty;
            return Status.DisabledForMod;
        }

        // 以降, 会議クールが空けてない時は, バニラのステータス及び文章を優先する。
        if (!(ShipStatus.Instance.Timer < 15f || ShipStatus.Instance.EmergencyCooldown > 0f)) return Status.Vanilla;

        // 自分の会議回数が残っているか
        bool HaveEmergencies = PlayerControl.LocalPlayer.RemainingEmergencies > 0;

        // 全体会議回数制限が有効なら
        if (PlusGameOptions.EmergencyMeetingsCallstate.enabledSetting && PlusGameOptions.EmergencyMeetingsCallstate.maxCount != byte.MaxValue)
        {
            // 全体回数に達してない時
            if (PlusGameOptions.EmergencyMeetingsCallstate.maxCount > ReportDeadBodyPatch.MeetingCount.emergency)
            {
                var personalCount = $"<color=#fe1919>{PlayerControl.LocalPlayer.RemainingEmergencies}</color>";
                var allCount = $"<color=#fe1919>{PlusGameOptions.EmergencyMeetingsCallstate.maxCount - ReportDeadBodyPatch.MeetingCount.emergency}</color>";

                var personalStatus = string.Format(ModTranslation.GetString("MeetingStatusPersonalEmergencyCount"), PlayerControl.LocalPlayer.Data.PlayerName, personalCount);
                var allStatus = string.Format(ModTranslation.GetString("MeetingStatusAllEmergencyCount"), allCount);

                statusText = $"{personalStatus}\n\n{allStatus}";
                numberText = string.Empty;

                return HaveEmergencies ? Status.EnabledForMod : Status.DisabledForMod;
            }
            else
            {
                statusText = ModTranslation.GetString("MeetingStatusUpperLimitReached");
                numberText = string.Empty;
                return Status.DisabledForMod;
            }
        }

        return Status.Vanilla;
    }
}