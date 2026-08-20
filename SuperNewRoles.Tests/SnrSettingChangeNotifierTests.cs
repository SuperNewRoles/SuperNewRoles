using FluentAssertions;
using SuperNewRoles.CustomOptions;
using Xunit;

namespace SuperNewRoles.Tests;

// 設定変更通知がバニラの StringNames 翻訳表を破壊せず、
// 偽キーを安全に識別できることを検証する。
public class SnrSettingChangeNotifierTests
{
    // 目的: 同じ通知キーからは常に同じ StringNames が生成される
    [Fact]
    public void GetNotificationStringName_IsDeterministic()
    {
        var first = SnrSettingChangeNotifier.GetNotificationStringName("role:Sheriff");
        var second = SnrSettingChangeNotifier.GetNotificationStringName("role:Sheriff");

        first.Should().Be(second);
    }

    // 目的: 異なる通知キーは異なる StringNames になる
    [Fact]
    public void GetNotificationStringName_DiffersByKey()
    {
        var roleKey = SnrSettingChangeNotifier.GetNotificationStringName("role:Sheriff");
        var optionKey = SnrSettingChangeNotifier.GetNotificationStringName("option:CoolTime");

        roleKey.Should().NotBe(optionKey);
    }

    // 目的: 生成した StringNames はバニラ翻訳表の範囲外として識別される
    [Fact]
    public void GeneratedStringName_IsIdentifiedAsSnrNotification()
    {
        var generated = SnrSettingChangeNotifier.GetNotificationStringName("role:Sheriff");

        SnrSettingChangeNotifier.IsSnrNotificationStringName(generated).Should().BeTrue();
        ((int)generated).Should().BeGreaterThanOrEqualTo(SnrSettingChangeNotifier.NotificationKeyBase);
    }

    // 目的: 通知キーの識別範囲が生成処理の境界内に限定される
    [Theory]
    [InlineData(SnrSettingChangeNotifier.NotificationKeyBase - 1, false)]
    [InlineData(SnrSettingChangeNotifier.NotificationKeyBase, true)]
    [InlineData(SnrSettingChangeNotifier.NotificationKeyMax, true)]
    [InlineData(SnrSettingChangeNotifier.NotificationKeyMax + 1, false)]
    public void NotificationStringName_IsIdentifiedOnlyWithinGeneratedRange(int value, bool expected)
    {
        var id = (StringNames)value;

        SnrSettingChangeNotifier.IsSnrNotificationStringName(id).Should().Be(expected);
        SnrSettingChangeNotifier.TryResolveNotificationString(id, out var result).Should().Be(expected);
        if (expected)
            result.Should().BeEmpty();
        else
            result.Should().BeNull();
    }

    // 目的: バニラの StringNames は SNR 通知キーとして誤検出しない
    [Theory]
    [InlineData(StringNames.None)]
    [InlineData(StringNames.GameNumImpostors)]
    [InlineData(StringNames.GameKillCooldown)]
    public void VanillaStringNames_AreNotSnrNotificationKeys(StringNames vanillaName)
    {
        SnrSettingChangeNotifier.IsSnrNotificationStringName(vanillaName).Should().BeFalse();
        SnrSettingChangeNotifier.TryResolveNotificationString(vanillaName, out var result).Should().BeFalse();
        result.Should().BeNull();
    }

    // 目的: SNR 通知キーはバニラ GetString に渡さず空文字へ解決できる
    [Fact]
    public void TryResolveNotificationString_ReturnsEmptyForUnregisteredSnrKey()
    {
        var generated = SnrSettingChangeNotifier.GetNotificationStringName("role:None");

        SnrSettingChangeNotifier.TryResolveNotificationString(generated, out var result).Should().BeTrue();
        result.Should().BeEmpty();
    }
}
