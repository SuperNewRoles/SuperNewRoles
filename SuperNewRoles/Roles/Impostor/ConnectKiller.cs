using System;
using System.Collections.Generic;
using System.Text;
namespace SuperNewRoles.Roles.Impostor
{
    public static class ConnectKiller
    {
        //ここにコードを書きこんでください
        public static void Update()
        {
            bool CommsData = RoleHelpers.IsComms();
            if (RoleClass.ConnectKiller.OldCommsData != CommsData)
            {
                VentDataModules.ConnectAllVent(CommsData);
                if (Vent.currentVent is not null) Vent.currentVent.SetButtons(true);
                RoleClass.ConnectKiller.OldCommsData = CommsData;
            }
        }
    }
}