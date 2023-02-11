using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class ShermansServant
{
    public static void SetupCustomOptions()
    {
    }
    
    public static List<PlayerControl> ShermansServantPlayer;
    public static Color32 color = new Color32(192, 177, 246, byte.MaxValue);
    public static void ClearAndReload()
    {
        ShermansServantPlayer = new();
    }
    
    // ここにコードを書きこんでください
}