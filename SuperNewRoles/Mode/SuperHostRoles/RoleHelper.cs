
using AmongUs.GameOptions;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Mode.SuperHostRoles;

public static class RoleHelper
{
    public static bool IsCrewVision(this PlayerControl player)
    {
        var IsCrewVision = false;
        if (player.GetRoleBase() is ISupportSHR supportSHR)
        {
            if (supportSHR.IsImpostorLight.HasValue)
                IsCrewVision = !supportSHR.IsImpostorLight.Value;
            else if (supportSHR.IsDesync && supportSHR.RealRole is RoleTypes.Crewmate or RoleTypes.Engineer or RoleTypes.Scientist)
                IsCrewVision = true;
        }
        if (!IsCrewVision)
        {
            switch (player.GetRole())
            {
                case RoleId.Sheriff:
                case RoleId.truelover:
                case RoleId.FalseCharges:
                case RoleId.RemoteSheriff:
                case RoleId.Arsonist:
                case RoleId.ToiletFan:
                case RoleId.NiceButtoner:
                case RoleId.Worshiper when !SuperNewRoles.Roles.Impostor.MadRole.Worshiper.RoleData.IsImpostorLight:
                case RoleId.MadRaccoon when !SuperNewRoles.Roles.Impostor.MadRole.MadRaccoon.RoleData.IsImpostorLight:
                    IsCrewVision = true;
                    break;
                    //クルー視界か
            }
        }
        return IsCrewVision;
    }
    public static bool IsImpostorVision(this PlayerControl player)
    {
        if (player.GetRoleBase() is ISupportSHR supportSHR &&
            supportSHR.IsImpostorLight.HasValue)
        {
            return supportSHR.IsImpostorLight.Value;
        }
        return player.GetRole() switch
        {
            RoleId.Madmate => RoleClass.Madmate.IsImpostorLight,
            RoleId.MadMayor => RoleClass.MadMayor.IsImpostorLight,
            RoleId.MadStuntMan => RoleClass.MadStuntMan.IsImpostorLight,
            RoleId.MadJester => RoleClass.MadJester.IsImpostorLight,
            RoleId.JackalFriends => RoleClass.JackalFriends.IsImpostorLight,
            RoleId.Fox => RoleClass.Fox.IsImpostorLight,
            RoleId.MayorFriends => RoleClass.MayorFriends.IsImpostorLight,
            RoleId.BlackCat => RoleClass.BlackCat.IsImpostorLight,
            RoleId.MadSeer => RoleClass.MadSeer.IsImpostorLight,
            RoleId.SeerFriends => RoleClass.SeerFriends.IsImpostorLight,
            _ => false,
        };
    }
    public static bool IsZeroCoolEngineer(this PlayerControl player)
    {
        var IsZeroCoolEngineer = false;
        if (player.GetRoleBase() is ISupportSHR supportSHR &&
            supportSHR.IsZeroCoolEngineer)
        {
            IsZeroCoolEngineer = true;
        }
        if (!IsZeroCoolEngineer)
        {
            switch (player.GetRole())
            {
                case RoleId.Technician:
                    IsZeroCoolEngineer = true;
                    break;
                case RoleId.Jester:
                    return RoleClass.Jester.IsUseVent;
                case RoleId.Madmate:
                    return RoleClass.Madmate.IsUseVent;
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
                case RoleId.MadSeer:
                    return RoleClass.MadSeer.IsUseVent;
                case RoleId.SeerFriends:
                    return RoleClass.SeerFriends.IsUseVent;
                    //ベント無限か
            }
        }
        return IsZeroCoolEngineer;
    }
}