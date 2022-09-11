using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapOptions;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    public static class WaitSpawn
    {
        private static Dictionary<PlayerControl, Vector3> SpawnPosition;
        private static int LastCount;
        private static bool IsWaitSpawnNow;
        public static bool CanChangeName() => IsWaitSpawnNow;
        public static void Reset()
        {
            SpawnPosition = new();
            LastCount = -1;
            IsWaitSpawnNow = true;
        }
        public static void SetNull()
        {
            SpawnPosition = null;
            IsWaitSpawnNow = false;
        }
        public static void SetSpawn(PlayerControl player, Vector3 Position)
        {
            SpawnPosition[player] = Position;
        }
        public static void AllSpawn()
        {
            foreach (var data in SpawnPosition)
            {
                if (data.Key == null) continue;
                if (data.Key.Data.Disconnected) continue;
                data.Key.RpcSnapTo(data.Value);
            }
            SetNull();
        }
        public static void Update()
        {
            Logger.Info($"{!MapOption.WaitSpawn} || {MapOption.RandomSpawn} || {!IsWaitSpawnNow}");
            if (!MapOption.WaitSpawn || MapOption.RandomSpawn || !IsWaitSpawnNow) return;
            int NotLoadedCount = 0;
            List<PlayerControl> players = new();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                Logger.Info($"{player.Data.PlayerName} : {player.transform.position.x},{player.transform.position.z}");
                if (player.IsDead()) continue;
                //未ロード者
                if (ModHelpers.IsPositionDistance(player.transform.position, new Vector2(3, 6), 0.5f) ||
                            ModHelpers.IsPositionDistance(player.transform.position, new Vector2(-25, 40), 0.5f) ||
                            ModHelpers.IsPositionDistance(player.transform.position, new Vector2(-1.4f, 2.3f), 0.5f)
                            )
                {
                    NotLoadedCount++;
                } else if (!ModHelpers.IsPositionDistance(player.transform.position, new Vector2(-30, 30), 0.5f))
                {
                    SetSpawn(player, player.transform.position);
                    player.RpcSnapTo(new Vector2(-30, 30));
                } else
                {
                    players.Add(player);
                    player.RpcSnapTo(new Vector2(-30, 30));
                }
            }
            if (LastCount != players.Count)
            {
                LastCount = players.Count;
                string name = "\n\n\n\n\n\n\n\n<size=300%><color=white>" + ModeHandler.PlayingOnSuperNewRoles + "</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=200%><color=white>" + string.Format(ModTranslation.GetString("CopsSpawnLoading"), NotLoadedCount);
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    p.RpcSetNamePrivate(name);
                }
            }
            if (NotLoadedCount <= 0) AllSpawn();
        }
    }
}
