using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        FungleSpawnType spawntype = (FungleSpawnType)MapCustom.TheFungleSpawnType.GetSelection();
        if (spawntype == FungleSpawnType.Select && !MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle))
            spawntype = FungleSpawnType.Default;
        return spawntype;
    }
    public static bool IsFungleSpawnType(FungleSpawnType fungleSpawnType)
    {
        return GetFungleSpawnType() == fungleSpawnType;
    }
}
