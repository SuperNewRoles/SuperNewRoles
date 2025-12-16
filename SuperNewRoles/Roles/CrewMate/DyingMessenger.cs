using System;
using System.Collections.Generic;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

class DyingMessenger : RoleBase<DyingMessenger>
{
    public override RoleId Role { get; } = RoleId.DyingMessenger;
    public override Color32 RoleColor { get; } = new(191, 197, 202, 255);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new DyingMessengerReportAbility(
            DyingMessengerGetRoleTime,
            DyingMessengerGetLightAndDarkerTime
        )
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionInt(nameof(DyingMessengerGetRoleTime), 0, 60, 1, 20, translationName: "DyingMessengerGetRoleTimeSetting", suffix: "Seconds")]
    public static int DyingMessengerGetRoleTime;

    [CustomOptionInt(nameof(DyingMessengerGetLightAndDarkerTime), 0, 60, 1, 2, translationName: "DyingMessengerGetLightAndDarkerTimeSetting", suffix: "Seconds")]
    public static int DyingMessengerGetLightAndDarkerTime;
}

public sealed class DyingMessengerReportAbility : AbilityBase
{
    private EventListener<ReportDeadBodyHostEventData> _reportListener;

    public int GetRoleTime { get; }
    public int GetLightAndDarkerTime { get; }

    public DyingMessengerReportAbility(int getRoleTime, int getLightAndDarkerTime)
    {
        GetRoleTime = getRoleTime;
        GetLightAndDarkerTime = getLightAndDarkerTime;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _reportListener = ReportDeadBodyHostEvent.Instance.AddListener(OnReportHost);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _reportListener?.RemoveListener();
    }

    // ホストが実行する
    private void OnReportHost(ReportDeadBodyHostEventData data)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (data?.reporter == null || data.target == null) return;

        // ここで役職を持っている人のみに制限している
        if (data.reporter.PlayerId != Player.PlayerId) return;

        ExPlayerControl victim = ExPlayerControl.ById(data.target.PlayerId);
        if (victim == null) return;
        if (!MurderDataManager.TryGetMurderData(victim, out var murderData)) return;
        if (murderData?.Killer == null) return;

        double secondsSinceDeath = (DateTime.UtcNow - murderData.DeathTimeUtc).TotalSeconds;

        bool canGetRole = secondsSinceDeath <= GetRoleTime;
        bool canGetLightDark = secondsSinceDeath <= GetLightAndDarkerTime;
        if (!canGetRole && !canGetLightDark) return;

        string firstPerson = ModHelpers.IsSuccessChance(9)
            ? ModTranslation.GetString("DyingMessengerFirstPerson1")
            : ModTranslation.GetString("DyingMessengerFirstPerson2");

        byte receiverId = data.reporter.PlayerId;

        if (canGetRole)
        {
            string roleName = ModTranslation.GetString(murderData.Killer.Role.ToString());
            string text = ModTranslation.GetString("DyingMessengerGetRoleText", firstPerson, roleName);
            new LateTask(() => RpcAddChatWarning(receiverId, text), 0.5f, "DyingMessengerGetRole");
        }

        if (canGetLightDark)
        {
            string colorType = CustomColors.IsLighter(murderData.Killer) ? ModTranslation.GetString("LightColor") : ModTranslation.GetString("DarkColor");
            string text = ModTranslation.GetString("DyingMessengerGetLightAndDarkerText", firstPerson, colorType);
            new LateTask(() => RpcAddChatWarning(receiverId, text), 0.5f, "DyingMessengerGetLightDark");
        }
    }

    [CustomRPC]
    private static void RpcAddChatWarning(byte receiverId, string text)
    {
        if (PlayerControl.LocalPlayer == null) return;
        if (PlayerControl.LocalPlayer.PlayerId != receiverId) return;
        if (HudManager.Instance?.Chat == null) return;
        HudManager.Instance.Chat.AddChatWarning(text);
    }
}

