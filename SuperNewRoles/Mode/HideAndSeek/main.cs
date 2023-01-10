namespace SuperNewRoles.Mode.HideAndSeek;

class main
{
    public static bool IsAllInMod;
    public static void ClearAndReloads()
    {
        IsAllInMod = true;
        foreach (InnerNet.ClientData client in AmongUsClient.Instance.allClients)
        {
            if (!client.IsMod())
            {
                IsAllInMod = false;
                break;
            }
        }
    }
}
