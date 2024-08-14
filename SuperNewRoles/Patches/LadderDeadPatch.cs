using System.Collections.Generic;
using HarmonyLib;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Patches;
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
            if (!TargetLadderData.TryGetValue(CachedPlayer.LocalPlayer.PlayerId, out Vector3 pos)) return;
            if (Vector2.Distance(pos, CachedPlayer.LocalPlayer.transform.position) >= 0.5f) return;
            if (!PlayerControl.LocalPlayer.moveable) return;
            PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
            PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.LadderDeath);
        }
        else if (AmongUsClient.Instance.AmHost)
        {
            foreach (var data in TargetLadderData)
            {
                PlayerControl player = ModHelpers.PlayerById(data.Key);
                if (player.IsDead()) continue;
                if (Vector2.Distance(data.Value, player.transform.position) >= 0.5f) continue;
                player.Data.IsDead = true;
                new LateTask(() =>
                {
                    player.RpcMurderPlayer(player, true);
                    player.RpcSetFinalStatus(FinalStatus.LadderDeath);
                }, 0.05f, "Ladder Murder");
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
                if (ModeHandler.IsMode(ModeId.VanillaHns) && __instance.myPlayer.IsImpostor()) return;
                if (!((Mode.PlusMode.PlusGameOptions.PlusGameOptionSetting.GetBool() && ModHelpers.IsSuccessChance(Mode.PlusMode.PlusGameOptions.LadderDeadChance.GetSelection() + 1) && Mode.PlusMode.PlusGameOptions.LadderDead.GetBool()) ||
                    (__instance.myPlayer.IsRole(RoleId.SuicidalIdeation) && ModHelpers.IsSuccessChance(CustomOptionHolder.SuicidalIdeationFallProbability.GetSelection() + 1)) ||
                    (__instance.myPlayer.IsRole(RoleId.Spelunker) && ModHelpers.IsSuccessChance(RoleClass.Spelunker.LadderDeathChance))
                    ))
                {
                    return;
                }
                TargetLadderData[__instance.myPlayer.PlayerId] = targetpos;
            }
        }
    }
}