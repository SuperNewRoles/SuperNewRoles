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
        public static void ClearAndReload()
        {
            Player = new();
            TimeForAbilityUse = CustomOptionData.TimeTheAbilityToInsureOthersIsAvailable.GetFloat();
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
        private static Dictionary<byte, byte> RecordOfTimesInsuranceClaimsAreReceived;

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
        /// 保険金の需給の有無を判定 及び 保存する
        /// </summary>
        internal static void SaveReceiptOfInsuranceProceeds()
        {
            // [ ]MEMO : 現状SHRSNR共用可能
            ReceivedTheInsuranceDictionary = new();

            var previousTurn = (byte)(ReportDeadBodyPatch.MeetingTurn_Now - 1);
            if (!SignDictionary.TryGetValue(previousTurn, out var signSituationOfNowTurnDic)) return;

            foreach (KeyValuePair<byte, byte> kvp in signSituationOfNowTurnDic)
            {
                var crookId = kvp.Key;
                var targetId = kvp.Value;
                if (ModHelpers.GetPlayerControl(targetId).IsAlive()) continue;

                SignDictionary[previousTurn][crookId] = targetId;
                ReceivedTheInsuranceDictionary[previousTurn] = targetId; // 現在ターンの需給の情報を保存

                if (RecordOfTimesInsuranceClaimsAreReceived.TryGetValue(previousTurn, out var times))
                {
                    times++;
                    RecordOfTimesInsuranceClaimsAreReceived[crookId] = times;
                }
                else
                {
                    RecordOfTimesInsuranceClaimsAreReceived.Add(crookId, 1);
                }

                if (AmongUsClient.Instance.AmHost)
                {
                    // [x]MEMO : 追放画面で保険金受給の有無をチャット通知
                    string chatText = $"<align={"left"}>{string.Format(ModTranslation.GetString("CrookReceiveSuccessChatAnnounce"), ModHelpers.GetPlayerControl(targetId).GetDefaultName())}</align>";
                    AddChatPatch.ChatInformation(ModHelpers.GetPlayerControl(crookId), ModTranslation.GetString("CrookName"), chatText, "#60a1bd");
                }

                // [ ]MEMO : 此処で勝利フラグを建てる // [ ]MEMO : 勝利実行はスポーン時, この時に死んでいたら(会議キルを受けたら)実行されない
            }
        }

        /// <summary>
        /// 保険金の需給の有無を, 保存した辞書から取得し, アナウンスを作成する。
        /// </summary>
        /// <returns>bool : 保険金の需給の有無 / string : 保険金を受給した旨のアナウンステキスト </returns>
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

            internal static void MeetingHudStartPostfix(MeetingHud __instance) // [ ]MEMO : (SHR, SNR双方で)ホストが動かす, 保険金受け取りのコードは此処にはおかない。
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