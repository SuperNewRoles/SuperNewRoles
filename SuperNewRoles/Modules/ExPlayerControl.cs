using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.Ability;
using SuperNewRoles.Events;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.SuperTrophies;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

public class ExPlayerControl
{
    public static ExPlayerControl LocalPlayer => _localPlayer;
    private static ExPlayerControl _localPlayer;
    private static List<ExPlayerControl> _exPlayerControls { get; } = new();
    public static List<ExPlayerControl> ExPlayerControls => _exPlayerControls;
    private static ExPlayerControl[] _exPlayerControlsArray;
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

    public override int GetHashCode()
    {
        return PlayerId;
    }
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
    public static implicit operator PlayerControl(ExPlayerControl exPlayer)
    {
        if (exPlayer == null) return null;
        return exPlayer.Player;
    }
    public static implicit operator ExPlayerControl(PlayerControl player)
    {
        if (player == null) return null;
        return ById(player.PlayerId);
    }
    public static implicit operator ExPlayerControl(PlayerVoteArea player)
    {
        if (player == null) return null;
        return ById(player.TargetPlayerId);
    }
    public static implicit operator ExPlayerControl(NetworkedPlayerInfo data)
    {
        if (data == null) return null;
        return ById(data.PlayerId);
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
        if (_hasAbilityCache.TryGetValue(typeof(T).Name, out bool hasAbility))
        {
            return hasAbility;
        }

        hasAbility = PlayerAbilities.Any(x => x is T);
        _hasAbilityCache[typeof(T).Name] = hasAbility;
        return hasAbility;
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
        if (CustomRoleManager.TryGetRoleById(roleId, out var role))
        {
            role.OnSetRole(Player);
            if (AmOwner)
                SuperTrophyManager.RegisterTrophy(Role);
            roleBase = role;
            foreach (var modifier in ModifierRoleBases.ToArray())
            {
                Logger.Info($"ModifierRole: {modifier.ModifierRole} AssignedTeams: {string.Join(",", modifier.AssignedTeams)} RoleBaseAssignedTeam: {roleBase.AssignedTeam}", "ExPlayerControl");
                if (modifier.AssignedTeams.Count != 0 && !modifier.AssignedTeams.Contains(roleBase.AssignedTeam))
                    DetachOldModifierRole(modifier.ModifierRole);
            }
        }
        else
        {
            Logger.Error($"Role {roleId} not found");
        }
        SetRoleEvent.Invoke(this, oldRole, roleId);
    }

    public bool HasCustomKillButton()
    {
        return _customKillButtonAbility != null;
    }
    public bool showKillButtonVanilla()
    {
        return IsImpostor() && IsAlive() && _customKillButtonAbility == null && (_killableAbility == null || _killableAbility.CanKill);
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
                    case AbilityParentRole parentRole when parentRole.ParentRole.Role == roleId:
                        abilitiesToDetach.Add(ability);
                        abilitiesToDetachParentRole.Add(parentRole);
                        parent = null;
                        break;
                    case AbilityParentAbility parentAbility:
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
                    case AbilityParentGhostRole parentGhostRole when parentGhostRole.ParentGhostRole.Role == ghostRoleId:
                        abilitiesToDetach.Add(ability);
                        parent = null;
                        break;
                    case AbilityParentAbility parentAbility:
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
                    case AbilityParentModifier parentModifier when modifierRoleId.HasFlag(parentModifier.ParentModifier.ModifierRole):
                        abilitiesToDetach.Add(ability);
                        parent = null;
                        break;
                    case AbilityParentAbility parentAbility:
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
    public void ReverseRole(ExPlayerControl target)
    {
        if (target == null || target.Player == null) return;

        // 自分と相手のAbilitiesとRoleを保存
        List<(AbilityBase ability, ulong abilityId)> myAbilities = new();
        List<(AbilityBase ability, ulong abilityId)> targetAbilities = new();

        foreach (var ability in PlayerAbilities.ToArray())
        {
            if (ability != null && ability.Parent != null && ability.Parent is not AbilityParentPlayer)
            {
                myAbilities.Add((ability, ability.AbilityId));
            }
        }

        foreach (var ability in target.PlayerAbilities.ToArray())
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
        _localPlayer = _exPlayerControlsArray[PlayerControl.LocalPlayer.PlayerId];
        DisconnectEvent.Instance.AddListener(x =>
        {
            ExPlayerControl exPlayer = (ExPlayerControl)x.disconnectedPlayer;
            if (exPlayer != null)
                exPlayer.Disconnected();
        });
    }
    public static ExPlayerControl ById(byte playerId)
    {
        if (_exPlayerControlsArray == null) return null;
        return _exPlayerControlsArray[playerId];
    }
    public bool IsKiller()
        => IsImpostor() || IsPavlovsDog() || Role == RoleId.MadKiller || IsJackal() || HasCustomKillButton() || Role == RoleId.Hitman;

    public bool IsNonCrewKiller()
        => IsKiller() && !IsCrewmate();

    public bool IsCrewmate()
        => roleBase != null ? (Role == RoleId.SchrodingersCat && GetAbility<SchrodingersCatAbility>()?.CurrentTeam == SchrodingersCatTeam.Crewmate) || (roleBase.AssignedTeam == AssignedTeamType.Crewmate && !IsMadRoles() && !IsFriendRoles()) : !Data.Role.IsImpostor;
    public bool IsCrewmateOrMadRoles()
        => IsCrewmate() || IsMadRoles() || IsFriendRoles();
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
           || (Role == RoleId.SatsumaAndImo && GetAbility<SatsumaAndImoAbility>()?.IsMadTeam == true);
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
    public bool IsDead()
        => Data == null || Data.Disconnected || Data.IsDead;
    public bool IsAlive()
        => !IsDead();
    public bool IsTaskTriggerRole()
        => _customTaskAbility != null ? _customTaskAbility.CheckIsTaskTrigger()?.isTaskTrigger ?? IsCrewmate() : IsCrewmate();
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
    public bool TryGetAbility<T>(out T result) where T : AbilityBase
    {
        result = null;
        if (!_abilityCache.TryGetValue(typeof(T).Name, out var ability))
            return false;
        result = (T)ability;
        return true;
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
    }
    public void AttachAbility(AbilityBase ability, AbilityParentBase parent)
    {
        AttachAbility(ability, IRoleBase.GenerateAbilityId(PlayerId, Role, lastAbilityId++), parent);
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
    }
    public T GetAbility<T>() where T : AbilityBase
    {
        return _abilityCache.TryGetValue(typeof(T).Name, out var ability) ? ability as T : null;
    }
    public List<T> GetAbilities<T>() where T : AbilityBase
    {
        return PlayerAbilities.Where(x => x is T).Cast<T>().ToList();
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
    public static ulong GenerateAbilityId(byte playerId, RoleId role, int abilityIndex)
    {
        return (ulong)(playerId * 1000000) + (ulong)((int)role * 1000) + (ulong)abilityIndex;
    }
}