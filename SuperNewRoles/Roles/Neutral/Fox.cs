using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Fox
    {
        public static void SetPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }

        public static class FoxMurderPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.FoxGuard, __instance))
                {
                    if (!RoleClass.Fox.KillGuard.ContainsKey(target.PlayerId))
                    {
                        target.RpcProtectPlayer(target, 0);
                    }
                    else
                    {
                        if (!(RoleClass.Fox.KillGuard[target.PlayerId] <= 0))
                        {
                            RoleClass.Fox.KillGuard[target.PlayerId]--;
                            target.RpcProtectPlayer(target, 0);
                        }
                    }
                }
            }
        }
    }
}