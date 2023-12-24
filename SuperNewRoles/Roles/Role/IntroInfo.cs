using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;

namespace SuperNewRoles.Roles.Role;
public class IntroInfo
{
    public static Dictionary<RoleId, IntroInfo> IntroInfos = new();
    public RoleTypes IntroSound;
    public RoleId Role;
    public short IntroNum;
    public string NameKey => _nameKey != null ? _nameKey : _nameKey = RoleInfoManager.GetRoleInfo(Role)?.NameKey;
    private string _nameKey;
    private string _titleDesc;
    public string IntroDesc
    {
        get
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                return _titleDesc;
            }
            return IntroData.GetTitle(NameKey, IntroNum, Role);
        }
    }
    public IntroInfo(RoleId role, short introNum = 1, RoleTypes introSound = RoleTypes.Crewmate)
    {
        this.Role = role;
        this.IntroNum = introNum;
        this.IntroSound = introSound;
        _titleDesc = IntroData.GetTitle(NameKey, IntroNum, Role);
        IntroInfos.Add(role, this);
    }
    public void UpdateIntroDesc()
    {
        _titleDesc = IntroData.GetTitle(NameKey, IntroNum, Role);
    }
    public static IntroInfo GetIntroInfo(RoleId role)
    {
        return IntroInfos.TryGetValue(role, out IntroInfo info) ? info : null;
    }
}