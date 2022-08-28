using HarmonyLib;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Patch;
using static SuperNewRoles.EndGame.CheckGameEndPatch;

namespace SuperNewRoles.Mode
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
    class CloseDoorsPatch
    {
        public static bool Prefix(ShipStatus __instance)
        {
            return !ModeHandler.IsMode(ModeId.Zombie) && !ModeHandler.IsMode(ModeId.Werewolf) &&
                    !ModeHandler.IsMode(ModeId.BattleRoyal) && !ModeHandler.IsMode(ModeId.HideAndSeek) &&
                    !ModeHandler.IsMode(ModeId.CopsRobbers);
        }
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
        LevelUp
    }
    public class ModeHandler
    {
        public static ModeId thisMode;
        public static void ClearAndReload()
        {
            PlusModeHandler.ClearAndReload();
            if (IsMode(ModeId.HideAndSeek, false))
            {
                thisMode = ModeId.HideAndSeek;
                HideAndSeek.Main.ClearAndReload();
            }
            else if (IsMode(ModeId.BattleRoyal, false))
            {
                thisMode = ModeId.BattleRoyal;
                BattleRoyal.Main.ClearAndReload();
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
            else if (IsMode(ModeId.Werewolf, false))
            {
                thisMode = ModeId.Werewolf;
                Werewolf.Main.ClearAndReload();
            }
            else if (IsMode(ModeId.CopsRobbers, false))
            {
                thisMode = ModeId.CopsRobbers;
                CopsRobbers.Main.ClearAndReloads();
            }
            /* else if (IsMode(ModeId.LevelUp, false))
            {
                thisMode = ModeId.LevelUp;
                LevelUp.main.ClearAndReloads();
            }*/
            else
            {
                thisMode = ModeId.Default;
            }
            if (!IsMode(ModeId.Default))
            {
                SuperHostRoles.BlockTool.IsCom = false;
            }
        }
        public static string[] modes = new string[] { ModTranslation.GetString("HideAndSeekModeName"), ModTranslation.GetString("SuperHostRolesModeName"), ModTranslation.GetString("BattleRoyalModeName"), ModTranslation.GetString("ZombieModeName"), ModTranslation.GetString("RandomColorModeName"), ModTranslation.GetString("NotImpostorCheckModeName"), ModTranslation.GetString("DetectiveModeName"), ModTranslation.GetString("CopsRobbersModeName") };//ModTranslation.GetString("WerewolfModeName") };

        public const string PlayingOnSuperNewRoles = "Playing on <color=#ffa500>Super</color><color=#ff0000>New</color><color=#00ff00>Roles</color>";

        public static CustomOptionBlank Mode;
        public static CustomOption.CustomOption ModeSetting;
        public static CustomOption.CustomOption ThisModeSetting;
        public static Il2CppSystem.Collections.Generic.List<PlayerControl> TeamHandler(IntroCutscene __instance)
        {
            if (IsMode(ModeId.HideAndSeek)) return HideAndSeek.Intro.ModeHandler();
            else if (IsMode(ModeId.BattleRoyal)) return BattleRoyal.Intro.ModeHandler();
            else if (IsMode(ModeId.SuperHostRoles)) return SuperHostRoles.Intro.ModeHandler(__instance);
            else if (IsMode(ModeId.Zombie)) return Zombie.Intro.ModeHandler();
            else if (IsMode(ModeId.RandomColor)) return SuperHostRoles.Intro.ModeHandler(__instance);
            else if (IsMode(ModeId.NotImpostorCheck)) return SuperHostRoles.Intro.ModeHandler(__instance);
            else if (IsMode(ModeId.Detective)) return SuperHostRoles.Intro.ModeHandler(__instance);
            else if (IsMode(ModeId.Werewolf))
            {
                var Data = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                Data.Add(PlayerControl.LocalPlayer);
                return Data;
            }
            else if (IsMode(ModeId.CopsRobbers))
            {
                var Data = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                Data.Add(PlayerControl.LocalPlayer);
                return Data;
            }
            return new Il2CppSystem.Collections.Generic.List<PlayerControl>();
        }
        public static void IntroHandler(IntroCutscene __instance)
        {
            if (IsMode(ModeId.HideAndSeek)) HideAndSeek.Intro.IntroHandler(__instance);
            else if (IsMode(ModeId.BattleRoyal)) BattleRoyal.Intro.IntroHandler(__instance);
            else if (IsMode(ModeId.SuperHostRoles)) SuperHostRoles.Intro.IntroHandler();
            else if (IsMode(ModeId.Zombie)) Zombie.Intro.IntroHandler(__instance);
            else if (IsMode(ModeId.Detective)) SuperHostRoles.Intro.IntroHandler();
            else if (IsMode(ModeId.Werewolf)) SuperHostRoles.Intro.IntroHandler();
        }
        public static void YouAreIntroHandler(IntroCutscene __instance)
        {
            if (IsMode(ModeId.Zombie)) Zombie.Intro.YouAreHandle(__instance);
            else if (IsMode(ModeId.Detective)) Detective.Intro.YouAreHandle(__instance);
            else if (IsMode(ModeId.Werewolf)) Werewolf.Intro.YouAreHandle(__instance);
        }
        public static void OptionLoad()
        {
            Mode = new CustomOptionBlank(null);
            ModeSetting = CustomOption.CustomOption.Create(484, true, CustomOptionType.Generic, "ModeSetting", false, Mode, isHeader: true);
            ThisModeSetting = CustomOption.CustomOption.Create(485, true, CustomOptionType.Generic, "SettingMode", modes, ModeSetting);
            HideAndSeek.HideAndSeekOptions.Load();
            BattleRoyal.BROption.Load();
            Zombie.ZombieOptions.Load();
            RandomColor.RandomColorOptions.Load();
            Detective.DetectiveOptions.Load();
            CopsRobbers.CopsRobbersOptions.Load();
            //Werewolf.WerewolfOptions.Load();
            //LevelUp.main.Load();

            PlusMode.Options.Load();
        }
        public static void HudUpdate(HudManager __instance)
        {
            switch (GetMode())
            {
                case ModeId.CopsRobbers:
                    CopsRobbers.Main.HudUpdate();
                    break;
            }
        }
        public static void FixedUpdate(PlayerControl __instance)
        {
            if (IsMode(ModeId.SuperHostRoles))
                //PlayerControl.LocalPlayer.RpcSetName("<size=>次のターゲット:よッキング</size>\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n");
                SuperHostRoles.FixedUpdate.Update();
            else if (IsMode(ModeId.HideAndSeek)) HideAndSeek.Patch.HASFixed.Postfix();
            else if (IsMode(ModeId.BattleRoyal)) BattleRoyal.Main.FixedUpdate();
            else if (IsMode(ModeId.Zombie)) Zombie.FixedUpdate.Update();
            else if (IsMode(ModeId.RandomColor)) RandomColor.FixedUpdate.Update();
            //else if (IsMode(ModeId.LevelUp)) LevelUp.main.FixedUpdate();

        }
        public static void Wrapup(GameData.PlayerInfo exiled)
        {
            if (IsMode(ModeId.Default)) return;
            if (IsMode(ModeId.Werewolf)) Werewolf.Main.Wrapup(exiled); return;
        }
        public static ModeId GetMode(bool IsChache = true)
        {
            if (!ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers.ContainsKey(AmongUsClient.Instance.HostId)) return ModeId.Default;
            if (IsChache)
            {
                return thisMode;
            }
            if (IsMode(ModeId.Default, false)) return ModeId.Default;
            if (IsMode(ModeId.HideAndSeek, false)) return ModeId.HideAndSeek;
            if (IsMode(ModeId.SuperHostRoles, false)) return ModeId.SuperHostRoles;
            if (IsMode(ModeId.BattleRoyal, false)) return ModeId.BattleRoyal;
            if (IsMode(ModeId.Zombie, false)) return ModeId.Zombie;
            return IsMode(ModeId.RandomColor, false)
                ? ModeId.RandomColor
                : IsMode(ModeId.NotImpostorCheck, false)
                ? ModeId.NotImpostorCheck
                : IsMode(ModeId.Detective, false)
                ? ModeId.Detective
                : IsMode(ModeId.Werewolf, false)
                ? ModeId.Werewolf
                : IsMode(ModeId.CopsRobbers, false) ? ModeId.CopsRobbers : IsMode(ModeId.LevelUp, false) ? ModeId.LevelUp : ModeId.No;
        }
        public static string GetThisModeIntro()
        {
            return ThisModeSetting.GetString();
        }
        public static bool IsMode(params ModeId[] modes)
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
        public static bool IsMode(ModeId mode, bool IsChache = true)
        {
            return AmongUsClient.Instance.GameMode == GameModes.FreePlay || !PlayerControlHepler.IsMod(AmongUsClient.Instance.HostId)
                ? mode == ModeId.Default
                : IsChache
                ? mode == thisMode
                : mode switch
                {
                    ModeId.Default => !ModeSetting.GetBool(),
                    ModeId.HideAndSeek => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[0],
                    ModeId.BattleRoyal => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[2],
                    ModeId.SuperHostRoles => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[1],
                    ModeId.Zombie => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[3],
                    ModeId.RandomColor => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[4],
                    ModeId.NotImpostorCheck => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[5],
                    ModeId.Detective => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[6],
                    ModeId.CopsRobbers => ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[7],
                    ModeId.Werewolf => false,//ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[7];
                    ModeId.LevelUp => false,//ModeSetting.GetBool() && ThisModeSetting.GetString() == modes[7];
                    _ => false,
                };
        }
        public static bool EndGameChecks(ShipStatus __instance, PlayerStatistics statistics)
        {
            if (IsMode(ModeId.HideAndSeek)) return HideAndSeek.Main.EndGameCheck(__instance, statistics);
            else if (IsMode(ModeId.BattleRoyal)) return BattleRoyal.Main.EndGameCheck(__instance);
            else if (IsMode(ModeId.SuperHostRoles)) return SuperHostRoles.EndGameCheck.CheckEndGame(__instance, statistics);
            else if (IsMode(ModeId.Zombie)) return Zombie.Main.EndGameCheck(__instance, statistics);
            else if (IsMode(ModeId.RandomColor)) return RandomColor.Main.CheckEndGame(__instance, statistics);
            else if (IsMode(ModeId.NotImpostorCheck)) return NotImpostorCheck.WinCheck.CheckEndGame(__instance);
            else if (IsMode(ModeId.Detective)) return Detective.WinCheckPatch.CheckEndGame(__instance);
            else if (IsMode(ModeId.Werewolf)) return SuperHostRoles.EndGameCheck.CheckEndGame(__instance, statistics);
            else if (IsMode(ModeId.CopsRobbers)) return CopsRobbers.Main.EndGameCheck(__instance);
            return false;
        }
        public static bool IsBlockVanilaRole()
        {
            return IsMode(ModeId.NotImpostorCheck) ? false : !IsMode(ModeId.Detective) && !IsMode(ModeId.Default);
        }
        public static bool IsBlockGuardianAngelRole()
        {
            return IsMode(ModeId.Default) || IsBlockVanilaRole();
        }
    }
}