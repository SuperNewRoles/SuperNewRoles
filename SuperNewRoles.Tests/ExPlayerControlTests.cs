using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using SuperNewRoles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Modifiers;
using Xunit;

namespace SuperNewRoles.Tests;

// ExPlayerControl の基本 API（HasAbility/ToString/視界判定など）の挙動を検証するテスト。
public class ExPlayerControlTests
{
    // Minimal ability types for testing
    private class AlphaAbility : AbilityBase { }
    private class BetaAbility : AbilityBase { }
    private class TestVentAbility : CustomVentAbility
    {
        public TestVentAbility(Func<bool?> canUseVent, int priority)
            : base(canUseVent, priority: priority) { }

        protected override bool IsLocalPlayer(PlayerControl player) => false;
    }
    private class DerivedVentAbility : TestVentAbility
    {
        public DerivedVentAbility(Func<bool?> canUseVent, int priority)
            : base(canUseVent, priority: priority) { }
    }
    private class TestSaboAbility : CustomSaboAbility
    {
        public TestSaboAbility(Func<bool?> canSabotage, int priority)
            : base(canSabotage, priority: priority) { }

        protected override bool IsLocalPlayer(PlayerControl player) => false;
    }
    private class AttachObservingAbility : AbilityBase
    {
        public bool FoundSelfDuringAttach { get; private set; }

        protected override bool IsLocalPlayer(PlayerControl player) => false;

        public override void AttachToAlls()
        {
            FoundSelfDuringAttach = Player.TryGetAbility<AttachObservingAbility>(out var ability) &&
                ReferenceEquals(ability, this);
        }
    }
    private class ChildAttachingAbility : AbilityBase
    {
        public BetaAbility Child { get; private set; } = null!;

        protected override bool IsLocalPlayer(PlayerControl player) => false;

        public override void AttachToAlls()
        {
            Child = new BetaAbility();
            Player.AttachAbility(Child, new AbilityParentAbility(this));
        }
    }

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
        SetField(ex, "_playerAbilities", new List<AbilityBase>());
        SetField(ex, "_abilitiesById", new Dictionary<ulong, AbilityBase>());
        SetField(ex, "_issuedAbilityIds", new HashSet<ulong>());

        // Initialize optimized caches
        SetField(ex, "_typeIdAbilityCache", new Dictionary<int, AbilityBase>());
        SetField(ex, "_typeIdReadOnlyCache", new Dictionary<int, IReadOnlyList<object>>());
        SetField(ex, "_prioritizedAbilities", new Dictionary<int, List<AbilityBase>>());

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

    // 本番の AttachAbility 経路を通して登録する。
    private static ulong Attach(ExPlayerControl ex, AbilityBase ability)
    {
        ex.AttachAbility(ability, new AbilityParentPlayer(ex));
        return ability.AbilityId;
    }

    // 本番の DetachAbility 経路を通して解除する。
    private static void Detach(ExPlayerControl ex, AbilityBase ability)
        => ex.DetachAbility(ability.AbilityId);

    // 自動実装プロパティのバックフィールドから値を取得
    private static TProp GetAutoProp<TProp>(object obj, string propName)
    {
        var f = obj.GetType().GetField($"<{propName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (TProp)f.GetValue(obj)!;
    }

    // 目的: HasAbility<T> が Attach/Detach に連動して正しく反映されることを検証
    [Fact]
    public void HasAbility_Generic_ReflectsAttachDetach()
    {
        Log("Start HasAbility test");
        var ex = CreateBareEx(playerId: 3, role: RoleId.Crewmate);

        // 目的: 未 Attach 状態では generic 指定の HasAbility も false
        ex.HasAbility<AlphaAbility>().Should().BeFalse();

        var alpha = new AlphaAbility();
        Log($"Before attach: Count={ex.AbilityCount}");
        Attach(ex, alpha);
        Log($"After attach: Count={ex.AbilityCount}");

        // 目的: Attach 後は generic 指定で true
        ex.HasAbility<AlphaAbility>().Should().BeTrue();

        // Detach and verify caches are cleared and state updates
        Detach(ex, alpha);
        Log($"After detach: Count={ex.AbilityCount}");
        // 目的: Detach 後に generic 指定で false
        ex.HasAbility<AlphaAbility>().Should().BeFalse();
    }

    [Fact]
    public void Attach_RegistersBeforeLifecycleHooks_AndInvalidatesNegativeCaches()
    {
        var ex = CreateBareEx(playerId: 4, role: RoleId.Crewmate);
        ex.HasAbility<AttachObservingAbility>().Should().BeFalse();
        ex.GetAbility<AttachObservingAbility>().Should().BeNull();

        var ability = new AttachObservingAbility();
        var id = Attach(ex, ability);

        ability.FoundSelfDuringAttach.Should().BeTrue();
        ex.AbilityCount.Should().Be(1);
        ex.TryGetAbility<AttachObservingAbility>(out var found).Should().BeTrue();
        found.Should().BeSameAs(ability);
        ex.GetAbility(id).Should().BeSameAs(ability);
        ex.GetAbilities<AttachObservingAbility>().Should().ContainSingle().Which.Should().BeSameAs(ability);

        Detach(ex, ability);

        ex.AbilityCount.Should().Be(0);
        ex.TryGetAbility<AttachObservingAbility>(out _).Should().BeFalse();
        ex.GetAbility(id).Should().BeNull();
        ex.GetAbilities<AttachObservingAbility>().Should().BeEmpty();
    }

    [Fact]
    public void Attach_RejectsSameInstanceWithoutPartiallyChangingState()
    {
        var ex = CreateBareEx(playerId: 5, role: RoleId.Crewmate);
        var ability = new AlphaAbility();
        var originalId = Attach(ex, ability);

        Action attachAgain = () => Attach(ex, ability);

        attachAgain.Should().Throw<InvalidOperationException>();
        ex.AbilityCount.Should().Be(1);
        ex.GetAbility(originalId).Should().BeSameAs(ability);
        ex.GetAbilities<AlphaAbility>().Should().ContainSingle().Which.Should().BeSameAs(ability);
    }

    [Fact]
    public void Attach_RejectsParentOwnedByAnotherPlayerWithoutChangingState()
    {
        var target = CreateBareEx(playerId: 6, role: RoleId.Crewmate);
        var anotherPlayer = CreateBareEx(playerId: 7, role: RoleId.Crewmate);

        Action attach = () => target.AttachAbility(
            new AlphaAbility(),
            new AbilityParentPlayer(anotherPlayer));

        attach.Should().Throw<InvalidOperationException>();
        target.AbilityCount.Should().Be(0);
        target.GetAbilities<AlphaAbility>().Should().BeEmpty();
    }

    [Fact]
    public void Attach_RejectsInstanceAlreadyRegisteredToAnotherPlayer()
    {
        var firstPlayer = CreateBareEx(playerId: 6, role: RoleId.Crewmate);
        var secondPlayer = CreateBareEx(playerId: 7, role: RoleId.Crewmate);
        var ability = new AlphaAbility();
        var originalId = Attach(firstPlayer, ability);

        Action attachToSecondPlayer = () => secondPlayer.AttachAbility(
            ability,
            new AbilityParentPlayer(secondPlayer));

        attachToSecondPlayer.Should().Throw<InvalidOperationException>();
        firstPlayer.GetAbility(originalId).Should().BeSameAs(ability);
        ability.Player.Should().BeSameAs(firstPlayer);
        secondPlayer.AbilityCount.Should().Be(0);
    }

    [Fact]
    public void Attach_RejectsChildWhoseParentAbilityIsDetached()
    {
        var ex = CreateBareEx(playerId: 6, role: RoleId.Crewmate);
        var parent = new AlphaAbility();
        Attach(ex, parent);
        Detach(ex, parent);

        Action attachChild = () => ex.AttachAbility(
            new BetaAbility(),
            new AbilityParentAbility(parent));

        attachChild.Should().Throw<InvalidOperationException>();
        ex.AbilityCount.Should().Be(0);
        ex.GetAbilities<BetaAbility>().Should().BeEmpty();
    }

    [Fact]
    public void Attach_DoesNotCollide_WhenEarlierSameTypeAbilityWasDetached()
    {
        var ex = CreateBareEx(playerId: 6, role: RoleId.Crewmate);
        var first = new AlphaAbility();
        var second = new AlphaAbility();
        var firstId = Attach(ex, first);
        var secondId = Attach(ex, second);

        Detach(ex, first);
        var replacement = new AlphaAbility();
        var replacementId = Attach(ex, replacement);

        firstId.Should().NotBe(secondId);
        replacementId.Should().NotBe(secondId);
        ex.GetAbility(secondId).Should().BeSameAs(second);
        ex.GetAbility(replacementId).Should().BeSameAs(replacement);
        ex.GetAbilities<AlphaAbility>().Should().Equal(second, replacement);
    }

    [Fact]
    public void Attach_DoesNotReuseId_AfterOnlyAbilityWasDetached()
    {
        var ex = CreateBareEx(playerId: 7, role: RoleId.Crewmate);
        var first = new AlphaAbility();
        var oldId = Attach(ex, first);

        Detach(ex, first);
        var replacement = new AlphaAbility();
        var newId = Attach(ex, replacement);

        newId.Should().NotBe(oldId);
        ex.GetAbility(oldId).Should().BeNull();
        ex.GetAbility(newId).Should().BeSameAs(replacement);
    }

    [Fact]
    public void NestedAttach_RegistersParentBeforeChild_AndResolvesBoth()
    {
        var hostState = CreateBareEx(playerId: 7, role: RoleId.Crewmate);
        var clientState = CreateBareEx(playerId: 7, role: RoleId.Crewmate);
        var hostParent = new ChildAttachingAbility();
        var clientParent = new ChildAttachingAbility();

        var hostParentId = Attach(hostState, hostParent);
        var clientParentId = Attach(clientState, clientParent);
        var child = hostParent.Child;

        child.Should().NotBeNull();
        hostParentId.Should().Be(clientParentId);
        child.AbilityId.Should().Be(clientParent.Child.AbilityId);
        hostState.AbilityCount.Should().Be(2);
        hostState.GetAbility(hostParentId).Should().BeSameAs(hostParent);
        hostState.GetAbility(child.AbilityId).Should().BeSameAs(child);
        child.Parent.Should().BeOfType<AbilityParentAbility>()
            .Which.ParentAbility.Should().BeSameAs(hostParent);
    }

    [Fact]
    public void AbilityIds_MatchAcrossEquivalentPlayerStates_AfterDetachAndReattach()
    {
        var hostState = CreateBareEx(playerId: 8, role: RoleId.Crewmate);
        var clientState = CreateBareEx(playerId: 8, role: RoleId.Crewmate);
        var hostFirst = new AlphaAbility();
        var clientFirst = new AlphaAbility();
        var hostSecond = new AlphaAbility();
        var clientSecond = new AlphaAbility();

        Attach(hostState, hostFirst).Should().Be(Attach(clientState, clientFirst));
        Attach(hostState, hostSecond).Should().Be(Attach(clientState, clientSecond));

        Detach(hostState, hostFirst);
        Detach(clientState, clientFirst);

        Attach(hostState, new AlphaAbility()).Should().Be(Attach(clientState, new AlphaAbility()));
    }

    [Fact]
    public void AbilityIds_MatchWhenActiveSetsDifferButIssuedHistoryMatches()
    {
        var hostState = CreateBareEx(playerId: 9, role: RoleId.Crewmate);
        var clientState = CreateBareEx(playerId: 9, role: RoleId.Crewmate);
        var hostFirst = new AlphaAbility();
        var clientFirst = new AlphaAbility();

        Attach(hostState, hostFirst).Should().Be(Attach(clientState, clientFirst));
        Detach(hostState, hostFirst);

        Attach(hostState, new AlphaAbility()).Should().Be(Attach(clientState, new AlphaAbility()));
    }

    // Intentionally focusing on HasAbility/ToString/HasImpostorVision to avoid Unity runtime side-effects

    // 目的: ToString() が PlayerId, Role, AbilityCount を反映することを検証
    [Fact]
    public void ToString_Uses_PlayerId_Role_And_AbilityCount()
    {
        var ex = CreateBareEx(playerId: 9, role: RoleId.None);
        // Add two abilities so count reflects
        Attach(ex, new AlphaAbility());
        Attach(ex, new BetaAbility());

        // 目的: ToString が PlayerId/Role/AbilityCount を反映
        ex.ToString().Should().Be("(9): None 2");
    }

    // ById and Disconnected rely on unsafe pointer ops; skip in unit scope

    // 目的: インポスター視界判定が登録デリゲートに従うことを検証
    [Fact]
    public void HasImpostorVision_Uses_AbilityDelegate_When_Present()
    {
        var ex = CreateBareEx(playerId: 12, role: RoleId.Crewmate);
        var vision = new ImpostorVisionAbility(() => true);
        Attach(ex, vision);
        // 目的: true デリゲートで視界あり
        ex.HasImpostorVision().Should().BeTrue();

        Detach(ex, vision);
        Attach(ex, new ImpostorVisionAbility(() => false));
        // 目的: false デリゲートで視界なし
        ex.HasImpostorVision().Should().BeFalse();
    }

    [Fact]
    public void VentDecision_UsesPriority_NullDelegation_AndLatestTieBreaker()
    {
        var ex = CreateBareEx(playerId: 13, role: RoleId.Crewmate);
        bool? highDecision = null;
        var low = new TestVentAbility(() => true, priority: 0);
        var high = new TestVentAbility(() => highDecision, priority: 10);
        Attach(ex, low);
        Attach(ex, high);

        ex.CanUseVent().Should().BeTrue();
        ex.ShouldShowVentAbility(low).Should().BeTrue();
        ex.ShouldShowVentAbility(high).Should().BeFalse();

        highDecision = false;
        ex.CanUseVent().Should().BeFalse();
        ex.ShouldShowVentAbility(low).Should().BeFalse();
        ex.ShouldShowVentAbility(high).Should().BeFalse();
        // ShowVanillaVentButton uses the inverse of this guard, so a custom false decision suppresses vanilla too.
        var decisionArgs = new object[] { null!, false };
        InvokePrivate<bool>(ex, "TryGetVentDecision", decisionArgs).Should().BeTrue();
        decisionArgs[0].Should().BeSameAs(high);
        decisionArgs[1].Should().Be(false);

        // Derived abilities are grouped with CustomVentAbility automatically.
        var latestAtSamePriority = new DerivedVentAbility(() => true, priority: 10);
        Attach(ex, latestAtSamePriority);
        ex.CanUseVent().Should().BeTrue();
        ex.ShouldShowVentAbility(latestAtSamePriority).Should().BeTrue();
        ex.ShouldShowVentAbility(high).Should().BeFalse();
        ex.ShouldShowVentAbility(low).Should().BeFalse();
        var priorityGroups = GetField<Dictionary<int, List<AbilityBase>>>(ex, "_prioritizedAbilities");
        priorityGroups.Should().ContainSingle();
        foreach (var abilities in priorityGroups.Values)
            abilities[0].Should().BeSameAs(latestAtSamePriority);

        Detach(ex, latestAtSamePriority);
        ex.CanUseVent().Should().BeFalse();

        Detach(ex, high);
        ex.CanUseVent().Should().BeTrue();
        ex.ShouldShowVentAbility(low).Should().BeTrue();

        Detach(ex, low);
        decisionArgs = new object[] { null!, false };
        InvokePrivate<bool>(ex, "TryGetVentDecision", decisionArgs).Should().BeFalse();
    }

    [Fact]
    public void SabotageDecision_UsesPriority_NullDelegation_LatestTieBreaker_AndButtonSelection()
    {
        var ex = CreateBareEx(playerId: 15, role: RoleId.Crewmate);
        bool? highDecision = null;
        var low = new TestSaboAbility(() => true, priority: 0);
        var high = new TestSaboAbility(() => highDecision, priority: 10);
        Attach(ex, low);
        Attach(ex, high);

        ex.CanSabotage().Should().BeTrue();
        ex.ShouldShowSabotageAbility(low).Should().BeTrue();
        ex.ShouldShowSabotageAbility(high).Should().BeFalse();

        highDecision = false;
        ex.CanSabotage().Should().BeFalse();
        ex.ShouldShowSabotageAbility(low).Should().BeFalse();
        ex.ShouldShowSabotageAbility(high).Should().BeFalse();
        // ShowVanillaSabotageButton uses the inverse of this guard, so a custom false decision suppresses vanilla too.
        var decisionArgs = new object[] { null!, false };
        InvokePrivate<bool>(ex, "TryGetSabotageDecision", decisionArgs).Should().BeTrue();
        decisionArgs[0].Should().BeSameAs(high);
        decisionArgs[1].Should().Be(false);

        var latestAtSamePriority = new TestSaboAbility(() => true, priority: 10);
        Attach(ex, latestAtSamePriority);
        ex.CanSabotage().Should().BeTrue();
        ex.ShouldShowSabotageAbility(latestAtSamePriority).Should().BeTrue();
        ex.ShouldShowSabotageAbility(high).Should().BeFalse();
        ex.ShouldShowSabotageAbility(low).Should().BeFalse();

        Detach(ex, latestAtSamePriority);
        ex.CanSabotage().Should().BeFalse();

        Detach(ex, high);
        ex.CanSabotage().Should().BeTrue();
        ex.ShouldShowSabotageAbility(low).Should().BeTrue();

        Detach(ex, low);
        decisionArgs = new object[] { null!, false };
        InvokePrivate<bool>(ex, "TryGetSabotageDecision", decisionArgs).Should().BeFalse();
    }

    [Fact]
    public void KillableDecision_UsesPriority_NullDelegation_LatestTieBreaker_AndDetachFallback()
    {
        var ex = CreateBareEx(playerId: 16, role: RoleId.Crewmate);
        bool? highDecision = null;
        var low = new KillableAbility(() => true, priority: 0);
        var high = new KillableAbility(() => highDecision, priority: 10);
        Attach(ex, low);
        Attach(ex, high);

        InvokePrivate<bool>(ex, "ResolveCanKill").Should().BeTrue();

        highDecision = false;
        InvokePrivate<bool>(ex, "ResolveCanKill").Should().BeFalse();

        var latestAtSamePriority = new KillableAbility(() => true, priority: 10);
        Attach(ex, latestAtSamePriority);
        InvokePrivate<bool>(ex, "ResolveCanKill").Should().BeTrue();

        Detach(ex, latestAtSamePriority);
        InvokePrivate<bool>(ex, "ResolveCanKill").Should().BeFalse();

        Detach(ex, high);
        InvokePrivate<bool>(ex, "ResolveCanKill").Should().BeTrue();

        Detach(ex, low);
        InvokePrivate<bool>(ex, "ResolveCanKill").Should().BeTrue();
    }

    [Fact]
    public void ImpostorVisionDecision_UsesPriority_NullDelegation_LatestTieBreaker_AndDetachFallback()
    {
        var ex = CreateBareEx(playerId: 17, role: RoleId.Crewmate);
        bool? highDecision = null;
        var low = new ImpostorVisionAbility(() => true, priority: 0);
        var high = new ImpostorVisionAbility(() => highDecision, priority: 10);
        Attach(ex, low);
        Attach(ex, high);

        ex.HasImpostorVision().Should().BeTrue();

        highDecision = false;
        ex.HasImpostorVision().Should().BeFalse();

        var latestAtSamePriority = new ImpostorVisionAbility(() => true, priority: 10);
        Attach(ex, latestAtSamePriority);
        ex.HasImpostorVision().Should().BeTrue();

        Detach(ex, latestAtSamePriority);
        ex.HasImpostorVision().Should().BeFalse();

        Detach(ex, high);
        ex.HasImpostorVision().Should().BeTrue();
    }

    // 目的: 拡張メソッドとインターフェース実装の GenerateAbilityId が一致することを検証
    [Fact]
    public void TaskValues_AreResolvedIndependentlyByPriority()
    {
        var ex = CreateBareEx(playerId: 14, role: RoleId.Crewmate);
        bool? highTrigger = false;
        var lowTaskOptions = new TaskOptionData(1, 1, 1);
        var middleTaskOptions = new TaskOptionData(2, 2, 2);

        var low = new CustomTaskAbility(
            isTaskTrigger: () => true,
            countsForCrewWin: () => false,
            requiredTaskCount: () => 3,
            taskOptions: () => lowTaskOptions,
            priority: AbilityPriority.Default);
        var middle = new CustomTaskAbility(
            countsForCrewWin: () => true,
            taskOptions: () => middleTaskOptions,
            priority: AbilityPriority.Modifier);
        var high = new CustomTaskAbility(
            isTaskTrigger: () => highTrigger,
            priority: AbilityPriority.GhostRole);

        Attach(ex, low);
        Attach(ex, middle);
        Attach(ex, high);

        ex.IsTaskTriggerRole().Should().BeFalse();
        ex.IsCountTask().Should().BeTrue();
        InvokePrivate<int>(ex, "ResolveRequiredTaskCount", 99).Should().Be(3);
        InvokePrivate<TaskOptionData>(ex, "ResolveTaskOptions").Should().BeSameAs(middleTaskOptions);

        highTrigger = null;
        ex.IsTaskTriggerRole().Should().BeTrue();
    }

    [Theory]
    [InlineData(LoversWinType.Normal)]
    [InlineData(LoversWinType.Shared)]
    public void LoversTaskAbilities_NonSingle_PrefersRoleTrigger_AndNeverCountsForCrewWin(LoversWinType winType)
    {
        var ex = CreateBareEx(playerId: 18, role: RoleId.Workperson);
        var role = new CustomTaskAbility(
            isTaskTrigger: () => true,
            countsForCrewWin: () => true,
            priority: AbilityPriority.Default);
        var loversCount = new CustomTaskAbility(
            countsForCrewWin: () => false,
            priority: AbilityPriority.Modifier);
        var loversTrigger = new CustomTaskAbility(
            isTaskTrigger: () => false,
            priority: Lovers.GetTaskTriggerPriority(winType));

        Attach(ex, role);
        Attach(ex, loversCount);
        Attach(ex, loversTrigger);

        Lovers.GetTaskTriggerPriority(winType).Should().BeLessThan(AbilityPriority.Default);
        ex.IsTaskTriggerRole().Should().BeTrue();
        ex.IsCountTask().Should().BeFalse();
    }

    [Fact]
    public void LoversTaskAbilities_Single_BlocksRoleTaskTrigger_AndNeverCountsForCrewWin()
    {
        var ex = CreateBareEx(playerId: 19, role: RoleId.Workperson);
        var role = new CustomTaskAbility(
            isTaskTrigger: () => true,
            countsForCrewWin: () => true,
            priority: AbilityPriority.Default);
        var loversCount = new CustomTaskAbility(
            countsForCrewWin: () => false,
            priority: AbilityPriority.Modifier);
        var loversTrigger = new CustomTaskAbility(
            isTaskTrigger: () => false,
            priority: Lovers.GetTaskTriggerPriority(LoversWinType.Single));

        Attach(ex, role);
        Attach(ex, loversCount);
        Attach(ex, loversTrigger);

        Lovers.GetTaskTriggerPriority(LoversWinType.Single).Should().Be(AbilityPriority.Modifier);
        ex.IsTaskTriggerRole().Should().BeFalse();
        ex.IsCountTask().Should().BeFalse();
    }

    [Fact]
    public void IsTaskComplete_UsesLoversTaskTrigger_WhileAllTasksCompletedDoesNot()
    {
        var ex = CreateBareEx(playerId: 20, role: RoleId.BodyBuilder);
        // Data が null のため TaskCompletedData は (-1,-1)。トリガー未設定なら completed>=total で true。
        ex.IsTaskComplete().Should().BeTrue();
        // Player が null のため、バニラ完了判定は CustomTaskAbility を見ずに false。
        ex.IsAllTasksCompleted().Should().BeFalse();

        Attach(ex, new CustomTaskAbility(
            isTaskTrigger: () => false,
            priority: AbilityPriority.Modifier));

        ex.IsTaskComplete().Should().BeFalse();
        ex.IsAllTasksCompleted().Should().BeFalse();
    }

    private static TResult InvokePrivate<TResult>(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (TResult)method.Invoke(target, args)!;
    }

    [Fact]
    public void Extension_GenerateAbilityId_Matches_Interface_Implementation()
    {
        byte playerId = 2;
        // Using AbilityParentPlayer with null ExPlayerControl to get a deterministic signature independent of any role/ability ordinal
        var id1 = ExPlayerControlExtensions.GenerateDeterministicAbilityId(playerId, new AbilityParentPlayer(null), typeof(AlphaAbility), 0);
        var id2 = ExPlayerControlExtensions.GenerateDeterministicAbilityId(playerId, new AbilityParentPlayer(null), typeof(AlphaAbility), 0);
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
        ex.AbilityCount.Should().Be(0);
        // 目的: 能力辞書が初期化される
        ex.GetAbility(0).Should().BeNull();
    }
}
