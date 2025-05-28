using System;
using System.Linq;
using System.Text;
using UnityEngine;
using TMPro;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Events;
using HarmonyLib;

namespace SuperNewRoles.Roles.Ability;

public class BlackHatHackerTaskPanel : AbilityBase
{
    private BlackHatHackerAbility _parentAbility;

    public static bool TaskTab = true;
    public static int Page = 0;

    public BlackHatHackerTaskPanel(BlackHatHackerAbility parentAbility)
    {
        _parentAbility = parentAbility;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        TaskTab = true;
        Page = 0;
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        TaskTab = true;
        Page = 0;
    }
}

[HarmonyPatch(typeof(HudManager))]
public static class BlackHatHackerHudManagerPatch
{
    public static string TaskText = "Tasks";
    private static GameObject ChangeTab;
    private static GameObject PageTab;
    private static TextMeshPro ChangeTabTextTMP;
    private static TextMeshPro PageTabTextTMP;
    private static TextMeshPro TabTextTMP;

    [HarmonyPatch(nameof(HudManager.Start)), HarmonyPostfix]
    public static void StartPostfix(HudManager __instance)
    {
        // 感染状況切り替えタブ
        ChangeTab = UnityEngine.Object.Instantiate(__instance.TaskPanel.tab.gameObject, __instance.TaskPanel.transform);
        ChangeTab.name = "ChangeTab";
        ChangeTab.SetActive(false);
        Vector3 pos = ChangeTab.transform.localPosition;
        pos.y -= 1.23f;
        ChangeTab.transform.localPosition = pos;

        ChangeTabTextTMP = ChangeTab.transform.Find("TabText_TMP").GetComponent<TextMeshPro>();
        ChangeTabTextTMP.text = ModTranslation.GetString("BlackHatHackerStatusOfInfection");

        ChangeTab.GetComponent<PassiveButton>().OnClick = new();
        ChangeTab.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() =>
        {
            BlackHatHackerTaskPanel.TaskTab = !BlackHatHackerTaskPanel.TaskTab;
            if (!__instance.TaskPanel.open) __instance.TaskPanel.open = true;
        }));

        // ページ切り替えタブ
        PageTab = UnityEngine.Object.Instantiate(__instance.TaskPanel.tab.gameObject, __instance.TaskPanel.transform);
        PageTab.name = "PageTab";
        PageTab.SetActive(false);
        pos = PageTab.transform.localPosition;
        pos.y -= 2.502f;
        PageTab.transform.localPosition = pos;

        PageTabTextTMP = PageTab.transform.Find("TabText_TMP").GetComponent<TextMeshPro>();
        PageTabTextTMP.text = ModTranslation.GetString("BlackHatHackerPageChange");

        PageTab.GetComponent<PassiveButton>().OnClick = new();
        PageTab.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() =>
        {
            // 安全チェック
            if (BlackHatHacker.Instance?.Role == null) return;

            var ability = BlackHatHackerAbility.LocalInstance;
            if (ability != null)
            {
                BlackHatHackerTaskPanel.Page++;
                // 全BlackHatHackerの感染データの数でページング
                var allPlayers = PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => p != null && ((ExPlayerControl)p).Role == BlackHatHacker.Instance.Role).ToList();
                if (BlackHatHackerTaskPanel.Page >= allPlayers.Count)
                    BlackHatHackerTaskPanel.Page = 0;
            }
            if (!__instance.TaskPanel.open) __instance.TaskPanel.open = true;
        }));

        TabTextTMP = __instance.TaskPanel.tab.transform.Find("TabText_TMP").GetComponent<TextMeshPro>();
    }

    [HarmonyPatch(nameof(HudManager.Update)), HarmonyPostfix]
    public static void UpdatePostfix(HudManager __instance)
    {
        if (ChangeTab == null) return;

        // ゲーム開始前や特定の状況での安全チェック
        if (PlayerControl.LocalPlayer == null || ExPlayerControl.LocalPlayer == null) return;
        if (BlackHatHacker.Instance?.Role == null) return;
        if (PlayerControl.LocalPlayer.Data == null) return;
        if (!RoleOptionManager.TryGetRoleOption(BlackHatHacker.Instance.Role, out var option) || option.Percentage == 0 || option.NumberOfCrews == 0) return;

        bool isBlackHatHacker = ExPlayerControl.LocalPlayer.Role == BlackHatHacker.Instance.Role;
        bool isDead = PlayerControl.LocalPlayer.Data.IsDead;

        // タブの表示制御
        ChangeTab.SetActive(isBlackHatHacker || isDead);
        if (ModHelpers.Not(isBlackHatHacker || isDead))
            return;
        Vector3 pos = ChangeTab.transform.localPosition;
        pos.x = __instance.TaskPanel.tab.transform.localPosition.x;
        ChangeTab.transform.localPosition = pos;
        ChangeTabTextTMP.text = BlackHatHackerTaskPanel.TaskTab ?
            ModTranslation.GetString("BlackHatHackerStatusOfInfection") : TaskText;

        TabTextTMP.text = !BlackHatHackerTaskPanel.TaskTab && isBlackHatHacker ?
            ModTranslation.GetString("BlackHatHackerStatusOfInfection") : TaskText;

        // ページタブ（死後のみ）
        PageTab.SetActive(!BlackHatHackerTaskPanel.TaskTab && isDead);
        pos = PageTab.transform.localPosition;
        pos.x = __instance.TaskPanel.tab.transform.localPosition.x;
        PageTab.transform.localPosition = pos;
        PageTabTextTMP.text = ModTranslation.GetString("BlackHatHackerPageChange");

        // タスクタブが選択されている場合は通常表示
        if (BlackHatHackerTaskPanel.TaskTab) return;

        // 感染状況の表示
        UpdateInfectionStatus(__instance);
    }

    private static void UpdateInfectionStatus(HudManager __instance)
    {
        // 安全チェック
        if (PlayerControl.LocalPlayer == null || ExPlayerControl.LocalPlayer == null) return;
        if (BlackHatHacker.Instance?.Role == null) return;
        if (PlayerControl.LocalPlayer.Data == null) return;

        StringBuilder text = new();
        if (!PlayerControl.LocalPlayer.Data.IsDead)
        {
            var ability = BlackHatHackerAbility.LocalInstance;
            if (ability == null || ExPlayerControl.LocalPlayer.Role != BlackHatHacker.Instance.Role)
            {
                BlackHatHackerTaskPanel.TaskTab = true;
                return;
            }

            // 生存中：自分の感染状況表示
            foreach (var data in ability.InfectionTimer)
            {
                double percentage = Math.Floor(data.Value / ability.Data.HackInfectiousTime * 1000) / 1000;
                var playerInfo = GameData.Instance.GetPlayerById(data.Key);
                string statusText = ability.DeadPlayers.Contains(data.Key) ?
                    ModTranslation.GetString("FinalStatusDead") :
                    (percentage < 1 ? percentage.ToString("P1") : ModTranslation.GetString("BlackHatHackerInfected"));

                text.AppendLine($"{playerInfo?.PlayerName}<pos=125%>{statusText}");
            }
        }
        else
        {
            // 死後：全BlackHatHackerの感染状況表示（ページング）
            var allBlackHatHackers = PlayerControl.AllPlayerControls.ToArray()
                .Where(p => p != null && ((ExPlayerControl)p).Role == BlackHatHacker.Instance.Role).ToList();

            if (allBlackHatHackers.Count == 0) return;

            if (BlackHatHackerTaskPanel.Page >= allBlackHatHackers.Count)
                BlackHatHackerTaskPanel.Page = 0;

            var currentPlayer = allBlackHatHackers[BlackHatHackerTaskPanel.Page];
            if (currentPlayer == null || currentPlayer.Data == null) return;

            var ability = ((ExPlayerControl)currentPlayer).GetAbility<BlackHatHackerAbility>();

            text.AppendLine($"{currentPlayer.Data.PlayerName} ({BlackHatHackerTaskPanel.Page + 1} / {allBlackHatHackers.Count})");

            if (ability != null)
            {
                foreach (var data in ability.InfectionTimer)
                {
                    double percentage = Math.Floor(data.Value / ability.Data.HackInfectiousTime * 1000) / 1000;
                    var playerInfo = GameData.Instance.GetPlayerById(data.Key);
                    string statusText = playerInfo?.IsDead == true ?
                        ModTranslation.GetString("FinalStatusDead") :
                        (percentage < 1 ? percentage.ToString("P1") : ModTranslation.GetString("BlackHatHackerInfected"));

                    text.AppendLine($"{playerInfo?.PlayerName}<pos=125%>{statusText}");
                }
            }
        }

        __instance.TaskPanel.SetTaskText(text.ToString());
    }
}