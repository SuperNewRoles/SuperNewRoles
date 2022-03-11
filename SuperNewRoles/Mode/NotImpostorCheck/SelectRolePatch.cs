using SuperNewRoles.Mode.SuperHostRoles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.NotImpostorCheck
{
    class SelectRolePatch
    {
        public static void SetDesync()
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.isImpostor())
                {
                    SuperNewRolesPlugin.Logger.LogInfo("ImpostorName:"+p.nameText.text);
                    main.Impostors.Add(p.PlayerId);
                }
            }
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (main.Impostors.Contains(p.PlayerId))
                {
                    if (p.PlayerId != 0)
                    {
                        p.RpcSetRoleDesync(RoleTypes.Impostor);//p.Data.Role.Role);
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (main.Impostors.Contains(pc.PlayerId))
                            {
                                p.RpcSetRoleDesync(RoleTypes.Scientist, pc);
                            }
                            else
                            {
                                p.RpcSetRoleDesync(RoleTypes.Impostor, pc);
                            }
                            pc.RpcSetRoleDesync(RoleTypes.Scientist, p);
                            DestroyableSingleton<RoleManager>.Instance.SetRole(pc,RoleTypes.Crewmate);
                        }
                    }
                    else
                    {
                        DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);//p.Data.Role.Role);
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc.PlayerId != 0)
                            {
                                if (main.Impostors.Contains(pc.PlayerId))
                                {
                                    p.RpcSetRoleDesync(RoleTypes.Scientist, pc);
                                }
                                else
                                {
                                    p.RpcSetRoleDesync(RoleTypes.Impostor, pc);
                                }
                                DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);
                            }
                        }
                    }
                } else
                {
                    p.RpcSetRole(p.Data.Role.Role);
                }
            }
        }
    }
}
