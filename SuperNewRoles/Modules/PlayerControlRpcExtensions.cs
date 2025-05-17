using System.Linq;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;
using SuperNewRoles.CustomCosmetics.UI;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class PlayerControlRpcExtensions
{
    [CustomRPC]
    public static void RpcExiledCustom(this ExPlayerControl player)
    {
        player.CustomDeath(CustomDeathType.Exile);
    }
    [CustomRPC]
    public static void RpcCustomSetRole(this ExPlayerControl player, RoleId roleId)
    {
        player.SetRole(roleId);
    }
    [CustomRPC]
    public static void RpcCustomSetRoleInGame(this ExPlayerControl player, RoleId roleId)
    {
        player.SetRole(roleId);
        NameText.UpdateNameInfo(player);
    }
    [CustomRPC]
    public static void RpcCustomSetModifierRole(this ExPlayerControl player, ModifierRoleId modifierRoleId)
    {
        player.SetModifierRole(modifierRoleId);
    }
    [CustomRPC]
    public static void RpcCustomSetModifierRoleInGame(this ExPlayerControl player, ModifierRoleId modifierRoleId)
    {
        player.SetModifierRole(modifierRoleId);
        NameText.UpdateNameInfo(player);
    }
    [CustomRPC]
    public static void RpcCustomSetGhostRoleInGame(this ExPlayerControl player, GhostRoleId ghostRoleId)
    {
        player.SetGhostRole(ghostRoleId);
        NameText.UpdateNameInfo(player);
    }
    [CustomRPC]
    public static void RpcCustomReportDeadBody(this ExPlayerControl player, NetworkedPlayerInfo target)
    {
        player.Player.ReportDeadBody(target);
    }
    [CustomRPC]
    public static void RpcCustomMurderPlayer(this PlayerControl player, PlayerControl target, bool didSucceed)
    {
        player.MurderPlayer(target, didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError);
    }
    [CustomRPC]
    public static void RpcCustomSnapTo(this ExPlayerControl player, Vector2 pos)
    {
        player.NetTransform.SnapTo(pos);
    }
    [CustomRPC]
    public static void RpcCustomSetCosmetics(byte playerId, CostumeTabType menuType, string cosmeticsId, int colorId)
        => PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == playerId)?.CustomSetCosmetics(menuType, cosmeticsId, colorId);
    public static void CustomSetCosmetics(this PlayerControl player, CostumeTabType menuType, string cosmeticsId, int colorId)
    {
        switch (menuType)
        {
            case CostumeTabType.Hat1:
                player.SetHat(cosmeticsId, colorId);
                break;
            case CostumeTabType.Hat2:
                CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics).hat2.SetHat(cosmeticsId, colorId);
                break;
            case CostumeTabType.Visor1:
                player.SetVisor(cosmeticsId, colorId);
                break;
            case CostumeTabType.Visor2:
                CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics).visor2.SetVisor(cosmeticsId, colorId);
                break;
            case CostumeTabType.Skin:
                player.SetSkin(cosmeticsId, colorId);
                break;
        }
    }
    [CustomRPC]
    public static void RpcShapeshiftModded(this PlayerControl shapeshifter, PlayerControl target, bool animate)
    {
        shapeshifter.Shapeshift(target, animate);
    }
}