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
        CustomRpcSender sender = CustomRpcSender.Create("OneClickShapeshiftSender", Hazel.SendOption.Reliable, false);
        // シェイプシフト状態にする
        sender
            .RpcShapeshift(player, PlayerControl.LocalPlayer, false)
            .RpcSetColor(player, (byte)player.Data.DefaultOutfit.ColorId)
            .RpcSetHat(player, player.Data.DefaultOutfit.HatId)
            .RpcSetSkin(player, player.Data.DefaultOutfit.SkinId)
            .RpcSetPet(player, player.Data.DefaultOutfit.PetId)
            .RpcSetVisor(player, player.Data.DefaultOutfit.HatId)
            .RpcSetName(player, player.GetDefaultName());
        ChangeName.SetRoleName(player, sender: sender);
        sender.SendMessage();
        new LateTask(() =>
        {
            sender = CustomRpcSender.Create("OneClickShapeshiftSender", Hazel.SendOption.Reliable, false);
            // シェイプシフト状態にする
            sender
                .RpcSetColor(player, (byte)player.Data.DefaultOutfit.ColorId)
                .RpcSetHat(player, player.Data.DefaultOutfit.HatId)
                .RpcSetSkin(player, player.Data.DefaultOutfit.SkinId)
                .RpcSetPet(player, player.Data.DefaultOutfit.PetId)
                .RpcSetVisor(player, player.Data.DefaultOutfit.HatId)
                .RpcSetName(player, player.GetDefaultName());
            ChangeName.SetRoleName(player, sender: sender);
            sender.SendMessage();
        }, 0.15f);
    }
}
