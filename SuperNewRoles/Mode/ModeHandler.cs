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
        SuperHostRoles
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
            }
            else if (isMode(ModeId.SuperHostRoles))
            {
                thisMode = ModeId.SuperHostRoles;
            }
            else {
                thisMode = ModeId.Default;
            }
        }
        public static string[] modes = new string[] { ModTranslation.getString("HideAndSeekModeName"), ModTranslation.getString("SuperHostRolesModeName"), ModTranslation.getString("BattleRoyalModeName") };
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
            else if (isMode(ModeId.BattleRoyal))
            {
                BattleRoyal.Intro.IntroHandler(__instance);
            }
        }
        public static void OptionLoad() {
            Mode = new CustomOptionBlank(null);
            ModeSetting = CustomOption.CustomOption.Create(132, CustomOptions.cs(Color.white, "ModeSetting"), false, Mode, isHeader: true);
            ThisModeSetting = CustomOption.CustomOption.Create(133, CustomOptions.cs(Color.white, "SettingMode"), modes , ModeSetting);
            HideAndSeek.HASOptions.Load();
            BattleRoyal.BROption.Load();
        }
        public static void FixedUpdate(PlayerControl __instance) {
            if (isMode(ModeId.Default)) return;
            if (isMode(ModeId.HideAndSeek)) {
                HideAndSeek.Patch.HASFixed.Postfix(__instance);
            }
            if (isMode(ModeId.SuperHostRoles))
            {
                SuperHostRoles.FixedUpdate.Update();
            }
            if (isMode(ModeId.BattleRoyal))
            {
                
            }
        }
        public static ModeId GetMode() {
            if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId)) return ModeId.Default;
            if (!ModeSetting.getBool()) return ModeId.Default;
            if (isMode(ModeId.HideAndSeek)) return ModeId.HideAndSeek;
            if (isMode(ModeId.SuperHostRoles)) return ModeId.HideAndSeek;
            if (isMode(ModeId.BattleRoyal)) return ModeId.BattleRoyal;
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
            return false;
        }
    }
}
