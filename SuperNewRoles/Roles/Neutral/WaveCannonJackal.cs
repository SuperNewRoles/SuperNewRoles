using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles.Neutral;

class WaveCannonJackal
{
    // CustomOption Start
    private static int OptionId = 1252;
    public static CustomRoleOption WaveCannonJackalOption;
    public static CustomOption WaveCannonJackalPlayerCount;
    public static CustomOption WaveCannonJackalCoolTime;
    public static CustomOption WaveCannonJackalChargeTime;
    public static CustomOption WaveCannonJackalKillCooldown;
    public static CustomOption WaveCannonJackalUseVent;
    public static CustomOption WaveCannonJackalUseSabo;
    public static CustomOption WaveCannonJackalIsImpostorLight;
    public static CustomOption WaveCannonJackalIsSyncKillCoolTime;
    public static CustomOption WaveCannonJackalCreateSidekick;
    public static CustomOption WaveCannonJackalCreateFriend;
    public static CustomOption WaveCannonJackalSKCooldown;
    public static CustomOption WaveCannonJackalNewJackalCreateSidekick;
    public static CustomOption WaveCannonJackalNewJackalHaveWaveCannon;
    public static void SetupCustomOptions()
    {
        WaveCannonJackalOption = SetupCustomRoleOption(OptionId, false, RoleId.WaveCannonJackal, CustomOptionType.Neutral); OptionId++;
        WaveCannonJackalPlayerCount = Create(OptionId, false, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], WaveCannonJackalOption); OptionId++;
        WaveCannonJackalCoolTime = Create(OptionId, false, CustomOptionType.Neutral, "NiceScientistCooldownSetting", 20f, 2.5f, 180f, 2.5f, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalChargeTime = Create(OptionId, false, CustomOptionType.Neutral, "WaveCannonChargeTime", 3f, 0.5f, 15f, 0.5f, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalKillCooldown = Create(OptionId, false, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, WaveCannonJackalOption, format: "unitSeconds"); OptionId++;
        WaveCannonJackalUseVent = Create(OptionId, false, CustomOptionType.Neutral, "JackalUseVentSetting", true, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalIsImpostorLight = Create(OptionId, false, CustomOptionType.Neutral, "MadmateImpostorLightSetting", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalUseSabo = Create(OptionId, false, CustomOptionType.Neutral, "JackalUseSaboSetting", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalIsSyncKillCoolTime = Create(OptionId, false, CustomOptionType.Neutral, "IsSyncKillCoolTime", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalCreateSidekick = Create(OptionId, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalSKCooldown = Create(OptionId, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, WaveCannonJackalCreateSidekick, format: "unitSeconds"); OptionId++;
        WaveCannonJackalNewJackalCreateSidekick = Create(OptionId, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, WaveCannonJackalCreateSidekick); OptionId++;
        WaveCannonJackalNewJackalHaveWaveCannon = Create(OptionId, false, CustomOptionType.Neutral, "WaveCannonJackalNewJackalHaveWaveCannon", false, WaveCannonJackalCreateSidekick); OptionId++;
        WaveCannonJackalCreateFriend = Create(OptionId, false, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, WaveCannonJackalOption);
    }
    // CustomOption End

    // RoleClass Start
    public static List<PlayerControl> WaveCannonJackalPlayer;
    public static List<PlayerControl> FakeSidekickWaveCannonPlayer;
    public static List<PlayerControl> SidekickWaveCannonPlayer;
    public static Color32 color = RoleClass.JackalBlue;
    public static List<int> CreatePlayers;
    public static bool CanCreateSidekick;
    public static bool CanCreateFriend;
    public static List<byte> IwasSidekicked;
    public static void ClearAndReload()
    {
        WaveCannonJackalPlayer = new();
        FakeSidekickWaveCannonPlayer = new();
        SidekickWaveCannonPlayer = new();
        CreatePlayers = new();
        CanCreateSidekick = WaveCannonJackalCreateSidekick.GetBool();
        CanCreateFriend = WaveCannonJackalCreateFriend.GetBool();
        IwasSidekicked = new();
    }
    // RoleClass End

    // Button Start
    public static CustomButton WaveCannonJackalSidekickButton;

    public static void MakeButtons(HudManager hm)
    {
        WaveCannonJackalSidekickButton = new(
            () =>
            {
                var target = JackalSetTarget();
                if (target && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && CanCreateSidekick)
                {
                    if (target.IsRole(RoleId.SideKiller)) // サイドキック相手がマッドキラーの場合
                    {
                        if (!RoleClass.SideKiller.IsUpMadKiller) // サイドキラーが未昇格の場合
                        {
                            var sidePlayer = RoleClass.SideKiller.GetSidePlayer(target); // targetのサイドキラーを取得
                            if (sidePlayer != null) // null(作っていない)ならば処理しない
                            {
                                sidePlayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                RoleClass.SideKiller.IsUpMadKiller = true;
                            }
                        }
                    }
                    if (CanCreateFriend)
                    {
                        Jackal.CreateJackalFriends(target); //クルーにして フレンズにする
                    }
                    else
                    {
                        bool IsFakeWaveCannonJackal = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.WaveCannonJackalSidekick, target);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CreateSidekickWaveCannon, SendOption.Reliable, -1);
                        killWriter.Write(target.PlayerId);
                        killWriter.Write(IsFakeWaveCannonJackal);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.CreateSidekickWaveCannon(target.PlayerId, IsFakeWaveCannonJackal);
                    }
                    CanCreateSidekick = false;
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role is RoleId.WaveCannonJackal && ModeHandler.IsMode(ModeId.Default) && CanCreateSidekick; },
            () =>
            {
                return JackalSetTarget() && PlayerControl.LocalPlayer.CanMove;
            },
            () => { EndMeeting(); },
            RoleClass.Jackal.GetButtonSprite(),
            new Vector3(-2.925f, -0.06f, 0),
            hm,
            hm.AbilityButton,
            null,
            0,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("JackalCreateSidekickButtonName"),
            showButtonText = true
        };
    }
    public static void ResetCooldowns()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = WaveCannonJackalKillCooldown.GetFloat();
        HudManagerStartPatch.JackalKillButton.Timer = HudManagerStartPatch.JackalKillButton.MaxTimer;
    }

    public static void EndMeetingResetCooldown()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = WaveCannonJackalKillCooldown.GetFloat();
        HudManagerStartPatch.JackalKillButton.Timer = HudManagerStartPatch.JackalKillButton.MaxTimer;
        WaveCannonJackalSidekickButton.MaxTimer = WaveCannonJackalSKCooldown.GetFloat();
        WaveCannonJackalSidekickButton.Timer = WaveCannonJackalSidekickButton.MaxTimer;
    }
    public static void EndMeeting() => EndMeetingResetCooldown();

    // Button Start

    public class WaveCannonJackalFixedPatch
    {
        public static void WaveCannonJackalPlayerOutLineTarget()
        {
            JackalSeer.SetPlayerOutline(JackalSetTarget(), color);
        }
        /// <summary>
        /// SK昇格処理を行うかの判定、追放処理時・キル発生時に判定される
        /// </summary>
        /// <param name="role">死亡者の役職</param>
        public static void Postfix(PlayerControl __instance, RoleId role)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (WaveCannonJackal.SidekickWaveCannonPlayer.Count > 0)
                {
                    var upflag = true;
                    foreach (PlayerControl p in WaveCannonJackalPlayer)
                    {
                        if (p.IsAlive())
                        {
                            upflag = false;
                        }
                    }
                    if (upflag)
                    {
                        byte jackalId = (byte)RoleId.WaveCannonJackal;
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, SendOption.Reliable, -1);
                        writer.Write(jackalId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.SidekickPromotes(jackalId);
                    }
                }
            }
            if (role == RoleId.WaveCannonJackal)
            {
                WaveCannonJackalPlayerOutLineTarget();
            }
        }
    }
}
