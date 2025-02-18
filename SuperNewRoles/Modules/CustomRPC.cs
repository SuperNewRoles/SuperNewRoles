using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

// Attribute for marking RPC methods
[AttributeUsage(AttributeTargets.Method)]
public class CustomRPCAttribute : Attribute
{

    public CustomRPCAttribute() { }
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
    private static string RpcHashGenerate(MethodInfo method, CustomRPCAttribute attribute)
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
            .OrderBy(m => RpcHashGenerate(m, m.GetCustomAttribute<CustomRPCAttribute>()))
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
            RegisterRPC(methods[i], i);
        }
        Logger.Info($"Registered {RpcMethods.Count} RPC methods");
    }


    /// <summary>
    /// RPCメソッドを登録し、送信用のメソッドに置き換える
    /// </summary>
    /// <param name="method">登録するメソッド</param>
    /// <param name="attribute">CustomRPCAttribute</param>
    private static void RegisterRPC(MethodInfo method, byte id)
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

            var id = RpcMethodIds[GetMethodFullName(__originalMethod)];
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
            Logger.Info($"Sent RPC: {__originalMethod.Name}");
            return true;
        }
        Logger.Info($"Registering RPC: {method.Name} {id}");
        var newMethod = NewMethod;
        RpcMethods[id] = method;
        RpcMethodIds[GetMethodFullName(method)] = id;

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
        switch (obj)
        {
            case byte b:
                writer.Write(b);
                break;
            case int i:
                writer.Write(i);
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
            case RoleId roleId:
                writer.Write((int)roleId);
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
        return type switch
        {
            Type t when t == typeof(byte) => reader.ReadByte(),
            Type t when t == typeof(int) => reader.ReadInt32(),
            Type t when t == typeof(float) => reader.ReadSingle(),
            Type t when t == typeof(bool) => reader.ReadBoolean(),
            Type t when t == typeof(string) => reader.ReadString(),
            Type t when t == typeof(PlayerControl) => ModHelpers.GetPlayerById(reader.ReadByte()),
            Type t when t == typeof(PlayerControl[]) => ReadPlayerControlArray(reader),
            Type t when t == typeof(ExPlayerControl) => ExPlayerControl.ById(reader.ReadByte()),
            Type t when t == typeof(ExPlayerControl[]) => ReadExPlayerControlArray(reader),
            Type t when t == typeof(NetworkedPlayerInfo) => GameData.Instance.GetPlayerById(reader.ReadByte()),
            Type t when t == typeof(RoleId) => (RoleId)reader.ReadInt32(),
            _ => throw new Exception($"Invalid type: {type}")
        };
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
}