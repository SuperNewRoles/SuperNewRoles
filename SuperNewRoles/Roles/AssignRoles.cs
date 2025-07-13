using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Modifiers;
using SuperNewRoles.Roles.Neutral;

namespace SuperNewRoles.Roles;

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
class GameStartManagerStartPatch
{
    public static void Postfix()
    {
        AssignRoles.LoversIndex = 0;
    }
}
[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class RoleManagerSelectRolesPatch
{
    public static bool Prefix(RoleManager __instance)
    {
        var list = AmongUsClient.Instance.allClients.ToArray();
        var list2 = (from c in list
                     where c.Character != null
                     where c.Character.Data != null
                     where !c.Character.Data.Disconnected && !c.Character.Data.IsDead
                     orderby c.Id
                     select c.Character.Data).ToList();
        foreach (NetworkedPlayerInfo allPlayer in GameData.Instance.AllPlayers)
        {
            if (allPlayer.Object != null && allPlayer.Object.isDummy)
            {
                list2.Add(allPlayer);
            }
        }
        IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        // バニラとの変更点ここ
        // インポスターの数をそのまま使ってバリデーションを防いでる
        int numImpostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        __instance.DebugRoleAssignments(list2.ToIl2CppList(), ref numImpostors);
        GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(list2.ToIl2CppList(), currentGameOptions, RoleTeamTypes.Impostor, numImpostors, new Il2CppSystem.Nullable<RoleTypes>(RoleTypes.Impostor));
        GameManager.Instance.LogicRoleSelection.AssignRolesForTeam(list2.ToIl2CppList(), currentGameOptions, RoleTeamTypes.Crewmate, int.MaxValue, new Il2CppSystem.Nullable<RoleTypes>(RoleTypes.Crewmate));
        return false;
    }
    public static void Postfix(RoleManager __instance)
    {
        AssignRoles.AssignCustomRoles();
    }
}

public static class AssignRoles
{
    [CustomOptionInt("AssignRoles_MaxImpostors", 0, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
    public static int MaxImpostors;
    [CustomOptionInt("AssignRoles_MaxCrews", 0, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
    public static int MaxCrews;
    [CustomOptionInt("AssignRoles_MaxNeutrals", 0, 15, 1, 2, parentFieldName: nameof(Categories.GeneralSettings))]
    public static int MaxNeutrals;

    public static byte LoversIndex = 0;

    private static Dictionary<AssignedTeamType, List<AssignTickets>> AssignTickets_HundredPercent = new();
    private static Dictionary<AssignedTeamType, List<AssignTickets>> AssignTickets_NotHundredPercent = new();
    private static List<RoleId> AssignedRoleIds = new(); // 既にアサインされた役職のIDを追跡

    public static void AssignCustomRoles()
    {
        Logger.Info("AssignCustomRoles() 開始: カスタム役職のアサイン処理を開始します。");
        CreateTickets();
        AssignedRoleIds.Clear(); // 役職アサイン前にクリア

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (!player.Data.Role.IsSimpleRole)
                AssignRole(player, player.Data.Role.IsImpostor ? RoleId.Impostor : RoleId.Crewmate);
        }

        // Assign Impostors
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Impostor],
        AssignTickets_NotHundredPercent[AssignedTeamType.Impostor],
        true, MaxImpostors);

        // Assign Neutral
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Neutral],
        AssignTickets_NotHundredPercent[AssignedTeamType.Neutral],
        false, MaxNeutrals);

        if (JackalFriends.JackalFriendsDontAssignIfJackalNotAssigned && !ExPlayerControl.ExPlayerControls.Any(player => player.IsJackalTeam()))
        {
            AssignTickets_HundredPercent[AssignedTeamType.Crewmate].RemoveAll(ticket => ticket.RoleOption.RoleId == RoleId.JackalFriends);
            AssignTickets_NotHundredPercent[AssignedTeamType.Crewmate].RemoveAll(ticket => ticket.RoleOption.RoleId == RoleId.JackalFriends);
        }

        // Assign Crews
        AssignTickets(AssignTickets_HundredPercent[AssignedTeamType.Crewmate],
        AssignTickets_NotHundredPercent[AssignedTeamType.Crewmate],
        false, MaxCrews);

        // Assign Modifiers
        AssignModifiers();

        // Assign Lovers
        AssignLovers();
    }
    private static void CreateTickets()
    {
        foreach (var team in Enum.GetValues(typeof(AssignedTeamType)).Cast<AssignedTeamType>())
        {
            AssignTickets_HundredPercent[team] = new();
            AssignTickets_NotHundredPercent[team] = new();
        }
        foreach (var roleOption in RoleOptionManager.RoleOptions)
        {
            if (roleOption.NumberOfCrews <= 0)
                continue;
            if (CustomRoleManager.TryGetRoleById(roleOption.RoleId, out var role) &&
                role.AvailableMaps.Length != 0 &&
                !role.AvailableMaps.Any(map => (byte)map == GameOptionsManager.Instance.CurrentGameOptions.MapId))
                continue;

            // LoversBreaker役職の特別な選出条件をチェック
            if (roleOption.RoleId == RoleId.LoversBreaker && ShouldSkipLoversBreakerAssignment())
                continue;

            if (roleOption.Percentage >= 100)
            {
                AssignTickets_HundredPercent[roleOption.AssignTeam].Add(new AssignTickets(roleOption));
            }
            else if (roleOption.Percentage > 0)
            {
                var ticket = new AssignTickets(roleOption);
                for (int i = 0; i < (roleOption.Percentage / 10); i++)
                {
                    AssignTickets_NotHundredPercent[roleOption.AssignTeam].Add(ticket);
                }
            }
        }
    }

    /// <summary>
    /// LoversBreaker役職の選出をスキップするかどうかを判定します。
    /// Lovers、Truelover、Cupidの全ての確率または選出数が0の場合にtrueを返します。
    /// </summary>
    private static bool ShouldSkipLoversBreakerAssignment()
    {
        // Lovers（Modifier）の確率と選出数をチェック
        bool loversDisabled = true;
        if (Lovers.LoversSpawnChance > 0 && Lovers.LoversMaxCoupleCount > 0)
            loversDisabled = false;

        // Truelover役職の確率と選出数をチェック
        bool trueloverDisabled = true;
        if (RoleOptionManager.TryGetRoleOption(RoleId.Truelover, out var trueloverOption))
        {
            if (trueloverOption.Percentage > 0 && trueloverOption.NumberOfCrews > 0)
            {
                trueloverDisabled = false;
            }
        }

        // Cupid役職の確率と選出数をチェック
        bool cupidDisabled = true;
        if (RoleOptionManager.TryGetRoleOption(RoleId.Cupid, out var cupidOption))
        {
            if (cupidOption.Percentage > 0 && cupidOption.NumberOfCrews > 0)
            {
                cupidDisabled = false;
            }
        }

        // 全ての役職が無効化されている場合はLoversBreaker役職をスキップ
        bool shouldSkip = loversDisabled && trueloverDisabled && cupidDisabled;

        if (shouldSkip)
        {
            Logger.Info("LoversBreaker役職をスキップします: Lovers、Truelover、Cupidの全ての確率または選出数が0です。");
        }

        return shouldSkip;
    }
    private static void AssignTickets(List<AssignTickets> tickets_hundred, List<AssignTickets> tickets_not_hundred, bool isImpostor, int maxBeans)
    {
        List<PlayerControl> targetPlayers = new();
        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.Role.IsSimpleRole && player.Data.Role.IsImpostor == isImpostor && (isImpostor || ((ExPlayerControl)player).Role is RoleId.Crewmate or RoleId.None))
                targetPlayers.Add(player);
        }
        if (targetPlayers.Count <= 0)
            return;

        // 100%チケットの割り当て処理
        while (tickets_hundred.Count > 0 && targetPlayers.Count > 0 && maxBeans > 0)
        {
            int ticketIndex = UnityEngine.Random.Range(0, tickets_hundred.Count);
            AssignTickets selectedTicket = tickets_hundred[ticketIndex];
            selectedTicket.IncrementRemainingAssignBeans();
            if (selectedTicket.RemainingAssignBeans <= 0)
                tickets_hundred.RemoveAt(ticketIndex);

            int playerIndex = UnityEngine.Random.Range(0, targetPlayers.Count);
            PlayerControl targetPlayer = targetPlayers[playerIndex];
            targetPlayers.RemoveAt(playerIndex);

            RoleId roleId = selectedTicket.RoleOption.RoleId;
            AssignRole(targetPlayer, roleId);
            AssignedRoleIds.Add(roleId); // アサインした役職を追跡
            maxBeans--;

            // 排他設定を再度適用（次のループのために）
            RoleOptionManager.ApplyExclusivitySettings(AssignedRoleIds, AssignTickets_NotHundredPercent.Values.ToArray(), AssignTickets_HundredPercent.Values.ToArray());
        }

        // 100%未満のチケットからランダムに選択して割り当てる
        while (tickets_not_hundred.Count > 0 && targetPlayers.Count > 0 && maxBeans > 0)
        {
            // 排他設定を適用
            RoleOptionManager.ApplyExclusivitySettings(AssignedRoleIds, AssignTickets_NotHundredPercent.Values.ToArray(), AssignTickets_HundredPercent.Values.ToArray());
            if (tickets_not_hundred.Count == 0)
                break;

            int ticketIndex = UnityEngine.Random.Range(0, tickets_not_hundred.Count);
            AssignTickets selectedTicket = tickets_not_hundred[ticketIndex];
            selectedTicket.IncrementRemainingAssignBeans();
            if (selectedTicket.RemainingAssignBeans <= 0)
                tickets_not_hundred.RemoveAll(x => x.RoleOption.RoleId == selectedTicket.RoleOption.RoleId);

            int playerIndex = targetPlayers.GetRandomIndex();
            PlayerControl targetPlayer = targetPlayers[playerIndex];
            targetPlayers.RemoveAt(playerIndex);

            RoleId roleId = selectedTicket.RoleOption.RoleId;
            AssignRole(targetPlayer, roleId);
            AssignedRoleIds.Add(roleId); // アサインした役職を追跡
            maxBeans--;

            // 排他設定を再度適用（次のループのために）
            RoleOptionManager.ApplyExclusivitySettings(AssignedRoleIds, AssignTickets_NotHundredPercent.Values.ToArray(), AssignTickets_HundredPercent.Values.ToArray());
        }
        foreach (var player in targetPlayers)
        {
            AssignRole(player, isImpostor ? RoleId.Impostor : RoleId.Crewmate);
        }
    }
    private static void AssignRole(PlayerControl player, RoleId roleId)
    {
        Logger.Info($"Assigning role {roleId} to player {player.PlayerId}");
        ((ExPlayerControl)player).RpcCustomSetRole(roleId);
    }
    private static void AssignModifiers()
    {
        Logger.Info("AssignModifiers() 開始: Modifierのアサイン処理を開始します。");
        var allPlayers = ExPlayerControl.ExPlayerControls;
        Logger.Info($"AssignModifiers: 全プレイヤー数 = {allPlayers.Count}");
        var allModifiers = CustomRoleManager.AllModifiers;
        Logger.Info($"AssignModifiers: 全Modifier数 = {allModifiers.Length}");

        foreach (var modifierBase in allModifiers)
        {
            var modifierRoleId = modifierBase.ModifierRole;
            Logger.Info($"AssignModifiers: Modifier処理開始 - ModifierRole = {modifierRoleId}");

            var modifierRoleOption = RoleOptionManager.ModifierRoleOptions.FirstOrDefault(x => x.ModifierRoleId == modifierRoleId);
            if (modifierRoleOption == null)
            {
                Logger.Info($"AssignModifiers: ModifierRoleOptionが見つからないため、ModifierRole {modifierRoleId} をスキップします。");
                continue;
            }
            if (modifierBase.HiddenOption)
            {
                Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} は非表示のためスキップします。");
                continue;
            }

            if (modifierBase.UseTeamSpecificAssignment)
            {
                AssignTeamSpecificModifier(modifierBase, modifierRoleOption);
            }
            else
            {
                if (modifierRoleOption.Percentage <= 0)
                {
                    Logger.Info($"AssignModifiers: ModifierRoleOptionのパーセンテージが0以下のため、ModifierRole {modifierRoleId} をスキップします。");
                    continue;
                }
                List<ExPlayerControl> targetPlayers = ExPlayerControl.ExPlayerControls
                    .Where(x => modifierBase.AssignedTeams.Count <= 0 || modifierBase.AssignedTeams.Contains(x.roleBase.AssignedTeam))
                    .Where(x => modifierRoleOption.AssignFilterList.Count == 0 || !modifierRoleOption.AssignFilterList.Contains(x.Role))
                    .Where(x => modifierBase.DoNotAssignRoles.Length == 0 || !modifierBase.DoNotAssignRoles.Contains(x.Role))
                    // .Where(x => modifierBase.ModifierAssignFilterTeam.Length == 0 || modifierBase.ModifierAssignFilterTeam.Contains(x.roleBase.AssignedTeam))
                    .ToList();
                Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} に適用可能なプレイヤー数 = {targetPlayers.Count}");

                for (int i = 0; i < modifierRoleOption.NumberOfCrews; i++)
                {
                    Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - ループ {i + 1}/{modifierRoleOption.NumberOfCrews} 開始");
                    int randomRoll = ModHelpers.GetRandomInt(0, 100);
                    Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - ループ {i + 1} のランダム値 = {randomRoll} (閾値: {modifierRoleOption.Percentage})");
                    if (randomRoll > modifierRoleOption.Percentage)
                    {
                        Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - ループ {i + 1} はランダム判定により割り当てをスキップします。");
                        continue;
                    }
                    if (targetPlayers.Count == 0)
                    {
                        Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - ループ {i + 1} で対象プレイヤーが存在しないため、ループを終了します。");
                        break;
                    }
                    int playerIndex = targetPlayers.GetRandomIndex();
                    Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - 選択されたプレイヤーインデックス = {playerIndex}");
                    PlayerControl targetPlayer = targetPlayers[playerIndex];
                    Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - 選択されたプレイヤーID = {targetPlayer.PlayerId}");
                    targetPlayers.RemoveAt(playerIndex);
                    Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - プレイヤー {targetPlayer.PlayerId} を対象リストから削除しました。");
                    Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} - プレイヤー {targetPlayer.PlayerId} に対してModifierの割り当てを試みます。");
                    AssignModifier(targetPlayer, modifierRoleId);
                }
            }
            Logger.Info($"AssignModifiers: ModifierRole {modifierRoleId} の処理が完了しました。");
        }
        Logger.Info("AssignModifiers() 終了: 全てのModifier処理が完了しました。");
    }

    private static void AssignTeamSpecificModifier(IModifierBase modifierBase, RoleOptionManager.ModifierRoleOption modifierRoleOption)
    {
        Logger.Info($"AssignTeamSpecificModifier() 開始: ModifierRole {modifierBase.ModifierRole} の陣営別アサイン処理を開始します。");
        var allPlayers = ExPlayerControl.ExPlayerControls;
        var modifierRoleId = modifierBase.ModifierRole;

        // インポスターへの割当
        var impostors = allPlayers.Where(x => x.IsImpostor() &&
                                        !x.ModifierRole.HasFlag(modifierRoleId) &&
                                        (modifierRoleOption.AssignFilterList.Count == 0 || !modifierRoleOption.AssignFilterList.Contains(x.Role)) &&
                                        (modifierBase.DoNotAssignRoles.Length == 0 || !modifierBase.DoNotAssignRoles.Contains(x.Role)))
                                .ToList();
        for (int i = 0; i < modifierRoleOption.MaxImpostors; i++)
        {
            int roll = ModHelpers.GetRandomInt(0, 100);
            if (roll <= modifierRoleOption.ImpostorChance && impostors.Count > 0)
            {
                var exPlayer = impostors[impostors.GetRandomIndex()];
                impostors.Remove(exPlayer);
                AssignModifier(exPlayer.Player, modifierRoleId);
            }
        }
        // 第三陣営への割当
        var neutrals = allPlayers.Where(x => x.IsNeutral() &&
                                       !x.ModifierRole.HasFlag(modifierRoleId) &&
                                       (modifierRoleOption.AssignFilterList.Count == 0 || !modifierRoleOption.AssignFilterList.Contains(x.Role)) &&
                                       (modifierBase.DoNotAssignRoles.Length == 0 || !modifierBase.DoNotAssignRoles.Contains(x.Role)))
                               .ToList();
        for (int i = 0; i < modifierRoleOption.MaxNeutrals; i++)
        {
            int roll = ModHelpers.GetRandomInt(0, 100);
            if (roll <= modifierRoleOption.NeutralChance && neutrals.Count > 0)
            {
                var exPlayer = neutrals[neutrals.GetRandomIndex()];
                neutrals.Remove(exPlayer);
                AssignModifier(exPlayer.Player, modifierRoleId);
            }
        }
        // クルーメイトへの割当
        var crewmates = allPlayers.Where(x => x.IsCrewmateOrMadRoles() &&
                                        !x.ModifierRole.HasFlag(modifierRoleId) &&
                                        (modifierRoleOption.AssignFilterList.Count == 0 || !modifierRoleOption.AssignFilterList.Contains(x.Role)) &&
                                        (modifierBase.DoNotAssignRoles.Length == 0 || !modifierBase.DoNotAssignRoles.Contains(x.Role)))
                                .ToList();
        for (int i = 0; i < modifierRoleOption.MaxCrewmates; i++)
        {
            int roll = ModHelpers.GetRandomInt(0, 100);
            if (roll <= modifierRoleOption.CrewmateChance && crewmates.Count > 0)
            {
                var exPlayer = crewmates[crewmates.GetRandomIndex()];
                crewmates.Remove(exPlayer);
                AssignModifier(exPlayer.Player, modifierRoleId);
            }
        }
        Logger.Info($"AssignTeamSpecificModifier() 終了: ModifierRole {modifierBase.ModifierRole} の陣営別アサイン処理が完了しました。");
    }

    private static void AssignLovers()
    {
        LoversIndex = 0;
        Logger.Info("AssignLovers() 開始: Loversのアサイン処理を開始します。");
        // スポーン確率チェック
        if (Lovers.LoversSpawnChance <= 0)
        {
            Logger.Info("AssignLovers: 生成確率が0以下のためスキップします。");
            return;
        }

        // 生存プレイヤー取得
        var candidates = ExPlayerControl.ExPlayerControls
            .Where(p => !p.IsDead());

        // オプションに応じて候補をフィルタリング
        if (!Lovers.LoversIncludeImpostorsInSelection)
        {
            candidates = candidates
                .Where(p => !p.Data.Role.IsImpostor);
        }
        if (!Lovers.LoversIncludeThirdTeamInSelection)
        {
            candidates = candidates
                .Where(p => !p.IsNeutral());
        }

        // ModifierGuesserのAssignFilterを取得
        if (RoleOptionManager.TryGetModifierRoleOption(ModifierRoleId.Lovers, out var modifierLovers))
        {
            if (modifierLovers.AssignFilterList.Count > 0)
            {
                candidates = candidates
                    .Where(p => !modifierLovers.AssignFilterList.Contains(p.Role));
            }
            if (Lovers.Instance.DoNotAssignRoles.Length > 0)
            {
                candidates = candidates
                    .Where(p => !Lovers.Instance.DoNotAssignRoles.Contains(p.Role));
            }
        }

        candidates = candidates.Where(p => p.Role is not RoleId.Truelover and not RoleId.Cupid and not RoleId.LoversBreaker);

        var candidatesList = candidates.ToList();

        // カップル数計算
        int maxCouples = (int)Lovers.LoversMaxCoupleCount;
        int coupleCount = Math.Min(maxCouples, candidatesList.Count / 2);
        Logger.Info($"AssignLovers: カップル数 = {coupleCount}");

        int impostorTriedCount = 0;

        // カップル作成ループ
        for (int i = 0; i < coupleCount; i++)
        {
            int roll = ModHelpers.GetRandomInt(0, 100);
            Logger.Info($"AssignLovers: カップル {i + 1}/{coupleCount} 判定 = {roll} (閾値: {Lovers.LoversSpawnChance})");
            if (roll >= Lovers.LoversSpawnChance)
            {
                Logger.Info($"AssignLovers: カップル {i + 1} はスキップ");
                continue;
            }
            if (candidatesList.Count < 2)
            {
                Logger.Info("AssignLovers: 候補が2名未満のため終了します。");
                break;
            }

            // ランダムで2名選択
            int idxA = candidatesList.GetRandomIndex();
            var playerA = candidatesList[idxA];
            candidatesList.RemoveAt(idxA);
            int idxB = candidatesList.GetRandomIndex();
            var playerB = candidatesList[idxB];
            candidatesList.RemoveAt(idxB);

            // インポスター同士の組み合わせはスキップ
            if ((playerA.IsImpostor() && playerB.IsImpostor()) ||
                (playerA.IsJackal() && playerB.IsJackal()))
            {
                Logger.Info($"AssignLovers: インポスター同士({playerA.PlayerId}, {playerB.PlayerId})のためスキップします。");
                // 候補リストに戻す
                candidatesList.Add(playerA);
                candidatesList.Add(playerB);
                impostorTriedCount++;
                // 100回までトライできる
                if (impostorTriedCount <= 100)
                    i--;
                continue;
            }

            Logger.Info($"AssignLovers: カップル {i + 1} - プレイヤー {playerA.PlayerId} と {playerB.PlayerId}");
            RpcCustomSetLovers(playerA, playerB, LoversIndex, false);
        }

        Logger.Info("AssignLovers() 終了: Loversのアサイン処理が完了しました。");
    }

    [CustomRPC]
    public static void RpcCustomSetLovers(ExPlayerControl playerA, ExPlayerControl playerB, byte loversIndex, bool setNameText)
    {
        CustomSetLovers(playerA, playerB, loversIndex, setNameText);
    }
    public static LoversCouple CustomSetLovers(ExPlayerControl playerA, ExPlayerControl playerB, byte loversIndex, bool setNameText)
    {
        playerA.SetModifierRole(ModifierRoleId.Lovers);
        playerB.SetModifierRole(ModifierRoleId.Lovers);
        LoversAbility loversAbilityA = playerA.GetAbility<LoversAbility>();
        LoversAbility loversAbilityB = playerB.GetAbility<LoversAbility>();
        LoversCouple loversCouple = new([loversAbilityA, loversAbilityB], loversIndex);
        loversAbilityA.SetCouple(loversCouple);
        loversAbilityB.SetCouple(loversCouple);
        if (setNameText)
        {
            NameText.UpdateNameInfo(playerA);
            NameText.UpdateNameInfo(playerB);
        }
        LoversIndex = (byte)(loversIndex + 1);
        return loversCouple;
    }

    private static void AssignModifier(PlayerControl player, ModifierRoleId modifierRoleId)
    {
        Logger.Info($"AssignModifier: プレイヤー {player.PlayerId} に ModifierRole {modifierRoleId} の割り当て処理を開始します。");
        ExPlayerControl exPlayer = player;

        // 既存のモディファイアとフラグの状態を確認
        if (exPlayer.ModifierRole.HasFlag(modifierRoleId))
        {
            Logger.Info($"AssignModifier: プレイヤー {player.PlayerId} は既にModifierRole {modifierRoleId} を保持しているため、割り当てをスキップします。");
            return;
        }

        Logger.Info($"AssignModifier: プレイヤー {player.PlayerId} にModifierRole {modifierRoleId} を割り当てます。");
        ModifierRoleId newModifierRole = modifierRoleId;
        exPlayer.RpcCustomSetModifierRole(newModifierRole);
        Logger.Info($"AssignModifier: RPCを使用してプレイヤー {player.PlayerId} にModifierRole {modifierRoleId} を適用しました。");
    }
}
public class AssignTickets
{
    public RoleOptionManager.RoleOption RoleOption { get; }
    public int RemainingAssignBeans { get; private set; }
    public AssignTickets(RoleOptionManager.RoleOption roleOption)
    {
        RoleOption = roleOption;
        RemainingAssignBeans = roleOption.NumberOfCrews;
    }
    public void IncrementRemainingAssignBeans()
    {
        RemainingAssignBeans--;
    }
}