using System.IO;
using System.Linq;
using FluentAssertions;
using SuperNewRoles.CustomCosmetics;
using UnityEngine;
using Xunit;

namespace SuperNewRoles.Tests;

public class CustomCosmeticsLoaderPrefetchTests
{
    [Fact]
    public void ShouldStartHttpPrefetch_Skips_WhenCosmeticsDisabled()
    {
        CustomCosmeticsLoader.ShouldStartHttpPrefetch(
                cosmeticsNotLoaded: true,
                NetworkReachability.ReachableViaLocalAreaNetwork,
                canUseDataConnection: true)
            .Should().BeFalse();
    }

    [Fact]
    public void ShouldStartHttpPrefetch_Skips_WhenCarrierDataDisallowed()
    {
        CustomCosmeticsLoader.ShouldStartHttpPrefetch(
                cosmeticsNotLoaded: false,
                NetworkReachability.ReachableViaCarrierDataNetwork,
                canUseDataConnection: false)
            .Should().BeFalse();
    }

    [Fact]
    public void ShouldStartHttpPrefetch_Allows_LanAndNotReachable()
    {
        CustomCosmeticsLoader.ShouldStartHttpPrefetch(
                cosmeticsNotLoaded: false,
                NetworkReachability.ReachableViaLocalAreaNetwork,
                canUseDataConnection: false)
            .Should().BeTrue();
        CustomCosmeticsLoader.ShouldStartHttpPrefetch(
                cosmeticsNotLoaded: false,
                NetworkReachability.NotReachable,
                canUseDataConnection: false)
            .Should().BeTrue();
    }

    [Fact]
    public void CollectDownloadJobs_ExtractsBundleAndSpriteUrls()
    {
        const string json = """
            {
              "assetbundles": [
                { "url": "https://cdn.example/hats.bundle", "hash": "abc123" }
              ],
              "hats": [
                { "name": "TestHat", "package": "Pack", "resource": "hat.png", "author": "A" }
              ]
            }
            """;

        var jobs = CustomCosmeticsLoader.CollectDownloadJobs(
            "https://cdn.example/cosmetics.json",
            json,
            isAndroid: false);

        jobs.Should().Contain(job => job.IsAssetBundle && job.Url == "https://cdn.example/hats.bundle" && job.ExpectedHash == "abc123");
        jobs.Should().Contain(job => !job.IsAssetBundle && job.Url == "https://cdn.example/hats/hat.png");
        jobs.Should().Contain(job => job.IsAssetBundle && job.TargetPath.Replace("\\", "/").EndsWith("hats.bundle/abc123.bundle"));
        jobs.Should().Contain(job => !job.IsAssetBundle && job.TargetPath.Replace("\\", "/").EndsWith("Pack/TestHat_front.png"));
        jobs.Should().OnlyContain(job => !string.IsNullOrEmpty(job.TargetPath));
    }

    [Fact]
    public void CollectDownloadJobs_UsesAndroidBundleUrl()
    {
        const string json = """
            {
              "assetbundles": [
                {
                  "url": "https://cdn.example/pc.bundle",
                  "hash": "pc",
                  "url_android": "https://cdn.example/android.bundle",
                  "hash_android": "and"
                }
              ]
            }
            """;

        var androidJobs = CustomCosmeticsLoader.CollectDownloadJobs("https://cdn.example/cosmetics.json", json, isAndroid: true);
        androidJobs.Should().ContainSingle();
        androidJobs[0].Url.Should().Be("https://cdn.example/android.bundle");
        androidJobs[0].ExpectedHash.Should().Be("and");
        androidJobs[0].TargetPath.Replace("\\", "/").Should().EndWith("android.bundle/and.bundle");

        var pcJobs = CustomCosmeticsLoader.CollectDownloadJobs("https://cdn.example/cosmetics.json", json, isAndroid: false);
        pcJobs.Should().ContainSingle();
        pcJobs[0].Url.Should().Be("https://cdn.example/pc.bundle");
        pcJobs[0].ExpectedHash.Should().Be("pc");
    }

    [Fact]
    public void CollectDownloadJobs_UsesVisorsFolder_WhenCapitalizedKey()
    {
        const string json = """
            {
              "Visors": [
                { "name": "TestVisor", "package": "Pack", "resource": "idle.png", "flipresource": "flip.png", "climbresource": "climb.png", "author": "A" }
              ]
            }
            """;

        var jobs = CustomCosmeticsLoader.CollectDownloadJobs("https://cdn.example/cosmetics.json", json, isAndroid: false);
        jobs.Select(job => job.Url).Should().BeEquivalentTo(
            "https://cdn.example/Visors/idle.png",
            "https://cdn.example/Visors/flip.png",
            "https://cdn.example/Visors/climb.png");
        jobs.Should().Contain(job => job.TargetPath.Replace("\\", "/").EndsWith("Pack/TestVisor_idle.png"));
    }

    [Fact]
    public void CollectDownloadJobs_UsesLowercaseVisorsFolder()
    {
        const string json = """
            {
              "visors": [
                { "name": "TestVisor", "package": "Pack", "resource": "idle.png", "author": "A" }
              ]
            }
            """;

        var jobs = CustomCosmeticsLoader.CollectDownloadJobs("https://cdn.example/cosmetics.json", json, isAndroid: false);
        jobs.Should().ContainSingle();
        jobs[0].Url.Should().Be("https://cdn.example/visors/idle.png");
    }

    [Fact]
    public void CollectDownloadJobs_CollectsHatResourceSuffixes()
    {
        const string json = """
            {
              "hats": [
                {
                  "name": "TestHat",
                  "package": "Pack",
                  "author": "A",
                  "resource": "front.png",
                  "resourceleft": "front_left.png",
                  "backresource": "back.png",
                  "backresourceleft": "back_left.png",
                  "backflipresource": "backflip.png",
                  "flipresource": "flip.png",
                  "climbresource": "climb.png"
                }
              ],
              "nameplates": [
                { "name": "Plate", "package": "Pack", "resource": "plate.png", "author": "A" }
              ]
            }
            """;

        var jobs = CustomCosmeticsLoader.CollectDownloadJobs("https://cdn.example/cosmetics.json", json, isAndroid: false);
        jobs.Where(job => !job.IsAssetBundle).Select(job => job.Url).Should().BeEquivalentTo(
            "https://cdn.example/hats/front.png",
            "https://cdn.example/hats/front_left.png",
            "https://cdn.example/hats/back.png",
            "https://cdn.example/hats/back_left.png",
            "https://cdn.example/hats/backflip.png",
            "https://cdn.example/hats/flip.png",
            "https://cdn.example/hats/climb.png",
            "https://cdn.example/nameplates/plate.png");
        jobs.Select(job => System.IO.Path.GetFileName(job.TargetPath)).Should().BeEquivalentTo(
            "TestHat_front.png",
            "TestHat_front_left.png",
            "TestHat_back.png",
            "TestHat_back_left.png",
            "TestHat_backflip.png",
            "TestHat_flip.png",
            "TestHat_climb.png",
            "Plate_nameplate.png");
    }

    [Fact]
    public void CollectDownloadJobs_ConstrainsUntrustedPathComponentsToCache()
    {
        const string json = """
            {
              "assetbundles": [
                { "url": "https://cdn.example/bundle.bundle", "hash": "../../outside" }
              ],
              "hats": [
                { "name": "../../Hat", "package": "../../Pack", "resource": "hat.png", "author": "A" }
              ]
            }
            """;

        var jobs = CustomCosmeticsLoader.CollectDownloadJobs(
            "https://cdn.example/cosmetics.json",
            json,
            isAndroid: false);

        jobs.Should().NotBeEmpty();
        jobs.Should().OnlyContain(job => CustomCosmeticsLoader.IsCachePath(job.TargetPath));
    }

    [Fact]
    public void IsLocalCosmeticsPath_DetectsRootedRelativeAndExistingFiles()
    {
        CustomCosmeticsLoader.IsLocalCosmeticsPath("./debug_assets.json").Should().BeTrue();
        CustomCosmeticsLoader.IsLocalCosmeticsPath("../debug_assets.json").Should().BeTrue();
        CustomCosmeticsLoader.IsLocalCosmeticsPath(Path.GetTempPath()).Should().BeTrue();
        CustomCosmeticsLoader.IsLocalCosmeticsPath("https://cdn.example/cosmetics.json").Should().BeFalse();
        CustomCosmeticsLoader.IsLocalCosmeticsPath("").Should().BeFalse();

        string tempFile = Path.GetTempFileName();
        try
        {
            CustomCosmeticsLoader.IsLocalCosmeticsPath(tempFile).Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void TryStartHttpPrefetch_DoesNothing_WhenConfigNotInitialized()
    {
        CustomCosmeticsLoader.TryStartHttpPrefetch();
        CustomCosmeticsLoader.HttpPrefetchTask.Should().BeNull();
    }
}
