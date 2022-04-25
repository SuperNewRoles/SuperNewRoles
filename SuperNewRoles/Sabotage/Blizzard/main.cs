﻿using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Button;

namespace SuperNewRoles.Sabotage.Blizzard
{
    public static class main
    {
        public static void StartSabotage()
        {
            IsLocalEnd = false;
            SuperNewRolesPlugin.Logger.LogInfo("スタートサボ！");
            SabotageManager.thisSabotage = SabotageManager.CustomSabotage.Blizzard;
            foreach (Arrow aw in ArrowDatas)
            {
                GameObject.Destroy(aw.arrow);
            }
            ArrowDatas = new List<Arrow>();
            IsYellow = true;
            foreach (Vector2 data in Datas)
            {
                Arrow arrow = new Arrow(Color.yellow);
                arrow.arrow.SetActive(true);
                ArrowDatas.Add(arrow);
            }
            UpdateTime = 0;// DefaultUpdateTime;
            DistanceTime = DefaultDistanceTime;
            ArrowUpdateColor = 0.25f;
            OKPlayers = new List<PlayerControl>();
        }
        public static float BlizzardSlowSpeedmagnification;
        private static float ArrowUpdateColor = 1;
        public static float UpdateTime;
        private static float DistanceTime;
        public static float DefaultDistanceTime = 5;
        private static bool IsYellow;
        private static List<Arrow> ArrowDatas = new List<Arrow>();
        private static Vector2[] Datas = new Vector2[] { new Vector2(-13.9f, -15.5f), new Vector2(-24.7f, -1f), new Vector2(10.6f, -15.5f) };
        public static List<PlayerControl> OKPlayers;
        public static bool IsLocalEnd;
        public static bool IsAllEndSabotage;
        public static float Timer;
        public static void Create(InfectedOverlay __instance)
        {
            if (SabotageManager.IsOK(SabotageManager.CustomSabotage.Blizzard))
            {
                ButtonBehavior button = InfectedOverlay.Instantiate(__instance.allButtons[0], __instance.allButtons[0].transform.parent);
                if (PlayerControl.GameOptions.MapId == 1)
                {
                    button.transform.localPosition += new Vector3(-3.48f, - 0.2624f, 0);
                }
                button.transform.localPosition += new Vector3(1.36f, 0, 0);
                button.spriteRenderer.sprite = IconManager.BlizzardgetButtonSprite();
                button.OnClick = new ButtonClickedEvent();

                button.OnClick.AddListener((Action)(() =>
                {
                    if (SabotageManager.InfectedOverlayInstance.CanUseSpecial)
                    {
                        SabotageManager.CustomSabotageRPC(PlayerControl.LocalPlayer, SabotageManager.CustomSabotage.Blizzard, true);
                    }
                }));
                __instance.allButtons.AddItem(button);
                SabotageManager.CustomButtons.Add(button);
            }
        }
        public static void Update()
        {
            if (SabotageManager.InfectedOverlayInstance != null)
            {
                if (IsAllEndSabotage)
                {
                    SabotageManager.InfectedOverlayInstance.SabSystem.Timer = SabotageManager.SabotageMaxTime;
                } else if (!IsLocalEnd)
                {
                    SabotageManager.InfectedOverlayInstance.SabSystem.Timer = SabotageManager.SabotageMaxTime;
                }
            }
            bool IsOK = true;
            /*foreach (PlayerControl p3 in PlayerControl.AllPlayerControls)
            {
                if (p3.isAlive() && !OKPlayers.IsCheckListPlayerControl(p3)) {
                    IsOK = false;
                    if (PlayerControl.LocalPlayer.isImpostor())
                    {
                        if (!(p3.isImpostor() || p3.isRole(CustomRPC.RoleId.MadKiller)))
                        {
                            SetNamesClass.SetPlayerNameColor(p3,new Color32(18, 112, 214,byte.MaxValue));
                        }
                    }
                }
            }*/
            if (IsOK)
            {
                SabotageManager.thisSabotage = SabotageManager.CustomSabotage.None;
                return;
            }
            if (!IsLocalEnd)
            {
                int arrowindex = 0;
                ArrowUpdateColor -= Time.fixedDeltaTime;
                Color? SetColor = null;
                if (ArrowUpdateColor <= 0)
                {
                    if (IsYellow)
                    {
                        SetColor = Color.red;
                        IsYellow = false;
                    }
                    else
                    {
                        SetColor = Color.yellow;
                        IsYellow = true;
                    }
                    ArrowUpdateColor = 0.25f;
                }
                foreach (Arrow arrow in ArrowDatas)
                {
                    arrow.Update(Datas[arrowindex], SetColor);
                    arrowindex++;
                }
                bool IsOK2 = false;
                foreach (Vector2 data in Datas)
                {
                    if (!IsOK2)
                    {
                        if (Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), data) <= 1)
                        {
                            IsOK2 = true;
                        }
                    }
                }
                if (IsOK2)
                {
                    DistanceTime -= Time.fixedDeltaTime;
                    if (DistanceTime <= 0)
                    {
                        SabotageManager.CustomSabotageRPC(PlayerControl.LocalPlayer, SabotageManager.CustomSabotage.Blizzard, false);
                    }
                }
                else
                {
                    DistanceTime = DefaultDistanceTime;
                }
                UpdateTime -= Time.fixedDeltaTime;
                /*if (UpdateTime <= 0)
                {
                    List<PlayerControl> target = new List<PlayerControl>();
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (!p.Data.Disconnected && p.isAlive())
                        {
                            target.Add(p);
                        }
                    }
                    /*foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (target.Count > 0)
                        {
                            var index = ModHelpers.GetRandomIndex(target);
                            OutfitManager.resetChange(p);
                            OutfitManager.changeToPlayer(p, target[index]);
                            target.RemoveAt(index);
                        }
                    }
                   // UpdateTime = DefaultUpdateTime;
                }*/
            }
        }
        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        public static void EndSabotage(PlayerControl p)
        {
            OKPlayers.Add(p);
            if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                IsLocalEnd = true;
                /*if (PlayerControl.GameOptions.TaskBarMode != TaskBarMode.Invisible)
                {
                    SlowSpeed.Instance.gameObject.SetActive(IsLocalEnd);
                }*/
                foreach (Arrow aw in ArrowDatas)
                {
                    GameObject.Destroy(aw.arrow);
                }
                ArrowDatas = new List<Arrow>();
                foreach (PlayerControl p2 in PlayerControl.AllPlayerControls)
                {
                    p2.resetChange();
                }
                static void Postfix(PlayerPhysics __instance)
                {
                    __instance.body.velocity /= main.BlizzardSlowSpeedmagnification;
                }
            }
        }
    }
}
