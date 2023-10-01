using System.Collections.Generic;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles.Neutral;

public static class PartTimer
{
    public static void FixedUpdate()
    {
        foreach (KeyValuePair<PlayerControl, byte> data in (Dictionary<PlayerControl, byte>)RoleClass.PartTimer.Data)
        {
            PlayerControl value = ModHelpers.PlayerById(data.Value);
            if (!data.Key.IsRole(RoleId.PartTimer))
            {
                RoleClass.PartTimer.Data.Remove(data.Key.PlayerId);
            }
            else if (value.IsDead())
            {
                if (data.Key.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                {
                    RoleClass.PartTimer.DeathTurn = RoleClass.PartTimer.DeathDefaultTurn;
                }
                RoleClass.PartTimer.Data.Remove(data.Key.PlayerId);
            }
        }
    }
    public static void WrapUp()
    {
        if (!PlayerControl.LocalPlayer.IsRole(RoleId.PartTimer)) return;
        if (RoleClass.PartTimer.DeathTurn <= 0 && CachedPlayer.LocalPlayer.IsAlive() && !RoleClass.PartTimer.IsLocalOn)
        {
            PlayerControl.LocalPlayer.RpcExiledUnchecked();
        }
        RoleClass.PartTimer.DeathTurn--;
    }
}