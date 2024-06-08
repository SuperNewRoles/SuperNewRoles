using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Mode.SuperHostRoles;
public static class OneClickShapeshift
{
    public static void OnStartTurn()
    {
        bool isOneClickShapeshiftProcessed = false;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Role.Role != AmongUs.GameOptions.RoleTypes.Shapeshifter)//.GetRoleBase() is not ISHROneClickShape oneClickShape)
                continue;
            FakeShape(player);
            isOneClickShapeshiftProcessed = true;
        }
        if (!isOneClickShapeshiftProcessed)
            return;
    }
    private static void FakeShape(PlayerControl player)
    {
        // シェイプシフト状態にする
        player.RpcShapeshift(PlayerControl.LocalPlayer, false);

        player.RpcSetColor((byte)player.Data.DefaultOutfit.ColorId);
        player.RpcSetHat(player.Data.DefaultOutfit.HatId);
        player.RpcSetSkin(player.Data.DefaultOutfit.SkinId);
        player.RpcSetPet(player.Data.DefaultOutfit.PetId);
        player.RpcSetHat(player.Data.DefaultOutfit.HatId);
        player.RpcSetName(player.Data.PlayerName);
        _ = new LateTask(() =>
        {
            player.RpcSetHat(player.Data.DefaultOutfit.HatId);
            player.RpcSetSkin(player.Data.DefaultOutfit.SkinId);
            player.RpcSetVisor(player.Data.DefaultOutfit.HatId);
        }, 0.3f);
    }
}
