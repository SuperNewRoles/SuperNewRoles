using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Neutral;

public class BlackHatHacker
{
    private const int OptionId = 303100;
    public static CustomRoleOption BlackHatHackerOption;
    public static CustomOption BlackHatHackerPlayerCount;
    public static CustomOption BlackHatHackerHackCoolTime;
    public static CustomOption BlackHatHackerHackCountable;
    public static CustomOption BlackHatHackerIsNotInfectionIncrease;
    public static CustomOption BlackHatHackerHackInfectiousTime;
    public static CustomOption BlackHatHackerNotInfectiousTime;
    public static CustomOption BlackHatHackerStartSelfPropagation;
    public static CustomOption BlackHatHackerInfectionScope;
    public static CustomOption BlackHatHackerCanInfectedAdmin;
    public static CustomOption BlackHatHackerCanMoveWhenUsesAdmin;
    public static CustomOption BlackHatHackerIsAdminColor;
    public static CustomOption BlackHatHackerCanInfectedVitals;
    public static CustomOption BlackHatHackerCanDeadWin;

    public static void SetupCustomOptions()
    {
        BlackHatHackerOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.BlackHatHacker);
        BlackHatHackerPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], BlackHatHackerOption);
        BlackHatHackerHackCoolTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "BlackHatHackerHackCoolTimeOption", 15f, 0f, 60f, 2.5f, BlackHatHackerOption);
        BlackHatHackerHackCountable = CustomOption.Create(OptionId + 3, false, CustomOptionType.Neutral, "BlackHatHackerHackCountableOption", 1f, 1f, 15f, 1f, BlackHatHackerOption);
        BlackHatHackerIsNotInfectionIncrease = CustomOption.Create(OptionId + 4, false, CustomOptionType.Neutral, "BlackHatHackerIsNotInfectionIncreaseOption", true, BlackHatHackerOption);
        BlackHatHackerHackInfectiousTime = CustomOption.Create(OptionId + 5, false, CustomOptionType.Neutral, "BlackHatHackerHackInfectiousTimeOption", 300f, 25f, 600f, 25f, BlackHatHackerOption);
        BlackHatHackerNotInfectiousTime = CustomOption.Create(OptionId + 6, false, CustomOptionType.Neutral, "BlackHatHackerNotInfectiousTimeOption", 12.5f, 0f, 30f, 2.5f, BlackHatHackerOption);
        BlackHatHackerStartSelfPropagation = CustomOption.Create(OptionId + 7, false, CustomOptionType.Neutral, "BlackHatHackerStartSelfPropagationOption", Safecracker.CustomRates[..(Safecracker.CustomRates.Length - 1)], BlackHatHackerOption);
        BlackHatHackerInfectionScope = CustomOption.Create(OptionId + 8, false, CustomOptionType.Neutral, "BlackHatHackerInfectionScopeOption", new string[] { "Short", "Medium", "Long" }, BlackHatHackerOption);
        BlackHatHackerCanInfectedAdmin = CustomOption.Create(OptionId + 9, false, CustomOptionType.Neutral, "BlackHatHackerCanInfectedAdminsOption", false, BlackHatHackerOption);
        BlackHatHackerCanMoveWhenUsesAdmin = CustomOption.Create(OptionId + 10, false, CustomOptionType.Neutral, "CanMoveWhenUsesAdmin", false, BlackHatHackerCanInfectedAdmin);
        BlackHatHackerIsAdminColor = CustomOption.Create(OptionId + 11, false, CustomOptionType.Neutral, "BlackHatHackerIsAdminColorOption", false, BlackHatHackerCanInfectedAdmin);
        BlackHatHackerCanInfectedVitals = CustomOption.Create(OptionId + 12, false, CustomOptionType.Neutral, "BlackHatHackerCanInfectedVitalOption", false, BlackHatHackerOption);
        BlackHatHackerCanDeadWin = CustomOption.Create(OptionId + 13, false, CustomOptionType.Neutral, "BlackHatHackerCanDeadWinOption", true, BlackHatHackerOption);
    }

    public static List<PlayerControl> BlackHatHackerPlayer;
    public static Color32 color = new(29, 255, 166, byte.MaxValue);
    public static int HackCount;
    public static Dictionary<byte, Dictionary<byte, float>> InfectionTimer;
    public static float NotInfectiousTimer;
    public static float SharedTimer;
    public static bool IsMyAdmin;
    public static bool IsMyVutals;
    public static bool TaskTab;
    public static int Page;
    public static List<byte> DeadPlayers;
    public static List<byte> _SelfPropagationPlayerId;
    public static List<byte> SelfPropagationPlayerId
    {
        get
        {
            if (!InfectionTimer.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return new();
            List<byte> add = new Dictionary<byte, float>(InfectionTimer[PlayerControl.LocalPlayer.PlayerId]).
                             Where(x => !_SelfPropagationPlayerId.Contains(x.Key) && IsSelfPropagation(x.Value)).ToDictionary(x => x.Key, y => y.Value).Keys.ToList();
            _SelfPropagationPlayerId.AddRange(add);
            return _SelfPropagationPlayerId;
        }
    }
    public static List<byte> _InfectedPlayerId;
    public static List<byte> InfectedPlayerId
    {
        get
        {
            if (!InfectionTimer.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return new();
            List<byte> check = new Dictionary<byte, float>(InfectionTimer[PlayerControl.LocalPlayer.PlayerId]).
                               Where(x => !_InfectedPlayerId.Contains(x.Key) && x.Value >= BlackHatHackerHackInfectiousTime.GetFloat()).ToDictionary(x => x.Key, y => y.Value).Keys.ToList();
            _InfectedPlayerId.AddRange(check);
            return _InfectedPlayerId;
        }
    }
    public static void ClearAndReload()
    {
        BlackHatHackerPlayer = new();
        HackCount = BlackHatHackerHackCountable.GetInt();
        InfectionTimer = new();
        NotInfectiousTimer = BlackHatHackerNotInfectiousTime.GetFloat();
        SharedTimer = 1f;
        IsMyAdmin = false;
        IsMyVutals = false;
        TaskTab = true;
        Page = 0;
        DeadPlayers = new();
        _SelfPropagationPlayerId = new();
        _InfectedPlayerId = new();
    }

    public static bool IsSelfPropagation(float timer)
    {
        if (BlackHatHackerStartSelfPropagation.GetSelection() == 0) return false;
        return timer >= BlackHatHackerHackInfectiousTime.GetFloat() * (int.Parse(BlackHatHackerStartSelfPropagation.GetString().Replace("%", "")) / 100f);
    }

    public static bool IsAllInfected(byte id)
    {
        if (!InfectionTimer.ContainsKey(id)) return false;
        return InfectionTimer[id].All(x => ModHelpers.PlayerById(x.Key).IsDead() || ModHelpers.PlayerById(x.Key).AmOwner || x.Value >= BlackHatHackerHackInfectiousTime.GetFloat());
    }

    public static CustomButton BlackHatHackerHackButtoon;
    public static TMP_Text BlackHatHackerHackNumText;
    public static CustomButton BlackHatHackerAdminButtoon;
    public static CustomButton BlackHatHackerVitalsButtoon;
    public static void SetupCustomButtons(HudManager __instance)
    {
        BlackHatHackerHackButtoon = new(
            () =>
            {
                if (HackCount <= 0) return;
                PlayerControl target = HudManagerStartPatch.SetTarget();
                if (!target) return;
                if (!InfectionTimer.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) InfectionTimer[PlayerControl.LocalPlayer.PlayerId] = new();
                InfectionTimer[PlayerControl.LocalPlayer.PlayerId][target.PlayerId] = BlackHatHackerHackInfectiousTime.GetFloat();
                HackCount--;
                BlackHatHackerHackButtoon.Timer = BlackHatHackerHackButtoon.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.BlackHatHacker; },
            () =>
            {
                BlackHatHackerHackNumText.text = string.Format(ModTranslation.GetString("BlackHatHackerHackNumText"), HackCount);
                if (HackCount <= 0) return false;
                PlayerControl target = HudManagerStartPatch.SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(target, color);
                if (!target) return false;
                if (!InfectionTimer.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return true;
                if (!InfectionTimer[PlayerControl.LocalPlayer.PlayerId].ContainsKey(target.PlayerId)) return true;
                if (InfectionTimer[PlayerControl.LocalPlayer.PlayerId][target.PlayerId] < BlackHatHackerHackInfectiousTime.GetFloat()) return true;
                return false;
            },
            () =>
            {
                BlackHatHackerHackButtoon.MaxTimer = BlackHatHackerHackCoolTime.GetFloat();
                BlackHatHackerHackButtoon.Timer = BlackHatHackerHackButtoon.MaxTimer;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CrackerButton.png", 115f),
            new(0, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("BlackHatHackerHackButtonName"),
            showButtonText = true
        };
        BlackHatHackerHackNumText = Object.Instantiate(BlackHatHackerHackButtoon.actionButton.cooldownTimerText, BlackHatHackerHackButtoon.actionButton.cooldownTimerText.transform.parent);
        BlackHatHackerHackNumText.text = string.Format(ModTranslation.GetString("BlackHatHackerHackNumText"), BlackHatHackerHackCountable.GetInt());
        BlackHatHackerHackNumText.enableWordWrapping = false;
        BlackHatHackerHackNumText.transform.localScale = Vector3.one * 0.5f;
        BlackHatHackerHackNumText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        BlackHatHackerAdminButtoon = new(
            () =>
            {
                __instance.ToggleMapVisible(new()
                {
                    Mode = MapOptions.Modes.CountOverlay,
                    AllowMovementWhileMapOpen = BlackHatHackerCanMoveWhenUsesAdmin.GetBool(),
                });
                PlayerControl.LocalPlayer.NetTransform.Halt();
                IsMyAdmin = true;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.BlackHatHacker && BlackHatHackerCanInfectedAdmin.GetBool(); },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                BlackHatHackerAdminButtoon.MaxTimer = 0f;
                BlackHatHackerAdminButtoon.Timer = 0f;
                IsMyAdmin = false;
            },
            RoleClass.EvilHacker.GetButtonSprite(),
            new(-1, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ADMINButton"),
            showButtonText = true
        };

        BlackHatHackerVitalsButtoon = new(
            () =>
            {
                RoleTypes role = PlayerControl.LocalPlayer.Data.Role.Role;
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Scientist);
                CachedPlayer.LocalPlayer.Data.Role.TryCast<ScientistRole>().UseAbility();
                FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, role);
                IsMyVutals = true;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.BlackHatHacker && BlackHatHackerCanInfectedVitals.GetBool(); },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                BlackHatHackerVitalsButtoon.MaxTimer = 0f;
                BlackHatHackerVitalsButtoon.Timer = 0f;
                IsMyVutals = false;
            },
            RoleClass.Doctor.GetVitalsSprite(),
            new(-2, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.V,
            50,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("DoctorVitalName"),
            showButtonText = true
        };
    }

    public static void FixedUpdate()
    {
        if (!InfectionTimer.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return;
        SharedTimer -= Time.fixedDeltaTime;
        if (SharedTimer <= 0)
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetInfectionTimer);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write(InfectionTimer[PlayerControl.LocalPlayer.PlayerId].Count);
            foreach (KeyValuePair<byte, float> timer in InfectionTimer[PlayerControl.LocalPlayer.PlayerId])
            {
                writer.Write(timer.Key);
                writer.Write(timer.Value);
            }
            writer.EndRPC();
            SharedTimer = 1;
        }
        if (!PlayerControl.LocalPlayer.IsRole(RoleId.BlackHatHacker)) return;
        if (IsAllInfected(PlayerControl.LocalPlayer.PlayerId) && (BlackHatHackerCanDeadWin.GetBool() || PlayerControl.LocalPlayer.IsAlive()) && !ModHelpers.IsDebugMode())
        {
            MessageWriter writer1 = RPCHelper.StartRPC(CustomRPC.ShareWinner);
            writer1.Write(PlayerControl.LocalPlayer.PlayerId);
            writer1.EndRPC();
            RPCProcedure.ShareWinner(PlayerControl.LocalPlayer.PlayerId);
            if (AmongUsClient.Instance.AmHost) CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.BlackHatHackerWin, false);
            else
            {
                MessageWriter writer2 = RPCHelper.StartRPC(CustomRPC.CustomEndGame);
                writer2.Write((byte)CustomGameOverReason.BlackHatHackerWin);
                writer2.Write(false);
                writer2.EndRPC();
            }
        }
        if (PlayerControl.LocalPlayer.IsDead() && !BlackHatHackerCanDeadWin.GetBool()) return;
        NotInfectiousTimer -= Time.fixedDeltaTime;
        if (NotInfectiousTimer > 0) return;
        if (RoleClass.IsMeeting) return;
        foreach (byte id in SelfPropagationPlayerId) InfectionTimer[PlayerControl.LocalPlayer.PlayerId][id] += Time.fixedDeltaTime / 5;
        foreach (byte id in InfectedPlayerId)
        {
            PlayerControl player = ModHelpers.PlayerById(id);
            if (!player) continue;
            if (player.IsDead()) continue;
            float scope = GameOptionsData.KillDistances[Mathf.Clamp(BlackHatHackerInfectionScope.GetSelection(), 0, 2)];
            List<PlayerControl> infection = PlayerControl.AllPlayerControls.ToList().
                                            FindAll(x => !x.AmOwner && Vector3.Distance(player.transform.position, x.transform.position) <= scope);
            foreach (PlayerControl target in infection) InfectionTimer[PlayerControl.LocalPlayer.PlayerId][target.PlayerId] += Time.fixedDeltaTime;
        }
    }

    public static void WrapUp()
    {
        NotInfectiousTimer = BlackHatHackerNotInfectiousTime.GetFloat();
        DeadPlayers = PlayerControl.AllPlayerControls.ToList().FindAll(x => x.IsDead()).ConvertAll(x => x.PlayerId);
        if (!InfectionTimer.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return;
        if (HackCount <= 0 && InfectionTimer[PlayerControl.LocalPlayer.PlayerId].All(x => x.Value <= 0 || ModHelpers.PlayerById(x.Key).IsDead()) && BlackHatHackerIsNotInfectionIncrease.GetBool())
            HackCount = 1;
    }

    [HarmonyPatch(typeof(IntroCutscene))]
    public static class IntroCutscenePatch
    {
        [HarmonyPatch(nameof(IntroCutscene.OnDestroy)), HarmonyPostfix]
        public static void OnDestroyPostfix()
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.BlackHatHacker)) return;
            InfectionTimer[PlayerControl.LocalPlayer.PlayerId] = new();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.AmOwner || player.IsBot()) continue;
                InfectionTimer[PlayerControl.LocalPlayer.PlayerId][player.PlayerId] = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(HudManager))]
    public static class HudManagerPatch
    {
        public static string TaskText = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Tasks);
        [HarmonyPatch(nameof(HudManager.Start)), HarmonyPostfix]
        public static void StartPostfix(HudManager __instance)
        {
            GameObject ChangeTab = Object.Instantiate(__instance.TaskPanel.tab.gameObject, __instance.TaskPanel.transform);
            ChangeTab.name = "ChangeTab";
            ChangeTab.SetActive(false);
            Vector3 pos = ChangeTab.transform.localPosition;
            pos.y -= 1.23f;
            ChangeTab.transform.localPosition = pos;
            GameObject ChangeTabText = ChangeTab.transform.Find("TabText_TMP").gameObject;
            ChangeTabText.name = "ChangeTabText_TMP";
            ChangeTabText.GetComponent<TextMeshPro>().text = ModTranslation.GetString("BlackHatHackerStatusOfInfection");
            ChangeTab.GetComponent<PassiveButton>().OnClick = new();
            ChangeTab.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() =>
            {
                TaskTab = !TaskTab;
                if (!__instance.TaskPanel.open) __instance.TaskPanel.open = true;
            }));
            __instance.TaskPanel.tab.transform.Find("TabText_TMP").GetComponent<TextMeshPro>().text = TaskText;

            GameObject PageTab = Object.Instantiate(__instance.TaskPanel.tab.gameObject, __instance.TaskPanel.transform);
            PageTab.name = "PageTab";
            PageTab.SetActive(false);
            pos = PageTab.transform.localPosition;
            pos.y -= 2.502f;
            PageTab.transform.localPosition = pos;
            GameObject PageTabText = PageTab.transform.Find("TabText_TMP").gameObject;
            PageTabText.name = "PageTabText_TMP";
            PageTabText.GetComponent<TextMeshPro>().text = ModTranslation.GetString("BlackHatHackerPageChange");
            PageTab.GetComponent<PassiveButton>().OnClick = new();
            PageTab.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() =>
            {
                Page++;
                if (Page >= InfectionTimer.Count) Page = 0;
                if (!__instance.TaskPanel.open) __instance.TaskPanel.open = true;
            }));
        }

        [HarmonyPatch(nameof(HudManager.Update)), HarmonyPostfix]
        public static void UpdatePostfix(HudManager __instance)
        {
            GameObject ChangeTab = __instance.TaskPanel?.transform.Find("ChangeTab")?.gameObject;
            ChangeTab.SetActive(PlayerControl.LocalPlayer.IsRole(RoleId.BlackHatHacker) || (PlayerControl.LocalPlayer.IsDead() && BlackHatHackerPlayerCount.GetSelection() != 0));
            Vector3 pos = ChangeTab.transform.localPosition;
            pos.x = __instance.TaskPanel.tab.transform.localPosition.x;
            ChangeTab.transform.localPosition = pos;
            ChangeTab.transform.Find("ChangeTabText_TMP").GetComponent<TextMeshPro>().text = TaskTab ? ModTranslation.GetString("BlackHatHackerStatusOfInfection") : TaskText;
            __instance.TaskPanel.tab.transform.Find("TabText_TMP").GetComponent<TextMeshPro>().text = !TaskTab && PlayerControl.LocalPlayer.IsRole(RoleId.BlackHatHacker) ?
                                                                                                       ModTranslation.GetString("BlackHatHackerStatusOfInfection") : TaskText;

            GameObject PageTab = __instance.TaskPanel.transform.Find("PageTab").gameObject;
            PageTab.SetActive(!TaskTab && PlayerControl.LocalPlayer.IsDead());
            pos = PageTab.transform.localPosition;
            pos.x = __instance.TaskPanel.tab.transform.localPosition.x;
            PageTab.transform.localPosition = pos;
            PageTab.transform.Find("PageTabText_TMP").GetComponent<TextMeshPro>().text = ModTranslation.GetString("BlackHatHackerPageChange");


            if (TaskTab) return;
            if (PlayerControl.LocalPlayer.IsAlive())
            {
                if (!PlayerControl.LocalPlayer.IsRole(RoleId.BlackHatHacker))
                {
                    TaskTab = true;
                    return;
                }
                if (!InfectionTimer.ContainsKey(PlayerControl.LocalPlayer.PlayerId)) return;
                StringBuilder text = new();
                text.AppendLine(PlayerControl.LocalPlayer.transform.Find("RoleTask").GetComponent<ImportantTextTask>().Text);
                text.AppendLine();
                foreach (KeyValuePair<byte, float> data in InfectionTimer[PlayerControl.LocalPlayer.PlayerId])
                {
                    double percentage = Math.Floor(data.Value / BlackHatHackerHackInfectiousTime.GetFloat() * 1000) / 1000;
                    string sabtext = !DeadPlayers.Contains(data.Key) ? (percentage < 1 ? percentage.ToString("P1") : ModTranslation.GetString("BlackHatHackerInfected")) : ModTranslation.GetString("FinalStatusDead");
                    text.AppendLine($"{ModHelpers.PlayerById(data.Key).Data.PlayerName}<pos=125%>{sabtext}");
                }
                __instance.TaskPanel.SetTaskText(text.ToString());
            }
            else if (PlayerControl.LocalPlayer.IsDead())
            {
                if (Page >= InfectionTimer.Count) Page = 0;
                byte key = InfectionTimer.Keys.ToList()[Page];
                StringBuilder text = new();
                text.AppendLine(PlayerControl.LocalPlayer.transform.Find("RoleTask").GetComponent<ImportantTextTask>().Text);
                text.AppendLine();
                text.AppendLine($"{ModHelpers.PlayerById(key).Data.PlayerName} ({Page + 1} / {InfectionTimer.Count})");
                foreach (KeyValuePair<byte, float> data in InfectionTimer[key])
                {
                    double percentage = Math.Floor(data.Value / BlackHatHackerHackInfectiousTime.GetFloat() * 1000) / 1000;
                    string sabtext = !DeadPlayers.Contains(data.Key) ? (percentage < 1 ? percentage.ToString("P1") : ModTranslation.GetString("BlackHatHackerInfected")) : ModTranslation.GetString("FinalStatusDead");
                    text.AppendLine($"{ModHelpers.PlayerById(data.Key).Data.PlayerName}<pos=125%>{sabtext}");
                }
                __instance.TaskPanel.SetTaskText(text.ToString());
            }
        }
    }
}