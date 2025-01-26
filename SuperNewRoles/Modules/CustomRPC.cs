using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hazel;
using InnerNet;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Modules;

// Attribute
[AttributeUsage(AttributeTargets.Method)]
public class CustomRPCAttribute : Attribute
{
    public byte Id { get; private set; }
    public CustomRPCAttribute()
    {
    }
    public void SetId(byte id)
    {
        Id = id;
    }
}

public static class CustomRPC
{
    public static Dictionary<byte, MethodInfo> RpcMethods = new();
    private static string RpcHashGenerate(MethodInfo method, CustomRPCAttribute attribute)
    {
        // methodのハッシュ値を名前とメソッドの引数の型内容を元に生成
        return method.Name + string.Join(",", method.GetParameters().Select(p => p.ParameterType.Name));
    }
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
            // staticのみ
            if (!methods[i].IsStatic)
            {
                Logger.Error($"CustomRPC: {methods[i].Name} is not static");
                continue;
            }
            CustomRPCManager.RegisterRPC(methods[i], methods[i].GetCustomAttribute<CustomRPCAttribute>());
        }
    }
}
public static class CustomRPCManager
{
    public static bool IsRpcReceived = false;
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    public static class HandleRpc
    {
        public static void Postfix(byte callId, MessageReader reader)
        {
            if (callId == SNRRpcId)
            {
                byte id = reader.ReadByte();
                if (!CustomRPC.RpcMethods.TryGetValue(id, out var method))
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
    [CustomRPC()]
    public static void TestMethod(PlayerControl pc)
    {
        Logger.Info($"TestMethod: {pc.PlayerId}");
    }
    private static byte SNRRpcId = byte.MaxValue;
    public static void RegisterRPC(MethodInfo method, CustomRPCAttribute attribute)
    {
        static bool NewMethod(object? __instance, object[] __args, MethodBase __originalMethod)
        {
            if (IsRpcReceived)
            {
                IsRpcReceived = false;
                return true;
            }
            CustomRPCAttribute attribute = __originalMethod.GetCustomAttribute<CustomRPCAttribute>();
            if (attribute == null)
            {
                throw new Exception("CustomRPCAttribute is not found");
            }
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, SNRRpcId, SendOption.Reliable, -1);
            writer.Write(attribute.Id);
            // 引数を設定していく
            foreach (var arg in __args)
            {
                writer.Write(arg);
            }
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            return true;
        }
        var newMethod = NewMethod;
        CustomRPC.RpcMethods[attribute.Id] = method;
        // methodの中身をRPCを送信するものに入れ替える
        SuperNewRolesPlugin.Instance.Harmony.Patch(method, new HarmonyMethod(newMethod.Method));
    }
    private static void Write(this MessageWriter writer, object obj)
    {
        if (obj is byte b)
        {
            writer.Write(b);
        }
        else if (obj is int i)
        {
            writer.Write(i);
        }
        else if (obj is float f)
        {
            writer.Write(f);
        }
        else if (obj is bool bl)
        {
            writer.Write(bl);
        }
        else if (obj is string s)
        {
            writer.Write(s);
        }
        else if (obj is PlayerControl pc)
        {
            writer.Write(pc.PlayerId);
        }
        else if (obj is PlayerControl[] pcArray)
        {
            writer.Write(pcArray.Length);
            foreach (var playerControl in pcArray)
            {
                writer.Write(playerControl.PlayerId);
            }
        }
        else if (obj is InnerNetObject innerNetObject)
        {
            writer.Write(innerNetObject.NetId);
        }
        else
        {
            throw new Exception($"Invalid type: {obj.GetType()}");
        }
    }
    public static object ReadFromType(this MessageReader reader, Type type)
    {
        if (type == typeof(byte))
        {
            return reader.ReadByte();
        }
        else if (type == typeof(int))
        {
            return reader.ReadInt32();
        }
        else if (type == typeof(float))
        {
            return reader.ReadSingle();
        }
        else if (type == typeof(bool))
        {
            return reader.ReadBoolean();
        }
        else if (type == typeof(string))
        {
            return reader.ReadString();
        }
        else if (type == typeof(PlayerControl))
        {
            return ModHelpers.GetPlayerById(reader.ReadByte());
        }
        else if (type == typeof(PlayerControl[]))
        {
            int length = reader.ReadInt32();
            PlayerControl[] array = new PlayerControl[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = ModHelpers.GetPlayerById(reader.ReadByte());
            }
            return array;
        }
        else
        {
            throw new Exception($"Invalid type: {type}");
        }
    }
}
