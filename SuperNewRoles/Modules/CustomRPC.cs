using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Modules;

// Attribute for marking RPC methods
[AttributeUsage(AttributeTargets.Method)]
public class CustomRPCAttribute : Attribute
{
    public bool OnlyOtherPlayer { get; }
    public CustomRPCAttribute(bool onlyOtherPlayer = false)
    {
        OnlyOtherPlayer = onlyOtherPlayer;
    }
}

public static class CustomRpcExts
{
    [CustomRPC]
    public static void RpcEndGameForHost(GameOverReason reason)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        GameManager.Instance.RpcEndGame(reason, false);
    }
}
/// <summary>
/// カスタムRPCを管理するクラス
/// </summary>
public static class CustomRPCManager
{

    /// <summary>
    /// RPC メソッドを保存するディクショナリ
    /// </summary>
    public static Dictionary<byte, MethodInfo> RpcMethods = new();
    /// <summary>
    /// RPC メソッドを保存するディクショナリ
    /// </summary>
    public static Dictionary<string, byte> RpcMethodIds = new();

    /// <summary>
    /// SuperNewRoles専用のRPC識別子
    /// </summary>
    private static byte SNRRpcId = byte.MaxValue;
    /// <summary>
    /// バージョン同期用のRPC識別子
    /// </summary>
    public static byte SNRSyncVersionRpc = byte.MaxValue - 1;
    /// <summary>
    /// RPCの受信状態を追跡するフラグ
    /// </summary>
    private static bool IsRpcReceived = false;

    /// <summary>
    /// メソッドのハッシュ値を生成
    /// </summary>
    /// <param name="method">ハッシュ値を生成するメソッド</param>
    /// <param name="attribute">CustomRPCAttribute</param>
    /// <returns>メソッドのハッシュ文字列</returns>
    private static string RpcHashGenerate(MethodInfo method)
    {
        // メソッドのハッシュ値を名前とメソッドの引数の型内容を元に生成
        return GetMethodFullName(method) + string.Join(",", method.GetParameters().Select(p => p.ParameterType.Name));
    }
    private static string RpcHashGenerate(MethodBase method)
    {
        // メソッドのハッシュ値を名前とメソッドの引数の型内容を元に生成
        return GetMethodFullName(method) + string.Join(",", method.GetParameters().Select(p => p.ParameterType.Name));
    }

    /// <summary>
    /// すべてのRPCメソッドを読み込み、登録する
    /// </summary>
    public static void Load()
    {
        // すべてのRPCメソッドのハッシュ値を収集
        var methods = Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            .Where(m => m.GetCustomAttribute<CustomRPCAttribute>() != null)
            .OrderBy(m => RpcHashGenerate(m))
            .ToList();
        Logger.Info($"Found {methods.Count} RPC methods");

        // ソートされたハッシュ値に基づいてIDを割り当て
        for (byte i = 0; i < methods.Count; i++)
        {
            var attribute = methods[i].GetCustomAttribute<CustomRPCAttribute>();
            // staticメソッドのみ許可
            if (!methods[i].IsStatic)
            {
                Logger.Error($"CustomRPC: {methods[i].Name} is not static");
                continue;
            }
            RegisterRPC(methods[i], attribute, i);
        }
        Logger.Info($"Registered {RpcMethods.Count} RPC methods");
    }


    /// <summary>
    /// RPCメソッドを登録し、送信用のメソッドに置き換える
    /// </summary>
    /// <param name="method">登録するメソッド</param>
    /// <param name="attribute">CustomRPCAttribute</param>
    private static void RegisterRPC(MethodInfo method, CustomRPCAttribute attribute, byte id)
    {
        // RPC送信用の新しいメソッドを定義
        static bool NewMethod(object? __instance, object[] __args, MethodBase __originalMethod)
        {
            // 重複RPC送信を防ぐ
            if (IsRpcReceived)
            {
                IsRpcReceived = false;
                return true;
            }

            var id = RpcMethodIds[RpcHashGenerate(__originalMethod)];
            // RPC送信の準備
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, SNRRpcId, SendOption.Reliable, -1);
            writer.Write(id);

            // 引数を設定
            foreach (var arg in __args)
            {
                writer.Write(arg);
            }

            // RPC送信
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            var attr = __originalMethod.GetCustomAttribute<CustomRPCAttribute>();
            Logger.Info($"Sent RPC: {__originalMethod.Name} {attr?.OnlyOtherPlayer}");
            return !attr?.OnlyOtherPlayer ?? true;
        }
        Logger.Info($"Registering RPC: {method.Name} {id}");
        var newMethod = NewMethod;
        RpcMethods[id] = method;
        RpcMethodIds[RpcHashGenerate(method)] = id;

        // メソッドの中身をRPCを送信するものに入れ替える
        SuperNewRolesPlugin.Instance.Harmony.Patch(method, new HarmonyMethod(newMethod.Method));
    }
    private static string GetMethodFullName(MethodInfo method)
    {
        return method.DeclaringType?.Name + "." + method.Name;
    }
    private static string GetMethodFullName(MethodBase method)
    {
        return method.DeclaringType?.Name + "." + method.Name;
    }
    /// <summary>
    /// RPC受信を処理するハーモニーパッチ
    /// </summary>
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    private static class HandleRpc
    {
        /// <summary>
        /// RPC受信後に呼び出されるメソッド
        /// </summary>
        public static void Postfix(byte callId, MessageReader reader)
        {
            Logger.Info($"Received RPC: {callId}");
            // SuperNewRoles専用のRPCの場合
            if (callId == SNRRpcId)
            {
                byte id = reader.ReadByte();
                Logger.Info($"Received RPC: {id}");
                if (!RpcMethods.TryGetValue(id, out var method))
                    return;

                // パラメーターを元にobject[]を作成
                List<object> args = new();
                for (int i = 0; i < method.GetParameters().Length; i++)
                {
                    args.Add(reader.ReadFromType(method.GetParameters()[i].ParameterType));
                }

                IsRpcReceived = true;
                Logger.Info($"Received RPC: {method.Name}");
                method.Invoke(null, args.ToArray());
            }
            else if (callId == SNRSyncVersionRpc)
            {
                SyncVersion.ReceivedSyncVersion(reader);
            }
        }
    }

    /// <summary>
    /// オブジェクトをMessageWriterに書き込む拡張メソッド
    /// </summary>
    private static void Write(this MessageWriter writer, object obj)
    {
        if (obj != null && obj.GetType().IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(obj.GetType());
            if (underlyingType == typeof(byte))
                writer.Write((byte)(object)obj);
            else if (underlyingType == typeof(short))
                ModHelpers.Write(writer, (short)(object)obj);
            else if (underlyingType == typeof(ushort))
                writer.Write((ushort)(object)obj);
            else if (underlyingType == typeof(int))
                writer.Write((int)(object)obj);
            else if (underlyingType == typeof(uint))
                writer.Write((uint)(object)obj);
            else if (underlyingType == typeof(ulong))
                writer.Write((ulong)(object)obj);
            else if (underlyingType == typeof(long))
                ModHelpers.Write(writer, (long)obj);
            else
                throw new Exception($"Unsupported enum underlying type: {underlyingType}");
            return;
        }
        switch (obj)
        {
            case byte b:
                writer.Write(b);
                break;
            case int i:
                writer.Write(i);
                break;
            case short s:
                ModHelpers.Write(writer, s);
                break;
            case ushort us:
                writer.Write(us);
                break;
            case uint ui:
                writer.Write(ui);
                break;
            case ulong ul:
                writer.Write(ul);
                break;
            case long l:
                ModHelpers.Write(writer, l);
                break;
            case float f:
                writer.Write(f);
                break;
            case bool bl:
                writer.Write(bl);
                break;
            case string s:
                writer.Write(s);
                break;
            case Color color:
                writer.Write(color.r);
                writer.Write(color.g);
                writer.Write(color.b);
                writer.Write(color.a);
                break;
            case Color32 color32:
                writer.Write(color32.r);
                writer.Write(color32.g);
                writer.Write(color32.b);
                writer.Write(color32.a);
                break;
            case Dictionary<string, string> dict:
                writer.Write(dict.Count);
                foreach (var kvp in dict)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
                break;
            case Dictionary<string, int> dictInt:
                writer.Write(dictInt.Count);
                foreach (var kvp in dictInt)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
                break;
            case Dictionary<byte, byte> dictByte:
                writer.Write(dictByte.Count);
                foreach (var kvp in dictByte)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
                break;
            case Dictionary<string, byte> dictStringByte:
                writer.Write(dictStringByte.Count);
                foreach (var kvp in dictStringByte)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
                break;
            case Dictionary<ushort, byte> dictUshortByte:
                writer.Write(dictUshortByte.Count);
                foreach (var kvp in dictUshortByte)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value);
                }
                break;
            case Dictionary<byte, (byte, int)> dictByteWithTuple:
                writer.Write(dictByteWithTuple.Count);
                foreach (var kvp in dictByteWithTuple)
                {
                    writer.Write(kvp.Key);
                    writer.Write(kvp.Value.Item1);
                    writer.Write(kvp.Value.Item2);
                }
                break;
            case PlayerControl pc:
                writer.Write(pc.PlayerId);
                break;
            case PlayerControl[] pcArray:
                writer.Write(pcArray.Length);
                foreach (var playerControl in pcArray)
                {
                    writer.Write(playerControl.PlayerId);
                }
                break;
            case ExPlayerControl exPc:
                writer.Write(exPc.PlayerId);
                break;
            case ExPlayerControl[] exPcArray:
                writer.Write(exPcArray.Length);
                foreach (var exPlayerControl in exPcArray)
                {
                    writer.Write(exPlayerControl.PlayerId);
                }
                break;
            case NetworkedPlayerInfo networkedPlayerInfo:
                writer.Write(networkedPlayerInfo.PlayerId);
                break;
            case InnerNetObject innerNetObject:
                writer.Write(innerNetObject.NetId);
                break;
            case List<byte> byteList:
                writer.Write(byteList.Count);
                foreach (var b in byteList)
                {
                    writer.Write(b);
                }
                break;
            case Vector2 v2:
                writer.Write(v2.x);
                writer.Write(v2.y);
                break;
            case Vector3 v3:
                writer.Write(v3.x);
                writer.Write(v3.y);
                writer.Write(v3.z);
                break;
            case AbilityBase abilityBase:
                if (abilityBase == null || abilityBase.Player == null)
                    writer.Write(byte.MaxValue);
                else
                {
                    writer.Write(abilityBase.Player.PlayerId);
                    writer.Write(abilityBase.AbilityId);
                }
                break;
            default:
                throw new Exception($"Invalid type: {obj.GetType()}");
        }
    }

    /// <summary>
    /// MessageReaderから指定された型のオブジェクトを読み取る拡張メソッド
    /// </summary>
    private static object ReadFromType(this MessageReader reader, Type type)
    {
        if (type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
            object value = underlyingType == typeof(byte) ? reader.ReadByte()
                         : underlyingType == typeof(short) ? reader.ReadInt16()
                         : underlyingType == typeof(ushort) ? reader.ReadUInt16()
                         : underlyingType == typeof(int) ? reader.ReadInt32()
                         : underlyingType == typeof(uint) ? reader.ReadUInt32()
                         : underlyingType == typeof(ulong) ? reader.ReadUInt64()
                         : underlyingType == typeof(long) ? reader.ReadInt64()
                         : throw new Exception($"Unsupported enum underlying type: {underlyingType}");
            return Enum.ToObject(type, value);
        }
        return type switch
        {
            Type t when t == typeof(byte) => reader.ReadByte(),
            Type t when t == typeof(int) => reader.ReadInt32(),
            Type t when t == typeof(short) => reader.ReadInt16(),
            Type t when t == typeof(ushort) => reader.ReadUInt16(),
            Type t when t == typeof(uint) => reader.ReadUInt32(),
            Type t when t == typeof(ulong) => reader.ReadUInt64(),
            Type t when t == typeof(float) => reader.ReadSingle(),
            Type t when t == typeof(bool) => reader.ReadBoolean(),
            Type t when t == typeof(string) => reader.ReadString(),
            Type t when t == typeof(PlayerControl) => ModHelpers.GetPlayerById(reader.ReadByte()),
            Type t when t == typeof(PlayerControl[]) => ReadPlayerControlArray(reader),
            Type t when t == typeof(ExPlayerControl) => ExPlayerControl.ById(reader.ReadByte()),
            Type t when t == typeof(ExPlayerControl[]) => ReadExPlayerControlArray(reader),
            Type t when t == typeof(NetworkedPlayerInfo) => GameData.Instance.GetPlayerById(reader.ReadByte()),
            Type t when t == typeof(RoleId) => (RoleId)reader.ReadInt32(),
            Type t when t == typeof(Dictionary<string, string>) => ReadDictionary<string, string>(reader, r => r.ReadString(), r => r.ReadString()),
            Type t when t == typeof(Dictionary<string, int>) => ReadDictionary<string, int>(reader, r => r.ReadString(), r => r.ReadInt32()),
            Type t when t == typeof(Dictionary<byte, byte>) => ReadDictionary<byte, byte>(reader, r => r.ReadByte(), r => r.ReadByte()),
            Type t when t == typeof(Dictionary<string, byte>) => ReadDictionary<string, byte>(reader, r => r.ReadString(), r => r.ReadByte()),
            Type t when t == typeof(Dictionary<ushort, byte>) => ReadDictionary<ushort, byte>(reader, r => r.ReadUInt16(), r => r.ReadByte()),
            Type t when t == typeof(Dictionary<byte, (byte, int)>) => ReadDictionaryWithTuple(reader),
            Type t when t == typeof(Color) => new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            Type t when t == typeof(Color32) => new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
            Type t when t == typeof(List<byte>) => ReadByteList(reader),
            Type t when t == typeof(Vector2) => new Vector2(reader.ReadSingle(), reader.ReadSingle()),
            Type t when t == typeof(Vector3) => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            Type t when t.IsSubclassOf(typeof(AbilityBase)) => ReadAbilityBase(reader),
            _ => throw new Exception($"Invalid type: {type}")
        };
    }
    /// <summary>
    /// AbilityBaseを読み取るヘルパーメソッド
    /// </summary>
    private static object ReadAbilityBase(MessageReader reader)
    {
        byte playerId = reader.ReadByte();
        if (playerId == byte.MaxValue) return null;
        ulong abilityId = reader.ReadUInt64();
        ExPlayerControl exPlayer = ExPlayerControl.ById(playerId);
        if (exPlayer == null) return null;
        return exPlayer.GetAbility(abilityId);
    }
    /// <summary>
    /// Dictionary を読み取るヘルパーメソッド
    /// </summary>
    private static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(
        MessageReader reader,
        Func<MessageReader, TKey> keyReader,
        Func<MessageReader, TValue> valueReader)
    {
        int count = reader.ReadInt32();
        var dict = new Dictionary<TKey, TValue>();
        for (int i = 0; i < count; i++)
        {
            var key = keyReader(reader);
            var value = valueReader(reader);
            dict[key] = value;
        }
        return dict;
    }

    /// <summary>
    /// タプルを含むDictionaryを読み取るヘルパーメソッド
    /// </summary>
    private static Dictionary<byte, (byte, int)> ReadDictionaryWithTuple(MessageReader reader)
    {
        int count = reader.ReadInt32();
        var dict = new Dictionary<byte, (byte, int)>();
        for (int i = 0; i < count; i++)
        {
            byte key = reader.ReadByte();
            byte tupleItem1 = reader.ReadByte();
            int tupleItem2 = reader.ReadInt32();
            dict[key] = (tupleItem1, tupleItem2);
        }
        return dict;
    }

    /// <summary>
    /// PlayerControlの配列を読み取る
    /// </summary>
    private static PlayerControl[] ReadPlayerControlArray(MessageReader reader)
    {
        int length = reader.ReadInt32();
        PlayerControl[] array = new PlayerControl[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = ModHelpers.GetPlayerById(reader.ReadByte());
        }
        return array;
    }

    /// <summary>
    /// ExPlayerControlの配列を読み取る
    /// </summary>
    private static ExPlayerControl[] ReadExPlayerControlArray(MessageReader reader)
    {
        int length = reader.ReadInt32();
        ExPlayerControl[] array = new ExPlayerControl[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = ExPlayerControl.ById(reader.ReadByte());
        }
        return array;
    }

    /// <summary>
    /// List<byte>を読み取る
    /// </summary>
    private static List<byte> ReadByteList(MessageReader reader)
    {
        int count = reader.ReadInt32();
        List<byte> list = new List<byte>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(reader.ReadByte());
        }
        return list;
    }
}