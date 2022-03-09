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
                    main.Impostors.Add(p.PlayerId);
                }
            }
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.isImpostor())
                {
                    if (p.PlayerId != 0)
                    {
                        p.RpcSetRoleDesync(p.Data.Role.Role);
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
                        //ホストは代わりに普通のクルーにする
                        p.RpcSetRole(RoleTypes.Crewmate);
                        DestroyableSingleton<RoleManager>.Instance.SetRole(p, p.Data.Role.Role);
                    }
                }
            }
        }
    }
}
