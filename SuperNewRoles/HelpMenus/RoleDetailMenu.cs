using System.Linq;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.HelpMenus;

public static class RoleDetailMenu
{
    private static GameObject selectedButton;
    private static GameObject menuObject;

    /// <summary>
    /// ボタンクリック時の処理
    /// </summary>
    /// <param name="role">選択された役職</param>
    /// <param name="buttonObject">クリックされたボタンオブジェクト</param>
    public static void OnRoleButtonClicked(IRoleInformation role, GameObject buttonObject)
    {
        if (buttonObject == null) return;

        // Deactivate previous selected button's "Selected"
        if (selectedButton != null)
        {
            var prevSelected = selectedButton.transform.Find("Selected");
            if (prevSelected != null) prevSelected.gameObject.SetActive(false);
        }

        // Activate new selected button's "Selected"
        var newSelected = buttonObject.transform.Find("Selected");
        if (newSelected != null) newSelected.gameObject.SetActive(true);
        selectedButton = buttonObject;

        Logger.Info($"{role.RoleName} ボタンがクリックされました");
        ShowRoleInformation(role);
    }

    /// <summary>
    /// 役職の情報を表示する
    /// </summary>
    /// <param name="role">表示する役職</param>
    public static void ShowRoleInformation(IRoleInformation role)
    {
        if (menuObject == null)
        {
            Logger.Error("MenuObjectがnullです。");
            return;
        }

        Transform roleInformation = menuObject.transform.Find("Scroller/RoleInformation");
        if (roleInformation == null)
        {
            Logger.Error("RoleInformationオブジェクトが見つかりませんでした。");
            return;
        }

        roleInformation.gameObject.SetActive(true);

        // テキスト設定処理を共通化
        SetTextComponent(roleInformation, "RoleName", role.RoleColor, role.RoleName);
        SetTextComponent(roleInformation, "RoleTeam", Color.white, role.AssignedTeams.Count == 0 ? "HelpMenu.AllTeams" : string.Join(", ", role.AssignedTeams.Select(t => ModHelpers.CsWithTranslation(GetTeamColor(t), t.ToString()))));
        SetTextComponent(roleInformation, "RoleDescription", Color.white, $"{role.RoleName}.Description");
        SetTextComponent(roleInformation, "RoleSettingsTitle", Color.white, "HelpMenu.MyRoleInformation.RoleSettingsTitle");
        string roleSettings = GenerateRoleSettingsText(role);
        SetTextComponent(roleInformation, "RoleSettings", Color.white, roleSettings);
    }

    /// <summary>
    /// 役職の設定情報のみを更新する
    /// </summary>
    /// <param name="role">更新する役職</param>
    public static void UpdateRoleSettingsInformation(IRoleBase role)
    {
        if (menuObject == null)
        {
            Logger.Error("MenuObjectがnullです。");
            return;
        }

        Transform roleInformation = menuObject.transform.Find("Scroller/RoleInformation");
        if (roleInformation == null)
        {
            Logger.Error("RoleInformationオブジェクトが見つかりませんでした。");
            return;
        }

        if (!roleInformation.gameObject.activeSelf)
        {
            Logger.Info("RoleInformationオブジェクトが非アクティブなため、更新をスキップします。");
            return;
        }

        // 設定テキストのみを更新
        string roleSettings = GenerateRoleSettingsText(role);
        SetTextComponent(roleInformation, "RoleSettings", Color.white, roleSettings);

        Logger.Info($"{role.Role} の設定情報を更新しました。");
    }

    /// <summary>
    /// 役職の設定テキストを生成する
    /// </summary>
    /// <param name="role">対象の役職</param>
    /// <returns>設定テキスト</returns>
    public static string GenerateRoleSettingsText(IRoleInformation role)
    {
        var scroller = menuObject.transform.Find("Scroller")?.GetComponent<Scroller>();
        if (false)//roleOption == null)
        {
            // random 1~2
            int num = Random.Range(1, 3);
            if (scroller != null)
                scroller.ContentYBounds.max = 0.2f;
            return ModTranslation.GetString($"HelpMenu.NoneOptions{num}");
        }
        var settings = role.Options.Where(o => o.ShouldDisplay())
            .Select(o => $"{ModTranslation.GetString(o.Name)}: {o.GetCurrentSelectionString()}");

        if (role.HiddenOption && role.PercentageOption != null && role.NumberOfCrews != null)
            settings = settings.Prepend($"{ModTranslation.GetString("AssignPer")}: {role.PercentageOption}%")
                    .Prepend($"{ModTranslation.GetString("NumberOfCrews")}: {role.NumberOfCrews}");


        // 設定項目数に応じてスクローラーの高さを調整
        float contentHeight = (settings.Count() - 2) * 0.267f + 0.15f; // 1項目あたり0.2の高さ
        if (scroller != null)
            scroller.ContentYBounds.max = contentHeight;

        return string.Join("\n", settings);
    }

    /// <summary>テキストコンポーネントの設定を共通処理化</summary>
    public static void SetTextComponent(Transform parent, string childName, Color color, string text)
    {
        var textComponent = parent.Find(childName)?.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = ModHelpers.CsWithTranslation(color, text);
        }
        else
            Logger.Error($"{childName} TextMeshProコンポーネントが見つかりませんでした。");
    }

    /// <summary>陣営に応じた色を取得</summary>
    public static Color GetTeamColor(AssignedTeamType team)
    {
        return team switch
        {
            AssignedTeamType.Crewmate => Color.white,
            AssignedTeamType.Neutral => Color.gray,
            _ => Palette.ImpostorRed
        };
    }
    /// <summary>陣営に応じた色を取得</summary>
    public static Color GetTeamColor(RoleOptionMenuType team)
    {
        return team switch
        {
            RoleOptionMenuType.Crewmate => Color.white,
            RoleOptionMenuType.Neutral => Color.gray,
            RoleOptionMenuType.Ghost => Color.cyan,
            RoleOptionMenuType.Modifier => new Color32(255, 112, 183, 255),
            _ => Palette.ImpostorRed
        };
    }

    /// <summary>
    /// メニューオブジェクトを設定する
    /// </summary>
    /// <param name="menu">設定するメニューオブジェクト</param>
    public static void SetMenuObject(GameObject menu)
    {
        menuObject = menu;
    }
}