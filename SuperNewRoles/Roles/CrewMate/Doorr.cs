using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Doorr
    {
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.DoorrDoorButton.MaxTimer = CachedPlayer.LocalPlayer.Data.Role.IsImpostor ? RoleClass.EvilDoorr.CoolTime : RoleClass.Doorr.CoolTime;
            HudManagerStartPatch.DoorrDoorButton.Timer = HudManagerStartPatch.DoorrDoorButton.MaxTimer;
        }
        public static bool IsDoorr(PlayerControl Player)
        {
            return Player.GetRole() is RoleId.Doorr or RoleId.EvilDoorr;
        }
        public static bool CheckTarget()
        {
            PlainDoor door = GetDoor();
            return door != null;
        }
        public static void DoorrBtn()
        {
            Logger.Info("ボタンクリック","DoorrBtn");
            PlainDoor door = GetDoor();
            Logger.Info($"nullチェック:{door != null}", "DoorrBtn");
            if (door != null)
            {
                door.RpcSetDoorway(!door.Open);
            }
        }
        private static float IsPos(Vector3 mypos, PlainDoor Door, float distance)
        {
            var Distance = Vector3.Distance(mypos, Door.transform.position);
            return Distance <= distance ? Distance : 0f;
        }
        private static PlainDoor GetDoor()
        {
            return GameObject.FindObjectsOfType<DoorConsole>().ToArray().FirstOrDefault(x => {
                if (x.MyDoor == null) return false;
                float num = Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), x.transform.position);
                return num <= x.UsableDistance;
            })?.MyDoor;
        }
        public static void EndMeeting()
        {
            HudManagerStartPatch.DoorrDoorButton.MaxTimer = CachedPlayer.LocalPlayer.Data.Role.IsImpostor ? RoleClass.EvilDoorr.CoolTime : RoleClass.Doorr.CoolTime;
            HudManagerStartPatch.DoorrDoorButton.Timer = HudManagerStartPatch.DoorrDoorButton.MaxTimer;
        }
    }
}