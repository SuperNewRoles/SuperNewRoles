using System.Linq;

namespace SuperNewRoles.Helpers;

public static class ModHelpers
{
    public static PlayerControl GetPlayerById(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player.PlayerId == id);
    }
}