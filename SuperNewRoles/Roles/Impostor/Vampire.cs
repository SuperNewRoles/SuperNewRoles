using System;

namespace SuperNewRoles.Roles
{
    class Vampire
    {
        public static class FixedUpdate
        {
            public static void Postfix()
            {
                if (RoleClass.Vampire.target == null) return;
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)RoleClass.Vampire.KillDelay);
                RoleClass.Vampire.Timer = (float)((RoleClass.Vampire.KillTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                SuperNewRolesPlugin.Logger.LogInfo("ヴァンパイア:" + RoleClass.Vampire.Timer);
                if (RoleClass.Vampire.Timer <= 0.1)
                {
                    RoleClass.Vampire.target.RpcMurderPlayer(RoleClass.Vampire.target);
                    RoleClass.Vampire.target = null;
                }
            }
        }
    }
}