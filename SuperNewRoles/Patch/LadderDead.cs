using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.CustomOption;
using UnityEngine;

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
            if (ModeHandler.isMode(ModeId.Default))
            {
                if (PlayerControl.LocalPlayer.isDead()) return;
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
                        if (player.isDead()) continue;
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
        class ladder
        {
            public static void Postfix(PlayerPhysics __instance, Ladder source, byte climbLadderSid)
            {
                var sourcepos = source.transform.position;
                var targetpos = source.Destination.transform.position;
                //降りている
                if (sourcepos.y > targetpos.y)
                {
                    //SuperNewRolesPlugin.Logger.LogInfo("降りています");
                    int Chance = UnityEngine.Random.Range(1, 10);
                    //SuperNewRolesPlugin.Logger.LogInfo(aaa);
                    //SuperNewRolesPlugin.Logger.LogInfo(100 - kakuritu);
                    if (Chance <= (CustomOptions.LadderDeadChance.getSelection() + 1))
                    {
                        TargetLadderData[__instance.myPlayer.PlayerId] = targetpos;
                    }
                }
            }
        }
    }
}
