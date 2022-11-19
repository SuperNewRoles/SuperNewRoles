using System.Collections.Generic;
using SuperNewRoles.CustomObject;
using UnityEngine;
using Hazel;
using SuperNewRoles.Buttons;

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
                if (RoleClass.Vulture.Arrow != null && target != null)
                {
                    RoleClass.Vulture.Arrow.Update(target.transform.position, color: RoleClass.Vulture.color);
                }
                RoleClass.Vulture.Arrow.arrow.SetActive(target != null);
            }
        }
        public static void RpcCleanDeadBody(int? count)
        {
            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
            {
                if (collider2D.tag != "DeadBody") continue;

                DeadBody component = collider2D.GetComponent<DeadBody>();
                if (component && !component.Reported)
                {
                    Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                    Vector2 truePosition2 = component.TruePosition;
                    if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance
                        && PlayerControl.LocalPlayer.CanMove
                        && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                    {
                        GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, SendOption.Reliable, -1);
                        writer.Write(playerInfo.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.CleanBody(playerInfo.PlayerId);
                        if (count != null)
                        {
                            count--;
                            Logger.Info($"DeadBodyCount:{count}", "Vulture");
                        }
                        break;
                    }
                }
            }
        }
    }
}