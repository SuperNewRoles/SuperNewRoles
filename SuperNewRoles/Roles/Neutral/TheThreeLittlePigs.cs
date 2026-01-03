using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

/// <summary>
/// 三匹の仔豚（n人チーム役職のサンプル実装）。
/// - 選出はメタ役職 `RoleId.TheThreeLittlePigs` として1枠で扱い、3人をまとめて選び
///   `TheFirst/Second/ThirdLittlePig` を割り当てる。
/// </summary>
internal sealed class TheThreeLittlePigs : TeamRoleBase<TheThreeLittlePigs>
{
    public override RoleId Role { get; } = RoleId.TheThreeLittlePigs;
    public override Color32 RoleColor { get; } = new(255, 99, 123, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = []; // メタ役職には能力を付けない
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Neutral;

    // メタ役職はUIに出すだけで、個別のメンバー役職はHiddenにする
    public override RoleId[] RelatedRoleIds { get; } = [RoleId.TheFirstLittlePig, RoleId.TheSecondLittlePig, RoleId.TheThirdLittlePig];

    public override int TeamSize => 3;
    public override IReadOnlyList<RoleId> MemberRoleIds => new[] { RoleId.TheFirstLittlePig, RoleId.TheSecondLittlePig, RoleId.TheThirdLittlePig };

    // Options
    // チーム数は RoleOptionMenu の「役職数(NumberOfCrews)」で設定する（= 何チーム出すか）
    [CustomOptionBool("TheThreeLittlePigsUseCustomTaskSetting", false)]
    public static bool TheThreeLittlePigsUseCustomTaskSetting;

    [CustomOptionTask("TheThreeLittlePigsTask", 1, 1, 1, parentFieldName: nameof(TheThreeLittlePigsUseCustomTaskSetting))]
    public static TaskOptionData TheThreeLittlePigsTaskData;

    [CustomOptionInt("TheFirstLittlePigClearTaskPercent", 0, 100, 5, 30)]
    public static int TheFirstLittlePigClearTaskPercent;

    [CustomOptionBool("TheFirstLittlePigUseCustomTimer", false)]
    public static bool TheFirstLittlePigUseCustomTimer;

    [CustomOptionFloat("TheFirstLittlePigFlashTimer", 5f, 60f, 2.5f, 30f, parentFieldName: nameof(TheFirstLittlePigUseCustomTimer))]
    public static float TheFirstLittlePigFlashTimer;

    [CustomOptionInt("TheSecondLittlePigClearTaskPercent", 0, 100, 5, 60)]
    public static int TheSecondLittlePigClearTaskPercent;

    [CustomOptionInt("TheSecondLittlePigMaxGuardCount", 1, 15, 1, 1)]
    public static int TheSecondLittlePigMaxGuardCount;

    [CustomOptionInt("TheThirdLittlePigClearTaskPercent", 0, 100, 5, 100)]
    public static int TheThirdLittlePigClearTaskPercent;

    [CustomOptionInt("TheThirdLittlePigMaxCounterCount", 1, 15, 1, 1)]
    public static int TheThirdLittlePigMaxCounterCount;

    // Team state
    public static readonly List<List<byte>> Teams = new(); // teams as playerId list (size=3)

    public override void AssignTeam(IReadOnlyList<PlayerControl> players)
    {
        if (players == null || players.Count != TeamSize)
            throw new ArgumentException($"players must be exactly {TeamSize}");

        var ids = players.Select(p => p.PlayerId).ToList();
        RpcRegisterTeam(ids);

        // Assign member roles in order
        ((ExPlayerControl)players[0]).RpcCustomSetRole(RoleId.TheFirstLittlePig);
        ((ExPlayerControl)players[1]).RpcCustomSetRole(RoleId.TheSecondLittlePig);
        ((ExPlayerControl)players[2]).RpcCustomSetRole(RoleId.TheThirdLittlePig);
    }

    public override void ClearAndReload()
    {
        Teams.Clear();
    }

    public static int GetAllTaskCount()
    {
        if (TheThreeLittlePigsUseCustomTaskSetting && TheThreeLittlePigsTaskData != null && TheThreeLittlePigsTaskData.Total > 0)
            return TheThreeLittlePigsTaskData.Total;

        return GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks) +
               GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks) +
               GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks);
    }

    public static bool IsLittlePig(ExPlayerControl p)
        => p != null && (p.Role is RoleId.TheFirstLittlePig or RoleId.TheSecondLittlePig or RoleId.TheThirdLittlePig);

    public static bool IsLittlePig(PlayerControl p) => IsLittlePig((ExPlayerControl)p);

    public static bool IsInSameTeam(ExPlayerControl a, ExPlayerControl b)
    {
        if (a == null || b == null) return false;
        foreach (var team in Teams)
        {
            if (team.Contains(a.PlayerId) && team.Contains(b.PlayerId))
                return true;
        }
        return false;
    }

    public static bool TryGetTeamOf(ExPlayerControl p, out List<byte> team)
    {
        team = null;
        if (p == null) return false;
        foreach (var t in Teams)
        {
            if (t.Contains(p.PlayerId))
            {
                team = t;
                return true;
            }
        }
        return false;
    }

    [CustomRPC]
    public static void RpcRegisterTeam(List<byte> playerIds)
    {
        if (playerIds == null || playerIds.Count != 3) return;
        // avoid duplicates
        if (Teams.Any(t => t.SequenceEqual(playerIds)))
            return;
        Teams.Add(new List<byte>(playerIds));
    }
}

internal sealed class TheFirstLittlePig : RoleBase<TheFirstLittlePig>
{
    public override RoleId Role { get; } = RoleId.TheFirstLittlePig;
    public override Color32 RoleColor { get; } = TheThreeLittlePigs.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new KnowOtherAbility((player) => TheThreeLittlePigs.IsInSameTeam(player, ExPlayerControl.LocalPlayer), () => true),
        () => new CustomTaskAbility(() => (true, false, Mathf.CeilToInt(TheThreeLittlePigs.GetAllTaskCount() * (TheThreeLittlePigs.TheFirstLittlePigClearTaskPercent / 100f))), TheThreeLittlePigs.TheThreeLittlePigsUseCustomTaskSetting ? TheThreeLittlePigs.TheThreeLittlePigsTaskData : null),
        () => new FirstLittlePigFlashAbility()
    ];
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Hidden;
}

internal sealed class TheSecondLittlePig : RoleBase<TheSecondLittlePig>
{
    public override RoleId Role { get; } = RoleId.TheSecondLittlePig;
    public override Color32 RoleColor { get; } = TheThreeLittlePigs.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new KnowOtherAbility((player) => TheThreeLittlePigs.IsInSameTeam(player, ExPlayerControl.LocalPlayer), () => true),
        () => new CustomTaskAbility(() => (true, false, Mathf.CeilToInt(TheThreeLittlePigs.GetAllTaskCount() * (TheThreeLittlePigs.TheSecondLittlePigClearTaskPercent / 100f))), TheThreeLittlePigs.TheThreeLittlePigsUseCustomTaskSetting ? TheThreeLittlePigs.TheThreeLittlePigsTaskData : null),
        () => new SecondLittlePigGuardAbility(TheThreeLittlePigs.TheSecondLittlePigMaxGuardCount)
    ];
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Hidden;
}

internal sealed class TheThirdLittlePig : RoleBase<TheThirdLittlePig>
{
    public override RoleId Role { get; } = RoleId.TheThirdLittlePig;
    public override Color32 RoleColor { get; } = TheThreeLittlePigs.Instance.RoleColor;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new KnowOtherAbility((player) => TheThreeLittlePigs.IsInSameTeam(player, ExPlayerControl.LocalPlayer), () => true),
        () => new CustomTaskAbility(() => (true, false, Mathf.CeilToInt(TheThreeLittlePigs.GetAllTaskCount() * (TheThreeLittlePigs.TheThirdLittlePigClearTaskPercent / 100f))), TheThreeLittlePigs.TheThreeLittlePigsUseCustomTaskSetting ? TheThreeLittlePigs.TheThreeLittlePigsTaskData : null),
        () => new ThirdLittlePigCounterAbility()
    ];
    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Neutral;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Neutral;
    public override TeamTag TeamTag { get; } = TeamTag.Neutral;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Hidden;
}

/// <summary>
/// 1番目: 一定時間ごとに条件達成していればフラッシュ（ローカルのみ）。
/// </summary>
internal sealed class FirstLittlePigFlashAbility : AbilityBase
{
    private EventListener _fixedUpdate;
    private float _timer;

    public override void AttachToLocalPlayer()
    {
        _timer = 0f;
        _fixedUpdate = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        _fixedUpdate?.RemoveListener();
    }

    private void OnFixedUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null || ExileController.Instance != null) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;

        var interval = TheThreeLittlePigs.TheFirstLittlePigUseCustomTimer
            ? TheThreeLittlePigs.TheFirstLittlePigFlashTimer
            : Math.Max(5f, GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
        _timer = interval;

        if (IsTaskConditionMet())
        {
            FlashHandler.ShowFlash(new Color32(245, 95, 71, byte.MaxValue), 2.5f);
        }
    }

    private bool IsTaskConditionMet()
    {
        int all = TheThreeLittlePigs.GetAllTaskCount();
        int need = Mathf.CeilToInt(all * (TheThreeLittlePigs.TheFirstLittlePigClearTaskPercent / 100f));
        int completed = ModHelpers.TaskCompletedData(Player.Data).completed;
        return completed >= need;
    }
}

/// <summary>
/// 2番目: 一定タスク達成後、指定回数までキルガード（TryKillEventで無効化）。
/// </summary>
internal sealed class SecondLittlePigGuardAbility : AbilityBase, IAbilityCount
{
    private EventListener<TryKillEventData> _tryKill;
    private int _needTasks;
    private int _allTasks;

    public SecondLittlePigGuardAbility(int maxGuardCount)
    {
        Count = maxGuardCount;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _allTasks = TheThreeLittlePigs.GetAllTaskCount();
        _needTasks = Mathf.CeilToInt(_allTasks * (TheThreeLittlePigs.TheSecondLittlePigClearTaskPercent / 100f));
        _tryKill = TryKillEvent.Instance.AddListener(OnTryKill);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _tryKill?.RemoveListener();
    }

    private void OnTryKill(TryKillEventData data)
    {
        if (data.RefTarget != Player) return;
        if (!HasCount) return;
        if (!IsTaskConditionMet()) return;

        Count--;
        data.RefSuccess = false;
        if (data.Killer.AmOwner || ExPlayerControl.LocalPlayer.IsDead())
            data.RefTarget.Player.ShowFailedMurder();
        if (data.Killer.AmOwner)
            data.Killer.ResetKillCooldown();
    }

    private bool IsTaskConditionMet()
    {
        int completed = ModHelpers.TaskCompletedData(Player.Data).completed;
        return completed >= _needTasks;
    }
}

/// <summary>
/// 3番目: 一定タスク達成後、指定回数まで「ガード＋カウンターキル」。
/// </summary>
internal sealed class ThirdLittlePigCounterAbility : AbilityBase, IAbilityCount
{
    private EventListener<TryKillEventData> _tryKill;
    private int _needTasks;
    private int _allTasks;

    public ThirdLittlePigCounterAbility()
    {
        Count = TheThreeLittlePigs.TheThirdLittlePigMaxCounterCount;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _allTasks = TheThreeLittlePigs.GetAllTaskCount();
        _needTasks = Mathf.CeilToInt(_allTasks * (TheThreeLittlePigs.TheThirdLittlePigClearTaskPercent / 100f));
        _tryKill = TryKillEvent.Instance.AddListener(OnTryKill);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _tryKill?.RemoveListener();
    }

    private void OnTryKill(TryKillEventData data)
    {
        if (data.RefTarget != Player) return;
        if (!HasCount) return;
        if (!IsTaskConditionMet()) return;

        // Block incoming kill
        Count--;
        data.RefSuccess = false;

        // Counter-kill the attacker (host only to avoid duplicate RPC spam)
        if (AmongUsClient.Instance.AmHost && data.Killer != null && data.Killer.IsAlive())
        {
            data.Killer.RpcCustomDeath(CustomDeathType.Suicide);
            FinalStatusManager.RpcSetFinalStatus(data.Killer, FinalStatus.TheThirdLittlePigCounterKill);
        }

        if (data.Killer.AmOwner || ExPlayerControl.LocalPlayer.IsDead())
            data.RefTarget.Player.ShowFailedMurder();
        if (data.Killer.AmOwner)
            data.Killer.ResetKillCooldown();
    }

    private bool IsTaskConditionMet()
    {
        int completed = ModHelpers.TaskCompletedData(Player.Data).completed;
        return completed >= _needTasks;
    }
}


