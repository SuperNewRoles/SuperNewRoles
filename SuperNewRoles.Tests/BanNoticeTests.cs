using FluentAssertions;
using SuperNewRoles.Safety;
using SuperNewRoles.Safety.Api;
using Xunit;

namespace SuperNewRoles.Tests;

public class BanNoticeTests
{
    [Fact]
    public void ParsesServerBanPayload()
    {
        const string raw = "SNR_BAN\n{\"inquiryId\":\"ticket-1\",\"reason\":\"spam\",\"expiresAt\":\"2026-01-01T00:00:00Z\"}";
        BanNotice.TryParse(raw, out BanInfo info).Should().BeTrue();
        info.InquiryId.Should().Be("ticket-1");
        info.Reason.Should().Be("spam");
        info.ExpiresAtIso.Should().Be("2026-01-01T00:00:00Z");
    }

    [Fact]
    public void RejectsUnrelatedDisconnectMessages()
    {
        BanNotice.TryParse("ホストにブロックされています", out _).Should().BeFalse();
        BanNotice.TryParse("ブロックしたホストの部屋には参加できません", out _).Should().BeFalse();
        BanNotice.TryParse("", out _).Should().BeFalse();
    }
}

public class ConductBanLeaveNoticeTests
{
    [Fact]
    public void ReplacesSanctionsOnly()
    {
        ConductBanLeaveNotice.ShouldReplace(DisconnectReasons.Sanctions).Should().BeTrue();
        ConductBanLeaveNotice.ShouldReplace(DisconnectReasons.Error).Should().BeFalse();
        ConductBanLeaveNotice.ShouldReplace(DisconnectReasons.Banned).Should().BeFalse();
        ConductBanLeaveNotice.ShouldReplace(DisconnectReasons.ExitGame).Should().BeFalse();
        ConductBanLeaveNotice.ShouldReplace(DisconnectReasons.Kicked).Should().BeFalse();
    }

    [Fact]
    public void SuppressesVanillaDisconnectTextForRememberedName()
    {
        ConductBanLeaveNotice.RememberName("スイカ");
        ConductBanLeaveNotice.ShouldSuppressVanilla("スイカはエラーのため退出しました").Should().BeTrue();
        ConductBanLeaveNotice.ShouldSuppressVanilla("スイカはエラーのため退出しました").Should().BeFalse();
        ConductBanLeaveNotice.ShouldSuppressVanilla("他の人はエラーのため退出しました").Should().BeFalse();
    }
}

public class HostBlockLeaveNoticeTests
{
    [Fact]
    public void SuppressesVanillaDisconnectTextForRememberedName()
    {
        HostBlockLeaveNotice.RememberName("メロン");
        HostBlockLeaveNotice.ShouldSuppressVanilla("メロンはエラーのため退出しました").Should().BeTrue();
        HostBlockLeaveNotice.ShouldSuppressVanilla("メロンはエラーのため退出しました").Should().BeFalse();
        HostBlockLeaveNotice.ShouldSuppressVanilla("他の人はエラーのため退出しました").Should().BeFalse();
    }
}

public class SafetyDisconnectCopyTests
{
    [Fact]
    public void DistinguishesHostBlockedFromViewerBlockedHost()
    {
        SafetyDisconnectCopy.TranslationKey("ホストにブロックされています")
            .Should().Be("SafetyHostBlockedDisconnect");
        SafetyDisconnectCopy.TranslationKey("ブロックしたホストの部屋には参加できません")
            .Should().Be("SafetyYouBlockedHostDisconnect");
        SafetyDisconnectCopy.TranslationKey("参加できません").Should().BeNull();
        SafetyDisconnectCopy.IsNeedConduct("行動規範への同意が必要です").Should().BeTrue();
        SafetyDisconnectCopy.IsNeedConduct("ホストにブロックされています").Should().BeFalse();
    }
}

public class SafetyBlockLookupTests
{
    [Fact]
    public void FindsByIdOrNameAndPrefersId()
    {
        var rows = new[]
        {
            new SuperNewRoles.Safety.Api.BlockRow { Id = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee", Name = "Host" },
            new SuperNewRoles.Safety.Api.BlockRow { Id = "11111111-2222-3333-4444-555555555555", Name = "Other" },
        };
        SafetyBlockLookup.Find(rows, "Host").Id.Should().Be("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        SafetyBlockLookup.Find(rows, rows[1].Id).Name.Should().Be("Other");
        SafetyBlockLookup.Find(rows, "missing").Should().BeNull();
    }
}
