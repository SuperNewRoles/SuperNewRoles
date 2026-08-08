using System;
using System.Reflection;
using BepInEx.Logging;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using Xunit;

namespace SuperNewRoles.Tests;

// イベントリスナーの登録/削除タイミング、Awake 中の遅延削除、
// およびデータ付きイベントの配送を検証するテスト。
public class EventListenerTests
{
    // Simple no-arg event for testing EventTargetBase<T>
    private class NoArgEvent : EventTargetBase<NoArgEvent> { }

    // Simple data type and event for testing EventTargetBase<T, U>
    private class DummyData : IEventData
    {
        public int Value { get; set; }
    }
    private class DataEvent : EventTargetBase<DataEvent, DummyData> { }
    private class ListenerOwningAbility : AbilityBase
    {
        private readonly Action _action;
        private readonly bool _throwOnDetachToAlls;

        public ListenerOwningAbility(Action action, bool throwOnDetachToAlls = false)
        {
            _action = action;
            _throwOnDetachToAlls = throwOnDetachToAlls;
        }

        protected override bool IsLocalPlayer(PlayerControl player) => true;

        public override void AttachToAlls() => SubscribeWithAbility(NoArgEvent.Instance, _action);

        public override void DetachToAlls()
        {
            if (_throwOnDetachToAlls)
                throw new InvalidOperationException("detach failed");
        }
    }

    private class FailingLifecycleAbility : AbilityBase
    {
        private readonly Action _action;

        public bool AttachToAllsCalled { get; private set; }
        public bool OnAttachedCalled { get; private set; }
        public int DetachToAllsCallCount { get; private set; }

        public FailingLifecycleAbility(Action action)
        {
            _action = action;
        }

        protected override bool IsLocalPlayer(PlayerControl player) => true;

        public override void AttachToLocalPlayer()
        {
            SubscribeWithAbility(NoArgEvent.Instance, _action);
            throw new InvalidOperationException("attach local failed");
        }

        public override void AttachToAlls() => AttachToAllsCalled = true;

        protected override void OnAttached(PlayerControl player) => OnAttachedCalled = true;

        public override void DetachToLocalPlayer() => throw new InvalidOperationException("detach local failed");

        public override void DetachToAlls() => DetachToAllsCallCount++;
    }

    private sealed class StubEventListener : IEventListener
    {
        private readonly bool _throwOnRemove;

        public bool Removed { get; private set; }

        public StubEventListener(bool throwOnRemove)
        {
            _throwOnRemove = throwOnRemove;
        }

        public void RemoveListener()
        {
            Removed = true;
            if (_throwOnRemove)
                throw new InvalidOperationException("listener removal failed");
        }
    }

    private class ListenerCleanupAbility : AbilityBase
    {
        public StubEventListener NormalListener { get; } = new(false);
        public StubEventListener ThrowingListener { get; } = new(true);

        protected override bool IsLocalPlayer(PlayerControl player) => true;

        public override void AttachToAlls()
        {
            TrackEventListener(NormalListener);
            TrackEventListener(ThrowingListener);
        }
    }

    private class AttachmentTargetAbility : AbilityBase
    {
        public bool IsLocal { get; set; }
        public int DetachToLocalPlayerCallCount { get; private set; }
        public int DetachToOthersCallCount { get; private set; }

        protected override bool IsLocalPlayer(PlayerControl player) => IsLocal;

        public override void DetachToLocalPlayer() => DetachToLocalPlayerCallCount++;

        public override void DetachToOthers() => DetachToOthersCallCount++;
    }

    private static void ResetEvents()
    {
        NoArgEvent.Instance.RemoveListenerAll();
        DataEvent.Instance.RemoveListenerAll();
    }

    // Ensure plugin logger exists so verbose code paths are safe if touched
    private static void EnsurePluginLogger()
    {
        var t = typeof(SuperNewRoles.SuperNewRolesPlugin);
        var f = t.GetField("<Logger>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        if (f != null && f.GetValue(null) == null)
        {
            f.SetValue(null, new ManualLogSource("Tests"));
        }
    }

    // 目的: Awake 実行時に事前登録したアクションが呼ばれることを検証
    [Fact]
    public void AddListener_Then_Awake_Invokes_Action()
    {
        EnsurePluginLogger();
        ResetEvents();
        int called = 0;
        NoArgEvent.Instance.AddListener(() => called++);
        NoArgEvent.Instance.Awake();
        // 目的: Awake により1回だけ呼び出される
        called.Should().Be(1);
    }

    // 目的: Awake 実行前に削除したリスナーは呼ばれないことを検証
    [Fact]
    public void RemoveListener_Immediately_Prevents_Future_Invocation()
    {
        EnsurePluginLogger();
        ResetEvents();
        int called = 0;
        var listener = NoArgEvent.Instance.AddListener(() => called++);
        // Remove outside of Awake, should take effect immediately
        listener.RemoveListener();
        NoArgEvent.Instance.Awake();
        // 目的: 事前に削除したため呼ばれない
        called.Should().Be(0);
    }

    // 目的: Awake 実行中に RemoveListener されたリスナーは次回以降に削除されることを検証
    [Fact]
    public void RemoveListener_During_Awake_Is_Deferred()
    {
        EnsurePluginLogger();
        ResetEvents();
        int a = 0, b = 0;
        EventListener? l1 = null;
        l1 = NoArgEvent.Instance.AddListener(() => { a++; l1!.RemoveListener(); });
        NoArgEvent.Instance.AddListener(() => b++);

        // First awake: both listeners run; l1 schedules its own removal
        NoArgEvent.Instance.Awake();
        // 目的: 最初の Awake では両方とも1回実行される
        a.Should().Be(1);
        b.Should().Be(1);

        // Second awake: l1 should have been removed, only b runs
        NoArgEvent.Instance.Awake();
        // 目的: 次回 Awake では削除が反映され a は増えず b のみ増える
        a.Should().Be(1);
        b.Should().Be(2);
    }

    // 目的: データ付きイベントがリスナーへ正しくデータを渡すことを検証
    [Fact]
    public void GenericEvent_Passes_Data_To_Action()
    {
        EnsurePluginLogger();
        ResetEvents();
        int observed = -1;
        DataEvent.Instance.AddListener(d => observed = d.Value);
        DataEvent.Instance.Awake(new DummyData { Value = 42 });
        // 目的: 渡したデータがそのままリスナーに届く
        observed.Should().Be(42);
    }

    [Fact]
    public void Detach_RemovesAbilityOwnedListeners()
    {
        EnsurePluginLogger();
        ResetEvents();
        var called = 0;
        var ability = new ListenerOwningAbility(() => called++);
        ability.Attach(null, 1, new AbilityParentPlayer(null));

        NoArgEvent.Instance.Awake();
        ability.Detach();
        NoArgEvent.Instance.Awake();

        called.Should().Be(1);
    }

    [Fact]
    public void Detach_WhenLifecycleHookThrows_StillRemovesAbilityOwnedListeners()
    {
        EnsurePluginLogger();
        ResetEvents();
        var called = 0;
        var ability = new ListenerOwningAbility(() => called++, throwOnDetachToAlls: true);
        ability.Attach(null, 1, new AbilityParentPlayer(null));

        Action act = ability.Detach;
        act.Should().NotThrow();
        NoArgEvent.Instance.Awake();

        called.Should().Be(0);
    }

    [Fact]
    public void LifecycleHookFailure_DoesNotStopRemainingHooksOrCleanup()
    {
        EnsurePluginLogger();
        ResetEvents();
        var called = 0;
        var ability = new FailingLifecycleAbility(() => called++);

        Action attach = () => ability.Attach(null, 1, new AbilityParentPlayer(null));
        attach.Should().NotThrow();
        ability.AttachToAllsCalled.Should().BeTrue();
        ability.OnAttachedCalled.Should().BeTrue();

        NoArgEvent.Instance.Awake();
        called.Should().Be(1);

        Action detach = ability.Detach;
        detach.Should().NotThrow();
        ability.DetachToAllsCallCount.Should().Be(1);

        ability.Detach();
        ability.DetachToAllsCallCount.Should().Be(1);
        NoArgEvent.Instance.Awake();
        called.Should().Be(1);
    }

    [Fact]
    public void Attach_WhenCalledTwice_DoesNotDuplicateSubscriptions()
    {
        EnsurePluginLogger();
        ResetEvents();
        var called = 0;
        var ability = new ListenerOwningAbility(() => called++);
        var parent = new AbilityParentPlayer(null);

        ability.Attach(null, 1, parent);
        ability.Attach(null, 1, parent);
        NoArgEvent.Instance.Awake();

        called.Should().Be(1);

        ability.Detach();
        ability.Attach(null, 1, parent);
        NoArgEvent.Instance.Awake();

        called.Should().Be(2);
        ability.Detach();
    }

    [Fact]
    public void Detach_WhenListenerRemovalThrows_StillRemovesRemainingListeners()
    {
        EnsurePluginLogger();
        var ability = new ListenerCleanupAbility();
        ability.Attach(null, 1, new AbilityParentPlayer(null));

        Action detach = ability.Detach;
        detach.Should().NotThrow();
        ability.ThrowingListener.Removed.Should().BeTrue();
        ability.NormalListener.Removed.Should().BeTrue();
    }

    [Fact]
    public void Detach_UsesTheAttachmentTargetResolvedDuringAttach()
    {
        EnsurePluginLogger();
        var ability = new AttachmentTargetAbility { IsLocal = true };
        ability.Attach(null, 1, new AbilityParentPlayer(null));

        ability.IsLocal = false;
        ability.Detach();

        ability.DetachToLocalPlayerCallCount.Should().Be(1);
        ability.DetachToOthersCallCount.Should().Be(0);
    }

    // 目的: 1つのリスナーが例外を投げても他のリスナー実行は継続されることを検証
    [Fact]
    public void NoArgEvent_RemoveListenerDuringNestedAwake_SkipsItInOuterAwake()
    {
        EnsurePluginLogger();
        ResetEvents();
        var nested = false;
        var called = 0;
        EventListener? secondListener = null;

        NoArgEvent.Instance.AddListener(() =>
        {
            if (!nested)
            {
                nested = true;
                NoArgEvent.Instance.Awake();
                nested = false;
                return;
            }

            secondListener!.RemoveListener();
        });
        secondListener = NoArgEvent.Instance.AddListener(() => called++);

        NoArgEvent.Instance.Awake();

        called.Should().Be(0);
    }

    [Fact]
    public void GenericEvent_RemoveListenerDuringNestedAwake_SkipsItInOuterAwake()
    {
        EnsurePluginLogger();
        ResetEvents();
        var nested = false;
        var called = 0;
        EventListener<DummyData>? secondListener = null;

        DataEvent.Instance.AddListener(data =>
        {
            if (!nested)
            {
                nested = true;
                DataEvent.Instance.Awake(data);
                nested = false;
                return;
            }

            secondListener!.RemoveListener();
        });
        secondListener = DataEvent.Instance.AddListener(_ => called++);

        DataEvent.Instance.Awake(new DummyData { Value = 1 });

        called.Should().Be(0);
    }

    [Fact]
    public void NoArgEvent_RemoveListenerAllDuringAwake_SkipsRemainingListeners()
    {
        EnsurePluginLogger();
        ResetEvents();
        var called = 0;
        NoArgEvent.Instance.AddListener(NoArgEvent.Instance.RemoveListenerAll);
        NoArgEvent.Instance.AddListener(() => called++);

        NoArgEvent.Instance.Awake();
        NoArgEvent.Instance.Awake();

        called.Should().Be(0);
    }

    [Fact]
    public void GenericEvent_RemoveListenerAllDuringAwake_SkipsRemainingListeners()
    {
        EnsurePluginLogger();
        ResetEvents();
        var called = 0;
        DataEvent.Instance.AddListener(_ => DataEvent.Instance.RemoveListenerAll());
        DataEvent.Instance.AddListener(_ => called++);

        DataEvent.Instance.Awake(new DummyData { Value = 1 });
        DataEvent.Instance.Awake(new DummyData { Value = 2 });

        called.Should().Be(0);
    }

    [Fact]
    public void NoArgEvent_Exception_In_Listener_Does_Not_Stop_Others()
    {
        EnsurePluginLogger();
        ResetEvents();

        int called = 0;
        NoArgEvent.Instance.AddListener(() => throw new InvalidOperationException("boom"));
        NoArgEvent.Instance.AddListener(() => called++);

        Action act = () => NoArgEvent.Instance.Awake();
        act.Should().NotThrow();
        called.Should().Be(1);
    }

    // 目的: データ付きイベントでも例外発生時に他リスナー実行が継続されることを検証
    [Fact]
    public void GenericEvent_Exception_In_Listener_Does_Not_Stop_Others()
    {
        EnsurePluginLogger();
        ResetEvents();

        int called = 0;
        DataEvent.Instance.AddListener(_ => throw new InvalidOperationException("boom"));
        DataEvent.Instance.AddListener(_ => called++);

        Action act = () => DataEvent.Instance.Awake(new DummyData { Value = 1 });
        act.Should().NotThrow();
        called.Should().Be(1);
    }
}