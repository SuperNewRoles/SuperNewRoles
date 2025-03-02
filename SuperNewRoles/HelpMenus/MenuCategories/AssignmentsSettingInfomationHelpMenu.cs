using System.Linq;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.HelpMenus.MenuCategories;

public class AssignmentsSettingInfomationHelpMenu : HelpMenuCategoryBase
{
    public override string Name => "AssignmentsSettingInfomation";
    public override HelpMenuCategory Category => HelpMenuCategory.AssignmentsSettingInfomation;
    private GameObject Container;
    private GameObject MenuObject;

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
        var rolesTMP = info.Find("Roles").GetComponent<TextMeshPro>();
        teamNameTMP.text = ModHelpers.CsWithTranslation(MyRoleInfomationMenu.GetTeamColor(assignedTeam), team) + " (" + ModTranslation.GetString("People", maxRoles) + ")";
        rolesTMP.text = "";
        foreach (var role in RoleOptionManager.RoleOptions)
        {
            if (role.AssignTeam != assignedTeam) continue;
            if (role.Percentage == 0 || role.NumberOfCrews == 0) continue;
            rolesTMP.text += $"{ModTranslation.GetString(role.RoleId.ToString())} x{role.NumberOfCrews} ({role.Percentage}%)\n";
        }
    }
    public override void Hide(GameObject Container)
    {
        // メニューを非表示にする処理を記述
        if (MenuObject != null)
            GameObject.Destroy(MenuObject);
    }
}