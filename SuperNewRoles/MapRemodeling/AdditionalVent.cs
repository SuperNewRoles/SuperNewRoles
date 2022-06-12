using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Mode;
using SuperNewRoles.MapOptions;

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
                AdditionalVents vents1 = new (new Vector3(23.44f, -5.084f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // 診察室
                AdditionalVents vents2 = new (new Vector3(24.70f, 4.98f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // ラウンジ
                AdditionalVents vents3 = new (new Vector3(5.70f, 3.51f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // メイン
                vents1.vent.Right = vents2.vent;
                vents2.vent.Left = vents1.vent;
                vents3.vent.Right = vents1.vent; // Vital - Specimen
                vents3.vent.Left = vents2.vent; // Vital - InitialSpawn
            }
        }

        public static void clearAndReload()
        {
            System.Console.WriteLine("additionalVentsClearAndReload");
            flag = false;
            AllVents = new List<AdditionalVents>();
        }
    }
}
