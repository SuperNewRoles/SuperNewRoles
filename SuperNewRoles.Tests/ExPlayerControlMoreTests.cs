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

public class ExPlayerControlMoreTests
{
    private class AlphaAbility : AbilityBase { }
    private class BetaAbility : AbilityBase { }

    private class TrackableAbility : AbilityBase
    {
        public bool Detached { get; private set; }
        public override void Detach()
        {
            Detached = true;
            // Avoid base which expects a valid Parent/Player in test context
        }
    }

    private static ExPlayerControl CreateBareEx(byte playerId = 1, RoleId role = RoleId.None)
    {
        var ex = (ExPlayerControl)FormatterServices.GetUninitializedObject(typeof(ExPlayerControl));

        SetAutoProp(ex, nameof(ExPlayerControl.Player), null);
        SetAutoProp(ex, nameof(ExPlayerControl.Data), null);
        SetAutoProp(ex, nameof(ExPlayerControl.PlayerId), playerId);
        SetAutoProp(ex, nameof(ExPlayerControl.AmOwner), false);
        SetAutoProp(ex, nameof(ExPlayerControl.Role), role);
        SetAutoProp(ex, nameof(ExPlayerControl.GhostRole), GhostRoleId.None);
        SetAutoProp(ex, nameof(ExPlayerControl.ModifierRole), ModifierRoleId.None);

        SetField(ex, "_abilityCache", new Dictionary<string, AbilityBase>());
        SetAutoProp(ex, nameof(ExPlayerControl.PlayerAbilities), new List<AbilityBase>());
        SetAutoProp(ex, nameof(ExPlayerControl.PlayerAbilitiesDictionary), new Dictionary<ulong, AbilityBase>());
        SetField(ex, "_impostorVisionAbilities", new List<ImpostorVisionAbility>());
        SetField(ex, "_hasAbilityCache", new Dictionary<string, bool>());

        SetField(ex, "_typeIdAbilityCache", new Dictionary<int, AbilityBase>());
        SetField(ex, "_typeIdAbilitiesCache", new Dictionary<int, List<AbilityBase>>());
        SetField(ex, "_typeIdReadOnlyCache", new Dictionary<int, IReadOnlyList<object>>());
        SetField(ex, "_hasAbilityByTypeId", new bool[1024]);
        SetField(ex, "_hasAbilityByTypeIdCached", new bool[1024]);

        typeof(ExPlayerControl).GetProperty(nameof(ExPlayerControl.lastAbilityId))!.SetValue(ex, 0);
        return ex;
    }

    private static void SetAutoProp(object obj, string propName, object? value)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null) throw new InvalidOperationException($"Backing field for '{propName}' not found");
        f.SetValue(obj, value);
    }
    private static void SetField(object obj, string fieldName, object? value)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null) throw new InvalidOperationException($"Field '{fieldName}' not found");
        f.SetValue(obj, value);
    }
    private static T GetField<T>(object obj, string fieldName)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (T)f.GetValue(obj)!;
    }

    private static ulong ShallowAttach(ExPlayerControl ex, AbilityBase ability)
    {
        var id = IRoleBase.GenerateAbilityId(ex.PlayerId, GetAutoProp<RoleId>(ex, nameof(ExPlayerControl.Role)), ex.lastAbilityId);
        typeof(ExPlayerControl).GetProperty(nameof(ExPlayerControl.lastAbilityId))!.SetValue(ex, ex.lastAbilityId + 1);
        ex.PlayerAbilities.Add(ability);
        ex.PlayerAbilitiesDictionary[id] = ability;
        // invalidate caches equivalent to attach
        GetField<Dictionary<string, bool>>(ex, "_hasAbilityCache").Clear();
        GetField<Dictionary<int, AbilityBase>>(ex, "_typeIdAbilityCache").Clear();
        GetField<Dictionary<int, List<AbilityBase>>>(ex, "_typeIdAbilitiesCache").Clear();
        GetField<Dictionary<int, IReadOnlyList<object>>>(ex, "_typeIdReadOnlyCache").Clear();
        Array.Clear(GetField<bool[]>(ex, "_hasAbilityByTypeIdCached"), 0, 1024);
        return id;
    }
    private static void ShallowDetach(ExPlayerControl ex, AbilityBase ability)
    {
        ulong id = 0;
        foreach (var kv in ex.PlayerAbilitiesDictionary)
        {
            if (ReferenceEquals(kv.Value, ability)) { id = kv.Key; break; }
        }
        ex.PlayerAbilities.Remove(ability);
        if (id != 0) ex.PlayerAbilitiesDictionary.Remove(id);
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
    public void TryGetAbility_Caches_And_Reflects_Detach()
    {
        var ex = CreateBareEx(playerId: 5, role: RoleId.Sheriff);
        var alpha = new AlphaAbility();
        var id = ShallowAttach(ex, alpha);

        ex.TryGetAbility<AlphaAbility>(out var first).Should().BeTrue();
        first.Should().BeSameAs(alpha);

        // Should hit cache now
        ex.TryGetAbility<AlphaAbility>(out var second).Should().BeTrue();
        second.Should().BeSameAs(alpha);

        // Detach and ensure cache invalidated via ShallowDetach
        ShallowDetach(ex, alpha);
        ex.TryGetAbility<AlphaAbility>(out var after).Should().BeFalse();
        after.Should().BeNull();

        // Also GetAbility(ulong) should return null after detach
        ex.GetAbility(id).Should().BeNull();
    }

    [Fact]
    public void GetAbilities_ReadOnly_Is_Cached_And_Refreshed()
    {
        var ex = CreateBareEx(playerId: 6, role: RoleId.Crewmate);
        var a1 = new AlphaAbility();
        var a2 = new AlphaAbility();
        ShallowAttach(ex, a1);

        var list1 = ex.GetAbilities<AlphaAbility>();
        list1.Count.Should().Be(1);

        // Repeated call should return same reference due to read-only cache
        var list2 = ex.GetAbilities<AlphaAbility>();
        ReferenceEquals(list1, list2).Should().BeTrue();

        // Now attach another AlphaAbility and ensure cache is cleared by our helper
        ShallowAttach(ex, a2);
        var list3 = ex.GetAbilities<AlphaAbility>();
        list3.Count.Should().Be(2);
        ReferenceEquals(list1, list3).Should().BeFalse();

        // Detach and check refresh again
        ShallowDetach(ex, a1);
        var list4 = ex.GetAbilities<AlphaAbility>();
        list4.Count.Should().Be(1);
    }

    [Fact]
    public void GetAbility_Generic_And_ById_Work_Together()
    {
        var ex = CreateBareEx(playerId: 7, role: RoleId.None);
        var beta = new BetaAbility();
        var id = ShallowAttach(ex, beta);

        ex.GetAbility<BetaAbility>().Should().BeSameAs(beta);
        ex.GetAbility(id).Should().BeSameAs(beta);
        ex.GetAbility<BetaAbility>(id).Should().BeSameAs(beta);
    }

    [Fact]
    public void Equals_And_HashCode_Based_On_PlayerId()
    {
        var ex1 = CreateBareEx(playerId: 9);
        var ex2 = CreateBareEx(playerId: 9);
        var ex3 = CreateBareEx(playerId: 10);

        ex1.Equals(ex2).Should().BeTrue();
        ex1.GetHashCode().Should().Be(ex2.GetHashCode());
        ex1.Equals(ex3).Should().BeFalse();
    }

    [Fact]
    public void ById_Returns_From_Static_Array()
    {
        var field = typeof(ExPlayerControl).GetField("_exPlayerControlsArray", BindingFlags.Static | BindingFlags.NonPublic)!;
        var original = (ExPlayerControl[]?)field.GetValue(null);
        try
        {
            var arr = new ExPlayerControl[256];
            var ex = CreateBareEx(playerId: 11);
            arr[11] = ex;
            field.SetValue(null, arr);

            var got = ExPlayerControl.ById(11);
            got.Should().BeSameAs(ex);
        }
        finally
        {
            field.SetValue(null, original);
        }
    }

    [Fact]
    public void Disconnected_Clears_Collections_And_Statics()
    {
        var ex = CreateBareEx(playerId: 12);
        var t = new TrackableAbility();
        var id = ShallowAttach(ex, t);

        // Prepare statics
        var field = typeof(ExPlayerControl).GetField("_exPlayerControlsArray", BindingFlags.Static | BindingFlags.NonPublic)!;
        var original = (ExPlayerControl[]?)field.GetValue(null);
        var arr = new ExPlayerControl[256];
        arr[12] = ex;
        field.SetValue(null, arr);
        ExPlayerControl.ExPlayerControls.Add(ex);

        ex.Disconnected();

        // ability.Detach invoked and cleared
        t.Detached.Should().BeTrue();
        ex.PlayerAbilities.Should().BeEmpty();
        ex.PlayerAbilitiesDictionary.Should().BeEmpty();
        ExPlayerControl.ExPlayerControls.Should().NotContain(ex);
        ((ExPlayerControl[])field.GetValue(null)!)[12].Should().BeNull();

        // caches cleared
        GetField<Dictionary<string, bool>>(ex, "_hasAbilityCache").Count.Should().Be(0);
        GetField<Dictionary<int, AbilityBase>>(ex, "_typeIdAbilityCache").Count.Should().Be(0);
        GetField<Dictionary<int, List<AbilityBase>>>(ex, "_typeIdAbilitiesCache").Count.Should().Be(0);
        GetField<Dictionary<int, IReadOnlyList<object>>>(ex, "_typeIdReadOnlyCache").Count.Should().Be(0);

        // restore statics
        field.SetValue(null, original);
    }
}
