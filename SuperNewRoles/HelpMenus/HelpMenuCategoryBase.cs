using UnityEngine;

namespace SuperNewRoles.HelpMenus;

public abstract class HelpMenuCategoryBase
{
    public abstract string Name { get; }
    public abstract HelpMenuCategory Category { get; }
    public abstract void Show(GameObject Container);
    public abstract void Hide(GameObject Container);
    public abstract void UpdateShow();
    public virtual void OnUpdate() { }
}

public enum HelpMenuCategory
{
    MyRoleInfomation,
    AssignmentsSettingInfomation,
    AssignmentsSettingInfomation2,
    ModSettingsInformation,
    RoleDictionary
}