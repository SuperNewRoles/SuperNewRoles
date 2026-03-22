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

// ExPlayerControl の能力キャッシュや切断時の後始末など、
// コアまわりの詳細挙動を集中的に検証するテスト群。
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

    // テスト用: コンストラクタを通さずに最小限の状態で ExPlayerControl を初期化する
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

    // リフレクションを用いて自動実装プロパティのバックフィールドへ直接代入する
    private static void SetAutoProp(object obj, string propName, object? value)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null) throw new InvalidOperationException($"Backing field for '{propName}' not found");
        f.SetValue(obj, value);
    }
    // 非公開フィールドへ直接代入するユーティリティ
    private static void SetField(object obj, string fieldName, object? value)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f == null) throw new InvalidOperationException($"Field '{fieldName}' not found");
        f.SetValue(obj, value);
    }
    // 非公開フィールドの値を取得するユーティリティ
    private static T GetField<T>(object obj, string fieldName)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (T)f.GetValue(obj)!;
    }

    // 簡易 Attach: 内部キャッシュの無効化まで含め、Attach 相当の状態遷移を再現する
    private static ulong ShallowAttach(ExPlayerControl ex, AbilityBase ability)
    {
        // Generate deterministic ability id using current player as parent for stable tests
        var id = ExPlayerControlExtensions.GenerateDeterministicAbilityId(ex.PlayerId, new AbilityParentPlayer(ex), ability.GetType());
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
    // 簡易 Detach: 能力の削除と関連キャッシュの無効化を行う
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
    // 自動実装プロパティのバックフィールドから値を取得する
    private static TProp GetAutoProp<TProp>(object obj, string propName)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (TProp)f.GetValue(obj)!;
    }

    // 目的: TryGetAbility のキャッシュ動作と Detach 後の反映を検証
    [Fact]
    public void TryGetAbility_Caches_And_Reflects_Detach()
    {
        var ex = CreateBareEx(playerId: 5, role: RoleId.Sheriff);
        var alpha = new AlphaAbility();
        var id = ShallowAttach(ex, alpha);

        // 目的: 最初の取得が成功すること
        ex.TryGetAbility<AlphaAbility>(out var first).Should().BeTrue();
        // 目的: 取得したインスタンスが同一参照であること
        first.Should().BeSameAs(alpha);

        // Should hit cache now
        // 目的: キャッシュ利用後も取得が成功すること
        ex.TryGetAbility<AlphaAbility>(out var second).Should().BeTrue();
        // 目的: キャッシュ経由でも同一参照が返ること
        second.Should().BeSameAs(alpha);

        // Detach and ensure cache invalidated via ShallowDetach
        ShallowDetach(ex, alpha);
        // 目的: Detach 後は取得に失敗すること
        ex.TryGetAbility<AlphaAbility>(out var after).Should().BeFalse();
        // 目的: 取得結果が null であること
        after.Should().BeNull();

        // Also GetAbility(ulong) should return null after detach
        // 目的: ID 指定の取得も Detach 後は null を返すこと
        ex.GetAbility(id).Should().BeNull();
    }

    // 目的: GetAbilities<T>() の ReadOnly キャッシュが適切に再利用・更新されるかを検証
    [Fact]
    public void GetAbilities_ReadOnly_Is_Cached_And_Refreshed()
    {
        var ex = CreateBareEx(playerId: 6, role: RoleId.Crewmate);
        var a1 = new AlphaAbility();
        var a2 = new AlphaAbility();
        ShallowAttach(ex, a1);

        var list1 = ex.GetAbilities<AlphaAbility>();
        // 目的: 初回は1件のみであること
        list1.Count.Should().Be(1);

        // Repeated call should return same reference due to read-only cache
        var list2 = ex.GetAbilities<AlphaAbility>();
        // 目的: ReadOnly キャッシュにより同一参照が返ること
        ReferenceEquals(list1, list2).Should().BeTrue();

        // Now attach another AlphaAbility and ensure cache is cleared by our helper
        ShallowAttach(ex, a2);
        var list3 = ex.GetAbilities<AlphaAbility>();
        // 目的: 追加後は2件に増えること
        list3.Count.Should().Be(2);
        // 目的: キャッシュが更新され参照が変わること
        ReferenceEquals(list1, list3).Should().BeFalse();

        // Detach and check refresh again
        ShallowDetach(ex, a1);
        var list4 = ex.GetAbilities<AlphaAbility>();
        // 目的: Detach 後は1件へ減ること
        list4.Count.Should().Be(1);
    }

    // 目的: ジェネリック版と ID 指定版の GetAbility が同一オブジェクトを返すことを検証
    [Fact]
    public void GetAbility_Generic_And_ById_Work_Together()
    {
        var ex = CreateBareEx(playerId: 7, role: RoleId.None);
        var beta = new BetaAbility();
        var id = ShallowAttach(ex, beta);

        // 目的: ジェネリック版取得で同一参照が得られること
        ex.GetAbility<BetaAbility>().Should().BeSameAs(beta);
        // 目的: ID 取得でも同一参照が得られること
        ex.GetAbility(id).Should().BeSameAs(beta);
        // 目的: ジェネリック+ID 取得でも同一参照が得られること
        ex.GetAbility<BetaAbility>(id).Should().BeSameAs(beta);
    }

    // 目的: Equals/GetHashCode が PlayerId ベースで動作することを検証
    [Fact]
    public void Equals_And_HashCode_Based_On_PlayerId()
    {
        var ex1 = CreateBareEx(playerId: 9);
        var ex2 = CreateBareEx(playerId: 9);
        var ex3 = CreateBareEx(playerId: 10);

        // 目的: 同一 PlayerId なら等値であること
        ex1.Equals(ex2).Should().BeTrue();
        // 目的: 同一 PlayerId ならハッシュも一致すること
        ex1.GetHashCode().Should().Be(ex2.GetHashCode());
        // 目的: 異なる PlayerId なら非等値であること
        ex1.Equals(ex3).Should().BeFalse();
    }

    // 目的: 静的配列からの ById 取得が期待通りであることを検証
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
            // 目的: 静的配列から同一インスタンスが取得できること
            got.Should().BeSameAs(ex);
        }
        finally
        {
            field.SetValue(null, original);
        }
    }

    // 目的: Disconnected() がコレクションと静的状態をクリーンアップすることを検証
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
        // 目的: Disconnected により Ability.Detach が呼ばれること
        t.Detached.Should().BeTrue();
        // 目的: 能力リストがクリアされること
        ex.PlayerAbilities.Should().BeEmpty();
        // 目的: 能力辞書がクリアされること
        ex.PlayerAbilitiesDictionary.Should().BeEmpty();
        // 目的: 静的コレクションから自身が取り除かれること
        ExPlayerControl.ExPlayerControls.Should().NotContain(ex);
        // 目的: 静的配列の該当要素が null にされること
        ((ExPlayerControl[])field.GetValue(null)!)[12].Should().BeNull();

        // caches cleared
        // 目的: hasAbility キャッシュが空であること
        GetField<Dictionary<string, bool>>(ex, "_hasAbilityCache").Count.Should().Be(0);
        // 目的: typeId→Ability キャッシュが空であること
        GetField<Dictionary<int, AbilityBase>>(ex, "_typeIdAbilityCache").Count.Should().Be(0);
        // 目的: typeId→Abilities キャッシュが空であること
        GetField<Dictionary<int, List<AbilityBase>>>(ex, "_typeIdAbilitiesCache").Count.Should().Be(0);
        // 目的: typeId→ReadOnlyList キャッシュが空であること
        GetField<Dictionary<int, IReadOnlyList<object>>>(ex, "_typeIdReadOnlyCache").Count.Should().Be(0);

        // restore statics
        field.SetValue(null, original);
    }
}
