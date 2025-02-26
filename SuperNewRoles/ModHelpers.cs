using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace SuperNewRoles;
public static class ModHelpers
{
    public static PlayerControl GetPlayerById(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player.PlayerId == id);
    }

    public static bool IsCustomServer()
    {
        if (FastDestroyableSingleton<ServerManager>.Instance == null) return false;
        StringNames n = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.TranslateName;
        return n is not StringNames.ServerNA and not StringNames.ServerEU and not StringNames.ServerAS;
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
    private static MD5 md5 = MD5.Create();
    public static string HashMD5(string str)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return HashMD5(bytes);
    }
    public static string HashMD5(byte[] bytes)
    {
        return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }
    public static ReadOnlySpan<T> AsSpan<T>(this List<T> list) => CollectionsMarshal.AsSpan(list);

    public static bool IsComms()
    {
        try
        {
            if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Comms, out ISystemType system)) return false;
            HudOverrideSystemType hudOverride;
            HqHudSystemType hqHud;
            if ((hudOverride = system.TryCast<HudOverrideSystemType>()) != null) return hudOverride.IsActive;
            if ((hqHud = system.TryCast<HqHudSystemType>()) != null) return hqHud.IsActive;

            return false;
        }
        catch (Exception e)
        {
            Logger.Error(e.ToString(), "IsComms");
            return false;
        }
    }
    public static string Cs(Color c, string s)
    {
        return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
    }
    public static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
    public static (int completed, int total) TaskCompletedData(NetworkedPlayerInfo playerInfo)
    {
        if (playerInfo?.Tasks == null)
            return (-1, -1);

        int TotalTasks = 0;
        int CompletedTasks = 0;

        for (int j = 0; j < playerInfo.Tasks.Count; j++)
        {
            if (playerInfo.Tasks[j] == null)
                continue;
            TotalTasks++;
            if (playerInfo.Tasks[j].Complete)
            {
                CompletedTasks++;
            }
        }
        return (CompletedTasks, TotalTasks);
    }
}