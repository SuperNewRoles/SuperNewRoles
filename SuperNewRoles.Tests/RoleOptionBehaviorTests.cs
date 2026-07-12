using AmongUs.GameOptions;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Neutral;
using Xunit;

namespace SuperNewRoles.Tests;

public class RoleOptionBehaviorTests
{
    [Theory]
    [InlineData(BlackHatHackerInfectionScopeType.Short, 1f)]
    [InlineData(BlackHatHackerInfectionScopeType.Medium, 1.8f)]
    [InlineData(BlackHatHackerInfectionScopeType.Long, 2.5f)]
    public void BlackHatHackerInfectionScope_UsesConfiguredDistance(
        BlackHatHackerInfectionScopeType scope,
        float expected)
    {
        BlackHatHackerInfectionScope.GetDistance(scope).Should().Be(expected);
    }

    [Theory]
    [InlineData(false, false, RoleId.Bullet)]
    [InlineData(false, true, RoleId.Bullet)]
    [InlineData(true, false, RoleId.Jackal)]
    [InlineData(true, true, RoleId.WaveCannonJackal)]
    public void BulletPromotion_RespectsWaveCannonInheritanceSetting(
        bool canPromote,
        bool hasWaveCannon,
        RoleId expected)
    {
        JackalAbility.GetBulletPromoteRole(canPromote, hasWaveCannon).Should().Be(expected);
    }

    [Theory]
    [InlineData(TaskTypes.ResetReactor, true, false, true, true, false)]
    [InlineData(TaskTypes.ResetSeismic, true, false, true, true, false)]
    [InlineData(TaskTypes.RestoreOxy, true, false, true, true, false)]
    [InlineData(TaskTypes.StopCharles, true, false, true, true, false)]
    [InlineData(TaskTypes.FixLights, true, true, false, true, false)]
    [InlineData(TaskTypes.FixComms, true, true, true, false, false)]
    [InlineData(TaskTypes.FixLights, true, true, true, true, true)]
    [InlineData(TaskTypes.MushroomMixupSabotage, true, false, false, false, true)]
    [InlineData(TaskTypes.ResetReactor, false, true, true, true, false)]
    [InlineData(TaskTypes.UploadData, true, true, true, true, false)]
    public void TaskmasterInstantRepair_RespectsChildSettings(
        TaskTypes taskType,
        bool enabled,
        bool reactorOxygenElevator,
        bool lights,
        bool comms,
        bool expected)
    {
        Taskmaster.CanFixSabotageInstantly(
            taskType,
            enabled,
            reactorOxygenElevator,
            lights,
            comms
        ).Should().Be(expected);
    }
}
