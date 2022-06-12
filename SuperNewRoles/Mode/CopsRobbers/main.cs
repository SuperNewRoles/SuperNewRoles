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
            IsStart = false;
            ArrestPositions = new Dictionary<int, SystemTypes?>();
            Arrest = new List<int>();
            IsMove = false;
            SpawnPosition = new Dictionary<int, SystemTypes?>();
            LastCount = 0;
        }
        public static bool IsStart;
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
        public static Dictionary<int, SystemTypes?> ArrestPositions;
        public static bool IsMove;
        public static Dictionary<int, SystemTypes?> SpawnPosition;
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
        public static bool EndGameCheck(ShipStatus __instance)
        {
            bool impostorwin = true;
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (!p.Data.Disconnected)
                {
                    if (!p.isImpostor() && !p.IsArrest())
                    {
                        impostorwin = false;
                    }
                }
            }
            if (impostorwin)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.ImpostorByKill, false);
                return true;
            }
            else if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
            {
                __instance.enabled = false;
                ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void ChangeCosmetics()
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                p.RpcSetPet("");
                p.RpcSetVisor("");
                if (p.isImpostor())
                {
                    p.RpcSetColor(1);
                    p.RpcSetHat("");
                    p.RpcSetSkin("skin_Police");
                }
                else
                {
                    p.RpcSetColor(6);
                    p.RpcSetHat("hat_pk04_Vagabond");
                    p.RpcSetSkin("");
                }
            }
        }
        public static SystemTypes GetRandomSpawnPosition(PlayerControl player)
        {
            if (!SpawnPosition.ContainsKey(player.PlayerId))
            {
                var type = ModHelpers.GetRandom(Rooms[GetMap()]);
                SpawnPosition[player.PlayerId] = type;
            }
            return (SystemTypes)SpawnPosition[player.PlayerId];
        }
        public static SystemTypes SetRandomArrestPosition(PlayerControl player)
        {
            var type = ModHelpers.GetRandom(Rooms[GetMap()]);
            ArrestPositions[player.PlayerId] = type;
            Arrest.Add(player.PlayerId);
            //player.MyPhysics.RpcClimbLadder(null);

            player.RpcSetColor(5);
            player.RpcSetHat("");
            player.RpcSetSkin("skin_Hazmat");
            player.RpcSetVisor("visor_pk01_DumStickerVisor");

            return type;
        }
        public static void RemoveArrest(PlayerControl player)
        {
            Arrest.Remove(player.PlayerId);

            player.RpcSetColor(6);
            player.RpcSetHat("hat_pk04_Vagabond");
            player.RpcSetSkin("");
            player.RpcSetVisor("");

        }
        public static Vector2 getPosition(SystemTypes type)
        {
            //return new Vector2(-13.4818f, -5.3336f);
            var MAP = GetMap();
            switch (type)
            {
                case SystemTypes.Comms:
                    switch (MAP)
                    {
                        case MapNames.Skeld:
                            return new Vector2(4.5f, -15.5f);
                        case MapNames.Mira:
                            return new Vector2();
                    }
                    break;
                case SystemTypes.Nav:
                    return new Vector2(16.6f, -4.6f);
                case SystemTypes.Security:
                    switch (MAP)
                    {
                        case MapNames.Skeld:
                            return new Vector2(-13.4818f, -5.3336f);
                        case MapNames.Airship:
                            return new Vector2(7.0886f, -12.501f);
                    }
                    break;
                case SystemTypes.Greenhouse:
                    return new Vector2(17.84f, 23.68f);
                case SystemTypes.Launchpad:
                    return new Vector2(-4.25f, 2.45f);
                case SystemTypes.Storage:
                    return new Vector2(19.53f, 2.48f);
                case SystemTypes.Electrical:
                    return new Vector2(8.79f, -12.1f);
                case SystemTypes.Specimens:
                    return new Vector2(36.70f, -20.84f);
                case SystemTypes.Weapons:
                    return new Vector2(12.05f, -23.33f);
                case SystemTypes.Records:
                    return new Vector2(20f, 10.5f);
                case SystemTypes.VaultRoom:
                    return new Vector2(-8.7701f, 12.4399f);
            }
            return new Vector2(0, 0);
        }
        static bool IsTeleport = false;
        static float ImpostorMoveTime;
        static int LastCount;
        static float LastUpdate;
        public static List<byte> TeleportIDs = new List<byte>();
        public static void Teleport(PlayerControl player,Vector2 position)
        {
            player.NetTransform.RpcSnapTo(position);
            return;
            /*
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
            */
        }
        public static void HudUpdate()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (!IsStart) return;
            if (!IsMove)
            {
                bool IsMoveOK = true;
                List<PlayerControl> players = new List<PlayerControl>();
                int NotLoadedCount = 0;
                if (PlayerControl.GameOptions.MapId == 4)
                {
                    /*
                    if (PlayerControl.LocalPlayer.name != "　" && PlayerControl.LocalPlayer.name != "<color=black><size=7500%>■</size></color>")
                    {
                        PlayerControl.LocalPlayer.RpcSetName("<color=black><size=7500%>■</size></color>");
                    }*/

                    foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                    {
                        if (ModHelpers.IsPositionDistance(p.transform.position, new Vector2(3, 6),0.5f) || 
                            ModHelpers.IsPositionDistance(p.transform.position, new Vector2(-25, 40), 0.5f) || 
                            ModHelpers.IsPositionDistance(p.transform.position, new Vector2(-1.4f, 2.3f), 0.5f)
                            )
                        {
                            IsMoveOK = false;
                            NotLoadedCount++;
                        }
                        else
                        {
                            players.Add(p.PlayerControl);
                        }
                    }
                }
                if (LastCount != players.Count)
                {
                    LastCount = players.Count;
                    string name = "\n\n\n\n\n\n\n\n<size=300%><color=white>" + ModeHandler.PlayingOnSuperNewRoles + "</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%><color=white>全プレイヤーのスポーンを待っています...\nロドー中:残り"+NotLoadedCount+"人</color></size>";
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        p.RpcSetNamePrivate(name);
                    }
                }
                /*
                if (!ModHelpers.IsPositionDistance(CachedPlayer.LocalPlayer.transform.position, new Vector2(9990, 8551f), 0.5f) &&
                    !ModHelpers.IsPositionDistance(CachedPlayer.LocalPlayer.transform.position, new Vector2(9990, 8550f), 0.5f))
                {
                    if (players.IsCheckListPlayerControl(PlayerControl.LocalPlayer))
                    {
                        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(9991, 8551f));
                        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(9990, 8550f));
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(9991, 8552f));
                        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(new Vector2(9990, 8551f));
                    }
                }
                */
                int i = 0;
                foreach (PlayerControl p in players)
                {
                    p.NetTransform.RpcSnapTo(new Vector2(-30, 30));
                    i++;
                }
                if (IsMoveOK)
                {
                    IsMove = true;
                    ImpostorMoveTime = 5;
                    LastUpdate = 6;
                    foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                    {
                        if (!p.PlayerControl.isImpostor())
                        {
                            p.NetTransform.RpcSnapTo(getPosition(GetRandomSpawnPosition(p)));
                        }
                    }
                }
                return;
            }

            if (ImpostorMoveTime >= 0)
            {
                ImpostorMoveTime -= Time.deltaTime;
                SuperNewRolesPlugin.Logger.LogInfo(ImpostorMoveTime - LastUpdate);
                if (LastUpdate - ImpostorMoveTime >= 1)
                {
                    //string name = "\n\n\n\n\n<size=300%><color=white>" + SuperNewRoles.Mode.ModeHandler.PlayingOnSuperNewRoles + "</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%>インポスターが来るまで残り5秒</size>";
                    //PlayerControl.LocalPlayer.RpcSetName(name);
                    LastUpdate = ImpostorMoveTime;
                    string name = "\n\n\n\n\n<size=300%><color=white>" + ModeHandler.PlayingOnSuperNewRoles + "</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%>インポスターが来るまで残り" + ((int)(LastUpdate + 1)).ToString() + "秒</size>";
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        if (!p.AmOwner)
                        {
                            p.RpcSetNamePrivate(name);
                        }
                        else
                        {
                            p.SetName(name);
                        }
                    }
                }
                int i = 0;
                foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                {
                    if (p.PlayerControl.isImpostor())
                    {
                        p.NetTransform.RpcSnapTo(new Vector2(-30, 30));
                        i++;
                    }
                }
                if (ImpostorMoveTime <= 0)
                {
                    foreach (PlayerControl p in CachedPlayer.AllPlayers)
                    {
                        p.RpcSetName("　");
                        if (p.isImpostor())
                        {
                            p.NetTransform.RpcSnapTo(getPosition(GetRandomSpawnPosition(p)));
                        }
                    }
                }
                return;
            }
            foreach (PlayerControl player in CachedPlayer.AllPlayers)
            {
                if (player.isImpostor())
                {
                    foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                    {
                        PlayerControl pc = p.PlayerControl;
                        if (!pc.isImpostor() && pc.IsPlayer())
                        {
                            if (p != null && !p.Data.Disconnected)
                            {
                                if (!pc.IsArrest())
                                {
                                    var DistanceData = Vector2.Distance(player.transform.position, p.transform.position);
                                    if (DistanceData <= 0.5f)
                                    {
                                        Teleport(p, getPosition(SetRandomArrestPosition(p)));
                                    }
                                }
                                else
                                {
                                    Vector2 getpos = getPosition((SystemTypes)ArrestPositions[p.PlayerId]);
                                    var DistanceData = Vector2.Distance(p.transform.position, getpos);
                                    bool flag = false;
                                    flag = DistanceData >= 1f;
                                    /*if (p.PlayerId == 0)
                                    {
                                        flag = DistanceData >= 2f;
                                    } else
                                    {
                                        flag = DistanceData >= 0.5;
                                    }
                                    */
                                    if (flag)
                                    {
                                        Teleport(p, getpos);
                                    }
                                }
                            }
                        }
                    }
                }
                else if (!player.IsArrest())
                {
                    foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                    {
                        PlayerControl pc = p.PlayerControl;
                        if (pc.IsArrest())
                        {
                            if (p != null && !p.Data.Disconnected)
                            {
                                var DistanceData = Vector2.Distance(player.transform.position, p.transform.position);
                                if (DistanceData <= 0.5f)
                                {
                                    RemoveArrest(p);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
