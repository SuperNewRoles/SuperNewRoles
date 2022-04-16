using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    public static class SetPosition
    {
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
            camera.Offset = new Vector3(0f, 0f, camera.Offset.z);
            camera.NewName = name;
            camera.transform.localRotation = new Quaternion(0, 0, 1, 1); // Polus and Airship 
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
            Sec.AddCamera(StringNames.Admin, new Vector3(-0.2f, 17.8f, 0f));
            Sec.AddCamera(StringNames.Admin, new Vector3(-0.2f, 4.5f, 0f));
            Sec.AddCamera(StringNames.Admin, new Vector3(7.3f, 15f, 0f));
            Sec.AddCamera(StringNames.Admin, new Vector3(23.8f, 10f, 0f));
        }
        public static int GetAvailableId()
        {
            var id = 0;

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
            Transform AdminVentObject = Miraship.FindChild("Admin").FindChild("AdminVent"); 
            Vent AdminVent = AdminVentObject.GetComponent<Vent>();

            Transform Locker = Miraship.FindChild("Locker");
            Locker.gameObject.GetChildren().SetActiveAllObject("LockerVent", false);
            Transform LockerVentObject = Locker.FindChild("LockerVent");
            LockerVentObject.name = "SecurityVent";
            Vent SecurityVent = LockerVentObject.GetComponent<Vent>();

            Transform Reactor = Miraship.FindChild("Reactor");
            Reactor.gameObject.GetChildren().SetActiveAllObject("ReactorVent", false);
            Transform ReactorVentObject = Reactor.FindChild("ReactorVent");
            ReactorVentObject.name = "CommunityVent";
            Vent CommunityVent = ReactorVentObject.GetComponent<Vent>();

            Transform ToolVentObject = GameObject.Instantiate(ReactorVentObject);
            ToolVentObject.name = "ToolVent";
            Vent ToolRoomVent = SetUpVent(ToolVentObject);

            Transform O2VentObject = GameObject.Instantiate(ReactorVentObject);
            O2VentObject.name = "O2Vent";
            Vent O2RoomVent = SetUpVent(O2VentObject);

            Transform MedbayVentObject = GameObject.Instantiate(ReactorVentObject);
            MedbayVentObject.name = "MedbayVent";
            Vent MedbayRoomVent = SetUpVent(MedbayVentObject);

            Transform WareHouseVentObject = GameObject.Instantiate(ReactorVentObject);
            WareHouseVentObject.name = "WareHouseVent";
            Vent WareHouseVent = SetUpVent(WareHouseVentObject); ;

            Transform WorkRoomVentObject = GameObject.Instantiate(ReactorVentObject);
            WorkRoomVentObject.name = "WorkRoomVent";
            Vent WorkRoomVent = SetUpVent(WorkRoomVentObject);

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

            WorkRoomVentObject.transform.position = new Vector3(-9.5f, 22.8f, 0.1f);
            WorkRoomVentObject.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
    }
}
