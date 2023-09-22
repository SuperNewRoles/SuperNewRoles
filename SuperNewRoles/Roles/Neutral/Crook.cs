using System;
using System.Linq;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Patches;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;
using HarmonyLib;
using System.Text;

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
        /// 詐欺師が勝利条件を満たしているかを保存する。
        /// </summary>
        /// <value>
        /// Item1 => true : 会議開始時に最後の保険金の取得が可能である,
        /// Item2 => true : 追放時に最後の保険金を受け取る事ができた
        /// </value>
        /// 2つフラグを保存するのは, [ 勝利条件を満たしたかの判定 (Meeting.start) ] と [ 勝利処理の実行 (WarpUp) ] の 実行タイミングが別の為。
        internal static (bool, bool) IsWinFlag;


        public static void ClearAndReload()
        {
            Player = new();
            TimeForAbilityUse = CustomOptionData.TimeTheAbilityToInsureOthersIsAvailable.GetFloat();
            IsWinFlag = (false, false);
            NumberNeededWin = CustomOptionData.NumberOfInsuranceClaimsReceivedRequiredToWin.GetFloat();
            Ability.ClearAndReload();
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
        /// (MeetingStartで前回ターンの情報を削除してから判定し保存, ExileController.Beginで参照する。)
        /// </summary>
        /// <value>key : 保険金を需給できた詐欺師のプレイヤーID / value : 保険金を掛けられていた対象のプレイヤーID</value>
        private static Dictionary<byte, byte> ReceivedTheInsuranceDictionary;

        internal static void ClearAndReload()
        {
            SignDictionary = new();
            RecordOfTimesInsuranceClaimsAreReceived = new();
            ReceivedTheInsuranceDictionary = new();
            Button.ClearAndReload();
        }

        /// <summary>
        /// 詐欺師と詐欺師が保険を掛けたプレイヤーの組み合わせを辞書に保存する。
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

            Logger.Info($"詐欺師({ModHelpers.GetPlayerControl(crookId).name})が, {ModHelpers.GetPlayerControl(TargetId).name}に保険を掛けさせました", "CrookAbility");
        }

        /// <summary>
        /// 保険金の受給の有無を判定 及び 保存する
        /// </summary>
        internal static void SaveReceiptOfInsuranceProceeds()
        {
            // [ ]MEMO : 現状SHRSNR共用可能
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

                if (AmongUsClient.Instance.AmHost)
                {
                    // [x]MEMO : 追放画面で保険金受給の有無をチャット通知
                    bool privateWinFlag = times >= RoleData.NumberNeededWin; // 詐欺師個人の勝利判定の取得
                    if (privateWinFlag) RoleData.IsWinFlag.Item1 = true; // IsWinFlag は 詐欺師全体の勝利判定を保存する。
                    var remainingNumber = RoleData.NumberNeededWin - times;
                    Logger.Info($"{crook.name} => 現在{times}回目の取得, 勝利に必要な回数は残り{remainingNumber}回", "crook");

                    string chatText;
                    if (privateWinFlag) chatText = $"<align={"left"}>{string.Format(ModTranslation.GetString("CrookReceiveSetWinFlagChatAnnounce"), target.GetDefaultName(), times)}</align>";
                    else chatText = $"<align={"left"}>{string.Format(ModTranslation.GetString("CrookReceiveSuccessChatAnnounce"), target.GetDefaultName(), times, remainingNumber)}</align>";
                    AddChatPatch.ChatInformation(crook, ModTranslation.GetString("CrookName"), chatText, "#60a1bd");

                    // [x]MEMO : 此処で勝利フラグを建てる // [x]MEMO : 勝利実行はスポーン時, この時に死んでいたら(会議キルを受けたら)実行されない
                }
            }
        }

        /// <summary>
        /// 詐欺師全体で勝利条件を満たしている者がいるかを取得し, 満たしていたら勝利処理を実行させる。
        /// </summary>
        internal static void CheckWinWrapUp()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            // 勝利条件を誰も満たしていなかったら以下を読まない。(無駄にforeach処理を読まない様, 会議開始時に記録した物を利用し, 2段階で判定している。)
            if (!RoleData.IsWinFlag.Item1) return;

            foreach (KeyValuePair<byte, byte> kvp in RecordOfTimesInsuranceClaimsAreReceived)
            {
                bool privateWinFlag = kvp.Value >= RoleData.NumberNeededWin; // 詐欺師個人の勝利判定の取得
                if (!privateWinFlag) continue; //

                var crook = ModHelpers.GetPlayerControl(kvp.Key);
                if (crook.IsDead()) continue; // 保険金受給時 (追放処理時) に死亡している 詐欺師は勝利不可。

                RoleData.IsWinFlag.Item2 = true; // 追放画面に遷移し最後の保険金を受け取れた。
                Logger.Info($"最後の保険金の受給にたどり着いた 詐欺師が存在した", "IsWinFlag.Item2");
                break;
            }

            if (!RoleData.IsWinFlag.Item2) return;
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
            writer.Write((byte)CustomGameOverReason.CrookWin);
            writer.EndRPC();
            RPCProcedure.SetWinCond((byte)CustomGameOverReason.CrookWin);

            var reason = (GameOverReason)CustomGameOverReason.CrookWin;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) reason = GameOverReason.ImpostorByKill;
            CheckGameEndPatch.CustomEndGame(reason, false);
        }

        /// <summary>
        /// 保険金の受給の有無を, 保存した辞書から取得し, アナウンスを作成する。
        /// </summary>
        /// <returns>bool : 保険金の受給の有無 / string : 保険金を受給した旨のアナウンステキスト </returns>
        internal static (bool, string) GetIsReceivedTheInsuranceAndAnnounce() // [x]MEMO : 実行はパン屋.csで行う, 実行の有無と文字渡す
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

        internal static class Button
        {
            // |:========== ボタン関係の変数管理 ==========:|
            private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CrookButton.png", 115f);
            private static bool AllladyDead;

            internal static void ClearAndReload()
            {
                AllladyDead = false;
            }

            // |:========================================:|

            internal static void MeetingHudStartPostfix(MeetingHud __instance) // [x]MEMO : (SHR, SNR双方で)ホストが動かす, 保険金受け取りのコードは此処にはおかない。 <= meetingStartに詐欺師の処理が2つある
                => ButtonCreate(__instance); // 詐欺師本人のみ実行するコード

            private static void ButtonCreate(MeetingHud __instance)
            {
                if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
                if (!PlayerControl.LocalPlayer.IsRole(RoleId.Crook)) return;
                if (PlayerControl.LocalPlayer.IsDead())
                {
                    AllladyDead = true;
                    return;
                }

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
                var target = ModHelpers.GetPlayerControl(TargetId);
                if (target.IsDead())
                {
                    // 既に死亡していて保険をかけられなかった場合, チャットで警告を行う。
                    FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(ModTranslation.GetString("CrookErrorTargetAllladyDead"), target.name));
                    return;
                }

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
                if (AllladyDead) return; // 会議開始時点で死亡していた場合, 以下の処理を実行しないようにする。

                if (PlayerControl.LocalPlayer.IsDead()) // [ ]MEMO : + 時間切れ時のボタン消去処理 (能力使用後のボタン消去はボタンの実行処理の中で行っている)
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
    }
}