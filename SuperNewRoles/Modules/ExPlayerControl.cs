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
}