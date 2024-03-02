using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Linq;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.PlusMode;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Patches;

class EmergencyMinigamePatch
{
    /// <summary>
    /// 緊急会議クールダウンを[死者が出るまで緊急ボタンクールダウンを変更する]設定に従い, 変更する
    /// [DeadPlayer.deadPlayers] に要素がある時, 死者が出たと判定する
    /// </summary>
    /// <param name="IsPlayerExpelled">追放者がいるか(deadPlayersに追放者の情報が保存される前に実行し判定から漏れる為, 補正する)</param>
    public static void SetFirstEmergencyCooldown(bool IsPlayerExpelled = false)
    {
        if (!PlusGameOptions.PlusGameOptionSetting.GetBool()) return;
        if (!(PlusGameOptions.ReportDeadBodySetting.GetBool() && PlusGameOptions.HaveFirstEmergencyCooldownSetting.GetBool())) return;

        int cooldown = (DeadPlayer.deadPlayers.Count > 0 || IsPlayerExpelled)
            ? GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.EmergencyCooldown)
            : PlusGameOptions.FirstEmergencyCooldownSetting.GetInt();

        var optData = SyncSetting.DefaultOption.DeepCopy();
        optData.SetInt(Int32OptionNames.EmergencyCooldown, cooldown);
        GameManager.Instance.LogicOptions.SetGameOptions(optData);

        ShipStatus.Instance.EmergencyCooldown = cooldown;

        Logger.Info($"緊急会議クールダウン セット : {cooldown}s", "EmergencyMinigamePatch");
    }
}