using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Patches;

#region PlayerControlCheckProtectPatch
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckProtect))]
static class CheckProtectPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;

        RoleId angelRole = __instance.GetGhostRole();

        bool useAbility; // アビリティを使用できたか
        (bool, int) limitCount; // Item1 : アビリティに回数制限があるか, Item2 : アビリティ残り使用回数
        string usingErrorMessage = null; // 役職固有の使用不可アナウンスを取得

        (bool, float) coolStatus = GuardianAngelCooldown.GetCooldownStatus(angelRole);
        bool isResetCool = coolStatus.Item1 && 0 < coolStatus.Item2; // クールタイムをリセットするか?
        float resetCooldown = coolStatus.Item2; // クールタイム

        // アビリティは使用可能か (死亡後, 会議を挟んだか)
        bool isAbilityRelease = !isResetCool || (isResetCool && ReleaseGhostAbility.FarstTurnHasAlreadyElapsed[__instance.PlayerId]);

        if (isAbilityRelease) // アビリティが解放されている場合
        {
            switch (angelRole)
            {
                case RoleId.GhostMechanic:
                    var status = GhostMechanic.UseRepairSHR(__instance);
                    useAbility = status.Item1;
                    limitCount = (true, status.Item2);
                    usingErrorMessage = status.Item3;
                    break;
                default:
                    useAbility = false;
                    limitCount = (false, 0);
                    break;
            }
        }
        else
        {
            useAbility = false;
            limitCount = (false, 0);
        }

        if (useAbility && isResetCool) { __instance.RpcShowGuardEffect(__instance); } // クールが存在する時は クールリセットを行う

        string infoName = ModTranslation.GetString($"{angelRole}Name");
        (string text, string colorStr) = AbilityAnnounce(__instance, useAbility, limitCount, usingErrorMessage);
        AddChatPatch.ChatInformation(__instance, infoName, text, colorStr); // アビリティ使用状況のアナウンスを送信する。

        return false; // SHRモードでは通常の守護は出来ないものとする
    }

    /// <summary>
    /// 守護置き換え能力の使用及び発動状況のアナウンスを作成する・
    /// </summary>
    /// <param name="angel">アビリティ使用者</param>
    /// <param name="useAbility">true : 発動成功, false : 発動失敗</param>
    /// <param name="limitCount">Item1 : アビリティに回数制限があるか, Item2 : アビリティ残り使用回数</param>
    /// <param name="usingErrorMessage">特殊アナウンス</param>
    /// <returns>Item1 : 送信するアナウンスの文面, Item2 : アビリティ使用者のロールカラー</returns>
    private static (string, string) AbilityAnnounce(PlayerControl angel, bool useAbility, (bool, int) limitCount, string usingErrorMessage)
    {
        if (angel == null || angel.IsBot()) return (null, null);

        StringBuilder announceBuilder = new();

        RoleId angelRole = angel.GetGhostRole();

        (bool, float) coolStatus = GuardianAngelCooldown.GetCooldownStatus(angelRole);
        bool isResetCool = coolStatus.Item1 && 0 < coolStatus.Item2; // クールタイムをリセットするか?
        float resetCooldown = coolStatus.Item2; // クールタイム

        bool isAbilityRelease = !isResetCool || (isResetCool && ReleaseGhostAbility.FarstTurnHasAlreadyElapsed[angel.PlayerId]);

        if (useAbility) // アビリティが使用できた時の, 通知処理
        {
            announceBuilder.AppendLine(ModTranslation.GetString("GhostRoleAbilitySuccess")); // 能力を使用した旨のアナウンス

            if (isResetCool) // クールが存在する時に行う アナウンスとクールリセット
            {
                announceBuilder.AppendLine(ModTranslation.GetString("GhostRoleAbilityCoolReset"));
            }
            else // クールタイムが無い時のアナウンス
            {
                announceBuilder.AppendLine(ModTranslation.GetString("GhostRoleAbilityNotCoolReset"));
            }

            if (limitCount.Item1) // 回数制限がある時のアナウンス
            {
                announceBuilder.AppendLine(string.Format(ModTranslation.GetString("GhostRoleAbilitylimitCount"), $"{limitCount.Item2}"));
            }
        }
        else // 使用できなかった時の通知処理
        {
            if (angelRole == RoleId.DefaultRole) // 幽霊役職でない為, 守護が使用できない旨の通知
            {
                announceBuilder.AppendLine(ModTranslation.GetString("GhostRoleAbilityMissNotRole"));
            }
            else if (!isAbilityRelease) // 1ターン経過しないとアビリティが使用できない旨の説明
            {
                announceBuilder.AppendLine(ModTranslation.GetString("GhostRoleAbilityMissNotRelease"));
            }
            else if (usingErrorMessage != null) // 役職固有のエラーメッセージ
            {
                announceBuilder.AppendLine(usingErrorMessage);
            }
            else if (limitCount.Item2 == 0) // 回数を使い切っている為に使用できなかった旨の通知
            {
                announceBuilder.AppendLine(ModTranslation.GetString("GhostRoleAbilityMissLimit"));
            }
            else
            {
                announceBuilder.AppendLine(ModTranslation.GetString("GhostRoleAbilityMissConditionNotMet"));
                Logger.Error($"異常な {angel.name}({angelRole}) による守護が呼び出されました。(回数制限 : {limitCount.Item1}, 残り回数 : {limitCount.Item2})", "CheckProtectPatch");
            }
        }

        Color roleColor = angelRole != RoleId.DefaultRole ? CustomRoles.GetRoleColor(angelRole) : CustomRoles.GetRoleColor(angel.GetRole());
        string colorCode = $"#{ModHelpers.ToByte(roleColor.r):X2}{ModHelpers.ToByte(roleColor.g):X2}{ModHelpers.ToByte(roleColor.b):X2}{ModHelpers.ToByte(roleColor.a):X2}";

        return (announceBuilder.ToString(), colorCode);
    }
}
#endregion

#region ReleaseGhostAbility
public static class ReleaseGhostAbility
{
    /// <summary>
    /// 幽霊役職になってから, 1ターン経過したかを保存する。
    /// 正常にクールタイムを反映させるために用いる。
    /// </summary>
    public static PlayerData<bool> FarstTurnHasAlreadyElapsed;

    public static void ClearAndReload() => FarstTurnHasAlreadyElapsed = new();

    /// <summary>
    /// 会議が開かれた時に死亡していたか保存している。
    /// (ReportDeadBodyで保存するのは, クールを送信するタイミングがMeetingHud.Startの為)
    /// </summary>
    public static void MeetingHudStartPostfix()
    {
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player == null || player.IsBot()) continue;
            if (player.IsDead()) FarstTurnHasAlreadyElapsed[player.PlayerId] = true;
        }
    }
}
#endregion

#region GuardianAngelCooldown
public static class GuardianAngelCooldown
{
    /// <summary>
    /// 守護置き換え能力のクールタイムを取得する
    /// </summary>
    /// <param name="role">取得したい幽霊役職</param>
    /// <returns>設定するクールタイム (クールタイムの設定が存在しない, 又は0秒の設定の場合バニラキルクールを返却する)</returns>
    public static float SetCooldown(RoleId role)
    {
        float defaultCooldown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.GuardianAngelCooldown);
        (bool, float) cooldownStatus = GetCooldownStatus(role);

        float cooldown =
            cooldownStatus.Item1 && 0 < cooldownStatus.Item2 // クールタイムが発生するか?
                ? cooldownStatus.Item2 // 発生するなら, 固有設定を取得する
                : defaultCooldown; // 発生しないなら, バニラ設定を取得する

        return cooldown;
    }

    /// <summary>
    /// 幽霊役職の クールタイムの設定を集約する
    /// </summary>
    /// <param name="role">取得したい幽霊役職</param>
    /// <returns>Item1 : クールタイムの設定を有するか, Item2 : クールタイム</returns>
    public static (bool, float) GetCooldownStatus(RoleId role)
    {
        float defaultCooldown = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.GuardianAngelCooldown);

        return role switch
        {
            RoleId.GhostMechanic => (true, RoleClass.GhostMechanic.Cooldown),
            _ => (true, defaultCooldown),
        };
    }
}
#endregion