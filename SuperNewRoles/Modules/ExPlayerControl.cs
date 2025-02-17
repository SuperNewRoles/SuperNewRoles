using System.Collections.Generic;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Modules;

public class ExPlayerControl
{
    public static ExPlayerControl LocalPlayer;
    private static ExPlayerControl _localPlayer;
    private static List<ExPlayerControl> _exPlayerControls { get; } = new();
    public static IReadOnlyList<ExPlayerControl> ExPlayerControls => _exPlayerControls.AsReadOnly();
    private static ExPlayerControl[] _exPlayerControlsArray;
    public static IReadOnlyCollection<ExPlayerControl> ExPlayerControlsArray => _exPlayerControlsArray;
    private PlayerControl player;
    public NetworkedPlayerInfo Data { get; }
    public byte PlayerId { get; }
    public RoleId Role { get; private set; }
    public IRoleBase roleBase { get; private set; }
    public List<AbilityBase> PlayerAbilities { get; private set; } = new();
    public ExPlayerControl(PlayerControl player)
    {
        this.player = player;
        this.PlayerId = player.PlayerId;
        this.Data = player.CachedPlayerData;
    }
    public static implicit operator PlayerControl(ExPlayerControl exPlayer)
    {
        return exPlayer.player;
    }
    public static implicit operator ExPlayerControl(PlayerControl player)
    {
        return ById(player.PlayerId);
    }
    public void SetRole(RoleId roleId)
    {
        Role = roleId;
        if (CustomRoleManager.TryGetRoleById(roleId, out var role))
        {
            role.OnSetRole(player);
            roleBase = role;
        }
    }
    public static void SetUpExPlayers()
    {
        _exPlayerControlsArray = new ExPlayerControl[255];
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            _exPlayerControls.Add(new ExPlayerControl(player));
            _exPlayerControlsArray[player.PlayerId] = _exPlayerControls[^1];
        }
        _localPlayer = _exPlayerControlsArray[PlayerControl.LocalPlayer.PlayerId];
    }
    public static ExPlayerControl ById(byte playerId)
    {
        return _exPlayerControlsArray[playerId];
    }
    public bool IsCrewmate()
        => roleBase.AssignedTeam == AssignedTeamType.Crewmate;
    public bool IsImpostor()
        => roleBase.AssignedTeam == AssignedTeamType.Impostor;
    public bool IsNeutral()
        => roleBase.AssignedTeam == AssignedTeamType.Neutral;
    // TODO: 後でMADロールを追加したらここに追加する
    public bool IsMadRoles()
        => false;
}
public static class ExPlayerControlExtensions
{
    public static void AddAbility(this PlayerControl player, AbilityBase ability)
    {
        ExPlayerControl exPlayer = player;
        exPlayer.PlayerAbilities.Add(ability);
        ability.Attach(player);
    }
}