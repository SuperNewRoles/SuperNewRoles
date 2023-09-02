using System.Collections.Generic;
using System.Linq;
using Agartha;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedElecLeverTask
{
    [HarmonyPatch(typeof(ElecLeverGame))]
    public static class ElecLeverGamePatch
    {
        [HarmonyPatch(nameof(ElecLeverGame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(ElecLeverGame __instance)
        {
            if (!Main.IsCursed) return;
            List<Sprite> sprites = new(__instance.NumberIcons);
            sprites.RemoveAt(__instance.MyNormTask.Data.IndexOf((byte)__instance.ConsoleId));
            __instance.NumberImage.sprite = sprites.GetRandom();
        }
    }

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        public static readonly List<Vector3> Position = new()
        {
            new(-17.3343f, 1.5375f, 0.1f),
            new(-14.75f, -11.196f, 0.1f),
            new(-8.203f, 13.5493f, 0.1f),
            new(11.0889f, 16.4263f, 0.1f),
            new(13.6699f, -5.3365f, 0.1f),
            new(24.5441f, 10.4653f, 0.1f),
            new(31.7973f, 2.2524f, 0.1f),
        };

        public static readonly List<SystemTypes> Rooms = new()
        {
            SystemTypes.Cockpit,
            SystemTypes.ViewingDeck,
            SystemTypes.VaultRoom,
            SystemTypes.MeetingRoom,
            SystemTypes.Electrical,
            SystemTypes.Lounge,
            SystemTypes.CargoBay,
        };

        [HarmonyPatch(nameof(ShipStatus.Start)), HarmonyPostfix]
        public static void StartPostfix(ShipStatus __instance)
        {
            if (!Main.IsCursed) return;
            if (!MapData.IsMap(CustomMapNames.Airship)) return;

            Transform electrical = __instance.transform.Find("Electrical");
            List<Transform> levers = new();
            int i = 0;
            for (int j = 0; j < electrical.childCount; j++)
            {
                Transform child = electrical.GetChild(j);
                if (child.name == "task_electriclever (2)") Object.Destroy(child.gameObject);
                if (child.name != "task_electriclever") continue;
                child.position = Position[i];
                if (i == 2) child.localScale = new(1.2f, 1.2f, 1f);
                levers.Add(child);
                i++;
            }
            for (int j = 0; j < levers.Count; j++) levers[j].SetParent(__instance.AllRooms.ToList().Find(n => n.RoomId == Rooms[j]).transform);
        }
    }
}