using System.Collections.Generic;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class RoleHelper
    {
        public static bool isCrewVision(this PlayerControl player)
        {
            var IsCrewVision = false;
            switch (player.getRole())
            {
                case RoleId.Sheriff:
                case RoleId.truelover:
                case RoleId.FalseCharges:
                case RoleId.RemoteSheriff:
                case RoleId.Arsonist:
                case RoleId.ToiletFan:
                    IsCrewVision = true;
                    break;
                    //クルー視界か
            }
            return IsCrewVision;
        }
        public static bool isImpostorVision(this PlayerControl player)
        {
            switch (player.getRole())
            {
                case RoleId.MadMate:
                    return RoleClass.MadMate.IsImpostorLight;
                case RoleId.MadMayor:
                    return RoleClass.MadMayor.IsImpostorLight;
                case RoleId.MadStuntMan:
                    return RoleClass.MadStuntMan.IsImpostorLight;
                case RoleId.MadJester:
                    return RoleClass.MadJester.IsImpostorLight;
                case RoleId.JackalFriends:
                    return RoleClass.JackalFriends.IsImpostorLight;
                case RoleId.Fox:
                    return RoleClass.Fox.IsImpostorLight;
                case RoleId.MayorFriends:
                    return RoleClass.MayorFriends.IsImpostorLight;
                case RoleId.BlackCat:
                    return RoleClass.BlackCat.IsImpostorLight;
                //インポ視界か
            }
            return false;
        }
    }
}
