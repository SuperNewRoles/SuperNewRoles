using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map
{
    class SetPosition
    {
        /*
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
        */
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

                    Agartha.Patch.SetPosition.SetVent(MiraShip);
                    Agartha.Patch.SetPosition.SetCamera();

                    Transform Wall = MiraShip.FindChild("Walls");

                    Wall.gameObject.AddComponent<EdgeCollider2D>().points =
                        new Vector2[] { new Vector2(-6.25f, 3f), new Vector2(10f, 3f), new Vector2(10f, 5.6f), new Vector2(12f, 5.6f), new Vector2(12f, 23.6f), new Vector2(11f, 23.6f), new Vector2(11f, 27.4f), new Vector2(-6.25f, 27.4f), new Vector2(-6.25f, 22f), 
                            new Vector2(-11.8f, 22f), new Vector2(-11.8f, 24f), new Vector2(-10.5f, 24f), new Vector2(-10.5f, 26.7f), new Vector2(-15.5f, 26.7f), new Vector2(-15.5f, 24f), new Vector2(-13.3f, 24f), new Vector2(-13.3f, 22f), new Vector2(-19f, 22f), new Vector2(-19f, 26.7f), new Vector2(-25.3f, 26.7f), new Vector2(-25.3f, 16f),
                            new Vector2(-23f,16f), new Vector2(-23f, 13f), new Vector2(-25.3f, 13f), new Vector2(-25.3f, 2.5f), new Vector2(-19f, 2.5f), new Vector2(-19f, 6.8f), new Vector2(-13.3f, 6.8f), new Vector2(-13.3f, 4.8f), new Vector2(-16f, 4.8f), new Vector2(-16f, 2.3f), new Vector2(-9.7f, 2.3f), new Vector2(-9.7f, 4.8f), new Vector2(-11.65f, 4.8f), new Vector2(-11.65f, 6.8f), new Vector2(-6.25f, 6.8f), new Vector2(-6.25f, 3f)
                        };
                    
                    Wall.gameObject.AddComponent<EdgeCollider2D>().points =
                        new Vector2[] { new Vector2(-5.25f, 4f), new Vector2(9f, 4f), new Vector2(9f, 6f), new Vector2(6.8f, 6f), new Vector2(6.8f, 8f), new Vector2(5.3f, 8f), new Vector2(5.3f, 7f), new Vector2(2.3f, 7f), new Vector2(2.3f, 11f), new Vector2(4.5f, 11f), new Vector2(5.3f, 10f), new Vector2(5.3f, 9.25f), new Vector2(6.8f, 9.25f), new Vector2(6.8f, 12.5f), new Vector2(10.8f, 12.5f), new Vector2(10.8f, 14.5f), new Vector2(5.8f, 14.5f), new Vector2(5.8f, 19.5f), new Vector2(4f, 19.5f),
                            new Vector2(4f, 14.3f), new Vector2(-2.8f, 14.3f), new Vector2(-2.8f, 17.8f), new Vector2(-5f, 17.8f), new Vector2(-5f, 9.2f), new Vector2(-3f, 9.2f), new Vector2(-3f, 10.2f), new Vector2(-2.5f, 11f), new Vector2(0f, 11f), new Vector2(0f, 7f), new Vector2(-3f, 7f), new Vector2(-3f, 8f), new Vector2(-5.25f, 8f), new Vector2(-5.25f, 4f)
                        };
                    //会議室左上
                    Wall.gameObject.AddComponent<EdgeCollider2D>().points =
                           new Vector2[] {
                               new Vector2(-5f, 19.3f),new Vector2(-5f, 25.5f),new Vector2(0f, 25.5f),new Vector2(0f, 23.7f), new Vector2(0f, 23.7f),new Vector2(-2.8f, 21.4f),new Vector2(-2.8f, 19.3f),new Vector2(-5f, 19.3f)
                           };
                    //会議室右上
                    Wall.gameObject.AddComponent<EdgeCollider2D>().points =
                           new Vector2[] {
                               new Vector2(1.5f, 23.5f),new Vector2(1.5f, 25.5f),new Vector2(9.5f, 25.5f),new Vector2(9.5f, 23.5f),new Vector2(5.8f, 23.5f),new Vector2(5.8f, 20.5f),new Vector2(4f, 20.5f),new Vector2(4f, 23.5f),new Vector2(1.5f, 23.5f)
                           };
                    //左側
                    Wall.gameObject.AddComponent<EdgeCollider2D>().points =
                           new Vector2[] {
                               new Vector2(-6f, 9f),new Vector2(-11.65f, 9f),new Vector2(-11.65f, 10.5f),new Vector2(-10f, 10.5f),new Vector2(-10f, 13f),new Vector2(-16f, 13f),new Vector2(-16f, 10.5f),new Vector2(-13.45f, 10.5f),new Vector2(-13.45f, 9f),new Vector2(-19f, 9f),
                               new Vector2(-19f, 13f),new Vector2(-21.7f, 13f),new Vector2(-21.7f, 16f), new Vector2(-19f, 16f), new Vector2(-19f, 20f), new Vector2(-13.45f, 20f), new Vector2(-13.45f, 18.5f), new Vector2(-16f, 18.5f), new Vector2(-16f, 15.8f), new Vector2(-10f, 15.8f),
                               new Vector2(-10f, 18.5f), new Vector2(-11.65f, 18.5f), new Vector2(-11.65f, 20f), new Vector2(-6f, 20f),new Vector2(-6f, 9f)
                           };

                    SuperNewRolesPlugin.Logger.LogInfo("オールドア:"+ShipStatus.Instance.AllDoors.Length);
                    SpriteRenderer CafeteriaWalls = Wall.FindChild("CafeteriaWalls").gameObject.GetComponent<SpriteRenderer>();
                    CafeteriaWalls.sprite = Agartha.ImageManager.Room_Meeting;
                    CafeteriaWalls.transform.position = new Vector3(13.15f, 16f, 4f);
                    CafeteriaWalls.transform.localScale *= 1.5f;

                    SpriteRenderer AdminWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    AdminWalls.name = "AdminWalls";
                    AdminWalls.sprite = Agartha.ImageManager.Room_Security;
                    AdminWalls.transform.position = new Vector3(10.97f, 5.87f, 4f);
                    AdminWalls.transform.localScale *= 0.7875f;
                    AdminWalls.transform.localScale = new Vector3(AdminWalls.transform.localScale.x * -1, AdminWalls.transform.localScale.y, AdminWalls.transform.localScale.z);

                    SpriteRenderer SecurityWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    SecurityWalls.name = "SecurityWalls";
                    SecurityWalls.sprite = Agartha.ImageManager.Room_Security;
                    SecurityWalls.transform.position = new Vector3(16.6f, 5.87f, 4f);
                    //SecurityWalls.transform.localScale *= 0.75f;
                    SecurityWalls.transform.localScale *= 0.7875f;

                    SpriteRenderer WorkRoomWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    WorkRoomWalls.name = "WorkRoomWalls";
                    WorkRoomWalls.sprite = Agartha.ImageManager.Room_WorkRoom;
                    WorkRoomWalls.transform.position = new Vector3(-9.5f, 17.8f, 4f);
                    WorkRoomWalls.transform.localScale = new Vector3(1.005f,1.097f,1.005f);

                    SpriteRenderer WareHouseWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    WareHouseWalls.name = "WareHouseWalls";
                    WareHouseWalls.sprite = Agartha.ImageManager.Room_WareHouse;
                    WareHouseWalls.transform.position = new Vector3(-9.65f, 4.8f, 4f);
                    WareHouseWalls.transform.localScale = new Vector3(2.1f,2.3f,2.1f);

                    SpriteRenderer CommsRoomWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    CommsRoomWalls.name = "CommsRoomWalls";
                    CommsRoomWalls.sprite = Agartha.ImageManager.AgarthagetSprite("Room_Up");
                    CommsRoomWalls.transform.position = new Vector3(-0.35f, 22.32f, 4f);
                    CommsRoomWalls.transform.localScale = new Vector3(1.41f, 1.35f, 1.35f);

                    SpriteRenderer O2RoomWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    O2RoomWalls.name = "O2RoomWalls";
                    O2RoomWalls.sprite = Agartha.ImageManager.AgarthagetSprite("Room_Up");
                    O2RoomWalls.transform.position = new Vector3(-0.4f, 8.7f, 4f);
                    O2RoomWalls.transform.localScale = new Vector3(1.7f, 1.25f, 1.35f);

                    SpriteRenderer MedBayRoomWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    MedBayRoomWalls.name = "MedBayRoomWalls";
                    MedBayRoomWalls.sprite = Agartha.ImageManager.AgarthagetSprite("Room_Up");
                    MedBayRoomWalls.transform.position = new Vector3(-0.4f, 13.25f, 4f);
                    MedBayRoomWalls.transform.localScale = new Vector3(1.7f, -1.3f, 1.35f);

                    SpriteRenderer ToolRoomWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    ToolRoomWalls.name = "ToolRoomWalls";
                    ToolRoomWalls.sprite = Agartha.ImageManager.AgarthagetSprite("Room_Up");
                    ToolRoomWalls.transform.position = new Vector3(-0.3f, -0.35f, 4f);
                    ToolRoomWalls.transform.localScale = new Vector3(1.75f, -1.2f, 1.35f);

                    SpriteRenderer ElectricalRoomWalls = ShipStatus.Instantiate(CafeteriaWalls).gameObject.GetComponent<SpriteRenderer>();
                    ElectricalRoomWalls.name = "ElectricalRoomWalls";
                    ElectricalRoomWalls.sprite = Agartha.ImageManager.AgarthagetSprite("Room_Electrical");
                    ElectricalRoomWalls.transform.position = new Vector3(21.5f, 15.3f, 4f);
                    ElectricalRoomWalls.transform.localScale *= 0.9f;

                    MiraShip.FindChild("CloudGen").gameObject.SetActive(false);
                    
                    Transform CafeObject = MiraShip.FindChild("Cafe");
                    CafeObject.gameObject.GetChildren().SetActiveAllObject("Table",false);
                    GameObject.Destroy(CafeObject.gameObject.GetComponent<EdgeCollider2D>());
                    Transform CafeObject_Table = CafeObject.FindChild("Table");
                    CafeObject_Table.gameObject.GetChildren().SetActiveAllObject("EmergencyConsole", false);
                    CafeObject_Table.GetComponent<SpriteRenderer>().sprite = Agartha.ImageManager.Object_Table1;
                    GameObject.Destroy(CafeObject_Table.GetComponent<BoxCollider2D>());
                    GameObject.Destroy(CafeObject_Table.GetComponent<PolygonCollider2D>());
                    CafeObject_Table.gameObject.AddComponent<PolygonCollider2D>();
                    CafeObject_Table.transform.position = new Vector3(12.7f, 16f, 3.14f);
                    CafeObject_Table.FindChild("EmergencyConsole").transform.localPosition = new Vector3(0.18f, -0.5f, 0);
                    //CafeObject.position = new Vector3(1000, 1000, 1000);

                    Transform SkyBri = MiraShip.FindChild("SkyBridge");
                    GameObject.Destroy(SkyBri.GetComponent<PolygonCollider2D>());
                    GameObject.Destroy(SkyBri.GetComponent<EdgeCollider2D>());
                    GameObject.Destroy(SkyBri.GetComponent<EdgeCollider2D>());
                    GameObject.Destroy(SkyBri.GetComponent<EdgeCollider2D>());
                    GameObject.Destroy(SkyBri.GetComponent<EdgeCollider2D>());
                    Transform Garden = MiraShip.FindChild("Garden");
                    GameObject.Destroy(Garden.GetComponent<PolygonCollider2D>());
                    GameObject.Destroy(Garden.GetComponent<EdgeCollider2D>());
                    GameObject.Destroy(Garden.GetComponent<EdgeCollider2D>());
                    GameObject.Destroy(Garden.GetComponent<EdgeCollider2D>());

                    MiraShip.FindChild("LaunchPad").gameObject.SetActive(false);
                    MiraShip.FindChild("LeftBottomRoom").gameObject.SetActive(false);
                    MiraShip.FindChild("Decontam").gameObject.SetActive(false);
                    MiraShip.FindChild("MedBay").gameObject.GetChildren().SetActiveAllObject("MedScanner",false);
                    MiraShip.FindChild("Storage").gameObject.SetActive(false);
                    MiraShip.FindChild("Comms").gameObject.GetChildren().SetActiveAllObject("comms-top",false);
                    SkyBri.gameObject.GetChildren().SetActiveAllObject("FixWiringConsole (2)",false);
                    Garden.gameObject.GetChildren().SetActiveAllObject("FixWiringConsole",false);
                    MiraShip.FindChild("Laboratory").gameObject.SetActive(false);
                    MiraShip.FindChild("LabHall").gameObject.GetChildren().SetActiveAllObject("FixWiringConsole", false);
                    MiraShip.FindChild("Walls").gameObject.GetChildren().SetActiveAllObject("CafeteriaWalls", false);
                    MiraShip.FindChild("Admin").gameObject.GetChildren().SetActiveAllObject("MapTable", false);
                    MiraShip.FindChild("Admin").FindChild("AdminVent").gameObject.SetActive(true);
                    GameObject.Destroy(MiraShip.FindChild("LabHall").gameObject.GetComponent<EdgeCollider2D>());
                    //MiraShip.FindChild("Locker").gameObject.SetActive(false);
                    MiraShip.FindChild("Office").gameObject.SetActive(false);
                    //MiraShip.FindChild("Office").gameObject.SetActive(false);

                    Agartha.Patch.SetTasksClass.SetTasks(MiraShip);
                    Agartha.Patch.SetPosition.SetObject(MiraShip);

                    Transform Admin = MiraShip.FindChild("Admin");
                    Transform AdminMapTable = Admin.FindChild("MapTable");
                    AdminMapTable.GetComponent<SpriteRenderer>().sprite = Agartha.ImageManager.Admin_Table;
                    AdminMapTable.position = new Vector3(11.85f, 6.95f, 4f);
                    Transform AdminMapConsole = AdminMapTable.FindChild("AdminMapConsole");
                    AdminMapConsole.GetComponent<SpriteRenderer>().sprite = Agartha.ImageManager.Admin_Table;
                    AdminMapConsole.position = AdminMapTable.position;
                    Transform AdminEnterCodeConsole = AdminMapTable.FindChild("EnterCodeConsole");
                    AdminEnterCodeConsole.gameObject.SetActive(false);
                    GameObject.Destroy(AdminMapTable.gameObject.GetComponent<PolygonCollider2D>());
                    AdminMapTable.gameObject.AddComponent<PolygonCollider2D>();
                    //AdminMapConsole.position = new Vector3(2.573f, -2, 0);

                    if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
                    {
                        MiraShip.FindChild("TaskAddConsole").position = new Vector3(12.7f, 13.7f, 0f);
                        Agartha.Patch.SetPosition.SetDummy();
                    }
                }
            }//
        }
    }
}
