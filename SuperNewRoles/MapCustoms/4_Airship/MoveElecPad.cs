using UnityEngine;

namespace SuperNewRoles.MapCustoms
{
    public class MoveElecPad
    {
        public static Vector3 Meetpos = new(16.908f, 14.7988f, 1.0f);//ミーティング
        public static Vector3 Safepos = new(37.0477f, -3.6707f, 1.0f);//金庫タスク
        public static bool flag = false;
        public static void ClearAndReload()
        {
            flag = false;
        }

        public static void MoveElecPads()
        {
            if (SpecimenVital.flag) return;
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) && MapCustom.MoveElecPad.GetBool())
            {
                var gap = GameObject.Find("task_lightssabotage (gap)");//昇降機配電盤
                var cargo = GameObject.Find("task_lightssabotage (cargo)");//貨物室配電盤
                if (gap != null)
                {
                    var transform = gap.GetComponent<Transform>();
                    transform.SetPositionAndRotation(Meetpos, transform.rotation);
                    SpecimenVital.flag = true;
                }
                if (cargo != null)
                {
                    var transform = cargo.GetComponent<Transform>();
                    transform.SetPositionAndRotation(Safepos, transform.rotation);
                    SpecimenVital.flag = true;
                }
            }
        }
    }
}