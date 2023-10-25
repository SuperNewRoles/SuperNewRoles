using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rewired.UI.ControlMapper;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface ICustomButton
{
    public CustomButtonInfo[] CustomButtonInfos { get; }
    public CustomButtonInfo ButtonInfo => CustomButtonInfos.FirstOrDefault();
}