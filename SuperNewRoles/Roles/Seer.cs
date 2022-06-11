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
using SuperNewRoles.Roles;
using System.Text;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles
{
    class Seer
    //&MadSeer & EvilSeer & SeerFriends & JackalSeer & Sidekick(Seer)
    {


        private static Sprite SoulSprite;
        public static Sprite getSoulSprite()
        {
            if (SoulSprite) return SoulSprite;
            SoulSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Soul.png", 500f);
            return SoulSprite;
        }

        public static class ExileControllerWrapUpPatch
        {

            public static void WrapUpPostfix(GameData.PlayerInfo exiled)
            {
                var role = PlayerControl.LocalPlayer.getRole();
                if (role == RoleId.Seer || role == RoleId.MadSeer || role == RoleId.EvilSeer || role == RoleId.SeerFriends || role == RoleId.JackalSeer || role == RoleId.SidekickSeer)
                {
                    List<Vector3> DeadBodyPositions = new List<Vector3>();
                    bool limitSoulDuration = false;
                    float soulDuration = 0f;
                    switch (role)
                    {
                        case RoleId.Seer:
                            DeadBodyPositions = RoleClass.Seer.deadBodyPositions;
                            RoleClass.Seer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.Seer.limitSoulDuration;
                            soulDuration = RoleClass.Seer.soulDuration;
                            if (RoleClass.Seer.mode != 0 && RoleClass.Seer.mode != 2) return;
                            break;
                        case RoleId.MadSeer:
                            DeadBodyPositions = RoleClass.MadSeer.deadBodyPositions;
                            RoleClass.MadSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.MadSeer.limitSoulDuration;
                            soulDuration = RoleClass.MadSeer.soulDuration;
                            if (RoleClass.MadSeer.mode != 0 && RoleClass.MadSeer.mode != 2) return;
                            break;
                        case RoleId.EvilSeer:
                            DeadBodyPositions = RoleClass.EvilSeer.deadBodyPositions;
                            RoleClass.EvilSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.EvilSeer.limitSoulDuration;
                            soulDuration = RoleClass.EvilSeer.soulDuration;
                            if (RoleClass.EvilSeer.mode != 0 && RoleClass.EvilSeer.mode != 2) return;
                            break;
                        case RoleId.SeerFriends:
                            DeadBodyPositions = RoleClass.SeerFriends.deadBodyPositions;
                            RoleClass.SeerFriends.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.SeerFriends.limitSoulDuration;
                            soulDuration = RoleClass.SeerFriends.soulDuration;
                            if (RoleClass.SeerFriends.mode != 0 && RoleClass.SeerFriends.mode != 2) return;
                            break;
                        case RoleId.JackalSeer:
                        case RoleId.SidekickSeer:
                            DeadBodyPositions = RoleClass.JackalSeer.deadBodyPositions;
                            RoleClass.JackalSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.JackalSeer.limitSoulDuration;
                            soulDuration = RoleClass.JackalSeer.soulDuration;
                            if (RoleClass.JackalSeer.mode != 0 && RoleClass.JackalSeer.mode != 2) return;
                            break;
                    }
                    foreach (Vector3 pos in DeadBodyPositions)
                    {
                        GameObject soul = new GameObject();
                        soul.transform.position = pos;
                        soul.layer = 5;
                        var rend = soul.AddComponent<SpriteRenderer>();
                        rend.sprite = getSoulSprite();

                        if (limitSoulDuration)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(soulDuration, new Action<float>((p) =>
                            {
                                if (rend != null)
                                {
                                    var tmp = rend.color;
                                    tmp.a = Mathf.Clamp01(1 - p);
                                    rend.color = tmp;
                                }
                                if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                            })));
                        }
                    }
                }
            }

            public static class MurderPlayerPatch
            {
                public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
                {
                    var role = PlayerControl.LocalPlayer.getRole();
                    if (role == RoleId.Seer || role == RoleId.MadSeer || role == RoleId.EvilSeer || role == RoleId.SeerFriends || role == RoleId.JackalSeer || role == RoleId.SidekickSeer)
                    {
                        bool ModeFlag = false;
                        switch (role)
                        {
                            case RoleId.Seer:
                                if (RoleClass.Seer.deadBodyPositions != null) RoleClass.Seer.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.Seer.mode <= 1;
                                break;
                            case RoleId.MadSeer:
                                if (RoleClass.MadSeer.deadBodyPositions != null) RoleClass.MadSeer.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.MadSeer.mode <= 1;
                                break;
                            case RoleId.EvilSeer:
                                if (RoleClass.EvilSeer.deadBodyPositions != null) RoleClass.EvilSeer.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.MadSeer.mode <= 1;
                                break;
                            case RoleId.SeerFriends:
                                if (RoleClass.SeerFriends.deadBodyPositions != null) RoleClass.SeerFriends.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.SeerFriends.mode <= 1;
                                break;
                            case RoleId.JackalSeer:
                            case RoleId.SidekickSeer:
                                if (RoleClass.JackalSeer.deadBodyPositions != null) RoleClass.JackalSeer.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.JackalSeer.mode <= 1;
                                break;
                        }
                        if (PlayerControl.LocalPlayer.isAlive() && CachedPlayer.LocalPlayer.PlayerId != target.PlayerId && ModeFlag)
                        {
                            RoleHelpers.ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
                        }
                    }
                }
            }

        }

    }


}


