using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using SuperNewRoles.Patches;
using System.Reflection;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;


namespace SuperNewRoles.Roles
{
	class EvilLighter
	{


        public static void ResetCoolDown()
        {
            HudManagerStartPatch.EvilLighterLightOffButton.MaxTimer = RoleClass.EvilLighter.LightOutCooldown;
            RoleClass.EvilLighter.ButtonTimer = DateTime.Now;
        }
        public static bool isEvilLighter(PlayerControl Player)
        {
            if (RoleClass.EvilLighter.EvilLighterPlayer.IsCheckListPlayerControl(Player))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void LightOffStart()
        {
            RoleClass.EvilLighter.IsLightOff = true;
        }

        public static void LightOffOffEnd()
        {
            if (!RoleClass.EvilLighter.IsLightOff) return;
            RoleClass.EvilLighter.IsLightOff = false;

        }

        public static void EndMeeting()
        {
            HudManagerStartPatch.EvilLighterLightOffButton.MaxTimer = RoleClass.EvilLighter.LightOutCooldown;
            RoleClass.EvilLighter.ButtonTimer = DateTime.Now;
            RoleClass.EvilLighter.IsLightOff = false;
        }

        /*---------------------------------------------------------------------------------------------------------------------*/

        public static void EvilLighterButton()
        {
            if (Roles.RoleClass.EvilLighter.IsLightOff)
            {
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.EvilLighter.LightsOutDuration);
                Buttons.HudManagerStartPatch.EvilLighterLightOffButton.MaxTimer = Roles.RoleClass.EvilLighter.LightsOutDuration;
                Buttons.HudManagerStartPatch.EvilLighterLightOffButton.Timer = (float)((Roles.RoleClass.Lighter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.EvilLighterLightOffButton.Timer <= 0f)
                {
                    Roles.EvilLighter.LightOffOffEnd();
                    Roles.EvilLighter.ResetCoolDown();
                    Buttons.HudManagerStartPatch.EvilLighterLightOffButton.MaxTimer = Roles.RoleClass.EvilLighter.LightOutCooldown;
                    Roles.RoleClass.EvilLighter.IsLightOff = false;
                    Buttons.HudManagerStartPatch.EvilLighterLightOffButton.actionButton.cooldownTimerText.color = Color.white;
                    Roles.RoleClass.EvilLighter.ButtonTimer = DateTime.Now;
                }
            }
            else
            {
                if (Roles.RoleClass.EvilLighter.ButtonTimer == null)
                {
                    Roles.RoleClass.EvilLighter.ButtonTimer = DateTime.Now;
                }
                var TimeSpanDate = new TimeSpan(0, 0, 0, (int)Roles.RoleClass.EvilLighter.LightOutCooldown);
                Buttons.HudManagerStartPatch.LighterLightOnButton.Timer = (float)((Roles.RoleClass.EvilLighter.ButtonTimer + TimeSpanDate) - DateTime.Now).TotalSeconds;
                if (Buttons.HudManagerStartPatch.LighterLightOnButton.Timer <= 0f) Buttons.HudManagerStartPatch.EvilLighterLightOffButton.Timer = 0f; return;
            }
        }



    }

}

//コード倉庫
//SuperNewRoles.Buttons
//lightsOutButton.EffectDuration = Trickster.lightsOutDuration;
//
//