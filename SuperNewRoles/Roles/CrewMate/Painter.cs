using System;
using System.Collections.Generic;
using System.Text;
using Hazel;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles.CrewMate
{
    public static class Painter
    {
        //ここにコードを書きこんでください
        public enum ActionType
        {
            TaskClear,
            Death
        }
        public static void SetTarget(PlayerControl Target)
        {
            bool Is = true;
            if (Target == null)
            {
                Target = RoleClass.Painter.CurrentTarget;
                RoleClass.Painter.CurrentTarget = null;
                Is = false;
            } else
            {
                RoleClass.Painter.CurrentTarget = Target;
            }
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.PainterSetTarget);
            writer.Write(Target.PlayerId);
            writer.Write(Is);
            writer.EndRPC();
            CustomRPC.RPCProcedure.PainterSetTarget(Target.PlayerId, Is);
        }
        public static void WrapUp()
        {
            if (RoleClass.Painter.CurrentTarget != null)
            {
                SetTarget(null);
            }
        }
        public static void Handle(ActionType type)
        {
            if (RoleClass.Painter.IsLocalActionSend && (type == ActionType.Death || PlayerControl.LocalPlayer.IsAlive()))
            {
                var pos = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 2];
                Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.PainterPaintSet);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write((byte)type);
                writer.WriteBytesAndSize(buff);
                writer.EndRPC();
                CustomRPC.RPCProcedure.PainterPaintSet(CachedPlayer.LocalPlayer.PlayerId, (byte)type, buff);
            }
        }
    }
}