using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    public static class ChangeRoomArea
    {
        public static void Change(Transform MiraShip)
        {
            Transform Cafe = MiraShip.FindChild("Cafe");
            PolygonCollider2D CafeCol = Cafe.gameObject.GetComponent<PolygonCollider2D>();
            CafeCol.points = new Vector2[] { new Vector2(1f, 4.9f), new Vector2(3.45f, 4.9f), new Vector2(3.45f, -5.15f), new Vector2(-3.45f, -5.15f), new Vector2(-3.45f, -1.8f), new Vector2(-3.42f, -0.4f), new Vector2(-3.2f, 3.3f), new Vector2(-1.65f, 5f), new Vector2(-0.3f, 5f), new Vector2(1f, 4.9f) };

            Transform Comms = MiraShip.FindChild("Comms");
            PolygonCollider2D CommsCol = Comms.gameObject.AddComponent<PolygonCollider2D>();
            CommsCol.isTrigger = true;
            CommsCol.points = new Vector2[] {new Vector2(-18.5f,19.5f), new Vector2(-18.5f, 16.7f),new Vector2(-13.5f, 16.7f), new Vector2(-13.5f, 19.5f), new Vector2(-18.5f, 19.5f) };
            Comms.GetComponent<SkeldShipRoom>().roomArea = CommsCol;

            Transform MedBay = MiraShip.FindChild("MedBay");
            PolygonCollider2D MedBayCol = MedBay.gameObject.AddComponent<PolygonCollider2D>();
            MedBayCol.isTrigger = true;
            MedBayCol.points = new Vector2[] { new Vector2(-18.5f, 19.5f), new Vector2(-18.5f, 16.7f), new Vector2(-13.5f, 16.7f), new Vector2(-13.5f, 19.5f), new Vector2(-18.5f, 19.5f) };
            MedBay.GetComponent<SkeldShipRoom>().roomArea = MedBayCol;
        }
        public static void ChangeAdmin(Transform Map)
        {
            Transform CountOverlay = Map.FindChild("CountOverlay");

            CountOverlay.FindChild("Cafeteria").localPosition = new Vector3(1.05f, 0.75f, 0f);
            CountOverlay.FindChild("Comms").localPosition = new Vector3(-2.25f, 2.36f, -20f);
        }
    }
}
