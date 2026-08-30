using FluentAssertions;
using SuperNewRoles.Safety;
using SuperNewRoles.Safety.Api;
using Xunit;

namespace SuperNewRoles.Tests;

public class ConductPlayDecisionTests
{
    [Fact]
    public void AllowsWhenIdentityIsOff()
    {
        ConductPlayDecision.Decide(false, false, false, false)
            .Should().Be(ConductPlayDecision.Result.Allow);
    }

    [Fact]
    public void FetchesWhenNotYetLoaded()
    {
        ConductPlayDecision.Decide(true, false, false, true)
            .Should().Be(ConductPlayDecision.Result.Fetch);
    }

    [Fact]
    public void ShowsConductWhenNotConsented()
    {
        ConductPlayDecision.Decide(true, true, false, false)
            .Should().Be(ConductPlayDecision.Result.ShowConduct);
    }

    [Fact]
    public void ShowsBanBeforeConduct()
    {
        ConductPlayDecision.Decide(true, true, true, false)
            .Should().Be(ConductPlayDecision.Result.ShowBan);
    }

    [Fact]
    public void AllowsWhenConsented()
    {
        ConductPlayDecision.Decide(true, true, false, true)
            .Should().Be(ConductPlayDecision.Result.Allow);
    }

    [Fact]
    public void QueuedDisconnectBanStillShowsBanWhenConsented()
    {
        ConductPlayDecision.Decide(true, true, banned: true, consented: true)
            .Should().Be(ConductPlayDecision.Result.ShowBan);
    }

    [Fact]
    public void ShowsWarningBeforeConduct()
    {
        ConductPlayDecision.Decide(true, true, false, false, unackedWarning: true)
            .Should().Be(ConductPlayDecision.Result.ShowWarning);
    }

    [Fact]
    public void ShowsWarningWhenConsentedButUnacked()
    {
        ConductPlayDecision.Decide(true, true, false, true, unackedWarning: true)
            .Should().Be(ConductPlayDecision.Result.ShowWarning);
    }

    [Fact]
    public void ShowsBanBeforeWarning()
    {
        ConductPlayDecision.Decide(true, true, banned: true, consented: true, unackedWarning: true)
            .Should().Be(ConductPlayDecision.Result.ShowBan);
    }

    [Fact]
    public void UnackedWarningBlocksOfficialPlay()
    {
        ConductPlayDecision.CanPlayOfficial(banned: false, consented: true, unackedWarning: true)
            .Should().BeFalse();
    }

    [Fact]
    public void AllowsOfficialPlayAfterWarningAckWhenConsented()
    {
        ConductPlayDecision.CanPlayOfficial(banned: false, consented: true, unackedWarning: false)
            .Should().BeTrue();
    }

    [Fact]
    public void BannedDoesNotTakeWarningPlayPath()
    {
        ConductPlayDecision.CanPlayOfficial(banned: true, consented: true, unackedWarning: true)
            .Should().BeFalse();
        ConductPlayDecision.Decide(true, true, true, true, unackedWarning: true)
            .Should().Be(ConductPlayDecision.Result.ShowBan);
    }
}

public class ConductResponseWarningTests
{
    [Fact]
    public void ParsesUnackedWarningFromConduct()
    {
        ConductResponse parsed = ConductResponse.Parse(
            "{\"version\":3,\"body\":\"rules\",\"consented\":true,\"banned\":false,\"warning\":{\"id\":\"warn-1\",\"body\":\"**hello**\"}}");
        parsed.Should().NotBeNull();
        parsed.HasUnackedWarning.Should().BeTrue();
        parsed.Warning.Id.Should().Be("warn-1");
        parsed.Warning.Body.Should().Be("**hello**");
        ConductPlayDecision.CanPlayOfficial(parsed.Banned, parsed.Consented, parsed.HasUnackedWarning)
            .Should().BeFalse();
        ConductPlayDecision.Decide(true, true, parsed.Banned, parsed.Consented, parsed.HasUnackedWarning)
            .Should().Be(ConductPlayDecision.Result.ShowWarning);
    }

    [Fact]
    public void ParsesNullWarningAsAllowedWhenConsented()
    {
        ConductResponse parsed = ConductResponse.Parse(
            "{\"version\":3,\"body\":\"rules\",\"consented\":true,\"banned\":false,\"warning\":null}");
        parsed.Should().NotBeNull();
        parsed.HasUnackedWarning.Should().BeFalse();
        parsed.Warning.Should().BeNull();
        ConductPlayDecision.CanPlayOfficial(parsed.Banned, parsed.Consented, parsed.HasUnackedWarning)
            .Should().BeTrue();
        ConductPlayDecision.Decide(true, true, parsed.Banned, parsed.Consented, parsed.HasUnackedWarning)
            .Should().Be(ConductPlayDecision.Result.Allow);
    }

    [Fact]
    public void BannedConductSkipsWarningUiEvenIfWarningPresent()
    {
        ConductResponse parsed = ConductResponse.Parse(
            "{\"version\":3,\"body\":\"rules\",\"consented\":true,\"banned\":true,\"ban\":{\"reason\":\"spam\"},\"warning\":{\"id\":\"warn-1\",\"body\":\"ignored\"}}");
        parsed.Should().NotBeNull();
        parsed.Banned.Should().BeTrue();
        parsed.HasUnackedWarning.Should().BeTrue();
        ConductPlayDecision.Decide(true, true, parsed.Banned, parsed.Consented, parsed.HasUnackedWarning)
            .Should().Be(ConductPlayDecision.Result.ShowBan);
        ConductPlayDecision.CanPlayOfficial(parsed.Banned, parsed.Consented, parsed.HasUnackedWarning)
            .Should().BeFalse();
    }
}
