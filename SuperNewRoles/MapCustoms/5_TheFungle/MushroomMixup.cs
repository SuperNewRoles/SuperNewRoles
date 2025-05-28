using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;

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
        EmergencyCheckEvent.Instance.AddListener(x =>
        {
            if (MapEditSettingsOptions.TheFungleMushroomMixupCantOpenMeeting && fungleShipStatus.specialSabotage.IsActive)
            {
                x.RefEnabledEmergency = false;
                x.RefEmergencyTexts.Add(ModTranslation.GetString("MushroomMixupCantOpenMeeting"));

            }
        });
    }
}