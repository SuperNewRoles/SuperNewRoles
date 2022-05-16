using SuperNewRoles.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.CopsRobbers
{
    public static class main
    {
        public static void ClearAndReloads()
        {
            ArrestPositions = new Dictionary<int, SystemTypes?>();
            Arrest = new List<int>();
            TeleportIDs = new List<byte>();
        }
        public static PlayerControl GetBot()
        {
            if (BotManager.AllBots.Count != 0)
            {
                if (BotManager.AllBots[0] != null)
                {
                    return BotManager.AllBots[0];
                }
            }
            return null;
        }
        public static List<int> Arrest;
        public static Dictionary<int,SystemTypes?> ArrestPositions;
        public static bool IsArrest(this PlayerControl player)
        {
            if (Arrest.Contains(player.PlayerId)) return true;
            return false;
        }
        public static Dictionary<MapNames, List<SystemTypes>> Rooms = new Dictionary<MapNames, List<SystemTypes>>()
        {
            { MapNames.Skeld, new List<SystemTypes>(){ SystemTypes.Comms, SystemTypes.Nav, SystemTypes.Security } },
            { MapNames.Mira, new List<SystemTypes>(){ SystemTypes.Comms, SystemTypes.Greenhouse, SystemTypes.Launchpad, SystemTypes.Storage} },
            { MapNames.Polus, new List<SystemTypes>(){ SystemTypes.Electrical, SystemTypes.Specimens, SystemTypes.Weapons} },
            { MapNames.Airship, new List<SystemTypes>(){ SystemTypes.Records, SystemTypes.Security, SystemTypes.VaultRoom} }
        };
        public static MapNames GetMap()
        {
            int mapid = PlayerControl.GameOptions.MapId;
            return (MapNames)mapid;
        }
        public static SystemTypes SetRandomArrestPosition(PlayerControl player)
        {
            var type = ModHelpers.GetRandom(Rooms[GetMap()]);
            ArrestPositions[player.PlayerId] = type;
            Arrest.Add(player.PlayerId);
            return type;
        }
        public static Vector2 getPosition(SystemTypes type)
        {
            return new Vector2(-13.4818f, -5.3336f);
            var MAP = GetMap();
            switch (type)
            {
                case SystemTypes.Comms:
                    switch (MAP)
                    {
                        case MapNames.Skeld:
                            return new Vector2();
                        case MapNames.Mira:
                            return new Vector2();
                    }
                    break;
                case SystemTypes.Nav:
                    return new Vector2();
                case SystemTypes.Security:
                    switch (MAP)
                    {
                        case MapNames.Skeld:
                            return new Vector2(-13.4818f, -5.3336f);
                        case MapNames.Airship:
                            return new Vector2();
                    }
                    break;
                case SystemTypes.Greenhouse:
                    return new Vector2();
                case SystemTypes.Launchpad:
                    return new Vector2();
                case SystemTypes.Storage:
                    return new Vector2();
                case SystemTypes.Electrical:
                    return new Vector2();
                case SystemTypes.Specimens:
                    return new Vector2();
                case SystemTypes.Weapons:
                    return new Vector2();
                case SystemTypes.Records:
                    return new Vector2(19.6306f, 8.6023f);
                case SystemTypes.VaultRoom:
                    return new Vector2();
            }
            return new Vector2(0, 0);
        }
        static bool IsTeleport = false;
        public static List<byte> TeleportIDs = new List<byte>();
        public static void Teleport(PlayerControl player,Vector2 position)
        {
            PlayerControl bot = GetBot();
            if (bot != null && !TeleportIDs.Contains(player.PlayerId))
            {
                IsTeleport = true;
                TeleportIDs.Add(player.PlayerId);
                bot.NetTransform.RpcSnapTo(position);
                IsTeleport = false;
                new LateTask(() =>
                {
                    SuperNewRolesPlugin.Logger.LogInfo("BOTX:"+ bot.transform.position.x);
                    if (bot.transform.position.x != 99999) {
                        player.RPCMurderPlayerPrivate(bot);
                    }
                    if (!IsTeleport)
                    {
                        bot.NetTransform.RpcSnapTo(new Vector2(99999, 99999));
                    }
                }, 0.5f);
                new LateTask(() => {
                    TeleportIDs.Remove(player.PlayerId);
                }, 0.6f);
            }
        }
        public static void FixedUpdate()
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.isImpostor() && player.IsPlayer())
                {
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (!p.isImpostor() && p.IsPlayer())
                        {
                            if (p != null && !p.Data.Disconnected)
                            {
                                if (!p.IsArrest())
                                {
                                    var DistanceData = Vector2.Distance(player.transform.position, p.transform.position);
                                    SuperNewRolesPlugin.Logger.LogInfo("DISTANCE:"+DistanceData);
                                    if (DistanceData <= 0.5f)
                                    {
                                        Teleport(p, getPosition(SetRandomArrestPosition(p)));
                                    }
                                } else
                                {
                                    Vector2 getpos = getPosition((SystemTypes)ArrestPositions[p.PlayerId]);
                                    var DistanceData = Vector2.Distance(p.transform.position, getpos);
                                    if (DistanceData >= 1.5f)
                                    {
                                        Teleport(p, getpos);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
