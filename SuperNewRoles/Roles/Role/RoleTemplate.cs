using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.CustomRPC;

//TODO:さつまいも、リファクタ
namespace SuperNewRoles.Roles
{
    class RoleTemplate
    {
        public abstract class Role
        {
            public static List<Role> allRoles = new();
            public PlayerControl player;
            public RoleId? roleId = null;

            public abstract void OnMeetingStart();
            public abstract void OnMeetingEnd();
            public abstract void FixedUpdate();
            public abstract void OnKill(PlayerControl target);
            public abstract void OnDeath(PlayerControl killer = null);
            public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);
            public virtual void ReSetRole() { }

            public static void ClearAll()
            {
                allRoles = new List<Role>();
            }
        }
        [HarmonyPatch]
        public abstract class RoleBase<T> : Role where T : RoleBase<T>, new()
        {
            public static List<T> players = new();
            public static RoleId? RoleType = null;

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
                    return players.Select(x => x.player).Where(x => x.IsAlive()).ToList();
                }
            }

            public static List<PlayerControl> deadPlayers
            {
                get
                {
                    return players.Select(x => x.player).Where(x => !x.IsAlive()).ToList();
                }
            }

            public static bool exists
            {
                get { return players.Count > 0; }
            }

            public static T GetRole(PlayerControl player = null)
            {
                player ??= PlayerControl.LocalPlayer;
                return players.FirstOrDefault(x => x.player == player);
            }

            public static bool IsRole(PlayerControl player)
            {
                return players.Any(x => x.player == player);
            }

            public static void SetRole(PlayerControl player)
            {
                if (!IsRole(player))
                {
                    T role = new();
                    role.Init(player);
                }
            }

            public static void eraseRole(PlayerControl player)
            {
                players.DoIf(x => x.player == player, x => x.ReSetRole());
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