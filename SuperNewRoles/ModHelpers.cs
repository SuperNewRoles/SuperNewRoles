using System.Linq;

namespace SuperNewRoles.Helpers;

public static class ModHelpers
{
    public static PlayerControl GetPlayerById(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player.PlayerId == id);
    }

    /// <summary>
    /// ランダムを取得します。max = 10だと0～10まで取得できます
    /// </summary>
    /// <param name="max"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    public static int GetRandomInt(int max, int min = 0)
    {
        return UnityEngine.Random.Range(min, max + 1);
    }
}