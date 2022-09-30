using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

//参考=>https://github.com/haoming37/TheOtherRoles-GM-Haoming/blob/haoming-main/TheOtherRoles/Objects/AdditionalVents.cs

namespace SuperNewRoles.MapCustoms
{
    public class AdditionalVents
    {
        public Vent vent;
        public static List<AdditionalVents> AllVents = new();
        public static bool flag = false;
        public AdditionalVents(Vector3 p)
        {
            // Create the vent
            var referenceVent = Object.FindObjectOfType<Vent>();
            vent = Object.Instantiate(referenceVent);
            vent.transform.position = p;
            vent.Left = null;
            vent.Right = null;
            vent.Center = null;
            Vent tmp = MapUtilities.CachedShipStatus.AllVents[0];
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
            if (flag) return;
            flag = true;
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            System.Console.WriteLine("AddAdditionalVents");

            //MiraHQにベントを追加する
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Mira) && MapCustom.MiraAdditionalVents.GetBool())
            {
                AdditionalVents vents1 = new(new Vector3(11.3518f, 10.4786f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // 研究室
                AdditionalVents vents2 = new(new Vector3(12.1288f, 7.2f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // Y字下
                AdditionalVents vents3 = new(new Vector3(19.574f, 17.3698f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // アドミン
                vents1.vent.Left = vents3.vent; // 研究室 - アドミン
                vents1.vent.Right = vents2.vent;// 研究室 - Y字下
                vents2.vent.Center = vents3.vent; // Y字下- アドミン
                vents2.vent.Left = vents1.vent; // Y字下- 研究室
                vents3.vent.Right = vents1.vent; // アドミン - 研究室
                vents3.vent.Left = vents2.vent; // アドミン - Y字下
            }

            // Polusにベントを追加する
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Polus) && MapCustom.PolusAdditionalVents.GetBool())
            {
                AdditionalVents vents1 = new(new Vector3(36.54f, -21.77f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // 標本室
                AdditionalVents vents2 = new(new Vector3(11.5522f, -21.1158f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // ウェポン
                AdditionalVents vents3 = new(new Vector3(26.67f, -17.54f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // バイタル
                vents1.vent.Left = vents3.vent; // 標本室 - バイタル
                vents1.vent.Right = vents2.vent;// 標本室 - ウェポン
                vents2.vent.Center = vents3.vent; // ウェポン- バイタル
                vents2.vent.Left = vents1.vent; // ウェポン- 標本室
                vents3.vent.Right = vents1.vent; // バイタル - 標本室
                vents3.vent.Left = vents2.vent; // バイタル - ウェポン
            }

            // AirShipにベントを追加する
            if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.Airship) && MapCustom.AirShipAdditionalVents.GetBool())
            {
                SuperNewRolesPlugin.Logger.LogInfo("べんとおおおお");
                AdditionalVents vents1 = new(new Vector3(23.5483f, -5.589f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // 診察室
                AdditionalVents vents2 = new(
                    new Vector3(CustomOptions.ConnectKillerOption.GetSelection() == 0 ? 24.8562f : 26.8562f, 5.2692f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // ラウンジ
                AdditionalVents vents3 = new(new Vector3(5.9356f, 3.0133f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // メイン
                vents1.vent.Right = vents2.vent;//診察-ラウンジ
                vents2.vent.Left = vents1.vent;//ラウンジ-診察
                vents2.vent.Right = vents3.vent;//ラウンジ-メイン
                vents3.vent.Right = vents2.vent; // メイン-ラウンジ

                AdditionalVents vents4 = new(new Vector3(6.7651f, -10.2f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // セキュ
                AdditionalVents vents5 = new(new Vector3(18.1884f, -3.991f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // エレキ
                AdditionalVents vents6 = new(new Vector3(21.1574f, -1.3543f, PlayerControl.LocalPlayer.transform.position.z + 1f)); // シャワー
                vents4.vent.Right = vents5.vent;//セキュ-エレキ
                vents5.vent.Left = vents4.vent;//エレキ-セキュ
                vents5.vent.Right = vents6.vent;//エレキ-シャワー
                vents6.vent.Left = vents5.vent;//シャワー-エレキF
            }
        }

        public static void ClearAndReload()
        {
            System.Console.WriteLine("additionalVentsClearAndReload");
            flag = false;
            AllVents = new List<AdditionalVents>();
        }
    }
}