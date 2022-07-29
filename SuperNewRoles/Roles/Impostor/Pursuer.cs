using System;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class Pursuer
    {
        public class PursureUpdate
        {
            public static void Postfix()
            {
                float min_target_distance = float.MaxValue;
                PlayerControl target = null;

                foreach (var p in CachedPlayer.AllPlayers)
                {
                    if (p.PlayerControl.IsAlive() && !p.PlayerControl.IsImpostor())
                    {
                        float target_distance = Vector3.Distance(CachedPlayer.LocalPlayer.transform.position, p.transform.position);

                        if (target_distance < min_target_distance)
                        {
                            min_target_distance = target_distance;
                            target = p;
                        }
                    }
                }
                SuperNewRolesPlugin.Logger.LogInfo("[Pursuer]Target:" + target?.NameText().text);
                if (target != null)
                {
                    try
                    {
                        RoleClass.Pursuer.arrow.Update(target.transform.position, color: RoleClass.Pursuer.color);
                    }
                    catch (Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("[Pursuer]Error:" + e);
                    }
                }
            }
        }
    }
}