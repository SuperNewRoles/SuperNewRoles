using System.Collections.Generic;
using UnityEngine;
using BepInEx.Configuration;
using System;
using System.Linq;
using HarmonyLib;
using Hazel;
using System.Reflection;
using System.Text;
using UnityEngine.Events;
using SuperNewRoles.Mode;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Intro;

namespace SuperNewRoles.CustomOption
{
    public enum CustomOptionType { 
        Generic,
        Impostor,
        Neutral,
        Crewmate

    }
    
    public class CustomOption
    {
        public static List<CustomOption> options = new List<CustomOption>();
        public static int preset = 0;

        public int id;
        public bool isSHROn;
        public CustomOptionType type;
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

        public CustomOption(int id, bool IsSHROn,CustomOptionType type,string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format)
        {
            this.id = id;
            this.isSHROn = IsSHROn;
            this.type = type;
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
                if (options.Any(x => x.id == id))
                {
                    SuperNewRolesPlugin.Logger.LogInfo("CustomOptionのId("+id+")が重複しています。");
                }
                if (Max < id)
                {
                    Max = id;
                }
            }
            options.Add(this);
        }
        public static int Max = 0;

        public static CustomOption Create(int id, bool IsSHROn, CustomOptionType type, string name, string[] selections, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, IsSHROn, type, name, selections, "", parent, isHeader, isHidden, format);
        }

        public static CustomOption Create(int id, bool IsSHROn, CustomOptionType type, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            List<float> selections = new List<float>();
            for (float s = min; s <= max; s += step)
                selections.Add(s);
            return new CustomOption(id, IsSHROn, type, name, selections.Cast<object>().ToArray(), defaultValue, parent, isHeader, isHidden, format);
        }

        public static CustomOption Create(int id, bool IsSHROn, CustomOptionType type, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "")
        {
            return new CustomOption(id, IsSHROn, type, name, new string[] { "optionOff", "optionOn" }, defaultValue ? "optionOn" : "optionOff", parent, isHeader, isHidden, format);
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
            if (CachedPlayer.AllPlayers.Count <= 1 || AmongUsClient.Instance?.AmHost == false && PlayerControl.LocalPlayer == null) return;

            MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareOptions, Hazel.SendOption.Reliable);
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
            return ModTranslation.getString(name);
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
        public static List<CustomRoleOption> RoleOptions = new List<CustomRoleOption>();

        public CustomOption countOption = null;

        public RoleId RoleId;

        public int rate
        {
            get
            {
                return getSelection();
            }
        }

        public bool isRoleEnable
        {
            get
            {
                return getSelection() != 0;
            }
        }

        public IntroDate Intro
        {
            get
            {
                return IntroDate.GetIntroDate(RoleId);
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

        public CustomRoleOption(int id, bool isSHROn, CustomOptionType type, string name, Color color, int max = 15) :
            base(id, isSHROn, type, CustomOptions.cs(color, name), CustomOptions.rates, "", null, true, false, "")
        {
            try
            {
                this.RoleId = IntroDate.IntroDatas.FirstOrDefault((_) => {
                    return _.NameKey + "Name" == name;
                }).RoleId;
            }
            catch { }
            RoleOptions.Add(this);
            if (max > 1)
                countOption = CustomOption.Create(id + 10000, isSHROn, type, "roleNumAssigned", 1f, 1f, 15f, 1f, this, format: "unitPlayers");
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
            
            if (Mode.ModeHandler.IsBlockVanilaRole()) __result = 0;

            if (role != RoleTypes.GuardianAngel) return;

            if (Mode.ModeHandler.IsBlockGuardianAngelRole()) __result = 0;

        }
    }

    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
    class GameOptionsMenuStartPatch
    {
        public static void Postfix(GameOptionsMenu __instance)
        {
            if (GameObject.Find("SNRSettings") != null)
            { // Settings setup has already been performed, fixing the title of the tab and returning
                GameObject.Find("SNRSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("SettingSuperNewRoles"));
                return;
            }
            if (GameObject.Find("ImpostorSettings") != null)
            {
                GameObject.Find("ImpostorSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("SettingImpostor"));
                return;
            }
            if (GameObject.Find("NeutralSettings") != null)
            {
                GameObject.Find("NeutralSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("SettingNeutral"));
                return;
            }
            if (GameObject.Find("CrewmateSettings") != null)
            {
                GameObject.Find("CrewmateSettings").transform.FindChild("GameGroup").FindChild("Text").GetComponent<TMPro.TextMeshPro>().SetText(ModTranslation.getString("SettingCrewmate"));
                return;
            }
            // Setup TOR tab
            var template = UnityEngine.Object.FindObjectsOfType<StringOption>().FirstOrDefault();
            if (template == null) return;
            var gameSettings = GameObject.Find("Game Settings");
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();

            var snrSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var snrMenu = snrSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            snrSettings.name = "SNRSettings";
            snrSettings.transform.FindChild("GameGroup").FindChild("SliderInner").name = "GenericSetting";

            var impostorSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var impostorMenu = impostorSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            impostorSettings.name = "ImpostorSettings";
            impostorSettings.transform.FindChild("GameGroup").FindChild("SliderInner").name = "ImpostorSetting";

            var neutralSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var neutralMenu = neutralSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            neutralSettings.name = "NeutralSettings";
            neutralSettings.transform.FindChild("GameGroup").FindChild("SliderInner").name = "NeutralSetting";

            var crewmateSettings = UnityEngine.Object.Instantiate(gameSettings, gameSettings.transform.parent);
            var crewmateMenu = crewmateSettings.transform.FindChild("GameGroup").FindChild("SliderInner").GetComponent<GameOptionsMenu>();
            crewmateSettings.name = "CrewmateSettings";
            crewmateSettings.transform.FindChild("GameGroup").FindChild("SliderInner").name = "CrewmateSetting";

            var roleTab = GameObject.Find("RoleTab");
            var gameTab = GameObject.Find("GameTab");

            var snrTab = UnityEngine.Object.Instantiate(roleTab, roleTab.transform.parent);
            var snrTabHighlight = snrTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            snrTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.TabIcon.png", 100f);

            var impostorTab = UnityEngine.Object.Instantiate(roleTab, snrTab.transform);
            var impostorTabHighlight = impostorTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            impostorTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Setting_Impostor.png", 100f);
            impostorTab.name = "ImpostorTab";

            var neutralTab = UnityEngine.Object.Instantiate(roleTab, impostorTab.transform);
            var neutralTabHighlight = neutralTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            neutralTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Setting_Neutral.png", 100f);
            neutralTab.name = "NeutralTab";

            var crewmateTab = UnityEngine.Object.Instantiate(roleTab, neutralTab.transform);
            var crewmateTabHighlight = crewmateTab.transform.FindChild("Hat Button").FindChild("Tab Background").GetComponent<SpriteRenderer>();
            crewmateTab.transform.FindChild("Hat Button").FindChild("Icon").GetComponent<SpriteRenderer>().sprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Setting_Crewmate.png", 100f);
            crewmateTab.name = "CrewmateTab";

            // Position of Tab Icons
            gameTab.transform.position += Vector3.left * 3f;
            roleTab.transform.position += Vector3.left * 3f;
            snrTab.transform.position += Vector3.left * 2f;
            impostorTab.transform.localPosition = Vector3.right * 1f;
            neutralTab.transform.localPosition = Vector3.right * 1f;
            crewmateTab.transform.localPosition = Vector3.right * 0.95f;

            var tabs = new GameObject[] { gameTab, roleTab, snrTab, impostorTab, neutralTab, crewmateTab};
            for (int i = 0; i < tabs.Length; i++)
            {
                var button = tabs[i].GetComponentInChildren<PassiveButton>();
                int copiedIndex = i;
                button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
                button.OnClick.AddListener((UnityAction)(() => {
                    gameSettingMenu.RegularGameSettings.SetActive(false);
                    gameSettingMenu.RolesSettings.gameObject.SetActive(false);
                    snrSettings.gameObject.SetActive(false);
                    impostorSettings.gameObject.SetActive(false);
                    neutralSettings.gameObject.SetActive(false);
                    crewmateSettings.gameObject.SetActive(false);
                    gameSettingMenu.GameSettingsHightlight.enabled = false;
                    gameSettingMenu.RolesSettingsHightlight.enabled = false;
                    snrTabHighlight.enabled = false;
                    impostorTabHighlight.enabled = false;
                    neutralTabHighlight.enabled = false;
                    crewmateTabHighlight.enabled = false;
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
                        snrSettings.gameObject.SetActive(true);
                        snrTabHighlight.enabled = true;
                    }
                    else if (copiedIndex == 3)
                    {
                        impostorSettings.gameObject.SetActive(true);
                        impostorTabHighlight.enabled = true;
                    }
                    else if (copiedIndex == 4)
                    {
                        neutralSettings.gameObject.SetActive(true);
                        neutralTabHighlight.enabled = true;
                    }
                    else if (copiedIndex == 5)
                    {
                        crewmateSettings.gameObject.SetActive(true);
                        crewmateTabHighlight.enabled = true;
                    }
                    /*
                    else if (copiedIndex == 6)
                    {
                        modifierSettings.gameObject.SetActive(true);
                        modifierTabHighlight.enabled = true;
                    }
                    */
                }));
            }


            foreach (OptionBehaviour option in snrMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            foreach (OptionBehaviour option in impostorMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            foreach (OptionBehaviour option in neutralMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            foreach (OptionBehaviour option in crewmateMenu.GetComponentsInChildren<OptionBehaviour>())
                UnityEngine.Object.Destroy(option.gameObject);
            List<OptionBehaviour> snrOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> impostorOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> neutralOptions = new List<OptionBehaviour>();
            List<OptionBehaviour> crewmateOptions = new List<OptionBehaviour>();

            List<Transform> menus = new List<Transform>() { snrMenu.transform, impostorMenu.transform, neutralMenu.transform, crewmateMenu.transform };
            List<List<OptionBehaviour>> optionBehaviours = new List<List<OptionBehaviour>>() { snrOptions, impostorOptions, neutralOptions, crewmateOptions };

            for (int i = 0; i < CustomOption.options.Count; i++)
            {
                CustomOption option = CustomOption.options[i];
                if (option.optionBehaviour == null)
                {
                    StringOption stringOption = UnityEngine.Object.Instantiate(template, menus[(int)option.type]);
                    optionBehaviours[(int)option.type].Add(stringOption);
                    stringOption.OnValueChanged = new Action<OptionBehaviour>((o) => { });
                    stringOption.TitleText.text = option.name;
                    stringOption.Value = stringOption.oldValue = option.selection;
                    stringOption.ValueText.text = option.selections[option.selection].ToString();

                    option.optionBehaviour = stringOption;
                }
                option.optionBehaviour.gameObject.SetActive(true);
            }

            snrMenu.Children = snrOptions.ToArray();
            snrSettings.gameObject.SetActive(false);

            impostorMenu.Children = impostorOptions.ToArray();
            impostorSettings.gameObject.SetActive(false);

            neutralMenu.Children = neutralOptions.ToArray();
            neutralSettings.gameObject.SetActive(false);

            crewmateMenu.Children = crewmateOptions.ToArray();
            crewmateSettings.gameObject.SetActive(false);

            var numImpostorsOption = __instance.Children.FirstOrDefault(x => x.name == "NumImpostors").TryCast<NumberOption>();
            if (numImpostorsOption != null) numImpostorsOption.ValidRange = new FloatRange(0f, 15f);

            var PlayerSpeedModOption = __instance.Children.FirstOrDefault(x => x.name == "PlayerSpeed").TryCast<NumberOption>();
            if (PlayerSpeedModOption != null) PlayerSpeedModOption.ValidRange = new FloatRange(-5.5f, 5.5f);

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
        public static bool Prefix(PlayerControl __instance,[HarmonyArgument(0)] GameOptionsData gameOptions)
        {
            if (AmongUsClient.Instance.AmHost && !DestroyableSingleton<TutorialManager>.InstanceExists)
            {
                PlayerControl.GameOptions = gameOptions;
                SaveManager.GameHostOptions = gameOptions;
                MessageWriter obj = AmongUsClient.Instance.StartRpc(__instance.NetId, 2, SendOption.Reliable);
                obj.WriteBytesAndSize(gameOptions.ToBytes(6));
                obj.EndMessage();
            }
            return false;
        }
        public static void Postfix()
        {
            CustomOption.ShareOptionSelections();
        }
    }


    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
   static class GameOptionsMenuUpdatePatch
    {
        private static float timer = 1f;
        public static CustomOptionType getCustomOptionType(string name)
        {
            switch (name)
            {
                case "GenericSetting":
                    return CustomOptionType.Generic;
                case "ImpostorSetting":
                    return CustomOptionType.Impostor;
                case "NeutralSetting":
                    return CustomOptionType.Neutral;
                case "CrewmateSetting":
                    return CustomOptionType.Crewmate;
            }
            return CustomOptionType.Crewmate;
        }
        public static bool isHidden(this CustomOption option)
        {
            if (option.isHidden) return true;
            if (option.isSHROn) { return false; }
            else { return ModeHandler.isMode(ModeId.SuperHostRoles, false); }
            return false;
        }
        public static void Postfix(GameOptionsMenu __instance)
        {
            var gameSettingMenu = UnityEngine.Object.FindObjectsOfType<GameSettingMenu>().FirstOrDefault();
            if (gameSettingMenu.RegularGameSettings.active || gameSettingMenu.RolesSettings.gameObject.active) return;

            timer += Time.deltaTime;
            if (timer < 0.1f) return;
            timer = 0f;

            float numItems = __instance.Children.Length;

            float offset = 2.75f;
            CustomOptionType type = getCustomOptionType(__instance.name);
            foreach (CustomOption option in CustomOption.options)
            {
                if (option.type != type) continue;
                if (option?.optionBehaviour != null && option.optionBehaviour.gameObject != null)
                {
                    bool enabled = true;
                    var parent = option.parent;

                    if (AmongUsClient.Instance?.AmHost == false && CustomOptions.hideSettings.getBool())
                    {
                        enabled = false;
                    }

                    if (option.isHidden())
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
            var mapNameTransform = __instance.AllItems.FirstOrDefault(x => x.name.Equals("MapName", StringComparison.OrdinalIgnoreCase));
            if (mapNameTransform == null) return;
            mapNameTransform.gameObject.active = true;

            foreach (Transform i in __instance.AllItems.ToList())
            {
                float num = -0.5f;
                if (i.name.Equals("MapName", StringComparison.OrdinalIgnoreCase)) num = 0.25f;
                if (i.name.Equals("NumImpostors", StringComparison.OrdinalIgnoreCase)) num = -0.5f;
                if (i.name.Equals("ResetToDefault", StringComparison.OrdinalIgnoreCase)) num = 0f;
                i.position += new Vector3(0, num, 0);
            }
            __instance.Scroller.ContentYBounds.max += 0.5F;
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

    [HarmonyPatch(typeof(GameOptionsData),nameof(GameOptionsData.ToHudString))]
    class tohudstring
    {
        public static bool Prefix(ref string __result,GameOptionsData __instance, [HarmonyArgument(0)] int numPlayers)
        {
            __instance.settings.Length = 0;
            try
            {
                __instance.settings.AppendLine(FastDestroyableSingleton<TranslationController>.Instance.GetString(__instance.isDefaults ? StringNames.GameRecommendedSettings : StringNames.GameCustomSettings));
                int num = 0;
                try
                {
                    num = GameOptionsData.MaxImpostors[numPlayers];
                }
                catch
                {
                    num = __instance.NumImpostors;
                }
                int num2 = ((__instance.MapId == 0 && Constants.ShouldFlipSkeld()) ? 3 : __instance.MapId);
                string value = Constants.MapNames[num2];
                __instance.AppendItem(__instance.settings, StringNames.GameMapName, value);
                __instance.settings.Append($"{FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameNumImpostors)}: {__instance.NumImpostors}");
                if (__instance.NumImpostors > num)
                {
                    __instance.settings.Append($" ({FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Limit)}: {num})");
                }
                __instance.settings.AppendLine();
                if (__instance.gameType == GameType.Normal)
                {
                    __instance.AppendItem(__instance.settings, StringNames.GameConfirmImpostor, __instance.ConfirmImpostor);
                    __instance.AppendItem(__instance.settings, StringNames.GameNumMeetings, __instance.NumEmergencyMeetings);
                    __instance.AppendItem(__instance.settings, StringNames.GameAnonymousVotes, __instance.AnonymousVotes);
                    __instance.AppendItem(__instance.settings, StringNames.GameEmergencyCooldown, string.Format(FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev), __instance.EmergencyCooldown));
                    __instance.AppendItem(__instance.settings, StringNames.GameDiscussTime, string.Format(FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev), __instance.DiscussionTime));
                    if (__instance.VotingTime > 0)
                    {
                        __instance.AppendItem(__instance.settings, StringNames.GameVotingTime, string.Format(FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev), __instance.VotingTime));
                    }
                    else
                    {
                        __instance.AppendItem(__instance.settings, StringNames.GameVotingTime, FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev, "∞"));
                    }
                    __instance.AppendItem(__instance.settings, StringNames.GamePlayerSpeed, __instance.PlayerSpeedMod, "x");
                    __instance.AppendItem(__instance.settings, StringNames.GameCrewLight, __instance.CrewLightMod, "x");
                    __instance.AppendItem(__instance.settings, StringNames.GameImpostorLight, __instance.ImpostorLightMod, "x");
                    __instance.AppendItem(__instance.settings, StringNames.GameKillCooldown, string.Format(FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSecondsAbbrev), __instance.KillCooldown));
                    __instance.AppendItem(__instance.settings, StringNames.GameKillDistance, FastDestroyableSingleton<TranslationController>.Instance.GetString((StringNames)(204 + __instance.KillDistance)));
                    __instance.AppendItem(__instance.settings, StringNames.GameTaskBarMode, FastDestroyableSingleton<TranslationController>.Instance.GetString((StringNames)(277 + __instance.TaskBarMode)));
                    __instance.AppendItem(__instance.settings, StringNames.GameVisualTasks, __instance.VisualTasks);
                    __instance.AppendItem(__instance.settings, StringNames.GameCommonTasks, __instance.NumCommonTasks);
                    __instance.AppendItem(__instance.settings, StringNames.GameLongTasks, __instance.NumLongTasks);
                    __instance.AppendItem(__instance.settings, StringNames.GameShortTasks, __instance.NumShortTasks);
                    if (__instance.gameType == GameType.Normal)
                    {
                        RoleBehaviour[] allRoles = DestroyableSingleton<RoleManager>.Instance.AllRoles;
                        foreach (RoleBehaviour roleBehaviour in allRoles)
                        {
                            if (roleBehaviour.Role != 0 && roleBehaviour.Role != RoleTypes.Impostor)
                            {
                                __instance.AppendItem(__instance.settings, FastDestroyableSingleton<TranslationController>.Instance.GetString(roleBehaviour.StringName) + ": " + string.Format(FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoleChanceAndQuantity), __instance.RoleOptions.GetNumPerGame(roleBehaviour.Role), __instance.RoleOptions.GetChancePerGame(roleBehaviour.Role)));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogInfo("エラー:" + e);
            }
            __result = __instance.settings.ToString();
            return false;
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
            if (!GameOptionsMenuUpdatePatch.isHidden(option) && !skipFirst) options.Add(optionToString(option));
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
        public static string DefaultResult = "";
        public static string ResultData()
        {
            bool hideSettings = AmongUsClient.Instance?.AmHost == false && CustomOptions.hideSettings.getBool();
            if (hideSettings)
            {
                return DefaultResult;
            }

            List<string> pages = new List<string>();
            pages.Add(DefaultResult);

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

            optionName = CustomOptions.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("SettingCrewmateGhostRoles"));
            min = CustomOptions.crewmateGhostRolesCountMax.getSelection();
            max = CustomOptions.crewmateGhostRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptions.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("SettingNeutralRoles"));
            min = CustomOptions.neutralRolesCountMax.getSelection();
            max = CustomOptions.neutralRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptions.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("SettingNeutralGhostRoles"));
            min = CustomOptions.neutralGhostRolesCountMax.getSelection();
            max = CustomOptions.neutralGhostRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptions.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("SettingImpostorRoles"));
            min = CustomOptions.impostorRolesCountMax.getSelection();
            max = CustomOptions.impostorRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            optionName = CustomOptions.cs(new Color(204f / 255f, 204f / 255f, 0, 1f), tl("SettingImpostorGhostRoles"));
            min = CustomOptions.impostorGhostRolesCountMax.getSelection();
            max = CustomOptions.impostorGhostRolesCountMax.getSelection();
            if (min > max) min = max;
            optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
            entry.AppendLine($"{optionName}: {optionValue}");

            entries.Add(entry.ToString().Trim('\r', '\n'));

            void addChildren(CustomOption option, ref StringBuilder entry, bool indent = true)
            {
                if (!option.enabled) return;

                foreach (var child in option.children)
                {
                    if (!GameOptionsMenuUpdatePatch.isHidden(option))
                        entry.AppendLine((indent ? "    " : "") + optionToString(child));
                    addChildren(child, ref entry, indent);
                }
            }

            foreach (CustomOption option in CustomOption.options)
            {
                if ((option == CustomOptions.presetSelection) ||
                    (option == CustomOptions.crewmateRolesCountMax) ||
                    (option == CustomOptions.crewmateGhostRolesCountMax) ||
                    (option == CustomOptions.neutralRolesCountMax) ||
                    (option == CustomOptions.neutralGhostRolesCountMax) ||
                    (option == CustomOptions.impostorRolesCountMax) ||
                    (option == CustomOptions.impostorGhostRolesCountMax) ||
                    (option == CustomOptions.hideSettings))
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
                    if (!GameOptionsMenuUpdatePatch.isHidden(option))
                    {
                        entry.AppendLine(optionToString(option));
                    }
                    addChildren(option, ref entry, !GameOptionsMenuUpdatePatch.isHidden(option));
                    if (entry.ToString().Trim('\n', '\r') != "\r" && entry.ToString().Trim('\n', '\r') != "")
                    {
                        entries.Add(entry.ToString().Trim('\n', '\r'));
                    }
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
            return pages[counter].Trim('\r', '\n') + "\n\n" + tl("SettingPressTabForMore") + $" ({counter + 1}/{numPages})";
        }
        public static void Postfix(ref string __result)
        {
            DefaultResult = __result;
            __result = ResultData();
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
            if ((Input.GetKeyDown(KeyCode.Tab) || ConsoleJoystick.player.GetButtonDown(7)))
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
    }/*

    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.))]
    public class CreateOptionsPickerPatch
    {
        public static void Postfix(CreateOptionsPicker __instance)
        {
            int numImpostors = Math.Clamp(__instance.GetTargetOptions().NumImpostors, 1, 3);
            __instance.SetImpostorButtons(numImpostors);
        }
    }*/
}