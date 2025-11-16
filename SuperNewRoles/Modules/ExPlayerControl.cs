using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Ability;
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
    public IRoleBase roleBase { get; private set; }
    public List<IModifierBase> ModifierRoleBases { get; private set; } = new();
    public IGhostRoleBase GhostRoleBase { get; private set; }
    public List<AbilityBase> PlayerAbilities { get; private set; } = new();
    public Dictionary<ulong, AbilityBase> PlayerAbilitiesDictionary { get; private set; } = new();
    private Dictionary<string, AbilityBase> _abilityCache = new();
    public TextMeshPro PlayerInfoText { get; set; }
    public TextMeshPro MeetingInfoText { get; set; }
    public PlayerVoteArea VoteArea { get; set; }
    public int lastAbilityId { get; set; }
    private FinalStatus? _finalStatus;
    public FinalStatus FinalStatus { get { return _finalStatus ?? FinalStatus.Alive; } set { _finalStatus = value; } }

    private CustomVentAbility _customVentAbility;
    private CustomSaboAbility _customSaboAbility;
    private CustomTaskAbility _customTaskAbility;
    private CustomKillButtonAbility _customKillButtonAbility;
    private KillableAbility _killableAbility;
    public CustomTaskAbility CustomTaskAbility => _customTaskAbility;
    private List<ImpostorVisionAbility> _impostorVisionAbilities = new();
    private Dictionary<string, bool> _hasAbilityCache = new();

    // パフォーマンス最適化用キャッシュ
    private readonly Dictionary<int, AbilityBase> _typeIdAbilityCache = new();
    private readonly Dictionary<int, List<AbilityBase>> _typeIdAbilitiesCache = new();
    private readonly Dictionary<int, IReadOnlyList<object>> _typeIdReadOnlyCache = new();

    // 型IDキャッシュ用配列。1024種類を超える型はキャッシュされないが、動作は継続する
    private readonly bool[] _hasAbilityByTypeId = new bool[1024]; // 最大1024種類のアビリティタイプを想定
    private readonly bool[] _hasAbilityByTypeIdCached = new bool[1024];

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
        this.lastAbilityId = 0;
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
    public bool HasAbility(string abilityName)
    {
        if (_hasAbilityCache.TryGetValue(abilityName, out bool hasAbility))
        {
            return hasAbility;
        }

        hasAbility = PlayerAbilities.Any(x => x.GetType().Name == abilityName);
        _hasAbilityCache[abilityName] = hasAbility;
        return hasAbility;
    }
    public bool HasAbility<T>() where T : AbilityBase
    {
        var typeId = TypeCache<T>.TypeId;
        if (typeId < 1024 && _hasAbilityByTypeIdCached[typeId])
            return _hasAbilityByTypeId[typeId];

        var count = PlayerAbilities.Count;
        for (var i = 0; i < count; i++)
        {
            if (PlayerAbilities[i] is T)
            {
                if (typeId < 1024)
                {
                    _hasAbilityByTypeId[typeId] = true;
                    _hasAbilityByTypeIdCached[typeId] = true;
                }
                return true;
            }
        }

        if (typeId < 1024)
        {
            _hasAbilityByTypeId[typeId] = false;
            _hasAbilityByTypeIdCached[typeId] = true;
        }
        return false;
    }
    public bool IsTaskComplete()
    {
        (int completed, int total) = ModHelpers.TaskCompletedData(Data);
        if (_customTaskAbility == null) return completed >= total;
        var (isTaskTrigger, countTask, all) = _customTaskAbility.CheckIsTaskTrigger() ?? (false, false, total);
        return isTaskTrigger && completed >= (all ?? total);
    }
    public void ResetKillCooldown()
    {
        if (!AmOwner) return;
        if (FastDestroyableSingleton<HudManager>.Instance.KillButton.isActiveAndEnabled)
        {
            float coolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
            SetKillTimerUnchecked(coolTime, coolTime);
        }
        PlayerAbilities.ForEach(x =>
        {
            if (x is CustomKillButtonAbility customKillButtonAbility)
            {
                customKillButtonAbility.ResetTimer();
            }
        });
    }
    public void SetKillTimerUnchecked(float time, float maxTime)
    {
        Player.killTimer = Mathf.Clamp(time, 0f, maxTime);
        FastDestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(Player.killTimer, maxTime);
    }
    public void SetModifierRole(ModifierRoleId modifierRoleId)
    {
        if (ModifierRole.HasFlag(modifierRoleId)) return;
        if (AmOwner)
            SuperTrophyManager.DetachTrophy(Role);
        ModifierRole |= modifierRoleId;
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
        DetachOldGhostRole(GhostRole);
        if (AmOwner && GhostRole != GhostRoleId.None)
            SuperTrophyManager.DetachTrophy(GhostRole);
        GhostRole = ghostRoleId;
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
    public void SetRole(RoleId roleId)
    {
        if (Role == roleId) return;
        RoleId oldRole = Role;
        DetachOldRole(Role);
        if (AmOwner)
            SuperTrophyManager.DetachTrophy(Role);
        Role = roleId;
        Logger.Info($"[SetRole] Player {Player?.name} ({PlayerId}) changing role from {oldRole} to {roleId}, AmOwner: {AmOwner}");
        if (CustomRoleManager.TryGetRoleById(roleId, out var role))
        {
            Logger.Info($"[SetRole] Calling OnSetRole for player {Player?.name} ({PlayerId})");
            role.OnSetRole(Player);
            Logger.Info($"[SetRole] OnSetRole completed, PlayerAbilities count: {PlayerAbilities.Count}");
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
    }

    public bool HasCustomKillButton()
    {
        return _customKillButtonAbility != null;
    }
    public bool showKillButtonVanilla()
    {
        return IsImpostor() && IsAlive() && (_killableAbility == null || _killableAbility.CanKill);
    }
    private void DetachOldRole(RoleId roleId)
    {
        List<AbilityBase> abilitiesToDetach = new();
        List<AbilityParentRole> abilitiesToDetachParentRole = new();
        foreach (var ability in PlayerAbilities)
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
        foreach (var ability in PlayerAbilities)
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
        foreach (var ability in PlayerAbilities)
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
        ModifierRole &= ~modifierRoleId;
        ModifierRoleBases.RemoveAll(x => modifierRoleId.HasFlag(x.ModifierRole));
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
    public void ReverseRole(ExPlayerControl target)
    {
        if (target == null || target.Player == null) return;

        // 自分と相手のAbilitiesとRoleを保存
        List<(AbilityBase ability, ulong abilityId)> myAbilities = new();
        List<(AbilityBase ability, ulong abilityId)> targetAbilities = new();

        // ToArray()を使わずに直接リストから収集
        foreach (var ability in PlayerAbilities)
        {
            if (ability != null && ability.Parent != null && ability.Parent is not AbilityParentPlayer)
            {
                myAbilities.Add((ability, ability.AbilityId));
            }
        }

        foreach (var ability in target.PlayerAbilities)
        {
            if (ability != null && ability.Parent != null && ability.Parent is not AbilityParentPlayer)
            {
                targetAbilities.Add((ability, ability.AbilityId));
            }
        }

        // 両方のプレイヤーのRoleを保存
        RoleId myRole = Role;
        RoleId targetRole = target.Role;

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

        Role = targetRole;
        roleBase = target.roleBase;
        target.Role = myRole;
        target.roleBase = roleBase;

        // アタッチする
        foreach (var ability in myAbilities)
        {
            var currentParent = ability.ability.Parent;
            if (currentParent is not AbilityParentRole)
                continue;
            currentParent.Player = Player;
            target.AttachAbility(ability.ability, currentParent);
        }
        foreach (var ability in targetAbilities)
        {
            var currentParent = ability.ability.Parent;
            if (currentParent is not AbilityParentRole)
                continue;
            currentParent.Player = Player;
            AttachAbility(ability.ability, currentParent);
        }
        // 名前情報を更新
        NameText.UpdateAllNameInfo();
    }
    public void Disconnected()
    {
        _exPlayerControls.Remove(this);
        _exPlayerControlsArray[PlayerId] = null;
        foreach (var ability in PlayerAbilities)
        {
            ability.Detach();
        }
        PlayerAbilities.Clear();
        PlayerAbilitiesDictionary.Clear();
        _hasAbilityCache.Clear();
        _typeIdAbilityCache.Clear();
        _typeIdAbilitiesCache.Clear();
        _typeIdReadOnlyCache.Clear();
        System.Array.Clear(_hasAbilityByTypeIdCached, 0, _hasAbilityByTypeIdCached.Length);
        System.Array.Clear(_hasAbilityByTypeId, 0, _hasAbilityByTypeId.Length);
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
        => IsImpostor() || IsPavlovsDog() || Role == RoleId.MadKiller || IsJackal() || HasCustomKillButton() || Role == RoleId.Hitman;

    public bool IsNonCrewKiller()
        => IsKiller() && !IsCrewmate();

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public bool IsCrewmate()
        => roleBase != null ? (Role == RoleId.SchrodingersCat && GetAbility<SchrodingersCatAbility>()?.CurrentTeam == SchrodingersCatTeam.Crewmate) || (roleBase.AssignedTeam == AssignedTeamType.Crewmate && !IsMadRoles() && !IsFriendRoles()) : !Data.Role.IsImpostor;

    public bool IsCrewmateWin()
        => IsCrewmate() && (roleBase == null || roleBase.WinnerTeam == WinnerTeamType.Crewmate);

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
        => HasAbility(nameof(MadmateAbility))
           || GhostRole == GhostRoleId.Revenant
           || (Role == RoleId.SatsumaAndImo && GetAbility<SatsumaAndImoAbility>()?.IsMadTeam == true)
           || (Role == RoleId.VampireDependent);
    public bool IsFriendRoles()
        => HasAbility(nameof(JFriendAbility)) || (Role == RoleId.Bullet && !WaveCannonJackal.WaveCannonJackalCreateBulletToJackal);
    public bool IsJackal()
        => HasAbility(nameof(JackalAbility)) || GetAbility<SchrodingersCatAbility>()?.CurrentTeam == SchrodingersCatTeam.Jackal;
    public bool IsSidekick()
        => HasAbility(nameof(JSidekickAbility)) || (Role == RoleId.Bullet && WaveCannonJackal.WaveCannonJackalCreateBulletToJackal);
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
        bool hasDefinitiveFalseFromCustom = false;
        foreach (var cta in PlayerAbilities.OfType<CustomTaskAbility>())
        {
            var triggerCheck = cta.CheckIsTaskTrigger();
            if (triggerCheck != null)
            {
                if (triggerCheck.Value.isTaskTrigger)
                {
                    return true;
                }
                hasDefinitiveFalseFromCustom = true;
            }
        }

        if (hasDefinitiveFalseFromCustom)
        {
            return false;
        }
        return IsCrewmate();
    }
    public bool IsCountTask()
        => _customTaskAbility != null ? _customTaskAbility.CheckIsTaskTrigger()?.countTask ?? IsCrewmate() : IsCrewmate();
    public (int complete, int all) GetAllTaskForShowProgress()
    {
        (int complete, int all) result = ModHelpers.TaskCompletedData(Data);
        if (_customTaskAbility == null)
        {
            return result;
        }
        var (isTaskTrigger, countTask, all) = _customTaskAbility.CheckIsTaskTrigger() ?? (false, false, result.all);
        return (result.complete, all ?? result.all);
    }
    public bool CanUseVent()
        => _customVentAbility != null ? _customVentAbility.CheckCanUseVent() : (IsImpostor() && !ModHelpers.IsHnS()) || Data.Role.Role == RoleTypes.Engineer;
    public bool ShowVanillaVentButton()
        => (IsImpostor() && !ModHelpers.IsHnS()) && IsAlive() && _customVentAbility == null;
    public bool CanSabotage()
        => _customSaboAbility != null ? _customSaboAbility.CheckCanSabotage() : (IsImpostor() && !ModHelpers.IsHnS());
    public bool ShowVanillaSabotageButton()
        => (IsImpostor() && !ModHelpers.IsHnS()) && _customSaboAbility == null;
    public AbilityBase GetAbility(ulong abilityId)
    {
        return PlayerAbilitiesDictionary.TryGetValue(abilityId, out var ability) ? ability : null;
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

        var count = PlayerAbilities.Count;
        for (var i = 0; i < count; i++)
        {
            if (PlayerAbilities[i] is T ability)
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
        PlayerAbilities.Add(ability);
        PlayerAbilitiesDictionary.Add(abilityId, ability);
        _abilityCache[ability.GetType().Name] = ability;
        SuperTrophyManager.RegisterTrophy(ability);
        switch (ability)
        {
            case CustomVentAbility customVentAbility:
                _customVentAbility = customVentAbility;
                break;
            case ImpostorVisionAbility impostorVisionAbility:
                _impostorVisionAbilities.Add(impostorVisionAbility);
                break;
            case CustomSaboAbility customSaboAbility:
                _customSaboAbility = customSaboAbility;
                break;
            case CustomTaskAbility customTaskAbility:
                _customTaskAbility = customTaskAbility;
                break;
            case CustomKillButtonAbility customKillButtonAbility:
                _customKillButtonAbility = customKillButtonAbility;
                break;
            case KillableAbility killableAbility:
                _killableAbility = killableAbility;
                break;
        }
        ability.Attach(Player, abilityId, parent);
        _hasAbilityCache.Clear();

        // 新しいキャッシュシステムのクリア（最適化版）
        _typeIdAbilityCache.Clear();
        _typeIdAbilitiesCache.Clear();
        _typeIdReadOnlyCache.Clear();
        System.Array.Clear(_hasAbilityByTypeIdCached, 0, _hasAbilityByTypeIdCached.Length);
    }
    /// <summary>
    /// アビリティをプレイヤーにアタッチします。
    /// AbilityId は「プレイヤーID/親コンテキスト/アビリティ型/序数」に基づく決定的IDで生成されます。
    /// これにより、環境差や付与順の差異があっても送受信で同一の AbilityId が保証されます。
    /// </summary>
    /// <param name="ability">アタッチするアビリティ</param>
    /// <param name="parent">親コンテキスト(Role/Modifier/Ghost/Ability/Player)</param>
    public void AttachAbility(AbilityBase ability, AbilityParentBase parent)
    {
        // 決定的な AbilityId を生成してからアタッチ
        AttachAbility(ability, ExPlayerControlExtensions.GenerateDeterministicAbilityId(PlayerId, parent, ability.GetType()), parent);
    }
    public bool HasImpostorVision()
    {
        if (_impostorVisionAbilities.Count == 0) return IsImpostor();
        return _impostorVisionAbilities.FirstOrDefault(x => x.HasImpostorVision?.Invoke() == true) != null;
    }
    public void DetachAbility(ulong abilityId)
    {
        if (!PlayerAbilitiesDictionary.TryGetValue(abilityId, out var ability))
            return;
        ability.Detach();
        SuperTrophyManager.DetachTrophy(ability);
        switch (ability)
        {
            case CustomVentAbility customVentAbility:
                _customVentAbility = null;
                break;
            case ImpostorVisionAbility impostorVisionAbility:
                _impostorVisionAbilities.Remove(impostorVisionAbility);
                break;
            case CustomSaboAbility customSaboAbility:
                _customSaboAbility = null;
                break;
            case CustomTaskAbility customTaskAbility:
                _customTaskAbility = null;
                break;
            case CustomKillButtonAbility customKillButtonAbility:
                _customKillButtonAbility = null;
                break;
            case KillableAbility killableAbility:
                _killableAbility = null;
                break;
        }
        PlayerAbilities.Remove(ability);
        PlayerAbilitiesDictionary.Remove(abilityId);
        _abilityCache.Remove(ability.GetType().Name);
        _hasAbilityCache.Clear();

        // 新しいキャッシュシステムのクリア（最適化版）
        _typeIdAbilityCache.Clear();
        _typeIdAbilitiesCache.Clear();
        _typeIdReadOnlyCache.Clear();
        System.Array.Clear(_hasAbilityByTypeIdCached, 0, _hasAbilityByTypeIdCached.Length);
    }
    public T GetAbility<T>() where T : AbilityBase
    {
        var typeId = TypeCache<T>.TypeId;
        if (_typeIdAbilityCache.TryGetValue(typeId, out var cachedAbility))
            return cachedAbility as T;

        var count = PlayerAbilities.Count;
        for (var i = 0; i < count; i++)
        {
            if (PlayerAbilities[i] is T ability)
            {
                _typeIdAbilityCache[typeId] = ability;
                return ability;
            }
        }

        _typeIdAbilityCache[typeId] = null;
        return null;
    }
    public IReadOnlyList<T> GetAbilities<T>() where T : AbilityBase
    {
        var typeId = TypeCache<T>.TypeId;

        // ReadOnlyキャッシュをチェック
        if (_typeIdReadOnlyCache.TryGetValue(typeId, out var cachedReadOnly))
        {
            return (IReadOnlyList<T>)cachedReadOnly;
        }

        // 既存のキャッシュをチェック
        if (_typeIdAbilitiesCache.TryGetValue(typeId, out var cachedList))
        {
            var typedList = new List<T>(cachedList.Count);
            for (var i = 0; i < cachedList.Count; i++)
                typedList.Add((T)cachedList[i]);

            var readOnlyResult = typedList.AsReadOnly();
            _typeIdReadOnlyCache[typeId] = readOnlyResult;
            return readOnlyResult;
        }

        // 新規作成
        var result = new List<T>();
        var cacheList = new List<AbilityBase>();
        var count = PlayerAbilities.Count;

        for (var i = 0; i < count; i++)
        {
            if (PlayerAbilities[i] is T ability)
            {
                result.Add(ability);
                cacheList.Add(ability);
            }
        }

        _typeIdAbilitiesCache[typeId] = cacheList;
        var readOnlyCollection = result.AsReadOnly();
        _typeIdReadOnlyCache[typeId] = readOnlyCollection;
        return readOnlyCollection;
    }
    public override string ToString()
    {
        return $"{Data?.PlayerName}({PlayerId}): {Role} {PlayerAbilities.Count}";
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
    /// <returns>決定的な AbilityId</returns>
    public static ulong GenerateDeterministicAbilityId(byte playerId, AbilityParentBase parent, Type abilityType)
    {
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

        // 同一親コンテキスト内の同一型アビリティ数(既存数)を数えて序数に反映
        int ordinal = 0;
        var exPlayer = parent.Player;
        if (exPlayer != null)
        {
            var abilities = exPlayer.PlayerAbilities;
            for (int i = 0; i < abilities.Count; i++)
            {
                var a = abilities[i];
                if (a != null && a.GetType() == abilityType)
                {
                    if (GetParentSignature(a.Parent) == parentSig)
                    {
                        ordinal++;
                    }
                }
            }
        }

        // 親シグネチャと型名からシグネチャ文字列を生成
        string signature = parentSig + "|" + (abilityType.FullName ?? abilityType.Name);
        ulong sigHash = Fnv1a64(signature) & 0x0FFFFFFFUL; // 28-bit
        ulong ord = (ulong)(ordinal & 0xFFF); // 12-bit
        ulong lower40 = (sigHash << 12) | ord;
        ulong id = (pid << 56) | (roleCode << 40) | lower40;
        return id;
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
    private static string GetParentSignature(AbilityParentBase parent)
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