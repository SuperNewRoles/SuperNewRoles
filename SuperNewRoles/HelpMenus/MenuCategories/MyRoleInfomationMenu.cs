using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.HelpMenus.MenuCategories;

public class MyRoleInfomationMenu : HelpMenuCategoryBase
{
    public override string Name => "MyRoleInformation";
    private GameObject Container;
    private GameObject MenuObject;
    public override void Show(GameObject Container)
    {
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);

        this.Container = Container;
        // MyRoleInfomationHelpMenu from AssetManager
        MenuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("MyRoleInfomationHelpMenu"), Container.transform);
        MenuObject.transform.localPosition = Vector3.zero;
        MenuObject.transform.localScale = Vector3.one;
        MenuObject.transform.localRotation = Quaternion.identity;
    }
    public override void UpdateShow()
    {
        // MenuObjectがnullの場合は処理を中断
        if (MenuObject == null) return;

        // ロビーにいるかどうかを判定
        bool inLobby = GameStartManager.InstanceExists;
        // "InLobbyText"オブジェクトを取得
        var inLobbyText = MenuObject.transform.Find("InLobbyText")?.gameObject;

        // inLobbyTextがnullの場合は処理を中断
        if (inLobbyText == null) return;
        // ロビーにいるかどうかに応じて、MenuObjectの子オブジェクトのアクティブ状態を切り替える
        ModHelpers.SetActiveAllObject(MenuObject.GetChildren(), !inLobby);

        // inLobbyTextオブジェクトのアクティブ状態をロビーにいるかどうかに合わせて設定
        inLobbyText.SetActive(inLobby);

        // ロビーにいる場合
        if (inLobby)
        {
            // inLobbyTextのTextMeshProコンポーネントのテキストをローカライズされたテキストに設定
            inLobbyText.GetComponent<TextMeshPro>().text =
                ModTranslation.GetString("HelpMenu.MyRoleInformation.InLobbyText");
            return;
        }

        // 現在の役職を表示

        // MenuObject生成後にRoleButtonsの設定を追加
        // BulkRoleButtonをAssetManagerから取得して、RoleButtonsコンテナに追加
        var roleButtonsContainer = MenuObject.transform.Find("RoleButtons")?.gameObject;
        if (roleButtonsContainer != null)
        {
            // roleButtonsContainerの子ボタン削除
            foreach (var child in roleButtonsContainer.GetChildren())
            {
                if (child.name.StartsWith("RoleButton_"))
                    GameObject.Destroy(child);
            }

            // サンプルボタンの代わりに実際の役職ボタンを生成
            IRoleBase[] roles = [ExPlayerControl.LocalPlayer.roleBase, CustomRoleManager.AllRoles.FirstOrDefault(x => x != null)]; // 役職IDのリストを取得（実装に応じて要調整）
            var bulkRoleButtonAsset = AssetManager.GetAsset<GameObject>("BulkRoleButton");

            // ボタン生成位置の基準値
            float baseY = 0f;
            float yInterval = 0.8f; // ボタン間の垂直間隔

            for (int i = 0; i < roles.Length; i++)
            {
                var role = roles[i];
                int totalRoles = roles.Length;
                float posX;
                if (totalRoles == 1)
                {
                    posX = 0f;
                }
                else if (totalRoles % 2 == 0)
                {
                    // 2個の場合: 左から1, -1 になるように設定（例: 2個なら、i=0で1, i=1で-1）
                    posX = ((totalRoles / 2 - i) * 2 - 1);
                }
                else
                {
                    // 3個以上の場合も中心に揃える（例: 3個なら、i=0で2, i=1で0, i=2で-2）
                    posX = (((totalRoles - 1) / 2 - i) * 2);
                }
                CreateRoleButton(role, roleButtonsContainer.transform, new Vector3(posX, 0f, 0f));
            }
        }
        else
        {
            Logger.Info("RoleButtonsコンテナが見つかりませんでした。");
        }
    }
    public override void Hide(GameObject Container)
    {
        // メニューを非表示にする処理を記述
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);
    }

    // 役職ボタン生成用のヘルパー関数
    private void CreateRoleButton(IRoleBase role, Transform parent, Vector3 position)
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
            textComponent.text = ModHelpers.CsWithTranslation(role.RoleColor, role.Role.ToString()); // 翻訳キーは役職ID+"Name"
        }

        // ボタンイベント設定
        var passiveButton = bulkRoleButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { bulkRoleButton.GetComponent<BoxCollider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            OnRoleButtonClicked(role);
        }));

        // マウスオーバー/アウト処理
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            bulkRoleButton.transform.Find("Selected")?.gameObject.SetActive(true);
        }));

        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            bulkRoleButton.transform.Find("Selected")?.gameObject.SetActive(false);
        }));
    }

    // ボタンクリック時の処理
    private void OnRoleButtonClicked(IRoleBase role)
    {
        Logger.Info($"{role.Role} ボタンがクリックされました");
        ShowRoleInformation(role);
    }

    /// <summary>
    /// 役職の情報を表示する
    /// </summary>
    /// <param name="role">表示する役職</param>
    private void ShowRoleInformation(IRoleBase role)
    {
        Transform roleInformation = MenuObject.transform.Find("Scroller/RoleInformation");
        if (roleInformation == null)
        {
            Logger.Error("RoleInformationオブジェクトが見つかりませんでした。");
            return;
        }

        roleInformation.gameObject.SetActive(true);

        // テキスト設定処理を共通化
        SetTextComponent(roleInformation, "RoleName", role.RoleColor, $"{role.Role}");
        SetTextComponent(roleInformation, "RoleTeam", GetTeamColor(role.AssignedTeam), role.AssignedTeam.ToString());
        SetTextComponent(roleInformation, "RoleDescription", Color.white, $"{role.Role}.Description");
        SetTextComponent(roleInformation, "RoleSettingsTitle", Color.white, "HelpMenu.MyRoleInformation.RoleSettingsTitle");
        string roleSettings = GenerateRoleSettingsText(role);
        SetTextComponent(roleInformation, "RoleSettings", Color.white, roleSettings);
    }

    private string GenerateRoleSettingsText(IRoleBase role)
    {
        var roleOption = RoleOptionManager.RoleOptions.FirstOrDefault(o => o.RoleId == role.Role);
        if (roleOption == null)
        {
            return $"{role.Role}.Settings";
        }
        var settings = roleOption.Options.Select(o => $"{o.Name}: {o.GetCurrentSelectionString()}");
        return string.Join("\n", settings);
    }

    #region HelperMethods
    /// <summary>テキストコンポーネントの設定を共通処理化</summary>
    private void SetTextComponent(Transform parent, string childName, Color color, string text)
    {
        var textComponent = parent.Find(childName)?.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = ModHelpers.CsWithTranslation(color, text);
        }
        else
            Logger.Error($"{childName} TextMeshProコンポーネントが見つかりませんでした。");
    }

    /// <summary>陣営に応じた色を取得（TODO: 後でModHelpersとかに移動）</summary>
    private Color GetTeamColor(AssignedTeamType team)
    {
        return team switch
        {
            AssignedTeamType.Crewmate => Color.white,
            AssignedTeamType.Neutral => Color.gray,
            _ => Palette.ImpostorRed
        };
    }
    #endregion
}