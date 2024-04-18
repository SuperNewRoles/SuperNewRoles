using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.WaveCannonObj;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOption;
using static SuperNewRoles.Modules.CustomOptionHolder;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;
using static SuperNewRoles.WaveCannonObj.WaveCannonObject;

namespace SuperNewRoles.Roles.Neutral;

public enum WCJackalSidekickType
{
    Sidekick,
    JackalFriends,
    WaveCannonSidekick,
    BulletSidekick,
}
public class WaveCannonJackal : RoleBase
{
    public static new RoleInfo Roleinfo = new(
        typeof(WaveCannonJackal),
        (p) => new WaveCannonJackal(p),
        RoleId.WaveCannonJackal,
        "WaveCannonJackal",
        RoleClass.JackalBlue,
        new(RoleId.WaveCannonJackal, TeamTag.Jackal,
            RoleTag.Killer, RoleTag.CanSidekick, RoleTag.SpecialKiller),
        TeamRoleType.Neutral,
        TeamType.Neutral
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.WaveCannonJackal, 300120, false,
            CoolTimeOption: (20f, 2.5f, 180, 2.5f, false),
            VentOption: (true, false),
            SaboOption: (false, false),
            ImpostorVisionOption: (true, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.WaveCannonJackal, introSound: RoleTypes.Shapeshifter);

    public static CustomOption ChargeTime;
    public static CustomOption KillCooldown;
    public static CustomOption IsSyncKillCoolTime;
    /*サイドキックオプションが煩雑になってきたので一回整理します

    サイドキックを出来る
        初期値:OFF (ON/OFF)

以下は「サイドキックを出来る」が有効なときのみ出現

    サイドキックのクールタイム
        初期値:30秒, 最低値:2.5, 最大値:60, 間隔:2.5
    サイドキックのタイプ　サイドキック系オプションが増えたので設定をリスト系に変更してほしい
        初期値:通常, 通常/ジャッカルフレンズ/波動砲/弾

以下はサイドキックのタイプが「通常or弾」のときのみ出現

    サイドキックがジャッカルに昇格出来る
        初期値:ON (ON/OFF)

以下はサイドキックのタイプが「弾」かつ「サイドキックがジャッカルに昇格出来る」がONのときのみ出現

    昇格時、波動砲を引き継ぐ
        初期値:ON (ON/OFF)

以下はサイドキックのタイプが「弾」のときのみ出現

    「装填」のクールタイム
        初期値:30秒, 最低値:0, 最大値:60, 間隔:2.5
    「装填」時の波動砲のチャージ時間
        初期値:10秒, 最低値:1, 最大値:30, 間隔:1
*/
    // サイドキック系の設定
    public static CustomOption CanCreateSidekick;
    public static CustomOption CanCreateSidekickNewByNewJackal;
    public static CustomOption CreateSidekickCoolTime;
    public static CustomOption CreateSidekickType;

    // 弾の設定
    public static CustomOption CreateBulletToJackal;
    public static CustomOption CreatedSidekickHasWaveCannon;

    // 弾装填設定
    public static CustomOption BulletLoadBulletCooltime;
    public static CustomOption BulletLoadedChargeTime;


    public static CustomOption AnimationOptionType;

    private static void CreateOption()
    {
        ChargeTime = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "WaveCannonChargeTime", 3f, 0.5f, 15f, 0.5f, Optioninfo.RoleOption); Optioninfo.OptionId++;
        KillCooldown = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption, format: "unitSeconds"); Optioninfo.OptionId++;
        IsSyncKillCoolTime = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "IsSyncKillCoolTime", false, Optioninfo.RoleOption); Optioninfo.OptionId++;
        // サイドキック系の設定
        CanCreateSidekick = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, Optioninfo.RoleOption); Optioninfo.OptionId++;
        CanCreateSidekickNewByNewJackal = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, CanCreateSidekick); Optioninfo.OptionId++;
        CreateSidekickCoolTime = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, CanCreateSidekick, format: "unitSeconds"); Optioninfo.OptionId++;

        List<string> SidekickTypeTexts = [];
        foreach (WCJackalSidekickType type in System.Enum.GetValues(typeof(WCJackalSidekickType)))
        {
            SidekickTypeTexts.Add(ModTranslation.GetString("WaveCannonJackalSidekickType" + type));
        }
        CreateSidekickType = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "WaveCannonJackalSidekickType", SidekickTypeTexts.ToArray(), CanCreateSidekick); Optioninfo.OptionId++;

        // Bullet
        CreateBulletToJackal = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "WaveCannonJackalCreateBulletToJackal", false, CreateSidekickType, ); Optioninfo.OptionId++;
        CreatedSidekickHasWaveCannon = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "WaveCannonJackalNewJackalHaveWaveCannon", false, CreateSidekickType); Optioninfo.OptionId++;

        BulletLoadBulletCooltime = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "WaveCannonJackalLoadBulletCoolTime", 30f, 2.5f, 60f, 2.5f, CreateSidekickType, format: "unitSeconds"); Optioninfo.OptionId++;
        BulletLoadedChargeTime = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "WaveCannonJackalBulletLoadedChargeTime", 10f, 1f, 30f, 1f, CreateSidekickType); Optioninfo.OptionId++;
        
        // AnimTypes
        string[] AnimTypeTexts = new string[WCCreateAnimHandlers.Count];
        int index = 0;
        foreach (string TypeName in WCCreateAnimHandlers.Keys)
        {
            if (TypeName == WCAnimType.None.ToString())
                break;
            AnimTypeTexts[index] = ModTranslation.GetString("WaveCannonAnimType" + TypeName);
            index++;
        }
        AnimationOptionType = Create(Optioninfo.OptionId, false, CustomOptionType.Neutral, "WaveCannonAnimationType", AnimTypeTexts, Optioninfo.RoleOption); Optioninfo.OptionId++;
    }
    public WaveCannonJackal(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }
}

class WaveCannonJackal
{
    // CustomOption Start
    private static int OptionId = 300100;
    public static CustomOption WaveCannonJackalChargeTime;
    public static CustomOption WaveCannonJackalKillCooldown;
    public static CustomOption WaveCannonJackalIsSyncKillCoolTime;
    public static CustomOption WaveCannonJackalCreateSidekick;
    public static CustomOption WaveCannonJackalCreateFriend;
    public static CustomOption WaveCannonJackalSKCooldown;
    public static CustomOption WaveCannonJackalNewJackalCreateSidekick;
    public static CustomOption WaveCannonJackalNewJackalHaveWaveCannon;
    public static void SetupCustomOptions()
    {
        WaveCannonJackalChargeTime = Create(OptionId, false, CustomOptionType.Neutral, "WaveCannonChargeTime", 3f, 0.5f, 15f, 0.5f, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalKillCooldown = Create(OptionId, false, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, WaveCannonJackalOption, format: "unitSeconds"); OptionId++;
        WaveCannonJackalIsSyncKillCoolTime = Create(OptionId, false, CustomOptionType.Neutral, "IsSyncKillCoolTime", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalCreateSidekick = Create(OptionId, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, WaveCannonJackalOption); OptionId++;
        WaveCannonJackalSKCooldown = Create(OptionId, false, CustomOptionType.Neutral, "PavlovsownerCreateDogCoolTime", 30f, 2.5f, 60f, 2.5f, WaveCannonJackalCreateSidekick, format: "unitSeconds"); OptionId++;
        WaveCannonJackalNewJackalCreateSidekick = Create(OptionId, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, WaveCannonJackalCreateSidekick); OptionId++;
        WaveCannonJackalNewJackalHaveWaveCannon = Create(OptionId, false, CustomOptionType.Neutral, "WaveCannonJackalNewJackalHaveWaveCannon", false, WaveCannonJackalCreateSidekick); OptionId++;
        WaveCannonJackalCreateFriend = Create(OptionId, false, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, WaveCannonJackalOption); OptionId++;
        
        WaveCannonJackalAnimTypeOption = Create(OptionId, false, CustomOptionType.Neutral, "WaveCannonAnimationType", AnimTypeTexts, WaveCannonJackalOption);
    }
    // CustomOption End

    // RoleClass Start
    public static List<PlayerControl> WaveCannonJackalPlayer;
    public static List<PlayerControl> FakeSidekickWaveCannonPlayer;
    public static List<PlayerControl> SidekickWaveCannonPlayer;
    public static Color32 color = RoleClass.JackalBlue;
    public static List<int> CreatePlayers;
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
                            if (sidePlayer != null && sidePlayer.IsAlive()) // null(作っていない)ならば処理しない
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
    public static void WCResetCooldowns()
    {
        HudManagerStartPatch.WaveCannonButton.MaxTimer = WaveCannonJackalCoolTime.GetFloat();
        HudManagerStartPatch.WaveCannonButton.Timer = HudManagerStartPatch.WaveCannonButton.MaxTimer;
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