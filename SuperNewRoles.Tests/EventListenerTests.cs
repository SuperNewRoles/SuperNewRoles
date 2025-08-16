using System;
using System.Reflection;
using BepInEx.Logging;
using FluentAssertions;
using SuperNewRoles.Modules.Events.Bases;
using Xunit;

namespace SuperNewRoles.Tests;

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

    [Fact]
    public void AddListener_Then_Awake_Invokes_Action()
    {
        EnsurePluginLogger();
        int called = 0;
        NoArgEvent.Instance.AddListener(() => called++);
        NoArgEvent.Instance.Awake();
        called.Should().Be(1);
    }

    [Fact]
    public void RemoveListener_Immediately_Prevents_Future_Invocation()
    {
        EnsurePluginLogger();
        int called = 0;
        var listener = NoArgEvent.Instance.AddListener(() => called++);
        // Remove outside of Awake, should take effect immediately
        listener.RemoveListener();
        NoArgEvent.Instance.Awake();
        called.Should().Be(0);
    }

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
        a.Should().Be(1);
        b.Should().Be(1);

        // Second awake: l1 should have been removed, only b runs
        NoArgEvent.Instance.Awake();
        a.Should().Be(1);
        b.Should().Be(2);
    }

    [Fact]
    public void GenericEvent_Passes_Data_To_Action()
    {
        EnsurePluginLogger();
        int observed = -1;
        DataEvent.Instance.AddListener(d => observed = d.Value);
        DataEvent.Instance.Awake(new DummyData { Value = 42 });
        observed.Should().Be(42);
    }
}

