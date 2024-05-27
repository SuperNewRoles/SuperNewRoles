using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface IJackal : IKiller, IVentAvailable
{
    public bool CanSidekick { get; }
    public bool isShowSidekickButton { get; }
    public bool isShowKillButton => true;
    public float SidekickCoolTime => 0f;
    public float JackalKillCoolTime => 0f;

    public void OnClickSidekickButton(PlayerControl target);
    public void SetAmSidekicked();
}