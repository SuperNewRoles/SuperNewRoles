using Hazel;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class CoEnterVent
    {
        public static void Prefix(PlayerPhysics __instance, int id)
        {
            if (!RoleClass.Minimalist.UseVent && __instance.myPlayer.isRole(CustomRPC.RoleId.Minimalist))
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
                writer.WritePacked(id);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
        }
    }
}
