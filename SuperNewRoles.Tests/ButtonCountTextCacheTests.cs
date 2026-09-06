using System;
using System.Globalization;
using SuperNewRoles.Roles.Ability.CustomButton;
using Xunit;

namespace SuperNewRoles.Tests;

[CollectionDefinition("PerformanceAllocation", DisableParallelization = true)]
public class PerformanceAllocationCollection { }

[Collection("PerformanceAllocation")]
public class ButtonCountTextCacheTests
{
    [Fact]
    public void CountAndTranslationChangesAreReflectedImmediately()
    {
        var cache = new ButtonCountTextCache();
        Assert.Equal("残り 3 回", cache.GetText("残り {0} 回", 3));
        Assert.Equal("残り 2 回", cache.GetText("残り {0} 回", 2));
        Assert.Equal("2 remaining", cache.GetText("{0} remaining", 2));
        Assert.Equal("残り 0 回", cache.GetText("残り {0} 回", 0));
    }

    [Fact]
    public void CultureChangeInvalidatesFormattedText()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            var cache = new ButtonCountTextCache();
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal("1,234", cache.GetText("{0:N0}", 1234));
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            Assert.Equal("1.234", cache.GetText("{0:N0}", 1234));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void UnchangedHudUpdatesDoNotAllocate()
    {
        var cache = new ButtonCountTextCache();
        string text = cache.GetText("残り {0} 回", 3);
        for (int i = 0; i < 10000; i++)
            cache.GetText("残り {0} 回", 3);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; i++)
            cache.GetText("残り {0} 回", 3);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0L, allocated);
        Assert.Same(text, cache.GetText("残り {0} 回", 3));
    }
}
