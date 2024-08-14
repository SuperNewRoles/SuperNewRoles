using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.Helpers;
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
            if (player.GetRoleBase() is not ISHROneClickShape oneClickShape)
                continue;
            FakeShape(player);
            isOneClickShapeshiftProcessed = true;
        }
        if (!isOneClickShapeshiftProcessed)
            return;
    }
    public static void OneClickShaped(PlayerControl player)
    {
        FakeShape(player);
    }
    private static void FakeShape(PlayerControl player)
    {
        player.RpcShapeshift(PlayerControl.LocalPlayer, false);
        player.RpcSetColor((byte)player.Data.DefaultOutfit.ColorId);
        player.RpcSetHat(player.Data.DefaultOutfit.HatId);
        player.RpcSetSkin(player.Data.DefaultOutfit.SkinId);
        player.RpcSetPet(player.Data.DefaultOutfit.PetId);
        player.RpcSetVisor(player.Data.DefaultOutfit.HatId);
        player.RpcSetName(player.GetDefaultName());
        player.SetName(player.GetDefaultName());

        NetworkedPlayerInfo.PlayerOutfit defaultOutfit = player.Data.DefaultOutfit;
        player.RawSetName(defaultOutfit.PlayerName);
        player.RawSetColor(defaultOutfit.ColorId);
        player.RawSetHat(defaultOutfit.HatId, defaultOutfit.ColorId);
        player.RawSetSkin(defaultOutfit.SkinId, defaultOutfit.ColorId);
        player.RawSetVisor(defaultOutfit.VisorId, defaultOutfit.ColorId);
        player.RawSetPet(defaultOutfit.PetId, defaultOutfit.ColorId);
        player.CurrentOutfitType = PlayerOutfitType.Default;

        ChangeName.UpdateRoleName(player, ChangeNameType.SelfOnly);
        new LateTask(() =>
        {
            ChangeName.UpdateRoleName(player, ChangeNameType.SelfOnly);
            player.RpcSetColor((byte)player.Data.DefaultOutfit.ColorId);
            player.RpcSetHat(player.Data.DefaultOutfit.HatId);
            player.RpcSetSkin(player.Data.DefaultOutfit.SkinId);
            player.RpcSetPet(player.Data.DefaultOutfit.PetId);
            player.RpcSetVisor(player.Data.DefaultOutfit.HatId);
            player.RpcSetName(player.GetDefaultName());
            player.SetName(player.GetDefaultName());
            new LateTask(() => ChangeName.UpdateRoleName(player, ChangeNameType.SelfOnly), 0.15f);
        }, 0.2f);
    }
}
