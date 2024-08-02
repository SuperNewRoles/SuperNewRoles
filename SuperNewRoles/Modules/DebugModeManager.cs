using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Modules;
public static class DebugModeManager
{
    public static bool IsDebugMode;
    public static void UpdateDebugModeState()
    {
        IsDebugMode = ConfigRoles.DebugMode.Value;
    }
}