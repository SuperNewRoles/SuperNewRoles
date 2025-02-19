using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Modules;

public class ExPlayerControl
{
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
}
public static class ExPlayerControlExtensions
{
    public static void AddAbility(this PlayerControl player, AbilityBase ability)
    {
        player.AddAbility(ability);
    }
}