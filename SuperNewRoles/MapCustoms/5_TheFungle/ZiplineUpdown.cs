using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.MapCustoms;
public static class ZiplineUpdown
{
    public static void Initialize()
    {
        if (!MapEditSettingsOptions.TheFungleZiplineOption && !MapEditSettingsOptions.TheFungleZiplineOption)
            return;
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle))
            return;
        FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
        if (fungleShipStatus == null)
            return;
        fungleShipStatus.Zipline.upTravelTime = MapEditSettingsOptions.TheFungleZiplineUpTime;
        fungleShipStatus.Zipline.downTravelTime = MapEditSettingsOptions.TheFungleZiplineDownTime;
    }
}
