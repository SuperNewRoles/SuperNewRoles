using System.Collections.Generic;
using SuperNewRoles.CustomObject;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class Vulture
    {
        // private static readonly List<DeadBody> Targets = new();
        public class FixedUpdate
        {
            public static void Postfix()
            {
                if (RoleClass.Vulture.Arrow == null)
                {
                    Arrow arrow = new(RoleClass.Vulture.color);
                    arrow.arrow.SetActive(true);
                    RoleClass.Vulture.Arrow = arrow;
                }
                float min_target_distance = float.MaxValue;
                DeadBody target = null;
                DeadBody[] deadBodies = Object.FindObjectsOfType<DeadBody>();
                foreach (DeadBody db in deadBodies)
                {
                    if (db == null)
                    {
                        RoleClass.Vulture.Arrow.arrow.SetActive(false);
                    }
                    float target_distance = Vector3.Distance(CachedPlayer.LocalPlayer.transform.position, db.transform.position);

                    if (target_distance < min_target_distance)
                    {
                        min_target_distance = target_distance;
                        target = db;
                    }
                }
                if (RoleClass.Vulture.Arrow != null)
                {
                    RoleClass.Vulture.Arrow.Update(target.transform.position, color: RoleClass.Vulture.color);
                }
                if (!PlayerControl.LocalPlayer.IsAlive())
                {
                    Object.Destroy(RoleClass.Vulture.Arrow.arrow);
                    return;
                }
            }
        }
    }
}