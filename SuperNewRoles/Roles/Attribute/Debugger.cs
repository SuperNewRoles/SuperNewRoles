using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Attribute;
public enum DebugTabs
{
    FunctionsMenu,
    PlayerController,
}

public static class Debugger
{
    public static DebugTabs currentTab = DebugTabs.FunctionsMenu;
    public static PlayerControl target;

    public static void CreateDebugMenu(ShapeshifterMinigame minigame)
    {
        minigame.transform.localScale *= 1.1f;
    }

    public static List<string> TabDictionary = new()
    {
        "Functions",
        "PlayerControls"
    };

    //デバッガーのファンクションボタン
    public static Dictionary</*Dictionary<*/string, Action/*>, DebugTabs*/> FunctionDictionary = new()
    {
        //{
            {
                "",
                () => {

                }
            }
        //}
    };
}

public class DebugPanel
{
    public static List<DebugPanel> Panels;

    public DebugPanel(string text, DebugTabs tab, Action action)
    {

    }
}