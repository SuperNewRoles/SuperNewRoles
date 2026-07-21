using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using SuperNewRoles.Patches;
using Xunit;

namespace SuperNewRoles.Tests;

public class ModdedNetworkTransformTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(25, 500, true)]
    [InlineData(-1, 0, false)]
    [InlineData(1, 19, false)]
    [InlineData(1, 21, false)]
    [InlineData(257, 5140, false)]
    public void IsValidBatchMovementPayload_ValidatesCountAndRemainingBytes(
        int count,
        int bytesRemaining,
        bool expected
    )
    {
        MethodInfo isValidBatchMovementPayload = typeof(ModdedNetworkTransform).GetMethod(
            "IsValidBatchMovementPayload",
            BindingFlags.Static | BindingFlags.NonPublic
        )!;

        bool result = (bool)isValidBatchMovementPayload.Invoke(null, [count, bytesRemaining])!;

        result.Should().Be(expected);
    }

    [Fact]
    public void EnqueueMovementData_CapsTheQueueAtTwentyEntries()
    {
        var queue = new Queue<ModdedNetworkTransform.MovementData>();
        MethodInfo enqueueMovementData = typeof(ModdedNetworkTransform).GetMethod(
            "EnqueueMovementData",
            BindingFlags.Static | BindingFlags.NonPublic
        )!;

        for (int i = 0; i < 25; i++)
        {
            var movementData = default(ModdedNetworkTransform.MovementData);
            movementData.velocity.x = i;
            enqueueMovementData.Invoke(null, [queue, movementData]);
        }

        queue.Should().HaveCount(20);
        queue.Peek().velocity.x.Should().Be(5);
    }
}
