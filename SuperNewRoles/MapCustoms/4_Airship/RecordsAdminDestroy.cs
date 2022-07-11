using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.MapCustoms
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public class RecordsAdminDestroy
    {
        public static void Postfix()
        {
            if (MapCustom.RecordsAdminDestroy.getBool() && MapCustomHandler.isMapCustom(MapCustomHandler.MapCustomId.Airship))
            {
                //アーカイブのアドミンをSeeyou!
                Transform Admin = GameObject.Find("Airship(Clone)").transform.FindChild("Records").FindChild("records_admin_map");
                GameObject.Destroy(Admin.gameObject);
            }
        }
    }
}
