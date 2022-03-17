using SuperNewRoles.CustomOption;
using SuperNewRoles.EndGame;
using SuperNewRoles.Patch;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode
{
    enum ModeId
    {
        No,
        Default,
        HideAndSeek,
        BattleRoyal,
        SuperHostRoles,
        Zombie,
        RandomColor,
        NotImpostorCheck,
        Detective
    }
    class ModeHandler
    {
        public static ModeId thisMode;
        public static void ClearAndReload()
        {
            if (isMode(ModeId.HideAndSeek))
            {
                thisMode = ModeId.HideAndSeek;
                HideAndSeek.main.ClearAndReload();
            } else if (isMode(ModeId.BattleRoyal))
            {
                thisMode = ModeId.BattleRoyal;
                BattleRoyal.main.ClearAndReload();
            }
            else if (isMode(ModeId.SuperHostRoles))
            {
                thisMode = ModeId.SuperHostRoles;
                SuperHostRoles.main.ClearAndReloads();
            }
            else if (isMode(ModeId.Zombie))
            {
                thisMode = ModeId.Zombie;
                Zombie.main.ClearAndReload();
            }
            else if (isMode(ModeId.RandomColor))
            {
                thisMode = ModeId.RandomColor;
                RandomColor.FixedUpdate.UpdateTime = 0f;
                RandomColor.FixedUpdate.IsRandomNameColor = RandomColor.RandomColorOptions.RandomNameColor.getBool();
                RandomColor.FixedUpdate.IsHideName = RandomColor.RandomColorOptions.HideName.getBool();
                RandomColor.FixedUpdate.IsRandomColorMeeting = RandomColor.RandomColorOptions.RandomColorMeeting.getBool();
                RandomColor.FixedUpdate.IsHideNameSet = false;
            }
            else if (isMode(ModeId.NotImpostorCheck))
            {
                thisMode = ModeId.NotImpostorCheck;
                NotImpostorCheck.main.ClearAndReload();
            }
            else if (isMode(ModeId.Detective))
            {
                thisMode = ModeId.Detective;
                Detective.main.ClearAndReload();
            }
            else {
                thisMode = ModeId.Default;
            }
        }
        public static string[] modes = new string[] { ModTranslation.getString("HideAndSeekModeName"), ModTranslation.getString("SuperHostRolesModeName"), ModTranslation.getString("BattleRoyalModeName"), ModTranslation.getString("ZombieModeName"), ModTranslation.getString("RandomColorModeName"), ModTranslation.getString("NotImpostorCheckModeName"), ModTranslation.getString("DetectiveModeName") };
        public static CustomOptionBlank Mode;
        public static CustomOption.CustomOption ModeSetting;
        public static CustomOption.CustomOption ThisModeSetting;
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> TeamHandler(IntroCutscene __instance) {
            if (isMode(ModeId.HideAndSeek)) {
                return HideAndSeek.Intro.ModeHandler(__instance);
            } else if (isMode(ModeId.BattleRoyal)) {
                return BattleRoyal.Intro.ModeHandler(__instance);
            }
            else if (isMode(ModeId.SuperHostRoles))
            {
                return SuperHostRoles.Intro.ModeHandler(__instance);
            }
            else if (isMode(ModeId.Zombie))
            {
                return Zombie.Intro.ModeHandler(__instance);
            }
            else if (isMode(ModeId.RandomColor))
            {
                return SuperHostRoles.Intro.ModeHandler(__instance);
            }
            else if (isMode(ModeId.NotImpostorCheck))
            {
                return SuperHostRoles.Intro.ModeHandler(__instance);
            }
            else if (isMode(ModeId.Detective))
            {
                return SuperHostRoles.Intro.ModeHandler(__instance);
            }
            return new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        }
        public static void IntroHandler(IntroCutscene __instance) {
            if (isMode(ModeId.HideAndSeek)) {
                HideAndSeek.Intro.IntroHandler(__instance);
            } else if (isMode(ModeId.BattleRoyal))
            {
                BattleRoyal.Intro.IntroHandler(__instance);
            }
            else if (isMode(ModeId.SuperHostRoles))
            {
                SuperHostRoles.Intro.IntroHandler(__instance);
            }
            else if (isMode(ModeId.Zombie))
            {
                Zombie.Intro.IntroHandler(__instance);
            }
            else if (isMode(ModeId.Detective))
            {
                SuperHostRoles.Intro.IntroHandler(__instance);
            }
        }

        public static void YouAreIntroHandler(IntroCutscene __instance)
        {
            if (isMode(ModeId.Zombie))
            {
                Zombie.Intro.YouAreHandle(__instance);
            }
            else if (isMode(ModeId.Detective))
            {
               　Detective.Intro.YouAreHandle(__instance);
            }
        }
        public static void OptionLoad() {
            Mode = new CustomOptionBlank(null);
            ModeSetting = CustomOption.CustomOption.Create(132, CustomOptions.cs(Color.white, "ModeSetting"), false, Mode, isHeader: true);
            ThisModeSetting = CustomOption.CustomOption.Create(133, CustomOptions.cs(Color.white, "SettingMode"), modes , ModeSetting);
            HideAndSeek.ZombieOptions.Load();
            BattleRoyal.BROption.Load();
            Zombie.ZombieOptions.Load();
            RandomColor.RandomColorOptions.Load();
            Detective.DetectiveOptions.Load();
        }
        public static void FixedUpdate(PlayerControl __instance) {
            if (isMode(ModeId.Default)) return;
            else if (isMode(ModeId.HideAndSeek)) {
                HideAndSeek.Patch.HASFixed.Postfix(__instance);
            }
            else if (isMode(ModeId.SuperHostRoles))
            {
                SuperHostRoles.FixedUpdate.Update();
            }
            else if (isMode(ModeId.BattleRoyal))
            {

            }
            else if (isMode(ModeId.Zombie))
            {
                Zombie.FixedUpdate.Update();
            }
            else if (isMode(ModeId.RandomColor))
            {
                RandomColor.FixedUpdate.Update();
            }
        }
        public static void Wrapup(GameData.PlayerInfo exiled)
        {
            if (isMode(ModeId.Default)) return;
        }
        public static ModeId GetMode() {
            if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId)) return ModeId.Default;
            if (!ModeSetting.getBool()) return ModeId.Default;
            if (isMode(ModeId.HideAndSeek)) return ModeId.HideAndSeek;
            if (isMode(ModeId.SuperHostRoles)) return ModeId.SuperHostRoles;
            if (isMode(ModeId.BattleRoyal)) return ModeId.BattleRoyal;
            if (isMode(ModeId.Zombie)) return ModeId.Zombie;
            if (isMode(ModeId.RandomColor)) return ModeId.RandomColor;
            if (isMode(ModeId.NotImpostorCheck)) return ModeId.NotImpostorCheck;
            if (isMode(ModeId.Detective)) return ModeId.Detective;
            return ModeId.No;
        }
        public static string GetThisModeIntro() {
            return ThisModeSetting.getString();
        }
        public static bool isMode(ModeId mode) {
            if (mode != ModeId.Default && !ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId))
            {
                return true;
            }
            switch (mode)
            {
                case ModeId.Default:
                    return !ModeSetting.getBool();
                case ModeId.HideAndSeek:
                    return ModeSetting.getBool() && ThisModeSetting.getString()==modes[0];
                case ModeId.BattleRoyal:
                    return ModeSetting.getBool() && ThisModeSetting.getString() == modes[2];
                case ModeId.SuperHostRoles:
                    return ModeSetting.getBool() && ThisModeSetting.getString() == modes[1];
                case ModeId.Zombie:
                    return ModeSetting.getBool() && ThisModeSetting.getString() == modes[3];
                case ModeId.RandomColor:
                    return ModeSetting.getBool() && ThisModeSetting.getString() == modes[4];
                case ModeId.NotImpostorCheck:
                    return ModeSetting.getBool() && ThisModeSetting.getString() == modes[5];
                case ModeId.Detective:
                    return ModeSetting.getBool() && ThisModeSetting.getString() == modes[6];
            }
            return false;
        }
        public static bool EndGameChecks(ShipStatus __instance,PlayerStatistics statistics) {
            if (isMode(ModeId.HideAndSeek))
            {
                return HideAndSeek.main.EndGameCheck(__instance,statistics);
            } else if (isMode(ModeId.BattleRoyal))
            {
                return BattleRoyal.main.EndGameCheck(__instance, statistics);
            }
            else if (isMode(ModeId.SuperHostRoles))
            {
                return SuperHostRoles.EndGameCheck.CheckEndGame(__instance, statistics);
            }
            else if (isMode(ModeId.Zombie))
            {
                return Zombie.main.EndGameCheck(__instance,statistics);
            }
            else if (isMode(ModeId.RandomColor))
            {
                return RandomColor.main.CheckEndGame(__instance, statistics);
            }
            else if (isMode(ModeId.NotImpostorCheck))
            {
                return NotImpostorCheck.WinCheck.CheckEndGame(__instance);
            }
            else if (isMode(ModeId.NotImpostorCheck))
            {
                return NotImpostorCheck.WinCheck.CheckEndGame(__instance);
            }
            else if (isMode(ModeId.Detective))
            {
                return Detective.WinCheckPatch.CheckEndGame(__instance);
            }
            return false;
        }
        public static bool IsBlockVanilaRole()
        {
            if (isMode(ModeId.NotImpostorCheck)) return false;
            if (isMode(ModeId.Detective)) return false;
            if (isMode(ModeId.Default)) return false;
            return true;
        }
        public static bool IsBlockGuardianAngelRole()
        {
            if (isMode(ModeId.Default)) return true;
            return IsBlockVanilaRole();
        }
    }
}
