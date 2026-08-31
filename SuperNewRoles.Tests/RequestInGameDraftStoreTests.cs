using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        RequestInGameDraftStore.Flush();
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
    public void Save_WhenSavePathCannotBeWritten_DoesNotThrow()
    {
        string directoryPath = Path.Combine(tempDirectory, "DraftDirectory");
        Directory.CreateDirectory(directoryPath);
        RequestInGameDraftStore.SetTestSaveFilePath(directoryPath);

        Action act = () =>
        {
            RequestInGameDraftStore.Save(
                RequestInGameType.Bug,
                new RequestInGameDraft("title", "description", "Skeld", "Sheriff", "Meeting"));
            RequestInGameDraftStore.Flush();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Load_CorruptJson_ReturnsEmptyDraft()
    {
        File.WriteAllText(saveFilePath, "{ this is not valid json");

        RequestInGameDraftStore.Load(RequestInGameType.Bug).Should().Be(RequestInGameDraft.Empty);
    }

    [Fact]
    public void Save_KeepsDraftInMemory_WithoutImmediateDiskWrite()
    {
        RequestInGameDraft draft = new("title", "description", "Skeld", "Sheriff", "Meeting");

        RequestInGameDraftStore.Save(RequestInGameType.Bug, draft);

        RequestInGameDraftStore.Load(RequestInGameType.Bug).Should().Be(draft);
        File.Exists(saveFilePath).Should().BeFalse();
    }

    [Fact]
    public void Flush_WritesDraftToDiskAtomically()
    {
        RequestInGameDraft draft = new("title", "description", "Skeld", "Sheriff", "Meeting");
        RequestInGameDraftStore.Save(RequestInGameType.Bug, draft);

        RequestInGameDraftStore.Flush();

        File.Exists(saveFilePath).Should().BeTrue();
        File.Exists(saveFilePath + ".tmp").Should().BeFalse();

        RequestInGameDraftStore.SetTestSaveFilePath(saveFilePath);
        RequestInGameDraftStore.Load(RequestInGameType.Bug).Should().Be(draft);
    }

    [Fact]
    public void Flush_WritesMostRecentDraft_AfterMultipleSaves()
    {
        RequestInGameDraft older = new("older", "older description", "Skeld", "Sheriff", "Meeting");
        RequestInGameDraft newer = new("newer", "newer description", "Mira", "Madmate", "Task");

        RequestInGameDraftStore.Save(RequestInGameType.Bug, older);
        RequestInGameDraftStore.Save(RequestInGameType.Bug, newer);
        RequestInGameDraftStore.Flush();

        RequestInGameDraftStore.SetTestSaveFilePath(saveFilePath);
        RequestInGameDraftStore.Load(RequestInGameType.Bug).Should().Be(newer);
    }

    [Fact]
    public async Task Flush_LatestSnapshotWinsOverInFlightOlderWrite()
    {
        RequestInGameDraft older = new("older", "older description", "Skeld", "Sheriff", "Meeting");
        RequestInGameDraft newer = new("newer", "newer description", "Mira", "Madmate", "Task");

        RequestInGameDraftStore.Save(RequestInGameType.Bug, older);
        RequestInGameDraftStore.SetTestDiskWriteHoldMilliseconds(400);
        Task olderFlush = Task.Run(RequestInGameDraftStore.Flush);
        RequestInGameDraftStore.WaitForTestDiskWriteHold(2000).Should().BeTrue();

        RequestInGameDraftStore.Save(RequestInGameType.Bug, newer);
        RequestInGameDraftStore.Flush();
        await olderFlush.WaitAsync(TimeSpan.FromSeconds(5));

        RequestInGameDraftStore.SetTestSaveFilePath(saveFilePath);
        RequestInGameDraftStore.Load(RequestInGameType.Bug).Should().Be(newer);
    }
}
