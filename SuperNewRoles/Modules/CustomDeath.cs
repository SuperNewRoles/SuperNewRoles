using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;

namespace SuperNewRoles.Modules;

public static class CustomDeathExtensions
{
    [CustomRPC]
    public static void RpcCustomDeath(this ExPlayerControl player, CustomDeathType deathType)
    {
        player.CustomDeath(deathType);
    }
    public static void CustomDeath(this ExPlayerControl player, CustomDeathType deathType)
    {
        switch (deathType)
        {
            case CustomDeathType.Exile:
                player.Player.Exiled();
                ExileEvent.Invoke(player);
                break;
        }
    }
    public static void Register()
    {
        WrapUpEvent.Instance.AddListener(x =>
        {
            if (x.exiled == null)
                return;
            ExPlayerControl exPlayer = (ExPlayerControl)x.exiled;
            exPlayer.CustomDeath(CustomDeathType.Exile);
        });
    }
}
public enum CustomDeathType
{
    Exile,
    Kill,
}
