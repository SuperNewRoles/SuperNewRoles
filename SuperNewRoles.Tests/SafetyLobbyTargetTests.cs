using FluentAssertions;
using SuperNewRoles.Safety;
using Xunit;

namespace SuperNewRoles.Tests;

public class SafetyLobbyTargetTests
{
    [Fact]
    public void RejectsPreviousMatchEvenWhenClientIdCollides()
    {
        var live = new[] { (Id: 5, Name: "CurrentPlayer") };
        SafetyLobbyTarget.ResolveClientId(
            currentGameId: 200,
            recordedGameId: 100,
            recordedClientId: 5,
            recordedName: "OldPlayer",
            live).Should().BeNull();
    }

    [Fact]
    public void AcceptsSameMatchClientId()
    {
        var live = new[] { (Id: 5, Name: "Blue") };
        SafetyLobbyTarget.ResolveClientId(200, 200, 5, "Blue", live).Should().Be(5);
    }

    [Fact]
    public void AcceptsSameMatchClientIdEvenIfDisplayedNameDiffers()
    {
        var live = new[] { (Id: 5, Name: "Blue") };
        SafetyLobbyTarget.ResolveClientId(200, 200, 5, "Red", live).Should().Be(5);
    }

    [Fact]
    public void AcceptsSameMatchWhenRecordedNameIsEmpty()
    {
        var live = new[] { (Id: 5, Name: "Blue") };
        SafetyLobbyTarget.ResolveClientId(200, 200, 5, "", live).Should().Be(5);
    }

    [Fact]
    public void RejectsWhenNotInAGame()
    {
        var live = new[] { (Id: 5, Name: "Blue") };
        SafetyLobbyTarget.ResolveClientId(0, 0, 5, "Blue", live).Should().BeNull();
        SafetyLobbyTarget.IsCurrentMatch(0, 0).Should().BeFalse();
    }

    [Fact]
    public void RejectsMissingLiveClient()
    {
        var live = new[] { (Id: 8, Name: "Blue") };
        SafetyLobbyTarget.ResolveClientId(200, 200, 5, "Blue", live).Should().BeNull();
    }
}
