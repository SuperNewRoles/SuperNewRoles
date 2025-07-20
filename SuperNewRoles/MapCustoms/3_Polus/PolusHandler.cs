using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.MapCustoms;
public static class PolusHandler
{
    public static SpawnTypeOptions GetPolusSpawnType()
    {
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Polus, false))
            return SpawnTypeOptions.Normal;
        return MapEditSettingsOptions.PolusSpawnType;
    }
    public static bool IsPolusSpawnType(SpawnTypeOptions spawnTypeOptions)
    {
        return GetPolusSpawnType() == spawnTypeOptions;
    }
}