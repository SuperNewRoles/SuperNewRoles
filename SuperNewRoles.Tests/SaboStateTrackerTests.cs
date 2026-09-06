using System;
using System.Collections.Generic;
using SuperNewRoles.Events;
using Xunit;

namespace SuperNewRoles.Tests;

[Collection("PerformanceAllocation")]
public sealed class SaboStateTrackerTests : IDisposable
{
    public SaboStateTrackerTests() => Reset();
    public void Dispose() => Reset();

    private static void Reset()
    {
        SaboStateTracker.activeSaboTypes.Clear();
        SaboStartEvent.Instance.RemoveListenerAll();
        SaboEndEvent.Instance.RemoveListenerAll();
    }

    [Fact]
    public void StartsAndEndsAreEmittedOnceAndStateIsCommittedBeforeNotification()
    {
        var events = new List<string>();
        SaboStartEvent.Instance.AddListener(data =>
        {
            Assert.Contains(data.saboType, SaboStateTracker.activeSaboTypes);
            events.Add("start:" + data.saboType);
        });
        SaboEndEvent.Instance.AddListener(data =>
        {
            Assert.DoesNotContain(data.saboType, SaboStateTracker.activeSaboTypes);
            events.Add("end:" + data.saboType);
        });

        var active = new[] { SystemTypes.Electrical, SystemTypes.Comms };
        SaboStateTracker.ApplyActiveSabotages(active);
        SaboStateTracker.ApplyActiveSabotages(active);
        SaboStateTracker.ApplyActiveSabotages(new[] { SystemTypes.Comms });
        SaboStateTracker.ApplyActiveSabotages(ReadOnlySpan<SystemTypes>.Empty);
        SaboStateTracker.ApplyActiveSabotages(ReadOnlySpan<SystemTypes>.Empty);

        Assert.Equal(new[] { "start:Electrical", "start:Comms", "end:Electrical", "end:Comms" }, events);
        Assert.Empty(SaboStateTracker.activeSaboTypes);
    }

    [Fact]
    public void ReentrantNotificationsKeepIndependentSnapshots()
    {
        var active = new[] { SystemTypes.Electrical, SystemTypes.Comms };
        int starts = 0;
        int ends = 0;
        SaboStartEvent.Instance.AddListener(data =>
        {
            starts++;
            if (data.saboType == SystemTypes.Electrical)
                SaboStateTracker.ApplyActiveSabotages(active);
        });
        SaboStateTracker.ApplyActiveSabotages(active);
        Assert.Equal(2, starts);

        SaboEndEvent.Instance.AddListener(data =>
        {
            ends++;
            if (data.saboType == SystemTypes.Electrical)
                SaboStateTracker.ApplyActiveSabotages(new[] { SystemTypes.Comms });
        });
        SaboStateTracker.ApplyActiveSabotages(ReadOnlySpan<SystemTypes>.Empty);
        Assert.Equal(2, ends);
        Assert.Empty(SaboStateTracker.activeSaboTypes);
    }

    [Fact]
    public void UnchangedSabotagesDoNotAllocate()
    {
        var active = new[] { SystemTypes.Electrical, SystemTypes.Comms };
        SaboStateTracker.ApplyActiveSabotages(active);
        for (int i = 0; i < 10000; i++)
            SaboStateTracker.ApplyActiveSabotages(active);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; i++)
            SaboStateTracker.ApplyActiveSabotages(active);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0L, allocated);
    }
}
