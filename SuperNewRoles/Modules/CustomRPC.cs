using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
    /*[CustomRPC(onlyOtherPlayer: true)]
    public static void RpcApplyDeadBodyImpulse(byte parentId, float impulseX, float impulseY)
    {
        foreach (DeadBody deadBody in UnityEngine.Object.FindObjectsOfType<DeadBody>())
        {
            if (deadBody.ParentId == parentId)
            {
                // 小さなキックオフセットを適用
                deadBody.transform.position += new UnityEngine.Vector3(impulseX, impulseY, 0f);
                break;
            }
        }
    }*/
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
    /// キャッシュ用：メソッドからRPC IDを高速取得
    /// </summary>
    private static Dictionary<MethodBase, byte> RpcIdsByMethod = new();
    /// <summary>
    /// キャッシュ用：メソッドからOnlyOtherPlayerフラグを高速取得
    /// </summary>
    private static Dictionary<MethodBase, bool> OnlyOtherFlagsByMethod = new();
    /// <summary>
    /// キャッシュ用：メソッドからパラメータ型配列を高速取得
    /// </summary>
    private static Dictionary<MethodBase, Type[]> ParamTypesByMethod = new();
    /// <summary>
    /// キャッシュ用：インスタンスメソッドかどうか
    /// </summary>
    private static HashSet<MethodBase> InstanceMethodSet = new();

    /// <summary>
    /// SuperNewRoles専用のRPC識別子
    /// </summary>
    private const byte SNRRpcId = byte.MaxValue;
    /// <summary>
    /// バージョン同期用のRPC識別子
    /// </summary>
    public const byte SNRSyncVersionRpc = byte.MaxValue - 1;
    /// <summary>
    /// ネットワーク移動用のRPC識別子
    /// </summary>
    public const byte SNRNetworkTransformRpc = byte.MaxValue - 2;
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
    public static List<Action> Load()
    {
        // すべてのRPCメソッドのハッシュ値を収集
        var methodsWithDetails = SuperNewRolesPlugin.Assembly
            .GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            .Select(m => new
            {
                Method = m,
                Attribute = m.GetCustomAttribute<CustomRPCAttribute>(),
                Hash = RpcHashGenerate(m), // RpcHashGenerateを一度だけ呼び出す
                ParamTypes = m.GetParameters().Select(p => p.ParameterType).ToArray() // パラメータ型もここで取得
            })
            .Where(m => m.Attribute != null)
            .OrderBy(m => m.Hash) // 事前に計算したハッシュでソート
            .ToList();

        List<Action> tasks = new();

        SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Start loading {methodsWithDetails.Count} RPC methods");

        // ソートされたハッシュ値に基づいてIDを割り当て
        for (byte i = 0; i < methodsWithDetails.Count; i++)
        {
            var methodDetail = methodsWithDetails[i];
            var method = methodDetail.Method;
            var attribute = methodDetail.Attribute;
            var hash = methodDetail.Hash; // 事前計算したハッシュ
            var paramTypes = methodDetail.ParamTypes; // 事前取得したパラメータ型

            // staticメソッドはもちろん、AbilityBaseのインスタンスメソッドも許可
            if (!method.IsStatic && !typeof(AbilityBase).IsAssignableFrom(method.DeclaringType))
            {
                Logger.Error($"CustomRPC: {method.Name} is not static and not an AbilityBase instance method");
                continue;
            }
            SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Loading RPC method ({i + 1}/{methodsWithDetails.Count}): {method.Name}");
            tasks.Add(RegisterRPC(method, attribute, i, hash, paramTypes)); // ハッシュとパラメータ型を渡す
        }
        SuperNewRolesPlugin.Logger.LogInfo($"[Splash] Registered {RpcMethods.Count} RPC methods");
        return tasks;
    }


    /// <summary>
    /// RPCメソッドを登録し、送信用のメソッドに置き換える
    /// </summary>
    /// <param name="method">登録するメソッド</param>
    /// <param name="attribute">CustomRPCAttribute</param>
    /// <param name="id">RPC ID</param>
    /// <param name="hash">メソッドのハッシュ値</param>
    /// <param name="paramTypes">メソッドのパラメータ型配列</param>
    private static Action RegisterRPC(MethodInfo method, CustomRPCAttribute attribute, byte id, string hash, Type[] paramTypes)
    {
        // キャッシュにメソッド情報を登録
        RpcIdsByMethod[method] = id;
        OnlyOtherFlagsByMethod[method] = attribute.OnlyOtherPlayer;
        ParamTypesByMethod[method] = paramTypes; // 事前取得したパラメータ型を使用
        if (!method.IsStatic && typeof(AbilityBase).IsAssignableFrom(method.DeclaringType))
        {
            InstanceMethodSet.Add(method);
        }
        // RPC送信用の新しいメソッドを定義
        static bool NewMethod(object? __instance, object[] __args, MethodBase __originalMethod)
        {
            // 重複RPC送信を防ぐ
            if (IsRpcReceived)
            {
                IsRpcReceived = false;
                return true;
            }

            // RPC ID をキャッシュから取得
            var rpcId = RpcIdsByMethod[__originalMethod];
            var onlyOther = OnlyOtherFlagsByMethod[__originalMethod];
            // RPC送信の準備
            var writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, SNRRpcId, SendOption.Reliable, -1);
            writer.Write(rpcId);

            // インスタンスメソッドならインスタンスを送信
            if (__instance != null && InstanceMethodSet.Contains(__originalMethod))
            {
                writer.Write(__instance, __originalMethod.DeclaringType);
            }

            // 引数を書き込み
            var originalParamTypes = ParamTypesByMethod[__originalMethod];
            for (int i = 0; i < __args.Length; i++)
            {
                writer.Write(__args[i], originalParamTypes[i]);
            }

            // RPC送信
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            Logger.Info($"Sent RPC: {__originalMethod.Name} OnlyOther={onlyOther}");
            return !onlyOther;
        }
        Logger.Info($"Registering RPC: {method.Name} {id}");
        var newHarmonyMethod = NewMethod;
        RpcMethods[id] = method;
        RpcMethodIds[hash] = id; // 事前計算したハッシュを使用

        // メソッドの中身をRPCを送信するものに入れ替える
        return () => SuperNewRolesPlugin.Instance.Harmony.Patch(method, new HarmonyMethod(newHarmonyMethod.Method));
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
            if (callId != 253)
                Logger.Info($"Received RPC: {callId}");
            // SuperNewRoles専用のRPCの場合
            switch (callId)
            {
                case SNRRpcId:
                    byte id = reader.ReadByte();
                    Logger.Info($"Received RPC: {id}");
                    if (!RpcMethods.TryGetValue(id, out var method)) return;
                    // インスタンスメソッドならインスタンスを読み込む
                    object? instance = null;
                    if (InstanceMethodSet.Contains(method))
                    {
                        instance = reader.ReadFromType(method.DeclaringType);
                    }
                    // パラメータを読み込み
                    var paramTypesRecv = ParamTypesByMethod[method];
                    var argsRecv = new object[paramTypesRecv.Length];
                    for (int i = 0; i < paramTypesRecv.Length; i++)
                    {
                        argsRecv[i] = reader.ReadFromType(paramTypesRecv[i]);
                    }
                    IsRpcReceived = true;
                    Logger.Info($"Received RPC: {method.Name}");
                    method.Invoke(instance, argsRecv);
                    break;
                case SNRSyncVersionRpc:
                    SyncVersion.ReceivedSyncVersion(reader);
                    break;
                case SNRNetworkTransformRpc:
                    ModdedNetworkTransform.ReceivedNetworkTransform(reader);
                    break;
            }
        }
    }

    /// <summary>
    /// オブジェクトをMessageWriterに書き込む拡張メソッド
    /// </summary>
    private static void Write(this MessageWriter writer, object obj, Type type)
    {
        if (obj != null && type.IsEnum)
        {
            var underlyingType = Enum.GetUnderlyingType(type);
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
            case List<string> list:
                writer.Write(list.Count);
                foreach (var s in list)
                {
                    writer.Write(s);
                }
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
            default:
                if (type == typeof(Dictionary<byte, byte>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<byte, byte>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                else if (type == typeof(Dictionary<string, int>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<string, int>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                else if (type == typeof(Dictionary<byte, byte>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<byte, byte>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                else if (type == typeof(Dictionary<string, byte>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<string, byte>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                else if (type == typeof(Dictionary<ushort, byte>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<ushort, byte>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                else if (type == typeof(Dictionary<byte, (byte, int)>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<byte, (byte, int)>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value.Item1);
                            writer.Write(kvp.Value.Item2);
                        }
                    }
                }
                else if (type == typeof(Dictionary<byte, byte>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<byte, byte>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                else if (type == typeof(Dictionary<byte, float>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<byte, float>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                else if (type == typeof(PlayerControl))
                {
                    if (obj == null)
                        writer.Write(byte.MaxValue);
                    else
                        writer.Write((obj as PlayerControl)?.PlayerId ?? byte.MaxValue);
                }
                else if (type == typeof(PlayerControl[]))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var pcArray = obj as PlayerControl[];
                        writer.Write(pcArray.Length);
                        foreach (var playerControl in pcArray)
                        {
                            if (playerControl == null)
                                writer.Write(byte.MaxValue);
                            else
                                writer.Write(playerControl.PlayerId);
                        }
                    }
                }
                else if (type == typeof(ExPlayerControl))
                {
                    if (obj == null)
                        writer.Write(byte.MaxValue);
                    else
                        writer.Write((obj as ExPlayerControl)?.PlayerId ?? byte.MaxValue);
                }
                else if (type == typeof(ExPlayerControl[]))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var exPcArray = obj as ExPlayerControl[];
                        writer.Write(exPcArray.Length);
                        foreach (var exPlayerControl in exPcArray)
                        {
                            if (exPlayerControl == null)
                                writer.Write(byte.MaxValue);
                            else
                                writer.Write(exPlayerControl.PlayerId);
                        }
                    }
                }
                else if (type == typeof(NetworkedPlayerInfo))
                {
                    if (obj == null)
                        writer.Write(byte.MaxValue);
                    else
                        writer.Write((obj as NetworkedPlayerInfo)?.PlayerId ?? byte.MaxValue);
                }
                else if (type.IsSubclassOf(typeof(InnerNetObject)))
                {
                    if (obj == null)
                        writer.Write(uint.MaxValue);
                    else
                        writer.Write((obj as InnerNetObject)?.NetId ?? uint.MaxValue);
                }
                else if (type == typeof(List<byte>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var byteList = obj as List<byte>;
                        writer.Write(byteList.Count);
                        foreach (var b in byteList)
                        {
                            writer.Write(b);
                        }
                    }
                }
                else if (type.IsSubclassOf(typeof(AbilityBase)))
                {
                    var abilityBase = obj as AbilityBase;
                    if (obj == null || abilityBase?.Player == null)
                        writer.Write(byte.MaxValue);
                    else
                    {
                        writer.Write(abilityBase.Player.PlayerId);
                        writer.Write(abilityBase.AbilityId);
                    }
                }
                else if (type == typeof(Vector2))
                {
                    if (obj == null)
                    {
                        writer.Write(0f);
                        writer.Write(0f);
                    }
                    else
                    {
                        Vector2 v2 = (Vector2)obj;
                        writer.Write(v2.x);
                        writer.Write(v2.y);
                    }
                }
                else if (type == typeof(Vector3))
                {
                    if (obj == null)
                    {
                        writer.Write(0f);
                        writer.Write(0f);
                        writer.Write(0f);
                    }
                    else
                    {
                        Vector3 v3 = (Vector3)obj;
                        writer.Write(v3.x);
                        writer.Write(v3.y);
                        writer.Write(v3.z);
                    }
                }
                else if (type == typeof(Vector2[]))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var v2Array = obj as Vector2[];
                        writer.Write(v2Array.Length);
                        foreach (var v in v2Array)
                        {
                            writer.Write(v.x);
                            writer.Write(v.y);
                        }
                    }
                }
                else if (type == typeof(Vector3[]))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var v3Array = obj as Vector3[];
                        writer.Write(v3Array.Length);
                        foreach (var v in v3Array)
                        {
                            writer.Write(v.x);
                            writer.Write(v.y);
                            writer.Write(v.z);
                        }
                    }
                }
                else if (type == typeof(Dictionary<byte, bool>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var dict = obj as Dictionary<byte, bool>;
                        writer.Write(dict.Count);
                        foreach (var kvp in dict)
                        {
                            writer.Write(kvp.Key);
                            writer.Write(kvp.Value);
                        }
                    }
                }
                else if (type == typeof(List<ExPlayerControl>))
                {
                    if (obj == null)
                        writer.Write(0);
                    else
                    {
                        var exPcList = obj as List<ExPlayerControl>;
                        writer.Write(exPcList.Count);
                        foreach (var exPlayerControl in exPcList)
                        {
                            if (exPlayerControl == null)
                                writer.Write(byte.MaxValue);
                            else
                                writer.Write(exPlayerControl.PlayerId);
                        }
                    }
                }
                else
                {
                    throw new Exception($"Invalid type: {obj.GetType()}");
                }
                break;
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
            Type t when t == typeof(List<string>) => ReadStringList(reader),
            Type t when t == typeof(PlayerControl) => (PlayerControl)ExPlayerControl.ById(reader.ReadByte()),
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
            Type t when t == typeof(Dictionary<byte, float>) => ReadDictionary<byte, float>(reader, r => r.ReadByte(), r => r.ReadSingle()),
            Type t when t == typeof(Dictionary<byte, bool>) => ReadDictionary<byte, byte>(reader, r => r.ReadByte(), r => r.ReadByte()),
            Type t when t == typeof(Dictionary<byte, (byte, int)>) => ReadDictionaryWithTuple(reader),
            Type t when t == typeof(Dictionary<byte, bool>) => ReadDictionary<byte, bool>(reader, r => r.ReadByte(), r => r.ReadBoolean()),
            Type t when t == typeof(Color) => new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            Type t when t == typeof(Color32) => new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
            Type t when t == typeof(List<byte>) => ReadByteList(reader),
            Type t when t == typeof(List<ExPlayerControl>) => ReadExPlayerControlList(reader),
            Type t when t == typeof(Vector2) => new Vector2(reader.ReadSingle(), reader.ReadSingle()),
            Type t when t == typeof(Vector3) => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            Type t when t == typeof(Vector2[]) => ReadVector2Array(reader),
            Type t when t == typeof(Vector3[]) => ReadVector3Array(reader),
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
    private static List<string> ReadStringList(MessageReader reader)
    {
        int count = reader.ReadInt32();
        var list = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(reader.ReadString());
        }
        return list;
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
            array[i] = (PlayerControl)ExPlayerControl.ById(reader.ReadByte());
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
    /// List<ExPlayerControl>を読み取る
    /// </summary>
    private static List<ExPlayerControl> ReadExPlayerControlList(MessageReader reader)
    {
        int count = reader.ReadInt32();
        List<ExPlayerControl> list = new(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(ExPlayerControl.ById(reader.ReadByte()));
        }
        return list;
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

    // Vector2[]を読み取るヘルパーメソッド
    private static Vector2[] ReadVector2Array(MessageReader reader)
    {
        int length = reader.ReadInt32();
        Vector2[] array = new Vector2[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
        return array;
    }

    // Vector3[]を読み取るヘルパーメソッド
    private static Vector3[] ReadVector3Array(MessageReader reader)
    {
        int length = reader.ReadInt32();
        Vector3[] array = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        return array;
    }
}