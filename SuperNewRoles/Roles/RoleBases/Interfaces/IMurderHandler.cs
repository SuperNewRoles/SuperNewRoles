using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface IMurderHandler
{
    public void OnMurderPlayer(PlayerControl source, PlayerControl target);
}