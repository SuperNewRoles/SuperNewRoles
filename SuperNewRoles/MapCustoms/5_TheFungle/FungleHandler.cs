using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.MapCustoms;
public static class FungleHandler
{
    public static SpawnTypeOptions GetFungleSpawnType()
    {
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, false))
            return SpawnTypeOptions.Normal;
        return MapEditSettingsOptions.TheFungleSpawnType;
    }
    public static bool IsFungleSpawnType(SpawnTypeOptions spawnTypeOptions)
    {
        return GetFungleSpawnType() == spawnTypeOptions;
    }
}