using System;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Patches.CheckGameEndPatch;

namespace SuperNewRoles.Mode;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
class CloseDoorsPatch
{
    public static bool Prefix() => !ModeHandler.IsMode(ModeId.Zombie, ModeId.BattleRoyal, ModeId.CopsRobbers, ModeId.HideAndSeek, ModeId.PantsRoyal);
}
public enum ModeId
{
    No,
    Default,
    HideAndSeek,
    BattleRoyal,
    SuperHostRoles,
    Zombie,
    RandomColor,
    NotImpostorCheck,
    Detective,
    Werewolf,
    CopsRobbers,
    VanillaHns,
    PantsRoyal
}
public static class ModeHandler
{
    public static ModeId thisMode;
    public static void ClearAndReload()
    {
        PlusModeHandler.ClearAndReload();
        BattleRoyal.Main.ClearAndReload();
        if (IsMode(ModeId.BattleRoyal, false))
        {
            thisMode = ModeId.BattleRoyal;
        }
        else if (IsMode(ModeId.HideAndSeek, false))
        {
            HideAndSeek.main.ClearAndReloads();
            thisMode = HideAndSeek.main.IsAllInMod ? ModeId.Default : ModeId.SuperHostRoles;
            if (thisMode == ModeId.SuperHostRoles) SuperHostRoles.Main.ClearAndReloads();
        }
        else if (IsMode(ModeId.SuperHostRoles, false))
        {
            thisMode = ModeId.SuperHostRoles;
            SuperHostRoles.Main.ClearAndReloads();
        }
        else if (IsMode(ModeId.Zombie, false))
        {
            thisMode = ModeId.Zombie;
            Zombie.Main.ClearAndReload();
        }
        else if (IsMode(ModeId.RandomColor, false))
        {
            thisMode = ModeId.RandomColor;
            RandomColor.FixedUpdate.UpdateTime = 0f;
            RandomColor.FixedUpdate.IsRandomNameColor = RandomColor.RandomColorOptions.RandomNameColor.GetBool();
            RandomColor.FixedUpdate.IsHideName = RandomColor.RandomColorOptions.HideName.GetBool();
            RandomColor.FixedUpdate.IsRandomColorMeeting = RandomColor.RandomColorOptions.RandomColorMeeting.GetBool();
            RandomColor.FixedUpdate.IsHideNameSet = false;
        }
        else if (IsMode(ModeId.NotImpostorCheck, false))
        {
            thisMode = ModeId.NotImpostorCheck;
            NotImpostorCheck.Main.ClearAndReload();
        }
        else if (IsMode(ModeId.Detective, false))
        {
            thisMode = ModeId.Detective;
            Detective.Main.ClearAndReload();
        }
        else if (IsMode(ModeId.CopsRobbers, false))
        {
            thisMode = ModeId.CopsRobbers;
            CopsRobbers.Main.ClearAndReloads();
        }
        else if (IsMode(ModeId.PantsRoyal, false))
        {
            thisMode = ModeId.PantsRoyal;
            PantsRoyal.main.ClearAndReloads();
        }
        else
        {
            thisMode = ModeId.Default;
        }
        if (!IsMode(ModeId.Default))
        {
            SuperHostRoles.BlockTool.IsCom = false;
        }
    }
    public static string[] modes = new string[] { ModTranslation.GetString("HideAndSeekModeName"), ModTranslation.GetString("SuperHostRolesModeName"), ModTranslation.GetString("BattleRoyalModeName"), ModTranslation.GetString("ZombieModeName"), ModTranslation.GetString("RandomColorModeName"), ModTranslation.GetString("NotImpostorCheckModeName"), ModTranslation.GetString("DetectiveModeName"), ModTranslation.GetString("CopsRobbersModeName"), ModTranslation.GetString("WerewolfModeName"), ModTranslation.GetString("PantsRoyalModeName") };

    public static string PlayingOnSuperNewRoles => $"Playing on {SuperNewRolesPlugin.ColorModName}";


    /// <summary>Mode設定を封印するか</summary>
    /// <value>true : 封印する, false : 封印しない</value>
    private const bool isSealModeOption = true;

    /// <summary>Modeの封印処理が有効か (外部取得 及び カスタムサーバ使用下における封印処理の除外)</summary>
    /// <returns>true : 有効(封印する) / false : 無効(封印しない)</returns>
    public static bool EnableModeSealing => isSealModeOption && !ModHelpers.IsCustomServer();

    public static CustomOptionBlank Mode;
    public static CustomOption ModeSetting;
    public static CustomOption ThisModeSetting;
    public static Il2CppSystem.Collections.Generic.List<PlayerControl> TeamHandler(IntroCutscene __instance)
    {
        if (IsMode(ModeId.BattleRoyal)) return BattleRoyal.Intro.ModeHandler();
        else if (IsMode(ModeId.SuperHostRoles)) return SuperHostRoles.Intro.ModeHandler(__instance);
        else if (IsMode(ModeId.Zombie)) return Zombie.Intro.ModeHandler();
        else if (IsMode(ModeId.RandomColor)) return SuperHostRoles.Intro.ModeHandler(__instance);
        else if (IsMode(ModeId.NotImpostorCheck)) return SuperHostRoles.Intro.ModeHandler(__instance);
        else if (IsMode(ModeId.Detective)) return SuperHostRoles.Intro.ModeHandler(__instance);
        else if (IsMode(ModeId.PantsRoyal)) return PantsRoyal.main.IntroHandler(__instance);
        else if (IsMode(ModeId.CopsRobbers))
        {
            var Data = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
            Data.Add(PlayerControl.LocalPlayer);
            return Data;
        }
        return new Il2CppSystem.Collections.Generic.List<PlayerControl>();
    }
    public static (string, string, Color) IntroHandler(IntroCutscene __instance)
    {
        if (IsMode(ModeId.BattleRoyal)) return BattleRoyal.Intro.IntroHandler(__instance);
        else if (IsMode(ModeId.Zombie)) return Zombie.Intro.IntroHandler(__instance);
        else if (IsMode(ModeId.PantsRoyal)) return PantsRoyal.main.PantsHandler(__instance);
        return ("NONE", "NONE", new());
    }
    public static void YouAreIntroHandler(IntroCutscene __instance)
    {
        if (IsMode(ModeId.Zombie)) Zombie.Intro.YouAreHandle(__instance);
        else if (IsMode(ModeId.Detective)) Detective.Intro.YouAreHandle(__instance);
        else if (IsMode(ModeId.BattleRoyal)) SuperNewRoles.Mode.BattleRoyal.Intro.YouAreHandle(__instance);
        else if (IsMode(ModeId.PantsRoyal)) PantsRoyal.main.YouAreHandle(__instance);
    }
    public static void OptionLoad()
    {
        Mode = new CustomOptionBlank(null);
        ModeSetting = CustomOption.Create(101200, true, CustomOptionType.Generic, Cs(new Color(252f / 187f, 200f / 255f, 0, 1f), "ModeSetting"), false, Mode, isHeader: true);
        ThisModeSetting = CustomOption.Create(101300, true, CustomOptionType.Generic, "SettingMode", modes, ModeSetting);
        BattleRoyal.BROption.Load();
        Zombie.ZombieOptions.Load();
        RandomColor.RandomColorOptions.Load();
        Detective.DetectiveOptions.Load();
        CopsRobbers.CopsRobbersOptions.Load();
        //Werewolf.WerewolfOptions.Load();
    }
    public static void HudUpdate(HudManager __instance)
    {
        switch (GetMode())
        {
            case ModeId.CopsRobbers:
                CopsRobbers.Main.HudUpdate();
                break;
            case ModeId.PantsRoyal:
                PantsRoyal.UpdateHandler.HudUpdate();
                break;
        }
    }
    public static void FixedUpdate(PlayerControl __instance)
    {
        if (IsMode(ModeId.SuperHostRoles))
            //PlayerControl.LocalPlayer.RpcSetName("<size=>次のターゲット:よッキング</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
            SuperHostRoles.FixedUpdate.Update();
        else if (IsMode(ModeId.BattleRoyal)) BattleRoyal.Main.FixedUpdate();
        else if (IsMode(ModeId.Zombie)) Zombie.FixedUpdate.Update();
        else if (IsMode(ModeId.RandomColor)) RandomColor.FixedUpdate.Update();

    }
    public static void Wrapup(GameData.PlayerInfo exiled)
    {
        if (IsMode(ModeId.Default)) return;
    }
    public static ModeId GetMode(bool IsChache = true)
    {
        if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId)) return ModeId.Default;
        if (EnableModeSealing) return ModeId.Default; // Modeの封印処理が有効な時, 強制で通常モードにする
        if (IsChache) return thisMode;
        foreach (ModeId id in Enum.GetValues(typeof(ModeId)))
            if (IsMode(id, false)) return id;
        return ModeId.No;
    }
    public static string GetThisModeIntro()
    {
        if (IsMode(ModeId.HideAndSeek))
        {
            return $"{ThisModeSetting.GetString()}({(HideAndSeek.main.IsAllInMod ? "Default" : "SHR")})";
        }
        return ThisModeSetting.GetString();
    }
    public static bool IsMode(params ModeId[] modes)
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay || (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId)))
        {
            foreach (ModeId mode in modes)
            {
                if (mode == ModeId.Default)
                {
                    return true;
                }
            }
            return false;
        }
        foreach (ModeId mode in modes)
        {
            if (IsMode(mode))
            {
                return true;
            }
        }
        return false;
    }
    public static bool IsMode(ModeId mode, bool IsChache = true)
    {
        // vanilla Mode
        if (mode is ModeId.VanillaHns)
            return GameOptionsManager.Instance.currentGameMode == GameModes.HideNSeek;
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay || !PlayerControlHelper.IsMod(AmongUsClient.Instance.HostId))
            return mode is ModeId.Default;

        // Mod Mode
        if (EnableModeSealing) // Modeの封印処理が有効な時, 強制で通常モードと判定する。
            return mode is ModeId.Default;
        if (mode is ModeId.HideAndSeek && IsChache)
            return IsMode(ModeId.HideAndSeek, false);
        if (mode is ModeId.Werewolf)
            return ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[8];
        if (IsChache)
            return mode == thisMode;
        if (mode is ModeId.Default)
            return !ModeSetting.GetBool();
        return mode switch
        {
            ModeId.HideAndSeek => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[0],
            ModeId.BattleRoyal => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[2],
            ModeId.SuperHostRoles => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[1],
            ModeId.Zombie => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[3],
            ModeId.RandomColor => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[4],
            ModeId.NotImpostorCheck => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[5],
            ModeId.Detective => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[6],
            ModeId.CopsRobbers => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[7],
            ModeId.PantsRoyal => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[9],
            _ => false,
        };
    }
    public static bool EndGameChecks(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (IsMode(ModeId.BattleRoyal)) return BattleRoyal.Main.EndGameCheck(__instance);
        else if (IsMode(ModeId.SuperHostRoles)) return SuperHostRoles.EndGameCheck.CheckEndGame(__instance, statistics);
        else if (IsMode(ModeId.Zombie)) return Zombie.Main.EndGameCheck(__instance, statistics);
        else if (IsMode(ModeId.RandomColor)) return RandomColor.Main.CheckEndGame(__instance, statistics);
        else if (IsMode(ModeId.NotImpostorCheck)) return NotImpostorCheck.WinCheck.CheckEndGame(__instance);
        else if (IsMode(ModeId.Detective)) return Detective.WinCheckPatch.CheckEndGame(__instance);
        else if (IsMode(ModeId.CopsRobbers)) return CopsRobbers.Main.EndGameCheck(__instance);
        else if (IsMode(ModeId.CopsRobbers)) return CopsRobbers.CheckEndGame.EndGameCheck(__instance);
        return false;
    }
    public static bool EndGameCheckHnSs(ShipStatus __instance, PlayerStatistics statistics)
    {
        if (IsMode(ModeId.SuperHostRoles)) return EndGameCheck.CheckEndGameHnSs(__instance, statistics);
        return false;
    }
    public static bool IsBlockVanillaRole()
    {
        return !IsMode(ModeId.NotImpostorCheck) && !IsMode(ModeId.Detective) && !IsMode(ModeId.Default);
    }
    public static bool IsBlockGuardianAngelRole()
    {
        return IsMode(ModeId.Default) || IsBlockVanillaRole();
    }
    public static void HideName(this PlayerControl p)
    {
        string name = "<color=#00000000>" + p.GetDefaultName();

        p.RpcSetName(name);
    }
    public static void HideName()
    {
        if (AmongUsClient.Instance.AmHost)
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                p.HideName();
                SuperNewRolesPlugin.Logger.LogInfo("[ModeHandler : HideName()]" + p.GetDefaultName() + "の名前を透明に変更しました");
            }
        }
        else SuperNewRolesPlugin.Logger.LogInfo("[ModeHandler : HideName()]" + PlayerControl.LocalPlayer.GetDefaultName() + "ホストでない為、名前を透明化する処理を飛ばしました。");
    }
}