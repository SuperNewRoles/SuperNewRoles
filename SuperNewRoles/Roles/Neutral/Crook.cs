using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class Crook
{
    public static class CustomOptionData
    {
        private const int optionId = 303600;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption NumberOfInsuranceClaimsReceivedRequiredToWin;
        public static CustomOption TimeTheAbilityToInsureOthersIsAvailable;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, true, RoleId.Crook);
            PlayerCount = CustomOption.Create(optionId + 1, true, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option);
            NumberOfInsuranceClaimsReceivedRequiredToWin = CustomOption.Create(optionId + 2, true, CustomOptionType.Neutral, "CrookNumberOfInsuranceClaimsReceivedRequiredToWin", 3f, 1f, 10f, 1f, Option);
            TimeTheAbilityToInsureOthersIsAvailable = CustomOption.Create(optionId + 3, true, CustomOptionType.Neutral, "CrookTimeTheAbilityToInsureOthersIsAvailable", 10f, 5f, 60f, 5f, Option);
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = new(96, 161, 189, byte.MaxValue);
        internal static float TimeForAbilityUse { get; private set; }
        internal static float NumberNeededWin { get; private set; }

        /// <summary>
        /// 勝利条件 [ 指定回数の保険金を受領可能か ] を, 詐欺師全体で 満たしている者がいるかの判定を保存する。
        /// </summary>
        internal static bool FirstWinFlag;

        public static void ClearAndReload()
        {
            Player = new();
            TimeForAbilityUse = CustomOptionData.TimeTheAbilityToInsureOthersIsAvailable.GetFloat() + 7f; // 7f は 会議開始アニメーション所要時間
            NumberNeededWin = CustomOptionData.NumberOfInsuranceClaimsReceivedRequiredToWin.GetFloat();
            FirstWinFlag = false;
            DecisionOfVictory.ExiledCrookPlayerId = new();
            Ability.ClearAndReload();
        }
    }

    internal static class WrapUp
    {
        internal static void GeneralProcess(PlayerControl exiledPlayer)
        {
            ResetTimerProcess();
            WinCheckProcess(exiledPlayer);
        }

        private static void ResetTimerProcess()
        {
            Ability.TimerStop(); // 会議中に能力終了しない場合を想定
            Ability.AbilityCountDown = RoleData.TimeForAbilityUse; // 能力使用可能時間をリセット (タイマーストップ時にリセットしないのは, これが0sの時 残り会議秒数の表示を置換しない制御にしている為, 会議中にリセットできないから)
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) Ability.InHostMode.ResetTimerRelatedVariableOnWarpUp();
        }

        /// <summary>
        /// 詐欺師全体で勝利条件を満たしている者がいるかを取得し, 満たしていたら詐欺師勝利処理を実行する。(SHR, SNR共通処理)
        /// </summary>
        private static void WinCheckProcess(PlayerControl exiledPlayer)
        {
            if (exiledPlayer != null && exiledPlayer.IsRole(RoleId.Crook)) // 追放者がいて, 役職が詐欺師なら
            {
                DecisionOfVictory.ExiledCrookPlayerId.Add(exiledPlayer.PlayerId); // Listに追加し, 受領判定時に死者として扱える様にする。
            }

            // 条件判定タイミング(会議開始時)に, 条件を誰も満たしていなかったら以下を読まない。
            if (!(AmongUsClient.Instance.AmHost && RoleData.FirstWinFlag)) return;
            Logger.Info($"勝利条件を満たした 詐欺師が存在した", "FirstWinFlag");

            if (DecisionOfVictory.GetTheLastDecisionAndWinners().Item1) // 受領判定時に勝利条件を満たしていたら, 勝利処理を実行する。
            {
                var reason = (GameOverReason)CustomGameOverReason.CrookWin;
                CheckGameEndPatch.CustomEndGame(reason, false);
            }
        }
    }

    internal static class DecisionOfVictory
    {
        /// <summary>
        /// 追放された詐欺師を保存する事で, 受領判定及び勝利判定時に死亡済みとして扱えるようにする。
        /// FIXME : 基幹バグである「追放中は未だ死んでいない」事により生じるバグを役職側で調整している状態。
        /// 基幹バグが修正され次第必要がなくなる変数
        /// </summary>
        internal static List<byte> ExiledCrookPlayerId;

        /// <summary>
        /// 勝利条件を達成していた詐欺師が, 勝利処理を実行可能か 判断する。(SHR, SNR共通処理)
        /// </summary>
        /// <returns>
        /// Item1 => true : 保険金受領場所(追放画面)に到達し, 最後の保険金を受給できた。 / false : 保険金受領場所に到達できず, 最後の保険金を受給できなかった。
        /// Item2 => 勝利可能な詐欺師達
        /// </returns>
        internal static (bool, List<PlayerControl>) GetTheLastDecisionAndWinners()
        {
            List<PlayerControl> winners = new();
            var winFlag = false;

            foreach (KeyValuePair<byte, byte> kvp in Ability.RecordOfTimesInsuranceClaimsAreReceived)
            {
                bool privateWinFlag = kvp.Value >= RoleData.NumberNeededWin; // 詐欺師個人の勝利判定の取得
                if (!privateWinFlag) continue;

                var crook = ModHelpers.GetPlayerControl(kvp.Key);
                if (!crook.IsRole(RoleId.Crook)) continue; // 役職が変わった「元詐欺師」は 勝利不可。
                if (crook.IsDead() || ExiledCrookPlayerId.Contains(crook.PlayerId)) continue; // 保険金受給時 (追放処理時) に死亡している 詐欺師は勝利不可。

                winFlag = true; // 追放画面に遷移し最後の保険金を受け取れた。
                winners.Add(crook);
            }

            if (winFlag) Logger.Info($"最後の保険金の受給にたどり着いた 詐欺師が存在した", "FinalWinFlag");
            else Logger.Info($"最後の保険金の受給にどの詐欺師もたどり着けなかった。", "FinalWinFlag");

            return (winFlag, winners);
        }
    }

    [HarmonyPatch]
    internal static class Ability
    {
        /// <summary>
        /// 保険を契約させた詐欺師と, 契約したプレイヤーを保存する辞書
        /// </summary>
        /// <value>key : 会議回数 , value : ( key : 詐欺師のPlayerId , value : 保険を契約したプレイヤーのPlayerId )</value>
        private static Dictionary<byte, Dictionary<byte, byte>> SignDictionary;

        /// <summary>
        /// 保険金受取回数記録
        /// </summary>
        /// <value>key : 詐欺師のPlayerId , value : 保険金を受け取った回数</value>
        internal static Dictionary<byte, byte> RecordOfTimesInsuranceClaimsAreReceived;

        /// <summary>
        /// 現在ターンで誰が誰の保険金を受け取っているか保存する。
        /// (MeetingStartで前回ターンの情報を削除してから判定し保存, WarpUpで参照する。)
        /// </summary>
        /// <value>key : 保険金を需給できた詐欺師のプレイヤーID / value : 保険金を掛けられていた対象のプレイヤーID</value>
        private static Dictionary<byte, byte> ReceivedTheInsuranceDictionary;
        internal static float AbilityCountDown;
        private static Timer CountDownTimer; // 能力使用可能時間を管理するタイマー
        private static Timer ChangeBlueTimer; // 能力使用可能時間の終了警告を, 5秒前から0.25秒間隔で文字を点滅させる事で行うタイマー
        private static bool IsChangeBlue; // 0.25秒間隔で文字を点滅させる為の変数

        internal static void ClearAndReload()
        {
            SignDictionary = new();
            RecordOfTimesInsuranceClaimsAreReceived = new();
            ReceivedTheInsuranceDictionary = new();
            AbilityCountDown = RoleData.TimeForAbilityUse;
            IsChangeBlue = false;
            InClientMode.ClearAndReload();
            InHostMode.ClearAndReload();
        }

        /// <summary>
        /// アビリティ使用可能時間の管理, 表示を行う。(SHR, SNR共通処理)
        /// </summary>
        private static void SetAvailabilityTimeTimer()
        {
            AbilityCountDown = RoleData.TimeForAbilityUse;
            bool isClientMode = ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf);

            if (!isClientMode)
            {
                float second = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.VotingTime) - RoleData.TimeForAbilityUse + 7f;
                string announce = string.Format(ModTranslation.GetString("CrookSHRMeetingStartAnnounce"), "#FF4B00", second, second - 10f);

                foreach (var crook in RoleData.Player)
                {
                    if (crook.IsDead()) continue;
                    AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), announce, "#60a1bd"); // 初ターンは役職説明の上に表示される為, このアナウンスに気づかない事があるがそれは仕様とする。
                }
            }
        }

        /// <summary>
        /// タイマーを停止させ, 関連する変数をリセットする。
        /// </summary>
        internal static void TimerStop(bool isEndGame = false, bool isStartGraceTimeTimer = false)
        {
            if (CountDownTimer != null)
            {
                CountDownTimer.Stop();
                if (isEndGame) CountDownTimer.Dispose();
            }
            AbilityCountDown = 0;

            if (ChangeBlueTimer != null)
            {
                ChangeBlueTimer.Stop();
                if (isEndGame) CountDownTimer.Dispose();
            }
            IsChangeBlue = false;

            if (!isStartGraceTimeTimer) InHostMode.TimerStop(isEndGame);
        }

        /// <summary>
        /// 詐欺師と詐欺師が保険を掛けたプレイヤーの組み合わせを辞書に保存する。(SHR, SNR共通処理)
        /// </summary>
        /// <param name="crookId">保険を掛けた詐欺師</param>
        /// <param name="TargetId">保険が掛けられたプレイヤー</param>
        internal static void SaveSignDictionary(byte crookId, byte TargetId)
        {
            if (SignDictionary.ContainsKey(ReportDeadBodyPatch.MeetingTurn_Now))
            {
                SignDictionary[ReportDeadBodyPatch.MeetingTurn_Now][crookId] = TargetId;
            }
            else
            {
                Dictionary<byte, byte> dic = new()
                {
                    { crookId, TargetId }
                };
                SignDictionary.Add(ReportDeadBodyPatch.MeetingTurn_Now, dic);
            }

            string announce = string.Format(ModTranslation.GetString("CrookInsuredChatAnnounce"), ModHelpers.GetPlayerControl(crookId).GetDefaultName(), ModHelpers.GetPlayerControl(TargetId).GetDefaultName());
            if (AmongUsClient.Instance.AmHost)
            {
                foreach (PlayerControl p in  PlayerControl.AllPlayerControls)
                {
                    if (p.IsAlive()) continue;
                    AddChatPatch.ChatInformation(p, ModTranslation.GetString("CrookName"), announce, "#60a1bd");
                }
            }
            Logger.Info(announce, "CrookAbility");
        }

        /// <summary>
        /// 会議開始時に, 今回ターンのアビリティの管理を実行し, 前回ターンの保険金の受給の有無を判定 及び 保存する
        /// </summary>
        internal static void SaveReceiptOfInsuranceProceeds()
        {
            SetAvailabilityTimeTimer(); // SHR, SNR両方でアビリティ使用可能時間の管理を実行する。

            ReceivedTheInsuranceDictionary = new();

            var previousTurn = (byte)(ReportDeadBodyPatch.MeetingTurn_Now - 1); // 前回ターンのターン数

            if (!SignDictionary.TryGetValue(previousTurn, out var signSituationOfNowTurnDic)) return; // 前回ターンの詐欺情報が保存されていなかったら以下を読まない。

            foreach (KeyValuePair<byte, byte> kvp in signSituationOfNowTurnDic)
            {
                var targetId = kvp.Value;
                var target = ModHelpers.GetPlayerControl(targetId);
                if (ModHelpers.GetPlayerControl(targetId).IsAlive()) continue; // ターゲットが生存していたら 以下を読まない

                var crookId = kvp.Key;
                var crook = ModHelpers.GetPlayerControl(crookId);
                if (crook.IsDead()) continue; // 詐欺師が死亡していたら 以下を読まない

                ReceivedTheInsuranceDictionary[crookId] = targetId; // 今回ターンの 受給の状況を保存

                // 詐欺師ごとの保険金受給回数を保存
                if (RecordOfTimesInsuranceClaimsAreReceived.TryGetValue(previousTurn, out var times))
                {
                    times++;
                    RecordOfTimesInsuranceClaimsAreReceived[crookId] = times;
                }
                else
                {
                    times = 1;
                    RecordOfTimesInsuranceClaimsAreReceived.Add(crookId, times);
                }

                bool privateWinFlag = times >= RoleData.NumberNeededWin; // 詐欺師個人の勝利判定の取得
                if (privateWinFlag) RoleData.FirstWinFlag = true;
                var remainingNumber = RoleData.NumberNeededWin - times;
                Logger.Info($"{crook.name} => 現在{times}回目の取得, 勝利に必要な回数は残り{remainingNumber}回", "crook");

                if (AmongUsClient.Instance.AmHost) // 追放画面で保険金受給の有無をチャット通知
                {
                    string chatText;
                    if (privateWinFlag) chatText = $"{string.Format(ModTranslation.GetString("CrookReceiveSetWinFlagChatAnnounce"), target.GetDefaultName(), times)}";
                    else chatText = $"{string.Format(ModTranslation.GetString("CrookReceiveSuccessChatAnnounce"), target.GetDefaultName(), times, remainingNumber)}";
                    AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), chatText, "#60a1bd");
                }
            }
        }

        /// <summary>
        /// 保険金の受給の有無を, 保存した辞書から取得し, アナウンスを作成する。(SHR, SNR共通処理)
        /// </summary>
        /// <returns>bool : 保険金の受給の有無 / string : 保険金を受給した旨のアナウンステキスト </returns>
        internal static (bool, string) GetIsReceivedTheInsuranceAndAnnounce()
        {
            bool IsReceivedTheInsurance = false;
            StringBuilder announceBuilder = new();
            byte previousTurn = (byte)(ReportDeadBodyPatch.MeetingTurn_Now - 1);

            foreach (KeyValuePair<byte, byte> kvp in ReceivedTheInsuranceDictionary)
            {
                IsReceivedTheInsurance = true;
                var target = ModHelpers.GetPlayerControl(kvp.Value);

                announceBuilder.AppendLine($"{string.Format(ModTranslation.GetString("CrookReceiveAnnounce"), target.GetDefaultName())}");
            }

            return (IsReceivedTheInsurance, announceBuilder.ToString());
        }

        [HarmonyPatch]
        internal static class InClientMode
        {
            // |:========== ボタン関係の変数管理 ==========:|
            private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CrookButton.png", 115f);
            private static bool AllladyDead;

            internal static void ClearAndReload()
            {
                AllladyDead = false;
            }

            // |:========================================:|

            /// <summary>
            /// 詐欺師の能力使用可能時間を, 議論時間のタイマーを置換して表示する。
            /// </summary>
            [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new Type[] { typeof(StringNames), typeof(Il2CppReferenceArray<Il2CppSystem.Object>) }), HarmonyPostfix]
            private static void StringPostfix(ref string __result, [HarmonyArgument(0)] StringNames id)
            {
                if (id != StringNames.MeetingVotingEnds) return;
                if (!ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf)) return;
                if (!PlayerControl.LocalPlayer.IsRole(RoleId.Crook)) return;
                if (PlayerControl.LocalPlayer.IsDead()) return;
                if (AbilityCountDown <= 0) return;

                string color = IsChangeBlue ? "<color=#60a1bd>" : "<color=#FFFFFF>";
                string announce = $"{color}{string.Format(ModTranslation.GetString("WerewolfAbilityTime"), Mathf.CeilToInt(AbilityCountDown))}</color>";

                __result = announce;
            }

            internal static void MeetingHudStartPostfix(MeetingHud __instance) // 詐欺師本人のみ実行する (詐欺師にはHostがSNR,SHR共通で動かす``MeetingHud.Start Postfix``もある。))
                => ButtonCreate(__instance);

            /// <summary>
            /// 詐欺師のボタンを作成するコード。
            /// </summary>
            /// <param name="__instance"></param>
            private static void ButtonCreate(MeetingHud __instance)
            {
                if (!ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf)) return;
                if (!PlayerControl.LocalPlayer.IsRole(RoleId.Crook)) return;
                if (PlayerControl.LocalPlayer.IsDead())
                {
                    AllladyDead = true;
                    return;
                }

                bool isClientMode = ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf);

                CountDownTimer = new Timer(1000);
                CountDownTimer.Elapsed += (source, e) =>
                {
                    if (AbilityCountDown > 0)
                    {
                        AbilityCountDown--;
                        if (AbilityCountDown <= 5) // 残り5秒目から
                        {
                            if (isClientMode) // クライアントモードなら 終了警告音を鳴らす
                            {
                                SoundManager.Instance.PlaySound(MeetingHud.Instance.VoteEndingSound, false, 1f, null).pitch = Mathf.Lerp(1.5f, 0.8f, AbilityCountDown / 10f);
                            }
                            else // ホストモードなら カウントダウンを表示させる。
                            {
                                InHostMode.IsAllladyCountdownToSecond = new();
                            }
                        }
                    }
                    else
                    {
                        if (isClientMode)
                        {
                            TimerStop();
                        }
                        else
                        {
                            TimerStop(isStartGraceTimeTimer: true);
                            InHostMode.SetGraceTimeTimer();
                        }
                    }
                };

                CountDownTimer.AutoReset = AbilityCountDown >= 0;
                CountDownTimer.Enabled = true;

                ChangeBlueTimer = new(250); // 0.25秒間隔で文字を点滅する
                ChangeBlueTimer.Elapsed += (source, e) =>
                {
                    if (AbilityCountDown > 5)
                    {
                        IsChangeBlue = false; // 残り5秒目までは文字色は白
                    }
                    else
                    {
                        IsChangeBlue ^= true; // 文字色を点滅させる
                    }
                };
                ChangeBlueTimer.AutoReset = AbilityCountDown > 0;
                ChangeBlueTimer.Enabled = true;

                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
                    var playerRole = player.GetRole();

                    // ネームプレートの対象が死亡している、又は自分自身ならば
                    if (player.IsDead() || player.PlayerId == CachedPlayer.LocalPlayer.PlayerId) continue;

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "CrookButton";
                    targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1.3f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = GetButtonSprite();
                    renderer.sortingOrder = 0;
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    byte TargetPlayerId = player.PlayerId;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => OnClick(TargetPlayerId, __instance)));
                    Logger.Info($"{player.name}に[保険をかける]ボタンを作成します。", "CrookButton");
                }
            }

            // ボタンを押したときの動作。
            private static void OnClick(byte TargetId, MeetingHud __instance)
            {
                if (!ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf)) return;

                var target = ModHelpers.GetPlayerControl(TargetId);
                if (target.IsDead())
                {
                    // 既に死亡していて保険をかけられなかった場合, チャットで警告を行う。
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(ModTranslation.GetString("CrookErrorTargetAllladyDead"), target.name));
                    return;
                }

                TimerStop();

                var crookId = PlayerControl.LocalPlayer.PlayerId;

                // 詐欺師本人の記録
                SaveSignDictionary(crookId, TargetId);

                // 他者への記録の送信
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CrookSaveSignDictionary);
                writer.Write(crookId);
                writer.Write(TargetId);
                writer.EndRPC();

                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("CrookButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("CrookButton").gameObject); }); // ボタン削除
            }

            /// <summary>
            /// 会議中のボタン表示を常時更新する。
            /// </summary>
            /// <param name="__instance"></param>
            internal static void UpdateButtonsPostfix(MeetingHud __instance)
            {
                if (!ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf)) return;
                if (AllladyDead) return; // 会議開始時点で死亡していた場合, 以下の処理を実行しないようにする。

                if (PlayerControl.LocalPlayer.IsDead() || AbilityCountDown == 0)
                {
                    __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("CrookButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("CrookButton").gameObject); });
                    AllladyDead = true;
                }
                else // 自身が生存しているなら
                {
                    __instance.playerStates.ToList().ForEach(x =>
                        {
                            if (x.transform.FindChild("CrookButton") != null)
                            {
                                var p = ModHelpers.PlayerById(x.TargetPlayerId);
                                if (p.IsDead())
                                {
                                    UnityEngine.Object.Destroy(x.transform.FindChild("CrookButton").gameObject);
                                }
                            }
                        });
                }
            }
        }

        internal static class InHostMode
        {
            /// <summary>
            /// 投票無効化時間 ( 能力指定受付時間 + 猶予時間 ) であるかを示す
            /// </summary>
            private static bool IsTimeToNullTheVote;

            /// <summary>
            /// 現在の秒数はカウントダウン済みかを示す
            /// </summary>
            internal static Dictionary<byte, bool> IsAllladyCountdownToSecond; // Countdownの変数の制御は共通処理側で行う。

            private static Timer GraceTimeTimer;

            internal static void ClearAndReload()
            {
                IsTimeToNullTheVote = true;
                IsAllladyCountdownToSecond = new();
            }

            internal static void TimeoutCountdownAnnounce()
            {
                if (!(ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)) return;

                if (AbilityCountDown is not (<= 5 and >= 0)) { return; }

                string warningStr = string.Format(ModTranslation.GetString("WerewolfAbilityTime"), AbilityCountDown);

                foreach (var crook in RoleData.Player)
                {
                    if (crook.IsDead()) continue;

                    IsAllladyCountdownToSecond.TryGetValue(crook.PlayerId, out bool isAllladyCountdown); // この秒数は既にカウントしているか?

                    // 以下順番前後変更禁止
                    if (!isAllladyCountdown && !SignDictionary.ContainsKey(ReportDeadBodyPatch.MeetingTurn_Now)) // 今回の会議の保険契約情報が, 誰一人保存されていないなら
                    {
                        IsAllladyCountdownToSecond[crook.PlayerId] = true;
                        AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), warningStr, "#60a1bd");
                    }
                    else if (!isAllladyCountdown && !SignDictionary[ReportDeadBodyPatch.MeetingTurn_Now].ContainsKey(crook.PlayerId)) // 特定の詐欺師の, 今回の会議の保険契約情報が保存されていないなら
                    {
                        IsAllladyCountdownToSecond[crook.PlayerId] = true;
                        AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), warningStr, "#60a1bd");
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            /// <summary>
            /// 投票形式による詐欺師の保険を契約させる対象指定
            /// </summary>
            /// <param name="srcPlayerId">投票者のplayerId</param>
            /// <param name="suspectPlayerId">投票先のplayerId</param>
            /// <returns> true : 投票を反映する / false : 投票を反映しない </returns>
            internal static bool MeetingHudCastVote_Prefix(byte crookId, byte targetId)
            {
                if (!(ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)) return true;
                if (targetId is 252 or 253/*スキップ*/ or 254/*無投票*/ or 255/*未投票*/) return true; // スキップ系統なら 時間判定を行わず 有効票を返す。
                if (SignDictionary.ContainsKey(ReportDeadBodyPatch.MeetingTurn_Now) && SignDictionary[ReportDeadBodyPatch.MeetingTurn_Now].ContainsKey(crookId)) return true; // 既に契約させているなら 時間判定を行わず 有効票を返す。

                if (!IsTimeToNullTheVote) // 投票無効化時間でないならば,
                {
                    return true; // 有効票を返す。
                }
                else // 投票無効化時間内であり,
                {
                    PlayerControl crook = ModHelpers.GetPlayerControl(crookId);
                    PlayerControl target = ModHelpers.GetPlayerControl(targetId);

                    if (AbilityCountDown > 0) // 能力使用可能な時間内なら
                    {
                        if (crookId == targetId) // 自投票なら
                        {
                            string infoStr = string.Format(ModTranslation.GetString("CrookVoteBotErrorMessage"), target.name);
                            AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), infoStr, "#60a1bd");
                        }
                        else if (target.IsBot()) //　ターゲットがBotなら
                        {
                            string infoStr = string.Format(ModTranslation.GetString("CrookVoteYourselfErrorMessage"), target.name);
                            AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), infoStr, "#60a1bd");
                        }
                        else
                        {
                            string infoStr = string.Format(ModTranslation.GetString("CrookVoteSuccessMessage"), target.name);
                            AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), infoStr, "#60a1bd");

                            // ホストの記録
                            SaveSignDictionary(crookId, targetId);

                            // 導入者ゲストへの記録送信
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CrookSaveSignDictionary);
                            writer.Write(crookId);
                            writer.Write(targetId);
                            writer.EndRPC();
                        }

                        return false; // 無効票を返す。
                    }
                    else // 能力使用不可な時間なら
                    {
                        float second = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.VotingTime) - RoleData.TimeForAbilityUse - 3f;
                        string warningStr = string.Format(ModTranslation.GetString("CrookErrorTimeout"), "#FF4B00", second);

                        Logger.Info($"{crook.name}は, 能力使用時間内に保険を契約させられず, 猶予時間以内に投票しました。[バニラ会議時間 : {GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.VotingTime)}, 能力使用可能時間 {RoleData.TimeForAbilityUse - 7f}, 猶予時間終了時間{second}]", "Crook Vote");
                        AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), warningStr, "#60a1bd");
                        return false; // 無効票を返す。
                    }
                }
            }

            /// <summary>
            /// 投票無効化猶予時間の制御
            /// </summary>
            internal static void SetGraceTimeTimer()
            {
                if (!(ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)) return;

                GraceTimeTimer = new(10000); // 10秒 リピートなしのタイマー
                GraceTimeTimer.Elapsed += (source, e) =>
                {
                    IsTimeToNullTheVote = false; // 投票を無効化する時間か? を オフにする (以降, 条件判定せず投票が可能になる)
                };
                GraceTimeTimer.AutoReset = false;
                GraceTimeTimer.Enabled = true;
            }

            internal static void TimerStop(bool isEndGame = false)
            {
                if (!AmongUsClient.Instance.AmHost) return;

                if (GraceTimeTimer != null)
                {
                    GraceTimeTimer.Stop();
                    if (isEndGame) GraceTimeTimer.Dispose();
                }
            }

            internal static void ResetTimerRelatedVariableOnWarpUp()
            {
                if (!(ModeHandler.IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)) return;

                IsTimeToNullTheVote = true;
                IsAllladyCountdownToSecond = new();
            }
        }
    }
}
