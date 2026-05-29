using System.Reflection;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Neutral;
using Xunit;

namespace SuperNewRoles.Tests;

public class OrientalShamanAbilityTests
{
    [Fact]
    public void DetachToAlls_ClearsServantReferenceFromBothSides()
    {
        var shaman = CreateShamanAbility();
        var servant = new ShermansServantAbility(new ShermansServantData(
            transformCooldown: 0f,
            suicideCooldown: 0f
        ));

        servant.SetParent(shaman);
        shaman._servant = servant;

        shaman.DetachToAlls();

        shaman._servant.Should().BeNull();
        GetParentShaman(servant).Should().BeNull();
    }

    [Fact]
    public void ClearParent_DoesNotClearDifferentShamanReference()
    {
        var currentShaman = CreateShamanAbility();
        var otherShaman = CreateShamanAbility();
        var servant = new ShermansServantAbility(new ShermansServantData(
            transformCooldown: 0f,
            suicideCooldown: 0f
        ));

        servant.SetParent(currentShaman);

        servant.ClearParent(otherShaman);

        GetParentShaman(servant).Should().BeSameAs(currentShaman);
    }

    private static OrientalShamanAbility CreateShamanAbility()
    {
        return new OrientalShamanAbility(new OrientalShamanData(
            canUseVent: false,
            ventCooldown: 0f,
            ventDuration: 0f,
            canCreateServant: true,
            servantCooldown: 0f,
            isImpostorVision: false,
            neededTaskComplete: false,
            task: new TaskOptionData(0, 0, 0)
        ));
    }

    private static OrientalShamanAbility? GetParentShaman(ShermansServantAbility servant)
    {
        var field = typeof(ShermansServantAbility).GetField(
            "_orientalShamanAbility",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        return (OrientalShamanAbility?)field!.GetValue(servant);
    }
}
