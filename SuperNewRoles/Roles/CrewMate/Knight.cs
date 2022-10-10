using HarmonyLib;
using Hazel;
using UnityEngine;
using SuperNewRoles.Mode.SuperHostRoles;
using System.Collections.Generic;
using System.Linq;
using static SuperNewRoles.Modules.CustomOptions;
using static SuperNewRoles.Roles.CrewMate.Knight;

namespace SuperNewRoles.Roles.CrewMate
{
    public static class Knight
    {
        private const int OptionId = 1052;// 設定のId
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
        public static PlayerControl ProtectedPlayer; // 護衛対象者
        public static Color32 color = new(229, 228, 230, byte.MaxValue);
        public static bool CanProtect;
        public static float Times;
        public static int NumberOfShieldsRemaining; // シールドの枚数を取得
        public static List<byte> GuardedPlayers;
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
            GuardedPlayers = new();

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
            var Target = ModHelpers.PlayerById((byte)Index);
            var TargetID = Target.PlayerId;
            var LocalID = CachedPlayer.LocalPlayer.PlayerId;
            if (GuardedPlayers.Contains(TargetID)) return;

            RPCProcedure.KnightProtected(LocalID, TargetID);

            MessageWriter ProtectWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.KnightProtected, SendOption.Reliable, -1);
            ProtectWriter.Write(LocalID);
            ProtectWriter.Write(TargetID);
            AmongUsClient.Instance.FinishRpcImmediately(ProtectWriter);
            if (KnightSetTheUpperLimitOfTheGuarding.GetBool())
            {
                Times--;
                SuperNewRolesPlugin.Logger.LogInfo($"護衛残り回数は{Times}回です");
            }
            NumberOfShieldsRemaining++;
            ProtectedPlayer = Target;

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
                        int copiedIndex = player.PlayerId;
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
        /// 騎士の護衛対象者がキルを受けた場合、シールドに関わる変数を初期化する。
        /// </summary>
        public static class MurderPlayerPatch
        {
            // 会議中にキルを受けていない場合、
            // 護衛時に記録した護衛対象者に、会議終了後護衛を張り直す制御をしている為、キル時に変数をリセットする必要がある。
            public static void Postfix([HarmonyArgument(0)] PlayerControl target)
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Knight) && ProtectedPlayer == target)
                {
                    SuperNewRolesPlugin.Logger.LogInfo($"[Knight] {ProtectedPlayer.GetDefaultName()}がキルを受けた為、シールドに関わる変数を初期化します。");
                    ProtectedPlayer = null;
                    NumberOfShieldsRemaining--;
                    SuperNewRolesPlugin.Logger.LogInfo($"[Knight] rotectedPlayer = {ProtectedPlayer},NumberOfShieldsRemaining = {NumberOfShieldsRemaining} : 初期化しました。");
                }
            }
        }
        /// <summary>
        /// 会議終了時、騎士が次回会議で守護を利用可能に戻す。
        /// 会議時にキルが起きていなかった場合張り直す。
        /// </summary>
        public static void WrapUp()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.Knight))
            {
                if (ProtectedPlayer != null)
                {
                    SuperNewRolesPlugin.Logger.LogInfo($"[Knight] 会議終了時に守護が残っている為付与し直します。");
                    var TargetID = ProtectedPlayer.PlayerId;
                    var LocalID = CachedPlayer.LocalPlayer.PlayerId;

                    RPCProcedure.KnightProtected(LocalID, TargetID);

                    MessageWriter ProtectWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.KnightProtected, SendOption.Reliable, -1);
                    ProtectWriter.Write(LocalID);
                    ProtectWriter.Write(TargetID);
                    AmongUsClient.Instance.FinishRpcImmediately(ProtectWriter);
                    SuperNewRolesPlugin.Logger.LogInfo($"[Knight] 会議終了時に守護が残っていた為、{ModHelpers.PlayerById(TargetID).GetDefaultName()}に付与し直しました。");
                }
                CanProtect = true;
                SuperNewRolesPlugin.Logger.LogInfo($"[Knight] CanProtect = {CanProtect} : 護衛可能な状態に戻し、シールド対象およびシールド枚数をリセットしました。");
            }
        }
    }
}