using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.MapCustoms;
public class FungleAdditionalAdmin
{
    public static void AddAdmin()
    {
        if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle) &&
            MapCustom.TheFungleAdditionalAdmin.GetBool())
        {
            Transform Admin = GameObject.Instantiate(
                AirshipObject.transform.FindChild("Cockpit/panel_cockpit_map"), ShipStatus.Instance.transform);
            Admin.transform.position = new Vector3(-10.3f, 13.5f, 0.1f);
            Admin.transform.Rotate(new(0,0,10f));
        }
    }
    public static GameObject AirshipObject => Agartha.MapLoader.AirshipObject;
    public static ShipStatus Airship => Agartha.MapLoader.Airship;
}
