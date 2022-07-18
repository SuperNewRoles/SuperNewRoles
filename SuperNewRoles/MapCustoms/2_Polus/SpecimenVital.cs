using UnityEngine;

namespace SuperNewRoles.MapCustoms
{
    public class SpecimenVital
    {
        public static Vector3 pos = new(35.39f, -22.10f, 1.0f);
        public static bool flag = false;
        public static void ClearAndReload()
        {
            flag = false;
        }

        public static void MoveVital()
        {
            if (flag) return;
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Polus) && MapCustom.SpecimenVital.GetBool())
            {
                var panel = GameObject.Find("panel_vitals");
                if (panel != null)
                {
                    var transform = panel.GetComponent<Transform>();
                    transform.SetPositionAndRotation(pos, transform.rotation);
                    flag = true;
                }
            }
        }
    }
}