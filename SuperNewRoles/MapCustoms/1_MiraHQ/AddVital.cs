using UnityEngine;

namespace SuperNewRoles.MapCustoms
{
    public class AddVitals
    {
        public static void AddVital()
        {
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Mira) && MapCustom.AddVitalsMira.GetBool())
            {
                Transform Vital = GameObject.Instantiate(PolusObject.transform.FindChild("Office").FindChild("panel_vitals"), GameObject.Find("MiraShip(Clone)").transform);
                Vital.transform.position = new Vector3(8.5969f, 14.6337f, 0.0142f);
            }
        }
        public static GameObject PolusObject => Agartha.MapLoader.PolusObject;
        public static ShipStatus Polus => Agartha.MapLoader.Polus;
    }
}