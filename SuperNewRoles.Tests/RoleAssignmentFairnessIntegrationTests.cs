#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SuperNewRoles.Roles;
using Xunit;

namespace SuperNewRoles.Tests;

/// <summary>
/// 公平化コアと実際の配役統合の境界にある、ゲーム間履歴・異常時の従来抽選復帰・
/// 役職構成維持の契約を検証します。重み計算と長期分布の詳細は
/// <see cref="RoleAssignmentFairnessTests"/>で検証します。
/// </summary>
public class RoleAssignmentFairnessIntegrationTests
{
    [Fact]
    public void WinnerAfterMaximumMisses_ResetsMissesBeforeApplyingPreviousWinnerPenalty()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);

        for (int game = 0; game < RoleAssignmentFairnessTracker.MaximumMissedGames; game++)
        {
            Begin(tracker, 1, 2);
            RecordImpostorResult(tracker, candidates, winnerCandidateKey: 1);
            tracker.CommitAssignment().Should().BeTrue();
        }

        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null)
            .Should().Be(RoleAssignmentFairnessTracker.MaximumWeight);

        Begin(tracker, 1, 2);
        RecordImpostorResult(tracker, candidates, winnerCandidateKey: 0);
        tracker.CommitAssignment().Should().BeTrue();

        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null)
            .Should().Be(0.5, "winning resets the miss counter to zero before the previous-game penalty");
    }

    [Fact]
    public void NewAndReappearingClients_StartAtBaseWeight()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] originalCandidates = Candidates(1, 2);

        Begin(tracker, 1, 2);
        RecordImpostorResult(tracker, originalCandidates, winnerCandidateKey: 0);
        tracker.CommitAssignment().Should().BeTrue();

        Begin(tracker, 1, 2, 99);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(0.5);
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1.25);
        tracker.GetSnapshotWeight(99, RoleAssignmentCategory.ImpostorTeam, null)
            .Should().Be(1, "a newly joined client has no lobby history");
        tracker.AbortAssignment();

        Begin(tracker, 2, 99);
        tracker.CommitAssignment().Should().BeTrue("an absent client is removed from the active roster snapshot");

        Begin(tracker, 1, 2, 99);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null)
            .Should().Be(1, "a client that reappears after leaving must not recover its old history");
    }

    [Fact]
    public void MinimumWeightCandidate_StillHasASelectableInterval()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom(0));
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);

        Begin(tracker, 1, 2);
        RecordCustomResult(tracker, candidates, winnerCandidateKey: 0, RoleId.Sheriff);
        tracker.CommitAssignment().Should().BeTrue();

        Begin(tracker, 1, 2);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(RoleAssignmentFairnessTracker.MinimumWeight);

        Select(
                tracker,
                candidates,
                RoleAssignmentCategory.CustomMainRole,
                RoleId.Sheriff)
            .Should().ContainSingle().Which.ClientId.Should().Be(1,
                "the minimum clamp must reduce repeats without making them impossible");
    }

    [Fact]
    public void InvalidLaterRandomDraw_ProducesNoPartialSelectionOrHistory()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom(0, double.PositiveInfinity));
        RoleAssignmentCandidate[] candidates = Candidates(1, 2, 3);
        Begin(tracker, 1, 2, 3);

        tracker.TrySelectWeightedWithoutReplacement(
                candidates,
                RoleAssignmentCategory.ImpostorTeam,
                new RoleId?[] { null, null },
                out IReadOnlyList<RoleAssignmentSelection> selections,
                out RoleAssignmentFairnessFailure failure)
            .Should().BeFalse();

        failure.Should().Be(RoleAssignmentFairnessFailure.InvalidRandomValue);
        selections.Should().BeEmpty("a failed multi-slot draw must be safe to replace with the legacy path");
        tracker.CommitAssignment().Should().BeTrue();
        foreach (int clientId in new[] { 1, 2, 3 })
            tracker.GetSnapshotWeight(clientId, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
    }

    [Fact]
    public void FairSelection_PreservesRequestedRoleCompositionAndCandidateInput()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom(0.8, 0.1, 0.5));
        RoleAssignmentCandidate[] candidates = Candidates(10, 11, 12, 13);
        RoleAssignmentCandidate[] originalCandidates = candidates.ToArray();
        RoleId?[] memberRoleIds = { RoleId.Sheriff, RoleId.Mayor, RoleId.Jackal };
        Begin(tracker, 10, 11, 12, 13);

        IReadOnlyList<RoleAssignmentSelection> selections = Select(
            tracker,
            candidates,
            RoleAssignmentCategory.CustomMainRole,
            memberRoleIds);

        selections.Select(selection => selection.RoleId).Should().Equal(memberRoleIds,
            "fairness may choose recipients but must not alter role slots or their order");
        candidates.Should().Equal(originalCandidates, "the legacy caller owns its candidate pool");
        selections.Select(selection => selection.CandidateKey).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void SettingTransition_ClearsPendingAndCommittedHistory()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);

        Begin(tracker, 1, 2);
        RecordImpostorResult(tracker, candidates, winnerCandidateKey: 0);
        tracker.CommitAssignment().Should().BeTrue();
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(0.5);

        Begin(tracker, 1, 2);
        RecordImpostorResult(tracker, candidates, winnerCandidateKey: 1);
        tracker.SetEnabled(false).Should().BeTrue();
        tracker.CommitAssignment().Should().BeFalse("turning the option off aborts the current assignment");

        tracker.SetEnabled(true).Should().BeTrue();
        Begin(tracker, 1, 2);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
    }

    [Fact]
    public void ResetAfterUntrackedFallback_DropsPreviousGameHistory()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);

        Begin(tracker, 1, 2);
        RecordImpostorResult(tracker, candidates, winnerCandidateKey: 0);
        tracker.CommitAssignment().Should().BeTrue();
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(0.5);

        Begin(tracker, 1, 2);
        tracker.Reset();

        Begin(tracker, 1, 2);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null)
            .Should().Be(1, "an untracked legacy game makes the older winner history stale");
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
    }

    private static RoleAssignmentCandidate[] Candidates(params int?[] clientIds) =>
        clientIds.Select((clientId, candidateKey) => new RoleAssignmentCandidate(candidateKey, clientId)).ToArray();

    private static void Begin(RoleAssignmentFairnessTracker tracker, params int[] clientIds)
    {
        tracker.TryBeginAssignment(clientIds, out RoleAssignmentFairnessFailure failure)
            .Should().BeTrue($"assignment should begin, but failed with {failure}");
        failure.Should().Be(RoleAssignmentFairnessFailure.None);
    }

    private static void RecordImpostorResult(
        RoleAssignmentFairnessTracker tracker,
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        int winnerCandidateKey)
    {
        RoleAssignmentCandidate winner = candidates.Single(candidate => candidate.CandidateKey == winnerCandidateKey);
        tracker.TryRecordPendingResult(
                candidates,
                RoleAssignmentCategory.ImpostorTeam,
                new[] { new RoleAssignmentSelection(winner.CandidateKey, winner.ClientId, null) },
                out RoleAssignmentFairnessFailure failure)
            .Should().BeTrue($"the result should be recorded, but failed with {failure}");
    }

    private static void RecordCustomResult(
        RoleAssignmentFairnessTracker tracker,
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        int winnerCandidateKey,
        RoleId roleId)
    {
        RoleAssignmentCandidate winner = candidates.Single(candidate => candidate.CandidateKey == winnerCandidateKey);
        tracker.TryRecordPendingResult(
                candidates,
                RoleAssignmentCategory.CustomMainRole,
                new[] { new RoleAssignmentSelection(winner.CandidateKey, winner.ClientId, roleId) },
                out RoleAssignmentFairnessFailure failure)
            .Should().BeTrue($"the result should be recorded, but failed with {failure}");
    }

    private static IReadOnlyList<RoleAssignmentSelection> Select(
        RoleAssignmentFairnessTracker tracker,
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        RoleAssignmentCategory category,
        params RoleId?[] roleIds)
    {
        tracker.TrySelectWeightedWithoutReplacement(
                candidates,
                category,
                roleIds,
                out IReadOnlyList<RoleAssignmentSelection> selections,
                out RoleAssignmentFairnessFailure failure)
            .Should().BeTrue($"selection should succeed, but failed with {failure}");
        failure.Should().Be(RoleAssignmentFairnessFailure.None);
        return selections;
    }

    private sealed class SequenceRandom : IRoleAssignmentRandom
    {
        private readonly Queue<double> values;

        internal SequenceRandom(params double[] values)
        {
            this.values = new Queue<double>(values);
        }

        public double NextDouble()
        {
            if (values.Count == 0)
                throw new InvalidOperationException("No deterministic random value remains.");
            return values.Dequeue();
        }
    }
}
