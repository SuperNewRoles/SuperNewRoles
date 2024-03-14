using System.Collections.Generic;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Linq;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.PlusMode;
using SuperNewRoles.Mode.SuperHostRoles;

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
        if (ShipStatus.Instance.Timer < 15f || ShipStatus.Instance.EmergencyCooldown > 0f) return Status.Vanilla;

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

    public static class SHRMeetingStatusAnnounce
    {
        public static void EmergencyCount(PlayerControl convenor)
        {
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles) || !AmongUsClient.Instance.AmHost) return;
            if (!(PlusGameOptions.EmergencyMeetingsCallstate.enabledSetting && PlusGameOptions.EmergencyMeetingsCallstate.maxCount != byte.MaxValue)) return;

            int count = PlusGameOptions.EmergencyMeetingsCallstate.maxCount - ReportDeadBodyPatch.MeetingCount.emergency;
            if (count <= 0) return;

            string info = $"<align={"left"}>{string.Format(ModTranslation.GetString("MeetingStatusAllEmergencyCount"), $"<color=#fe1919>{count}</color>")}\n\n{ModTranslation.GetString("SHRMeetingStatusAnnounceYourOnry")}</align>";
            AddChatPatch.ChatInformation(convenor, ModTranslation.GetString("ReportDeadBodySetting"), info);
        }

        public static void MakeSettingKnown()
        {
            if (!AmongUsClient.Instance.AmHost || !ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            if (!(PlusGameOptions.EmergencyMeetingsCallstate.enabledSetting && PlusGameOptions.EmergencyMeetingsCallstate.maxCount != byte.MaxValue)) return;

            const string line = "\n<color=#4d4398>|-----------------------------------------------------------------------------|</color>\n";

            string info_Enable = ModTranslation.GetString("SHRMeetingStatusEnableLimitSetting");
            string info_Count = string.Format(ModTranslation.GetString("SHRMeetingStatusMeetingsCallMaxCount"), $"<color=#fe1919>{PlusGameOptions.EmergencyMeetingsCallstate.maxCount}</color>");

            string info_All = $"<align={"left"}><size=50%><color=#00000000>SHRMeetingStatusAnnounce</size></color><size=80%>{line}{info_Enable}\n\n{info_Count}{line}</size></align>";

            AddChatPatch.SendCommand(null, info_All);
        }

        /// <summary>
        /// 緊急会議が全体の使用可能回数に達した時に, 全体に行うアナウンス
        /// </summary>
        public static void LimitAnnounce()
        {
            if (!AmongUsClient.Instance.AmHost || !ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            if (!(PlusGameOptions.EmergencyMeetingsCallstate.enabledSetting && PlusGameOptions.EmergencyMeetingsCallstate.maxCount != byte.MaxValue)) return;

            if (PlusGameOptions.EmergencyMeetingsCallstate.maxCount != ReportDeadBodyPatch.MeetingCount.emergency) return; // 会議残り回数が0以外ならreturn

            const string line = "\n<color=#4d4398>|-----------------------------------------------------------------------------|</color>\n";

            string info_Count = string.Format(ModTranslation.GetString("MeetingStatusAllEmergencyCount"), "<color=#fe1919>0</color>");
            string info_NotUse = $"<size=70%>{ModTranslation.GetString("SHRMeetingStatusNotUseButton")}</size>";
            string info_All = $"<align={"left"}><size=50%><color=#00000000>SHRMeetingStatusAnnounce</color></size><size=80%>{line}\n{info_Count}\n{line}\n{info_NotUse}{line}</size></align>";

            AddChatPatch.SendCommand(null, info_All);
        }
    }

    /// <summary>
    /// [死者が出るまで緊急ボタンクールダウンを変更する] 設定関連の処理を行う
    /// </summary>
    public class FirstEmergencyCooldown
    {
        public static void OnWrapUp(bool IsPlayerExpelled) => SetCooldown(GetEmergencyCooldown(IsThereDeadBeforeCurrentTurn(IsPlayerExpelled)));
        public static void OnCheckForEndVotingNotMod(bool IsPlayerExpelled) => SyncSettings(GetEmergencyCooldown(IsThereDeadBeforeCurrentTurn(IsPlayerExpelled)));

        /// <summary>
        /// 緊急会議クールダウンのリセットタイミングで死者が存在するか
        /// (DeadPlayer.deadPlayers.Countでは, リセット時に追放者の判定が間に合わない為, 補正して取得する)
        /// </summary>
        /// <param name="IsPlayerExpelled">追放者が存在するか</param>
        /// <returns>true : 存在する / false : 存在しない</returns>
        static bool IsThereDeadBeforeCurrentTurn(bool IsPlayerExpelled) => DeadPlayer.deadPlayers.Count > 0 || IsPlayerExpelled;
        /// <summary>
        /// 渡された死亡状態から, CTの判定及び取得を行う
        /// </summary>
        /// <param name="IsThereDeadBeforeCurrentTurn">死者存在後のCTを取得するか</param>
        /// <returns>現在の緊急会議CT</returns>
        static int GetEmergencyCooldown(bool IsThereDeadBeforeCurrentTurn) => IsThereDeadBeforeCurrentTurn
            ? GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.EmergencyCooldown)
            : PlusGameOptions.FirstEmergencyCooldownSetting.GetInt();

        /// <summary>
        /// 導入者個人でクールダウンを設定に従い反映する
        /// </summary>
        /// <param name="cooldown">設定CT</param>
        static void SetCooldown(int cooldown)
        {
            if (!PlusGameOptions.EnableFirstEmergencyCooldown) return;

            var optData = SyncSetting.OptionDatas.Local.DeepCopy();
            optData.SetInt(Int32OptionNames.EmergencyCooldown, cooldown);
            GameManager.Instance.LogicOptions.SetGameOptions(optData);

            ShipStatus.Instance.EmergencyCooldown = cooldown;

            Logger.Info($"緊急会議クールダウン セット : {cooldown}s", "EmergencyMinigamePatch");
        }
        /// <summary>
        /// 非導入者にクールダウンを送信する
        /// </summary>
        /// <param name="cooldown">設定CT</param>
        static void SyncSettings(int cooldown)
        {
            if (!PlusGameOptions.EnableFirstEmergencyCooldown) return;
            if (!(AmongUsClient.Instance.AmHost && ModeHandler.IsMode(ModeId.SuperHostRoles))) return;

            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.IsMod() || player.IsBot()) continue;

                IGameOptions optData = SyncSetting.OptionDatas[player].DeepCopy();

                if (optData.GetInt(Int32OptionNames.EmergencyCooldown) == cooldown) continue;

                optData.SetInt(Int32OptionNames.EmergencyCooldown, cooldown);
                optData.RpcSyncOption(player.GetClientId());
                SyncSetting.OptionDatas[player] = optData.DeepCopy();
            }
            Logger.Info($"非導入者の緊急会議クールダウン セット完了(設定CT : {cooldown}s)", "EmergencyMinigamePatch");
        }
    }
}