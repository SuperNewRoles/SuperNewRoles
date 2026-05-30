using FluentAssertions;
using SuperNewRoles.HelpMenus.MenuCategories;
using Xunit;

namespace SuperNewRoles.Tests;

public class ModSettingsInformationHelpMenuTests
{
    [Fact]
    public void CalculateExclusivityTextBounds_UsesDisplayedRowSpacing()
    {
        ModSettingsInformationHelpMenu.CalculateExclusivityTextBounds(20)
            .Should().Be(20 * ModSettingsInformationHelpMenu.optionYOffset);
    }

    [Fact]
    public void CalculateExclusivityTextBounds_ClampsNegativeLineCounts()
    {
        ModSettingsInformationHelpMenu.CalculateExclusivityTextBounds(-1)
            .Should().Be(0);
    }
}
