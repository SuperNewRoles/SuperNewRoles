using System;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class PoolablePrefabManager
{
    public static PoolablePlayer PlayerPrefab;
    public static void OnIntroCutsceneDestroy(IntroCutscene introCutscene)
    {
        PlayerPrefab = introCutscene.PlayerPrefab;
    }
    public static PoolablePlayer GeneratePlayer(PlayerControl player)
    {
        if (PlayerPrefab == null)
        {
            throw new Exception("PlayerPrefab is not set");
        }
        var playerControl = GameObject.Instantiate(PlayerPrefab);
        playerControl.UpdateFromPlayerData(player.Data, player.CurrentOutfitType, PlayerMaterial.MaskType.None, false);
        return playerControl;
    }
}
