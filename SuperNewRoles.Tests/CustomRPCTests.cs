using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using SuperNewRoles;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using Xunit;

namespace SuperNewRoles.Tests;

// CustomRPC のロード処理（決定論的 ID 付与/メソッド対応表）と、
// 既定の型別 Read/Write テーブル登録を検証するテスト。
public class CustomRPCTests
{
    // Minimal ability for ability serialization test
    private class AlphaAbility : AbilityBase { }

    private static void EnsurePluginLogger()
    {
        var t = typeof(SuperNewRoles.SuperNewRolesPlugin);
        var f = t.GetField("<Logger>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        if (f != null && f.GetValue(null) == null)
        {
            f.SetValue(null, new BepInEx.Logging.ManualLogSource("Tests"));
        }
    }

    private static string ComputeRpcHash(MethodInfo m)
    {
        var name = (m.DeclaringType?.Name ?? "") + "." + m.Name;
        var args = string.Join(",", m.GetParameters().Select(p => p.ParameterType.Name));
        return name + args;
    }

    // 目的: ロード時に [CustomRPC] メソッドがハッシュ順で決定論的に ID へ割り当てられることを検証
    [Fact]
    public void Load_AssignsDeterministicIds_And_MapsMethods()
    {
        EnsurePluginLogger();
        // Reset mappings for a clean test run
        CustomRPCManager.RpcMethods.Clear();
        CustomRPCManager.RpcMethodIds.Clear();

        // Discover all [CustomRPC] methods and compute expected ordering
        var assembly = SuperNewRolesPlugin.Assembly;
        var methods = assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            .Select(m => new { Method = m, Attr = m.GetCustomAttribute<CustomRPCAttribute>() })
            .Where(x => x.Attr != null)
            .Select(x => new { x.Method, Hash = ComputeRpcHash(x.Method) })
            .OrderBy(x => x.Hash, StringComparer.Ordinal)
            .ToList();

        // 目的: [CustomRPC] メソッドが最低1つ以上見つかること
        methods.Should().NotBeEmpty();

        // Pick two representative methods: a static one and an instance Ability method
        var staticTarget = methods.FirstOrDefault(x => x.Method.IsStatic);
        // 目的: 静的メソッド対象が存在すること
        staticTarget.Should().NotBeNull("there should be at least one static [CustomRPC] method");

        var instanceAbilityTarget = methods.FirstOrDefault(x => !x.Method.IsStatic && typeof(AbilityBase).IsAssignableFrom(x.Method.DeclaringType));
        // 目的: AbilityBase 派生のインスタンスメソッド対象が存在すること
        instanceAbilityTarget.Should().NotBeNull("there should be at least one AbilityBase instance [CustomRPC] method");

        // Execute load to populate mappings (patch actions are not executed in test)
        var actions = CustomRPCManager.Load();
        // 目的: Load が null 以外を返すこと
        actions.Should().NotBeNull();

        // Validate mapping for the selected methods
        var expectedStaticId = (byte)methods.FindIndex(x => x.Method == staticTarget!.Method);
        var expectedInstanceId = (byte)methods.FindIndex(x => x.Method == instanceAbilityTarget!.Method);

        // 目的: 静的ターゲットのハッシュが ID テーブルに含まれること
        CustomRPCManager.RpcMethodIds.Should().ContainKey(staticTarget!.Hash);
        // 目的: 割当 ID が期待通りであること
        CustomRPCManager.RpcMethodIds[staticTarget.Hash].Should().Be(expectedStaticId);
        // 目的: ID からメソッドが逆引きできること
        CustomRPCManager.RpcMethods[expectedStaticId].Should().BeSameAs(staticTarget.Method);

        // 目的: インスタンスターゲットのハッシュが ID テーブルに含まれること
        CustomRPCManager.RpcMethodIds.Should().ContainKey(instanceAbilityTarget!.Hash);
        // 目的: 割当 ID が期待通りであること
        CustomRPCManager.RpcMethodIds[instanceAbilityTarget.Hash].Should().Be(expectedInstanceId);
        // 目的: ID からメソッドが逆引きできること
        CustomRPCManager.RpcMethods[expectedInstanceId].Should().BeSameAs(instanceAbilityTarget.Method);
    }

    // 目的: 代表的なプリミティブ/コレクション/Unity 構造体が Read/Write テーブルへ登録されていることを検証
    [Fact]
    public void WriteRead_ActionTables_Contain_Expected_Types()
    {
        // Access private static dictionaries via reflection
        var t = typeof(CustomRPCManager);
        var writeField = t.GetField("WriteActions", BindingFlags.Static | BindingFlags.NonPublic);
        var readField = t.GetField("ReadActions", BindingFlags.Static | BindingFlags.NonPublic);
        writeField.Should().NotBeNull();
        readField.Should().NotBeNull();

        var write = (System.Collections.IDictionary)writeField!.GetValue(null)!;
        var read = (System.Collections.IDictionary)readField!.GetValue(null)!;

        // Core primitives
        // 目的: byte の書き込みが登録済み
        write.Contains(typeof(byte)).Should().BeTrue();
        // 目的: int の書き込みが登録済み
        write.Contains(typeof(int)).Should().BeTrue();
        // 目的: float の書き込みが登録済み
        write.Contains(typeof(float)).Should().BeTrue();
        // 目的: bool の書き込みが登録済み
        write.Contains(typeof(bool)).Should().BeTrue();
        // 目的: string の書き込みが登録済み
        write.Contains(typeof(string)).Should().BeTrue();

        // 目的: byte の読み取りが登録済み
        read.Contains(typeof(byte)).Should().BeTrue();
        // 目的: int の読み取りが登録済み
        read.Contains(typeof(int)).Should().BeTrue();
        // 目的: float の読み取りが登録済み
        read.Contains(typeof(float)).Should().BeTrue();
        // 目的: bool の読み取りが登録済み
        read.Contains(typeof(bool)).Should().BeTrue();
        // 目的: string の読み取りが登録済み
        read.Contains(typeof(string)).Should().BeTrue();

        // Collections present (types only — no instance creation)
        // 目的: List<string> の書き込みが登録済み
        write.Contains(typeof(List<string>)).Should().BeTrue();
        // 目的: List<string> の読み取りが登録済み
        read.Contains(typeof(List<string>)).Should().BeTrue();

        // 目的: Dictionary<string,int> の書き込みが登録済み
        write.Contains(typeof(Dictionary<string, int>)).Should().BeTrue();
        // 目的: Dictionary<string,int> の読み取りが登録済み
        read.Contains(typeof(Dictionary<string, int>)).Should().BeTrue();

        // Unity structs types registered
        // 目的: Vector2 の書き込みが登録済み
        write.Contains(typeof(Vector2)).Should().BeTrue();
        // 目的: Vector3 の書き込みが登録済み
        write.Contains(typeof(Vector3)).Should().BeTrue();
        // 目的: Vector2 の読み取りが登録済み
        read.Contains(typeof(Vector2)).Should().BeTrue();
        // 目的: Vector3 の読み取りが登録済み
        read.Contains(typeof(Vector3)).Should().BeTrue();
    }

    private static void SetAutoProp(object obj, string propName, object? value)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null) throw new InvalidOperationException($"Backing field for '{propName}' not found");
        f.SetValue(obj, value);
    }
}
