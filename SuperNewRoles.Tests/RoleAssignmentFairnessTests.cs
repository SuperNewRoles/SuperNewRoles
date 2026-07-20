using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SuperNewRoles.Roles;
using Xunit;

namespace SuperNewRoles.Tests;

/// <summary>
/// 役職配布公平化の重み境界、履歴更新、異常入力、非復元抽選、長期分布を検証します。
/// 実ゲームの乱数へ依存しないよう、単体テストでは差し替え可能な決定論的乱数を使用します。
/// </summary>
public class RoleAssignmentFairnessTests
{
    [Fact]
    public void CryptographicRandom_AlwaysReturnsHalfOpenUnitValues()
    {
        var random = new CryptographicRoleAssignmentRandom();

        for (int index = 0; index < 20_000; index++)
            random.NextDouble().Should().BeGreaterThanOrEqualTo(0).And.BeLessThan(1);
    }

    [Fact]
    public void Weight_UsesOnlyPreviousGameSnapshotUntilCommit()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom(0));
        RoleAssignmentCandidate[] candidates = Candidates(10, 11);

        Begin(tracker, 10, 11);
        IReadOnlyList<RoleAssignmentSelection> selected = Select(
            tracker,
            candidates,
            RoleAssignmentCategory.CustomMainRole,
            RoleId.Sheriff);
        Record(tracker, candidates, RoleAssignmentCategory.CustomMainRole, selected);

        tracker.GetSnapshotWeight(10, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(1, "pending results must not affect selections in the same game");
        tracker.CommitAssignment().Should().BeTrue();

        tracker.GetSnapshotWeight(10, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(0.25, "the previous-category and same-role penalties both apply");
        tracker.GetSnapshotWeight(10, RoleAssignmentCategory.CustomMainRole, RoleId.Mayor)
            .Should().Be(0.5, "only the previous-category penalty applies for another role");
        tracker.GetSnapshotWeight(11, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(1.25, "one eligible loss adds one missed game");
        tracker.GetSnapshotWeight(null, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(1, "unresolved identities are never tracked");
    }

    [Fact]
    public void Weight_MissedGamesAreCappedAtFour()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);

        for (int game = 1; game <= 6; game++)
        {
            Begin(tracker, 1, 2);
            var winner = new[] { new RoleAssignmentSelection(1, 2, null) };
            Record(tracker, candidates, RoleAssignmentCategory.ImpostorTeam, winner);
            tracker.CommitAssignment().Should().BeTrue();

            double expected = Math.Min(2, 1 + (0.25 * game));
            tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null)
                .Should().Be(expected);
        }
    }

    [Fact]
    public void Weight_TracksImpostorAndCustomMainRoleCategoriesIndependently()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);

        Begin(tracker, 1, 2);
        Record(
            tracker,
            candidates,
            RoleAssignmentCategory.ImpostorTeam,
            new[] { new RoleAssignmentSelection(0, 1, null) });
        tracker.CommitAssignment().Should().BeTrue();

        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(0.5);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(1);
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1.25);
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(1);
    }

    [Fact]
    public void Commit_IncrementsEligibilityOnlyOnceAcrossMultipleRolePools()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        Begin(tracker, 1, 2, 3);

        RoleAssignmentCandidate[] firstPool = Candidates(1, 2);
        Record(
            tracker,
            firstPool,
            RoleAssignmentCategory.CustomMainRole,
            new[] { new RoleAssignmentSelection(1, 2, RoleId.Sheriff) });

        RoleAssignmentCandidate[] secondPool = Candidates(1, 3);
        Record(
            tracker,
            secondPool,
            RoleAssignmentCategory.CustomMainRole,
            new[] { new RoleAssignmentSelection(1, 3, RoleId.Mayor) });

        tracker.CommitAssignment().Should().BeTrue();
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(1.25);
    }

    [Fact]
    public void Commit_NoOpportunityPreservesMissesButClearsPreviousWinnerPenalty()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);

        Begin(tracker, 1, 2);
        Record(
            tracker,
            candidates,
            RoleAssignmentCategory.ImpostorTeam,
            new[] { new RoleAssignmentSelection(0, 1, null) });
        tracker.CommitAssignment().Should().BeTrue();
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(0.5);
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1.25);

        Begin(tracker, 1, 2);
        tracker.CommitAssignment().Should().BeTrue();

        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null)
            .Should().Be(1, "the client did not win the immediately preceding game");
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.ImpostorTeam, null)
            .Should().Be(1.25, "a game without an opportunity must not add a miss");
    }

    [Fact]
    public void Selection_IsWeightedWithoutReplacementAndPreservesSlotRoleOrder()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom(0.99, 0));
        RoleAssignmentCandidate[] candidates = Candidates(10, 11, 12);
        Begin(tracker, 10, 11, 12);

        IReadOnlyList<RoleAssignmentSelection> selected = Select(
            tracker,
            candidates,
            RoleAssignmentCategory.CustomMainRole,
            RoleId.Sheriff,
            RoleId.Mayor);

        selected.Select(value => value.ClientId).Should().Equal(12, 10);
        selected.Select(value => value.RoleId).Should().Equal(RoleId.Sheriff, RoleId.Mayor);
        selected.Select(value => value.CandidateKey).Should().OnlyHaveUniqueItems();

        Record(tracker, candidates, RoleAssignmentCategory.CustomMainRole, selected);
        tracker.CommitAssignment().Should().BeTrue();
        tracker.GetSnapshotWeight(12, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(0.25, "the actual first team member role is retained");
        tracker.GetSnapshotWeight(10, RoleAssignmentCategory.CustomMainRole, RoleId.Mayor)
            .Should().Be(0.25, "the actual second team member role is retained");
    }

    [Fact]
    public void Selection_HandlesZeroOneAndExcessiveSlotCounts()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom(0));
        RoleAssignmentCandidate[] candidates = Candidates(7);
        Begin(tracker, 7);

        tracker.TrySelectWeightedWithoutReplacement(
                candidates,
                RoleAssignmentCategory.ImpostorTeam,
                Array.Empty<RoleId?>(),
                out IReadOnlyList<RoleAssignmentSelection> none,
                out RoleAssignmentFairnessFailure noSlotFailure)
            .Should().BeTrue();
        none.Should().BeEmpty();
        noSlotFailure.Should().Be(RoleAssignmentFairnessFailure.None);

        Select(tracker, candidates, RoleAssignmentCategory.ImpostorTeam, new RoleId?[] { null })
            .Should().ContainSingle().Which.ClientId.Should().Be(7);

        tracker.TrySelectWeightedWithoutReplacement(
                candidates,
                RoleAssignmentCategory.ImpostorTeam,
                new RoleId?[] { null, null },
                out _,
                out RoleAssignmentFairnessFailure failure)
            .Should().BeFalse();
        failure.Should().Be(RoleAssignmentFairnessFailure.TooManySlots);
    }

    [Fact]
    public void Selection_RejectsInvalidCandidatesAndRandomValuesWithoutPendingChanges()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom(double.NaN));
        Begin(tracker, 1, 2);

        tracker.TrySelectWeightedWithoutReplacement(
                new[]
                {
                    new RoleAssignmentCandidate(0, 1),
                    new RoleAssignmentCandidate(1, 1)
                },
                RoleAssignmentCategory.ImpostorTeam,
                new RoleId?[] { null },
                out _,
                out RoleAssignmentFairnessFailure duplicateFailure)
            .Should().BeFalse();
        duplicateFailure.Should().Be(RoleAssignmentFairnessFailure.DuplicateClientId);

        tracker.TrySelectWeightedWithoutReplacement(
                Candidates(1, 2),
                RoleAssignmentCategory.ImpostorTeam,
                new RoleId?[] { null },
                out _,
                out RoleAssignmentFairnessFailure randomFailure)
            .Should().BeFalse();
        randomFailure.Should().Be(RoleAssignmentFairnessFailure.InvalidRandomValue);

        tracker.CommitAssignment().Should().BeTrue();
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
    }

    [Fact]
    public void Validation_RejectsDuplicateActiveAndCandidateKeysBeforeDrawingRandomness()
    {
        var activeTracker = new RoleAssignmentFairnessTracker(new SequenceRandom(0));
        activeTracker.TryBeginAssignment(
                new[] { 1, 1 },
                out RoleAssignmentFairnessFailure activeFailure)
            .Should().BeFalse();
        activeFailure.Should().Be(RoleAssignmentFairnessFailure.DuplicateActiveClient);

        var random = new SequenceRandom(0);
        var candidateTracker = new RoleAssignmentFairnessTracker(random);
        Begin(candidateTracker, 1, 2);
        candidateTracker.TrySelectWeightedWithoutReplacement(
                new[]
                {
                    new RoleAssignmentCandidate(0, 1),
                    new RoleAssignmentCandidate(0, 2)
                },
                RoleAssignmentCategory.ImpostorTeam,
                new RoleId?[] { null },
                out _,
                out RoleAssignmentFairnessFailure candidateFailure)
            .Should().BeFalse();
        candidateFailure.Should().Be(RoleAssignmentFairnessFailure.DuplicateCandidate);
        random.Remaining.Should().Be(1, "invalid input must fall back before consuming randomness");
    }

    [Fact]
    public void PendingRecord_IsAtomicWhenASelectionIsInvalid()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);
        Begin(tracker, 1, 2);

        tracker.TryRecordPendingResult(
                candidates,
                RoleAssignmentCategory.CustomMainRole,
                new[] { new RoleAssignmentSelection(99, 1, RoleId.Sheriff) },
                out RoleAssignmentFairnessFailure failure)
            .Should().BeFalse();
        failure.Should().Be(RoleAssignmentFairnessFailure.SelectionNotEligible);

        tracker.CommitAssignment().Should().BeTrue();
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff).Should().Be(1);
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff).Should().Be(1);
    }

    [Fact]
    public void Selection_RejectsAPreviouslyPendingWinnerBeforeConsumingRandomness()
    {
        var random = new SequenceRandom(0);
        var tracker = new RoleAssignmentFairnessTracker(random);
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);
        Begin(tracker, 1, 2);
        Record(
            tracker,
            candidates,
            RoleAssignmentCategory.CustomMainRole,
            new[] { new RoleAssignmentSelection(0, 1, RoleId.Sheriff) });

        tracker.TrySelectWeightedWithoutReplacement(
                candidates,
                RoleAssignmentCategory.CustomMainRole,
                new RoleId?[] { RoleId.Mayor },
                out _,
                out RoleAssignmentFairnessFailure failure)
            .Should().BeFalse();
        failure.Should().Be(RoleAssignmentFairnessFailure.DuplicatePendingSelection);
        random.Remaining.Should().Be(1, "invalid candidates must fall back before drawing randomness");
    }

    [Fact]
    public void UnknownIdentity_AlwaysHasBaseWeightAndNoHistory()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom(0, 0));
        RoleAssignmentCandidate[] candidates = { new(0, null) };

        Begin(tracker);
        IReadOnlyList<RoleAssignmentSelection> selected = Select(
            tracker,
            candidates,
            RoleAssignmentCategory.CustomMainRole,
            RoleId.Sheriff);
        Record(tracker, candidates, RoleAssignmentCategory.CustomMainRole, selected);
        tracker.CommitAssignment().Should().BeTrue();

        Begin(tracker);
        tracker.GetSnapshotWeight(null, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().Be(1);
        Select(tracker, candidates, RoleAssignmentCategory.CustomMainRole, RoleId.Sheriff)
            .Should().ContainSingle();
    }

    [Fact]
    public void LifecycleOperations_ClearOnlyTheirDocumentedState()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        tracker.SetLobby(100).Should().BeTrue();
        MakeClientWinImpostorGame(tracker, 1, 2);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(0.5);

        tracker.SetLobby(100).Should().BeFalse("replaying in the same lobby preserves history");
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(0.5);

        tracker.SetLobby(101).Should().BeTrue();
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
        tracker.LobbyId.Should().Be(101);

        MakeClientWinImpostorGame(tracker, 1, 2);
        tracker.SetEnabled(false).Should().BeTrue();
        tracker.TryBeginAssignment(new[] { 1, 2 }, out RoleAssignmentFairnessFailure disabledFailure)
            .Should().BeFalse();
        disabledFailure.Should().Be(RoleAssignmentFairnessFailure.Disabled);
        tracker.SetEnabled(true).Should().BeTrue();
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);

        MakeClientWinImpostorGame(tracker, 1, 2);
        tracker.Reset();
        tracker.LobbyId.Should().Be(101, "mode and host resets retain lobby identity");
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);

        tracker.LeaveLobby();
        tracker.LobbyId.Should().BeNull();
    }

    [Fact]
    public void RemoveClient_ImmediatelyRemovesSnapshotPendingAndHistory()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        MakeClientWinImpostorGame(tracker, 1, 2);

        Begin(tracker, 1, 2);
        tracker.RemoveClient(1);
        tracker.TrySelectWeightedWithoutReplacement(
                Candidates(1),
                RoleAssignmentCategory.ImpostorTeam,
                new RoleId?[] { null },
                out _,
                out RoleAssignmentFairnessFailure failure)
            .Should().BeFalse();
        failure.Should().Be(RoleAssignmentFairnessFailure.CandidateNotActive);
        tracker.CommitAssignment().Should().BeTrue();

        Begin(tracker, 1, 2);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null)
            .Should().Be(1, "a reconnect is a new untracked identity state");
    }

    [Fact]
    public void AbortAssignment_DiscardsPendingResults()
    {
        var tracker = new RoleAssignmentFairnessTracker(new SequenceRandom());
        RoleAssignmentCandidate[] candidates = Candidates(1, 2);
        Begin(tracker, 1, 2);
        Record(
            tracker,
            candidates,
            RoleAssignmentCategory.ImpostorTeam,
            new[] { new RoleAssignmentSelection(0, 1, null) });

        tracker.AbortAssignment();
        tracker.CommitAssignment().Should().BeFalse();
        Begin(tracker, 1, 2);
        tracker.GetSnapshotWeight(1, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
        tracker.GetSnapshotWeight(2, RoleAssignmentCategory.ImpostorTeam, null).Should().Be(1);
    }

    [Fact]
    public void ImpostorSimulation_PreservesLongTermRateAndReducesImmediateRepeats()
    {
        // 10人から2人を選ぶゲームを10万回繰り返し、短期的な連投を抑えても
        // 各プレイヤーの長期当選率が本来の20%付近に残ることを確認する。
        const int games = 100_000;
        const int playerCount = 10;
        var tracker = new RoleAssignmentFairnessTracker(new SeededRandom(0x51A7));
        RoleAssignmentCandidate[] candidates = Candidates(Enumerable.Range(0, playerCount).Cast<int?>().ToArray());
        var roleSlots = new RoleId?[] { null, null };
        int[] wins = new int[playerCount];
        HashSet<int> previousWinners = new();
        int repeatedWinnerSlots = 0;

        for (int game = 0; game < games; game++)
        {
            Begin(tracker, Enumerable.Range(0, playerCount).ToArray());
            IReadOnlyList<RoleAssignmentSelection> selected = Select(
                tracker,
                candidates,
                RoleAssignmentCategory.ImpostorTeam,
                roleSlots);
            Record(tracker, candidates, RoleAssignmentCategory.ImpostorTeam, selected);
            tracker.CommitAssignment().Should().BeTrue();

            HashSet<int> currentWinners = selected.Select(value => value.ClientId!.Value).ToHashSet();
            foreach (int winner in currentWinners)
            {
                wins[winner]++;
                if (previousWinners.Contains(winner))
                    repeatedWinnerSlots++;
            }
            previousWinners = currentWinners;
        }

        foreach (int winCount in wins)
            ((double)winCount / games).Should().BeInRange(0.19, 0.21);

        double retentionRate = (double)repeatedWinnerSlots / ((games - 1) * 2);
        retentionRate.Should().BeLessThan(0.10);
    }

    [Fact]
    public void CustomRoleSimulation_PreservesLongTermRateAndReducesSameRoleRepeats()
    {
        // 10人から同じカスタム役職を1人へ配るゲームを10万回繰り返し、
        // 長期当選率10%を維持しながら同役職の連投率が下がることを確認する。
        const int games = 100_000;
        const int playerCount = 10;
        var tracker = new RoleAssignmentFairnessTracker(new SeededRandom(0xC0570));
        RoleAssignmentCandidate[] candidates = Candidates(Enumerable.Range(0, playerCount).Cast<int?>().ToArray());
        int[] wins = new int[playerCount];
        int? previousWinner = null;
        int repeatedWinnerGames = 0;

        for (int game = 0; game < games; game++)
        {
            Begin(tracker, Enumerable.Range(0, playerCount).ToArray());
            IReadOnlyList<RoleAssignmentSelection> selected = Select(
                tracker,
                candidates,
                RoleAssignmentCategory.CustomMainRole,
                RoleId.Sheriff);
            Record(tracker, candidates, RoleAssignmentCategory.CustomMainRole, selected);
            tracker.CommitAssignment().Should().BeTrue();

            int winner = selected[0].ClientId!.Value;
            wins[winner]++;
            if (previousWinner == winner)
                repeatedWinnerGames++;
            previousWinner = winner;
        }

        foreach (int winCount in wins)
            ((double)winCount / games).Should().BeInRange(0.09, 0.11);

        double repeatRate = (double)repeatedWinnerGames / (games - 1);
        repeatRate.Should().BeLessThan(0.03);
    }

    private static void MakeClientWinImpostorGame(
        RoleAssignmentFairnessTracker tracker,
        int winnerClientId,
        int loserClientId)
    {
        RoleAssignmentCandidate[] candidates = Candidates(winnerClientId, loserClientId);
        Begin(tracker, winnerClientId, loserClientId);
        Record(
            tracker,
            candidates,
            RoleAssignmentCategory.ImpostorTeam,
            new[] { new RoleAssignmentSelection(0, winnerClientId, null) });
        tracker.CommitAssignment().Should().BeTrue();
    }

    private static void Begin(RoleAssignmentFairnessTracker tracker, params int[] clientIds)
    {
        tracker.TryBeginAssignment(clientIds, out RoleAssignmentFairnessFailure failure)
            .Should().BeTrue($"assignment should begin, but failed with {failure}");
        failure.Should().Be(RoleAssignmentFairnessFailure.None);
    }

    private static IReadOnlyList<RoleAssignmentSelection> Select(
        RoleAssignmentFairnessTracker tracker,
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        RoleAssignmentCategory category,
        params RoleId?[] slotRoleIds)
    {
        tracker.TrySelectWeightedWithoutReplacement(
                candidates,
                category,
                slotRoleIds,
                out IReadOnlyList<RoleAssignmentSelection> selections,
                out RoleAssignmentFairnessFailure failure)
            .Should().BeTrue($"selection should succeed, but failed with {failure}");
        failure.Should().Be(RoleAssignmentFairnessFailure.None);
        return selections;
    }

    private static void Record(
        RoleAssignmentFairnessTracker tracker,
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        RoleAssignmentCategory category,
        IReadOnlyList<RoleAssignmentSelection> selections)
    {
        tracker.TryRecordPendingResult(candidates, category, selections, out RoleAssignmentFairnessFailure failure)
            .Should().BeTrue($"pending result should be recorded, but failed with {failure}");
        failure.Should().Be(RoleAssignmentFairnessFailure.None);
    }

    private static RoleAssignmentCandidate[] Candidates(params int?[] clientIds)
    {
        return clientIds
            .Select((clientId, index) => new RoleAssignmentCandidate(index, clientId))
            .ToArray();
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

        internal int Remaining => values.Count;
    }

    private sealed class SeededRandom : IRoleAssignmentRandom
    {
        private readonly Random random;

        internal SeededRandom(int seed)
        {
            random = new Random(seed);
        }

        public double NextDouble() => random.NextDouble();
    }
}
