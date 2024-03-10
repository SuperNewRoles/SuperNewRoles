using SuperNewRoles.Mode;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles.Crewmate;
public static class GhostMechanic
{
    // SHR時, 守護天使と同様に会議でクールタイムリセットを行わないようにする為, 会議開始時に現在のクールを保存する。
    public static void MeetingHudStart() =>
        RoleClass.GhostMechanic.KeepCooldown = Buttons.HudManagerStartPatch.GhostMechanicRepairButton.Timer;

    /// <summary>
    /// ボタン表示の条件を取得する。
    /// </summary>
    /// <returns>true : 表示可能 / false : 表示不可</returns>
    public static bool ButtonDisplayCondition()
    {
        bool flag = PlayerControl.LocalPlayer.IsGhostRole(RoleId.GhostMechanic) && RoleClass.GhostMechanic.LimitCount > 0;

        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            flag = flag && ReleaseGhostAbility.FarstTurnHasAlreadyElapsed.Local;
        }

        return flag;
    }

    public static void ResetCool(bool isRespawn = false)
    {
        Buttons.HudManagerStartPatch.GhostMechanicRepairButton.MaxTimer = RoleClass.GhostMechanic.Cooldown;
        Buttons.HudManagerStartPatch.GhostMechanicRepairButton.Timer =
            !isRespawn // 能力使用時のクールリセットなら
                ? RoleClass.GhostMechanic.Cooldown // 設定したクールにリセットする
                : RoleClass.GhostMechanic.MaxLimit == RoleClass.GhostMechanic.LimitCount // 未発動なら
                    ? 0f // 0秒にリセットする
                    : RoleClass.GhostMechanic.KeepCooldown; // 既にAbility使用済みの場合, 会議時に保存したクールでリセットする。
    }

    /// <summary>
    /// 非導入者の, 守護能力置き換えによるリペア処理
    /// </summary>
    /// <param name="ghostMechanic">リペアを使用した亡霊整備士</param>
    /// <returns>
    /// Item1 : アビリティの発動が 成功したか(true) 失敗したか(false),
    /// Item2 : アビリティ残り使用回数,
    /// Item3 : サボタージュ未発生時の発動失敗メッセージ
    /// </returns>
    public static (bool, int, string) UseRepairSHR(PlayerControl ghostMechanic)
    {
        TaskTypes sabotageType = TaskTypes.None;
        bool useAbility = false;
        int limitCount = 0;
        string errorMsg = null;

        if (!PlayerControl.LocalPlayer.IsMushroomMixupActive())
        {
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks) // 亡霊整備士自身のタスクを検索した場合サボタージュを取得できなかった為, ホストのタスクを参照している。
            {
                if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                {
                    sabotageType = task.TaskType;
                    break; // サボタージュが1つ取得された時点で, タスクを取得するforeachから抜ける
                }
            }
        }
        else
        {
            sabotageType = TaskTypes.MushroomMixupSabotage;
        }

        // タスクが取得されていて, 使用回数を使い切っていない場合リペアを発動する。
        if (sabotageType != TaskTypes.None)
        {
            if (RoleClass.GhostMechanic.AbilityUsedCountSHR[ghostMechanic.PlayerId] < RoleClass.GhostMechanic.MaxLimit)
            {
                useAbility = true;
                Sabotage.FixSabotage.RepairProcsee.ReceiptOfSabotageFixing(sabotageType);
                RoleClass.GhostMechanic.AbilityUsedCountSHR[ghostMechanic.PlayerId]++;
                limitCount = RoleClass.GhostMechanic.MaxLimit - RoleClass.GhostMechanic.AbilityUsedCountSHR[ghostMechanic.PlayerId];
            }
        }
        else
        {
            errorMsg = ModTranslation.GetString("GhostMechanicAbilityUseError");
        }

        return (useAbility, limitCount, errorMsg);
    }
}