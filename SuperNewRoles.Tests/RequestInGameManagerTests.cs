using FluentAssertions;
using SuperNewRoles.RequestInGame;
using UnityEngine.Networking;
using Xunit;

namespace SuperNewRoles.Tests;

public class RequestInGameManagerTests
{
    [Theory]
    [InlineData(UnityWebRequest.Result.ConnectionError, 0, true, false)]
    [InlineData(UnityWebRequest.Result.DataProcessingError, 0, true, false)]
    [InlineData(UnityWebRequest.Result.ProtocolError, 500, true, false)]
    [InlineData(UnityWebRequest.Result.ProtocolError, 503, true, false)]
    [InlineData(UnityWebRequest.Result.ProtocolError, 401, true, true)]
    [InlineData(UnityWebRequest.Result.ProtocolError, 403, true, true)]
    [InlineData(UnityWebRequest.Result.ProtocolError, 401, false, false)]
    [InlineData(UnityWebRequest.Result.ProtocolError, 403, false, false)]
    [InlineData(UnityWebRequest.Result.Success, 200, true, false)]
    public void ShouldRecreateTokenAfterValidationFailure_Recreates_Only_When_Auth_Is_Rejected_And_Creation_Is_Allowed(UnityWebRequest.Result result, long responseCode, bool createIfMissing, bool expected)
    {
        RequestInGameManager.ShouldRecreateTokenAfterValidationFailure(result, responseCode, createIfMissing).Should().Be(expected);
    }
}
