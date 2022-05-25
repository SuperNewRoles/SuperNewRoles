using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    class SeerFriends
    {
        public static List<byte> CheckedJackal;
        public static bool CheckJackal(PlayerControl p)
        {
            if (!RoleClass.MadSeer.IsImpostorCheck) return false;
            if (!p.isRole(RoleId.MadSeer)) return false;
            if (CheckedJackal.Contains(p.PlayerId)) return true;
            /*
            SuperNewRolesPlugin.Logger.LogInfo("インポスターチェックタスク量:"+RoleClass.MadSeer.ImpostorCheckTask);
            SuperNewRolesPlugin.Logger.LogInfo("終了タスク量:"+TaskCount.TaskDate(p.Data).Item1);*/
            SuperNewRolesPlugin.Logger.LogInfo("有効か:" + (RoleClass.MadSeer.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1));
            if (RoleClass.MadSeer.ImpostorCheckTask <= TaskCount.TaskDate(p.Data).Item1)
            {
                SuperNewRolesPlugin.Logger.LogInfo("有効を返しました");
                return true;
            }
            // SuperNewRolesPlugin.Logger.LogInfo("一番下まで通過");
            return false;
        }
    }
}

//今の作業で狂信化できたわけではない
//狂信設定はジャッカルフレンズにあるがそれを書き込めるようにした
//今回は基盤がなかった為MadSeerから持ってきてImpostorをJackalに書き換えたが、
//SeerFriendsが基盤になる為それはしなくてよい
//ただSeerFriendsを役職名Friendsにするだけでよい

//取りあえずここでイントロ画面(翻訳無し)は完成した
//(正確に言えば此処の書き込みは関係ないが此処を書かないとエラーが消えなかった為先に書いた。)