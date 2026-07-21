#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SuperNewRoles.Roles;

internal enum RoleAssignmentCategory
{
    ImpostorTeam,
    CustomMainRole
}

internal interface IRoleAssignmentRandom
{
    /// <summary>0以上1未満の乱数を返します。</summary>
    double NextDouble();
}

internal sealed class CryptographicRoleAssignmentRandom : IRoleAssignmentRandom
{
    private const double Unit53 = 1.0 / (1UL << 53);

    public double NextDouble()
    {
        Span<byte> bytes = stackalloc byte[sizeof(ulong)];
        RandomNumberGenerator.Fill(bytes);
        ulong randomBits = BitConverter.ToUInt64(bytes);
        return (randomBits >> 11) * Unit53;
    }
}

/// <summary>
/// 呼び出し元が所有する候補リスト内の1要素を表します。
/// CandidateKeyは1回の抽選内で一意であればよく、ClientIdがnullの候補は
/// Dummyなど履歴を追跡できないプレイヤーとして扱います。
/// </summary>
internal readonly record struct RoleAssignmentCandidate(int CandidateKey, int? ClientId);

/// <summary>
/// 抽選された候補と、その枠で実際に付与する役職を表します。
/// ImpostorTeamではRoleIdをnullにし、CustomMainRoleではチーム役職の代表RoleIdではなく、
/// 各メンバーへ実際に付与したRoleIdを記録します。
/// </summary>
internal readonly record struct RoleAssignmentSelection(int CandidateKey, int? ClientId, RoleId? RoleId);

internal enum RoleAssignmentFairnessFailure
{
    None,
    Disabled,
    AssignmentNotStarted,
    AssignmentAlreadyStarted,
    InvalidActiveClient,
    DuplicateActiveClient,
    InvalidCategory,
    InvalidSlotRoles,
    InvalidCandidate,
    DuplicateCandidate,
    DuplicateClientId,
    CandidateNotActive,
    TooManySlots,
    InvalidWeight,
    InvalidRandomValue,
    SelectionNotEligible,
    DuplicateSelection,
    DuplicatePendingSelection
}

/// <summary>
/// 同一ロビー内だけで利用する役職配布履歴をメモリ上に保持します。
/// 永続的な個人識別情報は扱わず、Among Usのメインスレッドから呼び出すことを前提とします。
/// 1ゲーム中の抽選は開始時に固定したスナップショットだけを参照し、
/// 途中の当選結果が後続枠の重みに影響しないようにします。
/// </summary>
internal sealed class RoleAssignmentFairnessTracker
{
    internal const int MaximumMissedGames = 4;
    internal const double MinimumWeight = 0.25;
    internal const double MaximumWeight = 2.0;

    private readonly IRoleAssignmentRandom random;
    private Dictionary<HistoryKey, HistoryEntry> history = new();
    private Dictionary<HistoryKey, HistoryEntry> snapshot = new();
    private readonly HashSet<HistoryKey> pendingEligible = new();
    private readonly Dictionary<HistoryKey, RoleId?> pendingSelections = new();
    private HashSet<int> activeClientIds = new();

    internal RoleAssignmentFairnessTracker(IRoleAssignmentRandom? random = null, bool enabled = true)
    {
        this.random = random ?? new CryptographicRoleAssignmentRandom();
        IsEnabled = enabled;
    }

    internal bool IsEnabled { get; private set; }

    internal int? LobbyId { get; private set; }

    internal bool HasActiveAssignment { get; private set; }

    /// <summary>
    /// 機能の有効状態を切り替えます。
    /// ON/OFFのどちらへ切り替えた場合も履歴を全消去し、再度ONにした際に
    /// 過去の設定状態で収集した重みが復活しないようにします。
    /// </summary>
    internal bool SetEnabled(bool enabled)
    {
        if (IsEnabled == enabled)
            return false;

        IsEnabled = enabled;
        Reset();
        return true;
    }

    /// <summary>
    /// 同じロビーIDなら再戦用の履歴を維持し、別ロビーIDへ変わった場合は全消去します。
    /// </summary>
    internal bool SetLobby(int lobbyId)
    {
        if (LobbyId == lobbyId)
            return false;

        Reset();
        LobbyId = lobbyId;
        return true;
    }

    /// <summary>切断時にロビーIDと配役履歴をまとめて消去します。</summary>
    internal void LeaveLobby()
    {
        Reset();
        LobbyId = null;
    }

    /// <summary>
    /// 現在のロビーIDと設定の有効状態を残したまま、配役履歴だけを消去します。
    /// ホスト移譲、モード変更、異常フォールバックなど、現在の履歴を
    /// 次のゲームへ利用できない状態になった場合に使用します。
    /// </summary>
    internal void Reset()
    {
        history.Clear();
        ClearActiveAssignment();
    }

    /// <summary>
    /// 退出したClientIdを確定履歴・スナップショット・保留結果の全層から即座に削除します。
    /// 同じ人物が再接続しても、新規参加者と同じ基礎重み1.0から始まります。
    /// </summary>
    internal void RemoveClient(int clientId)
    {
        if (clientId < 0)
            return;

        RemoveClientFrom(history, clientId);
        RemoveClientFrom(snapshot, clientId);
        RemoveClientFrom(pendingEligible, clientId);
        RemoveClientFrom(pendingSelections, clientId);
        activeClientIds.Remove(clientId);
    }

    /// <summary>
    /// 今回の全抽選で参照する前ゲーム履歴を固定します。
    /// activeClientIdsには、現在解決できる全ClientIdを重複なしで渡す必要があります。
    /// この時点で退出済みClientIdの履歴も除去します。
    /// </summary>
    internal bool TryBeginAssignment(
        IEnumerable<int> activeClientIds,
        out RoleAssignmentFairnessFailure failure)
    {
        failure = RoleAssignmentFairnessFailure.None;
        if (!IsEnabled)
        {
            failure = RoleAssignmentFairnessFailure.Disabled;
            return false;
        }

        if (HasActiveAssignment)
        {
            failure = RoleAssignmentFairnessFailure.AssignmentAlreadyStarted;
            return false;
        }

        if (activeClientIds is null)
        {
            failure = RoleAssignmentFairnessFailure.InvalidActiveClient;
            return false;
        }

        HashSet<int> clients = new();
        foreach (int clientId in activeClientIds)
        {
            if (clientId < 0)
            {
                failure = RoleAssignmentFairnessFailure.InvalidActiveClient;
                return false;
            }

            if (!clients.Add(clientId))
            {
                failure = RoleAssignmentFairnessFailure.DuplicateActiveClient;
                return false;
            }
        }

        // ロビーに残っているClientIdだけへ履歴を絞り、今回専用の読み取りスナップショットを作る。
        // 以降の結果記録は保留領域だけを更新するため、同じゲーム中の重みは変化しない。
        history = history
            .Where(pair => clients.Contains(pair.Key.ClientId))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        this.activeClientIds = clients;
        snapshot = new Dictionary<HistoryKey, HistoryEntry>(history);
        pendingEligible.Clear();
        pendingSelections.Clear();
        HasActiveAssignment = true;
        return true;
    }

    /// <summary>確定済み履歴は残し、今回のスナップショットと保留結果だけを破棄します。</summary>
    internal void AbortAssignment() => ClearActiveAssignment();

    /// <summary>
    /// 今回収集した候補機会と当選結果を、全配役完了後に1回だけ確定します。
    /// 同じClientIdが複数の役職候補リストに現れても、未当選試合数はカテゴリごとに
    /// 1ゲームにつき最大1回だけ増加します。
    /// </summary>
    internal bool CommitAssignment()
    {
        if (!HasActiveAssignment)
            return false;

        HashSet<HistoryKey> keys = snapshot.Keys.ToHashSet();
        keys.UnionWith(pendingEligible);
        keys.UnionWith(pendingSelections.Keys);

        Dictionary<HistoryKey, HistoryEntry> nextHistory = new();
        foreach (HistoryKey key in keys)
        {
            if (!activeClientIds.Contains(key.ClientId))
                continue;

            snapshot.TryGetValue(key, out HistoryEntry previous);
            bool wasEligible = pendingEligible.Contains(key);
            bool wasSelected = pendingSelections.TryGetValue(key, out RoleId? selectedRoleId);

            // 候補機会がなかったゲームでは未当選数を変えない。
            // 候補だった場合は、当選で0へ戻し、未当選なら上限4まで1だけ増やす。
            int missedGames = previous.MissedGames;
            if (wasEligible)
            {
                missedGames = wasSelected
                    ? 0
                    : Math.Min(MaximumMissedGames, previous.MissedGames + 1);
            }

            // 「直前のゲームで当選したか」と「直前に付与されたカスタムRoleId」は
            // 次ゲームのペナルティ計算にだけ利用する。
            RoleId? previousRoleId = wasSelected && key.Category == RoleAssignmentCategory.CustomMainRole
                ? selectedRoleId
                : null;
            nextHistory[key] = new HistoryEntry(missedGames, wasSelected, previousRoleId);
        }

        history = nextHistory;
        ClearActiveAssignment();
        return true;
    }

    /// <summary>
    /// 配役中は固定スナップショット、それ以外では最新の確定履歴から重みを返します。
    /// ClientIdを解決できない候補は履歴を持たず、常に基礎重み1.0です。
    /// </summary>
    internal double GetSnapshotWeight(
        int? clientId,
        RoleAssignmentCategory category,
        RoleId? roleId)
    {
        ValidateCategory(category);
        if (!clientId.HasValue)
            return 1.0;
        if (clientId.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        Dictionary<HistoryKey, HistoryEntry> source = HasActiveAssignment ? snapshot : history;
        source.TryGetValue(new HistoryKey(clientId.Value, category), out HistoryEntry entry);

        // 未当選が続いた候補は1ゲームごとに0.25加算し、4ゲーム・重み2.0で打ち止める。
        double weight = Math.Min(MaximumWeight, 1.0 + (0.25 * Math.Min(MaximumMissedGames, entry.MissedGames)));
        // 同じカテゴリへ前ゲームで当選していた場合は半減する。
        if (entry.WasSelectedPreviousGame)
            weight *= 0.5;
        // カスタム主要役職では、さらに同じRoleIdの連投を半減する。
        if (category == RoleAssignmentCategory.CustomMainRole
            && roleId.HasValue
            && entry.PreviousRoleId == roleId)
        {
            weight *= 0.5;
        }

        // 最小0.25を保証し、連投を禁止せず全候補の当選可能性を残す。
        return Math.Clamp(weight, MinimumWeight, MaximumWeight);
    }

    /// <summary>
    /// 固定スナップショットの重みを使い、役職枠ごとに重み付き非復元抽選を行います。
    /// 戻り値はslotRoleIdsの順序を維持します。このメソッドだけでは履歴を変更せず、
    /// 呼び出し元が結果を採用すると決めた後にTryRecordPendingResultを呼び出します。
    /// </summary>
    internal bool TrySelectWeightedWithoutReplacement(
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        RoleAssignmentCategory category,
        IReadOnlyList<RoleId?> slotRoleIds,
        out IReadOnlyList<RoleAssignmentSelection> selections,
        out RoleAssignmentFairnessFailure failure)
    {
        selections = Array.Empty<RoleAssignmentSelection>();
        if (!TryValidateSelectionContext(category, out failure))
            return false;
        if (candidates is null || slotRoleIds is null)
        {
            failure = RoleAssignmentFairnessFailure.InvalidCandidate;
            return false;
        }
        if (!TryValidateSlotRoles(category, slotRoleIds, out failure))
            return false;
        if (slotRoleIds.Count == 0)
            return true;
        if (slotRoleIds.Count > candidates.Count)
        {
            failure = RoleAssignmentFairnessFailure.TooManySlots;
            return false;
        }
        if (!TryValidateCandidates(candidates, out failure))
            return false;
        if (ContainsPendingSelection(candidates, category))
        {
            failure = RoleAssignmentFairnessFailure.DuplicatePendingSelection;
            return false;
        }

        // 呼び出し元の候補リストは変更せず、作業用コピーから当選者を1人ずつ除去する。
        List<RoleAssignmentCandidate> remaining = candidates.ToList();
        List<RoleAssignmentSelection> selected = new(slotRoleIds.Count);

        foreach (RoleId? slotRoleId in slotRoleIds)
        {
            double totalWeight = 0;
            double[] weights = new double[remaining.Count];
            for (int index = 0; index < remaining.Count; index++)
            {
                double weight = GetSnapshotWeight(remaining[index].ClientId, category, slotRoleId);
                if (!double.IsFinite(weight) || weight <= 0)
                {
                    failure = RoleAssignmentFairnessFailure.InvalidWeight;
                    return false;
                }

                weights[index] = weight;
                totalWeight += weight;
            }

            if (!double.IsFinite(totalWeight) || totalWeight <= 0)
            {
                failure = RoleAssignmentFairnessFailure.InvalidWeight;
                return false;
            }

            double unitValue = random.NextDouble();
            if (!double.IsFinite(unitValue) || unitValue < 0 || unitValue >= 1)
            {
                failure = RoleAssignmentFairnessFailure.InvalidRandomValue;
                return false;
            }

            // [0, 合計重み)上の位置へ乱数を写像し、累積重みが初めて上回る候補を選ぶ。
            double threshold = unitValue * totalWeight;
            int selectedIndex = remaining.Count - 1;
            double cumulativeWeight = 0;
            for (int index = 0; index < remaining.Count; index++)
            {
                cumulativeWeight += weights[index];
                if (threshold < cumulativeWeight)
                {
                    selectedIndex = index;
                    break;
                }
            }

            RoleAssignmentCandidate selectedCandidate = remaining[selectedIndex];
            selected.Add(new RoleAssignmentSelection(
                selectedCandidate.CandidateKey,
                selectedCandidate.ClientId,
                slotRoleId));
            remaining.RemoveAt(selectedIndex);
        }

        selections = selected;
        failure = RoleAssignmentFairnessFailure.None;
        return true;
    }

    /// <summary>
    /// 採用された候補機会と当選結果を、確定前の保留領域へ原子的に記録します。
    /// ClientId不明の候補は意図的に履歴対象外とします。複数のカスタム役職抽選から
    /// 同じ候補が渡された場合、候補機会は和集合として1回分だけ保持します。
    /// </summary>
    internal bool TryRecordPendingResult(
        IReadOnlyList<RoleAssignmentCandidate> eligibleCandidates,
        RoleAssignmentCategory category,
        IReadOnlyList<RoleAssignmentSelection> selections,
        out RoleAssignmentFairnessFailure failure)
    {
        if (!TryValidateSelectionContext(category, out failure))
            return false;
        if (eligibleCandidates is null || selections is null)
        {
            failure = RoleAssignmentFairnessFailure.InvalidCandidate;
            return false;
        }
        if (!TryValidateCandidates(eligibleCandidates, out failure))
            return false;

        Dictionary<int, RoleAssignmentCandidate> eligibleByKey = eligibleCandidates
            .ToDictionary(candidate => candidate.CandidateKey);
        HashSet<int> selectionKeys = new();
        HashSet<int> selectedClientIds = new();
        List<KeyValuePair<HistoryKey, RoleId?>> winners = new();

        // 先に全選択を検証し、1件でも不正なら保留領域へ何も書き込まない。
        foreach (RoleAssignmentSelection selection in selections)
        {
            if (!selectionKeys.Add(selection.CandidateKey))
            {
                failure = RoleAssignmentFairnessFailure.DuplicateSelection;
                return false;
            }
            if (!eligibleByKey.TryGetValue(selection.CandidateKey, out RoleAssignmentCandidate eligible)
                || eligible.ClientId != selection.ClientId)
            {
                failure = RoleAssignmentFairnessFailure.SelectionNotEligible;
                return false;
            }
            if (!IsRoleValidForCategory(category, selection.RoleId))
            {
                failure = RoleAssignmentFairnessFailure.InvalidSlotRoles;
                return false;
            }

            if (!selection.ClientId.HasValue)
                continue;
            if (!selectedClientIds.Add(selection.ClientId.Value))
            {
                failure = RoleAssignmentFairnessFailure.DuplicateClientId;
                return false;
            }

            HistoryKey key = new(selection.ClientId.Value, category);
            if (pendingSelections.ContainsKey(key))
            {
                failure = RoleAssignmentFairnessFailure.DuplicatePendingSelection;
                return false;
            }
            winners.Add(new KeyValuePair<HistoryKey, RoleId?>(key, selection.RoleId));
        }

        // 全件の検証に成功した後で初めて保留状態を更新する。
        foreach (RoleAssignmentCandidate candidate in eligibleCandidates)
        {
            if (candidate.ClientId.HasValue)
                pendingEligible.Add(new HistoryKey(candidate.ClientId.Value, category));
        }
        foreach (KeyValuePair<HistoryKey, RoleId?> winner in winners)
            pendingSelections.Add(winner.Key, winner.Value);

        failure = RoleAssignmentFairnessFailure.None;
        return true;
    }

    private bool TryValidateSelectionContext(
        RoleAssignmentCategory category,
        out RoleAssignmentFairnessFailure failure)
    {
        if (!IsEnabled)
        {
            failure = RoleAssignmentFairnessFailure.Disabled;
            return false;
        }
        if (!HasActiveAssignment)
        {
            failure = RoleAssignmentFairnessFailure.AssignmentNotStarted;
            return false;
        }
        if (!Enum.IsDefined(typeof(RoleAssignmentCategory), category))
        {
            failure = RoleAssignmentFairnessFailure.InvalidCategory;
            return false;
        }

        failure = RoleAssignmentFairnessFailure.None;
        return true;
    }

    private bool TryValidateCandidates(
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        out RoleAssignmentFairnessFailure failure)
    {
        HashSet<int> candidateKeys = new();
        HashSet<int> clientIds = new();
        foreach (RoleAssignmentCandidate candidate in candidates)
        {
            if (candidate.CandidateKey < 0
                || (candidate.ClientId.HasValue && candidate.ClientId.Value < 0))
            {
                failure = RoleAssignmentFairnessFailure.InvalidCandidate;
                return false;
            }
            if (!candidateKeys.Add(candidate.CandidateKey))
            {
                failure = RoleAssignmentFairnessFailure.DuplicateCandidate;
                return false;
            }
            if (!candidate.ClientId.HasValue)
                continue;
            if (!activeClientIds.Contains(candidate.ClientId.Value))
            {
                failure = RoleAssignmentFairnessFailure.CandidateNotActive;
                return false;
            }
            if (!clientIds.Add(candidate.ClientId.Value))
            {
                failure = RoleAssignmentFairnessFailure.DuplicateClientId;
                return false;
            }
        }

        failure = RoleAssignmentFairnessFailure.None;
        return true;
    }

    private static bool TryValidateSlotRoles(
        RoleAssignmentCategory category,
        IReadOnlyList<RoleId?> slotRoleIds,
        out RoleAssignmentFairnessFailure failure)
    {
        foreach (RoleId? roleId in slotRoleIds)
        {
            if (!IsRoleValidForCategory(category, roleId))
            {
                failure = RoleAssignmentFairnessFailure.InvalidSlotRoles;
                return false;
            }
        }

        failure = RoleAssignmentFairnessFailure.None;
        return true;
    }

    private static bool IsRoleValidForCategory(RoleAssignmentCategory category, RoleId? roleId)
    {
        return category switch
        {
            RoleAssignmentCategory.ImpostorTeam => !roleId.HasValue,
            RoleAssignmentCategory.CustomMainRole => roleId.HasValue && roleId.Value != RoleId.None,
            _ => false
        };
    }

    private static void ValidateCategory(RoleAssignmentCategory category)
    {
        if (!Enum.IsDefined(typeof(RoleAssignmentCategory), category))
            throw new ArgumentOutOfRangeException(nameof(category));
    }

    private bool ContainsPendingSelection(
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        RoleAssignmentCategory category)
    {
        return candidates.Any(candidate => candidate.ClientId.HasValue
            && pendingSelections.ContainsKey(new HistoryKey(candidate.ClientId.Value, category)));
    }

    private void ClearActiveAssignment()
    {
        snapshot.Clear();
        pendingEligible.Clear();
        pendingSelections.Clear();
        activeClientIds.Clear();
        HasActiveAssignment = false;
    }

    private static void RemoveClientFrom<TValue>(Dictionary<HistoryKey, TValue> source, int clientId)
    {
        foreach (HistoryKey key in source.Keys.Where(key => key.ClientId == clientId).ToArray())
            source.Remove(key);
    }

    private static void RemoveClientFrom(HashSet<HistoryKey> source, int clientId)
    {
        source.RemoveWhere(key => key.ClientId == clientId);
    }

    private readonly record struct HistoryKey(int ClientId, RoleAssignmentCategory Category);

    private readonly record struct HistoryEntry(
        int MissedGames,
        bool WasSelectedPreviousGame,
        RoleId? PreviousRoleId);
}
