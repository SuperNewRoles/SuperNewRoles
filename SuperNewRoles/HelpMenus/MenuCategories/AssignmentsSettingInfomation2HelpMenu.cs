using System.Linq;
using System.Text;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.HelpMenus;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.HelpMenus.MenuCategories;

public class AssignmentsSettingInfomation2HelpMenu : HelpMenuCategoryBase
{
    public override string Name => "AssignmentsSettingInfomation2";
    public override HelpMenuCategory Category => HelpMenuCategory.AssignmentsSettingInfomation2;
    private GameObject Container;
    private GameObject MenuObject;
    public string lastHash;
    private string lastRoleSettingsHash; // 現在表示中の役職の設定ハッシュ
    private DelayTask _updateShowTask;
    private GameObject RoleDetailObject; // 役職詳細表示用オブジェクト
    private GameObject LeftButtonsObject; // 左側のボタン

    // 役職詳細表示中かどうか
    private bool isShowingRoleDetail = false;
    // 現在表示中の役職ID（幽霊役職またはモディファイア）
    private object currentDisplayedRoleId = null;

    public override void Show(GameObject Container)
    {
        this.Container = Container;

        // 役職詳細表示中なら閉じる
        if (isShowingRoleDetail)
        {
            CloseRoleDetail();
            isShowingRoleDetail = false;
        }

        // 既存のMenuObjectがあれば破棄
        if (MenuObject != null)
        {
            GameObject.Destroy(MenuObject);
            MenuObject = null;
        }

        // AssignmentsSettingInfomationHelpMenu2 from AssetManager
        MenuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("AssignmentsSettingInfomationHelpMenu2"), Container.transform);
        MenuObject.transform.localPosition = Vector3.zero;
        MenuObject.transform.localScale = Vector3.one;
        MenuObject.transform.localRotation = Quaternion.identity;

        // 左側のボタンを保存
        LeftButtonsObject = GameObject.Find("HelpMenuObject/LeftButtons");

        // 表示を更新
        UpdateShow();
    }

    public override void UpdateShow()
    {
        DelayTask.CancelIfExist(ref _updateShowTask);

        // MenuObjectがnullの場合は処理を中断
        if (MenuObject == null) return;

        var roleInformation = MenuObject.transform.Find("Scroller/RoleInformation");
        if (roleInformation == null)
        {
            Logger.Error("RoleInformationオブジェクトが見つかりませんでした。");
            return;
        }

        float maxContentYBoundsMax = 0;

        // 幽霊役職の設定
        SetupGhostRoleInformation(roleInformation, out float ghostContentYBoundsMax);
        maxContentYBoundsMax = Mathf.Max(maxContentYBoundsMax, ghostContentYBoundsMax);

        // モディファイア役職の設定
        SetupModifierInformation(roleInformation, out float modifierContentYBoundsMax);
        maxContentYBoundsMax = Mathf.Max(maxContentYBoundsMax, modifierContentYBoundsMax);

        var scroller = MenuObject.transform.Find("Scroller").GetComponent<Scroller>();
        scroller.ContentYBounds.max = maxContentYBoundsMax;
    }

    private void SetupGhostRoleInformation(Transform infoObject, out float ContentYBoundsMax)
    {
        // 幽霊役職のオブジェクトを取得
        var info = infoObject.Find("GhostRoles");
        if (info == null)
        {
            Logger.Error("GhostRolesオブジェクトが見つかりませんでした。");
            ContentYBoundsMax = 0;
            return;
        }

        // 「TeamName」テキストコンポーネントの設定
        var teamNameTMP = info.Find("TeamName").GetComponent<TextMeshPro>();
        if (teamNameTMP != null)
        {
            teamNameTMP.text = ModHelpers.CsWithTranslation(Color.cyan, "GhostRoles");
        }
        else
        {
            Logger.Error("TeamName TextMeshProコンポーネントが見つかりませんでした。");
        }

        // Rolesのテンプレートを取得し非表示に設定
        var rolesTemplate = info.Find("Roles").GetComponent<TextMeshPro>();
        if (rolesTemplate == null)
        {
            Logger.Error("Rolesテンプレートが見つかりませんでした。");
            ContentYBoundsMax = 0;
            return;
        }
        rolesTemplate.gameObject.SetActive(false);

        // 既存のロールテキストを削除（テンプレート以外）
        foreach (GameObject child in info.gameObject.GetChildren())
        {
            if (child.name.StartsWith("Roles(Clone)"))
            {
                GameObject.Destroy(child);
            }
        }

        // ロール情報の表示開始位置
        float yPos = 1.42f;

        int index = 0;
        // 各幽霊役職について処理
        foreach (var ghostRole in RoleOptionManager.GhostRoleOptions)
        {
            if (ghostRole.Percentage == 0 || ghostRole.NumberOfCrews == 0)
                continue;

            // テンプレートからロール情報オブジェクトを複製
            var newRole = GameObject.Instantiate(rolesTemplate, info);
            newRole.gameObject.SetActive(true);
            newRole.transform.localPosition = new Vector3(0, yPos, 0);

            string baseRoleText = $"<b>{ModHelpers.CsWithTranslation(ghostRole.RoleColor, ghostRole.RoleId.ToString())}</b> x{ghostRole.NumberOfCrews} ({ghostRole.Percentage}%)\n";
            newRole.text = baseRoleText;

            // PassiveButtonの設定
            var boxCollider = newRole.GetComponent<BoxCollider2D>();
            PassiveButton passiveButton = newRole.gameObject.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { boxCollider };

            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info($"{ghostRole.RoleId} Selected");
                ShowGhostRoleDetail(ghostRole.RoleId);
            }));

            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                newRole.text = ModHelpers.Cs(Color.green, $"<b>{ModHelpers.CsWithTranslation(Color.green, ghostRole.RoleId.ToString())}</b> x{ghostRole.NumberOfCrews} ({ghostRole.Percentage}%)\n");
            }));

            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                newRole.text = baseRoleText;
            }));

            yPos -= 0.2f; // Y座標を調整
            index++;
        }
        ContentYBoundsMax = index <= 22 ? 0f : (index - 22) * 0.167f + 0.05f;
    }

    private void SetupModifierInformation(Transform infoObject, out float ContentYBoundsMax)
    {
        // モディファイア役職のオブジェクトを取得
        var info = infoObject.Find("Modifier");
        if (info == null)
        {
            Logger.Error("Modifierオブジェクトが見つかりませんでした。");
            ContentYBoundsMax = 0;
            return;
        }

        // 「TeamName」テキストコンポーネントの設定
        var teamNameTMP = info.Find("TeamName").GetComponent<TextMeshPro>();
        if (teamNameTMP != null)
        {
            teamNameTMP.text = ModHelpers.CsWithTranslation(new Color32(255, 112, 183, 255), "Modifier");
        }
        else
        {
            Logger.Error("TeamName TextMeshProコンポーネントが見つかりませんでした。");
        }

        // Rolesのテンプレートを取得し非表示に設定
        var rolesTemplate = info.Find("Roles").GetComponent<TextMeshPro>();
        if (rolesTemplate == null)
        {
            Logger.Error("Rolesテンプレートが見つかりませんでした。");
            ContentYBoundsMax = 0;
            return;
        }
        rolesTemplate.gameObject.SetActive(false);

        // 既存のロールテキストを削除（テンプレート以外）
        foreach (GameObject child in info.gameObject.GetChildren())
        {
            if (child.name.StartsWith("Roles(Clone)"))
            {
                GameObject.Destroy(child);
            }
        }

        // ロール情報の表示開始位置
        float yPos = 1.42f;

        int index = 0;
        // 各モディファイア役職について処理
        foreach (var modifierRole in CustomRoleManager.AllModifiers)
        {
            if (modifierRole.PercentageOption == 0 || modifierRole.NumberOfCrews == 0)
                continue;

            // テンプレートからロール情報オブジェクトを複製
            var newRole = GameObject.Instantiate(rolesTemplate, info);
            newRole.gameObject.SetActive(true);
            newRole.transform.localPosition = new Vector3(0, yPos, 0);

            string baseRoleText = $"<b>{ModHelpers.CsWithTranslation(modifierRole.RoleColor, modifierRole.ModifierRole.ToString())}</b> x{modifierRole.NumberOfCrews} ({modifierRole.PercentageOption}%)\n";
            newRole.text = baseRoleText;

            // PassiveButtonの設定
            var boxCollider = newRole.GetComponent<BoxCollider2D>();
            PassiveButton passiveButton = newRole.gameObject.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { boxCollider };

            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info($"{modifierRole.ModifierRole} Selected");
                ShowModifierDetail(modifierRole.ModifierRole);
            }));

            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                newRole.text = ModHelpers.Cs(Color.green, $"<b>{ModHelpers.CsWithTranslation(Color.green, modifierRole.ModifierRole.ToString())}</b> x{modifierRole.NumberOfCrews} ({modifierRole.PercentageOption}%)\n");
            }));

            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                newRole.text = baseRoleText;
            }));

            yPos -= 0.2f; // Y座標を調整
            index++;
        }
        ContentYBoundsMax = index <= 22 ? 0f : (index - 22) * 0.167f + 0.05f;
    }

    // 幽霊役職詳細を表示する
    private void ShowGhostRoleDetail(GhostRoleId ghostRoleId)
    {
        // 既存の詳細表示を削除
        if (RoleDetailObject != null)
            GameObject.Destroy(RoleDetailObject);

        // 役職詳細表示中フラグを設定
        isShowingRoleDetail = true;

        // 幽霊役職詳細オブジェクトを作成
        RoleDetailObject = RoleDetailHelper.ShowRoleDetail(RoleId.None, Container, MenuObject, CloseRoleDetail);

        // RoleDetailMenuにMenuObjectを設定
        RoleDetailMenu.SetMenuObject(RoleDetailObject);

        // 幽霊役職の情報を表示
        IGhostRoleBase ghostRoleBase = CustomRoleManager.GetGhostRoleById(ghostRoleId);
        if (ghostRoleBase != null)
        {
            RoleDetailMenu.ShowRoleInformation(ghostRoleBase);
        }

        // 現在表示中の役職IDを保存
        currentDisplayedRoleId = ghostRoleId;

        // 役職設定のハッシュを初期化
        lastRoleSettingsHash = GenerateGhostRoleSettingsHash(ghostRoleId);
    }

    // モディファイア詳細を表示する
    private void ShowModifierDetail(ModifierRoleId modifierRoleId)
    {
        // 既存の詳細表示を削除
        if (RoleDetailObject != null)
            GameObject.Destroy(RoleDetailObject);

        // 役職詳細表示中フラグを設定
        isShowingRoleDetail = true;

        // モディファイア詳細オブジェクトを作成
        RoleDetailObject = RoleDetailHelper.ShowRoleDetail(RoleId.None, Container, MenuObject, CloseRoleDetail);

        // RoleDetailMenuにMenuObjectを設定
        RoleDetailMenu.SetMenuObject(RoleDetailObject);

        // モディファイアの情報を表示
        IModifierBase modifierBase = CustomRoleManager.GetModifierById(modifierRoleId);
        if (modifierBase != null)
        {
            RoleDetailMenu.ShowRoleInformation(modifierBase);
        }

        // 現在表示中の役職IDを保存
        currentDisplayedRoleId = modifierRoleId;

        // 役職設定のハッシュを初期化
        lastRoleSettingsHash = GenerateModifierSettingsHash(modifierRoleId);
    }

    // 役職詳細を閉じる
    private void CloseRoleDetail()
    {
        // ヘルパークラスを使用して役職詳細を閉じる
        RoleDetailHelper.CloseRoleDetail(RoleDetailObject, MenuObject);
        RoleDetailObject = null;

        // 役職詳細表示中フラグをリセット
        isShowingRoleDetail = false;

        // 表示内容を更新し直す
        DelayTask.UpdateOrAdd(() => UpdateShow(), 0.1f, ref _updateShowTask, "UpdateShowTask");
    }

    public override void OnUpdate()
    {
        // 全体の役職リスト（人数・パーセンテージ）用のハッシュを生成
        StringBuilder sb = new();
        foreach (var ghostRole in RoleOptionManager.GhostRoleOptions)
        {
            sb.Append($"{ghostRole.RoleId} x{ghostRole.NumberOfCrews} ({ghostRole.Percentage}%)\n");
        }
        foreach (var modifierRole in RoleOptionManager.ModifierRoleOptions)
        {
            sb.Append($"{modifierRole.ModifierRoleId} x{modifierRole.NumberOfCrews} ({modifierRole.Percentage}%)\n");
        }
        string hash = sb.ToString();

        // 役職リストに変更があれば全体を更新
        if (lastHash != hash)
        {
            lastHash = hash;
            DelayTask.UpdateOrAdd(() => UpdateShow(), 0.5f, ref _updateShowTask, "UpdateShowTask");
        }

        // 役職詳細表示中の場合、現在表示中の役職の設定変更も検知する
        if (isShowingRoleDetail && currentDisplayedRoleId != null && RoleDetailObject != null)
        {
            string currentRoleSettingsHash = "";

            if (currentDisplayedRoleId is GhostRoleId ghostRoleId)
            {
                currentRoleSettingsHash = GenerateGhostRoleSettingsHash(ghostRoleId);
            }
            else if (currentDisplayedRoleId is ModifierRoleId modifierRoleId)
            {
                currentRoleSettingsHash = GenerateModifierSettingsHash(modifierRoleId);
            }

            // 役職設定に変更があれば詳細表示を更新
            if (lastRoleSettingsHash != currentRoleSettingsHash)
            {
                lastRoleSettingsHash = currentRoleSettingsHash;
                UpdateRoleDetailIfNeeded();
            }
        }
    }

    // 幽霊役職の設定からハッシュを生成するメソッド
    private string GenerateGhostRoleSettingsHash(GhostRoleId ghostRoleId)
    {
        var ghostRoleOption = RoleOptionManager.GhostRoleOptions.FirstOrDefault(o => o.RoleId == ghostRoleId);
        if (ghostRoleOption == null) return string.Empty;

        StringBuilder sb = new();

        // 役職の基本情報を含める
        sb.Append($"{ghostRoleOption.RoleId} x{ghostRoleOption.NumberOfCrews} ({ghostRoleOption.Percentage}%)\n");

        // 役職の詳細設定を含める
        foreach (var option in ghostRoleOption.Options.Where(o => o.ShouldDisplay()))
        {
            sb.Append($"{option.Name}: {option.GetCurrentSelectionString()}\n");
        }

        return sb.ToString();
    }

    // モディファイアの設定からハッシュを生成するメソッド
    private string GenerateModifierSettingsHash(ModifierRoleId modifierRoleId)
    {
        var modifierRoleOption = RoleOptionManager.ModifierRoleOptions.FirstOrDefault(o => o.ModifierRoleId == modifierRoleId);
        if (modifierRoleOption == null) return string.Empty;

        StringBuilder sb = new();

        // 役職の基本情報を含める
        sb.Append($"{modifierRoleOption.ModifierRoleId} x{modifierRoleOption.NumberOfCrews} ({modifierRoleOption.Percentage}%)\n");

        // 役職の詳細設定を含める
        foreach (var option in modifierRoleOption.Options.Where(o => o.ShouldDisplay()))
        {
            sb.Append($"{option.Name}: {option.GetCurrentSelectionString()}\n");
        }

        return sb.ToString();
    }

    // 表示中の役職詳細を更新する
    private void UpdateRoleDetailIfNeeded()
    {
        if (currentDisplayedRoleId is GhostRoleId ghostRoleId)
        {
            // 幽霊役職の設定情報のみを更新
            IGhostRoleBase ghostRoleBase = CustomRoleManager.GetGhostRoleById(ghostRoleId);
            if (ghostRoleBase != null)
            {
                string roleSettings = RoleDetailMenu.GenerateRoleSettingsText(ghostRoleBase);
                RoleDetailMenu.SetTextComponent(RoleDetailObject.transform.Find("Scroller/RoleInformation"), "RoleSettings", Color.white, roleSettings);
                Logger.Info($"幽霊役職 {ghostRoleId} の設定情報を更新しました");
            }
        }
        else if (currentDisplayedRoleId is ModifierRoleId modifierRoleId)
        {
            // モディファイアの設定情報のみを更新
            IModifierBase modifierBase = CustomRoleManager.GetModifierById(modifierRoleId);
            if (modifierBase != null)
            {
                string roleSettings = RoleDetailMenu.GenerateRoleSettingsText(modifierBase);
                RoleDetailMenu.SetTextComponent(RoleDetailObject.transform.Find("Scroller/RoleInformation"), "RoleSettings", Color.white, roleSettings);
                Logger.Info($"モディファイア {modifierRoleId} の設定情報を更新しました");
            }
        }
    }

    public override void Hide(GameObject Container)
    {
        // メニューを非表示にする処理を記述
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);
        if (RoleDetailObject != null)
            GameObject.Destroy(RoleDetailObject);

        // 役職詳細表示中フラグをリセット
        isShowingRoleDetail = false;
    }
}