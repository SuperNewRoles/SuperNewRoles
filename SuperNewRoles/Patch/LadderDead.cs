using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Mode;
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
                    //SuperNewRolesPlugin.Logger.LogInfo("降りています");
                    int Chance = UnityEngine.Random.Range(1, 10);
                    //自殺願望者の処理
                    if (Chance <= (CustomOptions.SuicidalIdeationFallProbability.GetSelection() + 1) && PlayerControl.LocalPlayer.IsRole(RoleId.SuicidalIdeation))
                    {
                        TargetLadderData[__instance.myPlayer.PlayerId] = targetpos;
                    }
                    //自殺願望者以外の処理
                    else if (Chance <= (CustomOptions.LadderDeadChance.GetSelection() + 1))
                    {
                        TargetLadderData[__instance.myPlayer.PlayerId] = targetpos;
                    }
                }
            }
        }
    }
}