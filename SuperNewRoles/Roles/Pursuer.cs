using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
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

                foreach (var p in PlayerControl.AllPlayerControls)
                {
                    if (p.isAlive() && !p.Data.Role.IsImpostor)
                    {
                        float target_distance = Vector3.Distance(PlayerControl.LocalPlayer.transform.position, p.transform.position);

                        if (target_distance < min_target_distance)
                        {
                            min_target_distance = target_distance;
                            target = p;
                        }
                    }
                }
                if (target != null)
                {
                    try
                    {
                        RoleClass.Pursuer.arrow.Update(target.transform.position, color: RoleClass.Pursuer.color);
                    }
                    catch(Exception e)
                    {
                        SuperNewRolesPlugin.Logger.LogInfo("error:"+e);
                    }
                }
            }
        }
    }
}
