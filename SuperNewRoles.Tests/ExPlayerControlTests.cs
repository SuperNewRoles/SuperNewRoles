using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using Xunit;

namespace SuperNewRoles.Tests;

public class ExPlayerControlTests
{
    // Minimal ability types for testing
    private class AlphaAbility : AbilityBase { }
    private class BetaAbility : AbilityBase { }

    // Helper: create ExPlayerControl without invoking its ctor and initialize required fields
    private static ExPlayerControl CreateBareEx(byte playerId = 7, RoleId role = RoleId.None)
    {
        var ex = (ExPlayerControl)FormatterServices.GetUninitializedObject(typeof(ExPlayerControl));
        // Initialize auto-properties' backing fields
        SetAutoProp(ex, nameof(ExPlayerControl.Player), null);
        SetAutoProp(ex, nameof(ExPlayerControl.Data), null);
        SetAutoProp(ex, nameof(ExPlayerControl.PlayerId), playerId);
        SetAutoProp(ex, nameof(ExPlayerControl.AmOwner), false);
        SetAutoProp(ex, nameof(ExPlayerControl.Role), role);
        SetAutoProp(ex, nameof(ExPlayerControl.GhostRole), GhostRoleId.None);
        SetAutoProp(ex, nameof(ExPlayerControl.ModifierRole), ModifierRoleId.None);

        // Initialize lists/dicts used by core logic
        SetField(ex, "_abilityCache", new Dictionary<string, AbilityBase>());
        SetAutoProp(ex, nameof(ExPlayerControl.PlayerAbilities), new List<AbilityBase>());
        SetAutoProp(ex, nameof(ExPlayerControl.PlayerAbilitiesDictionary), new Dictionary<ulong, AbilityBase>());
        SetField(ex, "_impostorVisionAbilities", new List<ImpostorVisionAbility>());
        SetField(ex, "_hasAbilityCache", new Dictionary<string, bool>());

        // Initialize optimized caches
        SetField(ex, "_typeIdAbilityCache", new Dictionary<int, AbilityBase>());
        SetField(ex, "_typeIdAbilitiesCache", new Dictionary<int, List<AbilityBase>>());
        SetField(ex, "_typeIdReadOnlyCache", new Dictionary<int, IReadOnlyList<object>>());
        SetField(ex, "_hasAbilityByTypeId", new bool[1024]);
        SetField(ex, "_hasAbilityByTypeIdCached", new bool[1024]);

        // Public auto-property
        typeof(ExPlayerControl).GetProperty(nameof(ExPlayerControl.lastAbilityId))!.SetValue(ex, 0);

        return ex;
    }

    private static void SetAutoProp(object obj, string propName, object? value)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null)
            throw new InvalidOperationException($"Backing field for property '{propName}' not found.");
        f.SetValue(obj, value);
    }

    private static void SetField(object obj, string fieldName, object? value)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found.");
        f.SetValue(obj, value);
    }

    private static T GetField<T>(object obj, string fieldName)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found.");
        return (T)f.GetValue(obj)!;
    }

    private static void Log(string message)
    {
        try
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "explayer_test_debug.log");
            System.IO.File.AppendAllText(path, message + Environment.NewLine);
        }
        catch { }
    }

    private static void ShallowAttach(ExPlayerControl ex, AbilityBase ability)
    {
        var abilities = ex.PlayerAbilities;
        var dict = ex.PlayerAbilitiesDictionary;
        var abilityId = IRoleBase.GenerateAbilityId(ex.PlayerId, GetAutoProp<RoleId>(ex, nameof(ExPlayerControl.Role)), ex.lastAbilityId);
        typeof(ExPlayerControl).GetProperty(nameof(ExPlayerControl.lastAbilityId))!.SetValue(ex, ex.lastAbilityId + 1);

        abilities.Add(ability);
        dict[abilityId] = ability;

        // Reset caches (equivalent to internal attach invalidation)
        GetField<Dictionary<string, bool>>(ex, "_hasAbilityCache").Clear();
        GetField<Dictionary<int, AbilityBase>>(ex, "_typeIdAbilityCache").Clear();
        GetField<Dictionary<int, List<AbilityBase>>>(ex, "_typeIdAbilitiesCache").Clear();
        GetField<Dictionary<int, IReadOnlyList<object>>>(ex, "_typeIdReadOnlyCache").Clear();
        Array.Clear(GetField<bool[]>(ex, "_hasAbilityByTypeIdCached"), 0, 1024);
    }

    private static void ShallowDetach(ExPlayerControl ex, AbilityBase ability)
    {
        var dict = ex.PlayerAbilitiesDictionary;
        // Find by value since we generated ids locally
        ulong key = 0;
        foreach (var kv in dict)
        {
            if (ReferenceEquals(kv.Value, ability)) { key = kv.Key; break; }
        }
        ex.PlayerAbilities.Remove(ability);
        if (key != 0) dict.Remove(key);

        // Reset caches
        GetField<Dictionary<string, bool>>(ex, "_hasAbilityCache").Clear();
        GetField<Dictionary<int, AbilityBase>>(ex, "_typeIdAbilityCache").Clear();
        GetField<Dictionary<int, List<AbilityBase>>>(ex, "_typeIdAbilitiesCache").Clear();
        GetField<Dictionary<int, IReadOnlyList<object>>>(ex, "_typeIdReadOnlyCache").Clear();
        Array.Clear(GetField<bool[]>(ex, "_hasAbilityByTypeIdCached"), 0, 1024);
    }

    private static TProp GetAutoProp<TProp>(object obj, string propName)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (TProp)f.GetValue(obj)!;
    }

    [Fact]
    public void HasAbility_ByName_And_Generic_ReflectsAttachDetach()
    {
        Log("Start HasAbility test");
        var ex = CreateBareEx(playerId: 3, role: RoleId.Crewmate);

        ex.HasAbility("AlphaAbility").Should().BeFalse();
        ex.HasAbility<AlphaAbility>().Should().BeFalse();

        var alpha = new AlphaAbility();
        Log($"Before attach: Count={ex.PlayerAbilities.Count}");
        ShallowAttach(ex, alpha);
        Log($"After attach: Count={ex.PlayerAbilities.Count}");

        ex.HasAbility("AlphaAbility").Should().BeTrue();
        ex.HasAbility<AlphaAbility>().Should().BeTrue();

        // Detach and verify caches are cleared and state updates
        ShallowDetach(ex, alpha);
        Log($"After detach: Count={ex.PlayerAbilities.Count}");
        ex.HasAbility("AlphaAbility").Should().BeFalse();
        ex.HasAbility<AlphaAbility>().Should().BeFalse();
    }

    // Intentionally focusing on HasAbility/ToString/HasImpostorVision to avoid Unity runtime side-effects

    [Fact]
    public void ToString_Uses_PlayerId_Role_And_AbilityCount()
    {
        var ex = CreateBareEx(playerId: 9, role: RoleId.None);
        // Add two abilities so count reflects
        ShallowAttach(ex, new AlphaAbility());
        ShallowAttach(ex, new BetaAbility());

        ex.ToString().Should().Be("(9): None 2");
    }

    // ById and Disconnected rely on unsafe pointer ops; skip in unit scope

    [Fact]
    public void HasImpostorVision_Uses_AbilityDelegate_When_Present()
    {
        var ex = CreateBareEx(playerId: 12, role: RoleId.Crewmate);
        // Directly add to internal impostor-vision list to avoid Attach logic
        GetField<List<ImpostorVisionAbility>>(ex, "_impostorVisionAbilities").Add(new ImpostorVisionAbility(() => true));
        ex.HasImpostorVision().Should().BeTrue();

        // Remove and add false-returning delegate
        GetField<List<ImpostorVisionAbility>>(ex, "_impostorVisionAbilities").Clear();
        GetField<List<ImpostorVisionAbility>>(ex, "_impostorVisionAbilities").Add(new ImpostorVisionAbility(() => false));
        ex.HasImpostorVision().Should().BeFalse();
    }

    [Fact]
    public void Extension_GenerateAbilityId_Matches_Interface_Implementation()
    {
        byte playerId = 2; var role = RoleId.Sheriff; int index = 5;
        var ext = ExPlayerControlExtensions.GenerateAbilityId(playerId, role, index);
        var iface = IRoleBase.GenerateAbilityId(playerId, role, index);
        ext.Should().Be(iface);
    }

    [Fact]
    public void Smoke_CreateBareEx_Works()
    {
        var ex = CreateBareEx();
        ex.Should().NotBeNull();
        ex.PlayerAbilities.Should().NotBeNull();
        ex.PlayerAbilitiesDictionary.Should().NotBeNull();
    }
}
