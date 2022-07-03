using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.CustomOption;
using SuperNewRoles.MapCustoms.Airship;

namespace SuperNewRoles.MapCustoms
{
    class MapCustom
    {
        public static CustomOption.CustomOption MapCustomOption;
        public static void CreateOption()
        {
            MapCustomOption = CustomOption.CustomOption.Create(623, false, CustomOptionType.Generic, "MapCustom", false, null, true);
            SecretRoom.CreateOption();
        }
    }
}
