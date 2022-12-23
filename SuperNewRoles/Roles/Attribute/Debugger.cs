using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Attribute;
public enum DebugTabs
{
    FunctionsTab,
    PlayerControlTab,
}

public static class Debugger
{
    public static DebugTabs currentTab = DebugTabs.FunctionsTab;
    public static PlayerControl target;

    public static void CreateDebugMenu(ShapeshifterMinigame minigame)
    {
        minigame.transform.localScale = new(1.05f, 1.05f, 1.05f);
        ReShowPanels(minigame);
    }



    //** ファンクション **//
    // 会議起こし
    public static DebugPanel StartMeeting = new("StartMeeting", DebugTabs.FunctionsTab,
        () => {

        });

    //** プレイヤー操作 **//
    // テレポート
    public static DebugPanel Teleport = new("Teleport", DebugTabs.PlayerControlTab,
        () => {

        });

    // 死体通報
    public static DebugPanel ReportDeadbody = new("ReportDeadbody", DebugTabs.PlayerControlTab,
        () => {

        });

    // 追放
    public static DebugPanel Exile = new("Exile", DebugTabs.PlayerControlTab,
        () => {

        });

    // 殺害
    public static DebugPanel Kill = new("Kill", DebugTabs.PlayerControlTab,
        () => {

        });

    // 蘇生
    public static DebugPanel Revive = new("Revive", DebugTabs.PlayerControlTab,
        () => {

        });

    // 死体掃除
    public static DebugPanel CleanDeadbody = new("CleanDeadbody", DebugTabs.PlayerControlTab,
        () => {

        });

    // 役職付与
    public static DebugPanel SetRole = new("SetRole", DebugTabs.PlayerControlTab,
        () => {

        });

    public static void ReShowPanels(ShapeshifterMinigame minigame)
    {
        foreach (ShapeshifterPanel panel in GameObject.FindObjectsOfType<ShapeshifterPanel>()) GameObject.Destroy(panel.gameObject);

        int index = 0;
        foreach (var data in DebugPanel.panels)
        {
            if (currentTab != data.tab) continue;

            int num = index % 3;
            int num2 = index / 3;
            ShapeshifterPanel panel = GameObject.Instantiate(minigame.PanelPrefab, minigame.transform);
            panel.transform.localPosition = new Vector3(minigame.XStart + (float)num * minigame.XOffset, minigame.YStart + (float)num2 * minigame.YOffset, -1f);
            static void Create(ShapeshifterPanel panel, int index, Action action)
            {
                panel.SetPlayer(index, CachedPlayer.LocalPlayer.Data, (Action)(() =>
                {
                    if (MeetingHud.Instance != null) MeetingHud.Instance.transform.FindChild("ButtonStuff").gameObject.SetActive(true);
                    action();
                }));
            }
            Create(panel, index, data.onClick);

            panel.PlayerIcon.gameObject.SetActive(false);
            panel.LevelNumberText.transform.parent.gameObject.SetActive(false);
            panel.ColorBlindName.gameObject.SetActive(false);
            panel.transform.FindChild("Nameplate").GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
            panel.NameText.text = ModTranslation.GetString(data.text);
            panel.NameText.transform.localPosition = new(0, 0, -0.1f);
            index++;
        }
    }
}

public class DebugPanel
{
    public static List<DebugPanel> panels = new();

    public string text;
    public DebugTabs tab;
    public Action onClick;
    public DebugPanel(string text, DebugTabs tab, Action onClick)
    {
        this.text = text;
        this.tab = tab;
        this.onClick = onClick;
        panels.Add(this);
    }
}