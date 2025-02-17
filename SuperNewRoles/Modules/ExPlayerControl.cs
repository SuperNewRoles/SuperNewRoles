using System.Collections.Generic;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Modules;

public class ExPlayerControl
{
    private static List<ExPlayerControl> _exPlayerControls { get; } = new();
    public static IReadOnlyList<ExPlayerControl> ExPlayerControls => _exPlayerControls.AsReadOnly();
    private static ExPlayerControl[] _exPlayerControlsArray;
    public static IReadOnlyCollection<ExPlayerControl> ExPlayerControlsArray => _exPlayerControlsArray;
    private PlayerControl player;
    public NetworkedPlayerInfo PlayerInfo { get; }
    public byte PlayerId { get; }
    public ExPlayerControl(PlayerControl player)
    {
        this.player = player;
        this.PlayerId = player.PlayerId;
        this.PlayerInfo = player.CachedPlayerData;
    }
    public static implicit operator PlayerControl(ExPlayerControl exPlayer)
    {
        return exPlayer.player;
    }
    public static void SetUpExPlayers()
    {
        _exPlayerControlsArray = new ExPlayerControl[255];
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            _exPlayerControls.Add(new ExPlayerControl(player));
            _exPlayerControlsArray[player.PlayerId] = _exPlayerControls[^1];
        }
    }
    public static ExPlayerControl ById(byte playerId)
    {
        return _exPlayerControlsArray[playerId];
    }
}
public static class ExPlayerControlExtensions
{
    public static void AddAbility(this PlayerControl player, AbilityBase ability)
    {
        player.AddAbility(ability);
    }
}