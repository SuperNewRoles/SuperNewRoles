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
            if (systemTypes == SystemTypes.Comms && PlayerControl.LocalPlayer.IsRole(RoleId.ConnectKiller))
            {
                VentDataModules.ConnectAllVent(!RoleHelpers.IsComms());
                if (Vent.currentVent is not null) Vent.currentVent.SetButtons(true);
                /*
                VentilationSystem system = ShipStatus.Instance.Systems[SystemTypes.Ventilation].TryCast<VentilationSystem>();
                foreach (var vent in VentData.VentMap)
                {
                    if (vent.Value?.Vent == null) continue;
                    vent.Value.Vent.UpdateArrows(system);
                }*/
            }
        }
    }
}