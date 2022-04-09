using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map
{
    class SetPosition
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
        public static class ShipStatusBeginPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch]
            public static void Postfix(ShipStatus __instance)
            {
                ApplyChanges(__instance);
            }
        }

        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
        public static class ShipStatusAwakePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch]
            public static void Postfix(ShipStatus __instance)
            {
                ApplyChanges(__instance);
            }
        }
        public static void ApplyChanges(ShipStatus __instance)
        {
            if (Data.IsMap(CustomMapNames.Agartha))
            {
                if (__instance.Type == ShipStatus.MapType.Hq)
                {
                    Transform MiraShip = GameObject.Find("MiraShip(Clone)").transform;

                    Transform Wall = MiraShip.FindChild("Walls");
                    Wall.gameObject.AddComponent<EdgeCollider2D>().points =
                        new Vector2[] { new Vector2(-6.25f, 3f), new Vector2(10f, 3f), new Vector2(10f, 5.6f), new Vector2(12f, 5.6f), new Vector2(12f, 23.6f), new Vector2(11f, 23.6f), new Vector2(11f, 27.4f), new Vector2(-6.25f, 27.4f), new Vector2(-6.25f, 22f), new Vector2(-12.25f, 22f), new Vector2(-12.5f, 24f), new Vector2(-10.5f, 24f), new Vector2(-10.5f, 26.7f), new Vector2(-15.5f, 26.7f), new Vector2(-15.5f, 24f), new Vector2(-13.5f, 24f), new Vector2(-13.5f, 22f), new Vector2(-19f, 22f), new Vector2(-19f, 26.7f), new Vector2(-25.3f, 26.7f), new Vector2(-25.3f, 15.5f), new Vector2(-23f,15.5f), new Vector2(-23f, 13f), new Vector2(-25f, 13f) };

                    GameObject.Destroy(MiraShip.FindChild("CloudGen").GetComponent<EdgeCollider2D>());

                    Transform Admin = MiraShip.FindChild("Admin");
                    Transform AdminMapTable = Admin.FindChild("MapTable");
                    AdminMapTable.GetComponent<SpriteRenderer>().sprite = Agartha.ImageManager.Admin_Table;
                    AdminMapTable.position = new Vector3(10.19f, 6.51f, 4f);
                    Transform AdminMapConsole = AdminMapTable.FindChild("AdminMapConsole");
                    AdminMapConsole.GetComponent<SpriteRenderer>().sprite = Agartha.ImageManager.Admin_Table;
                    AdminMapConsole.position = AdminMapTable.position;
                    Transform AdminEnterCodeConsole = AdminMapTable.FindChild("EnterCodeConsole");
                    AdminEnterCodeConsole.gameObject.SetActive(false);
                    GameObject.Destroy(AdminMapTable.gameObject.GetComponent<PolygonCollider2D>());
                    AdminMapTable.gameObject.AddComponent<PolygonCollider2D>();
                    //AdminMapConsole.position = new Vector3(2.573f, -2, 0);
                }
            }//
        }
    }
}
