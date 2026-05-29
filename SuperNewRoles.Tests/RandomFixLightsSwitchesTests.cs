using System.Reflection;
using FluentAssertions;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.GameSettings;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class RandomFixLightsSwitchesTests
{
    [Fact]
    public void Option_DefaultsOffAndBelongsToGameSettings()
    {
        var field = typeof(GameSettingOptions).GetField(nameof(GameSettingOptions.RandomizeFixLightsSwitches), BindingFlags.Public | BindingFlags.Static);

        field.Should().NotBeNull();
        var attribute = field!.GetCustomAttribute<CustomOptionBoolAttribute>();
        attribute.Should().NotBeNull();
        attribute!.DefaultValue.Should().BeFalse();
        attribute.ParentFieldName.Should().Be(nameof(Categories.GameSettings));
    }

    [Fact]
    public void CreateRandomActualSwitches_GuaranteesAtLeastOneDifferentSwitch()
    {
        for (byte expected = 0; expected < 32; expected++)
        {
            byte actual = RandomFixLightsSwitches.CreateRandomActualSwitches(
                expected,
                switchCount: 5,
                nextSwitchIndex: _ => 2,
                drawAdditionalSwitch: () => false);

            (actual ^ expected).Should().Be(1 << 2);
        }
    }

    [Fact]
    public void CreateRandomActualSwitches_CanLeaveCenterSwitchUnchanged()
    {
        byte actual = RandomFixLightsSwitches.CreateRandomActualSwitches(
            expectedSwitches: 0,
            switchCount: 5,
            nextSwitchIndex: _ => 0,
            drawAdditionalSwitch: () => false);

        actual.Should().Be(1);
        (actual & (1 << 2)).Should().Be(0);
    }

    [Fact]
    public void CreateRandomActualSwitches_DrawsRemainingSwitches()
    {
        byte actual = RandomFixLightsSwitches.CreateRandomActualSwitches(
            expectedSwitches: 0,
            switchCount: 5,
            nextSwitchIndex: _ => 1,
            drawAdditionalSwitch: () => true);

        actual.Should().Be(0b1_1111);
    }

    [Fact]
    public void CreateRandomDamageAmount_PreservesDamageBit()
    {
        byte amount = RandomFixLightsSwitches.CreateRandomDamageAmount(
            switchCount: 5,
            nextSwitchIndex: _ => 0,
            drawAdditionalSwitch: () => false);

        (amount & RandomFixLightsSwitches.DamageSystem).Should().Be(RandomFixLightsSwitches.DamageSystem);
        (amount & RandomFixLightsSwitches.SwitchesMask).Should().Be(1);
    }
}
