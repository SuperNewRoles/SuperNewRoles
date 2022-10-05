using Hazel;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class CoEnterVent
    {
        public static bool Prefix(PlayerPhysics __instance, int id)
        {
            if (!AmongUsClient.Instance.AmHost || !ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
            RoleId role = __instance.myPlayer.GetRole();
            switch (role)
            {
                case RoleId.Minimalist:
                    if (RoleClass.Minimalist.UseVent) return true;
                    break;
                case RoleId.Egoist:
                    if (RoleClass.Egoist.UseVent) return true;
                    break;
                case RoleId.Demon:
                    if (RoleClass.Demon.IsUseVent) return true;
                    break;
                case RoleId.Arsonist:
                    if (RoleClass.Arsonist.IsUseVent) return true;
                    break;
                case RoleId.Jackal:
                    if (RoleClass.Jackal.IsUseVent) return true;
                    break;
                case RoleId.MadMaker:
                    if (RoleClass.MadMaker.IsUseVent) return true;
                    break;
                case RoleId.Technician:
                    if (RoleHelpers.IsSabotage()) return true;
                    break;
                case RoleId.Samurai:
                    if (RoleClass.Samurai.UseVent) return true;
                    break;
                case RoleId.MadSeer:
                    if (RoleClass.MadSeer.IsUseVent) return true;
                    break;
                case RoleId.SeerFriends:
                    if (RoleClass.SeerFriends.IsUseVent) return true;
                    break;
                case RoleId.RemoteSheriff:
                case RoleId.Sheriff:
                case RoleId.truelover:
                case RoleId.FalseCharges:
                case RoleId.ToiletFan:
                case RoleId.NiceButtoner:
                    break;
                default:
                    return true;
            }
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
}