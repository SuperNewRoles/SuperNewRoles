using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.MapOptions
{
    [HarmonyPatch(typeof(ShipStatus),nameof(ShipStatus.Awake))]
    public class RecordsAdminDestroy
    {
        public static void Postfix()
        {
            Transform Airship = GameObject.Find("Airship(Clone)").transform;
            GameObject.Destroy(Airship.FindChild("Records").FindChild("records_admin_map"));
        }
    }
}
