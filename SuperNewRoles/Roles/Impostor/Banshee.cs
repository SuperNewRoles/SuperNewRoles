using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles;
using UnityEngine;
using System.Linq;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Impostor;

class Banshee : RoleBase<Banshee>
{
    public enum BansheeFairyRange
    {
        Short,
        Medium,
        Long,
    }
    public override RoleId Role { get; } = RoleId.Banshee;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;

    private static Dictionary<BansheeFairyRange, float> BansheeFairyRangeValues = new()
    {
        { BansheeFairyRange.Short, 1f },
        { BansheeFairyRange.Medium, 1.8f },
        { BansheeFairyRange.Long, 2.5f },
    };

    public override List<Func<AbilityBase>> Abilities { get; } = new()
    {
        () => new BansheeAbility(
            releaseCooldown: BansheeReleaseCooldown,
            whisperCooldown: BansheeWhisperCooldown,
            fairyRange: BansheeFairyRangeValues.GetOrDefault(BansheeFairyRangeOption),
            canKillImpostor: BansheeCanKillImpostor,
            canDefaultKill: BansheeCanNormalKill
        ),
    };

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    // オプション設定
    [CustomOptionFloat("BansheeReleaseCooldown", 2.5f, 60f, 2.5f, 25f)]
    public static float BansheeReleaseCooldown;

    [CustomOptionFloat("BansheeWhisperCooldown", 0f, 10f, 10f, 0f)]
    public static float BansheeWhisperCooldown;

    [CustomOptionSelect("BansheeFairyRange", typeof(BansheeFairyRange), "BansheeFairyRange.")]
    public static BansheeFairyRange BansheeFairyRangeOption;

    [CustomOptionBool("BansheeCanKillImpostor", false)]
    public static bool BansheeCanKillImpostor;

    [CustomOptionBool("BansheeCanNormalKill", false)]
    public static bool BansheeCanNormalKill;
}

public class BansheeAbility : AbilityBase
{
    /* Ability */
    private BansheeReleaseAbility bansheeReleaseAbility;
    private BansheeWhisperAbility bansheeWhisperAbility;
    private ShowPlayerUIAbility showPlayerUIAbility;
    private CustomKillButtonAbility customKillButtonAbility;

    /* クールタイム */
    private float releaseCooldown;
    private float whisperCooldown;
    private float fairyRange;
    private bool canKillImpostor;
    private bool canDefaultKill;

    /* イベント */
    private EventListener _fixedUpdateListener;
    private EventListener<MeetingStartEventData> _startMeetingListener;

    /* データ */
    private ExPlayerControl currentFairyPlayer;
    private bool whisperTriggered = false;
    private HashSet<byte> playersInRangeAlreadyChecked;

    public BansheeAbility(float releaseCooldown, float whisperCooldown, float fairyRange, bool canKillImpostor, bool canDefaultKill)
    {
        this.canDefaultKill = canDefaultKill;
        this.releaseCooldown = releaseCooldown;
        this.whisperCooldown = whisperCooldown;
        this.fairyRange = fairyRange;
        this.canKillImpostor = canKillImpostor;
    }

    public override void AttachToAlls()
    {
        bansheeReleaseAbility = new BansheeReleaseAbility(
            releaseCooldown,
            () => currentFairyPlayer == null && !whisperTriggered,
            (newFairyPlayer) => { currentFairyPlayer = newFairyPlayer; whisperTriggered = false; }
        );
        bansheeWhisperAbility = new BansheeWhisperAbility(
            whisperCooldown,
            () => currentFairyPlayer != null && !whisperTriggered,
            () => whisperTriggered = true
        );
        showPlayerUIAbility = new ShowPlayerUIAbility(() => currentFairyPlayer != null ? [currentFairyPlayer] : []);
        customKillButtonAbility = new CustomKillButtonAbility(
            // 囁いた後は押せない
            () => canDefaultKill && !whisperTriggered,
            () => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown),
            () => true,
            killedCallback: (player) =>
            {
                ResetStatus();
                bansheeReleaseAbility.ResetTimer();
                bansheeWhisperAbility.ResetTimer();
            });
        Player.AttachAbility(bansheeReleaseAbility, new AbilityParentAbility(this));
        Player.AttachAbility(bansheeWhisperAbility, new AbilityParentAbility(this));
        Player.AttachAbility(showPlayerUIAbility, new AbilityParentAbility(this));
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();

        playersInRangeAlreadyChecked = new();
        currentFairyPlayer = null;

        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _startMeetingListener = MeetingStartEvent.Instance.AddListener(OnStartMeeting);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _fixedUpdateListener?.RemoveListener();
        _startMeetingListener?.RemoveListener();
    }

    private void OnStartMeeting(MeetingStartEventData data)
    {
        ResetStatus();
    }
    private void ResetStatus()
    {
        currentFairyPlayer = null;
        whisperTriggered = false;
        playersInRangeAlreadyChecked.Clear();
    }

    private void OnFixedUpdate()
    {
        if (currentFairyPlayer == null) return;

        // 妖精がついたプレイヤーが死んだらリセット
        if (currentFairyPlayer.IsDead())
        {
            ResetStatus();
            return;
        }

        foreach (var player in ExPlayerControl.ExPlayerControls)
        {
            if (player.IsDead()) continue;
            if (player.AmOwner) continue;
            if (player == currentFairyPlayer) continue;
            if (Vector3.Distance(currentFairyPlayer.transform.position, player.transform.position) <= fairyRange)
            {
                if (playersInRangeAlreadyChecked.Contains(player.PlayerId)) continue;
                if (whisperTriggered)
                {
                    playersInRangeAlreadyChecked.Add(player.PlayerId);
                    if (player.IsImpostor() && !canKillImpostor) continue;
                    Player.RpcCustomDeath(player, CustomDeathType.BansheeWhisper);
                    Player.ResetKillCooldown();
                    ResetStatus();
                    break;
                }
                currentFairyPlayer = player;
                playersInRangeAlreadyChecked.Add(player.PlayerId);
            }
            else
            {
                playersInRangeAlreadyChecked.Remove(player.PlayerId);
            }
        }
    }
}

public class BansheeReleaseAbility : TargetCustomButtonBase
{
    public override float DefaultTimer { get; }
    public override string buttonText => ModTranslation.GetString("BansheeReleaseButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("BansheeReleaseButton.png");
    protected override KeyType keytype => KeyType.Ability1;
    public override Color32 OutlineColor => Palette.ImpostorRed;
    public override bool OnlyCrewmates => false;

    private Func<bool> CanReleaseFunc;
    private Action<ExPlayerControl> SetFairyPlayer;

    public BansheeReleaseAbility(float cooldown, Func<bool> canReleaseFunc, Action<ExPlayerControl> setFairyPlayer)
    {
        DefaultTimer = cooldown;
        CanReleaseFunc = canReleaseFunc;
        SetFairyPlayer = setFairyPlayer;
    }

    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && CanReleaseFunc();
    }

    public override bool CheckIsAvailable()
    {
        return Target != null && Target.IsAlive();
    }

    public override void OnClick()
    {
        // ターゲットに妖精を付ける処理
        SetFairyPlayer(Target);
        Logger.Info($"Banshee released fairy to {Target.PlayerId}");
    }
}

public class BansheeWhisperAbility : CustomButtonBase
{
    public override float DefaultTimer { get; }
    public override string buttonText => ModTranslation.GetString("BansheeWhisperButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("BansheeWhisperButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    private Func<bool> CanWhisperFunc;
    private Action TriggerWhisperAction;

    public BansheeWhisperAbility(float cooldown, Func<bool> canWhisperFunc, Action triggerWhisper)
    {
        DefaultTimer = cooldown;
        CanWhisperFunc = canWhisperFunc;
        TriggerWhisperAction = triggerWhisper;
    }

    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && CanWhisperFunc();
    }

    public override bool CheckIsAvailable()
    {
        return true;
    }

    public override void OnClick()
    {
        // 妖精の位置関係でキル処理
        TriggerWhisperAction();
        Logger.Info("Banshee whispered");
    }
}