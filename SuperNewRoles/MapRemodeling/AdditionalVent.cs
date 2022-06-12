using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Mode;
using SuperNewRoles.MapOptions;
using HarmonyLib;

//参考=>https://github.com/haoming37/TheOtherRoles-GM-Haoming/blob/haoming-main/TheOtherRoles/Objects/AdditionalVents.cs

namespace SuperNewRoles.MapRemodeling
{
    public class AdditionalVents
    {
        public Vent vent;
        public static System.Collections.Generic.List<AdditionalVents> AllVents = new();
        public static bool flag = false;
        public AdditionalVents(Vector3 p)
        {
            // Create the vent
            var referenceVent = UnityEngine.Object.FindObjectOfType<Vent>();
            vent = UnityEngine.Object.Instantiate<Vent>(referenceVent);
            vent.transform.position = p;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            Vent tmp = ShipStatus.Instance.AllVents[0];
            vent.EnterVentAnim = tmp.EnterVentAnim;
            vent.ExitVentAnim = tmp.ExitVentAnim;
            vent.Offset = new Vector3(0f, 0.25f, 0f);
            vent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
            var allVentsList = ShipStatus.Instance.AllVents.ToList();
            allVentsList.Add(vent);
            ShipStatus.Instance.AllVents = allVentsList.ToArray();
            vent.gameObject.SetActive(true);
            vent.name = "AdditionalVent_" + vent.Id;
            vent.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            AllVents.Add(this);
        }

        public static void AddAdditionalVents()
        {
            if (AdditionalVents.flag) return;
            AdditionalVents.flag = true;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            System.Console.WriteLine("AddAdditionalVents");

            // Polusにベントを追加する

            //if (PlayerControl.GameOptions.MapId == 2 /*&& CustomOptionHolder.additionalVents.getBool()*/)
            /*{
                AdditionalVents vents1 = new AdditionalVents(new Vector3(36.54f, -21.77f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // Specimen
                AdditionalVents vents2 = new AdditionalVents(new Vector3(16.64f, -2.46f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // InitialSpawn
                AdditionalVents vents3 = new AdditionalVents(new Vector3(26.67f, -17.54f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // Vital
                vents1.vent.Left = vents3.vent; // Specimen - Vital
                vents2.vent.Center = vents3.vent; // InitialSpawn - Vital
                vents3.vent.Right = vents1.vent; // Vital - Specimen
                vents3.vent.Left = vents2.vent; // Vital - InitialSpawn
            }*/

            // AirShipにベントを追加する
            if (PlayerControl.GameOptions.MapId == 4 && MapOptions.MapOption.AirShipAdditionalVents.getBool())
            {
                SuperNewRolesPlugin.Logger.LogInfo("べんとおおおお");
                AdditionalVents vents1 = new(new Vector3(23.5483f, -5.589f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // 診察室
                AdditionalVents vents2 = new(new Vector3(24.8562f, 5.2692f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // ラウンジ
                AdditionalVents vents3 = new(new Vector3(5.9356f, 3.0133f , PlayerControl.LocalPlayer.transform.position.z + 1f)); // メイン
                vents1.vent.Right = vents2.vent;//診察-ラウンジ
                vents2.vent.Left = vents1.vent;//ラウンジ-診察
                vents2.vent.Right = vents3.vent;//ラウンジ-メイン
                vents3.vent.Right = vents2.vent; // メイン-ラウンジ
            }
        }

        public static void ClearAndReload()
        {
            System.Console.WriteLine("additionalVentsClearAndReload");
            flag = false;
            AllVents = new List<AdditionalVents>();
        }
    }
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static void Prefix(IntroCutscene __instance)
        {
            // ベントを追加する
            AdditionalVents.AddAdditionalVents();
        }
    }
}
