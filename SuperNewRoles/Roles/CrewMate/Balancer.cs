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

namespace SuperNewRoles.Roles.Crewmate;

class Balancer : RoleBase<Balancer>
{
    public override RoleId Role { get; } = RoleId.Balancer;
    public override Color32 RoleColor { get; } = new(255, 128, 0, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new BalancerAbility()];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.PowerPlayResistance];

    // 天秤会議時間
    [CustomOptionFloat("BalancerVoteTime", 0f, 180f, 2.5f, 30f)]
    public static float BalancerVoteTime;

    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

class BalancerAbility : AbilityBase, IAbilityCount
{
    private EventListener meetingStartEventListener;
    private EventListener meetingCloseEventListener;
    private EventListener fixedUpdateEventListener;
    private EventListener updateEventListener;
    private List<PlayerControl> targetPlayers = new();
    public bool isAbilityUsed = false;
    private PlayerControl targetPlayerLeft;
    private PlayerControl targetPlayerRight;
    private bool isDoubleExile = false;

    // 天秤ボタン
    private BalancerMeetingButton balancerButton;

    public static SpriteRenderer BackObject;
    public static SpriteRenderer BackPictureObject;
    public static List<(SpriteRenderer, float, int)> ChainObjects;
    static string[] TitleTexts =
        [
            "BalancerTitleTextEither",
            "BalancerTitleTextAverage",
            "BalancerTitleTextYouVoteEither",
            "BalancerTitleTextEitherExile",
            "BalancerTitleTextWhoIsImpostor",
            "BalancerTitleTextGetBalance",
            "BalancerTitleTextPleaseVote"
        ];
    // アニメーション関連
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
    private static float openMADENOTimer;
    public static int PleaseVoteAnimIndex;

    public enum BalancerState
    {
        NotBalance,
        Animation_Chain,
        Animation_Eye,
        Animation_Open,
        WaitVote
    }
    public BalancerState CurrentState { get; private set; } = BalancerState.NotBalance;

    // 天秤の状態をリセットするメソッド
    private void ClearAndReload()
    {
        targetPlayers.Clear();
        CurrentState = BalancerState.NotBalance;
        isDoubleExile = false;
        targetPlayerLeft = null;
        targetPlayerRight = null;
        isAbilityUsed = false;
    }

    public override void AttachToLocalPlayer()
    {
        Count = 1; // 一会議に一回のみ使用可能
        updateEventListener = FixedUpdateEvent.Instance.AddListener(Update);
    }
    public override void Attach(PlayerControl player, ulong abilityId, AbilityParentBase parent)
    {
        base.Attach(player, abilityId, parent);
        // 天秤ボタンを初期化
        balancerButton = new BalancerMeetingButton(this);
        ExPlayerControl exPlayer = (ExPlayerControl)player;
        exPlayer.AddAbility(balancerButton, new AbilityParentAbility(this));
        meetingStartEventListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        meetingCloseEventListener = MeetingCloseEvent.Instance.AddListener(OnMeetingClose);
        // FixedUpdateイベントにアニメーション更新処理を登録
        fixedUpdateEventListener = FixedUpdateEvent.Instance.AddListener(FixedUpdateAnimation);
    }
    private void Update()
    {

    }
    public override void Detach()
    {
        base.Detach();
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
        // FixedUpdateイベントから登録を解除
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
    }

    public void OnMeetingStart()
    {
        if (isAbilityUsed) return;

        // 会議時にローカルプレイヤーが生存していて能力が使用可能なら
        ExPlayerControl exPlayer = (ExPlayerControl)Player;
        if (exPlayer.IsAlive() && HasCount)
        {
            // CustomMeetingButtonBaseが自動的にボタンを表示するので、ここでは特に何もする必要はない
        }
    }

    public void OnMeetingClose()
    {
        // 会議終了時の処理
        ClearAndReload();
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
        // 天秤能力の使用処理
        targetPlayerLeft = player1;
        targetPlayerRight = player2;

        PleaseVoteAnimIndex = 0;
        MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(false);
        // 能力使用回数を減らす
        isAbilityUsed = true;

        // 会議時間を変更
        MeetingHud.Instance.discussionTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.VotingTime) - Balancer.BalancerVoteTime - 6.5f;

        // 天秤会議の開始処理
        CurrentState = BalancerState.Animation_Chain;
        MeetingHud.Instance.ClearVote();

        // 選択された2人のプレイヤーだけを表示し、他のプレイヤーは非表示にする
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

        // 会議HUDを一時的に非表示
        SetActiveMeetingHud(false);

        // 背景オブジェクトの作成
        BackObject = new GameObject("BackObject").AddComponent<SpriteRenderer>();
        BackObject.transform.parent = MeetingHud.Instance.transform;
        BackObject.gameObject.layer = 5; // UIレイヤー
        BackObject.transform.localPosition = new(0, 0, -11);
        BackObject.transform.localScale = new(2f, 2f, 2f);

        // 背景画像オブジェクトの作成
        BackPictureObject = new GameObject("BackPictureObject").AddComponent<SpriteRenderer>();
        BackPictureObject.transform.parent = MeetingHud.Instance.transform;
        BackPictureObject.gameObject.layer = 5; // UIレイヤー
        BackPictureObject.transform.localPosition = new(0, 0, -0.1f);
        BackPictureObject.transform.localScale = Vector3.one * 1.65f;
        BackPictureObject.enabled = false;
        BackPictureObject.sprite = AssetManager.GetAsset<Sprite>("BalancerMeetingBack.png");

        // アニメーションの初期化
        animIndex = 0;
        if (chainSprites.Count <= 0)
        {
            Sprite emptySprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            for (int i = 0; i < 15; i++)
            {
                var sprite = AssetManager.GetAsset<Sprite>($"average_anim_chain_0{i + 17}.png");
                chainSprites.Add(sprite ?? emptySprite);
            }
        }

        // 目のオブジェクトの作成
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

        // 鎖のオブジェクトの作成
        ChainObjects = new();
        int objectNum = 11;
        for (int i = 0; i < objectNum; i++)
        {
            ChainObjects.Add((CreateChain(UnityEngine.Random.Range(1.8f, -1.7f), UnityEngine.Random.Range(-15f, 15f)), 0f, 0));
        }
        ChainObjects.Add((CreateChain(0, 0, -12f), 0f, 0));

        // テキスト作成
        textUseAbility = CreateText(new(0, 2.1f, -30), ModTranslation.GetString("BalancerAbilityUseText"), 12);
        textUseAbility.enabled = false;
        textPleaseVote = CreateText(new(0, -1f, -30f), ModTranslation.GetString("BalancerVoteText"), 8);
        textPleaseVote.enabled = false;
        textPleaseTimer = 0.35f;

        // 効果音再生
        if (SoundManager.Instance != null)
        {
            // 音声ファイルがAssetManagerに登録されている場合のみ再生
            AudioClip chainSound = AssetManager.GetAsset<AudioClip>("BalancerChain.mp3", AssetManager.AssetBundleType.Sprite);
            if (chainSound != null)
            {
                SoundManager.Instance.PlaySound(chainSound, false);
            }
        }
    }

    // 鎖のスプライトを作成するヘルパーメソッド
    private SpriteRenderer CreateChain(float y, float rotate, float z = 7)
    {
        SpriteRenderer renderer = new GameObject("ChainObject").AddComponent<SpriteRenderer>();
        renderer.transform.parent = MeetingHud.Instance.transform;
        renderer.gameObject.layer = 5;
        renderer.transform.localPosition = new(0, y, z);
        renderer.transform.localScale = new(2f, 1.7f, 2f);
        renderer.transform.Rotate(new(0, 0, rotate));
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
        if (MeetingHud.Instance.SkipVoteButton != null) MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(active);
        if (MeetingHud.Instance.SkippedVoting != null) MeetingHud.Instance.SkippedVoting.gameObject.SetActive(active);
    }
    /*
        // ターゲットプレイヤーを選択する処理
        public void SelectTarget(PlayerControl target)
        {
            if (!HasCount || isAbilityUsed) return;

            if (targetPlayers.Count == 0)
            {
                targetPlayers.Add(target);
                return;
            }

            if (targetPlayers.Count == 1)
            {
                // 同じプレイヤーを選択した場合は無視
                if (targetPlayers[0] == target) return;

                targetPlayers.Add(target);
                StartAbility(Player, targetPlayers[0], targetPlayers[1]);
            }
        }*/

    // アニメーションを更新するメソッド
    public void FixedUpdateAnimation()
    {
        // 天秤状態が「NotBalance」の場合は何もしない
        if (CurrentState == BalancerState.NotBalance) return;

        // 選択したプレイヤーが切断または死亡した場合の処理
        if (targetPlayerLeft == null || targetPlayerRight == null ||
            targetPlayerLeft.Data == null || targetPlayerRight.Data == null ||
            targetPlayerLeft.Data.IsDead || targetPlayerRight.Data.IsDead)
        {
            PlayerControl target = null;
            if (targetPlayerRight == null || targetPlayerRight.Data == null || targetPlayerRight.Data.IsDead)
            {
                target = targetPlayerLeft;
            }
            if (targetPlayerLeft == null || targetPlayerLeft.Data == null || targetPlayerLeft.Data.IsDead)
            {
                target = targetPlayerRight;
            }

            // 会議を終了する処理（ホストのみ実行）
            if (AmongUsClient.Instance.AmHost && target != null)
            {
                MeetingHud.Instance.RpcVotingComplete(new List<MeetingHud.VoterState>().ToArray(), target.Data, false);
            }
            return;
        }

        // 状態に応じたアニメーション処理
        switch (CurrentState)
        {
            case BalancerState.Animation_Chain:
                bool flag = true;
                for (int i = 0; i <= animIndex; i++)
                {
                    var cobj = ChainObjects[i];
                    if (cobj.Item3 < chainSprites.Count)
                    {
                        cobj.Item1.sprite = chainSprites[ChainObjects[i].Item3];
                        ChainObjects[i] = (cobj.Item1, cobj.Item2, cobj.Item3 + 1);
                        flag = false;
                    }
                }
                if ((animIndex + 1) < ChainObjects.Count)
                    animIndex++;
                if (flag)
                {
                    textPleaseTimer -= Time.fixedDeltaTime;
                    if (textPleaseTimer <= 0)
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
                        textPleaseTimer = 0.8f;
                        PleaseVoteAnimIndex = 0;
                        SoundManager.Instance.PlaySound(AssetManager.GetAsset<AudioClip>("BalancerBacksound.mp3", AssetManager.AssetBundleType.Sprite), false);
                    }
                }
                break;
            /*
                bool allChainAnimComplete = true;

                // 鎖のアニメーション更新
                for (int i = 0; i <= animIndex; i++)
                {
                    var chainObj = ChainObjects[i];
                    if (chainObj.Item3 < chainSprites.Count)
                    {
                        chainObj.Item1.sprite = chainSprites[chainObj.Item3];
                        chainObj.Item1.enabled = true;
                        ChainObjects[i] = (chainObj.Item1, chainObj.Item2, chainObj.Item3 + 1);
                        allChainAnimComplete = false;
                    }
                }

                // まだ表示していない鎖があれば次の鎖を表示
                if ((animIndex + 1) < ChainObjects.Count)
                {
                    animIndex++;
                }

                // 鎖のアニメーションが完了したら次の状態へ
                if (allChainAnimComplete)
                {
                    textPleaseTimer -= Time.fixedDeltaTime;
                    if (textPleaseTimer <= 0)
                    {
                        CurrentState = BalancerState.Animation_Eye;
                        animIndex = 0;

                        // 背景エフェクトの設定
                        BackObject.sprite = AssetManager.GetAsset<Sprite>("BalancerFlareEffect.png");
                        BackObject.color = new Color32(255, 255, 255, 0);

                        // 目のエフェクト表示準備
                        eyeRender.enabled = true;
                        eyeRender.color = new Color32(255, 255, 255, 0);
                        eyeBackRender.enabled = true;
                        eyeBackRender.color = new Color32(255, 255, 255, 0);

                        // テキスト表示準備
                        textUseAbility.enabled = true;
                        textUseAbility.color = new Color32(255, 255, 255, 0);

                        rotate = 360;
                        textPleaseTimer = 0.8f;
                        int pleasevoteanimIndex = 0;

                        // 音声再生
                        AudioClip backSound = AssetManager.GetAsset<AudioClip>("BalancerBacksound.mp3", AssetManager.AssetBundleType.Sprite);
                        if (backSound != null && SoundManager.Instance != null)
                        {
                            var audioSource = SoundManager.Instance.PlaySound(backSound, false);
                            if (audioSource != null)
                            {
                                audioSource.pitch = 0.9f; // ピッチ調整
                            }
                        }
                    }
                }
                break;
*/
            case BalancerState.Animation_Eye:
                animIndex++;

                // フェードイン処理
                if (animIndex <= 40)
                {
                    byte alpha = 255;
                    if (animIndex * 6.2f < 255)
                    {
                        alpha = (byte)(animIndex * 6.2f);
                    }

                    if (BackObject != null) BackObject.color = new Color32(255, 255, 255, alpha);
                    if (eyeRender != null) eyeRender.color = new Color32(255, 255, 255, alpha);
                    if (eyeBackRender != null) eyeBackRender.color = new Color32(255, 255, 255, alpha);
                    if (textUseAbility != null) textUseAbility.color = new Color32(255, 255, 255, alpha);
                }
                else
                {

                    if (textPleaseTimer > 0)
                    {
                        textPleaseTimer -= Time.fixedDeltaTime;
                        if (textPleaseTimer <= 0)
                        {
                            if (textPleaseVote != null) textPleaseVote.enabled = true;
                            if (textPleaseVote != null) textPleaseVote.color = new Color32(255, 255, 255, 0);
                        }
                    }
                    else if (PleaseVoteAnimIndex <= 20)
                    {
                        PleaseVoteAnimIndex++;
                        byte alpha = 255;
                        if (PleaseVoteAnimIndex * 13f < 255)
                        {
                            alpha = (byte)(PleaseVoteAnimIndex * 13f);
                        }
                        if (textPleaseVote != null) textPleaseVote.color = new Color32(255, 255, 255, alpha);
                        if (PleaseVoteAnimIndex > 20)
                        {
                            openMADENOTimer = 1f;
                        }
                    }
                    else
                    {
                        openMADENOTimer -= Time.fixedDeltaTime;
                        if (openMADENOTimer <= 0)
                        {
                            CurrentState = BalancerState.Animation_Open;
                            animIndex = 0;
                            BackPictureObject.enabled = true;
                            BackPictureObject.color = new Color32(255, 255, 255, 0);
                        }
                    }
                    /*
                    if (textPleaseTimer > 0)
                    {
                        textPleaseTimer -= Time.fixedDeltaTime;
                    }
                    if (textPleaseTimer <= 0)
                    {
                        CurrentState = BalancerState.Animation_Open;
                        animIndex = 0;

                        // オープンアニメーション準備
                        BackPictureObject.enabled = true;
                        BackPictureObject.color = new Color32(255, 255, 255, 0);
                        textPleaseVote.enabled = true;
                        textPleaseVote.color = new Color32(255, 255, 255, 0);
                        openMADENOTimer = 0.04f;

                        // 音声再生
                        AudioClip openSound = AssetManager.GetAsset<AudioClip>("BalancerBacksound_open.mp3", AssetManager.AssetBundleType.Sprite);
                        if (openSound != null && SoundManager.Instance != null)
                        {
                            SoundManager.Instance.PlaySound(openSound, false);
                        }
                    }*/
                }

                if (eyeBackRender != null) eyeBackRender.transform.localEulerAngles = new(0, 0, rotate);
                rotate -= 0.1f;
                if (rotate <= 0)
                {
                    rotate = 360;
                }
                break;

            case BalancerState.Animation_Open:
                animIndex++;

                // オープンアニメーションのフェードイン
                if (animIndex <= 20)
                {
                    byte alpha = 255;
                    if (animIndex * 16f < 255)
                    {
                        alpha = (byte)(animIndex * 16f);
                    }

                    if (BackPictureObject != null) BackPictureObject.color = new Color32(255, 255, 255, alpha);
                    if (textPleaseVote != null) textPleaseVote.color = new Color32(255, 255, 255, alpha);
                }
                /*
                else
                {
                    openMADENOTimer -= Time.fixedDeltaTime;
                    if (openMADENOTimer <= 0)
                    {
                        // 天秤会議の投票フェーズへ移行
                        CurrentState = BalancerState.WaitVote;

                        // プレイヤー投票エリアを表示
                        leftPlayerArea.transform.localPosition = new(-2f, 0, -5f);
                        rightPlayerArea.transform.localPosition = new(2f, 0, -5f);

                        // 会議タイマーの設定
                        MeetingHud.Instance.discussionTimer = Balancer.BalancerVoteTime;
                    }
                }*/

                Vector3 speed = new(0.6f, 0, 0);
                foreach (var objs in ChainObjects)
                {
                    objs.Item1.transform.localPosition -= speed;
                }
                eyeBackRender.transform.localPosition -= speed;
                eyeRender.transform.localPosition -= speed;
                BackObject.transform.localPosition -= speed;
                textPleaseVote.transform.localPosition -= speed;
                textUseAbility.transform.localPosition -= speed;
                if (BackObject.transform.localPosition.x <= -10)
                {
                    CurrentState = BalancerState.WaitVote;
                    SetActiveMeetingHud(true);
                    MeetingHud.Instance.transform.FindChild("MeetingContents/PhoneUI/baseGlass").transform.localPosition = new(0.012f, 0, 0);
                    MeetingHud.Instance.TitleText.GetComponent<TextTranslatorTMP>().enabled = false;
                    MeetingHud.Instance.TitleText.transform.localPosition = new(0, 2, -1);
                    MeetingHud.Instance.TitleText.transform.localScale = Vector3.one * 2f;
                    MeetingHud.Instance.TitleText.text = ModTranslation.GetString(TitleTexts[UnityEngine.Random.Range(0, TitleTexts.Length)]);
                    leftPlayerArea.transform.localPosition = new(-2.9f, 0, -0.9f);
                    rightPlayerArea.transform.localPosition = new(2.3f, 0, -0.9f);
                    MeetingHud.Instance.discussionTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.VotingTime) - Balancer.BalancerVoteTime;
                    MeetingHud.Instance.TimerText.gameObject.SetActive(true);
                    MeetingHud.Instance.TimerText.transform.localPosition = new(2.05f, -2, -1);
                    MeetingHud.Instance.ProceedButton.transform.localPosition = new(3.5f, -2, -1.05f);
                }

                // 目の回転処理を継続
                if (eyeBackRender != null)
                {
                    rotate -= Time.fixedDeltaTime * 25f;
                    eyeBackRender.transform.localEulerAngles = new(0, 0, rotate);
                }
                break;

            case BalancerState.WaitVote:
                // 投票フェーズ中の処理
                // 目の回転処理を継続
                if (eyeBackRender != null)
                {
                    rotate -= Time.fixedDeltaTime * 25f;
                    eyeBackRender.transform.localEulerAngles = new(0, 0, rotate);
                }
                break;
        }
    }
}

// 天秤会議ボタンクラス
class BalancerMeetingButton : CustomMeetingButtonBase
{
    private BalancerAbility parentAbility;
    private Sprite _sprite;
    private PlayerControl firstSelectedTarget;

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

    public override bool HasButtonLocalPlayer => false;

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
        if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) return false;
        if (player.IsDead()) return false;
        if (firstSelectedTarget != null && firstSelectedTarget.PlayerId == player.PlayerId) return false;
        return true;
    }

    // ボタンがクリックされた時の処理
    public override void OnClick(ExPlayerControl exPlayer, GameObject button)
    {
        PlayerControl clickedPlayer = exPlayer.Player;

        if (firstSelectedTarget == null)
        {
            // 1人目のターゲットを選択
            firstSelectedTarget = clickedPlayer;
            // 通知（任意）
            // TODO: 必要に応じて通知を実装
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