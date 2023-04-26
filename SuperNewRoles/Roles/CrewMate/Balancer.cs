using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SuperNewRoles.Roles.Crewmate;

public static class Balancer
{
    private const int OptionId = 1268;
    public static CustomRoleOption BalancerOption;
    public static CustomOption BalancerPlayerCount;
    public static CustomOption BalancerVoteTime;
    public static void SetupCustomOptions()
    {
        BalancerOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Balancer);
        BalancerPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Crewmate, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], BalancerOption);
        BalancerVoteTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Crewmate, "BalancerVoteTime", 30f, 0f, 180f, 2.5f, BalancerOption);
    }

    public static List<PlayerControl> BalancerPlayer;
    public static Color32 color = new(255, 128, 0, byte.MaxValue);
    public static void ClearAndReload()
    {
        BalancerPlayer = new();
        currentAbilityUser = null;
        CurrentState = BalancerState.NotBalance;
        IsDoubleExile = false;
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
    public enum BalancerState {
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
    public static void Update() {
        if (BackObject != null) {
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
                    if (BackObject.transform.localPosition.x <= -10) {
                        CurrentState = BalancerState.WaitVote;
                        SetActiveMeetingHud(true);
                        MeetingHud.Instance.transform.FindChild("MeetingContents/PhoneUI/baseGlass").gameObject.SetActive(false);
                        MeetingHud.Instance.TitleText.GetComponent<TextTranslatorTMP>().enabled = false;
                        MeetingHud.Instance.TitleText.transform.localPosition = new(0, 2, -200);
                        MeetingHud.Instance.TitleText.transform.localScale = Vector3.one * 2f;
                        MeetingHud.Instance.TitleText.text = titletext;
                        leftplayerarea.transform.localPosition = leftpos;
                        rightplayerarea.transform.localPosition = rightpos;
                        MeetingHud.Instance.discussionTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(AmongUs.GameOptions.Int32OptionNames.VotingTime) - BalancerVoteTime.GetFloat();
                        MeetingHud.Instance.TimerText.gameObject.SetActive(true);
                        MeetingHud.Instance.TimerText.transform.localPosition = new(2.05f, -2, -20);
                        MeetingHud.Instance.ProceedButton.transform.localPosition = new(3.5f, -2, -20.5f);
                    }
                    break;
            }
            //切断したなら
            if (targetplayerleft == null || targetplayerright == null) {

            }
        }
    }
    public static PlayerControl targetplayerleft;
    static PlayerVoteArea leftplayerarea;
    static readonly Vector3 leftpos = new(-2.9f, 0, -201f);
    public static PlayerControl targetplayerright;
    static PlayerVoteArea rightplayerarea;
    static readonly Vector3 rightpos = new(2.3f, 0, -201f);
    public static bool IsDoubleExile;
    static string[] titletexts =
        new string[] {
            "BalancerTitleTextEither",
            "BalancerTitleTextAverage",
            "BalancerTitleTextYouVoteEither",
            "BalancerTitleTextEitherExile",
            "BalancerTitleTextWhoIsImpostor"
        };
    static string titletext => ModTranslation.GetString(ModHelpers.GetRandom(titletexts));
    static void SetActiveMeetingHud(bool active) {
        MeetingHud.Instance.TitleText.gameObject.SetActive(active);
        MeetingHud.Instance.TimerText.gameObject.SetActive(active);
        if (!active)
        {
            MeetingHud.Instance.SkipVoteButton.gameObject.SetActive(active);
            MeetingHud.Instance.SkippedVoting.SetActive(active);
        }
    }
    public static void StartAbility(PlayerControl source, PlayerControl player1, PlayerControl player2)
    {
        MeetingHud.Instance.discussionTimer = GameOptionsManager.Instance.CurrentGameOptions.GetInt(AmongUs.GameOptions.Int32OptionNames.VotingTime) - BalancerVoteTime.GetFloat() - 6.5f;
        currentAbilityUser = source;
        targetplayerleft = player1;
        targetplayerright = player2;
        CurrentState = BalancerState.Animation_Chain;
        MeetingHud.Instance.ClearVote();
        foreach (PlayerVoteArea area in MeetingHud.Instance.playerStates) {
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
        BackPictureObject.transform.localPosition = new(0, 0, -20);
        BackPictureObject.transform.localScale = Vector3.one * 1.65f;
        //初期化
        BackPictureObject.enabled = false;
        BackPictureObject.sprite = BackSprite;

        // アニメーションの初期化
        animIndex = 0;
        if (chainsprites.Count <= 0) {
            for (int i = 0; i < 15; i++) {
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
        textuseability = createtext(new(0, 2.1f, -30), "能力発動", 12);
        textuseability.enabled = false;
        textpleasevote = createtext(new(0, -1f, -30f), "どちらかに投票せよ！", 8);
        textpleasevote.enabled = false;
        textpleasetimer = 0.35f;
        SoundManager.Instance.PlaySound(ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.Balancer.chain.raw"), false);
    }
    static TextMeshPro createtext(Vector3 pos, string text, float fontsize) {
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
    static SpriteRenderer createchain(float pos, float rotate, float zpos = 7f) {

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
    // ここにコードを書きこんでください
}