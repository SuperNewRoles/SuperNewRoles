using System;
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Attribute;
public enum TabType
{
    Normal,
    Role,
}
public enum DebugTabs
{
    Always,
    FunctionsTab,
    PlayersTab,             //プレイヤー
    PlayerControlTab,       //プレイヤー操作
    SetRole_Crewmate,
    SetRole_Impostor,
    SetRole_Neutral,
}

public static class Debugger
{
    public static DebugTabs currentTab = DebugTabs.FunctionsTab;
    public static ShapeshifterMinigame minigame;
    public static PlayerControl target;
    public static bool canSeeRole = false;

    public static void CreateDebugMenu(ShapeshifterMinigame minigame)
    {
        Debugger.minigame = minigame;
        minigame.transform.localScale = new(1.05f, 1.05f, 1.05f);
        currentTab = DebugTabs.FunctionsTab;
        ReShowPanels();
    }



    //** タブ **//
    // 普通
    public static DebugPanel FunctionsTab = new("Functions", DebugTabs.Always, true,
        () =>
        {
            currentTab = DebugTabs.FunctionsTab;
            ReShowPanels();
        });
    public static DebugPanel PlayerControlTab = new("PlayerControl", DebugTabs.Always, true,
        () =>
        {
            currentTab = DebugTabs.PlayersTab;
            ReShowPanels();
        });

    // 役職
    public static DebugPanel Crewmate = new(CustomOptionHolder.Cs(Palette.CrewmateBlue, "Crewmate"), DebugTabs.Always, true,
        () =>
        {
            currentTab = DebugTabs.SetRole_Crewmate;
            ReShowPanels();
        }, TabType.Role);
    public static DebugPanel Impostor = new(CustomOptionHolder.Cs(Palette.ImpostorRed, "Impostor"), DebugTabs.Always, true,
        () =>
        {
            currentTab = DebugTabs.SetRole_Impostor;
            ReShowPanels();
        }, TabType.Role);
    public static DebugPanel Neutral = new(CustomOptionHolder.Cs(Palette.DisabledGrey, "Neutral"), DebugTabs.Always, true,
        () =>
        {
            currentTab = DebugTabs.SetRole_Neutral;
            ReShowPanels();
        }, TabType.Role);

    //** ファンクション **//
    // 役職全表示
    public static DebugPanel ShowEveryoneRole = new("ShowRole", DebugTabs.FunctionsTab, false,
        () =>
        {
            canSeeRole = true;
            Minigame.Instance.Close();
        });

    // 役職全非表示
    public static DebugPanel DeShowEveryoneRole = new("DeShowRole", DebugTabs.FunctionsTab, false,
        () =>
        {
            canSeeRole = false;
            Minigame.Instance.Close();
        });

    //** プレイヤー操作 **//
    // テレポート
    public static DebugPanel Teleport = new("Teleport", DebugTabs.PlayerControlTab, false,
        () =>
        {
            if (!RoleClass.Debugger.AmDebugger) return;
            var source = CachedPlayer.LocalPlayer;
            var target = Debugger.target;
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RPCTeleport);
            writer.Write(source.PlayerId);
            writer.Write(target.PlayerId);
            writer.EndRPC();
            RPCProcedure.RPCTeleport(source.PlayerId, target.PlayerId);
            Minigame.Instance.Close();
        });

    // 死体通報
    public static DebugPanel ReportDeadbody = new("ReportDeadbody", DebugTabs.PlayerControlTab, false,
        () =>
        {
            if (!RoleClass.Debugger.AmDebugger) return;

            var source = CachedPlayer.LocalPlayer;
            var target = Debugger.target;
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ReportDeadBody);
            writer.Write(source.PlayerId);
            writer.Write(target.PlayerId);
            writer.EndRPC();
            RPCProcedure.ReportDeadBody(source.PlayerId, target.PlayerId);
            Minigame.Instance.Close();
        });

    // 追放
    public static DebugPanel Exile = new("Exile", DebugTabs.PlayerControlTab, false,
        () =>
        {
            if (!RoleClass.Debugger.AmDebugger) return;

            var target = Debugger.target;
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ExiledRPC);
            writer.Write(target.PlayerId);
            writer.EndRPC();
            RPCProcedure.ExiledRPC(target.PlayerId);
            Minigame.Instance.Close();
        });

    // 殺害
    public static DebugPanel Kill = new("Kill", DebugTabs.PlayerControlTab, false,
        () =>
        {
            if (!RoleClass.Debugger.AmDebugger) return;

            var target = Debugger.target;
            target.RpcMurderPlayer(target);
            Minigame.Instance.Close();
        });

    // 蘇生
    public static DebugPanel Revive = new("Revive", DebugTabs.PlayerControlTab, false,
        () =>
        {
            if (!RoleClass.Debugger.AmDebugger) return;

            var target = Debugger.target;
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ReviveRPC);
            writer.Write(target.PlayerId);
            writer.EndRPC();
            RPCProcedure.ReviveRPC(target.PlayerId);
            Minigame.Instance.Close();
        });

    // 死体掃除
    public static DebugPanel CleanDeadbody = new("CleanDeadbody", DebugTabs.PlayerControlTab, false,
        () =>
        {
            if (!RoleClass.Debugger.AmDebugger) return;

            var target = Debugger.target;
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CleanBody);
            writer.Write(target.PlayerId);
            writer.EndRPC();
            RPCProcedure.CleanBody(target.PlayerId);
            Minigame.Instance.Close();
        });

    // 会議起こし
    public static DebugPanel StartMeeting = new("StartMeeting", DebugTabs.PlayerControlTab, false,
        () =>
        {
            var source = target;
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UncheckedMeeting);
            writer.Write(source.PlayerId);
            writer.EndRPC();
            RPCProcedure.UncheckedMeeting(source.PlayerId);
        });

    // 役職付与
    public static DebugPanel SetRole = new("SetRole", DebugTabs.PlayerControlTab, false,
        () =>
        {
            currentTab = DebugTabs.SetRole_Crewmate;
            ReShowPanels();
        });

    public static void ReShowPanels()
    {
        foreach (ShapeshifterPanel panel in GameObject.FindObjectsOfType<ShapeshifterPanel>()) GameObject.Destroy(panel.gameObject);
        CreateTab();
        CreateAllPanels();
        CreatePlayerPanel();
        CreateRoleTab();
        CreateRolePanel();
    }

    public static void CreateTab()
    {
        int index = 0;
        foreach (var data in DebugPanel.panels)
        {
            if (!data.isTab || data.tabType != TabType.Normal) continue;

            int num = index % 3;

            ShapeshifterPanel panel = GameObject.Instantiate(minigame.PanelPrefab, minigame.transform);
            panel.transform.localScale *= 0.5f;
            panel.transform.localPosition = new Vector3(minigame.XStart + (float)num * 1.45f, minigame.YStart + 0.8f, -1f);
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
            // panel.transform.FindChild("Nameplate").GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
            panel.NameText.text = ModTranslation.GetString(data.text);
            panel.NameText.transform.localPosition = new(0, 0, -0.1f);
            index++;
        }
    }

    public static void CreateAllPanels()
    {
        int index = 0;
        foreach (var data in DebugPanel.panels)
        {
            if ((data.tab != currentTab && data.tab != DebugTabs.Always) || data.isTab) continue;

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
            // panel.transform.FindChild("Nameplate").GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
            panel.NameText.text = ModTranslation.GetString(data.text);
            panel.NameText.transform.localPosition = new(0, 0, -0.1f);
            index++;
        }
    }

    public static void CreatePlayerPanel()
    {
        if (currentTab != DebugTabs.PlayersTab) return;

        int index = 0;
        foreach (PlayerControl p in CachedPlayer.AllPlayers)
        {
            int num = index % 3;
            int num2 = index / 3;

            ShapeshifterPanel panel = GameObject.Instantiate(minigame.PanelPrefab, minigame.transform);
            panel.transform.localPosition = new Vector3(minigame.XStart + (float)num * minigame.XOffset, minigame.YStart + (float)num2 * minigame.YOffset, -1f);
            panel.SetPlayer(index, p.Data, (Action)(() =>
            {
                if (MeetingHud.Instance != null) MeetingHud.Instance.transform.FindChild("ButtonStuff").gameObject.SetActive(true);

                currentTab = DebugTabs.PlayerControlTab;
                ReShowPanels();
                target = p;
            }));
            panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
            index++;
        }
    }

    public static void CreateRoleTab()
    {
        if (currentTab is not (DebugTabs.SetRole_Crewmate or DebugTabs.SetRole_Impostor or DebugTabs.SetRole_Neutral)) return;

        int index = 0;
        foreach (var data in DebugPanel.panels)
        {
            if (!data.isTab || data.tabType != TabType.Role) continue;

            int num = index % 3;

            ShapeshifterPanel panel = GameObject.Instantiate(minigame.PanelPrefab, minigame.transform);
            panel.transform.localScale *= 0.4f;
            panel.transform.localPosition = new Vector3(minigame.XStart + (float)num * 1.5f, minigame.YStart + 0.45f, -1f);
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
            // panel.transform.FindChild("Nameplate").GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
            panel.NameText.text = ModTranslation.GetString(data.text);
            panel.NameText.transform.localPosition = new(0, 0, -0.1f);
            index++;
        }
    }

    public static void CreateRolePanel()
    {
        if (currentTab is not (DebugTabs.SetRole_Crewmate or DebugTabs.SetRole_Impostor or DebugTabs.SetRole_Neutral)) return;

        int index = 0;
        foreach (IntroData roleInfo in IntroData.Intros.Values)
        {
            if ((roleInfo.Team == TeamRoleType.Crewmate && currentTab != DebugTabs.SetRole_Crewmate)
                || (roleInfo.Team == TeamRoleType.Impostor && currentTab != DebugTabs.SetRole_Impostor)
                || (roleInfo.Team == TeamRoleType.Neutral && currentTab != DebugTabs.SetRole_Neutral)) continue;

            int num = index % 8;        //横方向のリミット
            int num2 = index / 8;       //縦方向のリミット

            ShapeshifterPanel panel = GameObject.Instantiate(minigame.PanelPrefab, minigame.transform);
            panel.transform.localScale *= 0.3f;
            panel.transform.localPosition = new Vector3(minigame.XStart - 0.7f + (float)num * 1f, minigame.YStart + (float)num2 * -0.3f, -1f);
            panel.SetPlayer(index, CachedPlayer.LocalPlayer.Data, (Action)(() =>
            {
                if (MeetingHud.Instance != null) MeetingHud.Instance.transform.FindChild("ButtonStuff").gameObject.SetActive(true);
                if (roleInfo.Team == TeamRoleType.Impostor) DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Impostor);
                else DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);

                target.SetRoleRPC(roleInfo.RoleId);
                Minigame.Instance.Close();
            }));
            panel.PlayerIcon.gameObject.SetActive(false);
            panel.LevelNumberText.transform.parent.gameObject.SetActive(false);
            panel.ColorBlindName.gameObject.SetActive(false);
            // panel.transform.FindChild("Nameplate").GetComponent<SpriteRenderer>().sprite = FastDestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
            panel.transform.FindChild("Nameplate/Highlight/ShapeshifterIcon").gameObject.SetActive(false);
            panel.NameText.text = CustomOptionHolder.Cs(roleInfo.color, ModTranslation.GetString(roleInfo.Name));
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
    public bool isTab;
    public Action onClick;
    public TabType tabType = TabType.Normal;
    public DebugPanel(string text, DebugTabs tab, bool isTab, Action onClick, TabType tabType = TabType.Normal)
    {
        this.text = text;
        this.tab = tab;
        this.isTab = isTab;
        this.onClick = onClick;
        this.tabType = tabType;
        panels.Add(this);
    }
}