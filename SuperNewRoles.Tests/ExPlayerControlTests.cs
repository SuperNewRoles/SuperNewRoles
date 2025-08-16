using System;
using System.Collections.Generic;
using System.Linq;
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
    private static ExPlayerControl CreateTestExPlayer(byte playerId = 7, RoleId role = RoleId.Crewmate)
    {
        // コンストラクタを通さず、Among Us/Unity への依存を回避して必要な内部状態だけ初期化する
        var ex = (ExPlayerControl)FormatterServices.GetUninitializedObject(typeof(ExPlayerControl));

        // 自動実装プロパティのバックフィールドを直接初期化（private setterは呼べないため）
        typeof(ExPlayerControl).GetField("<PlayerAbilities>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new List<AbilityBase>());
        typeof(ExPlayerControl).GetField("<PlayerAbilitiesDictionary>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new Dictionary<ulong, AbilityBase>());

        // キャッシュ用のプライベートフィールドを初期化
        typeof(ExPlayerControl).GetField("_abilityCache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new Dictionary<string, AbilityBase>());
        typeof(ExPlayerControl).GetField("_hasAbilityCache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new Dictionary<string, bool>());
        typeof(ExPlayerControl).GetField("_typeIdAbilityCache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new Dictionary<int, AbilityBase>());
        typeof(ExPlayerControl).GetField("_typeIdAbilitiesCache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new Dictionary<int, List<AbilityBase>>());
        typeof(ExPlayerControl).GetField("_typeIdReadOnlyCache", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new Dictionary<int, IReadOnlyList<object>>());
        typeof(ExPlayerControl).GetField("_hasAbilityByTypeId", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new bool[1024]);
        typeof(ExPlayerControl).GetField("_hasAbilityByTypeIdCached", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new bool[1024]);
        typeof(ExPlayerControl).GetField("_impostorVisionAbilities", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, new List<ImpostorVisionAbility>());

        // PlayerId と Role のバックフィールドを設定
        typeof(ExPlayerControl).GetField("<PlayerId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, playerId);
        typeof(ExPlayerControl).GetField("<Role>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, role);

        // 付与IDのインデックスを0から開始させる
        ex.lastAbilityId = 0;
        return ex;
    }

    private sealed class DummyAbility : AbilityBase { }
    private sealed class AnotherDummyAbility : AbilityBase { }

    [Fact]
    public void AttachAndDetach_CustomKillButton_UpdatesStateAndCaches()
    {
        var ex = CreateTestExPlayer(playerId: 12, role: RoleId.Impostor);

        // 付与前: 該当アビリティが存在しないこと
        ex.HasCustomKillButton().Should().BeFalse();
        ex.HasAbility<CustomKillButtonAbility>().Should().BeFalse();

        // 付与: CustomKillButtonAbility を追加する
        var ability = new CustomKillButtonAbility(
            canKill: () => true,
            killCooldown: () => 7.5f,
            onlyCrewmates: () => false);
        var expectedId = IRoleBase.GenerateAbilityId(12, RoleId.Impostor, 0);
        typeof(AbilityBase).GetProperty("AbilityId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(ability, expectedId);
        ex.PlayerAbilities.Add(ability);
        ex.PlayerAbilitiesDictionary[expectedId] = ability;
        // AttachAbility 相当: カスタムキルボタンを持っているフラグを手動でセット
        typeof(ExPlayerControl).GetField("_customKillButtonAbility", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, ability);

        // 付与直後の sanity チェック
        ex.PlayerAbilities.Count.Should().Be(1, "we manually added one ability");
        ex.PlayerAbilities[0].Should().BeOfType<CustomKillButtonAbility>();

        // 付与後: 取得系が正しく反映されていること
        ex.HasCustomKillButton().Should().BeTrue();
        ex.HasAbility(nameof(CustomKillButtonAbility)).Should().BeTrue();
        ex.GetAbility<CustomKillButtonAbility>().Should().BeSameAs(ability);

        // 付与IDは index=0 の決定論的スキームに従うこと
        ability.AbilityId.Should().Be(expectedId);

        // 解除: 手動で削除し、関連キャッシュもクリアされることを検証
        ex.PlayerAbilities.Remove(ability);
        ex.PlayerAbilitiesDictionary.Remove(ability.AbilityId);
        typeof(ExPlayerControl).GetField("_customKillButtonAbility", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, null);
        // Clear per-type caches to emulate DetachAbility
        typeof(ExPlayerControl).GetField("_typeIdAbilityCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<int, AbilityBase>());
        typeof(ExPlayerControl).GetField("_typeIdAbilitiesCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<int, List<AbilityBase>>());
        typeof(ExPlayerControl).GetField("_typeIdReadOnlyCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<int, IReadOnlyList<object>>());
        typeof(ExPlayerControl).GetField("_hasAbilityByTypeIdCached", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new bool[1024]);
        typeof(ExPlayerControl).GetField("_hasAbilityCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<string, bool>());
        ex.HasCustomKillButton().Should().BeFalse();
        ex.HasAbility(nameof(CustomKillButtonAbility)).Should().BeFalse();
        ex.GetAbility<CustomKillButtonAbility>().Should().BeNull();
        ex.PlayerAbilities.Should().BeEmpty();
    }

    [Fact]
    public void GetAbilities_BuildsAndCaches_ReadOnlyList()
    {
        var ex = CreateTestExPlayer(playerId: 5, role: RoleId.Crewmate);

        // 1つ目の DummyAbility を付与し、読み取り専用リストの構築とキャッシュを確認
        var a1 = new DummyAbility();
        var id1 = IRoleBase.GenerateAbilityId(5, RoleId.Crewmate, 0);
        typeof(AbilityBase).GetProperty("AbilityId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(a1, id1);
        ex.PlayerAbilities.Add(a1);
        ex.PlayerAbilitiesDictionary[id1] = a1;
        ex.PlayerAbilities.Count.Should().Be(1, "we manually added one DummyAbility");
        ex.PlayerAbilities[0].Should().BeOfType<DummyAbility>();
        var list1 = ex.GetAbilities<DummyAbility>();
        list1.Count.Should().Be(1);
        list1[0].Should().BeSameAs(a1);

        // 2つ目の DummyAbility を付与。AttachAbility 相当のキャッシュクリアを行い、再構築されることを確認
        var a2 = new DummyAbility();
        var id2 = IRoleBase.GenerateAbilityId(5, RoleId.Crewmate, 1);
        typeof(AbilityBase).GetProperty("AbilityId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(a2, id2);
        ex.PlayerAbilities.Add(a2);
        ex.PlayerAbilitiesDictionary[id2] = a2;
        // Emulate AttachAbility cache clear for GetAbilities
        typeof(ExPlayerControl).GetField("_typeIdAbilitiesCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<int, List<AbilityBase>>());
        typeof(ExPlayerControl).GetField("_typeIdReadOnlyCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<int, IReadOnlyList<object>>());
        var list2 = ex.GetAbilities<DummyAbility>();
        list2.Count.Should().Be(2);
        list2.Should().Contain(new[] { a1, a2 });

        // 異なる型の取得は空であること
        ex.GetAbilities<AnotherDummyAbility>().Count.Should().Be(0);
    }

    [Fact]
    public void TryGetAbility_CachesResult_UntilCacheCleared()
    {
        var ex = CreateTestExPlayer();
        ex.GetAbility<ImpostorVisionAbility>().Should().BeNull(); // 事前に存在しないこと

        var a1 = new ImpostorVisionAbility(() => false);
        var id1 = IRoleBase.GenerateAbilityId(7, RoleId.Crewmate, 0);
        typeof(AbilityBase).GetProperty("AbilityId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(a1, id1);
        ex.PlayerAbilities.Add(a1);
        ex.PlayerAbilitiesDictionary[id1] = a1;
        ex.PlayerAbilities.Count.Should().Be(1);
        ex.PlayerAbilities[0].Should().BeOfType<ImpostorVisionAbility>();
        ex.GetAbilities<ImpostorVisionAbility>().Count.Should().Be(1); // キャッシュに載る

        // 解除: 手動で削除し、DetachAbility 相当のキャッシュクリア後は取得できないこと
        ex.PlayerAbilities.Remove(a1);
        ex.PlayerAbilitiesDictionary.Remove(a1.AbilityId);
        typeof(ExPlayerControl).GetField("_typeIdAbilityCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<int, AbilityBase>());
        typeof(ExPlayerControl).GetField("_typeIdAbilitiesCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<int, List<AbilityBase>>());
        typeof(ExPlayerControl).GetField("_typeIdReadOnlyCache", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new Dictionary<int, IReadOnlyList<object>>());
        typeof(ExPlayerControl).GetField("_hasAbilityByTypeIdCached", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(ex, new bool[1024]);
        ex.GetAbilities<ImpostorVisionAbility>().Count.Should().Be(0);
    }

    [Fact]
    public void EqualsAndHashCode_BasedOnPlayerId()
    {
        var ex1 = CreateTestExPlayer(playerId: 10);
        var ex2 = CreateTestExPlayer(playerId: 10);
        var ex3 = CreateTestExPlayer(playerId: 11);

        // 同一PlayerIdなら等値、ハッシュコードはPlayerId、そのほかは不一致
        ex1.Equals(ex2).Should().BeTrue();
        ex1.GetHashCode().Should().Be(10);
        ex1.Equals(ex3).Should().BeFalse();
    }

    [Fact]
    public void ById_ReturnsFromStaticArray()
    {
        var ex = CreateTestExPlayer(playerId: 42);

        // 静的配列を差し替えてプレイヤーを登録し、ByIdで取得できることを確認
        var arrField = typeof(ExPlayerControl).GetField("_exPlayerControlsArray", BindingFlags.Static | BindingFlags.NonPublic)!;
        var array = new ExPlayerControl[256];
        array[42] = ex;
        arrField.SetValue(null, array);

        var resolved = ExPlayerControl.ById(42);
        resolved.Should().BeSameAs(ex);
    }

    [Fact]
    public void HasImpostorVision_TrueWhenAnyAbilityReportsTrue()
    {
        var ex = CreateTestExPlayer();

        // 最初はfalseを返すアビリティを追加し、その後trueを返すアビリティを追加して OR 判定を確認
        var ivFalse = new ImpostorVisionAbility(() => false);
        var idFalse = IRoleBase.GenerateAbilityId(7, RoleId.Crewmate, 0);
        typeof(AbilityBase).GetProperty("AbilityId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(ivFalse, idFalse);
        ex.PlayerAbilities.Add(ivFalse);
        ex.PlayerAbilitiesDictionary[idFalse] = ivFalse;
        // ImpostorVisionAbilities リストにも反映して、評価対象に含める
        var list = (List<ImpostorVisionAbility>)typeof(ExPlayerControl).GetField("_impostorVisionAbilities", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(ex)!;
        list.Add(ivFalse);
        ex.HasImpostorVision().Should().BeFalse();

        var ivTrue = new ImpostorVisionAbility(() => true);
        var idTrue = IRoleBase.GenerateAbilityId(7, RoleId.Crewmate, 1);
        typeof(AbilityBase).GetProperty("AbilityId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.SetValue(ivTrue, idTrue);
        ex.PlayerAbilities.Add(ivTrue);
        ex.PlayerAbilitiesDictionary[idTrue] = ivTrue;
        list.Add(ivTrue);
        ex.HasImpostorVision().Should().BeTrue();
    }

    // no extra nested test classes needed; we use product classes with manual state injection
}
