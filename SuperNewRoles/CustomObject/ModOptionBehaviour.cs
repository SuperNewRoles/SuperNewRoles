using System.Collections.Generic;

namespace SuperNewRoles.CustomObject;

public class ModOptionBehaviour : OptionBehaviour
{
    public ModSettingsMenu SettingsMenu;

    public List<PassiveButton> ControllerSelectable = new();

    public CustomOption ParentCustomOption;

    public CategoryHeaderMasked HeaderMasked;

    public virtual void UpdateValue() { }
}
