using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Mode.NotImpostorCheck
{
    class SelectRolePatch
    {
        public static void SetDesync()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsImpostor())
                {
                    SuperNewRolesPlugin.Logger.LogInfo("[NotImpostorCheck] ImpostorName:" + p.NameText().text);
                    Main.Impostors.Add(p.PlayerId);
                }
            }
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (Main.Impostors.Contains(p.PlayerId))
                {
                    if (p.PlayerId != 0)
                    {
                        p.RpcSetRoleDesync(RoleTypes.Impostor);//p.Data.Role.Role);
                        foreach (PlayerControl pc in CachedPlayer.AllPlayers)
                        {
                            if (Main.Impostors.Contains(pc.PlayerId))
                            {
                                p.RpcSetRoleDesync(RoleTypes.Scientist, pc);
                            }
                            else
                            {
                                p.RpcSetRoleDesync(RoleTypes.Impostor, pc);
                            }
                            pc.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            FastDestroyableSingleton<RoleManager>.Instance.SetRole(pc, RoleTypes.Crewmate);
                        }
                    }
                    else
                    {
                        FastDestroyableSingleton<RoleManager>.Instance.SetRole(CachedPlayer.LocalPlayer.PlayerControl, RoleTypes.Impostor);//p.Data.Role.Role);
                        foreach (PlayerControl pc in CachedPlayer.AllPlayers)
                        {
                            if (pc.PlayerId != 0)
                            {
                                if (Main.Impostors.Contains(pc.PlayerId))
                                {
                                    p.RpcSetRoleDesync(RoleTypes.Scientist, pc);
                                }
                                else
                                {
                                    p.RpcSetRoleDesync(RoleTypes.Impostor, pc);
                                }
                                FastDestroyableSingleton<RoleManager>.Instance.SetRole(CachedPlayer.LocalPlayer.PlayerControl, RoleTypes.Crewmate);
                            }
                        }
                    }
                }
                else
                {
                    p.RpcSetRole(p.Data.Role.Role);
                }
            }
        }
    }
}