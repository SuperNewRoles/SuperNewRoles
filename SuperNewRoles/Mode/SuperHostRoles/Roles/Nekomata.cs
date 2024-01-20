using System.Collections.Generic;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles;

class Nekomata
{
    public static void WrapUp(GameData.PlayerInfo exiled)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        //もし 追放された役職が猫であるならば
        if (exiled.Object.IsRole(RoleId.NiceNekomata) || exiled.Object.IsRole(RoleId.EvilNekomata) || exiled.Object.IsRole(RoleId.BlackCat))
        {
            NekomataEnd(exiled);//道連れにするプレイヤーの抽選リストを作成するクラスに移動する
        }
    }
    public static void NekomataEnd(GameData.PlayerInfo exiled)
    {
        List<PlayerControl> p = new();
        foreach (PlayerControl p1 in CachedPlayer.AllPlayers)
        {
            //もし イビル猫又・黒猫が追放され、Impostorを道連れしないがオンなら
            if ((exiled.Object.IsRole(RoleId.EvilNekomata) && RoleClass.EvilNekomata.NotImpostorExiled) || (exiled.Object.IsRole(RoleId.BlackCat) && RoleClass.BlackCat.NotImpostorExiled))
            {
                //もし 抜き出されたプレイヤーが　追放されたプレイヤーではない 生きている ボットでない インポスターでないなら
                if (p1.Data.PlayerId != exiled.PlayerId && p1.IsAlive() && !p1.IsBot() && !p1.IsImpostor())
                {
                    p.Add(p1);//道連れにするプレイヤーの抽選リストに追加する

                    //Logへの記載
                    if (exiled.Object.IsRole(RoleId.BlackCat)) SuperNewRolesPlugin.Logger.LogInfo("[SHR:黒猫Info]Impostorを道連れ対象から除外しました");
                    else if (exiled.Object.IsRole(RoleId.EvilNekomata)) SuperNewRolesPlugin.Logger.LogInfo("[SHR:イビル猫又Info]Impostorを道連れ対象から除外しました");
                    else SuperNewRolesPlugin.Logger.LogError("[SHR:猫又Error]&[SHR:イビル猫又Error][NotImpostorExiled == true] 異常な抽選リストです");
                }
            }
            //それ以外なら(ナイス猫又・設定オフ)
            else
            {
                if (p1.Data.PlayerId != exiled.PlayerId && p1.IsAlive() && !p1.IsBot())
                {
                    p.Add(p1); //道連れにするプレイヤーの抽選リストに追加する

                    //Logへの記載
                    if (exiled.Object.IsRole(RoleId.BlackCat)) SuperNewRolesPlugin.Logger.LogInfo("[SHR:黒猫Info]Impostorを道連れ対象から除外しませんでした");
                    else if (exiled.Object.IsRole(RoleId.EvilNekomata)) SuperNewRolesPlugin.Logger.LogInfo("[SHR:イビル猫又Info]Impostorを道連れ対象から除外しませんでした");
                    else SuperNewRolesPlugin.Logger.LogError("[SHR:猫又Error]&[SHR:イビル猫又Error][NotImpostorExiled != true ] 異常な抽選リストです");
                }
            }
        }
        NekomataProc(p);
    }
    public static void NekomataProc(List<PlayerControl> p)
    {
        if (p.Count <= 0) { Logger.Info("抽選リストにプレイヤーが存在しない為, 猫又の道連れ処理を終了しました。", "Nekomata Exiled (SHR)"); return; }

        var rdm = ModHelpers.GetRandomIndex(p);
        var random = p[rdm];
        random.RpcCheckExile();
        if (RoleClass.NiceNekomata.IsChain &&
            (random.IsRole(RoleId.NiceNekomata) || random.IsRole(RoleId.EvilNekomata) || random.IsRole(RoleId.BlackCat)))
        {
            p.RemoveAt(rdm);
            NekomataProc(p);
        }
        Jester.WrapUp(random.Data);
    }
}