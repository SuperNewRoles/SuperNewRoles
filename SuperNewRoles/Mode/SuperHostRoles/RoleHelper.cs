using SuperNewRoles.CustomRPC;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    public static class RoleHelper
    {
        public static bool IsCrewVision(this PlayerControl player)
        {
            var IsCrewVision = false;
            switch (player.GetRole())
            {
                case RoleId.Sheriff:
                case RoleId.truelover:
                case RoleId.FalseCharges:
                case RoleId.RemoteSheriff:
                case RoleId.Arsonist:
                case RoleId.ToiletFan:
                case RoleId.NiceButtoner:
                    IsCrewVision = true;
                    break;
                    //クルー視界か
            }
            return IsCrewVision;
        }
        public static bool IsImpostorVision(this PlayerControl player)
        {
            return player.GetRole() switch
            {
                RoleId.MadMate => RoleClass.MadMate.IsImpostorLight,
                RoleId.MadMayor => RoleClass.MadMayor.IsImpostorLight,
                RoleId.MadStuntMan => RoleClass.MadStuntMan.IsImpostorLight,
                RoleId.MadJester => RoleClass.MadJester.IsImpostorLight,
                RoleId.JackalFriends => RoleClass.JackalFriends.IsImpostorLight,
                RoleId.Fox => RoleClass.Fox.IsImpostorLight,
                RoleId.MayorFriends => RoleClass.MayorFriends.IsImpostorLight,
                RoleId.BlackCat => RoleClass.BlackCat.IsImpostorLight,
                _ => false,
            };
        }
        public static bool IsZeroCoolEngineer(this PlayerControl player)
        {
            var IsZeroCoolEngineer = false;
            switch (player.GetRole())
            {
                case RoleId.Technician:
                    IsZeroCoolEngineer = true;
                    break;
                case RoleId.Jester:
                    return RoleClass.Jester.IsUseVent;
                case RoleId.MadMate:
                    return RoleClass.MadMate.IsUseVent;
                case RoleId.MadMayor:
                    return RoleClass.MadMayor.IsUseVent;
                case RoleId.MadStuntMan:
                    return RoleClass.MadStuntMan.IsUseVent;
                case RoleId.MadJester:
                    return RoleClass.MadJester.IsUseVent;
                case RoleId.JackalFriends:
                    return RoleClass.JackalFriends.IsUseVent;
                case RoleId.Fox:
                    return RoleClass.Fox.IsUseVent;
                case RoleId.MayorFriends:
                    return RoleClass.MayorFriends.IsUseVent;
                case RoleId.Tuna:
                    return RoleClass.Tuna.IsUseVent;
                case RoleId.BlackCat:
                    return RoleClass.BlackCat.IsUseVent;
                case RoleId.Spy:
                    return RoleClass.Spy.CanUseVent;
                case RoleId.Arsonist:
                    return RoleClass.Arsonist.IsUseVent;
                    //ベント無限か
            }
            return IsZeroCoolEngineer;
        }
    }
}