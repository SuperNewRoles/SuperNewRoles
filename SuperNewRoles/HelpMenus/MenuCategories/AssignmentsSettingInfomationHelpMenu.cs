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
    private DelayTask _updateShowTask;

    public override void Show(GameObject Container)
    {
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);

        this.Container = Container;
        // MyRoleInfomationHelpMenu from AssetManager
        MenuObject = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("AssignmentsSettingInfomationHelpMenu"), Container.transform);
        MenuObject.transform.localPosition = Vector3.zero;
        MenuObject.transform.localScale = Vector3.one;
        MenuObject.transform.localRotation = Quaternion.identity;
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
        teamNameTMP.text = ModHelpers.CsWithTranslation(MyRoleInfomationMenu.GetTeamColor(assignedTeam), team) + " (" + ModTranslation.GetString("People", maxRoles) + ")";

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
    public override void OnUpdate()
    {
        StringBuilder sb = new();
        foreach (var role in RoleOptionManager.RoleOptions)
        {
            sb.Append($"{role.RoleId} x{role.NumberOfCrews} ({role.Percentage}%)\n");
        }
        string hash = sb.ToString();
        if (lastHash != hash)
        {
            lastHash = hash;
            DelayTask.UpdateOrAdd(() => UpdateShow(), 0.5f, ref _updateShowTask, "UpdateShowTask");
        }
    }
    public override void Hide(GameObject Container)
    {
        // メニューを非表示にする処理を記述
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);
    }
}