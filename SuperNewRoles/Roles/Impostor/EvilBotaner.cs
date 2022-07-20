using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnhollowerBaseLib;

namespace SuperNewRoles.Roles
{
    public static class EvilBotaner
    {
        public static void EvilBotanerStartMeeting(byte sourceId)
        {
            if (ModeHandler.IsMode(ModeId.Default))
            {
                PlayerControl source = ModHelpers.playerById(sourceId);
                source.ReportDeadBody(null);
            }
            else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
            {

            }
        }
    }
}