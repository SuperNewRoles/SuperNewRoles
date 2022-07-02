using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.CustomOption;
using UnityEngine;

namespace SuperNewRoles.MapCustoms.Airship
{
    public static class SecretRoom
    {
        public static CustomOption.CustomOption SecretRoomOption;
        public static void CreateOption()
        {
            SecretRoomOption = CustomOption.CustomOption.Create(624, false, CustomOptionType.Generic, "SecretRoom", false, MapCustom.MapCustomOption);
        }
        public static void ShipStatusAwake(ShipStatus __instance)
        {
            if (__instance.Type != ShipStatus.MapType.Ship || SecretRoomOption.getBool())
            {
                Transform room = __instance.transform.FindChild("HallwayPortrait");
                Transform Walls = room.FindChild("Walls");
                Transform Shadows = room.FindChild("Shadows");
                EdgeCollider2D collider = Walls.GetComponentsInChildren<EdgeCollider2D>()[1];
                EdgeCollider2D newcollider = Walls.gameObject.AddComponent<EdgeCollider2D>();
                EdgeCollider2D newdoorcollider = Walls.gameObject.AddComponent<EdgeCollider2D>();
                Vector2[] OldPoints = collider.points;
                List<Vector2> points1 = new();
                List<Vector2> points2 = new();
                points1 = collider.points.ToArray()[..3].ToList();
                points1.Add(new Vector2(1.85f, -0.0783f));
                points1.Add(new Vector2(1.85f, 10f));
                points1.Add(new Vector2(3.05f, 10f));

                points2.Add(new Vector2(3.05f, 10f));
                points2.Add(new Vector2(3.05f, -0.0783f));
                points2.Add(new Vector2(5.3f, -0.0783f));
                points2.Add(new Vector2(5.3f, -0.2f));
                collider.points = points1.ToArray();
                newcollider.points = points2.ToArray();
                newdoorcollider.points = new Vector2[] { new Vector2(1.85f, -0.0783f), new Vector2(3.05f, -0.0783f) };

                EdgeCollider2D shadow = Shadows.GetComponentsInChildren<EdgeCollider2D>()[0];
                EdgeCollider2D newshadow = GameObject.Instantiate(shadow, Shadows);
                List<Vector2> shadow_new = shadow.points.ToArray()[..14].ToList();
                shadow_new.Add(new Vector2(1.64f, 0.8122f));
                shadow_new.Add(new Vector2(1.64f, 6f));

                List<Vector2> newshadow_new = new();
                newshadow_new.Add(new Vector2(3.15f, 6f));
                newshadow_new.Add(new Vector2(3.15f, 0.8332f));
                newshadow_new.AddRange(shadow.points.ToArray()[16..].ToList());
                newshadow.points = newshadow_new.ToArray();
                shadow.points = shadow_new.ToArray();

                Transform entranse = GameObject.Instantiate(__instance.transform.FindChild("Cockpit/cockpit_chair"), room);
                entranse.GetComponent<SpriteRenderer>().sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_entrance.png", 115f);
                entranse.localPosition = new Vector3(2.45f, 1.23f, -0.0007f);
                entranse.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                entranse.name = "secretroom_entranse";

                Transform Aisle = GameObject.Instantiate(entranse, room);
                Aisle.GetComponent<SpriteRenderer>().sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Aisle.png", 115f);
                Aisle.localPosition = new Vector3(2.45f, 4.35f, -0.1f);
                Aisle.localScale = new Vector3(1.5f, 200f, 1.5f);
                Aisle.name = "secretroom_aisle";

                Transform Room = GameObject.Instantiate(entranse, room);
                Room.GetComponent<SpriteRenderer>().sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.SecretRoom_Room.png", 115f);
                Room.localPosition = new Vector3(2.5326f, 7.9f, -0.09f);
                Room.localScale = new Vector3(1.44f, 1.44f, 1.44f);
                Room.name = "secretroom_room";

            }
        }
    }
}
