using System.Collections.Generic;
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

    public TextMeshPro PlayerInfoText { get; set; }
    public TextMeshPro MeetingInfoText { get; set; }

    public ExPlayerControl(PlayerControl player)
    {
        this.Player = player;
        this.PlayerId = player.PlayerId;
        this.Data = player.CachedPlayerData;
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
    public void SetRole(RoleId roleId)
    {
        Role = roleId;
        if (CustomRoleManager.TryGetRoleById(roleId, out var role))
        {
            role.OnSetRole(Player);
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
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Crewmate : !Data.Role.IsImpostor;
    public bool IsImpostor()
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Impostor : Data.Role.IsImpostor;
    public bool IsNeutral()
        => roleBase != null ? roleBase.AssignedTeam == AssignedTeamType.Neutral : false;
    // TODO: 後でMADロールを追加したらここに追加する
    public bool IsMadRoles()
        => false;
    public bool IsDead()
        => Data == null || Data.Disconnected || Data.IsDead;
    public bool IsAlive()
        => !IsDead();
    // TODO: 後で書く
    public bool IsTaskTriggerRole()
        => roleBase != null ? IsCrewmate() : IsCrewmate();
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