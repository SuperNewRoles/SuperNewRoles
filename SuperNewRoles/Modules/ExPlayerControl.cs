using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Ability;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Events;
using SuperNewRoles.Extensions;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.SuperTrophies;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

// 型情報キャッシュシステム - 静的型IDによる高速ルックアップ
internal static class TypeCache<T> where T : AbilityBase
{
    internal static readonly int TypeId;
    internal static readonly Type Type;
    internal static readonly string TypeName;

    static TypeCache()
    {
        TypeId = System.Threading.Interlocked.Increment(ref TypeIdGenerator.NextId);
        Type = typeof(T);
        TypeName = Type.Name;
    }
}

internal static class TypeIdGenerator
{
    internal static int NextId = -1; // 0から始まるように-1で初期化
}

public class ExPlayerControl
{
    public static ExPlayerControl LocalPlayer;
    private static List<ExPlayerControl> _exPlayerControls { get; } = new();
    public static List<ExPlayerControl> ExPlayerControls => _exPlayerControls;
    private static ExPlayerControl[] _exPlayerControlsArray = new ExPlayerControl[256];
    public PlayerControl Player { get; }
    public NetworkedPlayerInfo Data { get; }
    public PlayerPhysics MyPhysics => Player?.MyPhysics;
    public CustomNetworkTransform NetTransform => Player?.NetTransform;
    public CosmeticsLayer cosmetics => Player?.cosmetics;
    public bool moveable => Player?.moveable ?? false;
    public Vector2 GetTruePosition() => Player?.GetTruePosition() ?? Vector2.zero;
    public float MaxReportDistance => Player?.MaxReportDistance ?? 1f;
    public Transform transform => Player?.transform;
    public byte PlayerId { get; }
    public bool AmOwner { get; private set; }
    public RoleId Role { get; private set; }
    public ModifierRoleId ModifierRole { get; private set; }
    public GhostRoleId GhostRole { get; private set; }
    public List<RoleId> RoleHistory { get; private set; } = new();
    public List<GhostRoleId> GhostRoleHistory { get; private set; } = new();
    public List<ModifierRoleId> ModifierRoleHistory { get; private set; } = new();
    public IRoleBase roleBase { get; private set; }
    public List<IModifierBase> ModifierRoleBases { get; private set; } = new();
    public IGhostRoleBase GhostRoleBase { get; private set; }
    private readonly List<AbilityBase> _playerAbilities = new();
    private readonly Dictionary<ulong, AbilityBase> _abilitiesById = new();
    // Detach 後に同じIDを再発行すると、遅れて届いたRPCが別Abilityへ誤配信されるため、接続中に発行したIDは保持する。
    private readonly HashSet<ulong> _issuedAbilityIds = new();
    internal int AbilityCount => _playerAbilities.Count;
    public TextMeshPro PlayerInfoText { get; set; }
    public TextMeshPro MeetingInfoText { get; set; }
    public PlayerVoteArea VoteArea { get; set; }
    private FinalStatus? _finalStatus;
    public FinalStatus FinalStatus { get { return _finalStatus ?? FinalStatus.Alive; } set { _finalStatus = value; } }

    // パフォーマンス最適化用キャッシュ
    private readonly Dictionary<int, AbilityBase> _typeIdAbilityCache = new();
    private readonly Dictionary<int, IReadOnlyList<object>> _typeIdReadOnlyCache = new();
    private readonly Dictionary<int, List<AbilityBase>> _prioritizedAbilities = new();

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return PlayerId;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
    {
        if (obj is ExPlayerControl other)
            return PlayerId == other.PlayerId;
        return false;
    }

    public ExPlayerControl(PlayerControl player)
    {
        this.Player = player;
        this.PlayerId = player.PlayerId;
        this.Data = player.CachedPlayerData;
        this.AmOwner = player.AmOwner;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static implicit operator PlayerControl(ExPlayerControl exPlayer)
    {
        return exPlayer?.Player;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static implicit operator ExPlayerControl(PlayerControl player)
    {
        if (player == null) return null;
        unsafe
        {
            fixed (ExPlayerControl* ptr = _exPlayerControlsArray)
            {
                return *(ptr + player.PlayerId);
            }
        }
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static implicit operator ExPlayerControl(PlayerVoteArea player)
    {
        if (player == null) return null;
        unsafe
        {
            fixed (ExPlayerControl* ptr = _exPlayerControlsArray)
            {
                return *(ptr + player.TargetPlayerId);
            }
        }
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static implicit operator ExPlayerControl(NetworkedPlayerInfo data)
    {
        if (data == null) return null;
        unsafe
        {
            fixed (ExPlayerControl* ptr = _exPlayerControlsArray)
            {
                return *(ptr + data.PlayerId);
            }
        }
    }
    public bool HasAbility<T>() where T : AbilityBase
        => TryGetAbility<T>(out _);
    public bool IsTaskComplete()
    {
        (int completed, int total) = ModHelpers.TaskCompletedData(Data);
        var taskAbilities = GetPrioritizedAbilities<CustomTaskAbility>();
        if (taskAbilities != null)
        {
            for (var i = 0; i < taskAbilities.Count; i++)
            {
                var isTaskTrigger = ((CustomTaskAbility)taskAbilities[i]).IsTaskTrigger?.Invoke();
                if (isTaskTrigger.HasValue)
                    return isTaskTrigger.Value && completed >= ResolveRequiredTaskCount(total);
            }
        }
        return completed >= ResolveRequiredTaskCount(total);
    }
    public void ResetKillCooldown()
    {
        if (!AmOwner) return;
        var killButton = FastDestroyableSingleton<HudManager>.Instance.KillButton;
        if (killButton.isActiveAndEnabled)
        {
            var options = GameOptionsManager.Instance.CurrentGameOptions;
            float coolTime = options.GetFloat(FloatOptionNames.KillCooldown);
            SetKillTimerUnchecked(coolTime, coolTime);
        }
        var customKillButtons = GetAbilities<CustomKillButtonAbility>();
        for (var i = 0; i < customKillButtons.Count; i++)
        {
            customKillButtons[i].ResetTimer();
        }
    }
    public void SetKillTimerUnchecked(float time, float maxTime)
    {
        Player.killTimer = Mathf.Clamp(time, 0f, maxTime);
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(Player.killTimer, maxTime);
    }
    public void SetModifierRole(ModifierRoleId modifierRoleId)
    {
        if (ModifierRole.HasFlag(modifierRoleId)) return;
        ModifierRoleId oldModifier = ModifierRole;
        if (AmOwner)
            SuperTrophyManager.DetachTrophy(Role);
        ModifierRole |= modifierRoleId;
        if (ModifierRoleHistory.Count == 0 && oldModifier != ModifierRoleId.None)
            ModifierRoleHistory.Add(oldModifier);
        ModifierRoleHistory.Add(ModifierRole);
        Logger.Info($"[Modifier] {PlayerId}:{Player?.name ?? "??"}({Role}) += {modifierRoleId}", "SNR.GameState");
        if (CustomRoleManager.TryGetModifierById(modifierRoleId, out var modifier))
        {
            modifier.OnSetRole(Player);
            if (AmOwner)
                SuperTrophyManager.RegisterTrophy(modifierRoleId);
            ModifierRoleBases.Add(modifier);
        }
        else
        {
            Logger.Error($"Modifier {modifierRoleId} not found");
        }
    }
    public void SetGhostRole(GhostRoleId ghostRoleId)
    {
        if (GhostRole == ghostRoleId) return;
        if (GhostRoleHistory.Count == 0 && GhostRole != GhostRoleId.None)
            GhostRoleHistory.Add(GhostRole);
        GhostRoleHistory.Add(ghostRoleId);
        DetachOldGhostRole(GhostRole);
        if (AmOwner && GhostRole != GhostRoleId.None)
            SuperTrophyManager.DetachTrophy(GhostRole);
        var oldGhostRole = GhostRole;
        GhostRole = ghostRoleId;
        Logger.Info($"[GhostRole] {PlayerId}:{Player?.name ?? "??"}({Role}): {oldGhostRole} -> {ghostRoleId}", "SNR.GameState");
        if (CustomRoleManager.TryGetGhostRoleById(ghostRoleId, out var role))
        {
            role.OnSetRole(Player);
            if (AmOwner)
                SuperTrophyManager.RegisterTrophy(Role);
            if (GhostRoleBase != null)
                DetachOldGhostRole(GhostRoleBase.Role);
            GhostRoleBase = role;
        }
        else
        {
            Logger.Error($"GhostRole {ghostRoleId} not found");
        }
    }
    public void SetRole(RoleId roleId, bool recordHistory = true)
    {
        if (Role == roleId) return;
        RoleId oldRole = Role;
        DetachOldRole(Role);
        if (AmOwner)
            SuperTrophyManager.DetachTrophy(Role);
        if (recordHistory)
            AddRoleHistory(RoleHistory, oldRole, roleId);
        Role = roleId;
        Logger.Info($"[SetRole] Player {Player?.name} ({PlayerId}) changing role from {oldRole} to {roleId}, AmOwner: {AmOwner}");
        if (CustomRoleManager.TryGetRoleById(roleId, out var role))
        {
            Logger.Info($"[SetRole] Calling OnSetRole for player {Player?.name} ({PlayerId})");
            role.OnSetRole(Player);
            Logger.Info($"[SetRole] OnSetRole completed, Ability count: {AbilityCount}");
            if (AmOwner)
                SuperTrophyManager.RegisterTrophy(Role);
            roleBase = role;
            // ToArray()を使わずに、削除対象を後で処理
            List<ModifierRoleId> toDetach = null;
            foreach (var modifier in ModifierRoleBases)
            {
                Logger.Info($"ModifierRole: {modifier.ModifierRole} AssignedTeams: {string.Join(",", modifier.AssignedTeams)} RoleBaseAssignedTeam: {roleBase.AssignedTeam}", "ExPlayerControl");
                if (modifier.AssignedTeams.Count != 0 && !modifier.AssignedTeams.Contains(roleBase.AssignedTeam))
                {
                    if (toDetach == null) toDetach = new List<ModifierRoleId>();
                    toDetach.Add(modifier.ModifierRole);
                }
            }
            if (toDetach != null)
            {
                foreach (var modifierId in toDetach)
                {
                    DetachOldModifierRole(modifierId);
                }
            }
        }
        else
        {
            Logger.Error($"Role {roleId} not found");
        }
        try
        {
            SetRoleEvent.Invoke(this, oldRole, roleId);
        }
        catch (Exception e)
        {
            Logger.Error($"SetRoleEvent.Invoke failed: {oldRole} -> {roleId}", "ExPlayerControl");
            Logger.Error(e.ToString(), "ExPlayerControl");
        }
        if (AmOwner)
        {
            AssignCustomTasks();
        }
    }

    internal static void AddRoleHistory(List<RoleId> roleHistory, RoleId oldRole, RoleId roleId)
    {
        if (roleHistory.Count == 0 && oldRole != RoleId.None)
            roleHistory.Add(oldRole);
        roleHistory.Add(roleId);
    }

    internal static void AddRoleHistoryIfNeeded(List<RoleId> roleHistory, RoleId oldRole, RoleId roleId, bool recordHistory)
    {
        if (!recordHistory) return;
        AddRoleHistory(roleHistory, oldRole, roleId);
    }

    public bool HasCustomKillButton()
    {
        return HasAbility<CustomKillButtonAbility>();
    }
    public bool showKillButtonVanilla()
    {
        var canKill = ResolveCanKill();
        return IsImpostor() && IsAlive() && canKill;
    }

    private bool ResolveCanKill()
    {
        var canKill = true;
        var killableAbilities = GetPrioritizedAbilities<KillableAbility>();
        if (killableAbilities != null)
        {
            for (var i = 0; i < killableAbilities.Count; i++)
            {
                var decision = ((KillableAbility)killableAbilities[i]).CanKill;
                if (!decision.HasValue) continue;
                canKill = decision.Value;
                break;
            }
        }
        return canKill;
    }
    private void DetachOldRole(RoleId roleId)
    {
        List<AbilityBase> abilitiesToDetach = new();
        List<AbilityParentRole> abilitiesToDetachParentRole = new();
        foreach (var ability in _playerAbilities)
        {
            if (ability.Parent == null) continue;
            var parent = ability.Parent;
            while (parent != null)
            {
                switch (parent)
                {
                    case AbilityParentRole parentRole when parentRole.ParentRole != null && parentRole.ParentRole.Role == roleId:
                        abilitiesToDetach.Add(ability);
                        abilitiesToDetachParentRole.Add(parentRole);
                        parent = null;
                        break;
                    case AbilityParentAbility parentAbility when parentAbility.ParentAbility != null:
                        parent = parentAbility.ParentAbility.Parent;
                        break;
                    default:
                        parent = null;
                        break;
                }
            }
        }
        foreach (var ability in abilitiesToDetach)
            DetachAbility(ability.AbilityId);
        foreach (var parentRole in abilitiesToDetachParentRole)
            parentRole.Player = null;
        if (AmOwner)
            SuperTrophyManager.DetachTrophy(abilitiesToDetach);
    }
    private void DetachOldGhostRole(GhostRoleId ghostRoleId)
    {
        List<AbilityBase> abilitiesToDetach = new();
        foreach (var ability in _playerAbilities)
        {
            if (ability.Parent == null) continue;
            var parent = ability.Parent;
            while (parent != null)
            {
                switch (parent)
                {
                    case AbilityParentGhostRole parentGhostRole when parentGhostRole.ParentGhostRole != null && parentGhostRole.ParentGhostRole.Role == ghostRoleId:
                        abilitiesToDetach.Add(ability);
                        parent = null;
                        break;
                    case AbilityParentAbility parentAbility when parentAbility.ParentAbility != null:
                        parent = parentAbility.ParentAbility.Parent;
                        break;
                    default:
                        parent = null;
                        break;
                }
            }
        }
        foreach (var ability in abilitiesToDetach)
        {
            DetachAbility(ability.AbilityId);
        }
        if (AmOwner)
            SuperTrophyManager.DetachTrophy(abilitiesToDetach);
    }
    private void DetachOldModifierRole(ModifierRoleId modifierRoleId)
    {
        List<AbilityBase> abilitiesToDetach = new();
        foreach (var ability in _playerAbilities)
        {
            if (ability.Parent == null) continue;
            var parent = ability.Parent;
            while (parent != null)
            {
                switch (parent)
                {
                    case AbilityParentModifier parentModifier when parentModifier.ParentModifier != null && modifierRoleId.HasFlag(parentModifier.ParentModifier.ModifierRole):
                        abilitiesToDetach.Add(ability);
                        parent = null;
                        break;
                    case AbilityParentAbility parentAbility when parentAbility.ParentAbility != null:
                        parent = parentAbility.ParentAbility.Parent;
                        break;
                    default:
                        parent = null;
                        break;
                }
            }
        }
        foreach (var ability in abilitiesToDetach)
        {
            DetachAbility(ability.AbilityId);
        }
        if (AmOwner)
            SuperTrophyManager.DetachTrophy(abilitiesToDetach);
        ModifierRoleId oldModifier = ModifierRole;
        ModifierRole &= ~modifierRoleId;
        ModifierRoleBases.RemoveAll(x => modifierRoleId.HasFlag(x.ModifierRole));
        if (ModifierRole != oldModifier)
        {
            if (ModifierRoleHistory.Count == 0 && oldModifier != ModifierRoleId.None)
                ModifierRoleHistory.Add(oldModifier);
            ModifierRoleHistory.Add(ModifierRole);
        }
    }
    public void ReverseTask(ExPlayerControl target)
    {
        if (target == null || target.Player == null) return;

        var myTasks = Player.myTasks.Where(x => x.TryCast<NormalPlayerTask>() != null).ToArray();
        var myTaskIds = myTasks.Select(x => (byte)x.Index).ToArray();
        var targetTasks = target.Player.myTasks.Where(x => x.TryCast<NormalPlayerTask>() != null).ToArray();
        var targetTaskIds = targetTasks.Select(x => (byte)x.Index).ToArray();

        // それぞれの完了済みタスクを保存 (TypeId ベース)
        var myCompletedTaskIds = myTasks.Where(x => x.IsComplete).Select(x => (byte)x.Id);
        var targetCompletedTaskIds = targetTasks.Where(x => x.IsComplete).Select(x => (byte)x.Id);

        // タスク一覧を入れ替え
        Data.SetTasks(targetTaskIds);
        target.Data.SetTasks(myTaskIds);
        // 交換後のタスクに完了状態を反映
        foreach (var taskId in targetCompletedTaskIds)
        {
            Player.CompleteTask((uint)taskId);
        }
        foreach (var taskId in myCompletedTaskIds)
        {
            target.Player.CompleteTask((uint)taskId);
        }

        // 表示を更新
        NameText.UpdateNameInfo(this);
        NameText.UpdateNameInfo(target);
    }
    public void CopyTaskProgressFrom(ExPlayerControl target)
    {
        if (target == null || target.Player == null) return;

        var targetTasks = target.Player.myTasks.Where(x => x.TryCast<NormalPlayerTask>() != null).ToArray();
        var targetTaskIds = targetTasks.Select(x => (byte)x.Index).ToArray();
        var targetCompletedTaskIds = targetTasks.Where(x => x.IsComplete).Select(x => (byte)x.Id);

        Data.SetTasks(targetTaskIds);
        foreach (var taskId in targetCompletedTaskIds)
        {
            Player.CompleteTask((uint)taskId);
        }

        NameText.UpdateNameInfo(this);
        NameText.UpdateNameInfo(target);
    }
    public void ReverseRole(ExPlayerControl target, bool recordTargetHistory = true)
    {
        if (target == null || target.Player == null) return;

        // 自分と相手のAbilitiesとRoleを保存
        List<(AbilityBase ability, ulong abilityId)> myAbilities = new();
        List<(AbilityBase ability, ulong abilityId)> targetAbilities = new();

        // ToArray()を使わずに直接リストから収集
        foreach (var ability in _playerAbilities)
        {
            if (ability != null && IsRoleAbility(ability.Parent))
            {
                myAbilities.Add((ability, ability.AbilityId));
            }
        }

        foreach (var ability in target._playerAbilities)
        {
            if (ability != null && IsRoleAbility(ability.Parent))
            {
                targetAbilities.Add((ability, ability.AbilityId));
            }
        }

        // 両方のプレイヤーのRoleを保存
        RoleId myRole = Role;
        RoleId targetRole = target.Role;
        IRoleBase myRoleBase = roleBase;
        IRoleBase targetRoleBase = target.roleBase;

        // 両方のプレイヤーからAbilitiesをすべてDetach
        foreach (var abilityData in myAbilities)
        {
            DetachAbility(abilityData.abilityId);
        }

        foreach (var abilityData in targetAbilities)
        {
            target.DetachAbility(abilityData.abilityId);
        }

        // お互いのRoleを入れ替え

        if (Player.AmOwner)
            SuperTrophyManager.DetachTrophy(Role);

        AddRoleHistoryIfNeeded(RoleHistory, myRole, targetRole, true);
        AddRoleHistoryIfNeeded(target.RoleHistory, targetRole, myRole, recordTargetHistory);

        Role = targetRole;
        roleBase = targetRoleBase;
        target.Role = myRole;
        target.roleBase = myRoleBase;

        // アタッチする
        foreach (var ability in myAbilities)
        {
            var currentParent = ability.ability.Parent;
            if (currentParent is AbilityParentAbility)
                continue;
            if (!TryMoveRoleParent(currentParent, target))
                continue;
            target.AttachAbility(ability.ability, currentParent);
        }
        foreach (var ability in targetAbilities)
        {
            var currentParent = ability.ability.Parent;
            if (currentParent is AbilityParentAbility)
                continue;
            if (!TryMoveRoleParent(currentParent, this))
                continue;
            AttachAbility(ability.ability, currentParent);
        }
        // 名前情報を更新
        NameText.UpdateAllNameInfo();
    }

    private static bool IsRoleAbility(AbilityParentBase parent)
    {
        while (parent != null)
        {
            switch (parent)
            {
                case AbilityParentRole:
                    return true;
                case AbilityParentAbility parentAbility when parentAbility.ParentAbility != null:
                    parent = parentAbility.ParentAbility.Parent;
                    continue;
                default:
                    return false;
            }
        }
        return false;
    }

    private static bool TryMoveRoleParent(AbilityParentBase parent, ExPlayerControl player)
    {
        while (parent != null)
        {
            switch (parent)
            {
                case AbilityParentRole parentRole:
                    parentRole.Player = player;
                    return true;
                case AbilityParentAbility parentAbility when parentAbility.ParentAbility != null:
                    parent = parentAbility.ParentAbility.Parent;
                    continue;
                default:
                    return false;
            }
        }
        return false;
    }
    public void Disconnected()
    {
        _exPlayerControls.Remove(this);
        _exPlayerControlsArray[PlayerId] = null;
        foreach (var ability in _playerAbilities.ToArray())
        {
            ability.Detach();
        }
        ClearAbilityState();
    }
    public static void SetUpExPlayers()
    {
        _exPlayerControlsArray = new ExPlayerControl[256];
        _exPlayerControls.Clear();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            _exPlayerControls.Add(new ExPlayerControl(player));
            _exPlayerControlsArray[player.PlayerId] = _exPlayerControls[^1];
        }
        LocalPlayer = _exPlayerControlsArray[PlayerControl.LocalPlayer.PlayerId];
        DisconnectEvent.Instance.AddListener(x =>
        {
            ExPlayerControl exPlayer = (ExPlayerControl)x.disconnectedPlayer;
            if (exPlayer != null)
                exPlayer.Disconnected();
        });
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static ExPlayerControl ById(byte playerId)
    {
        unsafe
        {
            fixed (ExPlayerControl* ptr = _exPlayerControlsArray)
            {
                return *(ptr + playerId);
            }
        }
    }
    public bool IsKiller()
    {
        if (IsImpostor() || IsPavlovsDog() || IsJackal() || Role == RoleId.Hitman)
            return true;

        var customKillButtons = GetAbilities<CustomKillButtonAbility>();
        for (var i = 0; i < customKillButtons.Count; i++)
        {
            var ability = customKillButtons[i];
            if (ability?.CanKill?.Invoke() == true)
                return true;
        }

        return false;
    }

    public bool IsNonCrewKiller()
        => IsKiller() && !IsCrewmate();

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool IsCrewmate()
        => roleBase != null ? (Role == RoleId.SchrodingersCat && GetAbility<SchrodingersCatAbility>()?.CurrentTeam == SchrodingersCatTeam.Crewmate) || (roleBase.AssignedTeam == AssignedTeamType.Crewmate && !IsMadRoles() && !IsFriendRoles()) : !Data.Role.IsImpostor;

    public bool IsCrewmateWin()
        => IsCrewmate() && (roleBase == null || roleBase.WinnerTeam == WinnerTeamType.Crewmate || (Role == RoleId.SchrodingersCat && GetAbility<SchrodingersCatAbility>()?.CurrentTeam == SchrodingersCatTeam.Crewmate));

    public bool IsCrewmateOrMadRoles()
        => IsCrewmate() || IsMadRoles() || IsFriendRoles();
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool IsImpostor()
        => Data.Role.IsImpostor || GetAbility<SchrodingersCatAbility>()?.CurrentTeam == SchrodingersCatTeam.Impostor;
    public bool IsNeutral()
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Neutral : false;
    public bool IsImpostorWinTeam()
        => IsImpostor() || IsMadRoles() || Role == RoleId.MadKiller;
    public bool IsPavlovsTeam()
        => Role is RoleId.PavlovsDog or RoleId.PavlovsOwner || GetAbility<SchrodingersCatAbility>()?.CurrentTeam is SchrodingersCatTeam.Pavlovs or SchrodingersCatTeam.PavlovFriends;
    public bool IsPavlovsDog()
        => Role == RoleId.PavlovsDog || GetAbility<SchrodingersCatAbility>()?.CurrentTeam == SchrodingersCatTeam.Pavlovs;
    public bool IsMadRoles()
        => HasAbility<MadmateAbility>()
           || GhostRole == GhostRoleId.Revenant
           || (Role == RoleId.SatsumaAndImo && GetAbility<SatsumaAndImoAbility>()?.IsMadTeam == true)
           || (Role == RoleId.VampireDependent)
           || (Role == RoleId.MadKiller && !IsImpostor());
    public bool IsFriendRoles()
        => HasAbility<JFriendAbility>() || (Role == RoleId.Bullet && !WaveCannonJackal.WaveCannonJackalCreateBulletToJackal);
    public bool IsJackal()
        => HasAbility<JackalAbility>() || GetAbility<SchrodingersCatAbility>()?.CurrentTeam == SchrodingersCatTeam.Jackal;
    public bool IsSidekick()
        => HasAbility<JSidekickAbility>() || (Role == RoleId.Bullet && WaveCannonJackal.WaveCannonJackalCreateBulletToJackal);
    public bool IsJackalTeam()
        => IsJackal() || IsSidekick();
    public bool IsJackalTeamWins()
        => IsJackal() || IsSidekick() || IsFriendRoles();
    public bool IsLovers()
        => ModifierRole.HasFlag(ModifierRoleId.Lovers);
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool IsDead()
        => Data == null || Data.Disconnected || Data.IsDead;
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool IsAlive()
        => !IsDead();
    public bool IsTaskTriggerRole()
    {
        var taskAbilities = GetPrioritizedAbilities<CustomTaskAbility>();
        if (taskAbilities != null)
        {
            for (var i = 0; i < taskAbilities.Count; i++)
            {
                var candidate = ((CustomTaskAbility)taskAbilities[i]).IsTaskTrigger?.Invoke();
                if (candidate.HasValue) return candidate.Value;
            }
        }
        return IsCrewmate();
    }
    public bool IsCountTask()
    {
        var taskAbilities = GetPrioritizedAbilities<CustomTaskAbility>();
        if (taskAbilities != null)
        {
            for (var i = 0; i < taskAbilities.Count; i++)
            {
                var candidate = ((CustomTaskAbility)taskAbilities[i]).CountsForCrewWin?.Invoke();
                if (candidate.HasValue) return candidate.Value;
            }
        }
        return IsCrewmate();
    }

    /// <summary>
    /// このプレイヤーの役職をローカルプレイヤーが見えるかどうかを判定します
    /// </summary>
    public bool CanSeeRoleOf(ExPlayerControl otherPlayer)
    {
        if (otherPlayer == null || otherPlayer.Player == null || !otherPlayer.Player.Visible)
        {
            return false;
        }

        // 自分自身の場合は常にtrue
        if (PlayerId == otherPlayer.PlayerId)
        {
            return true;
        }

        // 神は生存中でも全プレイヤーの役職を確認できる。
        if (Role == RoleId.God)
        {
            return true;
        }

        // ローカルプレイヤーが生きている場合はfalse
        if (!IsDead())
        {
            return false;
        }

        // バスカーの偽装死時は他のプレイヤーの役職を見えないようにする
        bool isBuskerFakeDeath = GetAbility<BuskerPseudocideAbility>()?.isEffectActive == true;
        if (isBuskerFakeDeath && PlayerId != otherPlayer.PlayerId)
        {
            return false;
        }

        if (SuperNewRoles.Roles.Impostor.OrpheusMainAbility.ShouldHideGhostRolesFor(PlayerId))
        {
            return false;
        }

        // Local player is ghost
        bool canSeeGhostRoles = !GameSettingOptions.HideGhostRoles ||
                                (IsImpostor() && GameSettingOptions.ShowGhostRolesToImpostor);

        if (!canSeeGhostRoles)
        {
            return false;
        }

        var hideRoleOnGhostAbility = GetAbility<HideRoleOnGhostAbility>();
        if (hideRoleOnGhostAbility != null && hideRoleOnGhostAbility.IsHideRole(otherPlayer))
        {
            return false;
        }

        return true;
    }
    public (int complete, int all) GetAllTaskForShowProgress()
    {
        (int complete, int all) result = ModHelpers.TaskCompletedData(Data);
        return (result.complete, ResolveRequiredTaskCount(result.all));
    }
    public bool CanUseVent()
        => TryGetVentDecision(out _, out var decision) ? decision : CanUseVentVanilla();
    public bool ShowVanillaVentButton()
        => CanUseVentVanilla() && IsAlive() && !TryGetVentDecision(out _, out _);
    internal bool ShouldShowVentAbility(CustomVentAbility ability)
        => TryGetVentDecision(out var selected, out var decision) &&
           ReferenceEquals(selected, ability) && decision;
    public bool CanSabotage()
        => TryGetSabotageDecision(out _, out var decision) ? decision : CanSabotageVanilla();
    public bool ShowVanillaSabotageButton()
        => CanSabotageVanilla() && !TryGetSabotageDecision(out _, out _);
    internal bool ShouldShowSabotageAbility(CustomSaboAbility ability)
        => TryGetSabotageDecision(out var selected, out var decision) &&
           ReferenceEquals(selected, ability) && decision;

    private bool TryGetVentDecision(out CustomVentAbility source, out bool decision)
    {
        var abilities = GetPrioritizedAbilities<CustomVentAbility>();
        if (abilities != null)
        {
            for (var i = 0; i < abilities.Count; i++)
            {
                var ability = (CustomVentAbility)abilities[i];
                var candidate = ability.CanUseVent?.Invoke();
                if (!candidate.HasValue) continue;
                source = ability;
                decision = candidate.Value;
                return true;
            }
        }
        source = null;
        decision = default;
        return false;
    }

    private bool TryGetSabotageDecision(out CustomSaboAbility source, out bool decision)
    {
        var abilities = GetPrioritizedAbilities<CustomSaboAbility>();
        if (abilities != null)
        {
            for (var i = 0; i < abilities.Count; i++)
            {
                var ability = (CustomSaboAbility)abilities[i];
                var candidate = ability.CanSabotage?.Invoke();
                if (!candidate.HasValue) continue;
                source = ability;
                decision = candidate.Value;
                return true;
            }
        }
        source = null;
        decision = default;
        return false;
    }

    private int ResolveRequiredTaskCount(int vanillaTaskCount)
    {
        var abilities = GetPrioritizedAbilities<CustomTaskAbility>();
        if (abilities != null)
        {
            for (var i = 0; i < abilities.Count; i++)
            {
                var candidate = ((CustomTaskAbility)abilities[i]).RequiredTaskCount?.Invoke();
                if (candidate.HasValue) return candidate.Value;
            }
        }
        return vanillaTaskCount;
    }

    public void AssignCustomTasks()
    {
        CustomTaskAbility.AssignTasks(this, ResolveTaskOptions(), GetAbility<CustomTaskTypeAbility>());
    }

    private TaskOptionData ResolveTaskOptions()
    {
        var abilities = GetPrioritizedAbilities<CustomTaskAbility>();
        if (abilities != null)
        {
            for (var i = 0; i < abilities.Count; i++)
            {
                var taskOptions = ((CustomTaskAbility)abilities[i]).TaskOptions?.Invoke();
                if (taskOptions != null) return taskOptions;
            }
        }
        return null;
    }

    private bool CanUseVentVanilla()
        => (IsImpostor() && !ModHelpers.IsHnS()) || Data.Role.Role == RoleTypes.Engineer;

    private bool CanSabotageVanilla()
        => IsImpostor() && !ModHelpers.IsHnS();
    public AbilityBase GetAbility(ulong abilityId)
    {
        return _abilitiesById.TryGetValue(abilityId, out var ability) ? ability : null;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool TryGetAbility<T>(out T result) where T : AbilityBase
    {
        var typeId = TypeCache<T>.TypeId;
        if (_typeIdAbilityCache.TryGetValue(typeId, out var cachedAbility))
        {
            result = cachedAbility as T;
            return result != null;
        }

        var count = _playerAbilities.Count;
        for (var i = 0; i < count; i++)
        {
            if (_playerAbilities[i] is T ability)
            {
                _typeIdAbilityCache[typeId] = ability;
                result = ability;
                return true;
            }
        }

        _typeIdAbilityCache[typeId] = null;
        result = null;
        return false;
    }
    public T GetAbility<T>(ulong abilityId) where T : AbilityBase
    {
        return GetAbility(abilityId) as T;
    }
    private void AttachAbility(AbilityBase ability, ulong abilityId, AbilityParentBase parent)
    {
        Logger.Info("AttachAbility: " + ability.GetType().Name + " to player: " + PlayerId);
        RegisterAbilityState(abilityId, ability);
        SuperTrophyManager.RegisterTrophy(ability);
        ability.Attach(Player, abilityId, parent);
    }
    /// <summary>
    /// アビリティをプレイヤーにアタッチします。
    /// AbilityId は「プレイヤーID/親コンテキスト/アビリティ型/序数」に基づく決定的IDで生成されます。
    /// 同じ親ツリーとライフサイクル順序を再現したクライアント間では、同一の AbilityId になります。
    /// </summary>
    /// <param name="ability">アタッチするアビリティ</param>
    /// <param name="parent">親コンテキスト(Role/Modifier/Ghost/Ability/Player)</param>
    public void AttachAbility(AbilityBase ability, AbilityParentBase parent)
    {
        if (ability == null)
            throw new ArgumentNullException(nameof(ability));
        if (parent == null)
            throw new ArgumentNullException(nameof(parent));

        if (ability.Parent != null)
        {
            var currentOwner = ability.Parent.Player;
            if (currentOwner != null &&
                ReferenceEquals(currentOwner.GetAbility(ability.AbilityId), ability))
            {
                throw new InvalidOperationException(
                    $"Ability {ability.GetType().FullName} is already registered for player {currentOwner.PlayerId}.");
            }
        }

        if (parent is AbilityParentAbility abilityParent &&
            (abilityParent.ParentAbility == null ||
             !ReferenceEquals(GetAbility(abilityParent.ParentAbility.AbilityId), abilityParent.ParentAbility)))
        {
            throw new InvalidOperationException("Parent ability is not registered for this player.");
        }
        if (!ReferenceEquals(parent.Player, this))
            throw new InvalidOperationException(
                $"Ability parent belongs to a different player than {PlayerId}.");

        var abilityType = ability.GetType();
        var ordinal = GetNextAvailableAbilityOrdinal(parent, abilityType);
        var abilityId = ExPlayerControlExtensions.GenerateDeterministicAbilityId(
            PlayerId,
            parent,
            abilityType,
            ordinal);
        AttachAbility(ability, abilityId, parent);
    }
    public bool HasImpostorVision()
    {
        var abilities = GetPrioritizedAbilities<ImpostorVisionAbility>();
        if (abilities != null)
        {
            for (var i = 0; i < abilities.Count; i++)
            {
                var decision = ((ImpostorVisionAbility)abilities[i]).HasImpostorVision?.Invoke();
                if (decision.HasValue) return decision.Value;
            }
        }
        return IsImpostor();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private List<AbilityBase> GetPrioritizedAbilities<T>() where T : AbilityBase, IPrioritizedAbility
    {
        _prioritizedAbilities.TryGetValue(AbilityPriorityGroup<T>.Id, out var abilities);
        return abilities;
    }

    public void DetachAbility(ulong abilityId)
    {
        if (!_abilitiesById.TryGetValue(abilityId, out var ability))
            return;
        ability.Detach();
        SuperTrophyManager.DetachTrophy(ability);
        UnregisterAbilityState(abilityId, ability);
    }
    public T GetAbility<T>() where T : AbilityBase
        => TryGetAbility<T>(out var ability) ? ability : null;
    public IReadOnlyList<T> GetAbilities<T>() where T : AbilityBase
    {
        var typeId = TypeCache<T>.TypeId;

        // ReadOnlyキャッシュをチェック
        if (_typeIdReadOnlyCache.TryGetValue(typeId, out var cachedReadOnly))
        {
            return (IReadOnlyList<T>)cachedReadOnly;
        }

        // 新規作成
        var result = new List<T>();
        var count = _playerAbilities.Count;

        for (var i = 0; i < count; i++)
        {
            if (_playerAbilities[i] is T ability)
            {
                result.Add(ability);
            }
        }

        var readOnlyCollection = result.AsReadOnly();
        _typeIdReadOnlyCache[typeId] = readOnlyCollection;
        return readOnlyCollection;
    }

    /// <summary>優先度を持つ Ability を、自動判定した系統別リストへ登録します。</summary>
    private void AddPrioritizedAbility(AbilityBase ability)
    {
        if (ability is not IPrioritizedAbility prioritizedAbility) return;

        var groupId = AbilityPriorityGroupRegistry.GetGroupId(ability.GetType());
        if (!_prioritizedAbilities.TryGetValue(groupId, out var abilities))
        {
            abilities = new List<AbilityBase>();
            _prioritizedAbilities.Add(groupId, abilities);
        }

        var index = 0;
        while (index < abilities.Count &&
               ((IPrioritizedAbility)abilities[index]).Priority > prioritizedAbility.Priority)
            index++;
        abilities.Insert(index, ability);
    }

    /// <summary>優先度を持つ Ability を、自動判定した系統別リストから解除します。</summary>
    private void RemovePrioritizedAbility(AbilityBase ability)
    {
        if (ability is not IPrioritizedAbility) return;

        var groupId = AbilityPriorityGroupRegistry.GetGroupId(ability.GetType());
        if (!_prioritizedAbilities.TryGetValue(groupId, out var abilities)) return;

        abilities.Remove(ability);
        if (abilities.Count == 0)
            _prioritizedAbilities.Remove(groupId);
    }

    private void RegisterAbilityState(ulong abilityId, AbilityBase ability)
    {
        if (_playerAbilities.Contains(ability))
            throw new InvalidOperationException($"Ability {ability.GetType().FullName} is already registered for player {PlayerId}.");
        if (_abilitiesById.ContainsKey(abilityId))
            throw new InvalidOperationException($"AbilityId {abilityId} is already registered for player {PlayerId}.");
        if (_issuedAbilityIds.Contains(abilityId))
            throw new InvalidOperationException($"AbilityId {abilityId} was already issued for player {PlayerId}.");

        _playerAbilities.Add(ability);
        _abilitiesById.Add(abilityId, ability);
        _issuedAbilityIds.Add(abilityId);
        AddPrioritizedAbility(ability);
        InvalidateAbilityCaches();
    }

    private void UnregisterAbilityState(ulong abilityId, AbilityBase ability)
    {
        RemovePrioritizedAbility(ability);
        _playerAbilities.Remove(ability);
        _abilitiesById.Remove(abilityId);
        InvalidateAbilityCaches();
    }

    private void ClearAbilityState()
    {
        _playerAbilities.Clear();
        _abilitiesById.Clear();
        _issuedAbilityIds.Clear();
        _prioritizedAbilities.Clear();
        InvalidateAbilityCaches();
    }

    private void InvalidateAbilityCaches()
    {
        _typeIdAbilityCache.Clear();
        _typeIdReadOnlyCache.Clear();
    }

    private int GetNextAvailableAbilityOrdinal(
        AbilityParentBase parent,
        Type abilityType)
    {
        for (var ordinal = 0; ordinal <= 0xFFF; ordinal++)
        {
            var abilityId = ExPlayerControlExtensions.GenerateDeterministicAbilityId(
                PlayerId,
                parent,
                abilityType,
                ordinal);
            if (!_issuedAbilityIds.Contains(abilityId))
                return ordinal;
        }

        throw new InvalidOperationException(
            $"Ability ordinal exhausted for player {PlayerId}, parent {ExPlayerControlExtensions.GetParentSignature(parent)}, type {abilityType.FullName}.");
    }

    public override string ToString()
    {
        return $"{Data?.PlayerName}({PlayerId}): {Role} {AbilityCount}";
    }
}
public static class ExPlayerControlExtensions
{
    public static void AddAbility(this ExPlayerControl player, AbilityBase ability, AbilityParentBase parent)
    {
        player.AttachAbility(ability, parent);
    }
    /// <summary>
    /// 決定的な AbilityId を生成します。
    /// 上位8bit: プレイヤーID, 次の16bit: 親コンテキストID(Role/Modifier/Ghost。該当なしは0)
    /// 下位40bit: (親シグネチャ + アビリティ型名)のFNV-1a(64)下位28bit + 同一親・同一型内での序数(12bit)
    /// </summary>
    /// <param name="playerId">プレイヤーID</param>
    /// <param name="parent">親コンテキスト(ロール/モディファイア/ゴースト/親アビリティ/プレイヤー)</param>
    /// <param name="abilityType">アビリティの型</param>
    /// <param name="ordinal">同一シグネチャ内で割り当てた序数</param>
    /// <returns>決定的な AbilityId</returns>
    internal static ulong GenerateDeterministicAbilityId(
        byte playerId,
        AbilityParentBase parent,
        Type abilityType,
        int ordinal)
    {
        if (ordinal is < 0 or > 0xFFF)
            throw new ArgumentOutOfRangeException(nameof(ordinal));

        ulong pid = playerId;
        ulong roleCode = 0;
        string parentSig = GetParentSignature(parent);
        if (parent is AbilityParentRole apr && apr.ParentRole != null)
        {
            roleCode = (ulong)(int)apr.ParentRole.Role & 0xFFFFUL;
        }
        else if (parent is AbilityParentModifier apm && apm.ParentModifier != null)
        {
            roleCode = (ulong)(int)apm.ParentModifier.ModifierRole & 0xFFFFUL;
        }
        else if (parent is AbilityParentGhostRole apg && apg.ParentGhostRole != null)
        {
            roleCode = (ulong)(int)apg.ParentGhostRole.Role & 0xFFFFUL;
        }

        // 親シグネチャと型名からシグネチャ文字列を生成
        string signature = parentSig + "|" + (abilityType.FullName ?? abilityType.Name);
        ulong sigHash = Fnv1a64(signature) & 0x0FFFFFFFUL; // 28-bit
        ulong ord = (ulong)ordinal; // 12-bit
        ulong lower40 = (sigHash << 12) | ord;
        return (pid << 56) | (roleCode << 40) | lower40;
    }

    /// <summary>
    /// FNV-1a(64bit) ハッシュ関数（簡易・高速）。
    /// AbilityId 生成のためのシグネチャハッシュに利用します。
    /// </summary>
    /// <param name="text">ハッシュ対象文字列</param>
    /// <returns>64bitハッシュ値</returns>
    private static ulong Fnv1a64(string text)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        ulong hash = offset;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
        for (int i = 0; i < bytes.Length; i++)
        {
            hash ^= bytes[i];
            hash *= prime;
        }
        return hash;
    }

    /// <summary>
    /// 親コンテキストを文字列シグネチャ化します。
    /// Role/Modifier/Ghost はそれぞれ "R:"/"M:"/"G:" にIDを付与。
    /// Ability 親は "A:" + 親アビリティID、プレイヤー直下は "P"、不明は "U"。
    /// </summary>
    /// <param name="parent">親コンテキスト</param>
    /// <returns>親シグネチャ</returns>
    internal static string GetParentSignature(AbilityParentBase parent)
    {
        switch (parent)
        {
            case AbilityParentRole apr when apr.ParentRole != null:
                return "R:" + (int)apr.ParentRole.Role;
            case AbilityParentModifier apm when apm.ParentModifier != null:
                return "M:" + (int)apm.ParentModifier.ModifierRole;
            case AbilityParentGhostRole apg when apg.ParentGhostRole != null:
                return "G:" + (int)apg.ParentGhostRole.Role;
            case AbilityParentAbility apa when apa.ParentAbility != null:
                return "A:" + apa.ParentAbility.AbilityId;
            case AbilityParentPlayer:
                return "P";
            default:
                return "U";
        }
    }
}
