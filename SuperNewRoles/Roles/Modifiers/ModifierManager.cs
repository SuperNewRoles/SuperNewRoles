using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace SuperNewRoles.Roles
{
    public enum ModifierType
    {
        Lovers = 0,

        // don't put anything below this
        NoModifier = int.MaxValue
    }

    [HarmonyPatch]
    public static class ModifierData
    {
        public static Dictionary<ModifierType, Type> allModTypes = new()
        {
            { ModifierType.Lovers, typeof(ModifierBase<Lovers>) },
        };
    }

    public abstract class Modifier
    {
        public static List<Modifier> allModifiers = new();
        public PlayerControl player;
        public ModifierType modId;

        /// <summary>
        /// RoleClassのクリアアンドリロード時に実行されます
        /// </summary>
        public abstract void ClearAndReload();

        /// <summary>
        /// HudManager.OpenMeetingRoomのPrefixで実行されます
        /// </summary>
        public abstract void OnMeetingStart();

        /// <summary>
        /// ExileController.ReEnableGameplayのPostfixで実行されます
        /// </summary>
        public abstract void OnMeetingEnd();

        /// <summary>
        /// PlayerControl.FixedUpdateのPostfixで実行されます
        /// </summary>
        public abstract void FixedUpdate();

        /// <summary>
        /// PlayerControl.MurderPlayerのPostfixで実行されます
        /// </summary>
        /// <param name="target"></param>
        public abstract void OnKill(PlayerControl target);

        /// <summary>
        /// PlayerControl.MurderPlayerのPostfixで実行されます
        /// PlayerControl.Exiledでkillerがnullで実行されます
        /// </summary>
        /// <param name="killer"></param>
        public abstract void OnDeath(PlayerControl killer = null);

        /// <summary>
        /// GameData.HandleDisconnectのPostfixで実行されます
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reason"></param>
        public abstract void HandleDisconnect(PlayerControl player, DisconnectReasons reason);

        public virtual void ResetModifier() { }
        public virtual string ModifyNameText(string nameText) { return nameText; }

        public static void ClearAll()
        {
            allModifiers = new List<Modifier>();
        }
    }

    [HarmonyPatch]
    public abstract class ModifierBase<T> : Modifier where T : ModifierBase<T>, new()
    {
        public static List<T> players = new();
        public static ModifierType ModType;
        public static List<RoleId> persistRoleChange = new();

        public void Init(PlayerControl player)
        {
            this.player = player;
            players.Add((T)this);
            allModifiers.Add(this);
        }

        public static T Local { get { return players.FirstOrDefault(x => x.player == CachedPlayer.LocalPlayer.PlayerControl); } }
        public static List<PlayerControl> AllPlayers { get { return players.Select(x => x.player).ToList(); } }
        public static List<PlayerControl> LivingPlayers { get { return players.Select(x => x.player).Where(x => x.IsAlive()).ToList(); } }
        public static List<PlayerControl> DeadPlayers { get { return players.Select(x => x.player).Where(x => !x.IsAlive()).ToList(); } }
        public static bool Exists { get { return players.Count > 0; } }

        public static T GetModifier(PlayerControl player = null)
        {
            player ??= CachedPlayer.LocalPlayer.PlayerControl;
            return players.FirstOrDefault(x => x.player == player);
        }

        public static bool HasModifier(PlayerControl player) { return players.Any(x => x.player == player); }

        public static T AddModifier(PlayerControl player)
        {
            T mod = new();
            mod.Init(player);
            return mod;
        }

        public static void EraseModifier(PlayerControl player, RoleId newRole = RoleId.DefaultRole)
        {
            List<T> toRemove = new();

            foreach (var p in players)
            {
                if (p.player == player && p.modId == ModType && !persistRoleChange.Contains(newRole))
                    toRemove.Add(p);
            }
            players.RemoveAll(x => toRemove.Contains(x));
            allModifiers.RemoveAll(x => toRemove.Contains(x));
        }

        public static void SwapModifier(PlayerControl p1, PlayerControl p2)
        {
            var index = players.FindIndex(x => x.player == p1);
            if (index >= 0)
            {
                players[index].player = p2;
            }
        }
    }


    public static class ModifierHelpers
    {
        public static bool HasModifier(this PlayerControl player, ModifierType mod)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (mod == t.Key)
                {
                    return (bool)t.Value.GetMethod("HasModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                }
            }
            return false;
        }

        public static void AddModifier(this PlayerControl player, ModifierType mod)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (mod == t.Key)
                {
                    t.Value.GetMethod("AddModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                    return;
                }
            }
        }

        public static void EraseModifier(this PlayerControl player, ModifierType mod)
        {
            if (HasModifier(player, mod))
            {
                foreach (var t in ModifierData.allModTypes)
                {
                    if (mod == t.Key)
                    {
                        t.Value.GetMethod("EraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player });
                        return;
                    }
                }
                Logger.Info($"eraseRole: no method found for role type {mod}");
            }
        }

        public static void EraseAllModifiers(this PlayerControl player, RoleId newRole = RoleId.DefaultRole)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                t.Value.GetMethod("EraseModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, newRole });
            }
        }

        public static void SwapModifiers(this PlayerControl player, PlayerControl target)
        {
            foreach (var t in ModifierData.allModTypes)
            {
                if (player.HasModifier(t.Key))
                {
                    t.Value.GetMethod("SwapModifier", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { player, target });
                }
            }
        }
    }
}
