using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
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
    public RoleId Role { get; private set; }
    public IRoleBase roleBase { get; private set; }
    public List<AbilityBase> PlayerAbilities { get; private set; } = new();
    public Dictionary<ulong, AbilityBase> PlayerAbilitiesDictionary { get; private set; } = new();

    public TextMeshPro PlayerInfoText { get; set; }
    public TextMeshPro MeetingInfoText { get; set; }
    public int lastAbilityId { get; set; }
    private FinalStatus? _finalStatus;
    public FinalStatus FinalStatus { get { return _finalStatus ?? FinalStatus.Alive; } set { _finalStatus = value; } }

    private CustomVentAbility _customVentAbility;
    private List<ImpostorVisionAbility> _impostorVisionAbilities = new();

    public ExPlayerControl(PlayerControl player)
    {
        this.Player = player;
        this.PlayerId = player.PlayerId;
        this.Data = player.CachedPlayerData;
        this.lastAbilityId = 0;
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
    public void SetRole(RoleId roleId)
    {
        Role = roleId;
        if (CustomRoleManager.TryGetRoleById(roleId, out var role))
        {
            role.OnSetRole(Player);
            roleBase = role;
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
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Crewmate : !Data.Role.IsImpostor;
    public bool IsImpostor()
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Impostor : Data.Role.IsImpostor;
    public bool IsNeutral()
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Neutral : false;
    // TODO: 後でMADロールを追加したらここに追加する
    public bool IsMadRoles()
        => false;
    // TODO: 後で追加する
    public bool IsFriendRoles()
        => false;
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
    public AbilityBase GetAbility(ulong abilityId)
    {
        return PlayerAbilitiesDictionary.TryGetValue(abilityId, out var ability) ? ability : null;
    }
    public void AttachAbility(AbilityBase ability, ulong abilityId)
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
        ability.Attach(Player, abilityId);
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
        PlayerAbilities.Remove(ability);
        PlayerAbilitiesDictionary.Remove(abilityId);
    }
}
public static class ExPlayerControlExtensions
{
    public static void AddAbility(this ExPlayerControl player, AbilityBase ability, ulong abilityId)
    {
        player.AttachAbility(ability, abilityId);
    }
}