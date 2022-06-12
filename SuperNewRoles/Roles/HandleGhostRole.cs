using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperNewRoles.Roles
{
    class HandleGhostRole
    {
        [HarmonyPatch(typeof(RoleManager),nameof(RoleManager.TryAssignRoleOnDeath))]
        class AssignRole
        {
            public static bool Prefix(RoleManager __instance, [HarmonyArgument(0)] PlayerControl player)
            {
                if (!(ModeHandler.isMode(ModeId.Default) || ModeHandler.isMode(ModeId.SuperHostRoles))) return true;
                //生存者と割り当て済みの人は弾く
                if (player.isAlive() || !player.isGhostRole(RoleId.DefaultRole)) return false;
                //幽霊役職がアサインされていたら守護天使をアサインしない
                return !HandleAssign(player);
            }
        }
        public static bool HandleAssign(PlayerControl player)
        {
            //各役職にあったアサインをする
            var Team = TeamRoleType.Error;
            if (player.isCrew())
            {
                Team = TeamRoleType.Crewmate;
            } else if (player.isNeutral())
            {
                Team = TeamRoleType.Neutral;
            } else
            {
                Team = TeamRoleType.Impostor;
            }
            List<IntroDate> GhostRoles = new List<IntroDate>();
            foreach (IntroDate intro in IntroDate.GhostRoleDatas)
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
                    {
                        return false;
                    }
                    AllRoleSetClass.ImpostorGhostRolePlayerNum--;
                    break;
                case TeamRoleType.Neutral:
                    if (AllRoleSetClass.NeutralGhostRolePlayerNum <= 0)
                    {
                        return false;
                    }
                    AllRoleSetClass.NeutralGhostRolePlayerNum--;
                    break;
                case TeamRoleType.Crewmate:
                    if (AllRoleSetClass.CrewMateGhostRolePlayerNum <= 0)
                    {
                        return false;
                    }
                    AllRoleSetClass.CrewMateGhostRolePlayerNum--;
                    break;

            }
            player.setRoleRPC(assignrole);
            return true;
        }

        //アサインする役職を決める
        public static RoleId Assing(List<IntroDate> datas)
        {
            List<RoleId> Assigns = new List<RoleId>();
            List<RoleId> Assignnos = new List<RoleId>();
            ModeId mode = ModeHandler.GetMode();
            foreach (IntroDate data in datas)
            {
                //その役職のプレイヤー数を取得
                var count = AllRoleSetClass.GetPlayerCount(data.RoleId);
                //設定を取得
                var option = IntroDate.GetOption(data.RoleId);
                //確率を取得
                var selection = option.getSelection();

                //確率が0%ではないかつ、
                //もう割り当てきられてないか(最大人数まで割り当てられていないか)
                if ((option.isSHROn || mode != ModeId.SuperHostRoles) && selection != 0 && count > CachedPlayer.AllPlayers.ToArray().ToList().Count((CachedPlayer pc)=> pc.PlayerControl.isGhostRole(data.RoleId)))
                {
                    //100%なら100%アサインListに入れる
                    if (selection == 10)
                    {
                        Assigns.Add(data.RoleId);
                    //100%アサインリストの中身が0だったら処理しない(100%アサインリストのほうがアサインされるため)
                    } else if (Assigns.Count <= 0){
                        //確率分だけRoleIdを入れる
                        for (int i = 0; i < selection;i++) {
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
            } else if (Assignnos.Count > 0)
            {
                return ModHelpers.GetRandom(Assignnos);
            }
            //どっちも中身が0だったら通常の役職(DefaultRole)を返す
            return RoleId.DefaultRole;
        }
    }
}
