using HarmonyLib;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
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
                //生存者と割り当て済みの人は弾く
                if (player.isAlive() || !player.isGhostRole(RoleId.DefaultRole)) return false;
                //幽霊役職がアサインされていたら守護天使をアサインしない
                return !HandleAssign(player);
            }
        }
        public static bool HandleAssign(PlayerControl player)
        {
            //各役職にあったアサインをする
            if (player.isCrew())
            {
                return HandleCrewAssign(player);
            } else if (player.isNeutral())
            {
                return HandleNeutralAssign(player);
            } else
            {
                return HandleImpostorAssign(player);
            }
        }
        //各役職のアサインを
        public static bool HandleCrewAssign(PlayerControl player)
        {
            List<IntroDate> GhostRoles = new List<IntroDate>();
            foreach (IntroDate intro in IntroDate.GhostRoleDatas)
            {
                if (intro.Team != TeamRoleType.Crewmate) continue;
                GhostRoles.Add(intro);
            }
            var assignrole = Assing(GhostRoles);
            if (assignrole == RoleId.DefaultRole) return false;
            player.setRoleRPC(assignrole);
            return true;
        }
        public static bool HandleNeutralAssign(PlayerControl player)
        {
            List<IntroDate> GhostRoles = new List<IntroDate>();
            foreach (IntroDate intro in IntroDate.GhostRoleDatas)
            {
                if (intro.Team != TeamRoleType.Neutral) continue;
                GhostRoles.Add(intro);
            }
            var assignrole = Assing(GhostRoles);
            if (assignrole == RoleId.DefaultRole) return false;
            player.setRoleRPC(assignrole);
            return true;
        }
        public static bool HandleImpostorAssign(PlayerControl player)
        {
            List<IntroDate> GhostRoles = new List<IntroDate>();
            foreach (IntroDate intro in IntroDate.GhostRoleDatas)
            {
                if (intro.Team != TeamRoleType.Impostor) continue;
                GhostRoles.Add(intro);
            }
            var assignrole = Assing(GhostRoles);
            if (assignrole == RoleId.DefaultRole) return false;
            player.setRoleRPC(assignrole);
            return true;
        }

        //アサインする役職を決める
        public static RoleId Assing(List<IntroDate> datas)
        {
            List<RoleId> Assigns = new List<RoleId>();
            List<RoleId> Assignnos = new List<RoleId>();
            foreach (IntroDate data in datas)
            {
                //その役職のプレイヤー数を取得
                var count = AllRoleSetClass.GetPlayerCount(data.RoleId);
                //確率を取得
                var selection = IntroDate.GetOption(data.RoleId).getSelection();
                //確率が0%ではないかつ、
                //もう割り当てきられてないか(最大人数まで割り当てられていないか)
                if (selection != 0 && count > PlayerControl.AllPlayerControls.ToArray().ToList().Count((PlayerControl pc)=> pc.isGhostRole(data.RoleId)))
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
