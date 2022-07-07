using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Mode;

namespace SuperNewRoles.MapCustoms
{
    [HarmonyPatch(typeof(ShipStatus),nameof(ShipStatus.Awake))]
    public class RecordsAdminDestroy
    {
        public static void Postfix()
        {
            if (MapCustom.RecordsAdminDestroy.getBool() && Booler.)
            {
                //アーカイブのアドミンをSeeyou!
                Transform Admin = GameObject.Find("Airship(Clone)").transform.FindChild("Records").FindChild("records_admin_map");
                GameObject.Destroy(Admin.gameObject);
            }
        }
    }
}
