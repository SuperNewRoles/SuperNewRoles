using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Attribute;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Impostor;

class EvilSeer
{
    internal static class CustomOptionData
    {
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption Mode;
        public static CustomOption LimitSoulDuration;
        public static CustomOption SoulDuration;
        public static CustomOption IsUniqueSetting;
        public static CustomOption FlashColorMode;
        public static CustomOption IsFlashBodyColor;
        public static CustomOption IsReportingBodyColorName;
        public static CustomOption IsCrewSoulColor;
        public static CustomOption IsDeadBodyArrow;
        public static CustomOption IsArrowColorAdaptive;
        public static CustomOption CreateSetting;
        public static CustomOption CreateMode;
        public static CustomOption EvilSeerButtonCooldown;

        internal static void SetupCustomOptions()
        {
            Option = SetupCustomRoleOption(201900, true, RoleId.EvilSeer);
            PlayerCount = Create(201901, true, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], Option);
            Mode = Create(201902, false, CustomOptionType.Impostor, "SeerMode", new string[] { "SeerModeBoth", "SeerModeFlash", "SeerModeSouls" }, Option);
            LimitSoulDuration = Create(201903, false, CustomOptionType.Impostor, "SeerLimitSoulDuration", false, Option);
            SoulDuration = Create(201904, false, CustomOptionType.Impostor, "SeerSoulDuration", 15f, 0f, 120f, 5f, LimitSoulDuration, format: "unitCouples");
            IsUniqueSetting = Create(201906, false, CustomOptionType.Impostor, "EvilSeerIsUniqueSetting", true, Option);
            FlashColorMode = Create(201909, false, CustomOptionType.Impostor, "EvilSeerFlashColorMode", new string[] { "EvilSeerColorModeclear", "EvilSeerColorModeLightAndDark" }, IsUniqueSetting);
            IsFlashBodyColor = Create(201907, false, CustomOptionType.Impostor, "EvilSeerIsFlashColor", true, IsUniqueSetting);
            IsReportingBodyColorName = Create(201908, false, CustomOptionType.Impostor, "EvilSeerIsReportingBodyColorName", true, IsFlashBodyColor);
            IsCrewSoulColor = Create(201910, false, CustomOptionType.Impostor, "EvilSeerIsCrewSoulColor", true, IsUniqueSetting);
            IsDeadBodyArrow = Create(201911, false, CustomOptionType.Impostor, "VultureShowArrowsSetting", true, IsUniqueSetting);
            IsArrowColorAdaptive = Create(201912, false, CustomOptionType.Impostor, "EvilSeerIsArrowColorAdaptive", true, IsDeadBodyArrow);
            CreateSetting = Create(201905, true, CustomOptionType.Impostor, "EvilSeerCreateSetting", false, Option);
            CreateMode = Create(201913, false, CustomOptionType.Impostor, "EvilSeerCreateHauntedWolfMode", new string[] { "optionOff", "EvilSeerCreateHauntedWolfModeBoth", "EvilSeerCreateHauntedWolfModeAbilityOnry", "EvilSeerCreateHauntedWolfModePassiveOnry", "CreateMadmateSetting" }, CreateSetting);
            EvilSeerButtonCooldown = Create(201914, false, CustomOptionType.Impostor, "EvilSeerButtonCooldownSetting", 30f, 0f, 60f, 2.5f, CreateSetting);
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = ImpostorRed;
        public static List<(Vector3, int)> deadBodyPositions;

        public static float soulDuration;
        public static bool limitSoulDuration;
        public static int mode;
        public static bool IsUniqueSetting;
        public static bool IsClearColor;
        public static bool IsArrow;
        public static bool IsArrowColorAdaptive;
        public static bool CanCreate;
        public static Dictionary<byte, bool> CanCreateSHR;
        /// <summary>
        /// 狼憑き, マッドメイトを作成する能力のモードを取得する
        /// </summary>
        /// <value>0 : オフ, 1 : 取り憑かせる＆憑り付く, 2 : 取り憑かせる, 3 : 取り付く, 4 : マッドメイトを作れる</value>
        public static int CreateMode { get; private set; }
        public static float EvilSeerButtonCooldown;

        internal const int DefaultBodyColorId = (int)CustomCosmetics.CustomColors.ColorType.Crasyublue;
        internal const int LightBodyColorId = (int)CustomCosmetics.CustomColors.ColorType.Pitchwhite;
        internal const int DarkBodyColorId = (int)CustomCosmetics.CustomColors.ColorType.Crasyublue;

        public static Dictionary<DeadBody, ArrowAdaptive> DeadPlayerArrows;

        public static void ClearAndReload()
        {
            Player = new();
            deadBodyPositions = new();
            limitSoulDuration = CustomOptionData.LimitSoulDuration.GetBool();
            soulDuration = CustomOptionData.SoulDuration.GetFloat();
            mode = ModeHandler.IsMode(ModeId.SuperHostRoles) ? 1 : CustomOptionData.Mode.GetSelection();
            IsUniqueSetting = !ModeHandler.IsMode(ModeId.SuperHostRoles) && CustomOptionData.IsUniqueSetting.GetBool();
            IsClearColor = CustomOptionData.FlashColorMode.GetSelection() == 0;
            IsArrow = IsUniqueSetting && CustomOptionData.IsDeadBodyArrow.GetBool();
            IsArrowColorAdaptive = IsArrow && CustomOptionData.IsArrowColorAdaptive.GetBool();
            DeadPlayerArrows = new();
            EvilSeerButtonCooldown = CustomOptionData.EvilSeerButtonCooldown.GetFloat();

            CreateMode =
                !CustomOptionData.CreateSetting.GetBool()
                    ? 0 // 設定がオフなら, モードもオフにする
                    : ModeHandler.IsMode(ModeId.SuperHostRoles)
                        ? 3 // SHRなら[取り憑く]モードに固定する・
                        : CustomOptionData.CreateMode.GetSelection(); // 設定が有効で, SHR出ない時モード設定を取得する。
            CanCreate = CreateMode != 0; // 「狼憑き或いはマッドメイトを作成できる状態か?」の初期値
            CanCreateSHR = new();
        }
    }

    internal static class Button
    {
        private static CustomButton CreateHauntedWolfButton;
        private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CreateHauntedWolfButton.png", 115f);

        internal static void SetupCustomButtons(HudManager hm)
        {
            CreateHauntedWolfButton = new(
                () =>
                {
                    var target = HudManagerStartPatch.SetTarget();
                    if (target == null) return;
                    RoleData.CanCreate = false;

                    HauntedWolf.Assign.SetHauntedWolf(target);
                    HauntedWolf.Assign.SetHauntedWolfRPC(target);
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.EvilSeer && RoleData.CreateMode is not 0 and <= 2 && RoleData.CanCreate; },
                () =>
                {
                    var target = HudManagerStartPatch.SetTarget();
                    PlayerControlFixedUpdatePatch.SetPlayerOutline(target, RoleData.color);
                    return PlayerControl.LocalPlayer.CanMove && target;
                },
                () =>
                {
                    CreateHauntedWolfButton.MaxTimer = RoleData.EvilSeerButtonCooldown;
                    CreateHauntedWolfButton.Timer = RoleData.EvilSeerButtonCooldown;
                },
                GetButtonSprite(),
                new Vector3(-2f, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                49,
                () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("EvilSeerButtonName"),
                showButtonText = true
            };
        }
    }

    internal static class Ability
    {
        public static void DeadBodyArrowFixedUpdate()
        {
            if (!RoleData.IsArrow) return;

            foreach (var arrow in RoleData.DeadPlayerArrows)
            {
                bool isTarget = false;
                foreach (DeadBody dead in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                {
                    if (arrow.Key.ParentId != dead.ParentId) continue;
                    isTarget = true;
                    break;
                }
                if (isTarget)
                {
                    var deadPlayer = ModHelpers.GetPlayerControl(arrow.Key.ParentId);
                    var arrowColor = RoleData.DefaultBodyColorId;

                    if (RoleData.IsArrowColorAdaptive && RoleData.IsClearColor) arrowColor = deadPlayer.Data.DefaultOutfit.ColorId; // 最高が最高
                    else if (RoleData.IsArrowColorAdaptive) // 明暗
                    {
                        var isLight = CustomCosmetics.CustomColors.lighterColors.Contains(deadPlayer.Data.DefaultOutfit.ColorId);
                        arrowColor = isLight ? RoleData.LightBodyColorId : RoleData.DarkBodyColorId;
                    }

                    if (arrow.Value == null)
                    {
                        RoleData.DeadPlayerArrows[arrow.Key] = new(arrowColor);
                    }
                    arrow.Value.Update(arrow.Key.transform.position, arrowColor);
                    arrow.Value.arrow.SetActive(true);
                }
                else
                {
                    if (arrow.Value?.arrow != null)
                        UnityEngine.Object.Destroy(arrow.Value.arrow);
                    RoleData.DeadPlayerArrows.Remove(arrow.Key);
                }
            }
            foreach (DeadBody dead in UnityEngine.Object.FindObjectsOfType<DeadBody>())
            {
                if (RoleData.DeadPlayerArrows.Any(x => x.Key.ParentId == dead.ParentId)) continue;

                var deadPlayer = ModHelpers.GetPlayerControl(dead.ParentId);
                var arrowColor = RoleData.IsArrowColorAdaptive ? deadPlayer.Data.DefaultOutfit.ColorId : RoleData.DefaultBodyColorId;

                RoleData.DeadPlayerArrows.Add(dead, new(arrowColor));
                RoleData.DeadPlayerArrows[dead].Update(dead.transform.position, arrowColor);
                RoleData.DeadPlayerArrows[dead].arrow.SetActive(true);
            }
        }

        internal static class OnKill
        {
            /// <summary>
            /// 通常モードでの[取り付く]能力の処理
            /// </summary>
            /// <param name="killer">自身をキルしたプレイヤー</param>
            internal static void DefaultMode(PlayerControl killer)
            {
                if (RoleData.CreateMode is not (1 or 3)) return;
                if (!RoleData.CanCreate || killer == PlayerControl.LocalPlayer || killer == null) return;

                RoleData.CanCreate = false;
                HauntedWolf.Assign.SetHauntedWolf(killer);
                HauntedWolf.Assign.SetHauntedWolfRPC(killer);
            }

            /// <summary>
            /// SHRモードでの[取り付く]能力の処理
            /// </summary>
            /// <param name="killer">自身をキルしたプレイヤー</param>
            internal static void SuperHostRolesMode(PlayerControl killer, PlayerControl victim)
            {
                if (!AmongUsClient.Instance.AmHost) return;
                if (RoleData.CreateMode is not (1 or 3)) return;
                if (killer == victim || killer == null) return;

                var killerId = killer.PlayerId;
                if (RoleData.CanCreateSHR.ContainsKey(killerId)) return;

                RoleData.CanCreateSHR.Add(killerId, false);
                HauntedWolf.Assign.SetHauntedWolf(killer);
                HauntedWolf.Assign.SetHauntedWolfRPC(killer);
            }
        }
    }
}