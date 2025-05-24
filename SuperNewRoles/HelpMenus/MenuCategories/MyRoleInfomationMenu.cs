using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.HelpMenus.MenuCategories;

public class MyRoleInfomationMenu : HelpMenuCategoryBase
{
    public override string Name => "MyRoleInformation";
    public override HelpMenuCategory Category => HelpMenuCategory.MyRoleInfomation;
    private GameObject Container;
    private GameObject MenuObject;
    private GameObject selectedButton;

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

        // RoleDetailMenuにMenuObjectを設定
        RoleDetailMenu.SetMenuObject(MenuObject);
    }
    public override void UpdateShow()
    {
        // MenuObjectがnullの場合は処理を中断
        if (MenuObject == null) return;

        // ロビーにいるかどうかを判定
        bool inLobby = AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined;
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

            // 役職IDのリストを取得
            List<IRoleInformation> roles = [ExPlayerControl.LocalPlayer.roleBase];
            foreach (var modifier in ExPlayerControl.LocalPlayer.ModifierRoleBases)
                roles.Add(modifier);
            if (ExPlayerControl.LocalPlayer.GhostRoleBase != null)
                roles.Add(ExPlayerControl.LocalPlayer.GhostRoleBase);

            var bulkRoleButtonAsset = AssetManager.GetAsset<GameObject>("BulkRoleButton");

            // ボタン生成位置の基準値
            float baseY = 0f;
            float yInterval = 0.8f; // ボタン間の垂直間隔

            GameObject firstButton = null;
            for (int i = 0; i < roles.Count; i++)
            {
                var role = roles[i];
                int totalRoles = roles.Count;
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
                var button = CreateRoleButton(role, roleButtonsContainer.transform, new Vector3(posX, 0f, 0f));
                if (i == 0)
                    firstButton = button;
            }
            if (firstButton != null)
                RoleDetailMenu.OnRoleButtonClicked(ExPlayerControl.LocalPlayer.roleBase, firstButton);
            else
                RoleDetailMenu.OnRoleButtonClicked(ExPlayerControl.LocalPlayer.roleBase, null);
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
    private GameObject CreateRoleButton(IRoleInformation role, Transform parent, Vector3 position)
    {
        var bulkRoleButtonAsset = AssetManager.GetAsset<GameObject>("BulkRoleButton");
        var bulkRoleButton = GameObject.Instantiate(bulkRoleButtonAsset, parent);
        bulkRoleButton.name = $"RoleButton_{role.RoleName}";
        bulkRoleButton.transform.localPosition = position;
        bulkRoleButton.transform.localScale = Vector3.one * 0.36f;

        // テキスト設定
        var textComponent = bulkRoleButton.transform.Find("Text")?.GetComponent<TextMeshPro>();
        if (textComponent != null)
        {
            textComponent.text = ModHelpers.CsWithTranslation(role.RoleColor, role.RoleName); // 翻訳キーは役職ID+"Name"
        }

        // ボタンイベント設定
        var passiveButton = bulkRoleButton.AddComponent<PassiveButton>();
        passiveButton.Colliders = new Collider2D[] { bulkRoleButton.GetComponent<BoxCollider2D>() };
        passiveButton.OnClick = new();
        passiveButton.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            RoleDetailMenu.OnRoleButtonClicked(role, bulkRoleButton);
        }));

        // マウスオーバー/アウト処理
        passiveButton.OnMouseOver = new();
        passiveButton.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            if (bulkRoleButton != selectedButton)
                bulkRoleButton.transform.Find("Selected")?.gameObject.SetActive(true);
        }));

        passiveButton.OnMouseOut = new();
        passiveButton.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() =>
        {
            if (bulkRoleButton != selectedButton)
                bulkRoleButton.transform.Find("Selected")?.gameObject.SetActive(false);
        }));

        return bulkRoleButton;
    }
}