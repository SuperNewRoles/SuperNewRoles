using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;

namespace SuperNewRoles.Roles.RoleBases
{
    public abstract class Role
    {
        public static List<Role> allRoles = new List<Role>();
        public PlayerControl player;
        public RoleId roleId;

        public abstract void OnMeetingStart();
        public abstract void OnMeetingEnd();
        public abstract void FixedUpdate();
        public abstract void OnKill(PlayerControl target);
        public abstract void OnDeath(PlayerControl killer = null);
        public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
        public virtual void ResetRole() { }
        public virtual void PostInit() { }
        public virtual string modifyNameText(string nameText) { return nameText; }
        public virtual string meetingInfoText() { return ""; }

        public static void ClearAll()
        {
            allRoles = new List<Role>();
        }
    }
    public abstract class RoleOptionBase<T> {
        public static List<CustomOption> CustomOptions = new();

    }

    public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
    {
        public static List<T> players = new();
        public static RoleId RoleId;

        public void Init(PlayerControl player)
        {
            this.player = player;
            players.Add((T)this);
            allRoles.Add(this);
            PostInit();
        }

        public static T local
        {
            get
            {
                return players.FirstOrDefault(x => x.player == PlayerControl.LocalPlayer);
            }
        }

        public static List<PlayerControl> allPlayers
        {
            get
            {
                return players.Select(x => x.player).ToList();
            }
        }

        public static List<PlayerControl> livingPlayers
        {
            get
            {
                return players.Select(x => x.player).Where(x => x.IsAlive()).ToList();
            }
        }

        public static List<PlayerControl> deadPlayers
        {
            get
            {
                return players.Select(x => x.player).Where(x => x.IsDead()).ToList();
            }
        }

        public static bool exists
        {
            get { return players.Count > 0; }
        }

        public static T GetRole(PlayerControl player = null)
        {
            player = player ?? PlayerControl.LocalPlayer;
            return players.FirstOrDefault(x => x.player == player);
        }

        public static bool IsRole(PlayerControl player)
        {
            return players.Any(x => x.player == player);
        }

        public static T SetRole(PlayerControl player)
        {
            if (!IsRole(player))
            {
                T role = new T();
                role.Init(player);
                return role;
            }
            return null;
        }

        public static void EraseRole(PlayerControl player)
        {
            players.DoIf(x => x.player == player, x => x.ResetRole());
            players.RemoveAll(x => x.player == player && x.roleId == RoleId);
            allRoles.RemoveAll(x => x.player == player && x.roleId == RoleId);
        }

        public static void SwapRole(PlayerControl p1, PlayerControl p2)
        {
            var index = players.FindIndex(x => x.player == p1);
            if (index >= 0)
            {
                players[index].player = p2;
            }
        }
    }
}
