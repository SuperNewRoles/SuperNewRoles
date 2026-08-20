using System.Linq;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Safety;

public static class OfficialSnrServer
{
    public static bool IsCurrent()
    {
        if (!ModHelpers.IsCustomServer()) return false;
        var region = FastDestroyableSingleton<ServerManager>.Instance?.CurrentRegion;
        if (region?.Servers == null) return false;
        return region.Servers.ToArray().Any(server =>
            server != null &&
            !string.IsNullOrEmpty(server.Ip) &&
            (server.Ip.Contains("supernewroles.com") || server.Ip.Contains("cs.supernewroles") || server.Ip.Contains("cs-useast")));
    }
}
