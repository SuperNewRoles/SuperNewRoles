

using HarmonyLib;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    class MiniMapPatch
    {
        public static void MinimapChange(MapBehaviour __instance)
        {
            Transform MapObject = GameObject.Find("HqMap(Clone)").transform;
            Transform Background = MapObject.FindChild("Background");
            Transform RoomNames = MapObject.FindChild("RoomNames");
            SpriteRenderer render = Background.GetComponent<SpriteRenderer>();
            render.sprite = ImageManager.MiniMap;
            render.transform.localScale *= 0.75f;
            //AdminRoomName
            Transform AdminRoom = RoomNames.FindChild("Admin");
            AdminRoom.GetComponent<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin);
            AdminRoom.localPosition = new Vector3(0.2f, -0.95f, 0f);

            //CommsRoomName
            Transform CommsRoom = RoomNames.FindChild("Comms");
            CommsRoom.localPosition = new Vector3(-1.72f, 1.95f, 0f);//1.38f, -0.95f, 0f);

            //LaboratoryRoomName
            Transform LaboratoryRoom = RoomNames.FindChild("Laboratory");
            LaboratoryRoom.localPosition = new Vector3(2.35f, -0.95f, 0f);

            //MedBayRoomName
            Transform MedBayRoom = RoomNames.FindChild("MedBay");
            MedBayRoom.localPosition = new Vector3(-1.7f, 0.52f, 0f);

            //SecurityRoomName
            Transform SecurityRoom = RoomNames.FindChild("Storage");
            SecurityRoom.name = "Security";
            new LateTask(() =>
            SecurityRoom.GetComponent<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Security)
            , 0f, "SetMapText");
            SecurityRoom.localPosition = new Vector3(1.38f, -0.95f, 0f);
            
            //LifeSuppRoomName
            Transform LifeSuppRoom = RoomNames.FindChild("Lockers");
            LifeSuppRoom.name = "LifeSupp";
            new LateTask(() =>
            LifeSuppRoom.GetComponent<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LifeSupp)
            , 0f, "SetMapText");
            LifeSuppRoom.localPosition = new Vector3(-1.72f, -0.48f, 0f);

            //MeetingRoomName
            Transform MeetingRoom = RoomNames.FindChild("Decontam");
            MeetingRoom.name = "Meeting";
            new LateTask(() =>
            MeetingRoom.GetComponent<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.MeetingRoom)
            , 0f, "SetMapText");
            MeetingRoom.localPosition = new Vector3(0.8f, 0.85f, 0f);

            //ElectricalRoomName
            Transform ElectricalRoom = RoomNames.FindChild("LaunchPad");
            ElectricalRoom.name = "Electrical";
            new LateTask(() =>
            ElectricalRoom.GetComponent<TextMeshPro>().text = FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Electrical)
            , 0f, "SetMapText");
            ElectricalRoom.localPosition = new Vector3(2.27f, 0.85f, 0);

            //ToolRoomName
            Transform ToolRoom = RoomNames.FindChild("Office");
            ToolRoom.name = "Tool";
            new LateTask(() =>
            ToolRoom.GetComponent<TextMeshPro>().text = ModTranslation.getString("Agartha_ToolRoom")
            , 0f, "SetMapText");
            ToolRoom.localPosition = new Vector3(-1.72f, -1.9f, 0f);

            //WareHouseName
            Transform WareHouse = RoomNames.FindChild("Balcony");
            WareHouse.name = "WareHouse";
            new LateTask(() =>
            WareHouse.GetComponent<TextMeshPro>().text = ModTranslation.getString("Agartha_WareHouse")
            , 0f, "SetMapText");
            WareHouse.localPosition = new Vector3(-3.37f, -1.2f, 0);

            //WorkRoomName
            Transform WorkRoom = RoomNames.FindChild("Reactor");
            WorkRoom.name = "WorkRoom";
            new LateTask(() =>
            WorkRoom.GetComponent<TextMeshPro>().text = ModTranslation.getString("Agartha_WorkRoom")
            , 0f, "SetMapText");
            WorkRoom.localPosition = new Vector3(-3.37f, 1.25f, 0);

            RoomNames.FindChild("Cafeteria").gameObject.SetActive(false);
            RoomNames.FindChild("Greenhouse").gameObject.SetActive(false);
        }
    }
}
