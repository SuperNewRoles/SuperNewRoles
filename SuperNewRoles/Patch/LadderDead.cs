using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using UnityEngine;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Patch
{
    public static class LadderDead
    {
        public static void Reset()
        {
            TargetLadderData = new();
        }
        public static void FixedUpdate()
        {
            if (ModeHandler.IsMode(ModeId.Default))
            {
                if (PlayerControl.LocalPlayer.IsDead()) return;
                if (TargetLadderData.ContainsKey(CachedPlayer.LocalPlayer.PlayerId))
                {
                    if (Vector2.Distance(TargetLadderData[CachedPlayer.LocalPlayer.PlayerId], CachedPlayer.LocalPlayer.transform.position) < 0.5f)
                    {
                        if (PlayerControl.LocalPlayer.moveable)
                        {
                            PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
                        }
                    }
                }
            }
            else
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    foreach (var data in TargetLadderData)
                    {
                        PlayerControl player = ModHelpers.playerById(data.Key);
                        if (player.IsDead()) continue;
                        if (Vector2.Distance(data.Value, player.transform.position) < 0.5f)
                        {
                            player.Data.IsDead = true;
                            new LateTask(() => player.RpcMurderPlayer(player), 0.05f);
                        }
                    }
                }
            }
        }
        public static Dictionary<byte, Vector3> TargetLadderData;
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ClimbLadder))]
        class Ladders
        {
            public static void Postfix(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
            {
                var sourcepos = source.transform.position;
                var targetpos = source.Destination.transform.position;
                //降りている
                if (sourcepos.y > targetpos.y)
                {
                    if (!(ModHelpers.IsSucsessChance(CustomOptions.LadderDeadChance.GetSelection() + 1) ||
                        (__instance.myPlayer.IsRole(RoleId.SuicidalIdeation) && ModHelpers.IsSucsessChance(CustomOptions.SuicidalIdeationFallProbability.GetSelection() + 1)) || 
                        (__instance.myPlayer.IsRole(RoleId.Spelunker) && ModHelpers.IsSucsessChance(RoleClass.Spelunker.LadderDeathChance))
                        ))
                    {
                        return;
                    }
                    TargetLadderData[__instance.myPlayer.PlayerId] = targetpos;
                }
            }
        }
    }
}