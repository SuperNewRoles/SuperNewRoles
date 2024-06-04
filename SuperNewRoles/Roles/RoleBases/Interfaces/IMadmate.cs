using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles.RoleBases.Interfaces;
public interface IMadmate : IVentAvailable, IImpostorVision
{
    public int CheckTask => -1;
    public bool HasCheckImpostorAbility { get; }
    public bool CanSeeImpostor(PlayerControl me)
    {
        return HasCheckImpostorAbility && CheckTask <= TaskCount.TaskDate(me.Data).Item1;
    }
}