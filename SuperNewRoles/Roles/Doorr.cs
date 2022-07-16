using System;
using System.Collections.Generic;
using SuperNewRoles.Buttons;
using UnityEngine;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class Doorr
    {
        public static void ResetCoolDown()
        {
            if (CachedPlayer.LocalPlayer.Data.Role.IsImpostor)
            {
                HudManagerStartPatch.DoorrDoorButton.MaxTimer = RoleClass.EvilDoorr.CoolTime;
            }
            else
            {
                HudManagerStartPatch.DoorrDoorButton.MaxTimer = RoleClass.Doorr.CoolTime;
            }
            RoleClass.Doorr.ButtonTimer = DateTime.Now;
        }
        public static bool isDoorr(PlayerControl Player)
        {
            if (Player.isRole(RoleId.Doorr) || Player.isRole(RoleId.EvilDoorr))
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
        private static float IsPos(Vector3 mypos, PlainDoor Door, float distance)
        {
            var Distance = Vector3.Distance(mypos, Door.transform.position);
            if (Distance <= distance)
            {
                return Distance;
            }
            return 0f;
        }
        private static PlainDoor GetDoor()
        {
            Vector3 position = CachedPlayer.LocalPlayer.transform.position;
            List<PlainDoor> selectdoors = new();
            foreach (PlainDoor door in MapUtilities.CachedShipStatus.AllDoors)
            {
                var getispos = IsPos(position, door, 2);
                if (getispos != 0)
                {
                    selectdoors.Add(door);
                }
            }
            bool flag = true;
            while (flag)
            {
                if (selectdoors.Count >= 2)
                {
                    if (IsPos(position, selectdoors[0], 2) >= IsPos(position, selectdoors[1], 2))
                    {
                        selectdoors.Remove(selectdoors[0]);
                    }
                    else
                    {
                        selectdoors.Remove(selectdoors[1]);
                    }
                }
                else
                {
                    flag = false;
                }
            }
            return selectdoors[0];
        }
        public static void EndMeeting()
        {
            if (CachedPlayer.LocalPlayer.Data.Role.IsImpostor)
            {
                HudManagerStartPatch.DoorrDoorButton.MaxTimer = RoleClass.EvilDoorr.CoolTime;
            }
            else
            {
                HudManagerStartPatch.DoorrDoorButton.MaxTimer = RoleClass.Doorr.CoolTime;
            }
            RoleClass.Doorr.ButtonTimer = DateTime.Now;
        }
    }
}
