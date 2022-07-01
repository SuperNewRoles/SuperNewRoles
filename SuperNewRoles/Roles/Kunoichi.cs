using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    internal static class Kunoichi
    {
        public static PlayerControl GetShootPlayer(float shotSize = 0.75f)
        {
            PlayerControl result = null;
            float num = 7;
            Vector3 pos;
            Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
            var mouseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                //自分自身は撃ち抜かれない
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;

                if (player.isDead()) continue;

                pos = player.transform.position - PlayerControl.LocalPlayer.transform.position;
                pos = new Vector3(
                    pos.x * MathF.Cos(mouseAngle) + pos.y * MathF.Sin(mouseAngle),
                    pos.y * MathF.Cos(mouseAngle) - pos.x * MathF.Sin(mouseAngle));
                if (Math.Abs(pos.y) < shotSize && (!(pos.x < 0)) && pos.x < num)
                {
                    num = pos.x;
                    result = player;
                }
            }
            return result;
        }
        public static void KillButtonClick()
        {
            if (!RoleClass.Kunoichi.Kunai.kunai.active) return;
            var shoot = GetShootPlayer();
            PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Kunoichi.KillCoolTime, RoleClass.Kunoichi.KillCoolTime);
            SuperNewRolesPlugin.Logger.LogInfo("ターゲット:"+shoot?.Data?.PlayerName);
            if (shoot != null)
            {
                if (!RoleClass.Kunoichi.HitCount.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId] = new();
                if (!RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId].ContainsKey(shoot.PlayerId)) RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId][shoot.PlayerId] = 0;
                RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId][shoot.PlayerId]++;
                if (RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId][shoot.PlayerId] >= RoleClass.Kunoichi.KillKunai)
                {
                    ModHelpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, shoot);
                    RoleClass.Kunoichi.HitCount[PlayerControl.LocalPlayer.PlayerId][shoot.PlayerId] = 0;
                }
            }
        }
        public static void Update()
        {
            Vector3 mouseDirection = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);
            var MouseAngle = Mathf.Atan2(mouseDirection.y, mouseDirection.x);
            var targetPosition = CachedPlayer.LocalPlayer.transform.position + new Vector3(0.8f * (float)Math.Cos(MouseAngle), 0.8f * (float)Math.Sin(MouseAngle));
            RoleClass.Kunoichi.Kunai.kunai.transform.position += (targetPosition - RoleClass.Kunoichi.Kunai.kunai.transform.position) * 0.4f;
            RoleClass.Kunoichi.Kunai.image.transform.eulerAngles = new Vector3(0f, 0f, (float)(MouseAngle * 360f / Math.PI / 2f));
            if (Math.Cos(MouseAngle) < 0.0)
            {
                if (RoleClass.Kunoichi.Kunai.image.transform.localScale.y > 0)
                    RoleClass.Kunoichi.Kunai.image.transform.localScale = new Vector3(1f, -1f);
            }
            else
            {
                if (RoleClass.Kunoichi.Kunai.image.transform.localScale.y < 0)
                    RoleClass.Kunoichi.Kunai.image.transform.localScale = new Vector3(1f, 1f);
            }

            if (PlayerControl.LocalPlayer.inVent)
                RoleClass.Kunoichi.Kunai.kunai.active = false;
        }
    }
}
