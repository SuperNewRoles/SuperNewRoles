using System;
using System.Collections.Generic;
using System.Text;
namespace SuperNewRoles.Roles.Impostor
{
    public static class ConnectKiller
    {
        //ここにコードを書きこんでください
        public static void OnRepairSystem(SystemTypes systemTypes)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.ConnectKiller))
            {
                VentDataModules.ConnectAllVent(!RoleHelpers.IsComms());
                if (Vent.currentVent is not null) Vent.currentVent.SetButtons(true);
            }
        }
    }
}