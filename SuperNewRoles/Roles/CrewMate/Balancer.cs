using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SuperNewRoles.Roles.Crewmate;

public static class Balancer
{
    private const int OptionId = 400600;
    public static CustomRoleOption BalancerOption;
    public static CustomOption BalancerPlayerCount;
    public static CustomOption BalancerVoteTime;
    public static void SetupCustomOptions()
    {
        BalancerOption = CustomOption.SetupCustomRoleOption(OptionId, true, RoleId.Balancer);
        BalancerPlayerCount = CustomOption.Create(OptionId + 1, true, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], BalancerOption);
        BalancerVoteTime = CustomOption.Create(OptionId + 2, true, CustomOptionType.Crewmate, "BalancerVoteTime", 30f, 0f, 180f, 2.5f, BalancerOption);
    }

    public static List<PlayerControl> BalancerPlayer;
    public static Color32 color = new(255, 128, 0, byte.MaxValue);
    public static bool IsAbilityUsed;
    public static void ClearAndReload()
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            InHostMode.ClearAndReload();
        }
        BalancerPlayer = new();
        currentAbilityUser = null;
        CurrentState = BalancerState.NotBalance;
        IsDoubleExile = false;
        currentTarget = null;
        IsAbilityUsed = false;
    }
    public static PlayerControl currentAbilityUser;
    public static SpriteRenderer BackObject;
    public static SpriteRenderer BackPictureObject;
    public static List<(SpriteRenderer, float, int)> ChainObjects;
    public static Sprite BackSprite => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Balancer.MeetingBack.png", 115f);
    static SpriteRenderer eyebackrender;
    static SpriteRenderer eyerender;
    static TextMeshPro textuseability;
    static TextMeshPro textpleasevote;
    static float textpleasetimer;
    public enum BalancerState
    {
        NotBalance,
        Animation_Chain,
        Animation_Eye,
        Animation_Open,
        WaitVote
    }
    public static BalancerState CurrentState = BalancerState.NotBalance;
    static List<Sprite> chainsprites = new();
    static int animIndex;
    static int pleasevoteanimIndex;
    static float rotate;
    static float openMADENOtimer;
    public static void Update()
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        if (BackObject != null)
        {
            //切断したなら
            if (targetplayerleft == null || targetplayerright == null || targetplayerleft.IsDead() || targetplayerright.IsDead())
            {
                PlayerControl target = null;
                if (targetplayerright == null || targetplayerright.IsDead())
                {
                    target = targetplayerleft;
                }
                if (targetplayerleft == null || targetplayerleft.IsDead())
                {
                    target = targetplayerright;
                }
                if (AmongUsClient.Instance.AmHost) MeetingHud.Instance.RpcVotingComplete(new List<MeetingHud.VoterState>().ToArray(), target.Data, false);
                return;
            }
            switch (CurrentState)
            {
                case BalancerState.NotBalance:
                    return;
                case BalancerState.Animation_Chain:
                    bool flag = true;
                    for (int i = 0; i <= animIndex; i++)
                    {
                        var cobj = ChainObjects[i];
                        if (cobj.Item3 < chainsprites.Count)
                        {
                            cobj.Item1.sprite = chainsprites[ChainObjects[i].Item3];
                            ChainObjects[i] = (cobj.Item1, cobj.Item2, cobj.Item3 + 1);
                            flag = false;
                        }
                    }
                    if ((animIndex + 1) < ChainObjects.Count)
                        animIndex++;
                    if (flag)
                    {
                        textpleasetimer -= Time.fixedDeltaTime;
                        if (textpleasetimer <= 0)
                        {
                            CurrentState = BalancerState.Animation_Eye;
                            animIndex = 0;
                            BackObject.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Balancer.FlareEffect.png", 115f);
                            BackObject.color = new Color32(255, 255, 255, 0);
                            eyerender.enabled = true;
                            eyerender.color = new Color32(255, 255, 255, 0);
                            eyebackrender.enabled = true;
                            eyebackrender.color = new Color32(255, 255, 255, 0);
                            textuseability.enabled = true;
                            textuseability.color = new Color32(255, 255, 255, 0);
                            rotate = 360;
                            textpleasetimer = 0.8f;
                            pleasevoteanimIndex = 0;
                            //なんか分からんけどピッチが変だから0.9倍にして解決！(無理やり)
                            ModHelpers.PlaySound(MeetingHud.Instance.transform, ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Balancer.backsound.raw"), false).pitch = 0.9f;
                        }
                    }
                    break;
                case BalancerState.Animation_Eye:
                    animIndex++;
                    if (animIndex <= 40)
                    {
                        byte alpha = 255;
                        if (animIndex * 6.2f < 255)
                        {
                            alpha = (byte)(animIndex * 6.2f);
                        }
                        if (BackObject != null) BackObject.color = new Color32(255, 255, 255, alpha);
                        if (eyerender != null) eyerender.color = new Color32(255, 255, 255, alpha);
                        if (eyebackrender != null) eyebackrender.color = new Color32(255, 255, 255, alpha);
                        if (textuseability != null) textuseability.color = new Color32(255, 255, 255, alpha);
                    }
                    else
                    {
                        if (textpleasetimer > 0)
                        {
                            textpleasetimer -= Time.fixedDeltaTime;
                            if (textpleasetimer <= 0)
                            {
                                if (textpleasevote != null) textpleasevote.enabled = true;
                                if (textpleasevote != null) textpleasevote.color = new Color32(255, 255, 255, 0);
                            }
                        }
                        else if (pleasevoteanimIndex <= 20)
                        {
                            pleasevoteanimIndex++;
                            byte alpha = 255;
                            if (pleasevoteanimIndex * 13f < 255)
                            {
                                alpha = (byte)(pleasevoteanimIndex * 13f);
                            }
                            if (textpleasevote != null) textpleasevote.color = new Color32(255, 255, 255, alpha);
                            if (pleasevoteanimIndex > 20)
                            {
                                openMADENOtimer = 1f;
                            }
                        }
                        else
                        {
                            openMADENOtimer -= Time.fixedDeltaTime;
                            if (openMADENOtimer <= 0)
                            {
                                CurrentState = BalancerState.Animation_Open;
                                animIndex = 0;
                                BackPictureObject.enabled = true;
                                BackPictureObject.color = new Color32(255, 255, 255, 0);
                            }
                        }
                    }
                    if (eyebackrender != null) eyebackrender.transform.localEulerAngles = new(0, 0, rotate);
                    rotate -= 0.1f;
                    if (rotate <= 0)
                    {
                        rotate = 360;
                    }
                    break;
                case BalancerState.Animation_Open:
                    animIndex++;
                    if (animIndex <= 20)
                    {
                        byte alpha = 255;
                        if (animIndex * 16f < 255)
                        {
                            alpha = (byte)(animIndex * 16f);
                        }
                        if (BackPictureObject != null) BackPictureObject.color = new Color32(255, 255, 255, alpha);
                    }
                    Vector3 speed = new(0.6f, 0, 0);
                    foreach (var objs in ChainObjects)
                    {
                        objs.Item1.transform.localPosition -= speed;
                    }
                    eyebackrender.transform.localPosition -= speed;
                    eyerender.transform.localPosition -= speed;
                    BackObject.transform.localPosition -= speed;
                    textpleasevote.transform.localPosition -= speed;
                    textuseability.transform.localPosition -= speed;
                    if (BackObject.transform.localPosition.x <= -10)
                    {
                        CurrentState = BalancerState.WaitVote;
                        SetActiveMeetingHud(true);
                        MeetingHud.Instance.transform.FindChild("MeetingContents/PhoneUI/baseGlass").transform.localPosition = new(0.012f, 0, 0);
                        MeetingHud.Instance.TitleText.GetComponent<TextTranslatorTMP>().enabled = false;
                        MeetingHud.Instance.TitleText.transform.localPosition = new(0, 2, -1);
                        MeetingHud.Instance.TitleText.transform.localScale = Vector3.one * 2f;
                        MeetingHud.Instance.TitleText.text = titletext;
                        leftplayerarea.transform.localPosition = leftpos;
                        rightplayerarea.transform.localPosition = rightpos;
                        MeetingHud.Instance.discussionTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(AmongUs.GameOptions.Int32OptionNames.VotingTime) - BalancerVoteTime.GetFloat();
                        MeetingHud.Instance.TimerText.gameObject.SetActive(true);
                        MeetingHud.Instance.TimerText.transform.localPosition = new(2.05f, -2, -1);
                        MeetingHud.Instance.ProceedButton.transform.localPosition = new(3.5f, -2, -1.05f);
                    }
                    break;
            }
        }
    }
    public static PlayerControl targetplayerleft;
    static PlayerVoteArea leftplayerarea;
    static readonly Vector3 leftpos = new(-2.9f, 0, -0.9f);
    public static PlayerControl targetplayerright;
    static PlayerVoteArea rightplayerarea;
    static readonly Vector3 rightpos = new(2.3f, 0, -0.9f);
    public static bool IsDoubleExile;
    static PlayerControl currentTarget;
    static string[] titletexts =
        new string[] {
            "BalancerTitleTextEither",
            "BalancerTitleTextAverage",
            "BalancerTitleTextYouVoteEither",
            "BalancerTitleTextEitherExile",
            "BalancerTitleTextWhoIsImpostor"
        };
    static string titletext => ModTranslation.GetString(ModHelpers.GetRandom(titletexts));
    static void SetActiveMeetingHud(bool active)
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        MeetingHud.Instance.TitleText.gameObject.SetActive(active);
        MeetingHud.Instance.TimerText.gameObject.SetActive(active);
        if (!active)
        {
            MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(active);
            MeetingHud.Instance.SkippedVoting.SetActive(active);
        }
    }
    public static void WrapUp(PlayerControl exiled)
    {
        if (exiled != null)
        {
            if (IsDoubleExile && exiled.PlayerId == targetplayerleft.PlayerId) return;
        }
        targetplayerright = null;
        targetplayerleft = null;
        IsDoubleExile = false;
        currentAbilityUser = null;
        CurrentState = BalancerState.NotBalance;
        currentTarget = null;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            InHostMode.AfterMeetingTasks();
        }
    }
    public static void StartAbility(PlayerControl source, PlayerControl player1, PlayerControl player2)
    {
        if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
        MeetingHud.Instance.discussionTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(AmongUs.GameOptions.Int32OptionNames.VotingTime) - BalancerVoteTime.GetFloat() - 6.5f;
        currentAbilityUser = source;
        targetplayerleft = player1;
        targetplayerright = player2;
        CurrentState = BalancerState.Animation_Chain;
        MeetingHud.Instance.ClearVote();
        foreach (PlayerVoteArea area in MeetingHud.Instance.playerStates)
        {
            if (area.TargetPlayerId == targetplayerleft.PlayerId)
            {
                area.transform.localPosition = new(999, 999, 999);
                leftplayerarea = area;
            }
            else if (area.TargetPlayerId == targetplayerright.PlayerId)
            {
                area.transform.localPosition = new(999, 999, 999);
                rightplayerarea = area;
            }
            else
                area.gameObject.SetActive(false);
        }
        //後で表示する
        SetActiveMeetingHud(false);

        BackObject = new GameObject("BackObject").AddComponent<SpriteRenderer>();
        BackObject.transform.parent = MeetingHud.Instance.transform;
        // UIレイヤーに移動
        BackObject.gameObject.layer = 5;
        // 位置移動
        BackObject.transform.localPosition = new(0, 0, -11);
        BackObject.transform.localScale = new(2f, 2f, 2f);

        BackPictureObject = new GameObject("BackPictureObject").AddComponent<SpriteRenderer>();
        BackPictureObject.transform.parent = MeetingHud.Instance.transform;
        // UIレイヤーに移動
        BackPictureObject.gameObject.layer = 5;
        // 位置移動
        BackPictureObject.transform.localPosition = new(0, 0, -0.1f);
        BackPictureObject.transform.localScale = Vector3.one * 1.65f;
        //初期化
        BackPictureObject.enabled = false;
        BackPictureObject.sprite = BackSprite;

        // アニメーションの初期化
        animIndex = 0;
        if (chainsprites.Count <= 0)
        {
            for (int i = 0; i < 15; i++)
            {
                chainsprites.Add(ModHelpers.LoadSpriteFromResources($"SuperNewRoles.Resources.Balancer.chain.average_anim_chain_0{i + 16}.png", 115f));
            }
        }
        eyebackrender = new GameObject("EyeBackRender").AddComponent<SpriteRenderer>();
        eyebackrender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Balancer.eye-of-horus_parts.png", 115f);
        eyebackrender.enabled = false;
        eyebackrender.gameObject.layer = 5;
        eyebackrender.transform.parent = MeetingHud.Instance.transform;
        eyebackrender.transform.localScale = new(1.7f, 1.7f, 1.7f);
        eyebackrender.transform.localPosition = new(0, 0, -30f);
        eyerender = new GameObject("EyeRender").AddComponent<SpriteRenderer>();
        eyerender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Balancer.eye-of-horus.png", 115f);
        eyerender.enabled = false;
        eyerender.gameObject.layer = 5;
        eyerender.transform.parent = MeetingHud.Instance.transform;
        eyerender.transform.localScale = new(1.7f, 1.7f, 1.7f);
        eyerender.transform.localPosition = new(0, 0.25f, -30f);
        ChainObjects = new();
        int objectnum = 11;
        for (int i = 0; i < objectnum; i++)
        {
            ChainObjects.Add((createchain(Random.Range(1.8f, -1.7f), Random.Range(-15f, 15f)), 0f, 0));
        }
        ChainObjects.Add((createchain(0, 0, -12f), 0f, 0));
        textuseability = createtext(new(0, 2.1f, -30), ModTranslation.GetString("BalancerAbilityUseText"), 12);
        textuseability.enabled = false;
        textpleasevote = createtext(new(0, -1f, -30f), ModTranslation.GetString("BalancerVoteText"), 8);
        textpleasevote.enabled = false;
        textpleasetimer = 0.35f;
        SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Balancer.chain.raw"), false);
    }
    static TextMeshPro createtext(Vector3 pos, string text, float fontsize)
    {
        TextMeshPro tmp = GameObject.Instantiate(MeetingHud.Instance.TitleText, MeetingHud.Instance.transform);
        tmp.text = text;
        tmp.gameObject.gameObject.layer = 5;
        tmp.transform.localScale = Vector3.one;
        tmp.transform.localPosition = pos;
        tmp.fontSize = fontsize;
        tmp.fontSizeMax = fontsize;
        tmp.fontSizeMin = fontsize;
        tmp.enableWordWrapping = false;
        tmp.gameObject.SetActive(true);
        GameObject.Destroy(tmp.GetComponent<TextTranslatorTMP>());
        return tmp;
    }
    static SpriteRenderer createchain(float pos, float rotate, float zpos = 7f)
    {

        SpriteRenderer obj = new GameObject("Chain").AddComponent<SpriteRenderer>();
        obj.transform.parent = MeetingHud.Instance.transform;
        // UIレイヤーに移動
        obj.gameObject.layer = 5;
        // 位置移動
        obj.transform.localPosition = new(0, pos, zpos);
        obj.transform.localScale = new(2f, 1.7f, 2f);
        obj.transform.Rotate(new(0, 0, rotate));
        return obj;
    }

    internal class Balancer_updatepatch
    {
        internal static void UpdateButtonsPostfix(MeetingHud __instance)
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            if (PlayerControl.LocalPlayer.IsDead())
            {
                __instance.playerStates.ForEach(x => { if (x.transform.FindChild("BalancerButton") != null) Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }
            if (currentAbilityUser != null)
            {
                foreach (PlayerVoteArea area in MeetingHud.Instance.playerStates)
                {
                    if (area.TargetPlayerId != targetplayerleft.PlayerId &&
                        area.TargetPlayerId != targetplayerright.PlayerId)
                        area.gameObject.SetActive(false);
                }
            }
        }
    }
    public static class Balancer_Patch
    {
        private static string nameData;
        static void BalancerOnClick(int Index, MeetingHud __instance)
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            if (currentAbilityUser != null) return;
            var Target = ModHelpers.PlayerById(__instance.playerStates[Index].TargetPlayerId);
            if (currentTarget == null)
            {
                currentTarget = Target;
                __instance.playerStates.ForEach(x => { if (x.TargetPlayerId == currentTarget.PlayerId && x.transform.FindChild("BalancerButton") != null) x.transform.FindChild("BalancerButton").gameObject.SetActive(false); });
                return;
            }
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.BalancerBalance);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(currentTarget.PlayerId);
            writer.Write(Target.PlayerId);
            writer.EndRPC();
            RPCProcedure.BalancerBalance(PlayerControl.LocalPlayer.PlayerId, currentTarget.PlayerId, Target.PlayerId);
            IsAbilityUsed = true;
            __instance.playerStates.ForEach(x => { if (x.transform.FindChild("BalancerButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("BalancerButton").gameObject); });
        }
        static void Event(MeetingHud __instance)
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            if (PlayerControl.LocalPlayer.IsAlive() && !IsAbilityUsed)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
                    if (player.IsAlive())
                    {
                        GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = Object.Instantiate(template, playerVoteArea.transform);
                        targetBox.name = "BalancerButton";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Balancer.icon_average.png", 115f);
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        int copiedIndex = i;
                        button.OnClick.AddListener((UnityAction)(() => BalancerOnClick(copiedIndex, __instance)));
                    }
                }
            }
        }

        internal static void MeetingHudStartPostfix(MeetingHud __instance)
        {
            if (ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            Event(__instance);
        }
    }
    public static class InHostMode
    {
        private static Dictionary<byte, int> NumOfBalance = new();
        private static Dictionary<byte, BalancerState> State = new();
        private static int OptionNumOfBalance = 1;
        private static SHRBalancerState CurrentState = SHRBalancerState.NotBalance;

        private class BalancerState
        {
            public bool selecting = false;
            public byte target1 = byte.MaxValue;
            public byte target2 = byte.MaxValue;
        }
        enum SHRBalancerState
        {
            NotBalance,
            ForBalancerMeeting,
            BalancerMeeting,
        }

        public static void ClearAndReload()
        {
            State.Clear();
            NumOfBalance.Clear();

            CurrentState = SHRBalancerState.NotBalance;
            currentAbilityUser = null;
            targetplayerleft = null;
            targetplayerright = null;
        }
        /// <summary>
        /// 投票形式による天秤の対象指定
        /// </summary>
        /// <param name="balancerId">投票者のplayerId</param>
        /// <param name="targetId">投票先のplayerId</param>
        /// <returns> true : 投票を反映する / false : 投票を反映しない </returns>
        internal static bool MeetingHudCastVote_Prefix(byte balancerId, byte targetId)
        {
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return true;
            if (!AmongUsClient.Instance.AmHost) return true;

            if (RoleClass.Assassin.TriggerPlayer != null) return true;
            if (CurrentState != SHRBalancerState.NotBalance) return true;

            var balancer = ModHelpers.GetPlayerControl(balancerId);
            if (balancer == null || balancer.GetRole() != RoleId.Balancer) return true;

            if (!NumOfBalance.TryGetValue(balancerId, out var numOfBalance))
            {
                NumOfBalance[balancerId] = numOfBalance = OptionNumOfBalance;
            }
            if (numOfBalance <= 0) return true;

            if (!State.TryGetValue(balancerId, out var state))
            {
                State[balancerId] = state = new();
            }

            if (state.selecting)
            {
                if (BotManager.IsBot(targetId)) return false;

                if (targetId is 252 or 253)
                {
                    //スキップで選択モード解除
                    state.selecting = false;
                    state.target1 = byte.MaxValue;
                    state.target2 = byte.MaxValue;
                    SendChat(balancer, ModTranslation.GetString("BalancerSelectionCancelText"), ModTranslation.GetString("BalancerSelectionCancel"));
                    return false;
                }
                if (targetId == balancerId)
                {
                    //選択モードでの自投票は通常投票（自分へ投票）として選択モード解除 & 投票完了
                    state.selecting = false;
                    state.target1 = byte.MaxValue;
                    state.target2 = byte.MaxValue;
                    SendChat(balancer, ModTranslation.GetString("BalancerSelectionSelfSelectText"), ModTranslation.GetString("BalancerSelectionCancel"));
                    return true;
                }

                if (state.target1 == byte.MaxValue || state.target1 == targetId)
                {
                    state.target1 = targetId;
                    Logger.Info($"BalancerSetTarget1 target: {targetId}", "Balancer.MeetingHudCastVote_Prefix");
                }
                else
                {
                    state.target2 = targetId;
                    Logger.Info($"BalancerSetTarget2 target: {targetId}", "Balancer.MeetingHudCastVote_Prefix");
                }

                if (state.target1 == byte.MaxValue)
                {
                    SendChat(balancer, $"{ModTranslation.GetString("BalancerSelectionText")}\n{ModTranslation.GetString("Balancer1st")}", ModTranslation.GetString("BalancerSelection"));
                    return false;
                }
                if (state.target2 == byte.MaxValue)
                {
                    var pc = ModHelpers.GetPlayerControl(state.target1);
                    SendChat(balancer, $"{ModTranslation.GetString("BalancerSelectionText")}\n{ModTranslation.GetString("Balancer1st")}：{pc?.name}\n\n{ModTranslation.GetString("Balancer2nd")}", ModTranslation.GetString("BalancerSelection"));
                    return false;
                }

                CurrentState = SHRBalancerState.ForBalancerMeeting;
                StartBalancerAbility(balancerId, state.target1, state.target2);

                return false;
            }
            if (targetId == byte.MaxValue) return true;

            if (balancerId == targetId)
            {
                //自投票で選択モード開始

                state.selecting = true;
                //Utils.SendMessage(Translator.GetString("message"), Player.PlayerId);
                SendChat(balancer, $"{ModTranslation.GetString("BalancerSelectionText")}\n{ModTranslation.GetString("Balancer1st")}", ModTranslation.GetString("BalancerSelection"));
                Logger.Info($"BalancerSelectStart balancer: {balancerId}", "Balancer.MeetingHudCastVote_Prefix");

                return false;
            }

            return true;
        }
        private static void StartBalancerAbility(byte balancerId, byte target1Id, byte target2Id)
        {
            //会議終了-天秤会議開始間に見えるゲーム画面の視界範囲を0にするためにプレイヤー位置を変更（視点は固定）
            PlayerControl.AllPlayerControls.ToArray().Where(x => x.IsAlive()).Do(x => x.RpcSnapTo(new(-30, 30)));

            _ = new LateTask(() => SwitchBalancerMeeting(balancerId, target1Id, target2Id), 0.3f, "SwitchBalancerMeeting");

            Logger.Info($"StartAbility balancer: {currentAbilityUser?.name}, target: {targetplayerleft?.name}, {targetplayerright?.name}", "Balancer.StartBalancerAbility");
        }
        private static void SwitchBalancerMeeting(byte balancerId, byte target1Id, byte target2Id)
        {
            MeetingHud.Instance.Despawn();

            _ = new LateTask(() => StartBalancerMeeting(balancerId, target1Id, target2Id), 0.3f, "StartBalancerMeeting");
        }
        private static void StartBalancerMeeting(byte balancerId, byte target1Id, byte target2Id)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;

            if (MeetingHud.Instance)
            {
                _ = new LateTask(() => StartBalancerMeeting(balancerId, target1Id, target2Id), 0.5f, "BalancerMeeting");
                return;
            }

            currentAbilityUser = ModHelpers.GetPlayerControl(balancerId);
            targetplayerleft = ModHelpers.GetPlayerControl(target1Id);
            targetplayerright = ModHelpers.GetPlayerControl(target2Id);

            //強制会議
            CurrentState = SHRBalancerState.BalancerMeeting;
            MeetingRoomManager.Instance.AssignSelf(PlayerControl.LocalPlayer, null);
            FastDestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(PlayerControl.LocalPlayer);
            PlayerControl.LocalPlayer.RemainingEmergencies++;
            PlayerControl.LocalPlayer.RpcStartMeeting(null);
        }
        public static void StartMeeting()
        {
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            if (!AmongUsClient.Instance.AmHost) return;

            Logger.Info($"StartMeeting Balancer: {currentAbilityUser?.name}", "Balancer.OnStartMeeting");
            if (CurrentState == SHRBalancerState.BalancerMeeting)
            {
                StartMeetingBalancer();
            }
            else
            {
                if (RoleClass.Assassin.TriggerPlayer != null) return;

                StartMeetingNomal();
            }
        }
        private static void StartMeetingNomal()
        {
            new LateTask(() =>
            {
                foreach (var player in PlayerControl.AllPlayerControls.ToArray().Where(x => x != null && !x.Data.IsDead && x.IsRole(RoleId.Balancer)))
                {
                    if (!State.TryGetValue(player.PlayerId, out var state))
                    {
                        State[player.PlayerId] = state = new();
                    }
                    if (!NumOfBalance.TryGetValue(player.PlayerId, out var numOfBalance))
                    {
                        NumOfBalance[player.PlayerId] = numOfBalance = OptionNumOfBalance;
                    }

                    if (numOfBalance > 0)
                        SendChat(player, ModTranslation.GetString("BalancerForActivate"), ModTranslation.GetString("BalancerName"));
                    else
                        SendChat(player, ModTranslation.GetString("BalancerUsed"), ModTranslation.GetString("BalancerName"));
                }
            }, 3f, "StartMeeting BalancerGuide");
        }
        private static void StartMeetingBalancer()
        {
            //天秤会議
            if (!NumOfBalance.TryGetValue(currentAbilityUser.PlayerId, out var num))
            {
                num = OptionNumOfBalance;
            }
            NumOfBalance[currentAbilityUser.PlayerId] = --num;
            Logger.Info($"BalancerMeetingStart Balancer: {currentAbilityUser?.name}, num: {num}, target: {targetplayerleft?.name}, {targetplayerright?.name}", "Balancer.OnStartMeeting");
            //BalancerMeeting = true;

            string decoration = $"<color=#ff8000><size=80%>～~*~≢⊕～~*~≢⊕～~*~≢⊕～~*~≢⊕～</size></color>\n" +
                                $"<color=#ff8000><size=150%>【{ModTranslation.GetString("BalancerMeeting")}】</size></color>\n" +
                                $"<color=#ff8000><size=100%>★△☀ </size></color><color=#fff200><size=200%>{ModTranslation.GetString("BalancerAbilityUseText")}</size></color><color=#ff8000><size=100%> ◎▲☆ </size></color>\n" +
                                $"<color=#ff8000><size=150%>{ModTranslation.GetString("BalancerVoteText")}</color>\n" +
                                $"<color=#ff8000><size=80%>～~*~≢⊕～~*~≢⊕～~*~≢⊕～~*~≢⊕～</size></color>";

            string targetText1 = $"{Palette.GetColorName(targetplayerleft.Data.DefaultOutfit.ColorId)} {targetplayerleft.name}";
            string targetText2 = $"{Palette.GetColorName(targetplayerright.Data.DefaultOutfit.ColorId)} {targetplayerright.name}";

            //string dispText = $"バランスを求めよ。\n\n" +
            string dispText = ModHelpers.Cs(Palette.PlayerColors[targetplayerleft.Data.DefaultOutfit.ColorId], targetText1) +
                               "\n<color=#ffffff>   vs </color>\n" +
                              ModHelpers.Cs(Palette.PlayerColors[targetplayerright.Data.DefaultOutfit.ColorId], targetText2);

            dispText = $"<size=100%>{dispText}</size>";
            dispText = $"{decoration}\n{dispText}\n";

            SendChat(null, dispText);
        }
        public static void AfterMeetingTasks()
        {
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;
            if (!AmongUsClient.Instance.AmHost) return;

            State.Clear();
            Logger.Info($"AfterMeeting Clear", "Balancer.AfterMeetingTasks");

            if (CurrentState != SHRBalancerState.BalancerMeeting) return;

            CurrentState = SHRBalancerState.NotBalance;
            currentAbilityUser = null;
            targetplayerleft = null;
            targetplayerright = null;
            Logger.Info($"AfterBalancerMeeting Clear", "Balancer.AfterMeetingTasks");
        }
        private static void SendChat(PlayerControl target, string text, string title = "")
        {
            if (title != null && title != "") text = $"<size=100%><color=#ff8000>【{title}】</color></size>\n{text}";
            AddChatPatch.SendCommand(target, "", text);
        }
        public static void SetMeetingSettings(IGameOptions optdata)
        {
            if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) return;

            if (CurrentState == SHRBalancerState.BalancerMeeting)
            {
                optdata.SetInt(Int32OptionNames.DiscussionTime, 0);
                optdata.SetInt(Int32OptionNames.VotingTime, (int)BalancerVoteTime.GetFloat());
            }
            else
            {
                optdata.SetInt(Int32OptionNames.DiscussionTime, optdata.GetInt(Int32OptionNames.DiscussionTime));
                optdata.SetInt(Int32OptionNames.VotingTime, optdata.GetInt(Int32OptionNames.VotingTime));
            }
        }
    }
    // ここにコードを書きこんでください
}