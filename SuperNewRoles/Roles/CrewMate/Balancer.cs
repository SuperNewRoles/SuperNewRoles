using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;
using TMPro;
using HarmonyLib;

namespace SuperNewRoles.Roles.Crewmate;

class Balancer : RoleBase<Balancer>
{
    public override RoleId Role { get; } = RoleId.Balancer;
    public override Color32 RoleColor { get; } = new(255, 128, 0, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new BalancerAbility(BalancerUseCount)];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.PowerPlayResistance];

    // 天秤会議時間
    [CustomOptionFloat("BalancerVoteTime", 0f, 180f, 2.5f, 30f)]
    public static float BalancerVoteTime;

    // 天秤使用回数
    [CustomOptionInt("BalancerUseCount", 1, 10, 1, 1)]
    public static int BalancerUseCount;

    // 未投票時ランダムに投票する
    [CustomOptionBool("BalancerRandomVoteWhenNoVote", true)]
    public static bool BalancerRandomVoteWhenNoVote;

    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

class BalancerAbility : AbilityBase, IAbilityCount
{
    #region Constants
    // アニメーション関連の定数
    private const int CHAIN_SPRITE_COUNT = 15;
    private const int CHAIN_OBJECT_COUNT = 11;
    private const float ANIMATION_FADE_STEP = 6.2f;
    private const float EYE_ROTATION_SPEED = 0.1f;
    private const float FAST_ROTATION_SPEED = 25f;
    private const float CHAIN_DELAY_TIME = 0.35f;
    private const float EYE_WAIT_TIME = 0.8f;
    private const float OPEN_WAIT_TIME = 1f;
    private const float SLIDE_SPEED = 0.6f;

    // UI位置の定数
    private const float LEFT_PLAYER_POS_X = -2.9f;
    private const float RIGHT_PLAYER_POS_X = 2.3f;
    private const float PLAYER_POS_Y = 0f;
    private const float PLAYER_POS_Z = -0.9f;

    private static readonly string[] TitleTexts =
    [
        "BalancerTitleTextEither",
        "BalancerTitleTextAverage",
        "BalancerTitleTextYouVoteEither",
        "BalancerTitleTextEitherExile",
        "BalancerTitleTextWhoIsImpostor",
        "BalancerTitleTextGetBalance",
        "BalancerTitleTextPleaseVote"
    ];
    #endregion

    #region Fields
    private EventListener<MeetingStartEventData> meetingStartEventListener;
    private EventListener<MeetingCloseEventData> meetingCloseEventListener;
    private EventListener fixedUpdateEventListener;
    private EventListener updateEventListener;
    private EventListener<VotingCompleteEventData> votingCompleteEventListener;
    private EventListener<WrapUpEventData> wrapUpEventListener;

    private List<PlayerControl> targetPlayers = new();
    public bool isAbilityUsed = false;
    private PlayerControl targetPlayerLeft;
    private PlayerControl targetPlayerRight;
    private bool isDoubleExile = false;
    public bool isOnePlayerDead = false;

    public static BalancerAbility BalancingAbility { get; private set; }
    public static MeetingHud currentMeetingHud;

    // 天秤ボタン
    private BalancerMeetingButton balancerButton;

    // アニメーション関連のオブジェクト
    public static SpriteRenderer BackObject;
    public static SpriteRenderer BackPictureObject;
    public static List<(SpriteRenderer renderer, float timer, int frameIndex)> ChainObjects;

    // アニメーション状態
    private static List<Sprite> chainSprites = new();
    private static int animIndex;
    private static SpriteRenderer eyeBackRender;
    private static SpriteRenderer eyeRender;
    private static TextMeshPro textUseAbility;
    private static TextMeshPro textPleaseVote;
    private static float textPleaseTimer;
    private static PlayerVoteArea leftPlayerArea;
    private static PlayerVoteArea rightPlayerArea;
    private static float rotate;
    private static float openTimer;
    public static int PleaseVoteAnimIndex;
    private static ExileController additionalExileController;

    // UI関連
    private TextMeshPro limitText;
    private string lastLimitText = "";
    #endregion

    #region Enums
    public enum BalancerState
    {
        NotBalance,
        Animation_Chain,
        Animation_Eye,
        Animation_Open,
        WaitVote
    }
    #endregion

    public BalancerState CurrentState { get; private set; } = BalancerState.NotBalance;

    public BalancerAbility(int useCount)
    {
        Count = useCount;
    }

    #region State Management
    private void ClearAndReload()
    {
        targetPlayers.Clear();
        CurrentState = BalancerState.NotBalance;
        isDoubleExile = false;
        targetPlayerLeft = null;
        targetPlayerRight = null;
        isAbilityUsed = false;
        BalancingAbility = null;
        currentMeetingHud = null;

        if (balancerButton != null)
        {
            balancerButton.firstSelectedTarget = null;
        }

        CleanupAnimationObjects();
    }

    private void CleanupAnimationObjects()
    {
        if (BackObject != null)
        {
            GameObject.Destroy(BackObject.gameObject);
            BackObject = null;
        }

        if (BackPictureObject != null)
        {
            GameObject.Destroy(BackPictureObject.gameObject);
            BackPictureObject = null;
        }

        if (ChainObjects != null)
        {
            foreach (var chainObj in ChainObjects)
            {
                if (chainObj.renderer != null)
                    GameObject.Destroy(chainObj.renderer.gameObject);
            }
            ChainObjects?.Clear();
        }

        if (eyeBackRender != null)
        {
            GameObject.Destroy(eyeBackRender.gameObject);
            eyeBackRender = null;
        }

        if (eyeRender != null)
        {
            GameObject.Destroy(eyeRender.gameObject);
            eyeRender = null;
        }

        if (textUseAbility != null)
        {
            GameObject.Destroy(textUseAbility.gameObject);
            textUseAbility = null;
        }

        if (textPleaseVote != null)
        {
            GameObject.Destroy(textPleaseVote.gameObject);
            textPleaseVote = null;
        }
    }
    #endregion

    #region Lifecycle
    public override void AttachToLocalPlayer()
    {
        updateEventListener = FixedUpdateEvent.Instance.AddListener(Update);
    }

    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);
        meetingStartEventListener = MeetingStartEvent.Instance.AddListener(x => OnMeetingStart());
        meetingCloseEventListener = MeetingCloseEvent.Instance.AddListener(x => OnMeetingClosed());
        fixedUpdateEventListener = FixedUpdateEvent.Instance.AddListener(FixedUpdateAnimation);
        votingCompleteEventListener = VotingCompleteEvent.Instance.AddListener(OnVotingComplete);
        wrapUpEventListener = WrapUpEvent.Instance.AddListener(OnWrapUp);

        balancerButton = new BalancerMeetingButton(this);
        ExPlayerControl exPlayer = (ExPlayerControl)player;
        exPlayer.AddAbility(balancerButton, new AbilityParentAbility(this));
    }

    public override void Detach()
    {
        base.Detach();

        // イベントリスナーを安全に解除
        if (meetingStartEventListener != null)
        {
            MeetingStartEvent.Instance.RemoveListener(meetingStartEventListener);
            meetingStartEventListener = null;
        }
        if (meetingCloseEventListener != null)
        {
            MeetingCloseEvent.Instance.RemoveListener(meetingCloseEventListener);
            meetingCloseEventListener = null;
        }
        if (fixedUpdateEventListener != null)
        {
            FixedUpdateEvent.Instance.RemoveListener(fixedUpdateEventListener);
            fixedUpdateEventListener = null;
        }
        if (updateEventListener != null)
        {
            FixedUpdateEvent.Instance.RemoveListener(updateEventListener);
            updateEventListener = null;
        }
        if (votingCompleteEventListener != null)
        {
            VotingCompleteEvent.Instance.RemoveListener(votingCompleteEventListener);
            votingCompleteEventListener = null;
        }
        wrapUpEventListener?.RemoveListener();

        if (BalancingAbility == this && AmongUsClient.Instance.AmHost)
        {
            EndBalancing();
        }

        if (BalancingAbility == this)
        {
            ClearAndReload();
        }
    }
    #endregion

    #region Update Methods
    private void Update()
    {
        if (limitText == null) return;

        if (Player.IsDead())
        {
            GameObject.Destroy(limitText.gameObject);
            limitText = null;
            lastLimitText = "";
        }
        else
        {
            string newText = ModTranslation.GetString("BalancerLimitText", Count);
            if (lastLimitText != newText)
            {
                limitText.text = newText;
                lastLimitText = newText;
            }
        }
    }

    public void FixedUpdateAnimation()
    {
        if (CurrentState == BalancerState.NotBalance) return;
        if (!IsValidMeetingState()) return;

        if (CheckPlayerStatus()) return;

        if (AmongUsClient.Instance.AmHost && CheckAllPlayersVoted())
        {
            EndBalancing();
            return;
        }

        UpdateAnimationByState();
        if (MeetingHud.Instance?.SkipVoteButton != null) MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
    }

    private bool IsValidMeetingState()
    {
        return MeetingHud.Instance.state is MeetingHud.VoteStates.Discussion
            or MeetingHud.VoteStates.Voted
            or MeetingHud.VoteStates.NotVoted;
    }

    private bool CheckPlayerStatus()
    {
        if (targetPlayerLeft?.Data?.IsDead == true || targetPlayerRight?.Data?.IsDead == true ||
            targetPlayerLeft == null || targetPlayerRight == null)
        {
            PlayerControl target = targetPlayerLeft?.Data?.IsDead != true ? targetPlayerLeft : targetPlayerRight;
            isOnePlayerDead = true;

            if (AmongUsClient.Instance.AmHost && target != null)
            {
                MeetingHud.Instance.RpcVotingComplete(new List<MeetingHud.VoterState>().ToArray(), target.Data, false);
            }
            return true;
        }
        return false;
    }

    private bool CheckAllPlayersVoted()
    {
        return MeetingHud.Instance.playerStates.All(area => area.AmDead || area.DidVote);
    }

    private void UpdateAnimationByState()
    {
        switch (CurrentState)
        {
            case BalancerState.Animation_Chain:
                UpdateChainAnimation();
                break;
            case BalancerState.Animation_Eye:
                UpdateEyeAnimation();
                break;
            case BalancerState.Animation_Open:
                UpdateOpenAnimation();
                break;
            case BalancerState.WaitVote:
                UpdateWaitVoteState();
                break;
        }
    }
    #endregion

    #region Animation Updates
    private void UpdateChainAnimation()
    {
        bool allChainsComplete = true;

        for (int i = 0; i <= animIndex && i < ChainObjects.Count; i++)
        {
            var chainObj = ChainObjects[i];
            if (chainObj.frameIndex < chainSprites.Count)
            {
                chainObj.renderer.sprite = chainSprites[chainObj.frameIndex];
                ChainObjects[i] = (chainObj.renderer, chainObj.timer, chainObj.frameIndex + 1);
                allChainsComplete = false;
            }
        }

        if (animIndex + 1 < ChainObjects.Count)
            animIndex++;

        if (allChainsComplete)
        {
            textPleaseTimer -= Time.fixedDeltaTime;
            if (textPleaseTimer <= 0)
            {
                TransitionToEyeAnimation();
            }
        }
    }

    private void UpdateEyeAnimation()
    {
        animIndex++;

        if (animIndex <= 40)
        {
            byte alpha = (byte)Mathf.Min(255, animIndex * ANIMATION_FADE_STEP);
            SetObjectsAlpha(alpha);
        }
        else
        {
            HandleEyeAnimationPhases();
        }

        UpdateEyeRotation();
    }

    private void UpdateOpenAnimation()
    {
        animIndex++;

        if (animIndex <= 20)
        {
            byte alpha = (byte)Mathf.Min(255, animIndex * 16f);
            SetOpenAnimationAlpha(alpha);
        }

        SlideObjectsLeft();

        if (BackObject.transform.localPosition.x <= -10)
        {
            TransitionToWaitVote();
        }

        UpdateEyeRotation();
    }

    private void UpdateWaitVoteState()
    {
        // プレイヤー以外のエリアを非表示に保つ
        foreach (var area in MeetingHud.Instance.playerStates)
        {
            if (area.TargetPlayerId != targetPlayerLeft.PlayerId &&
                area.TargetPlayerId != targetPlayerRight.PlayerId &&
                area.gameObject.activeSelf)
            {
                area.gameObject.SetActive(false);
            }
        }

        // プレイヤー位置の維持
        if (leftPlayerArea != null)
            leftPlayerArea.transform.localPosition = new(LEFT_PLAYER_POS_X, PLAYER_POS_Y, PLAYER_POS_Z);
        if (rightPlayerArea != null)
            rightPlayerArea.transform.localPosition = new(RIGHT_PLAYER_POS_X, PLAYER_POS_Y, PLAYER_POS_Z);

        UpdateEyeRotation();
    }
    #endregion

    public void OnWrapUp(WrapUpEventData data)
    {
        if (BalancingAbility != null &&
            BalancingAbility.isDoubleExile &&
            ExileController.Instance != additionalExileController &&
            additionalExileController != null)
        {
            if (additionalExileController.initData?.networkedPlayer?.Object != null)
            {
                ((ExPlayerControl)additionalExileController.initData.networkedPlayer.Object).CustomDeath(CustomDeathType.Exile);
            }
            GameObject.Destroy(additionalExileController.gameObject);
            additionalExileController = null;
        }
    }
    public void OnVotingComplete(VotingCompleteEventData data)
    {
        if (data.IsTie && isAbilityUsed)
        {
            isDoubleExile = true;
        }
    }
    public void OnMeetingStart()
    {
        ClearAndReload();
        if (!Player.AmOwner) return;
        limitText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, MeetingHud.Instance.transform);
        limitText.text = ModTranslation.GetString("BalancerLimitText", Count);
        limitText.enableWordWrapping = false;
        limitText.transform.localScale = Vector3.one * 0.5f;
        limitText.transform.localPosition = new(-3.58f, 2.27f, -10);
        limitText.alignment = TMPro.TextAlignmentOptions.Left;
    }

    public void OnMeetingClosed()
    {
        // 会議終了時の処理
        if (limitText != null)
            GameObject.Destroy(limitText.gameObject);
        limitText = null;
    }
    [CustomRPC]
    public static void RpcStartAbility(ExPlayerControl source, PlayerControl player1, PlayerControl player2, ulong abilityId)
    {
        if (source.GetAbility(abilityId) is BalancerAbility ability)
        {
            ability.StartAbility(source, player1, player2);
        }
        else
        {
            Logger.Error($"RpcStartAbility: {source.PlayerId} {player1.PlayerId} {player2.PlayerId} {abilityId}");
        }
    }
    public void StartAbility(PlayerControl source, PlayerControl player1, PlayerControl player2)
    {
        // 基本設定
        targetPlayerLeft = player1;
        targetPlayerRight = player2;
        BalancingAbility = this;
        currentMeetingHud = MeetingHud.Instance;

        PleaseVoteAnimIndex = 0;
        MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
        isAbilityUsed = true;

        // 会議時間を変更
        MeetingHud.Instance.discussionTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.VotingTime) - Balancer.BalancerVoteTime - 6.5f;

        // 天秤会議の開始処理
        CurrentState = BalancerState.Animation_Chain;
        MeetingHud.Instance.ClearVote();
        foreach (PlayerVoteArea area in MeetingHud.Instance.playerStates)
        {
            area.VotedFor = byte.MaxValue;
        }

        // プレイヤー配置の設定
        SetupPlayerAreas();

        // 会議HUDを一時的に非表示
        SetActiveMeetingHud(false);

        // アニメーションオブジェクトの初期化
        InitializeAnimationObjects();

        // 効果音再生
        PlayChainSound();
    }

    private void SetupPlayerAreas()
    {
        foreach (PlayerVoteArea area in MeetingHud.Instance.playerStates)
        {
            if (area.TargetPlayerId == targetPlayerLeft.PlayerId)
            {
                area.transform.localPosition = new(999, 999, 999);
                leftPlayerArea = area;
            }
            else if (area.TargetPlayerId == targetPlayerRight.PlayerId)
            {
                area.transform.localPosition = new(999, 999, 999);
                rightPlayerArea = area;
            }
            else
            {
                area.gameObject.SetActive(false);
            }
        }
    }

    private void InitializeAnimationObjects()
    {
        // 背景オブジェクトの作成
        CreateBackgroundObjects();

        // スプライト初期化
        InitializeChainSprites();

        // 目のオブジェクトの作成
        CreateEyeObjects();

        // 鎖のオブジェクトの作成
        CreateChainObjects();

        // テキスト作成
        CreateTexts();
    }

    private void CreateBackgroundObjects()
    {
        BackObject = new GameObject("BackObject").AddComponent<SpriteRenderer>();
        BackObject.transform.parent = MeetingHud.Instance.transform;
        BackObject.gameObject.layer = 5;
        BackObject.transform.localPosition = new(0, 0, -11);
        BackObject.transform.localScale = new(2f, 2f, 2f);

        BackPictureObject = new GameObject("BackPictureObject").AddComponent<SpriteRenderer>();
        BackPictureObject.transform.parent = MeetingHud.Instance.transform;
        BackPictureObject.gameObject.layer = 5;
        BackPictureObject.transform.localPosition = new(0, 0, -0.1f);
        BackPictureObject.transform.localScale = Vector3.one * 1.65f;
        BackPictureObject.enabled = false;
        BackPictureObject.sprite = AssetManager.GetAsset<Sprite>("BalancerMeetingBack.png");
    }

    private void InitializeChainSprites()
    {
        chainSprites.Clear();
        animIndex = 0;
        Sprite emptySprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);

        for (int i = 0; i < CHAIN_SPRITE_COUNT; i++)
        {
            var sprite = AssetManager.GetAsset<Sprite>($"average_anim_chain_0{i + 17}.png");
            chainSprites.Add(sprite ?? emptySprite);
        }
    }

    private void CreateEyeObjects()
    {
        eyeBackRender = new GameObject("EyeBackRender").AddComponent<SpriteRenderer>();
        eyeBackRender.sprite = AssetManager.GetAsset<Sprite>("BalancerEye-of-horus_parts.png");
        eyeBackRender.enabled = false;
        eyeBackRender.gameObject.layer = 5;
        eyeBackRender.transform.parent = MeetingHud.Instance.transform;
        eyeBackRender.transform.localScale = new(1.7f, 1.7f, 1.7f);
        eyeBackRender.transform.localPosition = new(0, 0, -30f);

        eyeRender = new GameObject("EyeRender").AddComponent<SpriteRenderer>();
        eyeRender.sprite = AssetManager.GetAsset<Sprite>("BalancerEye-of-horus.png");
        eyeRender.enabled = false;
        eyeRender.gameObject.layer = 5;
        eyeRender.transform.parent = MeetingHud.Instance.transform;
        eyeRender.transform.localScale = new(1.7f, 1.7f, 1.7f);
        eyeRender.transform.localPosition = new(0, 0.25f, -30f);
    }

    private void CreateChainObjects()
    {
        ChainObjects = new();

        for (int i = 0; i < CHAIN_OBJECT_COUNT; i++)
        {
            ChainObjects.Add((CreateChainRenderer(UnityEngine.Random.Range(1.8f, -1.7f), UnityEngine.Random.Range(-15f, 15f)), 0f, 0));
        }
        ChainObjects.Add((CreateChainRenderer(0, 0, -12f), 0f, 0));
    }

    private void CreateTexts()
    {
        textUseAbility = CreateText(new(0, 2.1f, -30), ModTranslation.GetString("BalancerAbilityUseText"), 12);
        textUseAbility.enabled = false;
        textPleaseVote = CreateText(new(0, -1f, -30f), ModTranslation.GetString("BalancerVoteText"), 8);
        textPleaseVote.enabled = false;
        textPleaseTimer = CHAIN_DELAY_TIME;
    }

    private void PlayChainSound()
    {
        if (SoundManager.Instance != null)
        {
            AudioClip chainSound = AssetManager.GetAsset<AudioClip>("BalancerChain.mp3", AssetManager.AssetBundleType.Sprite);
            if (chainSound != null)
            {
                SoundManager.Instance.PlaySound(chainSound, false);
            }
        }
    }

    // 鎖のスプライトを作成するヘルパーメソッド
    private SpriteRenderer CreateChainRenderer(float y, float rotation, float z = 7)
    {
        SpriteRenderer renderer = new GameObject("ChainObject").AddComponent<SpriteRenderer>();
        renderer.transform.parent = MeetingHud.Instance.transform;
        renderer.gameObject.layer = 5;
        renderer.transform.localPosition = new(0, y, z);
        renderer.transform.localScale = new(2f, 1.7f, 2f);
        renderer.transform.Rotate(new(0, 0, rotation));
        return renderer;
    }

    // テキストを作成するヘルパーメソッド
    private TextMeshPro CreateText(Vector3 pos, string text, float fontSize)
    {
        TextMeshPro tmp = GameObject.Instantiate(MeetingHud.Instance.TitleText, MeetingHud.Instance.transform);
        tmp.text = text;
        tmp.gameObject.layer = 5;
        tmp.transform.localScale = Vector3.one;
        tmp.transform.localPosition = pos;
        tmp.fontSize = fontSize;
        tmp.fontSizeMax = fontSize;
        tmp.fontSizeMin = fontSize;
        tmp.enableWordWrapping = false;
        tmp.gameObject.SetActive(true);

        // TextTranslatorTMPコンポーネントが存在する場合は破棄
        GameObject.Destroy(tmp.GetComponent<TextTranslatorTMP>());

        return tmp;
    }

    // MeetingHudの表示/非表示を切り替えるヘルパーメソッド
    private void SetActiveMeetingHud(bool active)
    {
        if (MeetingHud.Instance == null) return;

        // 会議HUDの主要コンポーネントを表示/非表示
        if (MeetingHud.Instance.TitleText != null) MeetingHud.Instance.TitleText.gameObject.SetActive(active);
        if (MeetingHud.Instance.SkippedVoting != null) MeetingHud.Instance.SkippedVoting.gameObject.SetActive(active);
    }

    private void UpdateEyeRotation()
    {
        if (eyeBackRender != null)
        {
            rotate -= EYE_ROTATION_SPEED;
            eyeBackRender.transform.localEulerAngles = new(0, 0, rotate);
        }
    }

    private void SetPlayerPositions()
    {
        leftPlayerArea.transform.localPosition = new(LEFT_PLAYER_POS_X, PLAYER_POS_Y, PLAYER_POS_Z);
        rightPlayerArea.transform.localPosition = new(RIGHT_PLAYER_POS_X, PLAYER_POS_Y, PLAYER_POS_Z);
    }

    private void SlideObjectsLeft()
    {
        Vector3 speed = new(SLIDE_SPEED, 0, 0);
        foreach (var objs in ChainObjects)
        {
            objs.renderer.transform.localPosition -= speed;
        }
        eyeBackRender.transform.localPosition -= speed;
        eyeRender.transform.localPosition -= speed;
        BackObject.transform.localPosition -= speed;
        textPleaseVote.transform.localPosition -= speed;
        textUseAbility.transform.localPosition -= speed;
    }

    private void SetObjectsAlpha(byte alpha)
    {
        if (BackObject != null) BackObject.color = new Color32(255, 255, 255, alpha);
        if (eyeRender != null) eyeRender.color = new Color32(255, 255, 255, alpha);
        if (eyeBackRender != null) eyeBackRender.color = new Color32(255, 255, 255, alpha);
        if (textUseAbility != null) textUseAbility.color = new Color32(255, 255, 255, alpha);
    }

    private void SetOpenAnimationAlpha(byte alpha)
    {
        if (BackPictureObject != null) BackPictureObject.color = new Color32(255, 255, 255, alpha);
        if (textPleaseVote != null) textPleaseVote.color = new Color32(255, 255, 255, alpha);
    }

    private void TransitionToEyeAnimation()
    {
        CurrentState = BalancerState.Animation_Eye;
        animIndex = 0;
        BackObject.sprite = AssetManager.GetAsset<Sprite>("BalancerFlareEffect.png");
        BackObject.color = new Color32(255, 255, 255, 0);
        eyeRender.enabled = true;
        eyeRender.color = new Color32(255, 255, 255, 0);
        eyeBackRender.enabled = true;
        eyeBackRender.color = new Color32(255, 255, 255, 0);
        textUseAbility.enabled = true;
        textUseAbility.color = new Color32(255, 255, 255, 0);
        rotate = 360;
        textPleaseTimer = EYE_WAIT_TIME;
        PleaseVoteAnimIndex = 0;

        // 効果音再生
        AudioClip backSound = AssetManager.GetAsset<AudioClip>("BalancerBacksound.mp3", AssetManager.AssetBundleType.Sprite);
        if (backSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(backSound, false);
        }
    }

    private void HandleEyeAnimationPhases()
    {
        if (textPleaseTimer > 0)
        {
            textPleaseTimer -= Time.fixedDeltaTime;
            if (textPleaseTimer <= 0)
            {
                if (textPleaseVote != null)
                {
                    textPleaseVote.enabled = true;
                    textPleaseVote.color = new Color32(255, 255, 255, 0);
                }
            }
        }
        else if (PleaseVoteAnimIndex <= 20)
        {
            PleaseVoteAnimIndex++;
            byte alpha = (byte)Mathf.Min(255, PleaseVoteAnimIndex * 13f);
            if (textPleaseVote != null) textPleaseVote.color = new Color32(255, 255, 255, alpha);

            if (PleaseVoteAnimIndex > 20)
            {
                openTimer = OPEN_WAIT_TIME;
            }
        }
        else
        {
            openTimer -= Time.fixedDeltaTime;
            if (openTimer <= 0)
            {
                TransitionToOpenAnimation();
            }
        }
    }

    private void TransitionToOpenAnimation()
    {
        CurrentState = BalancerState.Animation_Open;
        animIndex = 0;
        BackPictureObject.enabled = true;
        BackPictureObject.color = new Color32(255, 255, 255, 0);
    }

    private void TransitionToWaitVote()
    {
        CurrentState = BalancerState.WaitVote;
        SetActiveMeetingHud(true);

        // UI要素の配置
        MeetingHud.Instance.transform.FindChild("MeetingContents/PhoneUI/baseGlass").transform.localPosition = new(0.012f, 0, 0);
        MeetingHud.Instance.TitleText.GetComponent<TextTranslatorTMP>().enabled = false;
        MeetingHud.Instance.TitleText.transform.localPosition = new(0, 2, -1);
        MeetingHud.Instance.TitleText.transform.localScale = Vector3.one * 2f;
        MeetingHud.Instance.TitleText.text = ModTranslation.GetString(TitleTexts[UnityEngine.Random.Range(0, TitleTexts.Length)]);

        // プレイヤー投票エリアの配置
        leftPlayerArea.transform.localPosition = new(LEFT_PLAYER_POS_X, PLAYER_POS_Y, PLAYER_POS_Z);
        rightPlayerArea.transform.localPosition = new(RIGHT_PLAYER_POS_X, PLAYER_POS_Y, PLAYER_POS_Z);

        // タイマー設定
        MeetingHud.Instance.discussionTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.VotingTime) - Balancer.BalancerVoteTime;
        MeetingHud.Instance.TimerText.gameObject.SetActive(true);
        MeetingHud.Instance.TimerText.transform.localPosition = new(2.05f, -2, -1);
        MeetingHud.Instance.ProceedButton.transform.localPosition = new(3.5f, -2, -1.05f);
    }

    public void EndBalancing()
    {
        if (BalancingAbility != this) return;
        if (!MeetingHud.Instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote)) return;
        // 未投票時ランダムに投票する設定がONの場合のみ、投票先を変更する
        if (Balancer.BalancerRandomVoteWhenNoVote)
        {
            List<byte> targetIds = [targetPlayerLeft.PlayerId, targetPlayerRight.PlayerId];
            foreach (PlayerVoteArea area in MeetingHud.Instance.playerStates)
            {
                if (!area.AmDead && !targetIds.Contains(area.VotedFor))
                {
                    area.VotedFor = targetIds[UnityEngine.Random.Range(0, targetIds.Count)];
                }
            }
        }
        else
        {
            foreach (PlayerVoteArea area in MeetingHud.Instance.playerStates)
            {
                if (!area.AmDead && area.VotedFor < 250)
                    area.VotedFor = 255;
            }
        }
        bool tie;
        var max = MeetingHud.Instance.CalculateVotes().MaxPair(out tie);
        PlayerControl exiled = null;
        if (!tie)
        {
            exiled = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == max.Key);
        }
        MeetingHud.VoterState[] array = new MeetingHud.VoterState[MeetingHud.Instance.playerStates.Length];
        for (int i = 0; i < MeetingHud.Instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = MeetingHud.Instance.playerStates[i];
            MeetingHud.VoterState voterState = default;
            voterState.VoterId = playerVoteArea.TargetPlayerId;
            voterState.VotedForId = playerVoteArea.VotedFor;
            array[i] = voterState;
        }
        MeetingHud.Instance.RpcVotingComplete(array, exiled?.Data, tie);
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    public class BalancerDoubleExilePatch
    {
        static bool IsSec;
        private static TMPro.TextMeshPro confirmImpostorSecondText;
        public static bool Prefix(
            ExileController __instance,
            ref ExileController.InitProperties init)
        {
            if (BalancingAbility == null || !BalancingAbility.isDoubleExile)
                return true;

            __instance.initData = init;

            string printStr = "";

            if (BalancingAbility != null && BalancingAbility.isDoubleExile)
            {
                if (!IsSec)
                {
                    IsSec = true;
                    __instance.initData.networkedPlayer = null;
                    ExileController controller = GameObject.Instantiate(__instance, __instance.transform.parent);
                    controller.Begin(ModHelpers.GenerateExileInitProperties(BalancingAbility.targetPlayerRight.Data, false));
                    IsSec = false;
                    controller.completeString = string.Empty;

                    controller.Text.gameObject.SetActive(false);
                    controller.Player.UpdateFromEitherPlayerDataOrCache(controller.initData.networkedPlayer, PlayerOutfitType.Default, PlayerMaterial.MaskType.Exile, includePet: false);
                    controller.Player.ToggleName(active: false);
                    SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(controller.initData.outfit.SkinId);
                    controller.Player.FixSkinSprite(skin.EjectFrame);
                    BalancerAbility.additionalExileController = controller;
                    AudioClip sound = null;
                    if (controller.EjectSound != null)
                    {
                        sound = new(controller.EjectSound.Pointer);
                    }
                    controller.EjectSound = null;
                    void createlate(int index)
                    {
                        new LateTask(() => { controller.StopAllCoroutines(); controller.StartCoroutine(controller.Animate()); }, 0.025f + index * 0.025f);
                    }
                    new LateTask(() => controller.StartCoroutine(controller.Animate()), 0f);
                    for (int i = 0; i < 23; i++)
                    {
                        createlate(i);
                    }
                    new LateTask(() => { controller.StopAllCoroutines(); controller.EjectSound = sound; controller.StartCoroutine(controller.Animate()); }, 0.6f);
                    ExileController.Instance = __instance;
                    init = ModHelpers.GenerateExileInitProperties(BalancingAbility.targetPlayerLeft.Data, false);
                    if (ModHelpers.IsMap(MapNames.Fungle))
                    {
                        ModHelpers.SetActiveAllObject(controller.gameObject.GetChildren(), "RaftAnimation", false);
                        controller.transform.localPosition = new(-3.75f, -0.2f, -60f);
                    }
                    return true;
                }
                return false;
            }
            return true;
        }
        public static void Postfix(ExileController __instance, ExileController.InitProperties init)
        {
            if (BalancingAbility != null)
            {
                if (BalancingAbility.isDoubleExile)
                {
                    __instance.completeString = ModTranslation.GetString("BalancerDoubleExileText");
                }
                else if (BalancingAbility.isOnePlayerDead)
                {
                    // 片方のプレイヤーが死亡した場合のテキスト
                    __instance.completeString = ModTranslation.GetString("BalancerOnePlayerDeadText");
                }
            }
        }
    }
}

// 天秤会議ボタンクラス
class BalancerMeetingButton : CustomMeetingButtonBase
{
    private BalancerAbility parentAbility;
    private Sprite _sprite;
    public PlayerControl firstSelectedTarget;

    public BalancerMeetingButton(BalancerAbility parent)
    {
        parentAbility = parent;
    }

    public override Sprite Sprite
    {
        get
        {
            if (_sprite == null)
            {
                // アセットマネージャーからスプライトを読み込む
                _sprite = AssetManager.GetAsset<Sprite>("BalancerIcon.png");

                // アセットがない場合はデフォルトスプライトを使用
                if (_sprite == null)
                {
                    _sprite = HudManager.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton].Image;
                }
            }
            return _sprite;
        }
    }

    public override bool HasButtonLocalPlayer => true;

    // ボタンがクリック可能かどうか
    public override bool CheckIsAvailable(ExPlayerControl player)
    {
        // 既に能力を使用済みでなく、プレイヤーが生存していれば使用可能
        if (firstSelectedTarget != null)
        {
            // 1人目のターゲットが選択されている場合、そのプレイヤー自身は選択できない
            return firstSelectedTarget.PlayerId != player.PlayerId && player.IsAlive();
        }
        return parentAbility.HasCount && !parentAbility.isAbilityUsed && player.IsAlive();
    }

    // ボタンを表示するかどうか
    public override bool CheckHasButton(ExPlayerControl player)
    {
        // 自分自身にはボタンを表示しない
        if (ExPlayerControl.LocalPlayer.IsDead()) return false;
        if (player.IsDead()) return false;
        if (firstSelectedTarget != null && firstSelectedTarget.PlayerId == player.PlayerId) return false;
        // 現在の会議で既に能力を使用している場合はボタンを表示しない
        if (parentAbility.isAbilityUsed) return false;
        // 既に天秤会議が進行中の場合はボタンを表示しない
        if (BalancerAbility.BalancingAbility != null) return false;
        // 能力使用回数が残っているかどうかをチェック
        return parentAbility.HasCount;
    }

    // ボタンがクリックされた時の処理
    public override void OnClick(ExPlayerControl exPlayer, GameObject button)
    {
        PlayerControl clickedPlayer = exPlayer.Player;

        if (firstSelectedTarget == null)
        {
            // 1人目のターゲットを選択
            firstSelectedTarget = clickedPlayer;
        }
        else
        {
            // 2人目のターゲットを選択し、能力を発動
            parentAbility.UseAbilityCount();
            BalancerAbility.RpcStartAbility(PlayerControl.LocalPlayer, firstSelectedTarget, clickedPlayer, parentAbility.AbilityId);
            firstSelectedTarget = null;
        }
    }
}

