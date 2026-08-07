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
        act.Should().Throw<InvalidOperationException>().WithMessage("detach failed");
        NoArgEvent.Instance.Awake();

        called.Should().Be(0);
    }

    // 目的: 1つのリスナーが例外を投げても他のリスナー実行は継続されることを検証
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

