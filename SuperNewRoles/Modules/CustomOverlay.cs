using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.Data;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.Mode;
using UnityEngine;
namespace SuperNewRoles.Patches;
[Harmony]
public class CustomOverlays
{
    private static Sprite helpButton;
    private static Sprite colorBG;
    private static SpriteRenderer meetingUnderlay;
    private static SpriteRenderer infoUnderlay;
    private static TMPro.TextMeshPro infoOverlayLeft;
    private static TMPro.TextMeshPro infoOverlayCenter;
    private static TMPro.TextMeshPro infoOverlayRight;
    private static bool overlayShown = false;
    private static Dictionary<byte, string> playerDataDictionary = new();
    internal static Dictionary<byte, string> ActivateRolesDictionary = new();

    public static void ResetOverlays()
    {
        HideBlackBG();
        HideInfoOverlay();
        UnityEngine.Object.Destroy(meetingUnderlay);
        UnityEngine.Object.Destroy(infoUnderlay);
        UnityEngine.Object.Destroy(infoOverlayLeft);
        UnityEngine.Object.Destroy(infoOverlayCenter);
        UnityEngine.Object.Destroy(infoOverlayRight);
        meetingUnderlay = infoUnderlay = null;
        infoOverlayLeft = infoOverlayCenter = infoOverlayRight = null;
        overlayShown = false;
        nowPattern = CustomOverlayPattern.None;
    }

    public static bool InitializeOverlays()
    {
        HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
        if (hudManager == null) return false;

        if (helpButton == null)
        {
            helpButton = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.HelpButton.png", 115f);
        }

        if (colorBG == null)
        {
            colorBG = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.White.png", 100f);
        }

        if (meetingUnderlay == null)
        {
            meetingUnderlay = UnityEngine.Object.Instantiate(hudManager.FullScreen, hudManager.transform);
            meetingUnderlay.transform.localPosition = new Vector3(0f, 0f, 20f);
            meetingUnderlay.gameObject.SetActive(true);
            meetingUnderlay.enabled = false;
        }

        if (infoUnderlay == null)
        {
            infoUnderlay = UnityEngine.Object.Instantiate(meetingUnderlay, hudManager.transform);
            infoUnderlay.transform.localPosition = new Vector3(0f, 0f, -900f);
            infoUnderlay.gameObject.SetActive(true);
            infoUnderlay.enabled = false;
        }

        if (infoOverlayLeft == null)
        {
            infoOverlayLeft = UnityEngine.Object.Instantiate(hudManager.TaskPanel.taskText, hudManager.transform);
            infoOverlayLeft.fontSize = infoOverlayLeft.fontSizeMin = infoOverlayLeft.fontSizeMax = 1.15f;
            infoOverlayLeft.autoSizeTextContainer = false;
            infoOverlayLeft.enableWordWrapping = false;
            infoOverlayLeft.alignment = TMPro.TextAlignmentOptions.TopLeft;
            infoOverlayLeft.transform.position = Vector3.zero;
            infoOverlayLeft.transform.localPosition = new Vector3(-2.5f, 1.15f, -910f);
            infoOverlayLeft.transform.localScale = Vector3.one;
            infoOverlayLeft.color = Palette.White;
            infoOverlayLeft.enabled = false;
        }

        if (infoOverlayCenter == null)
        {
            infoOverlayCenter = UnityEngine.Object.Instantiate(infoOverlayLeft, hudManager.transform);
            infoOverlayCenter.maxVisibleLines = 30;
            infoOverlayCenter.fontSize = infoOverlayCenter.fontSizeMin = infoOverlayCenter.fontSizeMax = 1.15f;
            infoOverlayCenter.outlineWidth += 0.02f;
            infoOverlayCenter.autoSizeTextContainer = false;
            infoOverlayCenter.enableWordWrapping = false;
            infoOverlayCenter.alignment = TMPro.TextAlignmentOptions.TopLeft;
            infoOverlayCenter.transform.position = Vector3.zero;
            infoOverlayCenter.transform.localPosition = infoOverlayLeft.transform.localPosition + new Vector3(2.5f, 0.0f, 0.0f);
            infoOverlayCenter.transform.localScale = Vector3.one;
            infoOverlayCenter.color = Palette.White;
            infoOverlayCenter.enabled = false;
        }

        if (infoOverlayRight == null)
        {
            infoOverlayRight = UnityEngine.Object.Instantiate(infoOverlayCenter, hudManager.transform);
            infoOverlayRight.maxVisibleLines = 30;
            infoOverlayRight.fontSize = infoOverlayRight.fontSizeMin = infoOverlayRight.fontSizeMax = 1.15f;
            infoOverlayRight.outlineWidth += 0.02f;
            infoOverlayRight.autoSizeTextContainer = false;
            infoOverlayRight.enableWordWrapping = false;
            infoOverlayRight.alignment = TMPro.TextAlignmentOptions.TopLeft;
            infoOverlayRight.transform.position = Vector3.zero;
            infoOverlayRight.transform.localPosition = infoOverlayCenter.transform.localPosition + new Vector3(2.5f, 0.0f, 0.0f);
            infoOverlayRight.transform.localScale = Vector3.one;
            infoOverlayRight.color = Palette.White;
            infoOverlayRight.enabled = false;
        }
        return true;
    }

    public static void ShowBlackBG()
    {
        if (FastDestroyableSingleton<HudManager>.Instance == null) return;
        if (!InitializeOverlays()) return;

        meetingUnderlay.sprite = colorBG;
        meetingUnderlay.enabled = true;
        meetingUnderlay.transform.localScale = new Vector3(20f, 20f, 1f);
        var clearBlack = new Color32(0, 0, 0, 0);

        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            meetingUnderlay.color = Color.Lerp(clearBlack, Palette.Black, t);
        })));
    }

    public static void HideBlackBG()
    {
        if (meetingUnderlay == null) return;
        meetingUnderlay.enabled = false;
    }

    public static void ShowInfoOverlay(CustomOverlayPattern pattern, bool update = false)
    {
        if (overlayShown && !update) return;

        HudManager hudManager = FastDestroyableSingleton<HudManager>.Instance;
        if ((MapUtilities.CachedShipStatus == null || PlayerControl.LocalPlayer == null || hudManager == null || FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || PlayerControl.LocalPlayer.CanMove) && MeetingHud.Instance != null)
            return;

        if (!InitializeOverlays()) return;

        // 文字位置の初期化
        infoOverlayLeft.transform.localPosition = new Vector3(-2.5f, 1.15f, -910f);
        infoOverlayCenter.transform.localPosition = infoOverlayLeft.transform.localPosition + new Vector3(2.5f, 0.0f, 0.0f);
        infoOverlayRight.transform.localPosition = infoOverlayCenter.transform.localPosition + new Vector3(2.5f, 0.0f, 0.0f);

        if (MapBehaviour.Instance != null)
            MapBehaviour.Instance.Close();

        hudManager.SetHudActive(false);

        overlayShown = true;
        nowPattern = pattern;

        Transform parent = MeetingHud.Instance != null ? MeetingHud.Instance.transform : hudManager.transform;
        infoUnderlay.transform.parent = parent;
        infoOverlayLeft.transform.parent = parent;
        infoOverlayCenter.transform.parent = parent;
        infoOverlayRight.transform.parent = parent;

        infoUnderlay.sprite = colorBG;
        infoUnderlay.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);
        infoUnderlay.transform.localScale = new Vector3(7.5f, 5f, 1f);
        infoUnderlay.enabled = true;

        string leftText = "";
        string centerText = "";
        string rightText = "";

        // 文章を取得し、表示位置の調整を行う
        switch (pattern)
        {
            case CustomOverlayPattern.ActivateRoles:
                ActivateRoles(in update, out leftText, out centerText, out rightText);
                break;
            case CustomOverlayPattern.PlayerDataInfo:
                PlayerDataInfo(out leftText, out centerText, out rightText);
                break;
            case CustomOverlayPattern.MyRole:
                MyRole(out leftText, out centerText, out rightText);
                infoOverlayLeft.transform.localPosition += new Vector3(0.0f, +0.25f, 0.0f);
                infoOverlayCenter.transform.localPosition = infoOverlayLeft.transform.localPosition + new Vector3(0.0f, -0.30f, 0.0f);
                break;
            case CustomOverlayPattern.Regulation:
                Regulation(out leftText, out centerText, out rightText);
                infoOverlayRight.transform.localPosition = infoOverlayLeft.transform.localPosition + new Vector3(3.75f, 0.0f, 0.0f);
                break;
        }

        infoOverlayLeft.text = leftText;
        infoOverlayLeft.enabled = true;

        infoOverlayCenter.text = centerText;
        infoOverlayCenter.enabled = true;

        infoOverlayRight.text = rightText;
        infoOverlayRight.enabled = true;

        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            infoUnderlay.color = Color.Lerp(underlayTransparent, underlayOpaque, t);
            infoOverlayLeft.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            infoOverlayCenter.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
            infoOverlayRight.color = Color.Lerp(Palette.ClearWhite, Palette.White, t);
        })));
    }

    public static void HideInfoOverlay()
    {
        if (!overlayShown) return;

        if (MeetingHud.Instance == null) FastDestroyableSingleton<HudManager>.Instance.SetHudActive(true);

        overlayShown = false;
        nowPattern = CustomOverlayPattern.None;
        var underlayTransparent = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        var underlayOpaque = new Color(0.1f, 0.1f, 0.1f, 0.88f);

        FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(0.2f, new Action<float>(t =>
        {
            if (infoUnderlay != null)
            {
                infoUnderlay.color = Color.Lerp(underlayOpaque, underlayTransparent, t);
                if (t >= 1.0f) infoUnderlay.enabled = false;
            }

            if (infoOverlayLeft != null)
            {
                infoOverlayLeft.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                if (t >= 1.0f) infoOverlayLeft.enabled = false;
            }

            if (infoOverlayCenter != null)
            {
                infoOverlayCenter.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                if (t >= 1.0f) infoOverlayCenter.enabled = false;
            }

            if (infoOverlayRight != null)
            {
                infoOverlayRight.color = Color.Lerp(Palette.White, Palette.ClearWhite, t);
                if (t >= 1.0f) infoOverlayRight.enabled = false;
            }
        })));
    }

    public static void YoggleInfoOverlay(CustomOverlayPattern pattern, bool update = false)
    {
        if (overlayShown && !update)
            HideInfoOverlay();
        else
            ShowInfoOverlay(pattern, update);
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class CustomOverlayKeybinds
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen && overlayShown) HideInfoOverlay();
            if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen) return;

            if (Input.GetKeyDown(KeyCode.Escape) && overlayShown) HideInfoOverlay(); // overlayを閉じる
            else if (Input.GetKeyDown(KeyCode.F3)) YoggleInfoOverlay(CustomOverlayPattern.PlayerDataInfo); // 参加プレイヤーの情報を表示
            else if (Input.GetKeyDown(KeyCode.G)) YoggleInfoOverlay(CustomOverlayPattern.ActivateRoles); // 「現在配役されている役職」を表示
            else if (Input.GetKeyDown(KeyCode.Tab) && overlayShown) YoggleInfoOverlay(nowPattern, true); // 全てのoverlayの文章の更新 & GとIはページ送り

            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (Input.GetKeyDown(KeyCode.H)) YoggleInfoOverlay(CustomOverlayPattern.MyRole); // 自分の役職の説明を表示
            else if (Input.GetKeyDown(KeyCode.I)) YoggleInfoOverlay(CustomOverlayPattern.Regulation); // レギュレーション(バニラ設定 & SNRの設定)を表示
        }
    }

    /// <summary>
    /// CustomOverlayの状況を表す
    /// </summary>
    public enum CustomOverlayPattern
    {
        None, // 現在開いていない
        ActivateRoles, // Gキー : 現在配役している役職
        PlayerDataInfo, // F3キー : 参加プレイヤーの情報(プレイヤー名,カラー,導入状況,プラットフォーム,フレンドコード)
        MyRole, // Hキー : 自分の役職の説明と設定の情報
        Regulation, // Iキー : バニラとSNRのゲーム設定の情報
    }

    /// <summary>
    ///　現在のCustomOverlayの状況を保存する
    /// </summary>
    /// <returns>CustomOverlayPattern : 現在のCustomOverlayの状況</returns>
    internal static CustomOverlayPattern nowPattern = CustomOverlayPattern.None;

    // ゲーム開始時辞書に格納する, [内容 : PlayerData, 現在有効な役職(/grの結果)]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPostfix]
    private static void PlayerDataDictionaryAdd_CoBeginPostfix()
    {
        playerDataDictionary = new();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
            if (!p.IsBot()) playerDataDictionary.Add(p.PlayerId, GetPlayerData(p));

        // 現在有効な役職の保存は, IntroPatchの IntroCutscene.CoBegin postfixで行っている。
        // 理由は試合情報のlog記載を正常に行う為。
    }

    /// <summary>
    /// 現在有効な役職をListで取得し, 文章として加工及び辞書への保存を行うメソッドに渡す。
    /// (辞書 : ActivateRolesDictionary)
    /// </summary>
    /// <param name="isLogWrite">true => 現在配役されている役職のlogを記載する / false => しない</param>
    internal static void GetActivateRoles(bool isLogWrite = false)
    {
        ActivateRolesDictionary = new(); // 辞書の初期化
        List<CustomRoleOption> EnableOptions = new();

        foreach (CustomRoleOption option in CustomRoleOption.RoleOptions)
        {
            if (!option.IsRoleEnable) continue;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn) continue;
            EnableOptions.Add(option);
        }

        SaveActivateRoles(EnableOptions, isLogWrite);
    }

    // 「現在有効な役職」を overlayに表示する (Gキーの動作)
    private static void ActivateRoles(in bool update, out string left, out string center, out string right)
    {
        left = center = right = null;

        if (!(ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.SuperHostRoles, false) || ModeHandler.IsMode(ModeId.Werewolf, false)))
            left = ModTranslation.GetString("NotAssign");
        else
        {
            // ゲームが開始する前は、毎回辞書に保存する。開始後は保存してあるものから文章を取得する。 文章取得は共通処理
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
                GetActivateRoles();

            int impLine = 0, neuLine = 0, crewLine = 0;

            // 説明の行数をカウントする
            if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Impostor))
                impLine = ActivateRolesDictionary[(byte)TeamRoleType.Impostor].Count(c => c == '\n') + 1;
            if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Neutral))
                neuLine = ActivateRolesDictionary[(byte)TeamRoleType.Neutral].Count(c => c == '\n') + 1;
            if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Crewmate))
                crewLine = ActivateRolesDictionary[(byte)TeamRoleType.Crewmate].Count(c => c == '\n') + 1;

            // 複数ページにまたがらない場合、辞書内の調整して取得するメソッドを呼ばず、辞書から直接取得する。
            if (impLine <= 28 && neuLine <= 28 && crewLine <= 28)
            {
                if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Impostor))
                    left += ActivateRolesDictionary[(byte)TeamRoleType.Impostor];
                if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Neutral))
                    center += ActivateRolesDictionary[(byte)TeamRoleType.Neutral];
                if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Crewmate))
                    right += ActivateRolesDictionary[(byte)TeamRoleType.Crewmate];
            }
            else ActivatePage(update, out left, out center, out right);

            // インポスター役, 第三陣営役職, クルーメイト役職 前の列が空なら前に詰める
            // [例:第三陣営役職が配役されてなく、クルーメイト役職が配役されている場合] 左の列=>インポ役,中央の列=>クルーメイト, 右の列=>空欄
            string hollowImp = ModTranslation.GetString("NowRolesMessage") + "\n\r\n";
            const string hollow = "　\n\r\n";
            if (left == hollowImp || center == hollow || right == hollow)
            {
                if (left == hollowImp && center == hollow) { left = right.Replace(hollow, hollowImp); right = hollow; }
                if (left == hollowImp) { left = center.Replace(hollow, hollowImp); center = hollow; }
                if (center == hollow) { center = right; right = hollow; }
            }

            if (impLine <= 15 && neuLine <= 15 && crewLine <= 15) // 全陣営が10役職以内の場合、拡大する
            {
                const string size = "<size=135%>";
                left = size + left.Replace(": ", ": \n<size=100%>").Replace("%\r", "%\r</size>") + "</size>";
                center = size + center.Replace(": ", ": \n<size=100%>").Replace("%\r", "%\r</size>") + "</size>";
                right = size + right.Replace(": ", ": \n<size=100%>").Replace("%\r", "%\r</size>") + "</size>";
            }
        }
    }

    private static int ActiveRoleNowPage = 0;
    private static int ActiveRoleMaxPage = 0;

    private static void ActivatePage(in bool update, out string left, out string center, out string right)
    {
        // ページ更新でない場合と最大ページまで開いている場合は「現在の頁」を初期化する。そうでない場合は+1する。
        ActiveRoleNowPage = !update ? 0 : ActiveRoleNowPage < ActiveRoleMaxPage ? ActiveRoleNowPage += 1 : 0;
        ActiveRoleMaxPage = 0; // ActiveRoleMaxPageが使い終わり、設定変更で最大ページ数の変更が起きる事に対応する為初期化する。(ActiveRoleNowPageを決める前に動作させてはいけない。)

        // 頁数, インポ役, クルー役, 第三役
        Dictionary<int, string> impostorDictionary = new(), crewmateDictionary = new(), neutralDictionary = new();

        const int maxLines = 54; // 1行に"\n\n"が含まれる為、2倍にカウントされる。実際の最大行数は 27行 (+ 1(次の頁の文章用))
        int impoLineCount = 0, neuLineCount = 0, crewLineCount = 0; // 各役職の現在処理している行数をカウント。
        int impoPageCount = 0, neuPageCount = 0, crewPageCount = 0; // 各役職の現在のページ数をカウントする。

        string[] impostors = null, neutrals = null, crewmates = null; // /grを役職単位で分割し文字列を
        string impoContent = "", neuContent = "", crewContent = ""; // 全陣営共通変数にするとうまく動かない
        string teamText = $"{string.Format(ModTranslation.GetString("TeamRoleTypeMessage"), "{0}")}{string.Format(ModTranslation.GetString("SettingMaxRoleCount"), "{1}")}\n\n";

        // 陣営別に保存した「現在配役されている役職」を辞書から出し、1役職毎に配列に格納する。
        if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Impostor))
            if (ActivateRolesDictionary[(byte)TeamRoleType.Impostor].Trim('\n', '\r') is not "\r" and not "")
                impostors = ActivateRolesDictionary[(byte)TeamRoleType.Impostor].Split('\n');

        if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Neutral))
            if (ActivateRolesDictionary[(byte)TeamRoleType.Neutral].Trim('\n', '\r') is not "\r" and not "")
                neutrals = ActivateRolesDictionary[(byte)TeamRoleType.Neutral].Split('\n');

        if (ActivateRolesDictionary.ContainsKey((byte)TeamRoleType.Crewmate))
            if (ActivateRolesDictionary[(byte)TeamRoleType.Crewmate].Trim('\n', '\r') is not "\r" and not "")
                crewmates = ActivateRolesDictionary[(byte)TeamRoleType.Crewmate].Split('\n');

        // インポスター役職を頁振り分けする。
        if (impostors != null) // 配役していなかった場合、配列に要素が入らずnullの為Instance参照エラーが生じる。それを防止する為のnullチェック。
        {
            foreach (var imp in impostors)
            {
                int lines = imp.Count(c => c == '\n') + 1;
                if (impoLineCount + lines > maxLines) // 表示可能行数を超えた場合
                {
                    // 現在のページをkeyとして、辞書に表示可能な最大行数を満たしたインポスターの配役情報を保存する。
                    if (impostorDictionary.ContainsKey(impoPageCount)) impostorDictionary[impoPageCount] = impoContent; // keyが存在していた場合上書きする。
                    else impostorDictionary.Add(impoPageCount, impoContent); // keyが存在していなかった場合追加する。
                    impoPageCount++; // 次のページに移動する
                    impoLineCount = 4; // 現在の行数を初期化する。0でない理由は、下記の初期化で4行入る為。
                    // [ 現在入っている役職\n\n【インポスター】 最大x役職\n\n] と言う文字列で初期化
                    impoContent = $"{string.Format(teamText, GetRoleTypeText(TeamRoleType.Impostor), CustomOptionHolder.impostorRolesCountMax.GetSelection())}";
                }
                impoContent = impoContent + imp + "\n";
                impoLineCount += lines + 1; // 現在の行数をカウントする
            }
            // 表示可能な最大行数を満たさず余った文字列を辞書に追加する。
            if (impostorDictionary.ContainsKey(impoPageCount)) impostorDictionary[impoPageCount] = impoContent;
            else impostorDictionary.Add(impoPageCount, impoContent);
            // 最大ページ数をインポスター役職のページ数にする
            ActiveRoleMaxPage = impoPageCount;
        }

        // 第三陣営役職を頁振り分けする。
        if (neutrals != null)  // 配役していなかった場合、以下略
        {
            foreach (var neu in neutrals)
            {
                int lines = neu.Count(c => c == '\n') + 1;
                if (neuLineCount + lines > maxLines)
                {
                    if (neutralDictionary.ContainsKey(neuPageCount)) neutralDictionary[neuPageCount] = neuContent;
                    else neutralDictionary.Add(neuPageCount, neuContent);
                    neuPageCount++;
                    neuLineCount = 4;
                    neuContent = $"{string.Format(teamText, GetRoleTypeText(TeamRoleType.Neutral), CustomOptionHolder.neutralRolesCountMax.GetSelection())}";
                }
                neuContent = neuContent + neu + "\n";
                neuLineCount += lines + 1;
            }
            if (neutralDictionary.ContainsKey(neuPageCount)) neutralDictionary[neuPageCount] = neuContent;
            else neutralDictionary.Add(neuPageCount, neuContent);
            // 現在のページ数(=インポのページ数)より第三陣営役職のページ数の方が大きければ、現在のページ数を第三陣営役職のページ数にする。
            ActiveRoleMaxPage = neuPageCount < ActiveRoleMaxPage ? ActiveRoleMaxPage : neuPageCount;
        }

        // クルーメイト役職を頁振り分けする。
        if (crewmates != null) // 配役していなかった場合、以下略
        {
            foreach (var crew in crewmates)
            {
                int lines = crew.Count(c => c == '\n') + 1;
                if (crewLineCount + lines > maxLines)
                {
                    if (crewmateDictionary.ContainsKey(crewPageCount)) crewmateDictionary[crewPageCount] = crewContent;
                    else crewmateDictionary.Add(crewPageCount, crewContent);
                    crewPageCount++;
                    crewLineCount = 4;
                    crewContent = $"{string.Format(teamText, GetRoleTypeText(TeamRoleType.Crewmate), CustomOptionHolder.crewmateRolesCountMax.GetSelection())}";
                }
                crewContent = crewContent + crew + "\n";
                crewLineCount += lines + 1;
            }
            if (crewmateDictionary.ContainsKey(crewPageCount)) crewmateDictionary[crewPageCount] = crewContent;
            else crewmateDictionary.Add(crewPageCount, crewContent);
            ActiveRoleMaxPage = crewPageCount < ActiveRoleMaxPage ? ActiveRoleMaxPage : crewPageCount;
        }

        string pageText = $"\n{string.Format(ModTranslation.GetString("SettingPressTabUpdateString"), ActiveRoleNowPage + 1, ActiveRoleMaxPage + 1)}";
        // 文字位置を揃える為改行する。改行が必要な回数文字リテラル`*`を繰り返した文字列を作成し、それを文字列"\n"に置き換える。
        // 参考 => https://dobon.net/vb/dotnet/string/repeat.html
        string n = new string('*', 25).Replace("*", "　\n"); // 2行 (役職分類と分類別人数+改行) + 25行("　"(行幅調整のための全角space)+改行) + 3行(改行*2+頁数) => 全30行

        left = impostorDictionary.ContainsKey(ActiveRoleNowPage)
            ? $"{impostorDictionary[ActiveRoleNowPage]}{pageText}" // 辞書に含まれている場合そのまま表示
            : string.Format(teamText, GetRoleTypeText(TeamRoleType.Impostor), CustomOptionHolder.impostorRolesCountMax.GetSelection()) + $"{n}" + pageText; // 含まれていない場合、陣営&配役最大人数とページ関連の文を表示する

        center = neutralDictionary.ContainsKey(ActiveRoleNowPage)
            ? $"{neutralDictionary[ActiveRoleNowPage]}{pageText}"
            : string.Format(teamText, GetRoleTypeText(TeamRoleType.Neutral), CustomOptionHolder.neutralRolesCountMax.GetSelection()) + $"{n}" + pageText;

        right = crewmateDictionary.ContainsKey(ActiveRoleNowPage)
            ? $"{crewmateDictionary[ActiveRoleNowPage]}{pageText}"
            : string.Format(teamText, GetRoleTypeText(TeamRoleType.Crewmate), CustomOptionHolder.crewmateRolesCountMax.GetSelection()) + $"{n}" + pageText;
    }

    // 「現在有効な役職」を辞書に保存する
    private static void SaveActivateRoles(List<CustomRoleOption> optionsnotorder, bool isLogWrite = false)
    {
        // 一時的に設定を保持する。
        Dictionary<CustomRoleOption, (TeamRoleType, int, string)> dic = new();
        StringBuilder impostorRoles = new();
        StringBuilder neutralRoles = new();
        StringBuilder crewmateRoles = new();

        impostorRoles.AppendLine(ModTranslation.GetString("NowRolesMessage") + "\n");
        neutralRoles.AppendLine("　\n"); // インポスターと文字の高さを合わせる為の全角space
        crewmateRoles.AppendLine("　\n"); // インポスターと文字の高さを合わせる為の全角space

        var options = optionsnotorder.OrderBy((CustomRoleOption x) =>
        {
            return x.Intro.Team switch
            {
                TeamRoleType.Impostor => 0,
                TeamRoleType.Neutral => 1000,
                TeamRoleType.Crewmate => 2000,
                _ => 500,
            };
        });

        TeamRoleType type = TeamRoleType.Error;
        foreach (CustomRoleOption option in options)
        {
            // 役職の分類を記載する部分を作成 (【インポスター役職】等)
            if (type != option.Intro.Team)
            {
                type = option.Intro.Team;
                string teamText = $"{string.Format(ModTranslation.GetString("TeamRoleTypeMessage"), GetRoleTypeText(type))}{ModTranslation.GetString("SettingMaxRoleCount")}\n";
                if (type == TeamRoleType.Impostor) impostorRoles.AppendLine($"{string.Format(teamText, CustomOptionHolder.impostorRolesCountMax.GetSelection())}");
                else if (type == TeamRoleType.Crewmate) crewmateRoles.AppendLine($"{string.Format(teamText, CustomOptionHolder.crewmateRolesCountMax.GetSelection())}");
                else neutralRoles.AppendLine($"{string.Format(teamText, CustomOptionHolder.neutralRolesCountMax.GetSelection())}");
            }
            int PlayerCount = 0;
            int percentInt = option.Rate * 10; // 10をかけている理由 => [0.10,20...90,100]というstring[]で表された確率をGetSelectionで取得している為。
            string percent = percentInt < 100 ? $"  {percentInt}" : $"{percentInt}"; // 配役確率の文字数調整

            foreach (CustomOption opt in option.children)
            {
                if (opt.GetName() == CustomOptionHolder.SheriffPlayerCount.GetName())
                {
                    PlayerCount = (int)opt.GetFloat();
                    break;
                }
            }

            // 一時的な辞書に取得した設定を保存
            if (!dic.ContainsKey(option))
                dic.Add(option, (type, PlayerCount, percent));
            else dic[option] = (type, PlayerCount, percent);
        }

        // 一時的辞書の中身から[役職名, 人数, 配役確率]を配役確率の降順に並べ直して、陣営別に保存する
        string roleTextTemplate = $"{{0}} : <pos=75%>{{1}}{ModTranslation.GetString("PlayerCountMessage")}　{{2}}%"; // 文章構造をテンプレートとして取得
        foreach (KeyValuePair<CustomRoleOption, (TeamRoleType, int, string)> kvp in dic.OrderByDescending(i => i.Value.Item3))
        {
            string roleText = string.Format(roleTextTemplate, kvp.Key.Intro.Name, kvp.Value.Item2, kvp.Value.Item3);
            type = kvp.Value.Item1;
            if (type == TeamRoleType.Impostor) impostorRoles.AppendLine(roleText);
            else if (type == TeamRoleType.Crewmate) crewmateRoles.AppendLine(roleText);
            else neutralRoles.AppendLine(roleText);

            var log = type == TeamRoleType.Impostor ? "ImpostorRole" : type == TeamRoleType.Crewmate ? "CrewmateRole" : " NeutralRole";
            if (isLogWrite) Logger.Info($"{roleText.Replace("<pos=75%>", "").Replace("  ", "").Replace("　", "_")}", log);
        }

        // internalな辞書に陣営毎に保存する(keyは陣営)
        ActivateRolesDictionary.Add((byte)TeamRoleType.Impostor, impostorRoles.ToString());
        ActivateRolesDictionary.Add((byte)TeamRoleType.Neutral, neutralRoles.ToString());
        ActivateRolesDictionary.Add((byte)TeamRoleType.Crewmate, crewmateRoles.ToString());
    }

    /// <summary>
    /// 役職の所属(アサイン枠)を表す文字を作成
    /// </summary>
    /// <param name="type">アサイン枠の分類</param>
    /// <returns>string : 役職の所属(アサイン枠)</returns>
    private static string GetRoleTypeText(TeamRoleType type)
    {
        return type switch
        {
            TeamRoleType.Impostor => ModTranslation.GetString("ImpostorName"),
            TeamRoleType.Neutral => ModTranslation.GetString("NeutralName"),
            TeamRoleType.Crewmate => ModTranslation.GetString("CrewmateName"),
            _ => "",
        };
    }

    // PlayerData一覧(名前, カラー, 導入状態, フレンドコード, プラットフォーム)を overlayに表示する (F3キーの動作)
    private static void PlayerDataInfo(out string left, out string center, out string right)
    {
        left = center = right = null;

        StringBuilder leftBuilder = new();
        StringBuilder centerBuilder = new();
        StringBuilder rightBuilder = new();

        // ゲーム開始後はゲーム開始時に辞書に格納した情報から表示する。
        if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
        {
            foreach (var kvp in playerDataDictionary)
            {
                if (kvp.Key < 5) leftBuilder.AppendLine(kvp.Value);
                else if (kvp.Key < 10) centerBuilder.AppendLine(kvp.Value);
                else rightBuilder.AppendLine(kvp.Value);
            }
        }
        else // ゲーム開始前は表示時に情報を取得する。
        {
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.IsBot()) continue;
                string data = GetPlayerData(p);

                if (p.PlayerId < 5) leftBuilder.AppendLine(data);
                else if (p.PlayerId < 10) centerBuilder.AppendLine(data);
                else rightBuilder.AppendLine(data);
            }
        }

        left = leftBuilder.ToString(); // PlayerId 0 ~ 4 => 左の列に表示
        center = centerBuilder.ToString(); // PlayerId 5 ~ 9 => 中央の列に表示
        right = rightBuilder.ToString(); // PlayerId 10 ~ => 右の列に表示
    }

    // 渡されたPlayerControlから, PlayerDataのテキストを取得する。
    private static string GetPlayerData(PlayerControl p)
    {
        string data = null;
        string friendCode;

        // フレンドコードの取得&加工
        if (p.GetClient()?.FriendCode is not null and not "") friendCode = p.GetClient()?.FriendCode; // フレンドコードを所持している場合
        else friendCode = ModTranslation.GetString("NoFriendCode"); // クライアントデータやフレンドコードがない場合, フレンドコードがブランクだった場合
        if (DataManager.Settings.Gameplay.StreamerMode) friendCode = "**********#****"; // バニラ設定[配信者モード]が有効時フレンドコードを伏字風にする

        // プレイヤー名とクルーカラーを■で表記
        data += $"<size=150%>{p.PlayerId + 1}. {p.name}{ModHelpers.Cs(Palette.PlayerColors[p.Data.DefaultOutfit.ColorId], "■")}</size>\n";
        // クルーカラーとカラー名を表記
        data += $"<pos=10%>{ModHelpers.Cs(Palette.PlayerColors[p.Data.DefaultOutfit.ColorId], "■")} : {GetColorTranslation(Palette.ColorNames[p.Data.DefaultOutfit.ColorId])}\n";
        data += $"<size=90%><pos=10%>{ModTranslation.GetString("SNRIntroduction")} : {(p.IsMod() ? "〇" : "×")}\n"; // Mod導入状態
        data += $"<pos=10%>FriendCode : {friendCode}\n"; // フレンドコード
        data += $"<pos=10%>Platform : {p.GetClient()?.PlatformData?.Platform}</size>\n"; // プラットフォーム

        return data;
    }

    // クルーカラーの翻訳を取得する。
    // 参考=>https://github.com/tugaru1975/TownOfPlus/blob/main/Helpers.cs
    private static string GetColorTranslation(StringNames name) =>
        DestroyableSingleton<TranslationController>.Instance.GetString(name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));

    // 自分の役職の説明をoverlayに表示する (Hキーの動作)
    private static void MyRole(out string left, out string center, out string right)
    {
        left = center = right = null;
        StringBuilder option = new();

        // myRoleを表示できない時はエラーメッセージを表示する
        if (ModeHandler.IsMode(ModeId.SuperHostRoles, false))
            left = $"<size=200%>{ModTranslation.GetString("MyRoleErrorSHRMode")}</size>";
        else if (!(ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.Werewolf, false)))
            left = $"<size=200%>{ModTranslation.GetString("NotAssign")}</size>";
        if (left != null) return;

        // 役職を所持している時の記載
        RoleId myRole = PlayerControl.LocalPlayer.GetRole();

        // LINQ使用 ChatGPTさんに聞いたらforeach処理よりも簡潔で効率的な可能性が高い、後開発者の好みと返答された為。
        IEnumerable<CustomRoleOption> myRoleOptions = CustomRoleOption.RoleOptions.Where(option => option.RoleId == myRole).Select(option => { return option; });
        // foreach使用 ChatGPTさんに聞いたらLINQ使うより、可読性が高くより一般的と返答された為。
        foreach (CustomRoleOption roleOption in myRoleOptions)
        {
            IntroData intro = roleOption.Intro;

            left += $"<size=200%>\n{CustomOptionHolder.Cs(roleOption.Intro.color, roleOption.Intro.NameKey + "Name")}</size> <size=95%>: {AddChatPatch.GetTeamText(intro.TeamType)}</size>";
            option.AppendLine("\n");

            option.AppendLine($"<size=125%>「{CustomOptionHolder.Cs(roleOption.Intro.color, IntroData.GetTitle(intro.NameKey, intro.TitleNum))}」</size>\n");
            option.AppendLine($"<size=95%>{intro.Description}\n</size>");
            option.AppendLine($"<size=125%>{ModTranslation.GetString("MessageSettings")}:");
            option.AppendLine($"{AddChatPatch.GetOptionText(roleOption, intro)}</size>");
        }

        center = option.ToString();

        if (left != null) return;

        // 以下素インポスター及び素クルーメイト時の, 役職説明の記載
        Color color;
        string roleName;
        TeamType teamType;

        if (PlayerControl.LocalPlayer.IsImpostor())
        {
            color = Roles.RoleClass.ImpostorRed;
            roleName = "Impostor";
            teamType = TeamType.Impostor;
        }
        else
        {
            color = Palette.CrewmateBlue;
            roleName = "Crewmate";
            teamType = TeamType.Crewmate;
        }

        left = $"<size=200%>\n{CustomOptionHolder.Cs(color, roleName + "Name")}</size> <size=95%>: {AddChatPatch.GetTeamText(teamType)}</size>";

        option.AppendLine("\n");

        option.AppendLine($"<size=200%>「{CustomOptionHolder.Cs(color, roleName + "Title1")}」</size>\n");
        option.AppendLine($"<size=150%>{ModTranslation.GetString(roleName + "Description")}\n</size>");

        center = option.ToString();
    }

    // バニラ設定(カスタム設定)とSNRの設定を2頁毎にoverlayに表示する (Iキーの動作)
    private static void Regulation(out string left, out string center, out string right)
    {
        left = center = right = null;

        // 左の列が必ず奇数ページになるようにする
        switch (SuperNewRolesPlugin.optionsPage % 2)
        {
            case 0:
                break;
            case 1:
                SuperNewRolesPlugin.optionsPage -= 1;
                break;
        }

        int firstPage = SuperNewRolesPlugin.optionsPage;
        int page = firstPage;

        left = GameOptionsDataPatch.ResultData();
        SuperNewRolesPlugin.optionsPage = page + 1;

        if (SuperNewRolesPlugin.optionsPage <= SuperNewRolesPlugin.optionsMaxPage)
            right = GameOptionsDataPatch.ResultData();

        SuperNewRolesPlugin.optionsPage = firstPage; // 現在のページを左の列に表示しているページに戻す
    }
}