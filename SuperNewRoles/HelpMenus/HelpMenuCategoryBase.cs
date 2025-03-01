using UnityEngine;

namespace SuperNewRoles.HelpMenus;

public abstract class HelpMenuCategoryBase
{
    public abstract string Name { get; }
    public abstract void Show(GameObject Container);
    public abstract void Hide(GameObject Container);
}
