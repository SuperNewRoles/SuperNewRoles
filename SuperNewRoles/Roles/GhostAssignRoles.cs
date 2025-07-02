using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles;


[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
public class GhostAssignRole
{
    public static int ImpostorGhostRolePlayerNum = 0;
    public static int NeutralGhostRolePlayerNum = 0;
    public static int CrewmateGhostRolePlayerNum = 0;
    public static Dictionary<GhostRoleId, int> GhostRolePlayerNum = new();
    public static void ClearAndReloads()
    {
        ImpostorGhostRolePlayerNum = int.MaxValue;
        NeutralGhostRolePlayerNum = int.MaxValue;
        CrewmateGhostRolePlayerNum = int.MaxValue;
        GhostRolePlayerNum.Clear();
    }
    public static bool Prefix([HarmonyArgument(0)] PlayerControl player, bool specialRolesAllowed)
    {
        ExPlayerControl exPlayer = player;
        if (exPlayer.IsAlive()) return false; //生存者は弾く

        if (GetReleaseHauntAbility(player))
        {
            if (!player.Data.Role.IsImpostor && specialRolesAllowed)
            {
                // TryAssignSpecialGhostRoles
                RoleTypes roleTypes = RoleTypes.GuardianAngel;
                int num = PlayerControl.AllPlayerControls.Count((PlayerControl pc) => pc.Data.IsDead && !pc.Data.Role.IsImpostor);
                IRoleOptionsCollection roleOptions = GameOptionsManager.Instance.CurrentGameOptions.RoleOptions;
                if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                {
                    player.RpcSetRole(roleTypes, true);
                }
                else if (num <= roleOptions.GetNumPerGame(roleTypes))
                {
                    int chancePerGame = roleOptions.GetChancePerGame(roleTypes);
                    if (HashRandom.Next(101) < chancePerGame)
                    {
                        player.RpcSetRole(roleTypes, true);
                    }
                }
            }
            if (!RoleManager.IsGhostRole(player.Data.Role.Role))
                player.RpcSetRole(player.Data.Role.DefaultGhostRole, true);
            return false; // 憑依可能な設定なら
        }
        else
            return false; // 憑依不可能な設定なら
    }

    public static void Postfix([HarmonyArgument(0)] PlayerControl player)
    {
        ExPlayerControl exPlayer = player;
        if (exPlayer.IsAlive() || exPlayer.GhostRole != GhostRoleId.None) return; // 生存者と割り当て済みの人は弾く
        if (player.Data.Role.Role == RoleTypes.GuardianAngel) return; // 守護天使がアサインされていたら, Mod幽霊役職をアサインしない

        bool isAssign = HandleAssign(player);
    }

    /// <summary>
    /// 憑依能力を開放するか判定する
    /// </summary>
    /// <param name="player">判定するプレイヤー</param>
    /// <returns>true : 開放する / false : 開放しない</returns>
    public static bool GetReleaseHauntAbility(ExPlayerControl player)
    {
        if (player.IsAlive()) return false; // 生存している場合は開放しない物として早期return
        return true;
        /*
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
                }*/
    }

    public static bool HandleAssign(ExPlayerControl player)
    {
        var assignTeam = player.IsCrewmate() ? AssignedTeamType.Crewmate : player.IsNeutral() ? AssignedTeamType.Neutral : AssignedTeamType.Impostor;
        List<RoleOptionManager.GhostRoleOption> ghostRoles = new();
        foreach (var opt in RoleOptionManager.GhostRoleOptions)
        {
            var ghostRoleBase = CustomRoleManager.GetGhostRoleById(opt.RoleId);
            if (ghostRoleBase == null) continue;
            if (ghostRoleBase.AssignedTeam != assignTeam) continue;
            ghostRoles.Add(opt);
        }
        var assignrole = Assing(ghostRoles);
        if (assignrole == GhostRoleId.None) return false;
        GhostRolePlayerNum.AddOrUpdate(assignrole, 1);
        switch (assignTeam)
        {
            case AssignedTeamType.Impostor:
                if (ImpostorGhostRolePlayerNum <= 0)
                    return false;
                ImpostorGhostRolePlayerNum--;
                break;
            case AssignedTeamType.Neutral:
                if (NeutralGhostRolePlayerNum <= 0)
                    return false;
                NeutralGhostRolePlayerNum--;
                break;
            case AssignedTeamType.Crewmate:
                if (CrewmateGhostRolePlayerNum <= 0)
                    return false;
                CrewmateGhostRolePlayerNum--;
                break;

        }

        player.RpcCustomSetGhostRoleInGame(assignrole);
        return true;
    }

    //アサインする役職を決める
    public static GhostRoleId Assing(List<RoleOptionManager.GhostRoleOption> ghostRoles)
    {
        List<GhostRoleId> Assigns = new();
        List<GhostRoleId> Assignnos = new();
        foreach (var data in ghostRoles)
        {
            //その役職のプレイヤー数を取得
            var count = data.NumberOfCrews;
            //確率を取得
            var selection = data.Percentage;

            //確率が0%ではないかつ、
            //もう割り当てきられてないか(最大人数まで割り当てられていないか)
            if (selection != 0 && count > GhostRolePlayerNum.GetOrDefault(data.RoleId, 0))
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
        return GhostRoleId.None;
    }
}