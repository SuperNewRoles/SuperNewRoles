using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.MapCustoms;
public static class MushroomMixup
{
    public static void Initialize()
    {
        if (!MapEditSettingsOptions.TheFungleMushroomMixupOption)
            return;
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle))
            return;
        FungleShipStatus fungleShipStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
        if (fungleShipStatus == null)
            return;
        fungleShipStatus.specialSabotage.secondsForAutoHeal = MapEditSettingsOptions.TheFungleMushroomMixupTime;
    }
}