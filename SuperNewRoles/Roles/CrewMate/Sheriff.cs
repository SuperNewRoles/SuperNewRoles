using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles;

class Sheriff
{
    public static void ResetKillCooldown()
    {
        try
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
            {
                HudManagerStartPatch.SheriffKillButton.MaxTimer = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 0f : RoleClass.RemoteSheriff.CoolTime;
                HudManagerStartPatch.SheriffKillButton.Timer = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 0f : RoleClass.RemoteSheriff.CoolTime;
                RoleClass.Sheriff.ButtonTimer = DateTime.Now;
            }
            else
            {
                if (HudManagerStartPatch.SheriffKillButton != null)
                    HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.Chief.SheriffPlayer.Contains(CachedPlayer.LocalPlayer.PlayerId)
                        ? RoleClass.Chief.CoolTime
                        : RoleClass.Sheriff.CoolTime;
                HudManagerStartPatch.SheriffKillButton.Timer = HudManagerStartPatch.SheriffKillButton.MaxTimer;
            }
        }
        catch { }
    }

    /// <summary>
    /// シェリフキルの結果を取得する
    /// </summary>
    /// <param name="sheriff">キルを行うシェリフ</param>
    /// <param name="target">シェリフキルの対象</param>
    /// <returns>
    /// [ Item1 : ターゲットの状態 ] => Item1(bool) : ターゲットをキル可能か / Item2(FinalStatus) : ターゲットの死因,
    /// [ Item2 : シェリフの状態 ] => Item1(bool) : シェリフが自殺したか / Item2(FinalStatus) : シェリフの死因,
    /// </returns>
    public static ((bool, FinalStatus), (bool, FinalStatus)) SheriffKillResult(PlayerControl sheriff, PlayerControl target)
    {
        SheriffRoleExecutionData sheriffData = new(sheriff);
        bool isJudgment = IsSheriffJudgment(target, sheriffData);

        (bool isTargetKill, FinalStatus targetDeadStatus) killResult = IsSheriffKill(target, isJudgment, sheriffData);
        (bool isSuicide, FinalStatus sheriffDeadStatus) suicideResult = IsSheriffSuicide(isJudgment, sheriffData);

        return (killResult, suicideResult);
    }

    /// <summary>
    /// シェリフ考査判定
    /// </summary>
    /// <param name="target">キル対象</param>
    /// <param name="data">シェリフ執行関係のデータ</param>
    /// <returns>true : 正義執行 / false : 誤射 </returns>
    private static bool IsSheriffJudgment(PlayerControl target, SheriffRoleExecutionData data)
    {
        if (CountChanger.GetRoleType(target) == TeamRoleType.Impostor) return data.IsImpostorKill; // インポスターは, インポスターキル可能設定が有効な時切れる
        if (target.IsMadRoles() || target.IsRole(RoleId.MadKiller, RoleId.Dependents)) return data.IsMadRolesKill;
        if (target.IsNeutral()) return data.IsNeutralKill;
        if (target.IsFriendRoles()) return data.IsFriendRolesKill;
        if (target.IsLovers()) return data.IsLoversKill;
        if (target.IsQuarreled()) return data.IsQuarreledKill;
        if (target.IsHauntedWolf()) return data.IsImpostorKill; // 狼付きは, インポスターキル可能設定が有効な時切れる

        return false;
    }

    /// <summary>
    /// シェリフのキル判定と死因判定
    /// </summary>
    /// <param name="target">キル対象</param>
    /// <param name="isJudgment">true : 正義執行 / false : 誤射</param>
    /// <param name="data">シェリフ執行関係のデータ</param>
    /// <returns>
    /// Item1 => true : 対象をキルする / false : 対象をキルしない,
    /// Item2 => 対象の死因
    /// </returns>
    private static (bool, FinalStatus) IsSheriffKill(PlayerControl target, bool isJudgment, SheriffRoleExecutionData data)
    {
        (bool isTargetKill, FinalStatus targetDeadReason) killData = (false, FinalStatus.Alive);

        FinalStatus statusSuccess = data.IsHauntedWolfDecision ? FinalStatus.HauntedSheriffKill : FinalStatus.SheriffKill;

        killData.isTargetKill = isJudgment; // 判定成功 又は 判定失敗時に[誤射時も対象を殺す]設定が有効な場合, 対象をキル可能として返す
        killData.targetDeadReason = isJudgment ? statusSuccess : FinalStatus.Alive;

        bool isAlwaysKill = data.Mode == SheriffRoleExecutionData.ExecutionMode.AlwaysKill;

        if (isJudgment) // 判定成功時
        {
            killData.isTargetKill = true;
            killData.targetDeadReason = statusSuccess;

            // 対象の死因が特殊なキルの死因上書き処理
            if (target.IsHauntedWolf()) killData.targetDeadReason = FinalStatus.SheriffHauntedWolfKill;
        }
        else if (!isJudgment && isAlwaysKill) // 判定失敗時, [誤射時も対象を殺す]設定が有効な場合
        {
            killData.isTargetKill = true;
            killData.targetDeadReason = FinalStatus.SheriffInvolvedOutburst;
        }

        return killData;
    }

    /// <summary>
    /// シェリフの自殺判定と死因判定
    /// </summary>
    /// <param name="isJudgment">true : 正義執行 / false : 誤射</param>
    /// <param name="data">シェリフ執行関係のデータ</param>
    /// <returns>
    /// Item1 => true : シェリフ自殺 / false : シェリフ生存,
    /// Item2 => シェリフの死因
    /// </returns>
    private static (bool, FinalStatus) IsSheriffSuicide(bool isJudgment, SheriffRoleExecutionData data)
    {
        if (isJudgment) // 執行成功時
        {
            return data.Mode switch
            {
                SheriffRoleExecutionData.ExecutionMode.AlwaysSuicide => (true, !data.IsHauntedWolfDecision ? FinalStatus.SheriffSuicide : FinalStatus.HauntedSheriffSuicide),
                _ => (false, FinalStatus.Alive)
            };
        }
        else // 誤射時
        {
            return (true, !data.IsHauntedWolfDecision ? FinalStatus.SheriffMisFire : FinalStatus.HauntedSheriffMisFire);
        }
    }

    public static bool IsSheriff(PlayerControl Player)
    {
        return Player.IsRole(RoleId.Sheriff) || Player.IsRole(RoleId.RemoteSheriff);
    }
    public static bool IsSheriffButton(PlayerControl Player)
    {
        if (Player.IsRole(RoleId.Sheriff))
        {
            if (RoleClass.Sheriff.KillMaxCount > 0)
            {
                return true;
            }
        }
        else if (Player.IsRole(RoleId.RemoteSheriff))
        {
            if (RoleClass.RemoteSheriff.KillMaxCount > 0)
            {
                return true;
            }
        }
        return false;
    }
    public static void EndMeeting()
    {
        ResetKillCooldown();
    }
    class SheriffRoleExecutionData
    {
        /// <summary>執行挙動のモード</summary>
        public ExecutionMode Mode;

        /// <summary>狼憑きによる判定反転が有効か</summary>
        public bool IsHauntedWolfDecision;

        public bool IsImpostorKill;
        public bool IsMadRolesKill;
        public bool IsNeutralKill;
        public bool IsFriendRolesKill;
        public bool IsLoversKill;
        public bool IsQuarreledKill;

        public enum ExecutionMode
        {
            Default, // 通常 (成功時のみ対象を殺害, 誤射時のみ自殺)
            AlwaysSuicide, // 常に自殺する
            AlwaysKill // 誤射時も対象を殺す
        }


        public SheriffRoleExecutionData(PlayerControl sheriff)
        {
            ExecutionMode mode = ExecutionMode.Default;

            var isImpostorKill = true;
            var isMadRolesKill = false;
            var isNeutralKill = false;
            var isFriendRolesKill = false;
            var isLoversKill = false;
            var isQuarreledKill = false;

            RoleId role = sheriff.GetRole();

            switch (role)
            {
                case RoleId.Sheriff:
                    // 通常Sheriffの場合
                    if (!RoleClass.Chief.SheriffPlayer.Contains(sheriff.PlayerId))
                    {
                        mode = (ExecutionMode)CustomOptionHolder.SheriffExecutionMode.GetSelection();
                        isImpostorKill = CustomOptionHolder.SheriffCanKillImpostor.GetBool();
                        isMadRolesKill = CustomOptionHolder.SheriffMadRoleKill.GetBool(); ;
                        isNeutralKill = CustomOptionHolder.SheriffNeutralKill.GetBool();
                        isFriendRolesKill = CustomOptionHolder.SheriffFriendsRoleKill.GetBool();
                        isLoversKill = CustomOptionHolder.SheriffLoversKill.GetBool();
                        isQuarreledKill = CustomOptionHolder.SheriffQuarreledKill.GetBool();
                    }
                    else // 村長シェリフの場合
                    {
                        mode = (ExecutionMode)CustomOptionHolder.ChiefSheriffExecutionMode.GetSelection();
                        isImpostorKill = CustomOptionHolder.ChiefSheriffCanKillImpostor.GetBool();
                        isMadRolesKill = CustomOptionHolder.ChiefSheriffCanKillMadRole.GetBool();
                        isNeutralKill = CustomOptionHolder.ChiefSheriffCanKillNeutral.GetBool();
                        isFriendRolesKill = CustomOptionHolder.ChiefSheriffFriendsRoleKill.GetBool();
                        isLoversKill = CustomOptionHolder.ChiefSheriffCanKillLovers.GetBool();
                        isQuarreledKill = CustomOptionHolder.ChiefSheriffQuarreledKill.GetBool();
                    }
                    break;
                case RoleId.RemoteSheriff:
                    mode = (ExecutionMode)CustomOptionHolder.RemoteSheriffExecutionMode.GetSelection();
                    isMadRolesKill = CustomOptionHolder.RemoteSheriffMadRoleKill.GetBool();
                    isNeutralKill = CustomOptionHolder.RemoteSheriffNeutralKill.GetBool();
                    isFriendRolesKill = CustomOptionHolder.RemoteSheriffFriendRolesKill.GetBool();
                    isLoversKill = CustomOptionHolder.RemoteSheriffLoversKill.GetBool();
                    isQuarreledKill = CustomOptionHolder.RemoteSheriffQuarreledKill.GetBool();
                    break;
                case RoleId.MeetingSheriff:
                    mode = (ExecutionMode)CustomOptionHolder.MeetingSheriffExecutionMode.GetSelection();
                    isMadRolesKill = CustomOptionHolder.MeetingSheriffMadRoleKill.GetBool();
                    isNeutralKill = CustomOptionHolder.MeetingSheriffNeutralKill.GetBool();
                    isFriendRolesKill = CustomOptionHolder.MeetingSheriffFriendsRoleKill.GetBool();
                    isLoversKill = CustomOptionHolder.MeetingSheriffLoversKill.GetBool();
                    isQuarreledKill = CustomOptionHolder.MeetingSheriffQuarreledKill.GetBool();
                    break;
            }

            IsHauntedWolfDecision = sheriff.IsHauntedWolf() && Attribute.HauntedWolf.CustomOptionData.IsReverseSheriffDecision.GetBool();

            // シェリフが狼憑きであり設定が有効なら, キル判定を反転する
            if (IsHauntedWolfDecision)
            {
                isImpostorKill ^= true;
                isMadRolesKill ^= true;
                isNeutralKill ^= true;
                isFriendRolesKill ^= true;
                isLoversKill ^= true;
                isQuarreledKill ^= true;
            }

            Mode = mode;

            IsImpostorKill = isImpostorKill;
            IsMadRolesKill = isMadRolesKill;
            IsNeutralKill = isNeutralKill;
            IsFriendRolesKill = isFriendRolesKill;
            IsLoversKill = isLoversKill;
            IsQuarreledKill = isQuarreledKill;
        }
    }
}