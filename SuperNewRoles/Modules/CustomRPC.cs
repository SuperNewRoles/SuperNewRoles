using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Modules;

// Attribute for marking RPC methods
[AttributeUsage(AttributeTargets.Method)]
public class CustomRPCAttribute : Attribute
{
    /// <summary>
    /// RPC メソッドの一意の識別子
    /// </summary>
    public byte Id { get; private set; }

    public CustomRPCAttribute() { }

    /// <summary>
    /// RPC メソッドの識別子を設定
    /// </summary>
    /// <param name="id">割り当てる識別子</param>
    public void SetId(byte id)
    {
        Id = id;
    }
}

/// <summary>
/// カスタムRPCを管理するクラス
/// </summary>
public static class CustomRPCManager
{
    /// <summary>
    /// テスト用のRPCメソッド
    /// </summary>
    /// <param name="pc">プレイヤーコントロール</param>
    [CustomRPC()]
    public static void TestMethod(PlayerControl pc, PlayerControl[] pcArray)
    {
        Logger.Info($"TestMethod: {pc.PlayerId}");
        Logger.Info($"TestMethod2: {pcArray.Length}");
        foreach (var p in pcArray)
        {
            Logger.Info($"TestMethod3: {p.PlayerId}");
        }
    }

    /// <summary>
    /// RPC メソッドを保存するディクショナリ
    /// </summary>
    private static Dictionary<byte, MethodInfo> RpcMethods = new();

    /// <summary>
    /// SuperNewRoles専用のRPC識別子
    /// </summary>
    private static byte SNRRpcId = byte.MaxValue;

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
        return method.Name + string.Join(",", method.GetParameters().Select(p => p.ParameterType.Name));
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

        // ソートされたハッシュ値に基づいてIDを割り当て
        for (byte i = 0; i < methods.Count; i++)
        {
            methods[i].GetCustomAttribute<CustomRPCAttribute>().SetId(i);
            // staticメソッドのみ許可
            if (!methods[i].IsStatic)
            {
                Logger.Error($"CustomRPC: {methods[i].Name} is not static");
                continue;
            }
            RegisterRPC(methods[i], methods[i].GetCustomAttribute<CustomRPCAttribute>());
        }
    }


    /// <summary>
    /// RPCメソッドを登録し、送信用のメソッドに置き換える
    /// </summary>
    /// <param name="method">登録するメソッド</param>
    /// <param name="attribute">CustomRPCAttribute</param>
    private static void RegisterRPC(MethodInfo method, CustomRPCAttribute attribute)
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

            // RPC属性を取得
            CustomRPCAttribute attribute = __originalMethod.GetCustomAttribute<CustomRPCAttribute>();
            if (attribute == null)
            {
                throw new Exception("CustomRPCAttribute is not found");
            }

            // RPC送信の準備
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, SNRRpcId, SendOption.Reliable, -1);
            writer.Write(attribute.Id);

            // 引数を設定
            foreach (var arg in __args)
            {
                writer.Write(arg);
            }

            // RPC送信
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            return true;
        }

        var newMethod = NewMethod;
        RpcMethods[attribute.Id] = method;

        // メソッドの中身をRPCを送信するものに入れ替える
        SuperNewRolesPlugin.Instance.Harmony.Patch(method, new HarmonyMethod(newMethod.Method));
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
            // SuperNewRoles専用のRPCの場合
            if (callId == SNRRpcId)
            {
                byte id = reader.ReadByte();
                if (!RpcMethods.TryGetValue(id, out var method))
                    return;

                // パラメーターを元にobject[]を作成
                List<object> args = new();
                for (int i = 0; i < method.GetParameters().Length; i++)
                {
                    args.Add(reader.ReadFromType(method.GetParameters()[i].ParameterType));
                }

                IsRpcReceived = true;
                method.Invoke(null, args.ToArray());
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
            case InnerNetObject innerNetObject:
                writer.Write(innerNetObject.NetId);
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
}
