using HarmonyLib;
using SuperNewRoles.CustomOption;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(GameStartManager.Start))]
    public class Inversion
    {
        public static GameObject skeld;
        public static GameObject mira;
        public static GameObject polus;
        public static GameObject airship;
        public static void Prefix()
        {
            if (AmongUsClient.Instance.GameMode != GameModes.FreePlay && CustomOptions.enableMirroMap.GetBool())
            {
                if (PlayerControl.GameOptions.MapId == 0)
                {
                    skeld = GameObject.Find("SkeldShip(Clone)");
                    skeld.transform.localScale = new Vector3(-1.2f, 1.2f, 1.2f);
                    ShipStatus.Instance.InitialSpawnCenter = new Vector2(0.8f, 0.6f);
                    ShipStatus.Instance.MeetingSpawnCenter = new Vector2(0.8f, 0.6f);
                }
                else if (PlayerControl.GameOptions.MapId == 1)
                {
                    mira = GameObject.Find("MiraShip(Clone)");
                    mira.transform.localScale = new Vector3(-1f, 1f, 1f);
                    ShipStatus.Instance.InitialSpawnCenter = new Vector2(4.4f, 2.2f);
                    ShipStatus.Instance.MeetingSpawnCenter = new Vector2(-25.3921f, 2.5626f);
                    ShipStatus.Instance.MeetingSpawnCenter2 = new Vector2(-25.3921f, 2.5626f);
                }
                else if (PlayerControl.GameOptions.MapId == 2)
                {
                    polus = GameObject.Find("PolusShip(Clone)");
                    polus.transform.localScale = new Vector3(-1f, 1f, 1f);
                    ShipStatus.Instance.InitialSpawnCenter = new Vector2(-16.7f, -2.1f);
                    ShipStatus.Instance.MeetingSpawnCenter = new Vector2(-19.5f, -17f);
                    ShipStatus.Instance.MeetingSpawnCenter2 = new Vector2(-19.5f, -17f);
                }
                /*else if(PlayerControl.GameOptions.MapId == 4 && CustomOptionHolder.InversionAShip.GetBool())
                {
                    airship = GameObject.Find("Airship(Clone)");
                    airship.transform.localScale = new Vector3(-0.7f, 0.7f, 1f);
                    airshipの選択スポーンシステムの対応ができてないため非表示
                }*/
            }
        }
    }
}