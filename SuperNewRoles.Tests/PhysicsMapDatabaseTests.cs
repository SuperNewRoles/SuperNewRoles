using FluentAssertions;
using SuperNewRoles.MapDatabase;
using Xunit;

namespace SuperNewRoles.Tests;

public class PhysicsMapDatabaseTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void HasUsableMapGeometry_RequiresNonNullVentOrRoomWithArea(
        bool hasNonNullVent,
        bool hasRoomWithArea,
        bool expected)
    {
        PhysicsMapDatabase.HasUsableMapGeometry(hasNonNullVent, hasRoomWithArea)
            .Should().Be(expected);
    }

    [Fact]
    public void HasUsableMapGeometry_IsFalse_WhenVentsAndRoomsAreMissing()
    {
        PhysicsMapDatabase.HasUsableMapGeometry(default(Vent[]), default(PlainShipRoom[]))
            .Should().BeFalse();
        PhysicsMapDatabase.HasUsableMapGeometry([], [])
            .Should().BeFalse();
    }
}
