using System;
using System.Collections.Generic;

namespace SuperNewRoles.CustomOptions;
public abstract class OptionMenuBase
{
    public static Dictionary<Type, OptionMenuBase> OptionMenus { get; } = new();
    public abstract void Hide();
    public abstract void UpdateOptionDisplay();
    public OptionMenuBase()
    {
        Register(this);
    }
    public static void Register<T>(T optionMenu) where T : OptionMenuBase
    {
        // インスタンスの実際の型をキーとして使用するように変更
        OptionMenus[optionMenu.GetType()] = optionMenu;
    }
    public static void HideAll()
    {
        foreach (var optionMenu in OptionMenus.Values)
        {
            optionMenu.Hide();
        }
    }
    public static void UpdateOptionDisplayAll()
    {
        foreach (var optionMenu in OptionMenus.Values)
        {
            optionMenu.UpdateOptionDisplay();
        }
    }
}