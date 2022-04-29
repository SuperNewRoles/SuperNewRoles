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

        /*---------------------------------------------------------------------------------------------------------------------*/

        public static void ResetCoolDown()
        {
            HudManagerStartPatch.LighterLightOnButton.MaxTimer = RoleClass.Lighter.CoolTime;
            RoleClass.Lighter.ButtonTimer = DateTime.Now;
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

        public static void EndMeeting()
        {
            HudManagerStartPatch.EvilLighterLightOffButton.MaxTimer = RoleClass.EvilLighter.LightOutCooldown;
            RoleClass.EvilLighter.ButtonTimer = DateTime.Now;
            RoleClass.EvilLighter.IsLightOff = false;
        }




    }

}

//コード倉庫
//SuperNewRoles.Buttons
//lightsOutButton.MaxTimer = Trickster.lightsOutCooldown;
//lightsOutButton.EffectDuration = Trickster.lightsOutDuration;
//
//