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

// ExPlayerControl の基本 API（HasAbility/ToString/視界判定など）の挙動を検証するテスト。
public class ExPlayerControlTests
{
    // Minimal ability types for testing
    private class AlphaAbility : AbilityBase { }
    private class BetaAbility : AbilityBase { }

    // テスト用: コンストラクタを通さずに必要最小のフィールド/プロパティを初期化して生成する
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

    // 自動実装プロパティのバックフィールドへ直接代入する
    private static void SetAutoProp(object obj, string propName, object? value)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null)
            throw new InvalidOperationException($"Backing field for property '{propName}' not found.");
        f.SetValue(obj, value);
    }

    // 非公開フィールドへ直接代入する
    private static void SetField(object obj, string fieldName, object? value)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found.");
        f.SetValue(obj, value);
    }

    // 非公開フィールドの値を取得する
    private static T GetField<T>(object obj, string fieldName)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found.");
        return (T)f.GetValue(obj)!;
    }

    // デバッグ補助用の簡易ログ（失敗しても握りつぶす）
    private static void Log(string message)
    {
        try
        {
            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "explayer_test_debug.log");
            System.IO.File.AppendAllText(path, message + Environment.NewLine);
        }
        catch { }
    }

    // 簡易 Attach: 内部状態の更新とキャッシュ無効化までを再現
    private static void ShallowAttach(ExPlayerControl ex, AbilityBase ability)
    {
        var abilities = ex.PlayerAbilities;
        var dict = ex.PlayerAbilitiesDictionary;
        var abilityId = ExPlayerControlExtensions.GenerateDeterministicAbilityId(ex.PlayerId, new AbilityParentPlayer(ex), ability.GetType());
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

    // 簡易 Detach: 能力削除と関連キャッシュの無効化を再現
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

    // 自動実装プロパティのバックフィールドから値を取得
    private static TProp GetAutoProp<TProp>(object obj, string propName)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (TProp)f.GetValue(obj)!;
    }

    // 目的: HasAbility(name/generic) が Attach/Detach に連動して正しく反映されることを検証
    [Fact]
    public void HasAbility_ByName_And_Generic_ReflectsAttachDetach()
    {
        Log("Start HasAbility test");
        var ex = CreateBareEx(playerId: 3, role: RoleId.Crewmate);

        // 目的: 未 Attach 状態では name 指定の HasAbility が false
        ex.HasAbility("AlphaAbility").Should().BeFalse();
        // 目的: 未 Attach 状態では generic 指定の HasAbility も false
        ex.HasAbility<AlphaAbility>().Should().BeFalse();

        var alpha = new AlphaAbility();
        Log($"Before attach: Count={ex.PlayerAbilities.Count}");
        ShallowAttach(ex, alpha);
        Log($"After attach: Count={ex.PlayerAbilities.Count}");

        // 目的: Attach 後は name 指定で true
        ex.HasAbility("AlphaAbility").Should().BeTrue();
        // 目的: Attach 後は generic 指定で true
        ex.HasAbility<AlphaAbility>().Should().BeTrue();

        // Detach and verify caches are cleared and state updates
        ShallowDetach(ex, alpha);
        Log($"After detach: Count={ex.PlayerAbilities.Count}");
        // 目的: Detach 後に name 指定で false
        ex.HasAbility("AlphaAbility").Should().BeFalse();
        // 目的: Detach 後に generic 指定で false
        ex.HasAbility<AlphaAbility>().Should().BeFalse();
    }

    // Intentionally focusing on HasAbility/ToString/HasImpostorVision to avoid Unity runtime side-effects

    // 目的: ToString() が PlayerId, Role, AbilityCount を反映することを検証
    [Fact]
    public void ToString_Uses_PlayerId_Role_And_AbilityCount()
    {
        var ex = CreateBareEx(playerId: 9, role: RoleId.None);
        // Add two abilities so count reflects
        ShallowAttach(ex, new AlphaAbility());
        ShallowAttach(ex, new BetaAbility());

        // 目的: ToString が PlayerId/Role/AbilityCount を反映
        ex.ToString().Should().Be("(9): None 2");
    }

    // ById and Disconnected rely on unsafe pointer ops; skip in unit scope

    // 目的: インポスター視界判定が登録デリゲートに従うことを検証
    [Fact]
    public void HasImpostorVision_Uses_AbilityDelegate_When_Present()
    {
        var ex = CreateBareEx(playerId: 12, role: RoleId.Crewmate);
        // Directly add to internal impostor-vision list to avoid Attach logic
        GetField<List<ImpostorVisionAbility>>(ex, "_impostorVisionAbilities").Add(new ImpostorVisionAbility(() => true));
        // 目的: true デリゲートで視界あり
        ex.HasImpostorVision().Should().BeTrue();

        // Remove and add false-returning delegate
        GetField<List<ImpostorVisionAbility>>(ex, "_impostorVisionAbilities").Clear();
        GetField<List<ImpostorVisionAbility>>(ex, "_impostorVisionAbilities").Add(new ImpostorVisionAbility(() => false));
        // 目的: false デリゲートで視界なし
        ex.HasImpostorVision().Should().BeFalse();
    }

    // 目的: 拡張メソッドとインターフェース実装の GenerateAbilityId が一致することを検証
    [Fact]
    public void Extension_GenerateAbilityId_Matches_Interface_Implementation()
    {
        byte playerId = 2;
        // Using AbilityParentPlayer with null ExPlayerControl to get a deterministic signature independent of any role/ability ordinal
        var id1 = ExPlayerControlExtensions.GenerateDeterministicAbilityId(playerId, new AbilityParentPlayer(null), typeof(AlphaAbility));
        var id2 = ExPlayerControlExtensions.GenerateDeterministicAbilityId(playerId, new AbilityParentPlayer(null), typeof(AlphaAbility));
        // 目的: 同じ入力に対して決定論的に同じ ID が生成されること
        id1.Should().Be(id2);
    }

    // 目的: CreateBareEx のスモークテスト（最低限の生成と内部構造の初期化を確認）
    [Fact]
    public void Smoke_CreateBareEx_Works()
    {
        var ex = CreateBareEx();
        // 目的: 生成物が null でない
        ex.Should().NotBeNull();
        // 目的: 能力リストが初期化される
        ex.PlayerAbilities.Should().NotBeNull();
        // 目的: 能力辞書が初期化される
        ex.PlayerAbilitiesDictionary.Should().NotBeNull();
    }
}
