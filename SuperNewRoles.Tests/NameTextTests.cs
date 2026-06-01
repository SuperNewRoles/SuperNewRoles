using AmongUs.GameOptions;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

[Collection("ModTranslation")]
public class NameTextTests
{
    [Theory]
    [InlineData(RoleTypes.CrewmateGhost, SupportedLangs.Japanese, "クルーメイト")]
    [InlineData(RoleTypes.ImpostorGhost, SupportedLangs.Japanese, "インポスター")]
    [InlineData(RoleTypes.CrewmateGhost, SupportedLangs.English, "Crewmate")]
    [InlineData(RoleTypes.ImpostorGhost, SupportedLangs.English, "Impostor")]
    public void GetVanillaRoleDisplayName_NormalizesGhostRoles(RoleTypes roleType, SupportedLangs language, string expected)
    {
        try
        {
            ModTranslation.ClearTestTranslationCsv();
            ModTranslation.SetTestLanguage(language);
            ModTranslation.Cleanup();
            ModTranslation.Load();

            NameText.GetVanillaRoleDisplayName(roleType, "STRMISS").Should().Be(expected);
        }
        finally
        {
            ModTranslation.SetTestLanguage(SupportedLangs.English);
            ModTranslation.Cleanup();
        }
    }

    [Fact]
    public void GetVanillaRoleDisplayName_KeepsNonGhostRoleNiceName()
    {
        NameText.GetVanillaRoleDisplayName(RoleTypes.Engineer, "Engineer").Should().Be("Engineer");
    }
}
