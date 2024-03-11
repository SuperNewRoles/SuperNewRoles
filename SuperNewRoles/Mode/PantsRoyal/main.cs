using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;

namespace SuperNewRoles.Mode.PantsRoyal;
public static class main
{
    public static List<byte> PantsHaversId;
    public static bool IsStart;
    public static int LastCount;
    public static bool IsMove;
    public static float ShowRoleTime;
    public static float LastUpdateTime = -999;
    public static TurnData CurrentTurnData;
    public static Color ModeColor = new Color32(255, 20, 147, byte.MaxValue);
    public static List<PlayerControl> _pantsHavers;
    public static List<PlayerControl> PantsHavers
    {
        get
        {
            if (PantsHaversId.Count != _pantsHavers.Count)
                UpdatePantsHaverCache();
            return _pantsHavers;
        }
    }
    public static void ClearAndReloads()
    {
        PantsHaversId = new();
        _pantsHavers = new();
        LastCount = 99;
        IsMove = false;
        IsStart = false;
        ShowRoleTime = 6;
        LastUpdateTime = 7;
        CurrentTurnData = null;
    }
    readonly static Dictionary<SystemTypes, Vector2> AirshipSpawnPositions = new()
    {
        {SystemTypes.Security, new(7.0886f, -12.501f) }, //セキュ
        {SystemTypes.VaultRoom, new(-8.7701f, 12.4399f) }, //金庫
        {SystemTypes.MainHall, new(10.75f, -0.05f)},
        {SystemTypes.MedBay, new(26.6f, -5.7f)},
        {SystemTypes.Cockpit, new(-22.09f, -1)},
        //トイレの確率2倍！
        {SystemTypes.Lounge,new() },
        {SystemTypes.Records,new() },

    };
    readonly static List<Vector2> AirshipToiletSpawnPositions = new()
    {
        new(29.3f, 7.5f),
        new(30.825f, 7.5f),
        new(32.325f, 7.5f),
        new(33.75f, 7.5f),
    };
    public static Vector2 GetRandomAirshipPosition()
    {
        SystemTypes pos = ModHelpers.GetRandom(AirshipSpawnPositions.Keys.ToList());
        if (pos is SystemTypes.Lounge or SystemTypes.Records)
            return ModHelpers.GetRandom(AirshipToiletSpawnPositions);
        return AirshipSpawnPositions[pos];
    }
    public static void Debug_SpawnLocalPlayer()
    {
        PlayerControl.LocalPlayer.RpcSnapTo(GetRandomAirshipPosition());
    }
    public static void GameEnd()
    {
        main.CurrentTurnData = null;
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.IsAlive())
            {
                var writer = RPCHelper.StartRPC(CustomRPC.ShareWinner);
                writer.Write(player.PlayerId);
                writer.EndRPC();
                player.RpcSetRole(AmongUs.GameOptions.RoleTypes.CrewmateGhost);
                player.Data.IsDead = false;
                Logger.Info("クルー！");
            }
            else
            {
                player.RpcSetRole(AmongUs.GameOptions.RoleTypes.ImpostorGhost);
                Logger.Info("インポ");
            }
            Logger.Info(player.Data.Role.Role.ToString(), player.GetDefaultName());
        }
        RPCHelper.RpcSyncGameData();
        new LateTask(() =>
        {
            GameManager.Instance.LogicOptions.Manager.RpcEndGame(GameOverReason.HumansByTask, false);
        }, 0.1f);
    }
    public static void AssignPants()
    {
        int AlivePlayerCount = 0;
        List<PlayerControl> targets = new();
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            if (p.IsAlive())
            {
                targets.Add(p);
                AlivePlayerCount++;
            }
        int PantsHaversLimit = (int)Math.Floor(AlivePlayerCount / 2.0);
        Logger.Info(PantsHaversLimit.ToString() + ":" + targets.Count.ToString());
        for (int i = 0; i < PantsHaversLimit; i++)
        {
            int targetindex = ModHelpers.GetRandomIndex(targets);
            SetHavePants(targets[targetindex], false);
            targets.RemoveAt(targetindex);
        }
        foreach (PlayerControl target in targets)
            SetDontHavePants(target, false);
        UpdatePantsHaverCache();
    }
    public static void OnMurderClick(PlayerControl source, PlayerControl target)
    {
        if (!IsMove || ShowRoleTime >= 0 || !main.CurrentTurnData.IsStarted) return;
        if (!IsPantsHaver(source) && IsPantsHaver(target))
        {
            StealPants(target, source);
            source.RpcSetSkin(target.Data.DefaultOutfit.SkinId);
            target.RpcSetSkin("");
            target.RpcShowGuardEffect(target);
        }
    }
    public static void AssignRole()
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == 0)
                player.SetRole(AmongUs.GameOptions.RoleTypes.Impostor);
            else
                player.RpcSetRoleDesync(AmongUs.GameOptions.RoleTypes.Impostor);
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                if (player.PlayerId != p.PlayerId)
                {
                    if (p.PlayerId == 0)
                        player.SetRole(AmongUs.GameOptions.RoleTypes.Scientist);
                    else
                        player.RpcSetRoleDesync(AmongUs.GameOptions.RoleTypes.Scientist, p);
                }
        }
        CurrentTurnData = new();
        AssignPants();
        AssignAllHaversPants();
    }
    public static void AssignAllHaversPants()
    {
        UpdatePantsHaverCache();
        List<SkinData> targetSkins = FastDestroyableSingleton<HatManager>.Instance.allSkins.ToList();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            player.RpcSetHat("");
            player.RpcSetPet("");
            player.RpcSetVisor("");
            if (IsPantsHaver(player))
            {
                int targetSkinIndex = ModHelpers.GetRandomIndex(targetSkins);
                player.RpcSetSkin(targetSkins[targetSkinIndex].ProductId);
                targetSkins.RemoveAt(targetSkinIndex);
            }
            else
            {
                player.RpcSetSkin("");
            }
        }
    }
    public static Il2CppSystem.Collections.Generic.List<PlayerControl> IntroHandler(IntroCutscene __instance)
    {
        Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            Teams.Add(player);
        }
        return Teams;
    }
    public static (string, string, Color) PantsHandler(IntroCutscene __instance)
    {
        return (ModTranslation.GetString("PantsRoyalModeName"), IntroData.GetTitle("PantsRoyal", 14), ModeColor);
    }
    public static void YouAreHandle(IntroCutscene __instance)
    {
        __instance.YouAreText.text = ModTranslation.GetString("PantsRoyalPantsYourText");
        var data = ModeColor;
        if (IsPantsHaver(PlayerControl.LocalPlayer))
        {
            __instance.RoleText.text = ModTranslation.GetString("PantsRoyalPantsHaverIntroName");
            __instance.RoleBlurbText.text = IntroData.GetTitle("PantsRoyalPantsHaverIntro", 1);
        }
        else
        {
            __instance.RoleText.text = ModTranslation.GetString("PantsRoyalPantsDontHaverIntroName");
            __instance.RoleBlurbText.text = IntroData.GetTitle("PantsRoyalPantsDontHaverIntro", 1);
        }
        __instance.YouAreText.text = ModTranslation.GetString("PantsRoyalPantsYourText");
        /*
        __instance.YouAreText.color = data.color;          //あなたのロールは...を役職の色に変更
        __instance.RoleText.color = data.color;             //役職名の色を変更
        __instance.RoleBlurbText.color = data.color;        //イントロの簡易説明の色を変更
        __instance.YouAreText.color = Palette.ImpostorRed;     //あなたのロールは...を役職の色に変更
        __instance.RoleText.color = Palette.ImpostorRed;       //役職名の色を変更
        __instance.RoleBlurbText.color = Palette.ImpostorRed;  //イントロの簡易説明の色を変更
        */
    }
    public static void UpdatePantsHaverCache()
    {
        List<PlayerControl> newList = new();
        foreach (byte playerId in PantsHaversId)
        {
            newList.Add(ModHelpers.PlayerById(playerId));
        }
        _pantsHavers = newList;
    }
    public static void SetDontHavePants(PlayerControl player, bool IsUpdateCache = true)
    {
        PantsHaversId.Remove(player.PlayerId);
        if (IsUpdateCache)
            UpdatePantsHaverCache();
    }
    public static void SetHavePants(PlayerControl player, bool IsUpdateCache = true)
    {
        PantsHaversId.Add(player.PlayerId);
        if (IsUpdateCache)
            UpdatePantsHaverCache();
    }
    public static void StealPants(PlayerControl oldHaver, PlayerControl newHaver)
    {
        SetDontHavePants(oldHaver, false);
        SetHavePants(newHaver, false);
        UpdatePantsHaverCache();
    }
    public static bool IsPantsHaver(PlayerControl player) => IsPantsHaver(player.PlayerId);
    public static bool IsPantsHaver(byte playerId) => PantsHaversId.Contains(playerId);
}