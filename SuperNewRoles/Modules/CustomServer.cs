using System;
using System.Linq;

namespace SuperNewRoles.Modules;

public static class CustomServer
{
    public static IRegionInfo[] defaultRegions;
    public static string SNRServerName => "<size=150%><color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color></size>\n<align=\"center\">Tokyo</align>";
    public static IRegionInfo SNRRegion { get; private set; }
    public static void UpdateRegions()
    {
        bool snrAdded = FastDestroyableSingleton<ServerManager>.Instance.AvailableRegions.Any(x => x.Name == SNRServerName);
        ServerManager serverManager = FastDestroyableSingleton<ServerManager>.Instance;
        SNRRegion = new StaticHttpRegionInfo(SNRServerName, StringNames.NoTranslation,
                "cs.supernewroles.com", new([
                        new("http-1", SNRURLs.SNRCS, 443, false)
                    ])).TryCast<IRegionInfo>();
        var regions = new IRegionInfo[2] {
                SNRRegion,
                new StaticHttpRegionInfo("Custom", StringNames.NoTranslation,
                "127.0.0.1", new([
                        new("Custom", "127.0.0.1", 443, false)
                    ])).TryCast<IRegionInfo>(),
            };

        IRegionInfo currentRegion = serverManager.CurrentRegion;
        Logger.Info($"Adding {regions.Length} regions");
        foreach (IRegionInfo region in regions)
        {
            if (region == null)
                Logger.Error("Could not add region", "CustomServer");
            else
            {
                if (currentRegion != null && region.Name.Equals(currentRegion.Name, StringComparison.OrdinalIgnoreCase))
                    currentRegion = region;
                serverManager.AddOrUpdateRegion(region);
            }
        }

        // AU remembers the previous region that was set, so we need to restore it
        if (currentRegion != null)
        {
            Logger.Info("Resetting previous region");
            serverManager.SetRegion(currentRegion);
        }

        if (!snrAdded)
            serverManager.SetRegion(SNRRegion);
    }
}