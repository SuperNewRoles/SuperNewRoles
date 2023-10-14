using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SuperNewRoles.Modules;
public static class DefaultNameSaver
{
    //変更前の名前を保存する
    private static Dictionary<int, string> DefaultNames = new();

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
    class CoShowIntroPatch
    {
        public static void Prefix()
        {
            //初期化
            DefaultNames = new();
            //全員分設定していく
            foreach (PlayerControl pc in CachedPlayer.AllPlayers)
            {
                //SuperNewRolesPlugin.Logger.LogInfo($"{pc.PlayerId}:{pc.name}:{pc.NameText().text}");
                DefaultNames[pc.PlayerId] = pc.Data.PlayerName;
                pc.NameText().text = pc.Data.PlayerName;
            }
        }
    }
    public static string GetDefaultName(this PlayerControl player)
    {
        //nullチェック
        if (player == null)
            return string.Empty;
        byte playerid = player.PlayerId;
        string defaultname;
        //デフォルトの名前がないならセットする
        if (!DefaultNames.TryGetValue(playerid, out defaultname))
        {
            DefaultNames[playerid] = defaultname = player.Data.PlayerName;
        }
        return defaultname;
    }
}
