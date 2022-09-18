using SuperNewRoles.Patch;
using static SuperNewRoles.Modules.CustomOptions;
using static SuperNewRoles.Roles.RoleClass;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Mode;
using static SuperNewRoles.Roles.CrewMate.Knight;
using SuperNewRoles.Mode.SuperHostRoles;

namespace SuperNewRoles.Roles.CrewMate
{
    public static class Knight
    {
        public const int OptionId = 992;// 設定のId
        // CustomOptionDate
        public static CustomRoleOption KnightOption;
        public static CustomOption KnightPlayerCount;
        public static CustomOption KnightCanAnnounceOfProtected;
        public static CustomOption KnightSetTheUpperLimitOfTheGuarding;
        public static CustomOption KnightMaximumNumberOfTimes;
        private static Sprite buttonSprite;
        public static void SetupCustomOptions()
        {
            KnightOption = new(OptionId, false, CustomOptionType.Crewmate, "KnightName", color, 1);
            KnightPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], KnightOption);
            KnightCanAnnounceOfProtected = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "KnightCanAnnounceOfProtected", true, KnightOption);
            KnightSetTheUpperLimitOfTheGuarding = CustomOption.Create(OptionId + 3, false, CustomOptionType.Crewmate, "KnightSetTheUpperLimitOfTheGuarding", false, KnightOption);
            KnightMaximumNumberOfTimes = CustomOption.Create(OptionId + 4, false, CustomOptionType.Crewmate, "KnightMaximumNumberOfTimes", 5f, 0f, 30f, 1f, KnightSetTheUpperLimitOfTheGuarding);
        }

        // RoleClass
        public static List<PlayerControl> Player;
        public static PlayerControl ProtectedPlayer;
        public static Color32 color = new(229, 228, 230, byte.MaxValue);
        public static bool CanProtect;
        public static float Times;
        public static int NumberOfShieldsRemaining;
        public static Sprite GetButtonSprite()
        {
            if (buttonSprite) return buttonSprite;
            buttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ProtectAbilityButton.png", 115f);
            return buttonSprite;
        }

        public static void ClearAndReload()
        {
            Player = new();
            Times = KnightMaximumNumberOfTimes.GetFloat(); //最大護衛回数の取得
            CanProtect = true; //護衛を使用可能に変更
            ProtectedPlayer = null;
            NumberOfShieldsRemaining = 0;

            //CustomOptionからのGetBool()は判定が必要な場所でその都度行う為、ここに入れない。
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class KnightProtected_Patch
    {
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
        class KnightProtected_updatepatch
        {
            static void Postfix(MeetingHud __instance)
            {
                //もし プレイヤーが騎士であり尚且つ死んでいる場合
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Knight) && PlayerControl.LocalPlayer.IsDead())
                {
                    KnightProtectButtonDestroy(__instance);
                }
            }
        }
        /// <summary>
        /// 護衛ボタンを押したときの動作。
        /// </summary>
        static void KnightOnClick(int Index, MeetingHud __instance)
        {
            var Target = ModHelpers.PlayerById(__instance.playerStates[Index].TargetPlayerId);
            var TargetID = Target.PlayerId;
            var LocalID = CachedPlayer.LocalPlayer.PlayerId;

            RPCProcedure.RPCKnightProtected(LocalID, TargetID);

            MessageWriter ProtectWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCKnightProtected, SendOption.Reliable, -1);
            ProtectWriter.Write(LocalID);
            ProtectWriter.Write(TargetID);
            AmongUsClient.Instance.FinishRpcImmediately(ProtectWriter);
            if (KnightSetTheUpperLimitOfTheGuarding.GetBool())
            {
                Times--;
                SuperNewRolesPlugin.Logger.LogInfo($"護衛残り回数は{Times}回です");
            }
            CanProtect = false;
            SuperNewRolesPlugin.Logger.LogInfo($"[Knight] CanProtect = {CanProtect} : 護衛を使用済みに変更しました。");
            //もし 護衛可能な上限回数に達している時　または　護衛不可能な状態の場合
            if ((KnightSetTheUpperLimitOfTheGuarding.GetBool() && Times <= 0) || !CanProtect)
            {
                KnightProtectButtonDestroy(__instance);
            }
        }
        static void Event(MeetingHud __instance)
        {
            //もし　プレーヤーが護衛可能な状態の生きている[騎士]であり　且つ
            //護衛可能回数に上限がない状態　又は　護衛可能回数に条件があるが上限に達していない場合なら
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Knight) && PlayerControl.LocalPlayer.IsAlive() && CanProtect && (!KnightSetTheUpperLimitOfTheGuarding.GetBool() || (KnightSetTheUpperLimitOfTheGuarding.GetBool() && Times >= 1)))
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
                    // もし　プレイヤーが生きていて　護衛可能な状態で　そのネームプレートがプレイヤー本人でないなら
                    if (player.IsAlive() && CanProtect && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = Object.Instantiate(template, playerVoteArea.transform);
                        targetBox.name = "KnightProtectButton";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = GetButtonSprite();
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        int copiedIndex = i;
                        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => KnightOnClick(copiedIndex, __instance)));
                        SuperNewRolesPlugin.Logger.LogInfo($"[Knight]{player.GetDefaultName()}に護衛ボタンを表示します。");
                    }
                }
            }
        }
        static void Postfix(MeetingHud __instance)
        {
            Event(__instance);
        }
        /// <summary>
        /// 護衛ボタンを削除します。
        /// </summary>
        static void KnightProtectButtonDestroy(MeetingHud __instance)
        {
            __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("KnightProtectButton") != null) Object.Destroy(x.transform.FindChild("KnightProtectButton").gameObject); });
            SuperNewRolesPlugin.Logger.LogInfo("[Knight] 護衛可能な条件を満たしていない為、護衛ボタンを消去しました。");
        }
        /// <summary>
        /// 騎士の護衛ボタンを使用可能に戻す。
        /// </summary>
        public static void WrapUp()
        {
            CanProtect = true;
            if (ProtectedPlayer != null && NumberOfShieldsRemaining > 0)
            {
                ProtectedPlayer.RpcMurderPlayer(ProtectedPlayer);
                SuperNewRolesPlugin.Logger.LogInfo($"[Knight] {ProtectedPlayer.GetDefaultName()} のシールドが会議開始時にも残っていた為、削除しました。");
            }
            ProtectedPlayer = null;
            NumberOfShieldsRemaining = 0;
            SuperNewRolesPlugin.Logger.LogInfo($"[Knight] CanProtect = {CanProtect} : 護衛可能な状態に戻し、シールド対象およびシールド枚数をリセットしました。");
        }
    }
}