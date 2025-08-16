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

        methods.Should().NotBeEmpty();

        // Pick two representative methods: a static one and an instance Ability method
        var staticTarget = methods.FirstOrDefault(x => x.Method.IsStatic);
        staticTarget.Should().NotBeNull("there should be at least one static [CustomRPC] method");

        var instanceAbilityTarget = methods.FirstOrDefault(x => !x.Method.IsStatic && typeof(AbilityBase).IsAssignableFrom(x.Method.DeclaringType));
        instanceAbilityTarget.Should().NotBeNull("there should be at least one AbilityBase instance [CustomRPC] method");

        // Execute load to populate mappings (patch actions are not executed in test)
        var actions = CustomRPCManager.Load();
        actions.Should().NotBeNull();

        // Validate mapping for the selected methods
        var expectedStaticId = (byte)methods.FindIndex(x => x.Method == staticTarget!.Method);
        var expectedInstanceId = (byte)methods.FindIndex(x => x.Method == instanceAbilityTarget!.Method);

        CustomRPCManager.RpcMethodIds.Should().ContainKey(staticTarget!.Hash);
        CustomRPCManager.RpcMethodIds[staticTarget.Hash].Should().Be(expectedStaticId);
        CustomRPCManager.RpcMethods[expectedStaticId].Should().BeSameAs(staticTarget.Method);

        CustomRPCManager.RpcMethodIds.Should().ContainKey(instanceAbilityTarget!.Hash);
        CustomRPCManager.RpcMethodIds[instanceAbilityTarget.Hash].Should().Be(expectedInstanceId);
        CustomRPCManager.RpcMethods[expectedInstanceId].Should().BeSameAs(instanceAbilityTarget.Method);
    }

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
        write.Contains(typeof(byte)).Should().BeTrue();
        write.Contains(typeof(int)).Should().BeTrue();
        write.Contains(typeof(float)).Should().BeTrue();
        write.Contains(typeof(bool)).Should().BeTrue();
        write.Contains(typeof(string)).Should().BeTrue();

        read.Contains(typeof(byte)).Should().BeTrue();
        read.Contains(typeof(int)).Should().BeTrue();
        read.Contains(typeof(float)).Should().BeTrue();
        read.Contains(typeof(bool)).Should().BeTrue();
        read.Contains(typeof(string)).Should().BeTrue();

        // Collections present (types only — no instance creation)
        write.Contains(typeof(List<string>)).Should().BeTrue();
        read.Contains(typeof(List<string>)).Should().BeTrue();

        write.Contains(typeof(Dictionary<string, int>)).Should().BeTrue();
        read.Contains(typeof(Dictionary<string, int>)).Should().BeTrue();

        // Unity structs types registered
        write.Contains(typeof(Vector2)).Should().BeTrue();
        write.Contains(typeof(Vector3)).Should().BeTrue();
        read.Contains(typeof(Vector2)).Should().BeTrue();
        read.Contains(typeof(Vector3)).Should().BeTrue();
    }

    private static void SetAutoProp(object obj, string propName, object? value)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null) throw new InvalidOperationException($"Backing field for '{propName}' not found");
        f.SetValue(obj, value);
    }
}
