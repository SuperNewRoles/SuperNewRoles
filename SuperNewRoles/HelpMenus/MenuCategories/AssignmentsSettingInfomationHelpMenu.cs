using System.Linq;
using System.Text;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using SuperNewRoles.HelpMenus;
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

        // MyRoleInfomationHelpMenu from AssetManager
        MenuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("AssignmentsSettingInfomationHelpMenu"), Container.transform);
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
        (string, AssignedTeamType, int)[] teams = [
            ("Impostor", AssignedTeamType.Impostor, AssignRoles.MaxImpostors),
            ("Neutral", AssignedTeamType.Neutral, AssignRoles.MaxNeutrals),
            ("Crewmate", AssignedTeamType.Crewmate, AssignRoles.MaxCrews)
        ];
        float maxContentYBoundsMax = 0;
        foreach (var team in teams)
        {
            SetupInformation(roleInformation, team.Item1, team.Item2, team.Item3, out float ContentYBoundsMax);
            maxContentYBoundsMax = Mathf.Max(maxContentYBoundsMax, ContentYBoundsMax);
        }
        var scroller = MenuObject.transform.Find("Scroller").GetComponent<Scroller>();
        scroller.ContentYBounds.max = maxContentYBoundsMax;
    }
    private void SetupInformation(Transform infoObject, string team, AssignedTeamType assignedTeam, int maxRoles, out float ContentYBoundsMax)
    {
        // 指定チームのオブジェクトを取得
        var info = infoObject.Find(team);
        if (info == null)
        {
            Logger.Error($"{team}オブジェクトが見つかりませんでした。");
            ContentYBoundsMax = 0;
            return;
        }

        // 「TeamName」テキストコンポーネントの設定
        var teamNameTMP = info.Find("TeamName").GetComponent<TextMeshPro>();
        if (teamNameTMP != null)
        {
            teamNameTMP.text = $"{ModHelpers.CsWithTranslation(RoleDetailMenu.GetTeamColor(assignedTeam), team)} ({ModTranslation.GetString("People", maxRoles)})";
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

        // ロールの基本テキストを生成するローカル関数
        string GetRoleText(RoleOptionManager.RoleOption role, Color? color = null) =>
        $"<b>{ModHelpers.CsWithTranslation(color ?? role.RoleColor, role.RoleId.ToString())}</b> x{role.NumberOfCrews} ({role.Percentage}%)\n";

        int index = 0;
        // 各ロールについて処理
        foreach (var role in RoleOptionManager.RoleOptions)
        {
            if (role.AssignTeam != assignedTeam || role.Percentage == 0 || role.NumberOfCrews == 0)
                continue;

            // テンプレートからロール情報オブジェクトを複製
            var newRole = GameObject.Instantiate(rolesTemplate, info);
            newRole.gameObject.SetActive(true);
            newRole.transform.localPosition = new Vector3(0, yPos, 0);

            string baseRoleText = GetRoleText(role);
            newRole.text = baseRoleText;

            // PassiveButtonの設定
            var boxCollider = newRole.GetComponent<BoxCollider2D>();
            PassiveButton passiveButton = newRole.gameObject.AddComponent<PassiveButton>();
            passiveButton.Colliders = new Collider2D[] { boxCollider };

            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener((UnityAction)(() =>
            {
                Logger.Info($"{role.RoleId} Selected");
                ShowRoleDetail(role.RoleId);
            }));

            passiveButton.OnMouseOver = new();
            passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
            {
                newRole.text = ModHelpers.Cs(Color.green, GetRoleText(role, Color.green));
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

    // 役職詳細を表示する
    private void ShowRoleDetail(RoleId roleId)
    {
        // 既存の詳細表示を削除
        if (RoleDetailObject != null)
            GameObject.Destroy(RoleDetailObject);

        // 役職詳細表示中フラグを設定
        isShowingRoleDetail = true;

        // 役職詳細オブジェクトを作成
        RoleDetailObject = RoleDetailHelper.ShowRoleDetail(roleId, Container, MenuObject, CloseRoleDetail);

        // 現在表示中の役職IDを保存
        currentDisplayedRoleId = roleId;

        // 役職設定のハッシュを初期化
        lastRoleSettingsHash = GenerateRoleSettingsHash(roleId);
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