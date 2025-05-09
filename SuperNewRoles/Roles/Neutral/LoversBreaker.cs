using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles.Neutral;

class LoversBreaker : RoleBase<LoversBreaker>
{
    public override RoleId Role { get; } = RoleId.LoversBreaker;
    public override Color32 RoleColor { get; } = new Color32(199, 21, 133, 255); // 紫色
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new LoversBreakerAbility(new LoversBreakerData(
            KillCooldown: LoversBreakerKillCooldown,
            WinKillCount: LoversBreakerWinKillCount
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    [CustomOptionFloat("LoversBreakerKillCooldown", 2.5f, 60f, 2.5f, 10f, translationName: "CoolTime")]
    public static float LoversBreakerKillCooldown;

    [CustomOptionInt("LoversBreakerWinKillCount", 1, 10, 1, 3)]
    public static int LoversBreakerWinKillCount;
}

public record LoversBreakerData(float KillCooldown, int WinKillCount);

public class LoversBreakerAbility : TargetCustomButtonBase
{
    public LoversBreakerData Data { get; set; }

    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;

    private int _successCount;

    public LoversBreakerAbility(LoversBreakerData data)
    {
        Data = data;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateListener?.RemoveListener();
    }

    public override Color32 OutlineColor => LoversBreaker.Instance.RoleColor;
    protected override KeyType keytype => KeyType.Kill;
    public override float DefaultTimer => Data.KillCooldown;
    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.KillLabel);
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("LoversBreakerButton.png");
    public override bool OnlyCrewmates => false;
    public override bool TargetPlayersInVents => false;
    public override ShowTextType showTextType => ShowTextType.Show;
    public override string showText => ModHelpers.Cs(Color.cyan, $"({_successCount}/{Data.WinKillCount})");

    public override bool CheckIsAvailable()
    {
        return TargetIsExist && Timer <= 0f && PlayerControl.LocalPlayer.CanMove && !ExPlayerControl.LocalPlayer.IsDead();
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (Target.IsLovers())
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(Target, CustomDeathType.Kill);
            _successCount++;
            RpcSyncCount();
            if (_successCount >= Data.WinKillCount)
            {
                EndGamer.RpcEndGameWithWinner(SuperNewRoles.Patches.CustomGameOverReason.LoversBreakerWin, WinType.SingleNeutral, new ExPlayerControl[] { ExPlayerControl.LocalPlayer }, LoversBreaker.Instance.RoleColor, "LoversBreaker", string.Empty);
            }
        }
        else
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
        }
        ResetTimer();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player != Player) return;
        if (!data.Player.Player.Visible) return;
        string text = ModHelpers.Cs(Color.cyan, $"({_successCount}/{Data.WinKillCount})");
        Player.PlayerInfoText.text += text;
        if (Player.MeetingInfoText != null)
            Player.MeetingInfoText.text += text;
    }

    private void RpcSyncCount()
    {
        RpcSyncCount(this, _successCount);
    }

    [CustomRPC]
    public static void RpcSyncCount(LoversBreakerAbility ability, int successCount)
    {
        if (ability != null)
            ability.SetCount(successCount);
        else
            Logger.Error("LoversBreakerAbility is null");
    }

    private void SetCount(int successCount)
    {
        _successCount = successCount;
    }
}