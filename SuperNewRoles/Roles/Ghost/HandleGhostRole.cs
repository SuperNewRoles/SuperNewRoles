using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles;

public class HandleGhostRole
{
    [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
    public class AssignRole
    {
        public static bool Prefix([HarmonyArgument(0)] PlayerControl player)
        {
            if (!ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf, ModeId.SuperHostRoles)) return true; // クラシック以外は弾く
            if (player.IsAlive()) return false; //生存者は弾く

            if (GetReleaseHauntAbility(player)) return true; // 憑依可能な設定なら
            else return false; // 憑依不可能な設定なら
        }

        public static void Postfix([HarmonyArgument(0)] PlayerControl player)
        {
            if (!ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf, ModeId.SuperHostRoles)) return;

            if (player.IsAlive() || !player.IsGhostRole(RoleId.DefaultRole)) return; // 生存者と割り当て済みの人は弾く
            if (player.IsRole(AmongUs.GameOptions.RoleTypes.GuardianAngel)) return; // 守護天使がアサインされていたら, Mod幽霊役職をアサインしない

            bool isAssign = HandleAssign(player);
            if (isAssign && ModeHandler.IsMode(ModeId.SuperHostRoles)) // 幽霊役職が配布された非導入者の役職を守護天使に変更する
            {
                if (!player.IsMod()) player.RpcSetRole(AmongUs.GameOptions.RoleTypes.GuardianAngel);
            }
        }

        /// <summary>
        /// 憑依能力を開放するか判定する
        /// </summary>
        /// <param name="player">判定するプレイヤー</param>
        /// <returns>true : 開放する / false : 開放しない</returns>
        public static bool GetReleaseHauntAbility(PlayerControl player)
        {
            if (player.IsAlive()) return false; // 生存している場合は開放しない物として早期return

            // 無効化しない設定なら早期リターン
            if (!Mode.PlusMode.PlusGameOptions.IsNotGhostHaveHaunt) return true;
            if (player == null || player.IsBot()) return true; // PLCのnullチェック

            if (!Mode.PlusMode.PlusGameOptions.IsReleasingHauntAfterCompleteTasks) return false;
            else // タスク完了後に開放する設定なら, タスク数の確認処理を行う
            {
                bool isCompleteTasks;
                if (player.IsCrew() && !(player.IsMadRoles() || player.IsFriendRoles())) // クルーはタスクが完了次第解放, クルー以外は初期開放
                {
                    var taskdata = TaskCount.TaskDate(player.Data); // タスク状況の取得
                    isCompleteTasks = taskdata.Item1 >= taskdata.Item2; // 全タスクが完了しているなら, true
                }
                else isCompleteTasks = true;

                return isCompleteTasks;
            }
        }
    }

    public static bool HandleAssign(PlayerControl player)
    {
        //各役職にあったアサインをする
        var Team = TeamRoleType.Error;
        Team = player.IsCrew() ? TeamRoleType.Crewmate : player.IsNeutral() ? TeamRoleType.Neutral : TeamRoleType.Impostor;
        List<IntroData> GhostRoles = new();
        foreach (IntroData intro in IntroData.GhostRoleData)
        {
            if (intro.Team != Team) continue;
            GhostRoles.Add(intro);
        }
        var assignrole = Assing(GhostRoles);
        if (assignrole == RoleId.DefaultRole) return false;
        switch (Team)
        {
            case TeamRoleType.Impostor:
                if (AllRoleSetClass.ImpostorGhostRolePlayerNum <= 0)
                    return false;
                AllRoleSetClass.ImpostorGhostRolePlayerNum--;
                break;
            case TeamRoleType.Neutral:
                if (AllRoleSetClass.NeutralGhostRolePlayerNum <= 0)
                    return false;
                AllRoleSetClass.NeutralGhostRolePlayerNum--;
                break;
            case TeamRoleType.Crewmate:
                if (AllRoleSetClass.CrewmateGhostRolePlayerNum <= 0)
                    return false;
                AllRoleSetClass.CrewmateGhostRolePlayerNum--;
                break;

        }

        player.SetRoleRPC(assignrole);
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && !player.IsMod())
            player.RpcSetRole(AmongUs.GameOptions.RoleTypes.GuardianAngel);

        return true;
    }

    //アサインする役職を決める
    public static RoleId Assing(List<IntroData> introData)
    {
        List<RoleId> Assigns = new();
        List<RoleId> Assignnos = new();
        ModeId mode = ModeHandler.GetMode();
        foreach (IntroData data in introData)
        {
            //その役職のプレイヤー数を取得
            var count = AllRoleSetClass.GetPlayerCount(data.RoleId);
            //設定を取得
            var option = IntroData.GetOption(data.RoleId);
            //確率を取得
            var selection = option.GetSelection();

            //確率が0%ではないかつ、
            //もう割り当てきられてないか(最大人数まで割り当てられていないか)
            if ((option.isSHROn || mode != ModeId.SuperHostRoles) && selection != 0 && count > CachedPlayer.AllPlayers.ToArray().ToList().Count((CachedPlayer pc) => pc.PlayerControl.IsGhostRole(data.RoleId)))
            {
                //100%なら100%アサインListに入れる
                if (selection == 10)
                {
                    Assigns.Add(data.RoleId);
                    //100%アサインリストの中身が0だったら処理しない(100%アサインリストのほうがアサインされるため)
                }
                else if (Assigns.Count <= 0)
                {
                    //確率分だけRoleIdを入れる
                    for (int i = 0; i < selection; i++)
                    {
                        Assignnos.Add(data.RoleId);
                    }
                }
            }
        }
        //100%アサインリストの中身が0ではなかったらランダムに選んでアサイン
        if (Assigns.Count > 0)
        {
            return ModHelpers.GetRandom(Assigns);
            //100%ではない、アサインリストの中身が0ではなかったらランダムに選んでアサイン
        }
        else if (Assignnos.Count > 0)
        {
            return ModHelpers.GetRandom(Assignnos);
        }
        //どっちも中身が0だったら通常の役職(DefaultRole)を返す
        return RoleId.DefaultRole;
    }
}
