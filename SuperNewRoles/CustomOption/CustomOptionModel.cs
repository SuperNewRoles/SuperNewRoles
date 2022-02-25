using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Reflection;
using System.Text;

namespace SuperNewRoles.CustomOption
{
    public class CustomOption
    {
        public static List<CustomOption> options = new List<CustomOption>();
        public static int preset = 0;

        public int id;
        public string name;
        public string format;
        public System.Object[] selections;

        public int defaultSelection;
        public ConfigEntry<int> entry;
        public int selection;
        public OptionBehaviour optionBehaviour;
        public CustomOption parent;
        public List<CustomOption> children;
        public bool isHeader;
        public bool isHidden;

        public virtual bool enabled
        {
            get
            {
                return this.getBool();
            }
        }

        // Option creation
        public CustomOption()
        {

        }

        public CustomOption(int id, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            this.id = id;
            this.name = name;
            this.format = format;
            this.selections = selections;
            int index = Array.IndexOf(selections, defaultValue);
            this.defaultSelection = index >= 0 ? index : 0;
            this.parent = parent;
            this.isHeader = isHeader;
            this.isHidden = isHidden;

            this.children = new List<CustomOption>();
            if (parent != null)
            {
                parent.children.Add(this);
            }

            selection = 0;
            if (id > 0)
            {
                entry = SuperNewRolesPlugin.Instance.Config.Bind($"Preset{preset}", id.ToString(), defaultSelection);
                selection = Mathf.Clamp(entry.Value, 0, selections.Length - 1);
            }
            options.Add(this);
        }

        public static CustomOption Create(int id, string name, string[] selections, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, name, selections, "", parent, isHeader, isHidden, format);
        }

        public static CustomOption Create(int id, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, name, selections.Cast<object>().ToArray(), defaultValue, parent, isHeader, isHidden, format);
        }

        public static CustomOption Create(int id, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, name, new string[] { "optionOff", "optionOn" }, defaultValue ? "optionOn" : "optionOff", parent, isHeader, isHidden, format);
        }

        // Static behaviour

        public static void switchPreset(int newPreset)
        {
            CustomOption.preset = newPreset;
            foreach (CustomOption option in CustomOption.options)
            {
                if (option.id <= 0) continue;

                option.entry = SuperNewRolesPlugin.Instance.Config.Bind($"Preset{preset}", option.id.ToString(), option.defaultSelection);
                option.selection = Mathf.Clamp(option.entry.Value, 0, option.selections.Length - 1);
                if (option.optionBehaviour != null && option.optionBehaviour is StringOption stringOption)
                {
                    stringOption.oldValue = stringOption.Value = option.selection;
                    stringOption.ValueText.text = option.getString();
                }
            }
        }

        public static void ShareOptionSelections()
        {
            if (PlayerControl.AllPlayerControls.Count <= 1 || AmongUsClient.Instance?.AmHost == false && PlayerControl.LocalPlayer == null) return;

            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareOptions, Hazel.SendOption.Reliable);
            messageWriter.WritePacked((uint)CustomOption.options.Count);
            foreach (CustomOption option in CustomOption.options)
            {
                messageWriter.WritePacked((uint)option.id);
                messageWriter.WritePacked((uint)Convert.ToUInt32(option.selection));
            }
            messageWriter.EndMessage();
        }

        // Getter

        public virtual int getSelection()
        {
            return selection;
        }

        public virtual bool getBool()
        {
            return selection > 0;
        }

        public virtual float getFloat()
        {
            return (float)selections[selection];
        }

        public virtual string getString()
        {
            string sel = selections[selection].ToString();
            if (format != "")
            {
                return sel;
            }
            return ModTranslation.getString(sel);
        }

        public virtual string getName()
        {
            return name;
        }

        // Option changes

        public virtual void updateSelection(int newSelection)
        {
            selection = Mathf.Clamp((newSelection + selections.Length) % selections.Length, 0, selections.Length - 1);
            if (optionBehaviour != null && optionBehaviour is StringOption stringOption)
            {
                stringOption.oldValue = stringOption.Value = selection;
                stringOption.ValueText.text = getString();

                if (AmongUsClient.Instance?.AmHost == true && PlayerControl.LocalPlayer)
                {
                    if (id == 0) switchPreset(selection); // Switch presets
                    else if (entry != null) entry.Value = selection; // Save selection to config

                    ShareOptionSelections();// Share all selections
                }
            }
        }
    }

    public class CustomRoleOption : CustomOption
    {
        public CustomOption countOption = null;

        public int rate
        {
            get
            {
                return getSelection();
            }
        }

        public int count
        {
            get
            {
                if (countOption != null)
                    return Mathf.RoundToInt(countOption.getFloat());

                return 1;
            }
        }

        public (int, int) data
        {
            get
            {
                return (rate, count);
            }
        }

        public CustomRoleOption(int id, string name, Color color, int max = 15) :
            base(id, CustomOptions.cs(color, name), CustomOptions.rates, "", null, true, false, "")
        {
            if (max > 1)
                countOption = CustomOption.Create(id + 10000, "roleNumAssigned", 1f, 1f, 15f, 1f, this, format: "unitPlayers");
        }
    }

    public class CustomOptionBlank : CustomOption
    {
        public CustomOptionBlank(CustomOption parent)
        {
            this.parent = parent;
            this.id = -1;
            this.name = "";
            this.isHeader = false;
            this.isHidden = true;
            this.children = new List<CustomOption>();
            this.selections = new string[] { "" };
            options.Add(this);
        }

        public override int getSelection()
        {
            return 0;
        }

        public override bool getBool()
        {
            return true;
        }

        public override float getFloat()
        {
            return 0f;
        }

        public override string getString()
        {
            return "";
        }

        public override void updateSelection(int newSelection)
        {
            return;
        }

    }

    [HarmonyPatch(typeof(RoleOptionsData), nameof(RoleOptionsData.GetNumPerGame))]
    class RoleOptionsDataGetNumPerGamePatch
    {
        public static void Postfix(ref int __result, ref RoleTypes role)
        {
            if (role == RoleTypes.Crewmate || role == RoleTypes.Impostor) return;

            __result = 0;
        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (GameObject.Find("TORSettings") != null)
            { // Settings setup has already been performed, fixing the title of the tab and returning
                GameObject.Find("TORSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("SettingSuperNewRolesSetting"));
                return;
            }

            // Setup TOR tab
            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;
            var gameSettings = GameObject.Find("Game Settings");
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            var torSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var torMenu = torSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            torSettings.name = "TORSettings";

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var torTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var torTabHighlight = torTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            torTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.CrewMateSetting.png", 100f);

            gameTab.transform.position += Vector3.left * 0.5f;
            torTab.transform.position += Vector3.right * 0.5f;
            roleTab.transform.position += Vector3.left * 0.5f;

            var tabs = new GameObject[] { gameTab, roleTab, torTab };
            for (int i = 0; i < tabs.Length; i++)
            {
                var button = tabs[i].GetComponentInChildren<PassiveButton>();
                if (button == null) continue;
                int copiedIndex = i;
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => {
                    gameSettingMenu.RegularGameSettings.SetActive(false);
                    gameSettingMenu.RolesSettings.gameObject.SetActive(false);
                    torSettings.gameObject.SetActive(false);
                    gameSettingMenu.GameSettingsHightlight.enabled = false;
                    gameSettingMenu.RolesSettingsHightlight.enabled = false;
                    torTabHighlight.enabled = false;
                    if (copiedIndex == 0)
                    {
                        gameSettingMenu.RegularGameSettings.SetActive(true);
                        gameSettingMenu.GameSettingsHightlight.enabled = true;
                    }
                    else if (copiedIndex == 1)
                    {
                        gameSettingMenu.RolesSettings.gameObject.SetActive(true);
                        gameSettingMenu.RolesSettingsHightlight.enabled = true;
                    }
                    else if (copiedIndex == 2)
                    {
                        torSettings.gameObject.SetActive(true);
                        torTabHighlight.enabled = true;
                    }
                }));
            }

            foreach (OptionBehaviour option in torMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> torOptions = new List<OptionBehaviour>();

            for (int i = 0; i < CustomOption.options.Count; i++)
            {
                CustomOption option = CustomOption.options[i];
                if (option.optionBehaviour == null)
                {
                    StringOption stringOption = UnityEngine.Object.Instantiate(template, torMenu.transform);
                    torOptions.Add(stringOption);
                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                    stringOption.TitleText.text = option.name;
                    stringOption.Value = stringOption.oldValue = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();

                    option.optionBehaviour = stringOption;
                }
                option.optionBehaviour.gameObject.SetActive(true);
            }

            torMenu.Children = torOptions.ToArray();
            torSettings.gameObject.SetActive(false);

            /*            var numImpostorsOption = __instance.Children.FirstOrDefault(x => x.name == "NumImpostors").TryCast<NumberOption>();
                        if (numImpostorsOption != null) numImpostorsOption.ValidRange = new FloatRange(0f, 15f);*/

            var killCoolOption = __instance.Children.FirstOrDefault(x => x.name == "KillCooldown").TryCast<NumberOption>();
            if (killCoolOption != null) killCoolOption.ValidRange = new FloatRange(2.5f, 60f);

            var commonTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumCommonTasks").TryCast<NumberOption>();
            if (commonTasksOption != null) commonTasksOption.ValidRange = new FloatRange(0f, 4f);

            var shortTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumShortTasks").TryCast<NumberOption>();
            if (shortTasksOption != null) shortTasksOption.ValidRange = new FloatRange(0f, 23f);

            var longTasksOption = __instance.Children.FirstOrDefault(x => x.name == "NumLongTasks").TryCast<NumberOption>();
            if (longTasksOption != null) longTasksOption.ValidRange = new FloatRange(0f, 15f);
        }
    }

    [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.OnEnable))]
    public class KeyValueOptionEnablePatch
    {
        public static void Postfix(KeyValueOption __instance)
        {
            GameOptionsData gameOptions = PlayerControl.GameOptions;
            if (__instance.Title == StringNames.GameMapName)
            {
                __instance.Selected = gameOptions.MapId;
            }
            try
            {
                __instance.ValueText.text = __instance.Values[Mathf.Clamp(__instance.Selected, 0, __instance.Values.Count - 1)].Key;
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.OnEnable))]
    public class StringOptionEnablePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;

            __instance.OnValueChanged = new Action<OptionBehaviour>((o) => { });
            __instance.TitleText.text = option.getName();
            __instance.Value = __instance.oldValue = option.selection;
            __instance.ValueText.text = option.getString();

            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public class StringOptionIncreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection + 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public class StringOptionDecreasePatch
    {
        public static bool Prefix(StringOption __instance)
        {
            CustomOption option = CustomOption.options.FirstOrDefault(option => option.optionBehaviour == __instance);
            if (option == null) return true;
            option.updateSelection(option.selection - 1);
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSyncSettings))]
    public class RpcSyncSettingsPatch
    {
        public static void Postfix()
        {
            CustomOption.ShareOptionSelections();
        }
    }


    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
    class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (__instance.Children.Length < 100) return; // TODO: Introduce a cleaner way to seperate the TOR settings from the game settings

            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            float numItems = __instance.Children.Length;

            float offset = 2.75f;
            foreach (CustomOption option in CustomOption.options)
            {
                if (option?.optionBehaviour != null && option.optionBehaviour.gameObject != null)
                {
                    bool enabled = true;
                    var parent = option.parent;

                    if (AmongUsClient.Instance?.AmHost == false && CustomOptions.hideSettings.getBool())
                    {
                        enabled = false;
                    }

                    if (option.isHidden)
                    {
                        enabled = false;
                    }

                    while (parent != null && enabled)
                    {
                        enabled = parent.enabled;
                        parent = parent.parent;
                    }

                    option.optionBehaviour.gameObject.SetActive(enabled);
                    if (enabled)
                    {
                        offset -= option.isHeader ? 0.75f : 0.5f;
                        option.optionBehaviour.transform.localPosition = new Vector3(option.optionBehaviour.transform.localPosition.x, offset, option.optionBehaviour.transform.localPosition.z);

                        if (option.isHeader)
                        {
                            numItems += 0.5f;
                        }
                    }
                    else
                    {
                        numItems--;
                    }
                }
            }
            __instance.GetComponentInParent<Scroller>().ContentYBounds.max = -4.0f + numItems * 0.5f;
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Start))]
    class GameSettingMenuStartPatch
    {
        public static void Prefix(GameSettingMenu __instance)
        {
            __instance.HideForOnline = new Transform[] { };
        }

        public static void Postfix(GameSettingMenu __instance)
        {
            // Setup mapNameTransform
            var mapNameTransform = __instance.AllItems.FirstOrDefault(x => x.gameObject.activeSelf && x.name.Equals("MapName", StringComparison.OrdinalIgnoreCase));
            if (mapNameTransform == null) return;

            var options = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Collections.Generic.KeyValuePair<string, int>>();
            for (int i = 0; i < Constants.MapNames.Length; i++)
            {
                var kvp = new Il2CppSystem.Collections.Generic.KeyValuePair<string, int>();
                kvp.key = Constants.MapNames[i];
                kvp.value = i;
                options.Add(kvp);
            }
            mapNameTransform.GetComponent<KeyValueOption>().Values = options;
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldFlipSkeld))]
    class ConstantsShouldFlipSkeldPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (PlayerControl.GameOptions == null) return true;
            __result = PlayerControl.GameOptions.MapId == 3;
            return false;
        }

        public static bool aprilFools
        {
            get
            {
                try
                {
                    DateTime utcNow = DateTime.UtcNow;
                    DateTime t = new DateTime(utcNow.Year, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    DateTime t2 = t.AddDays(1.0);
                    if (utcNow >= t && utcNow <= t2)
                    {
                        return true;
                    }
                }
                catch
                {
                }
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(FreeWeekendShower), nameof(FreeWeekendShower.Start))]
    class FreeWeekendShowerPatch
    {
        public static bool Prefix()
        {
            return ConstantsShouldFlipSkeldPatch.aprilFools;
        }
    }

    [HarmonyPatch]
    class GameOptionsDataPatch
    {
        public static string tl(string key)
        {
            return ModTranslation.getString(key);
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(GameOptionsData).GetMethods().Where(x => x.ReturnType == typeof(string) && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(int));
        }

        public static string optionToString(CustomOption option)
        {
            if (option == null) return "";
            return $"{option.getName()}: {option.getString()}";
        }

        public static string optionsToString(CustomOption option, bool skipFirst = false)
        {
            if (option == null) return "";

            List<string> options = new List<string>();
            if (!option.isHidden && !skipFirst) options.Add(optionToString(option));
            if (option.enabled)
            {
                foreach (CustomOption op in option.children)
                {
                    string str = optionsToString(op);
                    if (str != "") options.Add(str);
                }
            }
            return string.Join("\n", options);
        }

        private static void Postfix(ref string __result)
        {

            bool hideSettings = AmongUsClient.Instance?.AmHost == false && CustomOptions.hideSettings.getBool();
            if (hideSettings)
            {
                return;
            }

            List<string> pages = new List<string>();
            pages.Add(__result);

            StringBuilder entry = new StringBuilder();
            List<string> entries = new List<string>();

            // First add the presets and the role counts
            entries.Add(optionToString(CustomOptions.presetSelection));

            var optionName = CustomOptions.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("SettingCrewmateRoles"));
            var min = CustomOptions.crewmateRolesCountMax.getSelection();
            var max = CustomOptions.crewmateRolesCountMax.getSelection();
            if (min > max) min = max;
            var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptions.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("SettingNeutralRoles"));
            min = CustomOptions.neutralRolesCountMax.getSelection();
            max = CustomOptions.neutralRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptions.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("SettingImpostorRoles"));
            min = CustomOptions.impostorRolesCountMax.getSelection();
            max = CustomOptions.impostorRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            entries.Add(entry.ToString().Trim('\r', '\n'));

            void addChildren(CustomOption option, ref StringBuilder entry, bool indent = true)
            {
                if (!option.enabled) return;

                foreach (var child in option.children)
                {
                    if (!child.isHidden)
                        entry.AppendLine((indent ? "    " : "") + optionToString(child));
                    addChildren(child, ref entry, indent);
                }
            }

            foreach (CustomOption option in CustomOption.options)
            {
                if ((option == CustomOptions.presetSelection) ||
                    (option == CustomOptions.crewmateRolesCountMax) ||
                    (option == CustomOptions.neutralRolesCountMax) ||
                    (option == CustomOptions.impostorRolesCountMax))
                {
                    continue;
                }

                if (option.parent == null)
                {
                    if (!option.enabled)
                    {
                        continue;
                    }

                    entry = new StringBuilder();
                    if (!option.isHidden)
                        entry.AppendLine(optionToString(option));

                    addChildren(option, ref entry, !option.isHidden);
                    entries.Add(entry.ToString().Trim('\r', '\n'));
                }
            }

            int maxLines = 28;
            int lineCount = 0;
            string page = "";
            foreach (var e in entries)
            {
                int lines = e.Count(c => c == '\n') + 1;

                if (lineCount + lines > maxLines)
                {
                    pages.Add(page);
                    page = "";
                    lineCount = 0;
                }

                page = page + e + "\n\n";
                lineCount += lines + 1;
            }

            page = page.Trim('\r', '\n');
            if (page != "")
            {
                pages.Add(page);
            }

            int numPages = pages.Count;
            int counter = SuperNewRolesPlugin.optionsPage = SuperNewRolesPlugin.optionsPage % numPages;

            __result = pages[counter].Trim('\r', '\n') + "\n\n" + tl("SettingPressTabForMore") + $" ({counter + 1}/{numPages})";
        }
    }

    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.GetAdjustedNumImpostors))]
    public static class GameOptionsGetAdjustedNumImpostorsPatch
    {
        public static bool Prefix(GameOptionsData __instance, ref int __result)
        {
            __result = PlayerControl.GameOptions.NumImpostors;
            return false;
        }
    }

    [HarmonyPatch(typeof(SaveManager), "GameHostOptions", MethodType.Getter)]
    public static class SaveManagerGameHostOptionsPatch
    {
        private static int numImpostors;
        public static void Prefix()
        {
            if (SaveManager.hostOptionsData == null)
            {
                SaveManager.hostOptionsData = SaveManager.LoadGameOptions("gameHostOptions");
            }

            numImpostors = SaveManager.hostOptionsData.NumImpostors;
        }

        public static void Postfix(ref GameOptionsData __result)
        {
            __result.NumImpostors = numImpostors;
        }
    }

    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static class GameOptionsNextPagePatch
    {
        public static void Postfix(KeyboardJoystick __instance)
        {
            if ((Input.GetKeyDown(KeyCode.Tab) || ConsoleJoystick.player.GetButtonDown(7)) && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
            {
                SuperNewRolesPlugin.optionsPage = SuperNewRolesPlugin.optionsPage + 1;
            }
        }
    }


    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public class GameSettingsScalePatch
    {
        public static void Prefix(HudManager __instance)
        {
            if (__instance.GameSettings != null) __instance.GameSettings.fontSize = 1.2f;
        }
    }

    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Start))]
    public class CreateOptionsPickerPatch
    {
        public static void Postfix(CreateOptionsPicker __instance)
        {
            int numImpostors = Math.Clamp(__instance.GetTargetOptions().NumImpostors, 1, 3);
            __instance.SetImpostorButtons(numImpostors);
        }
    }
}