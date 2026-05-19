using System;
using System.IO;
using FluentAssertions;
using SuperNewRoles.RequestInGame;
using Xunit;

namespace SuperNewRoles.Tests;

public class RequestInGameDraftStoreTests : IDisposable
{
    private readonly string tempDirectory;
    private readonly string saveFilePath;

    public RequestInGameDraftStoreTests()
    {
        tempDirectory = Path.Combine(Path.GetTempPath(), "SNR_RequestInGameDraftStoreTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        saveFilePath = Path.Combine(tempDirectory, "RequestInGameDrafts.json");
        RequestInGameDraftStore.SetTestSaveFilePath(saveFilePath);
    }

    public void Dispose()
    {
        RequestInGameDraftStore.ClearTestSaveFilePath();
        if (Directory.Exists(tempDirectory))
            Directory.Delete(tempDirectory, true);
    }

    [Fact]
    public void Load_ReturnsSavedDraft_PerRequestType()
    {
        RequestInGameDraft bugDraft = new("bug title", "bug description", "Skeld", "Sheriff", "Meeting");
        RequestInGameDraft questionDraft = new("question title", "question description", string.Empty, string.Empty, string.Empty);

        RequestInGameDraftStore.Save(RequestInGameType.Bug, bugDraft);
        RequestInGameDraftStore.Save(RequestInGameType.Question, questionDraft);

        RequestInGameDraftStore.Load(RequestInGameType.Bug).Should().Be(bugDraft);
        RequestInGameDraftStore.Load(RequestInGameType.Question).Should().Be(questionDraft);
    }

    [Fact]
    public void Clear_RemovesOnlyTargetRequestType()
    {
        RequestInGameDraft bugDraft = new("bug title", "bug description", "Skeld", "Sheriff", "Meeting");
        RequestInGameDraft requestDraft = new("request title", "request description", string.Empty, string.Empty, string.Empty);
        RequestInGameDraftStore.Save(RequestInGameType.Bug, bugDraft);
        RequestInGameDraftStore.Save(RequestInGameType.Request, requestDraft);

        RequestInGameDraftStore.Clear(RequestInGameType.Bug);

        RequestInGameDraftStore.Load(RequestInGameType.Bug).Should().Be(RequestInGameDraft.Empty);
        RequestInGameDraftStore.Load(RequestInGameType.Request).Should().Be(requestDraft);
    }

    [Fact]
    public void Save_EmptyDraft_RemovesTargetRequestType()
    {
        RequestInGameDraft draft = new("title", "description", string.Empty, string.Empty, string.Empty);
        RequestInGameDraftStore.Save(RequestInGameType.Other, draft);

        RequestInGameDraftStore.Save(RequestInGameType.Other, RequestInGameDraft.Empty);

        RequestInGameDraftStore.Load(RequestInGameType.Other).Should().Be(RequestInGameDraft.Empty);
    }

    [Fact]
    public void Load_CorruptJson_ReturnsEmptyDraft()
    {
        File.WriteAllText(saveFilePath, "{ this is not valid json");

        RequestInGameDraftStore.Load(RequestInGameType.Bug).Should().Be(RequestInGameDraft.Empty);
    }
}
