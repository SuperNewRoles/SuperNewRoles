using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.MapCustoms
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public class RecordsAdminDestroy
    {
        public static void Postfix()
        {
            if (MapCustom.RecordsAdminDestroy.GetBool() && MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship))
            {
                //アーカイブのアドミンをSeeyou!
                Transform Admin = GameObject.Find("Airship(Clone)").transform.FindChild("Records").FindChild("records_admin_map");
                Object.Destroy(Admin.gameObject);
            }
        }
    }
}