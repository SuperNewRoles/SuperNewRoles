using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Hazel;
using SuperNewRoles.Modules;
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
    public static string CsWithTranslation(Color c, string s)
    {
        return Cs(c, ModTranslation.GetString(s));
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
    public static long ReadInt64(this MessageReader reader)
    {

        long output = (long)reader.FastByte()
            | (long)reader.FastByte() << 8
            | (long)reader.FastByte() << 16
            | (long)reader.FastByte() << 24
            | (long)reader.FastByte() << 32
            | (long)reader.FastByte() << 40
            | (long)reader.FastByte() << 48
            | (long)reader.FastByte() << 56;
        return output;
    }
    /// <summary>keyCodesが押されているか</summary>
    public static bool GetManyKeyDown(KeyCode[] keyCodes) =>
        keyCodes.All(x => Input.GetKey(x)) && keyCodes.Any(x => Input.GetKeyDown(x));
    public static ExileController.InitProperties GenerateExileInitProperties(NetworkedPlayerInfo player, bool voteTie)
    {
        ExileController.InitProperties initProperties = new();
        if (player != null)
        {
            initProperties.outfit = player.Outfits[PlayerOutfitType.Default];
            initProperties.networkedPlayer = player;
            initProperties.isImpostor = player.Role.IsImpostor;
        }
        initProperties.voteTie = voteTie;
        initProperties.confirmImpostor = GameManager.Instance.LogicOptions.GetConfirmImpostor();
        initProperties.totalImpostorCount = GameData.Instance.AllPlayers.Count((NetworkedPlayerInfo p) => p.Role.IsImpostor);
        initProperties.remainingImpostorCount = GameData.Instance.AllPlayers.Count((NetworkedPlayerInfo p) => p.Role.IsImpostor && !p.IsDead && !p.Disconnected);
        if (player != null && player.Role.IsImpostor && !player.Disconnected)
            initProperties.remainingImpostorCount--;
        return initProperties;
    }
    public static bool IsMap(this MapNames map)
    {
        return GameOptionsManager.Instance.CurrentGameOptions.MapId == (byte)map;
    }
    public static void SetActiveAllObject(this IEnumerable<GameObject> gameObjects, bool active)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject == null) continue;
            gameObject.SetActive(active);
        }
    }
    public static void SetActiveAllObject(this IEnumerable<GameObject> gameObjects, string name, bool active)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject == null) continue;
            if (gameObject.name != name) continue;
            gameObject.SetActive(active);
        }
    }
    public static IEnumerable<GameObject> GetChildren(this GameObject gameObject)
    {
        List<GameObject> list = new();
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject == null) continue;
            list.Add(child.gameObject);
        }
        return list;
    }
    public static void Shuffle<T>(this List<T> list)
    {
        // Fisher–Yatesアルゴリズムを使ってリストをシャッフル
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    public static List<T> Shuffled<T>(this List<T> list)
    {
        list.Shuffle();
        return list;
    }
    public static void Write(this MessageWriter writer, short value)
    {
        writer.Buffer[writer.Position++] = (byte)value;
        writer.Buffer[writer.Position++] = (byte)(value >> 8);
        if (writer.Position > writer.Length) writer.Length = writer.Position;
    }
    public static void Write(this MessageWriter writer, long value)
    {
        writer.Buffer[writer.Position++] = (byte)value;
        writer.Buffer[writer.Position++] = (byte)(value >> 8);
        writer.Buffer[writer.Position++] = (byte)(value >> 16);
        writer.Buffer[writer.Position++] = (byte)(value >> 24);
        writer.Buffer[writer.Position++] = (byte)(value >> 32);
        writer.Buffer[writer.Position++] = (byte)(value >> 40);
        writer.Buffer[writer.Position++] = (byte)(value >> 48);
        writer.Buffer[writer.Position++] = (byte)(value >> 56);
        if (writer.Position > writer.Length) writer.Length = writer.Position;
    }
}