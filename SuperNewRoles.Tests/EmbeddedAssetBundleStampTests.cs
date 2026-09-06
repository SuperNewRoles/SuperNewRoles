using System.IO;
using System.Reflection;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class EmbeddedAssetBundleStampTests
{
    [Fact]
    public void SameBinaryLoadedFromMemoryKeepsCacheIdentity()
    {
        var original = typeof(EmbeddedAssetBundleStampTests).Assembly;
        var relocated = Assembly.Load(File.ReadAllBytes(original.Location));

        Assert.NotEqual(original.Location, relocated.Location);
        Assert.Equal(EmbeddedAssetBundleStamp.Create(original, "sprites_android.bundle"),
            EmbeddedAssetBundleStamp.Create(relocated, "sprites_android.bundle"));
    }

    [Fact]
    public void DifferentBuildOrResourceInvalidatesCache()
    {
        var assembly = typeof(EmbeddedAssetBundleStampTests).Assembly;
        string stamp = EmbeddedAssetBundleStamp.Create(assembly, "sprites_android.bundle");

        Assert.NotEqual(stamp, EmbeddedAssetBundleStamp.Create(typeof(object).Assembly, "sprites_android.bundle"));
        Assert.NotEqual(stamp, EmbeddedAssetBundleStamp.Create(assembly, "other_android.bundle"));
    }
}
