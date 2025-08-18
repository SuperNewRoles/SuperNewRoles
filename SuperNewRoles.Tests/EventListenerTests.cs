using System;
using System.Reflection;
using BepInEx.Logging;
using FluentAssertions;
using SuperNewRoles.Modules.Events.Bases;
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
        int observed = -1;
        DataEvent.Instance.AddListener(d => observed = d.Value);
        DataEvent.Instance.Awake(new DummyData { Value = 42 });
        // 目的: 渡したデータがそのままリスナーに届く
        observed.Should().Be(42);
    }
}

