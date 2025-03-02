using System.Linq;
using System.Text;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.HelpMenus.MenuCategories;

public class AssignmentsSettingInfomationHelpMenu : HelpMenuCategoryBase
{
    public override string Name => "AssignmentsSettingInfomation";
    public override HelpMenuCategory Category => HelpMenuCategory.AssignmentsSettingInfomation;
    private GameObject Container;
    private GameObject MenuObject;
    public string lastHash;
    private string lastRoleSettingsHash; // 現在表示中の役職の設定ハッシュ
    private DelayTask _updateShowTask;
    private GameObject RoleDetailObject; // 役職詳細表示用オブジェクト
    private GameObject LeftButtonsObject; // 左側のボタン

    // 役職詳細表示中かどうか
    private bool isShowingRoleDetail = false;
    // 現在表示中の役職ID
    private RoleId currentDisplayedRoleId = RoleId.None;

    public override void Show(GameObject Container)
    {
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);

        this.Container = Container;

        // 役職詳細表示中なら閉じる
        if (isShowingRoleDetail)
        {
            CloseRoleDetail();
            isShowingRoleDetail = false;
        }

        // MyRoleInfomationHelpMenu from AssetManager
        MenuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("AssignmentsSettingInfomationHelpMenu"), Container.transform);
        MenuObject.transform.localPosition = Vector3.zero;
        MenuObject.transform.localScale = Vector3.one;
        MenuObject.transform.localRotation = Quaternion.identity;

        // 左側のボタンを保存
        LeftButtonsObject = GameObject.Find("HelpMenuObject/LeftButtons");
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
        (string, AssignedTeamType, int)[] teams = [
            ("Impostor", AssignedTeamType.Impostor, AssignRoles.MaxImpostors),
            ("Neutral", AssignedTeamType.Neutral, AssignRoles.MaxNeutrals),
            ("Crewmate", AssignedTeamType.Crewmate, AssignRoles.MaxCrews)
        ];
        foreach (var team in teams)
        {
            SetupInformation(roleInformation, team.Item1, team.Item2, team.Item3);
        }
    }
    private void SetupInformation(Transform infoObject, string team, AssignedTeamType assignedTeam, int maxRoles)
    {
        var info = infoObject.Find(team);
        if (info == null)
        {
            Logger.Error($"{team}オブジェクトが見つかりませんでした。");
            return;
        }
        var teamNameTMP = info.Find("TeamName").GetComponent<TextMeshPro>();
        teamNameTMP.text = ModHelpers.CsWithTranslation(RoleDetailMenu.GetTeamColor(assignedTeam), team) + " (" + ModTranslation.GetString("People", maxRoles) + ")";

        // Rolesテンプレートを取得
        var rolesTemplate = info.Find("Roles").GetComponent<TextMeshPro>();
        rolesTemplate.gameObject.SetActive(false); // テンプレートは非表示に

        float yPos = 1.42f;
        foreach (var role in RoleOptionManager.RoleOptions)
        {
            if (role.AssignTeam != assignedTeam) continue;
            if (role.Percentage == 0 || role.NumberOfCrews == 0) continue;

            // Rolesを複製して位置調整
            var newRole = GameObject.Instantiate(rolesTemplate, info);
            newRole.gameObject.SetActive(true);
            newRole.transform.localPosition = new Vector3(0, yPos, 0);

            newRole.text = $"{ModTranslation.GetString(role.RoleId.ToString())} x{role.NumberOfCrews} ({role.Percentage}%)\n";

            PassiveButton passiveButton = newRole.gameObject.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { newRole.GetComponent<BoxCollider2D>() };
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info($"{role.RoleId} Selected");
                ShowRoleDetail(role.RoleId);
            }));
            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                newRole.GetComponent<TextMeshPro>().color = Color.green; // 緑色に変更
            }));
            passiveButton.OnMouseOut = new();
            passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
            {
                newRole.GetComponent<TextMeshPro>().color = Color.white; // 白色に戻す
            }));

            yPos -= 0.5f; // Y座標を調整
        }
    }

    // 役職詳細を表示する
    private void ShowRoleDetail(RoleId roleId)
    {
        // 既存の詳細表示を削除
        if (RoleDetailObject != null)
            GameObject.Destroy(RoleDetailObject);

        // 役職詳細表示中フラグを設定
        isShowingRoleDetail = true;

        // Assignmentsのタブを非表示にする
        if (MenuObject != null)
            MenuObject.SetActive(false);

        // 左側のボタンを非表示にする
        if (LeftButtonsObject != null)
            LeftButtonsObject.SetActive(false);

        // MyRoleInfomationHelpMenuをAssetManagerから取得
        RoleDetailObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("MyRoleInfomationHelpMenu"), Container.transform);
        RoleDetailObject.transform.localPosition = Vector3.zero;
        RoleDetailObject.transform.localScale = Vector3.one;
        RoleDetailObject.transform.localRotation = Quaternion.identity;

        var mask = RoleDetailObject.transform.Find("Mask");
        if (mask != null)
        {
            mask.transform.localScale = new Vector3(mask.transform.localScale.x, 4.19f, mask.transform.localScale.z);
            mask.transform.localPosition = new Vector3(mask.transform.localPosition.x, 0, mask.transform.localPosition.z);
        }
        var scroller = RoleDetailObject.transform.Find("Scroller");
        if (scroller != null)
        {
            scroller.transform.localPosition = new Vector3(scroller.transform.localPosition.x, 0.45f, scroller.transform.localPosition.z);
        }

        // RoleDetailMenuにMenuObjectを設定
        RoleDetailMenu.SetMenuObject(RoleDetailObject);

        // InLobbyTextを非表示にする
        var inLobbyText = RoleDetailObject.transform.Find("InLobbyText")?.gameObject;
        if (inLobbyText != null)
            inLobbyText.SetActive(false);

        // 役職ボタンを生成
        var roleButtonsContainer = RoleDetailObject.transform.Find("RoleButtons")?.gameObject;
        if (roleButtonsContainer != null)
        {
            // roleButtonsContainerの子ボタン削除
            foreach (var child in roleButtonsContainer.GetChildren())
            {
                if (child.name.StartsWith("RoleButton_"))
                    GameObject.Destroy(child);
            }

            // 選択された役職のIRoleBaseを取得
            IRoleBase roleBase = CustomRoleManager.GetRoleById(roleId);
            if (roleBase != null)
            {
                // 役職ボタンを生成
                var button = CreateRoleButton(roleBase, roleButtonsContainer.transform, Vector3.zero);
                // 役職詳細を表示
                RoleDetailMenu.OnRoleButtonClicked(roleBase, button);
            }
            else
            {
                Logger.Error($"役職ID {roleId} のIRoleBaseが見つかりませんでした。");
            }
        }
        else
        {
            Logger.Error("RoleButtonsコンテナが見つかりませんでした。");
        }

        // 閉じるボタンを追加
        var closeButton = RoleDetailObject.transform.Find("CloseButton")?.gameObject;
        if (closeButton == null)
        {
            // 閉じるボタンがない場合は作成
            closeButton = new GameObject("CloseButton");
            closeButton.transform.SetParent(RoleDetailObject.transform);
            closeButton.transform.localPosition = new Vector3(2.5f, 2.5f, 0);
            closeButton.transform.localScale = Vector3.one;

            // ボタンのコンポーネント追加
            var buttonRenderer = closeButton.AddComponent<SpriteRenderer>();
            buttonRenderer.sprite = AssetManager.GetAsset<Sprite>("CloseButton");

            var collider = closeButton.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.5f, 0.5f);

            var passiveButton = closeButton.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { collider };
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                CloseRoleDetail();
            }));
        }
        else
        {
            // 既存の閉じるボタンのイベント設定
            var passiveButton = closeButton.GetComponent<PassiveButton>();
            if (passiveButton != null)
            {
                passiveButton.OnClick = new();
                passiveButton.OnClick.AddListener((UnityAction)(() =>
                {
                    CloseRoleDetail();
                }));
            }
        }

        // 戻るボタンを追加
        var backButton = new GameObject("BackButton");
        backButton.transform.SetParent(RoleDetailObject.transform);
        backButton.transform.localPosition = new Vector3(-2.5f, 2.5f, 0);
        backButton.transform.localScale = Vector3.one;

        // ボタンのコンポーネント追加
        var backButtonRenderer = backButton.AddComponent<SpriteRenderer>();
        backButtonRenderer.sprite = AssetManager.GetAsset<Sprite>("BackButton");

        var backCollider = backButton.AddComponent<BoxCollider2D>();
        backCollider.size = new Vector2(0.5f, 0.5f);

        var backPassiveButton = backButton.AddComponent<PassiveButton>();
        backPassiveButton.Colliders = new Collider2D[] { backCollider };
        backPassiveButton.OnClick = new();
        backPassiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            CloseRoleDetail();
        }));

        // 現在表示中の役職IDを保存
        currentDisplayedRoleId = roleId;

        // 役職設定のハッシュを初期化
        lastRoleSettingsHash = GenerateRoleSettingsHash(roleId);
    }

    // 役職詳細を閉じる
    private void CloseRoleDetail()
    {
        // 役職詳細を削除
        if (RoleDetailObject != null)
        {
            GameObject.Destroy(RoleDetailObject);
            RoleDetailObject = null;
        }

        // 役職詳細表示中フラグをリセット
        isShowingRoleDetail = false;

        // Assignmentsのタブを再表示
        if (MenuObject != null)
            MenuObject.SetActive(true);

        // 左側のボタンを再表示
        if (LeftButtonsObject != null)
            LeftButtonsObject.SetActive(true);

        // 現在表示中の役職IDをリセット
        currentDisplayedRoleId = RoleId.None;

        // 役職設定のハッシュをリセット
        lastRoleSettingsHash = string.Empty;
    }

    // 役職ボタン生成用のヘルパー関数
    private GameObject CreateRoleButton(IRoleBase role, Transform parent, Vector3 position)
    {
        var bulkRoleButtonAsset = AssetManager.GetAsset<GameObject>("BulkRoleButton");
        var bulkRoleButton = GameObject.Instantiate(bulkRoleButtonAsset, parent);
        bulkRoleButton.name = $"RoleButton_{role.Role}";
        bulkRoleButton.transform.localPosition = position;
        bulkRoleButton.transform.localScale = Vector3.one * 0.36f;

        // テキスト設定
        var textComponent = bulkRoleButton.transform.Find("Text")?.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = ModHelpers.CsWithTranslation(role.RoleColor, role.Role.ToString());
        }

        // ボタンイベント設定
        var passiveButton = bulkRoleButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { bulkRoleButton.GetComponent<BoxCollider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            RoleDetailMenu.OnRoleButtonClicked(role, bulkRoleButton);
        }));

        return bulkRoleButton;
    }

    public override void OnUpdate()
    {
        // 全体の役職リスト（人数・パーセンテージ）用のハッシュを生成
        StringBuilder sb = new();
        foreach (var role in RoleOptionManager.RoleOptions)
        {
            sb.Append($"{role.RoleId} x{role.NumberOfCrews} ({role.Percentage}%)\n");
        }
        string hash = sb.ToString();

        // 役職リストに変更があれば全体を更新
        if (lastHash != hash)
        {
            lastHash = hash;
            DelayTask.UpdateOrAdd(() => UpdateShow(), 0.5f, ref _updateShowTask, "UpdateShowTask");
        }

        // 役職詳細表示中の場合、現在表示中の役職の設定変更も検知する
        if (isShowingRoleDetail && currentDisplayedRoleId != RoleId.None && RoleDetailObject != null)
        {
            // 現在表示中の役職の設定ハッシュを生成
            string currentRoleSettingsHash = GenerateRoleSettingsHash(currentDisplayedRoleId);

            // 役職設定に変更があれば詳細表示を更新
            if (lastRoleSettingsHash != currentRoleSettingsHash)
            {
                lastRoleSettingsHash = currentRoleSettingsHash;
                UpdateRoleDetailIfNeeded();
            }
        }
    }

    // 役職の設定からハッシュを生成するメソッド
    private string GenerateRoleSettingsHash(RoleId roleId)
    {
        var roleOption = RoleOptionManager.RoleOptions.FirstOrDefault(o => o.RoleId == roleId);
        if (roleOption == null) return string.Empty;

        StringBuilder sb = new();

        // 役職の基本情報を含める
        sb.Append($"{roleOption.RoleId} x{roleOption.NumberOfCrews} ({roleOption.Percentage}%)\n");

        // 役職の詳細設定を含める
        foreach (var option in roleOption.Options.Where(o => o.ShouldDisplay()))
        {
            sb.Append($"{option.Name}: {option.GetCurrentSelectionString()}\n");
        }

        return sb.ToString();
    }

    // 表示中の役職詳細を更新する
    private void UpdateRoleDetailIfNeeded()
    {
        // 現在表示中の役職のIRoleBaseを取得
        IRoleBase roleBase = CustomRoleManager.GetRoleById(currentDisplayedRoleId);
        if (roleBase != null)
        {
            // 役職の設定情報のみを更新（全体を更新する必要はない）
            RoleDetailMenu.UpdateRoleSettingsInformation(roleBase);
            Logger.Info($"役職 {currentDisplayedRoleId} の設定情報を更新しました");
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