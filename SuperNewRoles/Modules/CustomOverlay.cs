using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;
using UnhollowerBaseLib;
using AmongUs.Data;
namespace SuperNewRoles.Patches;
[Harmony]
public class CustomOverlays
{
    public static Sprite helpButton;
    private static Sprite colorBG;
    private static SpriteRenderer meetingUnderlay;
    private static SpriteRenderer infoUnderlay;
    private static TMPro.TextMeshPro infoOverlayLeft;
    private static TMPro.TextMeshPro infoOverlayCenter;
    private static TMPro.TextMeshPro infoOverlayRight;
    public static bool overlayShown = false;
    private static Dictionary<byte, string> playerDataDictionary = new();
    internal static Dictionary<byte, string> GetInRolesDictionary = new();

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

    public static void ShowInfoOverlay(int pattern, bool update = false)
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

        switch (pattern)
        {
            case (int)CustomOverlayPattern.GetInRoles:
                GetInRoles(out leftText, out centerText, out rightText);
                // [ ]MEMO : 行数オーバーで...表示欲しい
                break;
            case (int)CustomOverlayPattern.PlayerDataInfo:
                PlayerDataInfo(out leftText, out centerText, out rightText);
                // [x]MEMO : TOPではバニラ設定を表示していた物を代わりに /gr の情報を載せても面白い?
                break;
            case (int)CustomOverlayPattern.MyRole:
                MyRole(out leftText, out centerText, out rightText);
                infoOverlayLeft.transform.localPosition += new Vector3(0.0f, +0.25f, 0.0f);
                infoOverlayCenter.transform.localPosition = infoOverlayLeft.transform.localPosition + new Vector3(0.0f, -0.30f, 0.0f);
                break;
            case (int)CustomOverlayPattern.Regulation:
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

    public static void YoggleInfoOverlay(int pattern, bool update = false)
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
            if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen && overlayShown)
                HideInfoOverlay();

            if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen) return;
            if (Input.GetKeyDown(KeyCode.F3)) YoggleInfoOverlay((int)CustomOverlayPattern.PlayerDataInfo);
            else if (Input.GetKeyDown(KeyCode.G)) YoggleInfoOverlay((int)CustomOverlayPattern.GetInRoles);

            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (Input.GetKeyDown(KeyCode.H)) YoggleInfoOverlay((int)CustomOverlayPattern.MyRole);
            else if (Input.GetKeyDown(KeyCode.I)) YoggleInfoOverlay((int)CustomOverlayPattern.Regulation);
            else if (Input.GetKeyDown(KeyCode.Tab) && overlayShown) YoggleInfoOverlay((int)CustomOverlayPattern.Regulation, true);
        }
    }

    private enum CustomOverlayPattern
    {
        GetInRoles,
        PlayerDataInfo,
        MyRole,
        Regulation,
    }

    // ゲーム開始時辞書に格納する, [内容 : PlayerData, 現在入っている役職(/grの結果)]
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin)), HarmonyPostfix]
    private static void PlayerDataDictionaryAdd_CoBeginPostfix()
    {
        GetInRolesDictionary = new();
        RetrieveGetInRoles();

        playerDataDictionary = new();
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
            if (!p.IsBot()) playerDataDictionary.Add(p.PlayerId, GetPlayerData(p));
    }

    /// <summary>
    /// 「現在入っている役職」(/grコマンドの返答)を取得する。
    /// </summary>
    internal static void RetrieveGetInRoles()
    {
        List<CustomRoleOption> EnableOptions = new();
        foreach (CustomRoleOption option in CustomRoleOption.RoleOptions)
        {
            if (!option.IsRoleEnable) continue;
            if (ModeHandler.IsMode(ModeId.SuperHostRoles, false) && !option.isSHROn) continue;
            EnableOptions.Add(option);
        }

        GetInRolesSave(EnableOptions);
    }
    // [ ]MEMO:役職数(行数)により文字サイズ調整したい
    // [ ]MEMO:頁切り替えにしたい
    // [x]MEMO:パーセント表示したい <= なんで1/100じゃなくて1/10なのか <= GetSelection([0,10,20...90,100])の変換結果なんだからそりゃそうなる() 確率が10倍表記されてるのではなく、indexを取得しているからそうなっただけ()
    // [x]MEMO:パーセント順に変更したい<=これは無理かな()<=*1
    // [x]MEMO:インポスター,第三,クルーで役名,人数,percentを辞書で保存する。<=*1 <= Local変数で
    // [x]MEMO:*1辞書で保存したものをorder Byで並び替えればよい?

    // 「現在入っている役職」を overlayに表示する (Gキーの動作)
    private static void GetInRoles(out string left, out string center, out string right)
    {
        left = center = right = null;

        if (!(ModeHandler.IsMode(ModeId.Default, false) || ModeHandler.IsMode(ModeId.SuperHostRoles, false) || ModeHandler.IsMode(ModeId.Werewolf, false)))
            left = ModTranslation.GetString("NotAssign");
        else
        {
            // ゲームが開始する前は、毎回辞書を初期化し再度取得する。
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            {
                GetInRolesDictionary = new();
                RetrieveGetInRoles();
            }
            if (GetInRolesDictionary.ContainsKey((byte)TeamRoleType.Impostor))
                left += GetInRolesDictionary[(byte)TeamRoleType.Impostor];
            if (GetInRolesDictionary.ContainsKey((byte)TeamRoleType.Crewmate))
                center += GetInRolesDictionary[(byte)TeamRoleType.Crewmate];
            if (GetInRolesDictionary.ContainsKey((byte)TeamRoleType.Neutral))
                right += GetInRolesDictionary[(byte)TeamRoleType.Neutral];
        }
    }

    // 「現在の役職」を辞書に保存する
    private static void GetInRolesSave(List<CustomRoleOption> optionsnotorder)
    {
        // 一時的に設定を保持する。
        Dictionary<CustomRoleOption, (TeamRoleType, int, string)> dic = new();
        StringBuilder impostorRoles = new();
        StringBuilder crewmateRoles = new();
        StringBuilder neutralRoles = new();

        impostorRoles.AppendLine(ModTranslation.GetString("NowRolesMessage") + "\n");
        crewmateRoles.AppendLine("\n");
        neutralRoles.AppendLine("\n");

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
            if (kvp.Value.Item1 == TeamRoleType.Impostor) impostorRoles.AppendLine(roleText);
            else if (kvp.Value.Item1 == TeamRoleType.Crewmate) crewmateRoles.AppendLine(roleText);
            else neutralRoles.AppendLine(roleText);
        }

        // internalな辞書に陣営毎に保存する(kewは陣営)
        GetInRolesDictionary.Add((byte)TeamRoleType.Impostor, impostorRoles.ToString());
        GetInRolesDictionary.Add((byte)TeamRoleType.Crewmate, crewmateRoles.ToString());
        GetInRolesDictionary.Add((byte)TeamRoleType.Neutral, neutralRoles.ToString());
    }

    // 役職の所属を表す文字を作成
    private static string GetRoleTypeText(TeamRoleType type)
    {
        return type switch
        {
            TeamRoleType.Crewmate => ModTranslation.GetString("CrewmateName"),
            TeamRoleType.Impostor => ModTranslation.GetString("ImpostorName"),
            TeamRoleType.Neutral => ModTranslation.GetString("NeutralName"),
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

        // [x]MEMO : AddChatPatchのMyRoleCommandと翻訳key[MyRoleErrorNotGameStart]を一つのCommitで削除したい(復活する可能性もあるから)
        // [x]MEMO : 置換で設定の上に改行と区切り線入れたい <= returnBuilderに変更, center直代入でなく中間変数作る <= 文字サイズ変更で区切られてるから区切り線は逆に見づらそうなので止める
        // [x]MEMO : 陣営と鍵括弧の間に改行、できたらイントロとデスクリプションの間にも改行入れたい <= 流用ではなくコードを持ってきて代入する文字を変えた
        // [x]MEMO : 位置調整
        // [x]MEMO : 素インポ素クルーの時何か情報出そう()
    }

    // バニラ設定(カスタム設定)とSNRの設定を2頁毎にoverlayに表示する (Iキーの動作)
    private static void Regulation(out string left, out string center, out string right)
    {
        left = center = right = null;
        if (SuperNewRolesPlugin.optionsPage > SuperNewRolesPlugin.optionsMaxPage) SuperNewRolesPlugin.optionsPage = 0;

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

        SuperNewRolesPlugin.optionsPage = firstPage;
    }
}