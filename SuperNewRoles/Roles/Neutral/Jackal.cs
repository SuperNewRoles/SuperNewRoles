using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;
using static SuperNewRoles.Helpers.RPCHelper;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles;

public class Jackal : RoleBase<Jackal>
{
    public static Color color = RoleClass.JackalBlue;

    public Jackal()
    {
        RoleId = roleId = RoleId.Jackal;
        //以下いるもののみ変更
        HasTask = false;
        IsKiller = true;
        OptionId = 58;
        IsSHRRole = true;
        OptionType = CustomOptionType.Neutral;
        CanUseVentOptionOn = true;
        CanUseVentOptionDefault = true;
        CanUseSaboOptionOn = true;
        IsImpostorViewOptionOn = true;
        CoolTimeOptionOn = true;
        DurationTimeOptionOn = true;
    }

    public override void OnMeetingStart() { }
    public override void OnWrapUp() { }
    public override void FixedUpdate()
    {
        if (AmongUsClient.Instance.AmHost)
        {
            if (Sidekick.allPlayers.Count > 0)
            {
                var upflag = true;
                foreach (PlayerControl p in Jackal.allPlayers)
                {
                    if (p.IsAlive())
                    {
                        upflag = false;
                    }
                }
                if (upflag)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SidekickPromotes, SendOption.Reliable, -1);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.SidekickPromotes(false);
                }
            }
        }
    }
    public override void MeFixedUpdateAlive()
    {
        JackalPlayerOutLineTarget();
    }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { FixedUpdate(); }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { FixedUpdate(); }
    public override void EndUseAbility() { }
    public override void ResetRole() { }
    public override void PostInit() { CanCreateFriend = JackalCreateFriend.GetBool(); CanCreateSidekick = !CanCreateFriend && JackalCreateSidekick.GetBool(); }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    public bool CanCreateSidekick;
    public bool CanCreateFriend;

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static void MakeButtons(HudManager hm) { }
    public static void SetButtonCooldowns() { }

    public static List<PlayerControl> FakeSidekickPlayer;

    public static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.JackalSidekickButton.png", 115f);

    public static CustomOption JackalKillCooldown;
    public static CustomOption JackalCreateFriend;
    public static CustomOption JackalCreateSidekick;
    public static CustomOption JackalNewJackalCreateSidekick;

    public override void SetupMyOptions() {
        JackalKillCooldown = CustomOption.Create(60, true, CustomOptionType.Neutral, "JackalCooldownSetting", 30f, 2.5f, 60f, 2.5f, RoleOption, format: "unitSeconds");
        JackalCreateFriend = CustomOption.Create(666, true, CustomOptionType.Neutral, "JackalCreateFriendSetting", false, RoleOption);
        JackalCreateSidekick = CustomOption.Create(64, false, CustomOptionType.Neutral, "JackalCreateSidekickSetting", false, RoleOption);
        JackalNewJackalCreateSidekick = CustomOption.Create(65, false, CustomOptionType.Neutral, "JackalNewJackalCreateSidekickSetting", false, JackalCreateSidekick);
    }

    public static void Clear()
    {
        players = new();
    }


    public static void ResetCooldown()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = JackalKillCooldown.GetFloat();
        HudManagerStartPatch.JackalKillButton.Timer = HudManagerStartPatch.JackalKillButton.MaxTimer;
    }
    public static void EndMeetingResetCooldown()
    {
        HudManagerStartPatch.JackalKillButton.MaxTimer = JackalKillCooldown.GetFloat();
        HudManagerStartPatch.JackalKillButton.Timer = JackalKillCooldown.GetFloat();
        HudManagerStartPatch.JackalSidekickButton.MaxTimer = Jackal.CoolTimeS;
        HudManagerStartPatch.JackalSidekickButton.Timer = HudManagerStartPatch.JackalSidekickButton.MaxTimer;
    }
    public static void EndMeeting() => EndMeetingResetCooldown();
    public static void SetPlayerOutline(PlayerControl target, Color color)
    {
        if (target == null) return;
        SpriteRenderer rend = target.MyRend();
        if (rend == null) return;
        rend.material.SetFloat("_Outline", 1f);
        rend.material.SetColor("_OutlineColor", color);
    }
        public static void JackalPlayerOutLineTarget()
            => SetPlayerOutline(JackalSetTarget(), Jackal.color);
    /// <summary>
    /// (役職をリセットし、)ジャッカルフレンズに割り当てます。
    /// </summary>
    /// <param name="target">役職がJackalFriendsに変更される対象</param>
    public static void CreateJackalFriends(PlayerControl target)
    {
        target.ResetAndSetRole(RoleId.JackalFriends);
    }
}