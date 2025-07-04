using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;

namespace SuperNewRoles.MapCustoms;
public static class ZiplineUpdown
{
    public static void Initialize()
    {
        if (!MapEditSettingsOptions.TheFungleZiplineOption)
            return;
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle))
            return;
        
        try
        {
            FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
            if (fungleShipStatus == null)
            {
                Logger.Warning("Failed to get FungleShipStatus for zipline modifications");
                return;
            }
            
            if (fungleShipStatus.Zipline == null)
            {
                Logger.Warning("Zipline component is null on FungleShipStatus");
                return;
            }
            
            fungleShipStatus.Zipline.upTravelTime = MapEditSettingsOptions.TheFungleZiplineUpTime;
            fungleShipStatus.Zipline.downTravelTime = MapEditSettingsOptions.TheFungleZiplineDownTime;
            
            Logger.Info($"Successfully set zipline times: up={MapEditSettingsOptions.TheFungleZiplineUpTime}s, down={MapEditSettingsOptions.TheFungleZiplineDownTime}s");
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Error setting zipline travel times: {ex}");
        }
    }
}
