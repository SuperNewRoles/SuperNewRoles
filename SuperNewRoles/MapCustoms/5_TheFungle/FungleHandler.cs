using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperNewRoles.CustomOptions.Categories;

namespace SuperNewRoles.MapCustoms;
public static class FungleHandler
{
    public enum FungleSpawnType
    {
        Default,
        Random,
        Select
    }
    public static FungleSpawnType GetFungleSpawnType()
    {
        if (!MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, false))
            return FungleSpawnType.Default;
        return MapEditSettingsOptions.TheFungleSpawnType;
    }
    public static bool IsFungleSpawnType(FungleSpawnType fungleSpawnType)
    {
        return GetFungleSpawnType() == fungleSpawnType;
    }
}