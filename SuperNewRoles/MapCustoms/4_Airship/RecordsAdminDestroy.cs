using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.MapCustoms
{
    public class RecordsAdminDestroy
    {
        public static void AdminDestroy()
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