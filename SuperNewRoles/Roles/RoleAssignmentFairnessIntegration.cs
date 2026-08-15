using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Mode;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles;

/// <summary>
/// Among Us の一時 ClientId と公平化トラッカーをつなぐホスト専用ランタイム。
/// 個人を永続的に識別できる値は保持しない。
/// </summary>
internal static class RoleAssignmentFairnessRuntime
{
    private const string LogSource = "RoleFairness";

    private static readonly RoleAssignmentFairnessTracker Tracker = new();

    private static int? observedHostId;
    private static bool fairnessActiveForAssignment;
    private static bool fairnessStartedForAssignment;

    internal static bool IsActive => fairnessActiveForAssignment && Tracker.HasActiveAssignment;
    internal static bool MustStopAfterRoleRpcFailure => fairnessStartedForAssignment;

    internal static bool TryBeginAssignment()
    {
        // 前回の配役が例外で中断していても、ゲーム境界をまたいで実行中フラグを残さない。
        fairnessActiveForAssignment = false;
        fairnessStartedForAssignment = false;

        try
        {
            if (!TryGetSupportedContext(out var client, out int lobbyId, out int hostId))
            {
                Tracker.SetEnabled(false);
                return false;
            }

            Tracker.SetEnabled(true);
            // 対応コンテキストで公平化を試みた時点から、異常時に従来抽選へ戻った後も
            // チーム役職の部分的なRPCを同じ配役内で再試行させない。
            fairnessStartedForAssignment = true;

            if (observedHostId.HasValue && observedHostId.Value != hostId)
            {
                Tracker.Reset();
            }
            observedHostId = hostId;
            Tracker.SetLobby(lobbyId);

            if (Tracker.HasActiveAssignment)
            {
                AbortWithFallback(RoleAssignmentFairnessFailure.AssignmentAlreadyStarted, null);
                return false;
            }

            // ClientIdを解決できる接続中プレイヤーだけを履歴対象にする。
            // Dummyはここに含まれなくても、後続の候補生成ではClientId=nullの候補として残る。
            var activeClientIds = client.allClients.ToArray()
                .Where(data => data != null && data.Id >= 0 && data.Character != null &&
                               data.Character.Data != null && !data.Character.Data.Disconnected)
                .Select(data => data.Id)
                .ToArray();

            if (!Tracker.TryBeginAssignment(activeClientIds, out var failure))
            {
                AbortWithFallback(failure, null);
                return false;
            }

            fairnessActiveForAssignment = true;
            fairnessStartedForAssignment = true;
            return true;
        }
        catch (Exception ex)
        {
            // ゲーム側コレクションの列挙を含む開始処理で例外が起きても、
            // 配役全体へ伝播させず、履歴を破棄して従来抽選へ安全に降格する。
            AbortWithFallback($"TryBeginAssignmentException:{ex.GetType().Name}", null);
            return false;
        }
    }

    internal static bool TrySelectImpostors(
        IReadOnlyList<NetworkedPlayerInfo> players,
        int count,
        out List<NetworkedPlayerInfo> selectedPlayers,
        out PendingFairnessResult pendingResult)
    {
        selectedPlayers = null;
        pendingResult = null;

        if (!IsActive)
            return false;

        // DebugRoleAssignments適用後の候補だけを公平抽選し、選んだ部分集合をバニラ処理へ渡す。
        var candidates = BuildCandidates(players, ResolveClientId);
        if (candidates == null)
        {
            AbortWithFallback(RoleAssignmentFairnessFailure.InvalidCandidate, RoleAssignmentCategory.ImpostorTeam);
            return false;
        }

        var slotRoles = Enumerable.Repeat<RoleId?>(null, Math.Max(0, count)).ToArray();
        if (!TrySelect(candidates, RoleAssignmentCategory.ImpostorTeam, slotRoles, out var selections))
            return false;

        selectedPlayers = selections
            .Select(selection => players[selection.CandidateKey])
            .ToList();
        pendingResult = new PendingFairnessResult(candidates, RoleAssignmentCategory.ImpostorTeam, selections);
        return true;
    }

    internal static bool TrySelectCustomRolePlayer(
        IReadOnlyList<PlayerControl> players,
        RoleId roleId,
        out int selectedIndex,
        out PendingFairnessResult pendingResult)
    {
        selectedIndex = -1;
        pendingResult = null;

        if (!IsActive)
            return false;

        // 役職チケットや出現確率には触れず、既に確定した1枠の受取人だけを選ぶ。
        var candidates = BuildCandidates(players, ResolveClientId);
        if (candidates == null)
        {
            AbortWithFallback(RoleAssignmentFairnessFailure.InvalidCandidate, RoleAssignmentCategory.CustomMainRole);
            return false;
        }

        if (!TrySelect(candidates, RoleAssignmentCategory.CustomMainRole, new RoleId?[] { roleId }, out var selections))
            return false;

        selectedIndex = selections[0].CandidateKey;
        pendingResult = new PendingFairnessResult(candidates, RoleAssignmentCategory.CustomMainRole, selections);
        return true;
    }

    internal static bool TrySelectTeamRolePlayers(
        IReadOnlyList<PlayerControl> players,
        ITeamRoleBase teamRole,
        out List<PlayerControl> selectedPlayers,
        out PendingFairnessResult pendingResult)
    {
        selectedPlayers = null;
        pendingResult = null;

        if (!IsActive)
            return false;

        var candidates = BuildCandidates(players, ResolveClientId);
        if (candidates == null)
        {
            AbortWithFallback(RoleAssignmentFairnessFailure.InvalidCandidate, RoleAssignmentCategory.CustomMainRole);
            return false;
        }

        // MemberRoleIdsが1件なら全メンバーへ同じ役職を付与し、
        // TeamSize件なら定義順をそのまま各抽選枠へ対応させる。
        RoleId?[] slotRoles;
        if (teamRole.MemberRoleIds == null || teamRole.MemberRoleIds.Count == 0)
        {
            AbortWithFallback(RoleAssignmentFairnessFailure.InvalidSlotRoles, RoleAssignmentCategory.CustomMainRole);
            return false;
        }
        if (teamRole.MemberRoleIds.Count == 1)
        {
            slotRoles = Enumerable.Repeat<RoleId?>(teamRole.MemberRoleIds[0], teamRole.TeamSize).ToArray();
        }
        else if (teamRole.MemberRoleIds.Count == teamRole.TeamSize)
        {
            slotRoles = teamRole.MemberRoleIds.Select(roleId => (RoleId?)roleId).ToArray();
        }
        else
        {
            AbortWithFallback(RoleAssignmentFairnessFailure.InvalidSlotRoles, RoleAssignmentCategory.CustomMainRole);
            return false;
        }

        if (!TrySelect(candidates, RoleAssignmentCategory.CustomMainRole, slotRoles, out var selections))
            return false;

        selectedPlayers = selections
            .Select(selection => players[selection.CandidateKey])
            .ToList();
        pendingResult = new PendingFairnessResult(candidates, RoleAssignmentCategory.CustomMainRole, selections);
        return true;
    }

    internal static bool Record(PendingFairnessResult pendingResult)
    {
        if (!IsActive || pendingResult == null)
            return false;

        if (Tracker.TryRecordPendingResult(
                pendingResult.EligibleCandidates,
                pendingResult.Category,
                pendingResult.Selections,
                out var failure))
        {
            return true;
        }

        AbortWithFallback(failure, pendingResult.Category);
        return false;
    }

    internal static void FinishAssignment(bool customAssignmentSucceeded)
    {
        if (!fairnessStartedForAssignment)
            return;

        try
        {
            if (!fairnessActiveForAssignment)
                return;

            // カスタム配役まで完走した場合だけ、今回収集した全カテゴリの結果を一括確定する。
            // 途中失敗時は不完全なゲームを履歴へ混ぜない。
            if (!customAssignmentSucceeded || !Tracker.HasActiveAssignment)
            {
                AbortAssignment(
                    customAssignmentSucceeded
                        ? RoleAssignmentFairnessFailure.AssignmentNotStarted
                        : "CustomAssignmentFailed",
                    null);
                return;
            }

            if (!Tracker.CommitAssignment())
            {
                AbortAssignment("CommitFailed", null);
            }
        }
        finally
        {
            fairnessActiveForAssignment = false;
            fairnessStartedForAssignment = false;
        }
    }

    internal static void AbortAssignment(object fallbackReason, RoleAssignmentCategory? category)
    {
        LogFallback(fallbackReason, category);
        // このゲームは従来抽選または部分配役で進む可能性があり、完全な結果を追跡できない。
        // 二戦前の履歴を「前ゲーム」として誤用しないよう、ロビー履歴ごと破棄する。
        Tracker.Reset();
        fairnessActiveForAssignment = false;
    }

    internal static void EnterLobby(int lobbyId)
    {
        bool lobbyChanged = Tracker.SetLobby(lobbyId);
        // Play Again は同じ GameId を再利用するため、同一ロビーの履歴は保持する。
        // ただし未完了の配役処理があれば、その保留状態だけは持ち越さない。
        Tracker.AbortAssignment();
        fairnessActiveForAssignment = false;
        fairnessStartedForAssignment = false;
        if (lobbyChanged)
            observedHostId = null;
    }

    internal static void LeaveLobby()
    {
        // 切断後に同じGameIdが再利用されても、以前のロビー履歴を復元しない。
        Tracker.LeaveLobby();
        fairnessActiveForAssignment = false;
        fairnessStartedForAssignment = false;
        observedHostId = null;
    }

    internal static void Reset()
    {
        // ロビーID自体は維持しつつ、現在の文脈では信頼できなくなった履歴を破棄する。
        Tracker.Reset();
        fairnessActiveForAssignment = false;
        fairnessStartedForAssignment = false;
        observedHostId = null;
    }

    internal static void SetEnabled(bool enabled)
    {
        // 設定切替時はトラッカー側が履歴を消去する。
        // 同じ値が再設定されただけなら、同一ロビー再戦の履歴は維持する。
        if (Tracker.SetEnabled(enabled))
        {
            fairnessActiveForAssignment = false;
            fairnessStartedForAssignment = false;
        }
    }

    internal static void RemoveClient(int clientId)
    {
        // 負のClientIdは解決失敗値なので、履歴削除対象として扱わない。
        if (clientId >= 0)
            Tracker.RemoveClient(clientId);
    }

    private static bool TrySelect(
        IReadOnlyList<RoleAssignmentCandidate> candidates,
        RoleAssignmentCategory category,
        IReadOnlyList<RoleId?> slotRoles,
        out IReadOnlyList<RoleAssignmentSelection> selections)
    {
        selections = null;

        // デバッグログには一時ClientId・カテゴリ・重みだけを記録し、
        // 表示名、色、フレンドコードなど個人を識別し得る情報は出力しない。
        foreach (var roleId in slotRoles.Count == 0 ? new RoleId?[] { null } : slotRoles)
        {
            foreach (var candidate in candidates)
            {
                double weight = Tracker.GetSnapshotWeight(candidate.ClientId, category, roleId);
                Logger.Debug(
                    $"ClientId={FormatClientId(candidate.ClientId)}, Category={category}, Weight={weight:0.###}",
                    LogSource);
            }
        }

        if (Tracker.TrySelectWeightedWithoutReplacement(candidates, category, slotRoles, out selections, out var failure))
            return true;

        AbortWithFallback(failure, category);
        return false;
    }

    private static List<RoleAssignmentCandidate> BuildCandidates<T>(
        IReadOnlyList<T> values,
        Func<T, int?> clientIdResolver)
        where T : class
    {
        if (values == null)
            return null;

        // 同じプレイヤーオブジェクトが重複している場合は、抽選前に異常として扱う。
        // 呼び出し元のリスト順をCandidateKeyへ保存し、結果を元の型へ安全に戻せるようにする。
        var candidates = new List<RoleAssignmentCandidate>(values.Count);
        var seenValues = new HashSet<T>();
        for (int index = 0; index < values.Count; index++)
        {
            T value = values[index];
            if (value is null || !seenValues.Add(value))
                return null;

            candidates.Add(new RoleAssignmentCandidate(index, clientIdResolver(value)));
        }
        return candidates;
    }

    private static int? ResolveClientId(NetworkedPlayerInfo player)
    {
        try
        {
            int id = AmongUsClient.Instance?.GetClientFromPlayerInfo(player)?.Id ?? -1;
            return id >= 0 ? id : null;
        }
        catch
        {
            return null;
        }
    }

    private static int? ResolveClientId(PlayerControl player)
    {
        try
        {
            int id = AmongUsClient.Instance?.GetClientFromCharacter(player)?.Id ?? -1;
            return id >= 0 ? id : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetSupportedContext(out AmongUsClient client, out int lobbyId, out int hostId)
    {
        client = AmongUsClient.Instance;
        lobbyId = 0;
        hostId = 0;

        try
        {
            if (client == null || !client.AmHost || client.NetworkMode == NetworkModes.FreePlay ||
                !GeneralSettingOptions.ReduceRoleAssignmentBias || Categories.ModeOption != ModeId.Default ||
                GameOptionsManager.Instance?.CurrentGameOptions?.GameMode != GameModes.Normal)
            {
                return false;
            }

            lobbyId = client.GameId;
            hostId = client.HostId;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void AbortWithFallback(object failure, RoleAssignmentCategory? category)
    {
        AbortAssignment(failure, category);
    }

    private static void LogFallback(object reason, RoleAssignmentCategory? category)
    {
        Logger.Debug(
            $"Category={(category?.ToString() ?? "None")}, FallbackReason={reason}",
            LogSource);
    }

    private static string FormatClientId(int? clientId) => clientId?.ToString() ?? "Unresolved";

    internal sealed class PendingFairnessResult
    {
        internal PendingFairnessResult(
            IReadOnlyList<RoleAssignmentCandidate> eligibleCandidates,
            RoleAssignmentCategory category,
            IReadOnlyList<RoleAssignmentSelection> selections)
        {
            EligibleCandidates = eligibleCandidates;
            Category = category;
            Selections = selections;
        }

        internal IReadOnlyList<RoleAssignmentCandidate> EligibleCandidates { get; }
        internal RoleAssignmentCategory Category { get; }
        internal IReadOnlyList<RoleAssignmentSelection> Selections { get; }
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnDisconnected))]
internal static class RoleAssignmentFairnessDisconnectedPatch
{
    // ロビー切断時はGameIdを含む全状態を破棄する。
    private static void Prefix()
        => RoleAssignmentFairnessRuntime.LeaveLobby();
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnBecomeHost))]
internal static class RoleAssignmentFairnessBecomeHostPatch
{
    // ホスト移譲で新ホストになった端末には、旧ホストの確定履歴が存在しないため基礎状態へ戻す。
    private static void Prefix()
        => RoleAssignmentFairnessRuntime.Reset();
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
internal static class RoleAssignmentFairnessPlayerLeftPatch
{
    // 退出通知を受けた時点でClientIdの履歴を消し、同じ配役中の候補からも除外する。
    private static void Prefix([HarmonyArgument(0)] ClientData data)
    {
        if (data != null)
            RoleAssignmentFairnessRuntime.RemoveClient(data.Id);
    }
}

[HarmonyPatch(typeof(CustomOption), nameof(CustomOption.UpdateSelection))]
internal static class RoleAssignmentFairnessOptionChangedPatch
{
    private readonly record struct OptionState(string FieldName, byte Selection);

    private static void Prefix(CustomOption __instance, out OptionState __state)
    {
        // 更新前後を比較するため、公平化に関係する設定だけを一時保存する。
        string fieldName = __instance?.FieldInfo?.Name;
        bool isRelevant = fieldName is nameof(GeneralSettingOptions.ReduceRoleAssignmentBias) or nameof(Categories.ModeOption);
        __state = new OptionState(isRelevant ? fieldName : null, isRelevant ? __instance.Selection : (byte)0);
    }

    private static void Postfix(CustomOption __instance, OptionState __state)
    {
        // 同じ値の再同期では履歴を消さず、実際に設定が変わった場合だけリセットする。
        if (__state.FieldName == null || __state.Selection == __instance.Selection)
            return;

        if (__state.FieldName == nameof(GeneralSettingOptions.ReduceRoleAssignmentBias))
            RoleAssignmentFairnessRuntime.SetEnabled(GeneralSettingOptions.ReduceRoleAssignmentBias);
        else
            RoleAssignmentFairnessRuntime.Reset();
    }
}

[HarmonyPatch(typeof(GameOptionsManager), nameof(GameOptionsManager.SwitchGameMode))]
internal static class RoleAssignmentFairnessGameModeChangedPatch
{
    // Among Us側のゲームモードを直前値と比較するための、プロセス内だけの観測値。
    private static GameModes? observedGameMode;

    private static void Prefix([HarmonyArgument(0)] GameModes gameMode)
    {
        // CurrentGameOptionsの取得処理はSwitchGameModeを呼ぶ場合があるため、
        // このパッチ内ではCurrentGameOptionsを取得せず、呼び出し引数だけを観測する。
        // これによりSwitchGameModeの無限再入を防ぐ。
        if (observedGameMode.HasValue && observedGameMode.Value != gameMode)
            RoleAssignmentFairnessRuntime.Reset();
        observedGameMode = gameMode;
    }
}
