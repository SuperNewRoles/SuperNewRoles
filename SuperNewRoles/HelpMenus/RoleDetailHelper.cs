using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.HelpMenus;

/// <summary>
/// 役職詳細表示の共通機能を提供するヘルパークラス
/// </summary>
public static class RoleDetailHelper
{
    /// <summary>
    /// 役職詳細を表示する
    /// </summary>
    /// <param name="roleId">表示する役職ID</param>
    /// <param name="container">親コンテナ</param>
    /// <param name="menuObjectToHide">非表示にする元のメニューオブジェクト</param>
    /// <param name="roleDetailObject">設定する役職詳細オブジェクト参照</param>
    /// <param name="onCloseCallback">閉じるボタン押下時のコールバック</param>
    /// <returns>作成された役職詳細オブジェクト</returns>
    public static GameObject ShowRoleDetail(
        RoleId roleId,
        GameObject container,
        GameObject menuObjectToHide,
        System.Action onCloseCallback,
        bool useDirectRoleDetailLayout = false)
    {
        // メニューを非表示にする
        if (menuObjectToHide != null)
            menuObjectToHide.SetActive(false);

        // 左側のボタンを非表示にする
        var leftButtonsObject = GameObject.Find("HelpMenuObject/LeftButtons");
        if (leftButtonsObject != null)
            leftButtonsObject.SetActive(false);

        // MyRoleInfomationHelpMenuをAssetManagerから取得
        var roleDetailObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("MyRoleInfomationHelpMenu"), container.transform);
        roleDetailObject.transform.localPosition = Vector3.zero;
        roleDetailObject.transform.localScale = Vector3.one;
        roleDetailObject.transform.localRotation = Quaternion.identity;

        // マスクとスクローラーの調整
        var mask = roleDetailObject.transform.Find("Mask");
        if (mask != null)
        {
            mask.transform.localScale = new Vector3(mask.transform.localScale.x, 4.19f, mask.transform.localScale.z);
            mask.transform.localPosition = useDirectRoleDetailLayout
                ? new Vector3(-0.8221f, 0f, 0f)
                : new Vector3(mask.transform.localPosition.x, 0f, mask.transform.localPosition.z);
        }
        var scroller = roleDetailObject.transform.Find("Scroller");
        if (scroller != null)
        {
            scroller.transform.localPosition = useDirectRoleDetailLayout
                ? new Vector3(-2.15f, 0.45f, 0f)
                : new Vector3(scroller.transform.localPosition.x, 0.45f, scroller.transform.localPosition.z);
        }

        // RoleDetailMenuにMenuObjectを設定
        RoleDetailMenu.SetMenuObject(roleDetailObject);

        // InLobbyTextを非表示にする
        var inLobbyText = roleDetailObject.transform.Find("InLobbyText")?.gameObject;
        if (inLobbyText != null)
            inLobbyText.SetActive(false);

        // 役職ボタンを生成
        var roleButtonsContainer = roleDetailObject.transform.Find("RoleButtons")?.gameObject;
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
                var button = CreateDetailRoleButton(roleBase, roleButtonsContainer.transform, Vector3.zero);
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

        return roleDetailObject;
    }

    /// <summary>
    /// 役職詳細表示用のボタンを作成
    /// </summary>
    /// <param name="role">役職情報</param>
    /// <param name="parent">親オブジェクト</param>
    /// <param name="position">位置</param>
    /// <returns>作成されたボタン</returns>
    public static GameObject CreateDetailRoleButton(IRoleBase role, Transform parent, Vector3 position)
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
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            RoleDetailMenu.OnRoleButtonClicked(role, bulkRoleButton);
        }));
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOut = new();

        return bulkRoleButton;
    }

    /// <summary>
    /// 役職詳細を閉じる
    /// </summary>
    /// <param name="roleDetailObject">役職詳細オブジェクト</param>
    /// <param name="menuObjectToShow">表示する元のメニューオブジェクト</param>
    public static void CloseRoleDetail(GameObject roleDetailObject, GameObject menuObjectToShow)
    {
        // 役職詳細を削除
        if (roleDetailObject != null)
        {
            GameObject.Destroy(roleDetailObject);
        }

        // メニューを再表示
        if (menuObjectToShow != null)
        {
            menuObjectToShow.SetActive(true);
        }

        // 左側のボタンを再表示
        var leftButtonsObject = GameObject.Find("HelpMenuObject/LeftButtons");
        if (leftButtonsObject != null)
        {
            leftButtonsObject.SetActive(true);
        }
    }
}
