using BepInEx.IL2CPP.Utils;
using PowerTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    public static class SetPosition
    {
        public static PlainDoor CreateDoor(Vector3 position,Vector3? scale = null,int id = -1,int index = 3)
        {
            if (id == -1)
            {
                id = GetDoorAvailableId();
            }
            if (scale == null)
            {
                scale = new Vector3(1,1,1);
            }
            PlainDoor door = GameObject.Instantiate(MapLoader.Airship.AllDoors[index]);
            door.transform.localScale = (Vector3)scale;
            door.transform.position = position;
            door.Id = id;
            new LateTask(() =>
            {
                door.SetDoorway(false);
            }, 4f, "SetDummyPosition");
            return door;
        }
        public static void SetDoor()//Transform Miraship)
        {
            List<PlainDoor> doors = new List<PlainDoor>();
            doors.Add(CreateDoor(new Vector3(13.5f, 20.55f, 7f),new Vector3(0.85f,0.75f,0.75f)));
            doors.Add(CreateDoor(new Vector3(0.03f, 20.5f, 4f),new Vector3(0.95f,0.8f,1f)));
            doors.Add(CreateDoor(new Vector3(0.03f, 15.4f, 4f), new Vector3(0.95f, 0.8f, 1f)));
            doors.Add(CreateDoor(new Vector3(-6.2f, 17.8f, 4f), new Vector3(1f, 1f, 1f),-1,0));
            doors.Add(CreateDoor(new Vector3(-9.6f, 12f, 4f), new Vector3(0.65f, 0.75f, 0.75f)));
            doors.Add(CreateDoor(new Vector3(0.1f, 7.2f, 4f), new Vector3(0.95f, 0.8f, 1f)));
            doors.Add(CreateDoor(new Vector3(0.22f, 1.67f, 4f), new Vector3(0.95f, 0.8f, 1f)));
            doors.Add(CreateDoor(new Vector3(-6.2f, 4.5f, 4f), new Vector3(0.7f, 0.8f, 1f),-1,0));
            doors.Add(CreateDoor(new Vector3(9.35f, 5.3f, 4f), new Vector3(0.5f, 0.7f, 0.5f), -1, 0));
            ShipStatus.Instance.AllDoors = doors.ToArray();
        }
        public static int GetDoorAvailableId()
        {
            var id = 0;
            if (ShipStatus.Instance.AllDoors.Count == 0) return 0;
            while (true)
            {
                if (ShipStatus.Instance.AllDoors.All(v => v.Id != id)) return id;
                id++;
            }
        }
        public static void SetObject(Transform MiraShip)
        {
            Transform CommsTop = MiraShip.FindChild("Comms").FindChild("comms-top");
            CommsTop.gameObject.GetChildren().SetActiveAllObject("", false);
            CommsTop.GetComponent<SpriteRenderer>().sprite = ImageManager.AgarthagetSprite("Object_ComputerTable");
            GameObject.Destroy(CommsTop.GetComponent<PolygonCollider2D>());
            CommsTop.gameObject.AddComponent<PolygonCollider2D>();
            CommsTop.position = new Vector3(-11.3f, 23f, 0.1f);
            CommsTop.localScale *= 0.5f;
            Transform CommsTop2 = GameObject.Instantiate(CommsTop,CommsTop.parent);
            CommsTop2.position = new Vector3(-7.8f, 23f, 0.1f);

            Transform Template = GameObject.Instantiate(MiraShip.FindChild("Office").FindChild("computerTableB"));
            //GameObject.Destroy(Template.GetComponent<PolygonCollider2D>());
            Template.gameObject.SetActive(true);
            Template.localScale *= 0.5f;

            Transform Labo_Object_fossil_1 = GameObject.Instantiate(Template);
            Labo_Object_fossil_1.position = new Vector3(19.8f, 3.4f, 0.1f);
            Labo_Object_fossil_1.GetComponent<SpriteRenderer>().sprite = ImageManager.AgarthagetSprite("Object_fossil_1");
            Labo_Object_fossil_1.gameObject.AddComponent<PolygonCollider2D>();
            Labo_Object_fossil_1.name = "Object_fossil_1";

            Transform Labo_Object_fossil_2 = GameObject.Instantiate(Template);
            Labo_Object_fossil_2.position = new Vector3(20.9f, 3.4f, 0.1f);
            Labo_Object_fossil_2.GetComponent<SpriteRenderer>().sprite = ImageManager.AgarthagetSprite("Object_fossil_2");
            Labo_Object_fossil_2.gameObject.AddComponent<PolygonCollider2D>();
            Labo_Object_fossil_2.name = "Object_fossil_2";

            Transform Labo_Object_kitchen_1 = GameObject.Instantiate(Template);
            Labo_Object_kitchen_1.position = new Vector3(20.7f, 9.2f, 0.1f);
            Labo_Object_kitchen_1.GetComponent<SpriteRenderer>().sprite = ImageManager.AgarthagetSprite("Object_kitchen_1");
            Labo_Object_kitchen_1.gameObject.AddComponent<PolygonCollider2D>();
            Labo_Object_kitchen_1.name = "Object_kitchen_1";
            Labo_Object_kitchen_1.localScale *= 1.75f;

            Transform Labo_Object_shelves_1 = GameObject.Instantiate(Template);
            Labo_Object_shelves_1.position = new Vector3(22.8f, 9f, 0.1f);
            Labo_Object_shelves_1.GetComponent<SpriteRenderer>().sprite = ImageManager.AgarthagetSprite("Object_shelves_1");
            Labo_Object_shelves_1.gameObject.AddComponent<PolygonCollider2D>();
            Labo_Object_shelves_1.name = "Object_shelves_1";
            Labo_Object_shelves_1.localScale *= 1.75f;

            Transform Labo_Object_LaboTable_1 = GameObject.Instantiate(Template);
            Labo_Object_LaboTable_1.position = new Vector3(21f, 7.9f, 0.1f);
            Labo_Object_LaboTable_1.GetComponent<SpriteRenderer>().sprite = ImageManager.AgarthagetSprite("Object_LaboTable_1");
            Labo_Object_LaboTable_1.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(4, 1);
            Labo_Object_LaboTable_1.name = "Object_LaboTable_1";
            Labo_Object_LaboTable_1.localScale *= 0.75f;

            GameObject.Destroy(Template);
            Template = null;

        }
        private static List<Vector3> DummyPositions = new List<Vector3>() { new Vector3(10.7f, 7.1f, 0f),new Vector3(11.8f, 15.6f, 0f),new Vector3(-0.2f, 8.5f, 0f),new Vector3(13.7f, 0.38f, 0f),new Vector3(21.7f, 15.6f, 0f),new Vector3(-10.7f, 16f, 0f) };
        public static void SetDummy()
        {
            new LateTask(() =>
            {
                for (int i = 1; i < 7; i++)
                {
                    if (DummyPositions.Count >= i)
                    {
                        GameObject obj = GameObject.Find(TranslationController.Instance.GetString(StringNames.Dummy) + " " + i.ToString());
                        obj.transform.position = DummyPositions[i - 1];
                    }
                }
            }, 0f, "SetDummyPosition");
        }
        public static void AddCamera(this Transform Sec,StringNames name,Vector3 pos)
        {
            var referenceCamera = Sec.FindChild("camoff").GetComponent<SurvCamera>();
            byte[] buff = new byte[sizeof(float) * 2];
            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            var camera = UnityEngine.Object.Instantiate<SurvCamera>(referenceCamera);
            camera.transform.position = new Vector3(position.x, position.y, referenceCamera.transform.position.z - 1f);
            camera.CamName = $"Security Camera";
            //camera.Offset = new Vector3(0f, 0f, camera.Offset.z);
            camera.NewName = name;
            //camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 
            List<SurvCamera> camlist = ShipStatus.Instance.AllCameras.ToList();
            camlist.Add(camera);
            ShipStatus.Instance.AllCameras = camlist.ToArray();
        }
        public static void SetCamera()
        {
            //カメラ生成
            Transform Sec = MapLoader.AirshipObject.transform.FindChild("Security");
            SuperNewRolesPlugin.Logger.LogInfo("ドア数:" + MapLoader.Airship.AllDoors.Count);
            Transform newcam = UnityEngine.Object.Instantiate(Sec.FindChild("task_cams"));
            newcam.transform.position = new Vector3(16.3f, 7.3f, 0.1f);
            newcam.gameObject.AddComponent<PolygonCollider2D>();
            Sec.AddCamera(StringNames.Admin, new Vector3(-0.9f, 19.31f, 0f));
            Sec.AddCamera(StringNames.Security, new Vector3(-0.2f, 4.5f, 0f));
            Sec.AddCamera(StringNames.LogNorth, new Vector3(7.3f, 15f, 0f));
            Sec.AddCamera(StringNames.ExileTextPP, new Vector3(23.8f, 10f, 0f));
        }
        public static int GetAvailableId()
        {
            var id = 0;
            if (ShipStatus.Instance.AllVents.Count == 0) return 0;
            while (true)
            {
                if (ShipStatus.Instance.AllVents.All(v => v.Id != id)) return id;
                id++;
            }
        }
        public static Vent SetUpVent(this Transform ventobj)
        {
            Vent vent = ventobj.gameObject.GetComponent<Vent>();
            SetUpVent(vent);
            var allVents = ShipStatus.Instance.AllVents.ToList();
            allVents.Add(vent);
            ShipStatus.Instance.AllVents = allVents.ToArray();
            return vent;

        }
        public static void SetUpVent(this Vent vent)
        {
            vent.Id = GetAvailableId();
        }
        public static void SetVent(Transform Miraship)
        {
            ShipStatus.Instance.AllVents = new List<Vent>().ToArray();
            Transform AdminVentObject = Miraship.FindChild("Admin").FindChild("AdminVent"); 
            Vent AdminVent = SetUpVent(AdminVentObject);

            Transform Locker = Miraship.FindChild("Locker");
            Locker.gameObject.GetChildren().SetActiveAllObject("LockerVent", false);
            Transform LockerVentObject = Locker.FindChild("LockerVent");
            //LockerVentObject.name = "SecurityVent";
            Vent SecurityVent = SetUpVent(LockerVentObject);

            Transform Reactor = Miraship.FindChild("Reactor");
            Reactor.gameObject.GetChildren().SetActiveAllObject("ReactorVent", false);
            Transform ReactorVentObject = Reactor.FindChild("ReactorVent");
            //ReactorVentObject.name = "CommunityVent";
            Vent CommunityVent = SetUpVent(ReactorVentObject);

            Transform ToolVentObject = Miraship.FindChild("Office").FindChild("OfficeVent");
            //ToolVentObject.name = "ToolVent";
            ToolVentObject.gameObject.SetActive(true);
            Vent ToolRoomVent = SetUpVent(ToolVentObject);

            Transform O2VentObject = Miraship.FindChild("MedBay").FindChild("MedVent");
            //O2VentObject.name = "O2Vent";
            Vent O2RoomVent = SetUpVent(O2VentObject);

            Transform MedbayVentObject = Miraship.FindChild("Garden").FindChild("AgriVent");
            //MedbayVentObject.name = "MedbayVent";
            Vent MedbayRoomVent = SetUpVent(MedbayVentObject);

            Transform WareHouseVentObject = Miraship.FindChild("LaunchPad").FindChild("LaunchVent");
            //WareHouseVentObject.name = "WareHouseVent";
            Vent WareHouseVent = SetUpVent(WareHouseVentObject);

            Transform WorkRoomVentObject = Miraship.FindChild("Cafe").FindChild("BalconyVent");
            //WorkRoomVentObject.name = "WorkRoomVent";
            WorkRoomVentObject.gameObject.SetActive(true);
            Vent WorkRoomVent = SetUpVent(WorkRoomVentObject);

            Transform MeetingRoomVentObject = Miraship.FindChild("Decontam").FindChild("DeconVent");
            //WorkRoomVentObject.name = "WorkRoomVent";
            Vent MeetingRoomVent = SetUpVent(MeetingRoomVentObject);

            Transform ElecRoomVentObject = Miraship.FindChild("Laboratory").FindChild("LabVent");
            //WorkRoomVentObject.name = "WorkRoomVent";
            Vent ElecRoomVent = SetUpVent(ElecRoomVentObject);

            Transform LaboVentObject = Miraship.FindChild("SkyBridge").FindChild("YHallRightVent");
            Vent LaboRoomVent = SetUpVent(LaboVentObject);

            AdminVent.Right = SecurityVent;
            AdminVent.Left = null;
            AdminVent.Center = null;

            SecurityVent.Right = AdminVent;
            SecurityVent.Left = null;
            SecurityVent.Center = null;

            CommunityVent.Right = null;
            CommunityVent.Left = null;
            CommunityVent.Center = null;

            ToolRoomVent.Right = null;
            ToolRoomVent.Left = null;
            ToolRoomVent.Center = null;

            O2RoomVent.Right = MedbayRoomVent;
            O2RoomVent.Left = null;
            O2RoomVent.Center = null;

            MedbayRoomVent.Right = O2RoomVent;
            MedbayRoomVent.Left = null;
            MedbayRoomVent.Center = null;

            WareHouseVent.Right = WorkRoomVent;
            WareHouseVent.Left = null;
            WareHouseVent.Center = null;

            WorkRoomVent.Right = WareHouseVent;
            WorkRoomVent.Left = null;
            WorkRoomVent.Center = null;

            MeetingRoomVent.Right = ElecRoomVent;
            MeetingRoomVent.Left = LaboRoomVent;
            MeetingRoomVent.Center = null;

            ElecRoomVent.Right = MeetingRoomVent;
            ElecRoomVent.Left = LaboRoomVent;
            ElecRoomVent.Center = null;

            LaboRoomVent.Right = MeetingRoomVent;
            LaboRoomVent.Left = ElecRoomVent;
            LaboRoomVent.Center = null;

            AdminVentObject.transform.position = new Vector3(12.3f, 4f, 0.1f);
            AdminVentObject.localScale = new Vector3(1.2f,1.2f,1.2f);

            LockerVentObject.transform.position = new Vector3(15.3f, 4f, 0.1f);
            LockerVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            ReactorVentObject.transform.position = new Vector3(1.47f, 22.8f, 0.1f);
            ReactorVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            ToolVentObject.transform.position = new Vector3(2.2f, -0.5f, 0.1f);
            ToolVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            O2VentObject.transform.position = new Vector3(1.95f, 9.1f, 0.1f);
            O2VentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            MedbayVentObject.transform.position = new Vector3(1.95f, 13f, 0.1f);
            MedbayVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            WareHouseVentObject.transform.position = new Vector3(-9.5f, 0.1f, 0.1f);
            WareHouseVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            WorkRoomVentObject.transform.position = new Vector3(-9.5f, 22.9f, 0.1f);
            WorkRoomVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            MeetingRoomVentObject.transform.position = new Vector3(15.8f, 11.5f, 0.1f);
            MeetingRoomVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            ElecRoomVentObject.transform.position = new Vector3(19.6f, 11.8f, 0.1f);
            ElecRoomVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);

            LaboVentObject.transform.position = new Vector3(20.1f, 8.6f, 0.1f);
            LaboVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
    }
}
