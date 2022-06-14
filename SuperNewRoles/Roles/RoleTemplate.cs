using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
namespace SuperNewRoles.Roles
{
    class RoleTemplate
    {
        public abstract class Role
        {
            public static List<Role> allRoles = new List<Role>();
            public PlayerControl player;
            public CustomRPC.RoleId roleId;

            public abstract void OnMeetingStart();
            public abstract void OnMeetingEnd();
            public abstract void FixedUpdate();
            public abstract void OnKill(PlayerControl target);
            public abstract void OnDeath(PlayerControl killer = null);
            public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
            public virtual void ResetRole() { }

            public static void ClearAll()
            {
                allRoles = new List<Role>();
            }
        }
        [HarmonyPatch]
        public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
        {
            public static List<T> players = new List<T>();
            public static CustomRPC.RoleId RoleType;

            public void Init(PlayerControl player)
            {
                this.player = player;
                players.Add((T)this);
                allRoles.Add(this);
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
                    return players.Select(x => x.player).Where(x => x.isAlive()).ToList();
                }
            }

            public static List<PlayerControl> deadPlayers
            {
                get
                {
                    return players.Select(x => x.player).Where(x => !x.isAlive()).ToList();
                }
            }

            public static bool exists
            {
                get { return players.Count > 0; }
            }

            public static T getRole(PlayerControl player = null)
            {
                player = player ?? PlayerControl.LocalPlayer;
                return players.FirstOrDefault(x => x.player == player);
            }

            public static bool isRole(PlayerControl player)
            {
                return players.Any(x => x.player == player);
            }

            public static void setRole(PlayerControl player)
            {
                if (!isRole(player))
                {
                    T role = new T();
                    role.Init(player);
                }
            }

            public static void eraseRole(PlayerControl player)
            {
                players.DoIf(x => x.player == player, x => x.ResetRole());
                players.RemoveAll(x => x.player == player && x.roleId == RoleType);
                allRoles.RemoveAll(x => x.player == player && x.roleId == RoleType);
            }

            public static void swapRole(PlayerControl p1, PlayerControl p2)
            {
                var index = players.FindIndex(x => x.player == p1);
                if (index >= 0)
                {
                    players[index].player = p2;
                }
            }
        }
    }
}