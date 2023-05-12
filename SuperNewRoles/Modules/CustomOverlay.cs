using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;

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
            infoOverlayCenter.maxVisibleLines = 28;
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
            infoOverlayRight.maxVisibleLines = 28;
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
            case (int)CustomOverlayPattern.GameInfo:
                SuperNewRolesPlugin.optionsPage = 0;
                // [ ]MEMO : TOPではバニラ設定を表示していた物を代わりに /gr の情報を載せても面白い?
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

            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (FastDestroyableSingleton<HudManager>.Instance.Chat.IsOpen) return;

            if (Input.GetKeyDown(KeyCode.F3)) YoggleInfoOverlay((int)CustomOverlayPattern.GameInfo);
            else if (Input.GetKeyDown(KeyCode.H)) YoggleInfoOverlay((int)CustomOverlayPattern.MyRole);
            else if (Input.GetKeyDown(KeyCode.I)) YoggleInfoOverlay((int)CustomOverlayPattern.Regulation);
            else if (Input.GetKeyDown(KeyCode.Tab) && overlayShown) YoggleInfoOverlay((int)CustomOverlayPattern.Regulation, true);
        }
    }

    private enum CustomOverlayPattern
    {
        GameInfo,
        MyRole,
        Regulation,
    }

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