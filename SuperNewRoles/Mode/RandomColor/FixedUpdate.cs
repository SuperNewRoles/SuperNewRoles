using HarmonyLib;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patch;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.RandomColor
{
    public static class FixedUpdate
    {
        private static int MaxColorCount = 15;
        public static float UpdateTime;
        public static bool IsRandomNameColor;
        public static bool IsHideName;
        public static bool IsHideNameSet;
        public static bool IsRandomColorMeeting;
        public static bool IsMeetingIn = false;
        public static void Update()
        {
            if (!IsHideNameSet)
            {
                IsHideNameSet = true;
                if (IsHideName)
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (!p.Data.Disconnected)
                        {
                            p.RpcSetName("　");
                        }
                    }
                }
            }
            UpdateTime -= Time.fixedDeltaTime;
            if (UpdateTime <= 0)
            {
                UpdateTime = 0.1f;
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.Data.Disconnected)
                    {
                        byte SetColor = (byte)(p.Data.DefaultOutfit.ColorId + 1);
                        if (p.Data.DefaultOutfit.ColorId >= MaxColorCount - 1)
                        {
                            SetColor = 0;
                        }
                        SuperNewRolesPlugin.Logger.LogInfo("UPDATED!");
                        if (!RoleClass.IsMeeting || (RoleClass.IsMeeting && IsRandomColorMeeting))
                        {
                            p.RpcSetColor(SetColor);
                        }
                        if (!IsHideName && IsRandomNameColor && (!RoleClass.IsMeeting || (RoleClass.IsMeeting && IsRandomColorMeeting)))
                        {
                            p.RpcSetName(ModHelpers.cs(Palette.PlayerColors[SetColor], p.getDefaultName()));
                        }
                        if (RoleClass.IsMeeting && IsHideName)
                        {
                            p.RpcSetName(ModHelpers.cs(Color.yellow, "RandomColorMode!"));
                            IsMeetingIn = true;
                        }
                        if (IsMeetingIn && !RoleClass.IsMeeting && IsHideName)
                        {
                            IsHideNameSet = false;
                        }
                    }
                }
            }
        }
    }
}
