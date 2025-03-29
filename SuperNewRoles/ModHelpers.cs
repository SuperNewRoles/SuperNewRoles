using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
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
    public static string GetRandomFormat(string format, int min, int max)
    {
        return format.Replace("{0}", GetRandomInt(max, min).ToString());
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
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            var child = gameObject.transform.GetChild(i);
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
    public static int GetRandomIndex<T>(List<T> list)
    {
        return UnityEngine.Random.Range(0, list.Count);
    }
    /// <summary>
    /// 2つの位置が指定された距離以内かどうかを確認します
    /// </summary>
    /// <param name="pos">位置1</param>
    /// <param name="pos2">位置2</param>
    /// <param name="distance">最大距離</param>
    /// <returns>距離以内の場合はtrue</returns>
    public static bool IsPositionDistance(Vector2 pos, Vector2 pos2, float distance)
    {
        float dis = Vector2.Distance(pos, pos2);
        return dis <= distance;
    }

    /// <summary>
    /// リストからランダムな要素を取得します
    /// </summary>
    /// <typeparam name="T">リストの型</typeparam>
    /// <param name="list">リスト</param>
    /// <returns>ランダムな要素</returns>
    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0) return default;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    /// <summary>
    /// リストからランダムなインデックスを取得します
    /// </summary>
    /// <typeparam name="T">リストの型</typeparam>
    /// <param name="list">リスト</param>
    /// <returns>ランダムなインデックス</returns>
    public static int GetRandomIndex<T>(this IEnumerable<T> list)
    {
        var array = list as T[] ?? list.ToArray();
        return UnityEngine.Random.Range(0, array.Length);
    }
    public static IEnumerable<T> GetSystemEnumerable<T>(this Il2CppSystem.Collections.IEnumerable list)
    {
        var castedList = list.WrapToManaged().OfType<T>();
        foreach (var item in castedList)
        {
            yield return item;
        }
    }
    public static IEnumerable<T> GetSystemEnumerable<T>(this Il2CppSystem.Collections.Generic.IEnumerable<T> list)
    {
        return GetSystemEnumerable<T>(list.TryCast<Il2CppSystem.Collections.IEnumerable>());
    }
    public static T TryGetIndex<T>(this List<T> list, int index)
    {
        if (index < 0 || index >= list.Count) return default;
        return list[index];
    }
    public static T TryGetIndex<T>(this T[] array, int index)
    {
        if (index < 0 || index >= array.Length) return default;
        return array[index];
    }
    public static byte ParseToByte(this string str)
    {
        return byte.Parse(str);
    }
}