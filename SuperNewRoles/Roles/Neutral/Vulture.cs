using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Vulture
{
    // private static readonly List<DeadBody> Targets = new();
    public class FixedUpdate
    {
        public static void Postfix()
        {
            foreach (var arrow in RoleClass.Vulture.DeadPlayerArrows)
            {
                bool isTarget = false;
                foreach (DeadBody dead in Object.FindObjectsOfType<DeadBody>())
                {
                    if (arrow.Key.ParentId != dead.ParentId) continue;
                    isTarget = true;
                    break;
                }
                if (isTarget)
                {
                    if (arrow.Value == null) RoleClass.Vulture.DeadPlayerArrows[arrow.Key] = new(RoleClass.Vulture.color);
                    arrow.Value.Update(arrow.Key.transform.position, color: RoleClass.Vulture.color);
                    arrow.Value.arrow.SetActive(true);
                }
                else
                {
                    if (arrow.Value?.arrow != null)
                        Object.Destroy(arrow.Value.arrow);
                    RoleClass.Vulture.DeadPlayerArrows.Remove(arrow.Key);
                }
            }
            foreach (DeadBody dead in Object.FindObjectsOfType<DeadBody>())
            {
                if (RoleClass.Vulture.DeadPlayerArrows.Any(x => x.Key.ParentId == dead.ParentId)) continue;
                RoleClass.Vulture.DeadPlayerArrows.Add(dead, new(RoleClass.Vulture.color));
                RoleClass.Vulture.DeadPlayerArrows[dead].Update(dead.transform.position, color: RoleClass.Vulture.color);
                RoleClass.Vulture.DeadPlayerArrows[dead].arrow.SetActive(true);
            }
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
                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

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

    public static void EndMeeting()
    {
        ResetCoolDown();
    }

    public static void ResetCoolDown()
    {
        HudManagerStartPatch.VultureButton.MaxTimer = RoleClass.Vulture.CoolTime;
        HudManagerStartPatch.VultureButton.Timer = RoleClass.Vulture.CoolTime;
    }
}