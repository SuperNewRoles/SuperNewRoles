using SuperNewRoles.Buttons;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Doorr
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.DoorrDoorButton.MaxTimer = RoleClass.Doorr.CoolTime;
            RoleClass.Doorr.ButtonTimer = DateTime.Now;
        }
        public static bool isDoorr(PlayerControl Player)
        {
            if (RoleClass.Doorr.DoorrPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool CheckTarget()
        {
            PlainDoor door = GetDoor();
            if (door == null) return false; else return true;
        }
        public static void DoorrBtn()
        {
            PlainDoor door = GetDoor();
            if (door != null)
            {
                door.SetDoorway(!door.Open);
            }
        }
        private static PlainDoor GetDoor()
        {
            Vector3 position = PlayerControl.LocalPlayer.transform.position;
            SuperNewRolesPlugin.Logger.LogInfo(position);
            foreach (PlainDoor door in ShipStatus.Instance.AllDoors)
            {
                Vector3 Doorposition = door.transform.position;
                if ((position.x + 2 >= Doorposition.x) && (Doorposition.x >= position.x - 2))
                {
                    if ((position.y + 2 >= Doorposition.y) && (Doorposition.y >= position.y - 2))
                    {
                        if ((position.z + 2 >= Doorposition.z) && (Doorposition.z >= position.z - 2))
                        {
                            return door;
                        }
                    }
                }
            }
            return null;
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.DoorrDoorButton.MaxTimer = RoleClass.Doorr.CoolTime;
            RoleClass.Doorr.ButtonTimer = DateTime.Now;
        }
    }
}
