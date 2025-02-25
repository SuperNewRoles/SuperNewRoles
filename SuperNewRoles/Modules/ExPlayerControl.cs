using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Neutral;
using TMPro;

namespace SuperNewRoles.Modules;

public class ExPlayerControl
{
    public static ExPlayerControl LocalPlayer => _localPlayer;
    private static ExPlayerControl _localPlayer;
    private static List<ExPlayerControl> _exPlayerControls { get; } = new();
    public static IReadOnlyList<ExPlayerControl> ExPlayerControls => _exPlayerControls.AsReadOnly();
    private static ExPlayerControl[] _exPlayerControlsArray;
    public static IReadOnlyCollection<ExPlayerControl> ExPlayerControlsArray => _exPlayerControlsArray;
    public PlayerControl Player { get; }
    public NetworkedPlayerInfo Data { get; }
    public byte PlayerId { get; }
    public bool AmOwner { get; private set; }
    public RoleId Role { get; private set; }
    public IRoleBase roleBase { get; private set; }
    public List<AbilityBase> PlayerAbilities { get; private set; } = new();
    public Dictionary<ulong, AbilityBase> PlayerAbilitiesDictionary { get; private set; } = new();
    public ExPlayerControl Parent { get; set; }
    public TextMeshPro PlayerInfoText { get; set; }
    public TextMeshPro MeetingInfoText { get; set; }
    public int lastAbilityId { get; set; }
    private FinalStatus? _finalStatus;
    public FinalStatus FinalStatus { get { return _finalStatus ?? FinalStatus.Alive; } set { _finalStatus = value; } }

    private CustomVentAbility _customVentAbility;
    private CustomSaboAbility _customSaboAbility;
    private List<ImpostorVisionAbility> _impostorVisionAbilities = new();
    private Dictionary<string, bool> _hasAbilityCache = new();

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
    public void SetRole(RoleId roleId)
    {
        if (Role == roleId) return;
        DetachOldRole(Role);
        Role = roleId;
        if (CustomRoleManager.TryGetRoleById(roleId, out var role))
        {
            role.OnSetRole(Player);
            roleBase = role;
        }
        else
        {
            Logger.Error($"Role {roleId} not found");
        }
    }
    private void DetachOldRole(RoleId roleId)
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
                    case AbilityParentRole parentRole when parentRole.ParentRole.Role == roleId:
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
            ability.Detach();
            PlayerAbilities.Remove(ability);
            PlayerAbilitiesDictionary.Remove(ability.AbilityId);
            _hasAbilityCache.Remove(ability.GetType().Name);
        }
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
        _exPlayerControlsArray = new ExPlayerControl[255];
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
        return _exPlayerControlsArray[playerId];
    }
    public bool IsCrewmate()
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Crewmate && !IsMadRoles() : !Data.Role.IsImpostor;
    public bool IsImpostor()
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Impostor : Data.Role.IsImpostor;
    public bool IsNeutral()
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Neutral : false;
    public bool IsImpostorWinTeam()
        => IsImpostor() || IsMadRoles();
    // TODO: 後でMADロールを追加したらここに追加する
    public bool IsMadRoles()
        => HasAbility(nameof(MadmateAbility));
    public bool IsFriendRoles()
        => false;
    public bool IsJackal()
        => HasAbility(nameof(JackalAbility));
    public bool IsSidekick()
        => HasAbility(nameof(JSidekickAbility));
    public bool IsJackalTeam()
        => IsJackal() || IsSidekick();
    // TODO: 後で追加する
    public bool IsLovers()
        => false;
    public bool IsDead()
        => Data == null || Data.Disconnected || Data.IsDead;
    public bool IsAlive()
        => !IsDead();
    // TODO: 後で書く
    public bool IsTaskTriggerRole()
        => roleBase != null ? IsCrewmate() : IsCrewmate();
    public bool CanUseVent()
        => _customVentAbility != null ? _customVentAbility.CheckCanUseVent() : IsImpostor();
    public bool CanSabotage()
        => _customSaboAbility != null ? _customSaboAbility.CheckCanSabotage() : IsImpostor();
    public AbilityBase GetAbility(ulong abilityId)
    {
        return PlayerAbilitiesDictionary.TryGetValue(abilityId, out var ability) ? ability : null;
    }
    private void AttachAbility(AbilityBase ability, ulong abilityId, AbilityParentBase parent)
    {
        PlayerAbilities.Add(ability);
        PlayerAbilitiesDictionary.Add(abilityId, ability);
        if (ability is CustomVentAbility customVentAbility)
        {
            _customVentAbility = customVentAbility;
        }
        else if (ability is ImpostorVisionAbility impostorVisionAbility)
        {
            _impostorVisionAbilities.Add(impostorVisionAbility);
        }
        else if (ability is CustomSaboAbility customSaboAbility)
        {
            _customSaboAbility = customSaboAbility;
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
        if (PlayerAbilitiesDictionary.TryGetValue(abilityId, out var ability))
        {
            ability.Detach();
        }
        if (ability is CustomVentAbility customVentAbility)
        {
            _customVentAbility = null;
        }
        else if (ability is ImpostorVisionAbility impostorVisionAbility)
        {
            _impostorVisionAbilities.Remove(impostorVisionAbility);
        }
        else if (ability is CustomSaboAbility customSaboAbility)
        {
            _customSaboAbility = null;
        }
        PlayerAbilities.Remove(ability);
        PlayerAbilitiesDictionary.Remove(abilityId);
        _hasAbilityCache.Clear();
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