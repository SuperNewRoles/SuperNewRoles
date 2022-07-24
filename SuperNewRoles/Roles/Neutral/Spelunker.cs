using System.Linq;
using UnityEngine;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles.Neutral
{
    public static class Spelunker
    {
        //ここにコードを書きこんでください
        public static bool CheckSetRole(PlayerControl player, RoleId role)
        {
            if (player.IsRole(RoleId.Spelunker)) {
                if (role != RoleId.Spelunker)
                {
                    player.RpcMurderPlayer(player);
                    return false;
                }
            }
            return true;
        }
        const float VentDistance = 0.35f;
        public static void FixedUpdate()
        {
            Vent CurrentVent = null;
            if (!MapUtilities.CachedShipStatus.AllVents.ToList().TrueForAll(vent => {
                bool isok = Vector2.Distance(vent.transform.position + new Vector3(0,0.15f,0), CachedPlayer.LocalPlayer.transform.position) < VentDistance;
                if (isok) {
                    CurrentVent = vent;
                }
                return !isok;
            }))
            {
                Logger.Info("キタコレ");
                if (!RoleClass.Spelunker.IsVentChecked) {
                    RoleClass.Spelunker.IsVentChecked = true;
                    if (ModHelpers.IsSucsessChance(RoleClass.Spelunker.VentDeathChance)) PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
                }
            } else
            {
                RoleClass.Spelunker.IsVentChecked = false;
            }
        }
    }
}