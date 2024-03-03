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