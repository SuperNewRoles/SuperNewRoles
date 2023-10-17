using Hazel;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Impostor.MadRole;

namespace SuperNewRoles.Mode.SuperHostRoles;

class CoEnterVent
{
    public static bool Prefix(PlayerPhysics __instance, int id)
    {
        if (!AmongUsClient.Instance.AmHost || !ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
        if (__instance.myPlayer.IsUseVent())
            return true;

        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, -1);
        writer.WritePacked(127);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        new LateTask(() =>
        {
            int clientId = __instance.myPlayer.GetClientId();
            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(__instance.NetId, (byte)RpcCalls.BootFromVent, SendOption.Reliable, clientId);
            writer2.Write(id);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);
        }, 0.5f, "Anti Vent");
        return false;
    }
}