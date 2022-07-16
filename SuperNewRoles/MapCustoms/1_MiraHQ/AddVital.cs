using System.Collections;
using BepInEx.IL2CPP.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SuperNewRoles.MapCustoms
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    public class AddVitals
    {
        public static void Postfix()
        {
            if (MapCustomHandler.isMapCustom(MapCustomHandler.MapCustomId.Mira) && MapCustoms.MapCustom.AddVitalsMira.getBool())
            {
                Transform Vital = GameObject.Instantiate(PolusObject.transform.FindChild("Office").FindChild("panel_vitals"), GameObject.Find("MiraShip(Clone)").transform);
                Vital.transform.position = new Vector3(8.5969f, 14.6337f, 0.0142f);
            }
        }
        public static GameObject PolusObject => Agartha.MapLoader.PolusObject;
        public static ShipStatus Polus => Agartha.MapLoader.Polus;
    }
}
