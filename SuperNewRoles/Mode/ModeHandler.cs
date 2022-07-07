using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.EndGame;
using SuperNewRoles.Patch;
using UnityEngine;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
    class CloseDoorsPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            if (ModeHandler.isMode(ModeId.Zombie) || ModeHandler.isMode(ModeId.Werewolf) ||
                ModeHandler.isMode(ModeId.BattleRoyal) || ModeHandler.isMode(ModeId.HideAndSeek) ||
                ModeHandler.isMode(ModeId.CopsRobbers)) return false;
            return true;
        }
    }
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
        Detective,
        Werewolf,
        CopsRobbers,
        LevelUp
    }
    class ModeHandler
    {
        public static ModeId thisMode;
        public static void ClearAndReload()
        {
            PlusModeHandler.ClearAndReload();
            if (isMode(ModeId.HideAndSeek, false))
            {
                thisMode = ModeId.HideAndSeek;
                HideAndSeek.main.ClearAndReload();
            }
            else if (isMode(ModeId.BattleRoyal, false))
            {
                thisMode = ModeId.BattleRoyal;
                BattleRoyal.main.ClearAndReload();
            }
            else if (isMode(ModeId.SuperHostRoles, false))
            {
                thisMode = ModeId.SuperHostRoles;
                SuperHostRoles.main.ClearAndReloads();
            }
            else if (isMode(ModeId.Zombie, false))
            {
                thisMode = ModeId.Zombie;
                Zombie.main.ClearAndReload();
            }
            else if (isMode(ModeId.RandomColor, false))
            {
                thisMode = ModeId.RandomColor;
                RandomColor.FixedUpdate.UpdateTime = 0f;
                RandomColor.FixedUpdate.IsRandomNameColor = RandomColor.RandomColorOptions.RandomNameColor.getBool();
                RandomColor.FixedUpdate.IsHideName = RandomColor.RandomColorOptions.HideName.getBool();
                RandomColor.FixedUpdate.IsRandomColorMeeting = RandomColor.RandomColorOptions.RandomColorMeeting.getBool();
                RandomColor.FixedUpdate.IsHideNameSet = false;
            }
            else if (isMode(ModeId.NotImpostorCheck, false))
            {
                thisMode = ModeId.NotImpostorCheck;
                NotImpostorCheck.main.ClearAndReload();
            }
            else if (isMode(ModeId.Detective, false))
            {
                thisMode = ModeId.Detective;
                Detective.main.ClearAndReload();
            }
            else if (isMode(ModeId.Werewolf, false))
            {
                thisMode = ModeId.Werewolf;
                Werewolf.main.ClearAndReload();
            }
            else if (isMode(ModeId.CopsRobbers, false))
            {
                thisMode = ModeId.CopsRobbers;
                CopsRobbers.main.ClearAndReloads();
            }
            /*            else if (isMode(ModeId.LevelUp, false))
                        {
                            thisMode = ModeId.LevelUp;
                            LevelUp.main.ClearAndReloads();
                        }*/
            else
            {
                thisMode = ModeId.Default;
            }
            if (!isMode(ModeId.Default))
            {
                SuperHostRoles.BlockTool.IsCom = false;
            }
        }
        public static string[] modes = new string[] { ModTranslation.getString("HideAndSeekModeName"), ModTranslation.getString("SuperHostRolesModeName"), ModTranslation.getString("BattleRoyalModeName"), ModTranslation.getString("ZombieModeName"), ModTranslation.getString("RandomColorModeName"), ModTranslation.getString("NotImpostorCheckModeName"), ModTranslation.getString("DetectiveModeName"), ModTranslation.getString("CopsRobbersModeName") };//ModTranslation.getString("WerewolfModeName") };

        public const string PlayingOnSuperNewRoles = "Playing on <color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";

        public static CustomOptionBlank Mode;
        public static CustomOption.CustomOption ModeSetting;
        public static CustomOption.CustomOption ThisModeSetting;
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> TeamHandler(IntroCutscene __instance)
        {
            if (isMode(ModeId.HideAndSeek))
            {
                return HideAndSeek.Intro.ModeHandler(__instance);
            }
            else if (isMode(ModeId.BattleRoyal))
            {
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
            else if (isMode(ModeId.Werewolf))
            {
                var Data = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                Data.Add(PlayerControl.LocalPlayer);
                return Data;
            }
            else if (isMode(ModeId.CopsRobbers))
            {
                var Data = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                Data.Add(PlayerControl.LocalPlayer);
                return Data;
            }
            return new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        }
        public static void IntroHandler(IntroCutscene __instance)
        {
            if (isMode(ModeId.HideAndSeek))
            {
                HideAndSeek.Intro.IntroHandler(__instance);
            }
            else if (isMode(ModeId.BattleRoyal))
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
            else if (isMode(ModeId.Werewolf))
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
            else if (isMode(ModeId.Werewolf))
            {
                Werewolf.Intro.YouAreHandle(__instance);
            }
        }
        public static void OptionLoad()
        {
            Mode = new CustomOptionBlank(null);
            ModeSetting = CustomOption.CustomOption.Create(484, true, CustomOptionType.Generic, "ModeSetting", false, Mode, isHeader: true);
            ThisModeSetting = CustomOption.CustomOption.Create(485, true, CustomOptionType.Generic, "SettingMode", modes, ModeSetting);
            HideAndSeek.ZombieOptions.Load();
            BattleRoyal.BROption.Load();
            Zombie.ZombieOptions.Load();
            RandomColor.RandomColorOptions.Load();
            Detective.DetectiveOptions.Load();
            Werewolf.WerewolfOptions.Load();
            //LevelUp.main.Load();

            PlusMode.Options.Load();
        }
        public static void HudUpdate(HudManager __instance)
        {
            switch (GetMode())
            {
                case ModeId.CopsRobbers:
                    CopsRobbers.main.HudUpdate();
                    break;
            }
        }
        public static void FixedUpdate(PlayerControl __instance)
        {
            if (isMode(ModeId.SuperHostRoles))
            {
                //PlayerControl.LocalPlayer.RpcSetName("<size=>次のターゲット:よッキング</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                SuperHostRoles.FixedUpdate.Update();
            }
            else if (isMode(ModeId.HideAndSeek))
            {
                HideAndSeek.Patch.HASFixed.Postfix(__instance);
            }
            else if (isMode(ModeId.BattleRoyal))
            {
                BattleRoyal.main.FixedUpdate();
            }
            else if (isMode(ModeId.Zombie))
            {
                Zombie.FixedUpdate.Update();
            }
            else if (isMode(ModeId.RandomColor))
            {
                RandomColor.FixedUpdate.Update();
            }
            else if (isMode(ModeId.CopsRobbers))
            {
            }
            /*else if (isMode(ModeId.LevelUp))
            {
                LevelUp.main.FixedUpdate();
            }*/
        }
        public static void Wrapup(GameData.PlayerInfo exiled)
        {
            if (isMode(ModeId.Default)) return;
            if (isMode(ModeId.Werewolf)) Werewolf.main.Wrapup(exiled); return;
        }
        public static ModeId GetMode(bool IsChache = true)
        {
            if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId)) return ModeId.Default;
            if (IsChache)
            {
                return thisMode;
            }
            if (isMode(ModeId.Default, false)) return ModeId.Default;
            if (isMode(ModeId.HideAndSeek, false)) return ModeId.HideAndSeek;
            if (isMode(ModeId.SuperHostRoles, false)) return ModeId.SuperHostRoles;
            if (isMode(ModeId.BattleRoyal, false)) return ModeId.BattleRoyal;
            if (isMode(ModeId.Zombie, false)) return ModeId.Zombie;
            if (isMode(ModeId.RandomColor, false)) return ModeId.RandomColor;
            if (isMode(ModeId.NotImpostorCheck, false)) return ModeId.NotImpostorCheck;
            if (isMode(ModeId.Detective, false)) return ModeId.Detective;
            if (isMode(ModeId.Werewolf, false)) return ModeId.Werewolf;
            if (isMode(ModeId.CopsRobbers, false)) return ModeId.CopsRobbers;
            if (isMode(ModeId.LevelUp, false)) return ModeId.LevelUp;
            return ModeId.No;
        }
        public static string GetThisModeIntro()
        {
            return ThisModeSetting.getString();
        }
        public static bool isMode(params ModeId[] modes)
        {
            if (AmongUsClient.Instance.GameMode == GameModes.FreePlay || (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId)))
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
                if (thisMode == mode)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool isMode(ModeId mode, bool IsChache = true)
        {
            if (AmongUsClient.Instance.GameMode == GameModes.FreePlay || !PlayerControlHepler.IsMod(AmongUsClient.Instance.HostId))
            {
                if (mode == ModeId.Default)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (IsChache)
            {
                return mode == thisMode;
            }
            return mode switch
            {
                ModeId.Default => !ModeSetting.getBool(),
                ModeId.HideAndSeek => ModeSetting.getBool() && ThisModeSetting.getString() == modes[0],
                ModeId.BattleRoyal => ModeSetting.getBool() && ThisModeSetting.getString() == modes[2],
                ModeId.SuperHostRoles => ModeSetting.getBool() && ThisModeSetting.getString() == modes[1],
                ModeId.Zombie => ModeSetting.getBool() && ThisModeSetting.getString() == modes[3],
                ModeId.RandomColor => ModeSetting.getBool() && ThisModeSetting.getString() == modes[4],
                ModeId.NotImpostorCheck => ModeSetting.getBool() && ThisModeSetting.getString() == modes[5],
                ModeId.Detective => ModeSetting.getBool() && ThisModeSetting.getString() == modes[6],
                ModeId.CopsRobbers => ModeSetting.getBool() && ThisModeSetting.getString() == modes[7],
                ModeId.Werewolf => false,//ModeSetting.getBool() && ThisModeSetting.getString() == modes[7];
                ModeId.LevelUp => false,//ModeSetting.getBool() && ThisModeSetting.getString() == modes[7];
                _ => false,
            };
        }
        public static bool EndGameChecks(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (isMode(ModeId.HideAndSeek))
            {
                return HideAndSeek.main.EndGameCheck(__instance, statistics);
            }
            else if (isMode(ModeId.BattleRoyal))
            {
                return BattleRoyal.main.EndGameCheck(__instance, statistics);
            }
            else if (isMode(ModeId.SuperHostRoles))
            {
                return SuperHostRoles.EndGameCheck.CheckEndGame(__instance, statistics);
            }
            else if (isMode(ModeId.Zombie))
            {
                return Zombie.main.EndGameCheck(__instance, statistics);
            }
            else if (isMode(ModeId.RandomColor))
            {
                return RandomColor.main.CheckEndGame(__instance, statistics);
            }
            else if (isMode(ModeId.NotImpostorCheck))
            {
                return NotImpostorCheck.WinCheck.CheckEndGame(__instance);
            }
            else if (isMode(ModeId.Detective))
            {
                return Detective.WinCheckPatch.CheckEndGame(__instance);
            }
            else if (isMode(ModeId.Werewolf))
            {
                return SuperHostRoles.EndGameCheck.CheckEndGame(__instance, statistics);
            }
            else if (isMode(ModeId.CopsRobbers))
            {
                return CopsRobbers.main.EndGameCheck(__instance);
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
