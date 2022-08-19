using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BepInEx.IL2CPP.Utils;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate
{
    public static class SeeThroughPerson
    {
        public static void AwakePatch()
        {
            foreach (PlainDoor door in ShipStatus.Instance.AllDoors)
            {
                door.animator.Play(door.CloseDoorAnim);
                new LateTask(() =>
                {
                    var newcollider = new GameObject("Door-SeeThroughPersonCollider-" + door.transform.position.x + "." + door.transform.position.y + "." + door.Id);
                    newcollider.transform.position = door.transform.position;
                    var TempCollider = door.gameObject.AddComponent<PolygonCollider2D>();
                    newcollider.AddComponent<EdgeCollider2D>().points = TempCollider.points;
                    GameObject.Destroy(TempCollider);
                    door.myCollider.isTrigger = true;
                    RoleClass.SeeThroughPerson.Objects.Add(newcollider.GetComponent<EdgeCollider2D>());
                    door.animator.Play(door.OpenDoorAnim);
                }, 0.5f);
            }
        }
        public static void FixedUpdate()
        {
            foreach (PlainDoor door in ShipStatus.Instance.AllDoors)
            {
                var obj = RoleClass.SeeThroughPerson.Objects.Find(data => data.name == "Door-SeeThroughPersonCollider-" + door.transform.position.x + "." + door.transform.position.y + "." + door.Id);
                if (obj == null) continue;
                door.myCollider.isTrigger = true;
                obj.enabled = !door.Open;
            }
        }
    }
}