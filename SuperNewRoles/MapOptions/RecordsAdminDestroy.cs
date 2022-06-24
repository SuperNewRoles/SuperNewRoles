using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Mode;

namespace SuperNewRoles.MapOptions
{
    [HarmonyPatch(typeof(ShipStatus),nameof(ShipStatus.Awake))]
    public class RecordsAdminDestroy
    {
        public static void Postfix()
        {
            if (MapOption.RecordsAdminDestroy.getBool() && MapOption.MapOptionSetting.getBool() && ModeHandler.isMode(ModeId.Default))
            {
                //アーカイブのアドミンをSeeyou!
                Transform Admin = GameObject.Find("Airship(Clone)").transform.FindChild("Records").FindChild("records_admin_map");
                GameObject.Destroy(Admin.gameObject);
            }
        }
    }
}
