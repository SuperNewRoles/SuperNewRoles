using System.Collections.Generic;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Nekomata
    {
        public static void WrapUp(GameData.PlayerInfo exiled)
        {
            //もし 追放された役職が猫であるならば
            if (exiled.Object.isRole(RoleId.NiceNekomata) || exiled.Object.isRole(RoleId.EvilNekomata) || exiled.Object.isRole(RoleId.BlackCat))
            {
                //道連れにするプレイヤーの抽選リストを作成するクラスに移動する
                NekomataEnd(exiled);
            }
        }
        public static void NekomataEnd(GameData.PlayerInfo exiled)
        {
            List<PlayerControl> p = new();
            foreach (PlayerControl p1 in CachedPlayer.AllPlayers)
            {
                //もし イビル猫又が追放され尚且つImpostorを道連れしない設定がオンになっているにゃら 又は 黒猫が追放され尚且つImpostorを道連れしない設定がオンなっているにゃら
                if ((exiled.Object.isRole(RoleId.EvilNekomata) && RoleClass.EvilNekomata.NotImpostorExiled ) || ( exiled.Object.isRole(RoleId.BlackCat) && RoleClass.BlackCat.NotImpostorExiled))
                {
                    //もし 抜き出されたプレイヤーが　追放されたプレイヤーではない 且つ生きている 且つプレイヤーである(Botでない) 且つインポスターでないにゃら
                    if (p1.Data.PlayerId != exiled.PlayerId && p1.isAlive()  && p1.IsPlayer() && !p1.isImpostor() )
                    {
                        //道連れにするプレイヤーの抽選リストに追加する
                        p.Add(p1);
                        SuperNewRolesPlugin.Logger.LogInfo("[SHR:黒猫Info]Impostorを道連れ対象から除外しました");
                    }
                }
                //それ以外にゃら(ナイス猫又の追放　あるいは　イビル猫又・黒猫の追放でインポスターを道連れにしない設定がオフになっているにゃら)
                else
                {
                    //もし 抜き出されたプレイヤーが　追放されたプレイヤーではない 且つ プレイヤーである 且つ 生きているにゃら
                    if (p1.Data.PlayerId != exiled.PlayerId && p1.isAlive() && p1.IsPlayer())
                    {
                        //道連れにするプレイヤーの抽選リストに追加する
                        p.Add(p1);
                        SuperNewRolesPlugin.Logger.LogInfo("[SHR:黒猫Info]Impostorを道連れ対象から除外しませんでした");
                    }
                }
            }
            NekomataProc(p);
        }
        public static void NekomataProc(List<PlayerControl> p)
        {
            var rdm = ModHelpers.GetRandomIndex(p);
            var random = p[rdm];
            random.RpcCheckExile();
            if ((random.isRole(RoleId.NiceNekomata) || random.isRole(RoleId.EvilNekomata) || random.isRole(RoleId.BlackCat)) && RoleClass.NiceNekomata.IsChain)
            {
                p.RemoveAt(rdm);
                NekomataProc(p);
            }
            Jester.WrapUp(random.Data);
        }
    }
}
