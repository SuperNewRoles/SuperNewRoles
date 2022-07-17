namespace SuperNewRoles.Roles
{
    class StuntMan_Patch
    {/*
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        class StuntManMurderPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (AmongUsClient.Instance.AmHost && __instance.PlayerId != target.PlayerId)
                {
                    if (target.isRole(RoleId.StuntMan))
                    {
                        if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.StuntmanGuard,__instance)) {
                            if (!RoleClass.StuntMan.GuardCount.ContainsKey(target.PlayerId))
                            {
                                RoleClass.StuntMan.GuardCount[target.PlayerId] = (int)CustomOptions.StuntManMaxGuardCount.GetFloat() - 1;
                                target.RpcProtectPlayer(target, 0);
                            }
                            else
                            {
                                if (!(RoleClass.StuntMan.GuardCount[target.PlayerId] <= 0))
                                {
                                    RoleClass.StuntMan.GuardCount[target.PlayerId]--;
                                    target.RpcProtectPlayer(target, 0);
                                }
                            }
                        }
                    }
                }
            }
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
            }
        }
    */
    }
}