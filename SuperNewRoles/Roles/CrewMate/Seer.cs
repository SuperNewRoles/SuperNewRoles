using System;
using System.Collections.Generic;
using HarmonyLib;

using UnityEngine;

namespace SuperNewRoles.Roles
{
    class Seer
    //マッド・イビル・フレンズ・ジャッカル・サイドキック　シーア
    {
        public static List<List<PlayerControl>> Seers = new() {
            RoleClass.Seer.SeerPlayer,
            RoleClass.EvilSeer.EvilSeerPlayer,
            RoleClass.MadSeer.MadSeerPlayer,
            RoleClass.JackalSeer.JackalSeerPlayer,
            RoleClass.SeerFriends.SeerFriendsPlayer
        };

        public static void ShowFlash(Color color, float duration = 1f)
        {//画面を光らせる
            if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;
                if (p < 0.5)
                {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                }
                else
                {
                    if (renderer != null)
                        renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                }
                if (p == 1f && renderer != null) renderer.enabled = false;
            })));
        }
        private static Sprite SoulSprite;
        public static Sprite GetSoulSprite()
        {
            if (SoulSprite) return SoulSprite;
            SoulSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Soul.png", 500f);
            return SoulSprite;
        }

        public static class ExileControllerWrapUpPatch
        {
            public static void WrapUpPostfix()
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                if (role is RoleId.Seer or RoleId.MadSeer or RoleId.EvilSeer or RoleId.SeerFriends or RoleId.JackalSeer or RoleId.SidekickSeer)
                {
                    List<Vector3> DeadBodyPositions = new();
                    bool limitSoulDuration = false;
                    float soulDuration = 0f;
                    switch (role)
                    {
                        case RoleId.Seer:
                            DeadBodyPositions = RoleClass.Seer.deadBodyPositions;
                            RoleClass.Seer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.Seer.limitSoulDuration;
                            soulDuration = RoleClass.Seer.soulDuration;
                            if (RoleClass.Seer.mode is not 0 and not 2) return;
                            break;
                        case RoleId.MadSeer:
                            DeadBodyPositions = RoleClass.MadSeer.deadBodyPositions;
                            RoleClass.MadSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.MadSeer.limitSoulDuration;
                            soulDuration = RoleClass.MadSeer.soulDuration;
                            if (RoleClass.MadSeer.mode is not 0 and not 2) return;
                            break;
                        case RoleId.EvilSeer:
                            DeadBodyPositions = RoleClass.EvilSeer.deadBodyPositions;
                            RoleClass.EvilSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.EvilSeer.limitSoulDuration;
                            soulDuration = RoleClass.EvilSeer.soulDuration;
                            if (RoleClass.EvilSeer.mode is not 0 and not 2) return;
                            break;
                        case RoleId.SeerFriends:
                            DeadBodyPositions = RoleClass.SeerFriends.deadBodyPositions;
                            RoleClass.SeerFriends.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.SeerFriends.limitSoulDuration;
                            soulDuration = RoleClass.SeerFriends.soulDuration;
                            if (RoleClass.SeerFriends.mode is not 0 and not 2) return;
                            break;
                        case RoleId.JackalSeer:
                        case RoleId.SidekickSeer:
                            DeadBodyPositions = RoleClass.JackalSeer.deadBodyPositions;
                            RoleClass.JackalSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.JackalSeer.limitSoulDuration;
                            soulDuration = RoleClass.JackalSeer.soulDuration;
                            if (RoleClass.JackalSeer.mode is not 0 and not 2) return;
                            break;
                    }
                    foreach (Vector3 pos in DeadBodyPositions)
                    {
                        GameObject soul = new();
                        soul.transform.position = pos;
                        soul.layer = 5;
                        var rend = soul.AddComponent<SpriteRenderer>();
                        rend.sprite = GetSoulSprite();

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
                public static void Postfix([HarmonyArgument(0)] PlayerControl target)
                {
                    var role = PlayerControl.LocalPlayer.GetRole();
                    if (role is RoleId.Seer or RoleId.MadSeer or RoleId.EvilSeer or RoleId.SeerFriends or RoleId.JackalSeer or RoleId.SidekickSeer)
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
                        if (PlayerControl.LocalPlayer.IsAlive() && CachedPlayer.LocalPlayer.PlayerId != target.PlayerId && ModeFlag)
                        {
                            ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));
                        }
                    }
                }
            }
        }
    }
}