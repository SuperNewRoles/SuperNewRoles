using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Mode.CopsRobbers;

public static class Main
{
    public static void ClearAndReloads()
    {
        IsStart = false;
        ArrestPositions = new();
        Arrest = new();
        IsMove = false;
        SpawnPosition = new();
        LastCount = 0;
        OnGameEndPatch.EndData = null;
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
        return Arrest.Contains(player.PlayerId);
    }
    public static Dictionary<MapNames, List<SystemTypes>> Rooms = new()
        {
            { MapNames.Skeld, new List<SystemTypes>() { SystemTypes.Comms, SystemTypes.Nav, SystemTypes.Security } },
            { MapNames.Mira, new List<SystemTypes>() { SystemTypes.Comms, SystemTypes.Greenhouse, SystemTypes.Launchpad, SystemTypes.Storage } },
            { MapNames.Polus, new List<SystemTypes>() { SystemTypes.Electrical, SystemTypes.Specimens, SystemTypes.Weapons } },
            { MapNames.Airship, new List<SystemTypes>() { SystemTypes.Records, SystemTypes.Security, SystemTypes.VaultRoom } }
        };
    public static MapNames GetMap()
    {
        int mapid = GameManager.Instance.LogicOptions.currentGameOptions.MapId;
        return (MapNames)mapid;
    }
    public static bool EndGameCheck(ShipStatus __instance)
    {
        bool impostorwin = true;
        int ImpostorCount = 0;
        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
        {
            if (!p.Data.Disconnected)
            {
                if (p.IsImpostor()) ImpostorCount++;
                if (!p.IsImpostor() && !p.IsArrest())
                {
                    impostorwin = false;
                }
            }
        }
        if (impostorwin)
        {
            __instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return true;
        }
        else if (GameData.Instance.TotalTasks > 0 && GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
        {
            __instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
            return true;
        }
        else if (ImpostorCount <= 0)
        {
            __instance.enabled = false;
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
            return true;
        }
        else if (SuperHostRoles.EndGameCheck.CheckAndEndGameForFoxHouwaWin(__instance))
            return false;
        else if (SuperHostRoles.EndGameCheck.CheckAndEndGameForWorkpersonWin(__instance))
            return false;
        else
            return false;
    }
    public static void ChangeCosmetics()
    {
        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
        {
            p.RpcSetPet("");
            p.RpcSetVisor("");
            if (p.IsImpostor())
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
    public static Vector2 GetPosition(SystemTypes type)
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
    static float ImpostorMoveTime;
    static int LastCount;
    static float LastUpdate;
    static float UpdateTime = 0.1f;
    public static List<byte> TeleportIDs = new();
    public static void Teleport(PlayerControl player, Vector2 position) => player.RpcSnapTo(position);

    public static void HudUpdate()
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            if (PlayerControl.LocalPlayer.Data.PlayerName == PlayerControl.LocalPlayer.GetDefaultName())
            {
                SetNameUpdate.Postfix(CachedPlayer.LocalPlayer);
            }
            return;
        }
        if (!IsStart) return;
        if (!IsMove)
        {
            bool IsMoveOK = true;
            List<PlayerControl> players = new();
            int NotLoadedCount = 0;
            if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4)
            {
                foreach (CachedPlayer p in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (ModHelpers.IsPositionDistance(p.transform.position, new Vector2(3, 6), 0.5f) ||
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
                string name = "\n\n\n\n\n\n\n\n<size=300%><color=white>" + ModeHandler.PlayingOnSuperNewRoles + "</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%><color=white>" + string.Format(ModTranslation.GetString("CopsSpawnLoading"), NotLoadedCount);
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                {
                    p.RpcSetNamePrivate(name);
                }
            }

            int i = 0;
            foreach (PlayerControl p in players.AsSpan())
            {
                if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4)
                {
                    p.RpcSnapTo(new Vector2(-30, 30));
                    i++;
                }
            }
            if (IsMoveOK)
            {
                IsMove = true;
                ImpostorMoveTime = 5;
                LastUpdate = 6;
                foreach (CachedPlayer p in CachedPlayer.AllPlayers.AsSpan())
                {
                    if (!p.PlayerControl.IsImpostor())
                    {
                        p.PlayerControl.RpcSnapTo(GetPosition(GetRandomSpawnPosition(p)));
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
                string name = "\n\n\n\n\n<size=300%><color=white>" + ModeHandler.PlayingOnSuperNewRoles + "</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%>" + ModTranslation.GetString("CopsImpostorCome") + ((int)(LastUpdate + 1)).ToString() + ModTranslation.GetString("second") + "</size>";
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
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
            foreach (CachedPlayer p in CachedPlayer.AllPlayers.AsSpan())
            {
                if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4 && p.PlayerControl.IsImpostor())
                {
                    p.PlayerControl.RpcSnapTo(new Vector2(-30, 30));
                    i++;
                }
            }
            if (ImpostorMoveTime <= 0)
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
                {
                    p.RpcSetName(p.GetDefaultName());
                    if (CopsRobbersOptions.CRHideName.GetBool() && CopsRobbersOptions.CopsRobbersMode.GetBool()) ModeHandler.HideName();
                    if (GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4 && p.IsImpostor())
                    {
                        p.RpcSnapTo(GetPosition(GetRandomSpawnPosition(p)));
                    }
                }
            }
            return;
        }

        SetNameUpdate.Postfix(CachedPlayer.LocalPlayer);
        UpdateTime -= Time.deltaTime;
        if (UpdateTime <= 0)
        {
            UpdateTime = 0.5f;
            RoleSystem.RoleSetName();
        }
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.IsImpostor())
            {
                foreach (CachedPlayer p in CachedPlayer.AllPlayers.AsSpan())
                {
                    PlayerControl pc = p.PlayerControl;
                    if (!pc.IsImpostor() && !pc.IsBot())
                    {
                        if (p != null && !p.Data.Disconnected)
                        {
                            if (!pc.IsArrest())
                            {
                                var DistanceData = Vector2.Distance(player.transform.position, p.transform.position);
                                if (DistanceData <= 0.5f)
                                {
                                    Teleport(p, GetPosition(SetRandomArrestPosition(p)));
                                }
                            }
                            else
                            {
                                Vector2 getpos = GetPosition((SystemTypes)ArrestPositions[p.PlayerId]);
                                var DistanceData = Vector2.Distance(p.transform.position, getpos);
                                bool flag = false;
                                flag = DistanceData >= 1f;

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
                foreach (CachedPlayer p in CachedPlayer.AllPlayers.AsSpan())
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