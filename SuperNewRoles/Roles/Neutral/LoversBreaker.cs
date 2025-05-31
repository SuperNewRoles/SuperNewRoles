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
using System.Linq;

namespace SuperNewRoles.Roles.Neutral;

class LoversBreaker : RoleBase<LoversBreaker>
{
    public override RoleId Role { get; } = RoleId.LoversBreaker;
    public override Color32 RoleColor { get; } = new Color32(199, 21, 133, 255); // 紫色
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new LoversBreakerAbility(new LoversBreakerData(
            KillCooldown: LoversBreakerKillCooldown,
            WinKillCount: LoversBreakerWinKillCount,
            IsDeathWin: LoversBreakerIsDeathWin
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

    [CustomOptionFloat("LoversBreakerKillCooldown", 2.5f, 60f, 2.5f, 30f, translationName: "CoolTime")]
    public static float LoversBreakerKillCooldown;

    [CustomOptionInt("LoversBreakerWinKillCount", 1, 10, 1, 1)]
    public static int LoversBreakerWinKillCount;

    [CustomOptionBool("LoversBreakerIsDeathWin", false)]
    public static bool LoversBreakerIsDeathWin;
}

public record LoversBreakerData(float KillCooldown, int WinKillCount, bool IsDeathWin);

public class LoversBreakerAbility : TargetCustomButtonBase
{
    public LoversBreakerData Data { get; set; }

    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;
    private EventListener _fixedUpdateListener;
    private EventListener<DieEventData> _dieListener;

    private int _successCount;

    public LoversBreakerAbility(LoversBreakerData data)
    {
        Data = data;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _dieListener = DieEvent.Instance.AddListener(OnDie);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _nameTextUpdateListener?.RemoveListener();
        _fixedUpdateListener?.RemoveListener();
        _dieListener?.RemoveListener();
    }

    public override Color32 OutlineColor => LoversBreaker.Instance.RoleColor;
    protected override KeyType keytype => KeyType.Kill;
    public override float DefaultTimer => Data.KillCooldown;
    public override string buttonText => ModTranslation.GetString("LoversBreakerBreak");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("LoversBreakerButton.png");
    public override bool OnlyCrewmates => false;
    public override bool TargetPlayersInVents => false;
    public override ShowTextType showTextType => ShowTextType.Show;
    public override string showText => ModHelpers.Cs(Color.cyan, $"({_successCount}/{Data.WinKillCount})");

    public override bool CheckIsAvailable()
    {
        return TargetIsExist && PlayerControl.LocalPlayer.CanMove && ExPlayerControl.LocalPlayer.IsAlive();
    }

    private int checkCounter = 0;

    private void OnFixedUpdate()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        checkCounter++;
        if (checkCounter % 10 != 0) return;
        checkCounter = 0;
        CheckWin();
    }
    private void CheckWin()
    {
        if (!Data.IsDeathWin && Player.IsDead()) return;
        if (_successCount >= Data.WinKillCount && !ExPlayerControl.ExPlayerControls.Any(p => p.IsLovers() && p.IsAlive()))
        {
            EndGamer.RpcEndGameWithWinner(CustomGameOverReason.LoversBreakerWin, WinType.SingleNeutral, [Player], LoversBreaker.Instance.RoleColor, "LoversBreaker", string.Empty);
        }
    }

    private void OnDie(DieEventData data)
    {
        if (AmongUsClient.Instance.AmHost)
            CheckWin();
    }

    public override void OnClick()
    {
        if (Target == null) return;
        if (Target.IsLovers() || Target.Role is RoleId.Cupid or RoleId.Truelover)
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(Target, CustomDeathType.Kill);
            if (Target.Role != RoleId.Cupid)
            {
                _successCount++;
                RpcSyncCount(_successCount);
            }
            CheckWin();
        }
        else
        {
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
        }
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

    [CustomRPC]
    public void RpcSyncCount(int successCount)
    {
        SetCount(successCount);
    }

    private void SetCount(int successCount)
    {
        _successCount = successCount;
    }
}