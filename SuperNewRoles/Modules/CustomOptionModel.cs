using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomModOption;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using TMPro;
using UnityEngine;
using static SuperNewRoles.Modules.CustomRegulation;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Modules;

public enum CustomOptionType
{
    Generic,
    Impostor,
    Neutral,
    Crewmate,
    Modifier,
    MatchTag,
    Empty // 使用されない
}

public class CustomOption
{
    public static List<CustomOption> options = new();
    public static List<CustomOption> SpecialHiddenRuleOptions { get; } = new();
    private static Dictionary<int, CustomOption> optionids = new();
    public static int preset = 0;
    public static Dictionary<uint, byte> CurrentValues;
    public static bool IsValuesUpdated;

    public int id;
    public bool isSHROn;
    public CustomOptionType type;
    public string name;
    public string format;
    public System.Object[] selections;

    public int defaultSelection;
    public int HostSelection;
    public int ClientSelection;
    public int ClientSelectedSelection;
    public int selection
    {
        get
        {
            return AmongUsClient.Instance == null || AmongUsClient.Instance.AmHost ? RegulationData.Selected == 0 ? ClientSelection : ClientSelectedSelection : HostSelection;
        }
        set
        {
            if (AmongUsClient.Instance == null || AmongUsClient.Instance.AmHost)
            {
                if (RegulationData.Selected == 0)
                {
                    ClientSelection = value;
                }
                else
                {
                    ClientSelectedSelection = value;
                }
                if (AmongUsClient.Instance != null && AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Joined && AmongUsClient.Instance.IsGamePublic)
                {
                    if (id is (>= 100000 and < 600000) or 0) MatchMaker.UpdateOption();
                    else if (id is >= 600000 and < 700000) MatchMaker.UpdateTags();
                    else Logger.Error($"設定idが規定範囲外でした : {id}", "MatchMakerUpdate");
                }
            }
            else
            {
                HostSelection = value;
            }
            UpdateCanShows(this);
        }
    }
    public CustomOption parent;
    public List<CustomOption> children;
    public bool isHeader;
    public bool isHidden;
    public RoleId RoleId;
    public int openSelection { get; } = -1;
    public bool IsToggle { get; }
    public Func<bool> CanShowFunc { get; }
    public bool HasCanShowAction { get; }
    public bool CanShowByFunc;
    public bool WithHeader;
    public string HeaderText;

    public static void UpdateCanShows(CustomOption opt)
    {
        Task.Run(() =>
        {
            ModeId modeId = ModeHandler.GetMode(false);
            foreach (CustomOption option in SpecialHiddenRuleOptions)
            {
                if (option == opt)
                    continue;
                if (!option.Enabled)
                    continue;
                if (option.IsHidden(modeId))
                    continue;
                option.CanShowByFunc = option.CanShowFunc();
            }
        });
    }

    public virtual bool Enabled
    {
        get
        {
            return GetBool();
        }
    }

    // Option creation
    public CustomOption()
    {
        openSelection = -1;
    }
    public static CustomOption GetOption(int id)
    {
        return optionids.TryGetValue(id, out CustomOption opt) ? opt : null;
    }

    public CustomOption(int Id, bool IsSHROn, CustomOptionType type, string name, System.Object[] selections, System.Object defaultValue, CustomOption parent, bool isHeader, bool isHidden, string format, int openSelection = -1, RoleId? roleId = null, Func<bool> canShow = null, bool isToggle = false, bool withHeader = false, string headerText = null)
    {
        this.id = Id;
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
        this.RoleId = roleId.HasValue ? roleId.Value : RoleId.DefaultRole;
        this.openSelection = openSelection;
        this.IsToggle = isToggle;
        this.WithHeader = withHeader;
        this.HeaderText = headerText;

        this.CanShowFunc = canShow;
        this.HasCanShowAction = canShow != null;
        this.CanShowByFunc = false;
        if (HasCanShowAction)
        {
            SpecialHiddenRuleOptions.Add(this);
        }

        if (parent != null)
        {
            this.RoleId = parent.RoleId;
        }

        this.children = new List<CustomOption>();
        if (parent != null)
        {
            parent.children.Add(this);
        }

        selection = Mathf.Clamp(CurrentValues.TryGetValue((uint)id, out byte valueselection) ? valueselection : defaultSelection, 0, selections.Length - 1);

        bool duplication = options.Any(x => x.id == Id);
        string duplicationString = $"CustomOptionのId({Id})が重複しています。";

        SettingPattern pattern = GetSettingPattern(Id);
        switch (pattern)
        {
            case SettingPattern.ErrorId:
                Logger.Info($"CustomOptionのId({Id})は Id規則に従っていません。", $"{SettingPattern.ErrorId}");
                if (duplication) Logger.Info(duplicationString, $"{SettingPattern.ErrorId}");
                break;
            case SettingPattern.GenericId:
                if (GenericIdMax < Id) GenericIdMax = Id;
                if (duplication) Logger.Info(duplicationString, $"{SettingPattern.GenericId}");
                break;
            case SettingPattern.ImpostorId:
                if (ImpostorIdMax < Id) ImpostorIdMax = Id;
                if (duplication) Logger.Info(duplicationString, $"{SettingPattern.ImpostorId}");
                break;
            case SettingPattern.NeutralId:
                if (NeutralIdMax < Id) NeutralIdMax = Id;
                if (duplication) Logger.Info(duplicationString, $"{SettingPattern.NeutralId}");
                break;
            case SettingPattern.CrewmateId:
                if (CrewmateIdMax < Id) CrewmateIdMax = Id;
                if (duplication) Logger.Info(duplicationString, $"{SettingPattern.CrewmateId}");
                break;
            case SettingPattern.ModifierId:
                if (ModifierIdMax < Id) ModifierIdMax = Id;
                if (duplication) Logger.Info(duplicationString, $"{SettingPattern.ModifierId}");
                break;
            case SettingPattern.MatchingTagId:
                if (MatchingTagIdMax < Id) MatchingTagIdMax = Id;
                if (duplication) Logger.Info(duplicationString, $"{SettingPattern.MatchingTagId}");
                break;
        }
        options.Add(this);
        if (!optionids.TryAdd(id, this))
        {
            Logger.Info("optionidsの追加に失敗しました。");
        }
    }

    public static int GenericIdMax = 0;
    public static int ImpostorIdMax = 0;
    public static int NeutralIdMax = 0;
    public static int CrewmateIdMax = 0;
    public static int ModifierIdMax = 0;
    public static int MatchingTagIdMax = 0;

    private SettingPattern GetSettingPattern(int id)
    {
        if (id == 0) return SettingPattern.GenericId;
        if (id is >= 100000 and < 200000) return SettingPattern.GenericId;
        if (id is >= 200000 and < 300000) return SettingPattern.ImpostorId;
        if (id is >= 300000 and < 400000) return SettingPattern.NeutralId;
        if (id is >= 400000 and < 500000) return SettingPattern.CrewmateId;
        if (id is >= 500000 and < 600000) return SettingPattern.ModifierId;
        if (id is >= 600000 and < 700000) return SettingPattern.MatchingTagId;

        return SettingPattern.ErrorId;
    }

    private enum SettingPattern
    {
        ErrorId = 0,
        GenericId = 100000,
        ImpostorId = 200000,
        NeutralId = 300000,
        CrewmateId = 400000,
        ModifierId = 500000,
        MatchingTagId = 600000,
    }
    public static CustomOption Create(int id, bool IsSHROn, CustomOptionType type, string name, string[] selections, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "", int openSelection = -1, Func<bool> canShow = null, bool withHeader = false, string headerText = null)
    {
        return new CustomOption(id, IsSHROn, type, name, selections, "", parent, isHeader, isHidden, format, openSelection, canShow: canShow, withHeader: withHeader, headerText: headerText);
    }

    public static CustomOption Create(int id, bool IsSHROn, CustomOptionType type, string name, float defaultValue, float min, float max, float step, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "", int openSelection = -1, Func<bool> canShow = null, bool withHeader = false, string headerText = null)
    {
        List<float> selections = new();
        for (float s = min; s <= max; s += step)
            selections.Add(s);
        return new CustomOption(id, IsSHROn, type, name, selections.Cast<object>().ToArray(), defaultValue, parent, isHeader, isHidden, format, openSelection, canShow: canShow, withHeader: withHeader, headerText: headerText);
    }

    public static CustomOption Create(int id, bool IsSHROn, CustomOptionType type, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "", int openSelection = -1, Func<bool> canShow = null, bool withHeader = false, string headerText = null)
    {
        return new CustomOption(id, IsSHROn, type, name, new string[] { "optionOff", "optionOn" }, defaultValue ? "optionOn" : "optionOff", parent, isHeader, isHidden, format, openSelection, canShow: canShow, isToggle: true, withHeader: withHeader, headerText: headerText);
    }

    public static CustomRoleOption SetupCustomRoleOption(int id, bool IsSHROn, RoleId roleId, CustomOptionType type = CustomOptionType.Empty, int max = 1, bool isHidden = false)
    {
        if (type is CustomOptionType.Empty)
            type = CustomRoles.GetRoleTeam(roleId) switch
            {
                TeamRoleType.Impostor => CustomOptionType.Impostor,
                TeamRoleType.Neutral => CustomOptionType.Neutral,
                TeamRoleType.Crewmate => CustomOptionType.Crewmate,
                _ => CustomOptionType.Generic
            };
        return new CustomRoleOption(id, IsSHROn, type, $"{roleId}Name", CustomRoles.GetRoleColor(roleId), max, isHidden, roleId);
    }

    public static CustomOption CreateMatchMakeTag(int id, bool IsSHROn, string name, bool defaultValue, CustomOption parent = null, bool isHeader = false, bool isHidden = false, string format = "", CustomOptionType type = CustomOptionType.MatchTag, bool withHeader = false, string headerText = null)
    {
        return new CustomOption(id, IsSHROn, type, name, new string[] { "optionOff", "optionOn" }, defaultValue ? "optionOn" : "optionOff", parent, isHeader, isHidden, format, isToggle: true, withHeader: withHeader, headerText: headerText);
    }

    // Static behaviour

    public static void SwitchPreset(int newPreset)
    {
        OptionSaver.WriteNowPreset();
        preset = newPreset;
        (bool suc, int code, Dictionary<uint, byte> data) = OptionSaver.LoadPreset(preset);
        if (!suc && code == -1)
        {
            foreach (CustomOption option in options)
            {
                if (option.id <= 0) continue;
                option.selection = option.defaultSelection;
            }
            CurrentValues = new();
            OptionSaver.WriteNowPreset();
            return;
        }
        else if (!suc)
        {
            Logger.Info("CustomOptionGetPresetError:" + code.ToString());
            return;
        }
        foreach (CustomOption option in options)
        {
            if (option.id <= 0) continue;

            option.selection = Mathf.Clamp(data.TryGetValue((uint)option.id, out byte value) ? value : option.defaultSelection, 0, option.selections.Length - 1);
        }
        CurrentValues = data;
    }

    public static void ShareOptionSelections(CustomOption opt)
    {
        if (CachedPlayer.AllPlayers.Count <= 1 || AmongUsClient.Instance?.AmHost == false || PlayerControl.LocalPlayer == null) return;

        MessageWriter messageWriter = RPCHelper.StartRPC(CustomRPC.ShareOptions);
        messageWriter.WritePacked((uint)1);
        messageWriter.WritePacked((uint)opt.id);
        messageWriter.WritePacked(Convert.ToUInt32(opt.selection));
        messageWriter.EndRPC();
    }

    public static void ShareOptionSelections()
    {
        if (CachedPlayer.AllPlayers.Count <= 1 || AmongUsClient.Instance?.AmHost == false || PlayerControl.LocalPlayer == null) return;

        int count = 0;
        MessageWriter messageWriter;
        while (true)
        {
            messageWriter = RPCHelper.StartRPC(CustomRPC.ShareOptions);
            if ((options.Count - count) <= 200)
            {
                messageWriter.WritePacked((uint)(options.Count - count));
            }
            else
            {
                messageWriter.WritePacked((uint)200);
            }
            for (int i = 0; i < 200; i++)
            {
                if (options.Count <= count) break;
                CustomOption option = options[count];
                messageWriter.WritePacked((uint)option.id);
                messageWriter.WritePacked(Convert.ToUInt32(option.selection));
                count++;
            }
            messageWriter.EndRPC();
            if (options.Count <= count) break;
        }
    }

    // Getter

    public virtual int GetSelection()
    {
        return selection;
    }

    public virtual bool GetBool()
    {
        return selection > 0;
    }

    public virtual float GetFloat()
    {
        return (float)selections[selection];
    }

    public virtual int GetInt()
    {
        return (int)GetFloat();
    }

    public virtual string GetString()
    {
        string sel = selections[selection].ToString();
        return format != "" ? sel : ModTranslation.GetString(sel);
    }

    public virtual string GetName() => ModTranslation.GetString(name);

    /* 今後文字列の結合が必要になった時にコメントアウトを解除してください。
    // "+="で文字を連結するより、連結特化のStringBuilderクラスを使用して連結する事で、
    // オブジェクト作成回数を減らし、メモリ使用量を削減できる為効率的であると、ChatGPTさんがこのコードを提案して下さったので使用。
    public virtual string GetName()
    {
        string pattern = @"[ + ]|(<)|(>)";
        Regex regex = new(pattern);

        string[] names = regex.Split(name);
        StringBuilder translatedNameBuilder = new();
        foreach (string str in names)
        {
            string translatedStr = ModTranslation.GetString(str);
            translatedNameBuilder.Append(translatedStr);
        }
        return translatedNameBuilder.ToString();
    }
    */

    // Option changes

    public virtual void SetSelection(int set)
    {
        selection = (set + selections.Length) % selections.Length;
        ShareOption();
    }
    public virtual void SetSelection(bool set)
    {
        selection = set ? 1 : 0;
        ShareOption();
    }

    public virtual void SelectionAddition(int addition)
    {
        selection = (selection + addition + selections.Length) % selections.Length;
        ShareOption();
    }

    public virtual void ShareOption()
    {
        if ((AmongUsClient.Instance?.AmHost) != true || !PlayerControl.LocalPlayer) return;
        if (id == 0)
        {
            SwitchPreset(selection);
            ShareOptionSelections();
        }
        else if (AmongUsClient.Instance.AmHost && RegulationData.Selected == 0)
        {
            CurrentValues[(uint)id] = (byte)selection;
            IsValuesUpdated = true;
            ShareOptionSelections(this);
        }
        else ShareOptionSelections(this);
    }

}
public class CustomRoleOption : CustomOption
{
    public static Dictionary<RoleId, CustomRoleOption> RoleOptions = new();

    public CustomOption countOption = null;

    public int Rate
    {
        get
        {
            return GetSelection();
        }
    }

    public bool IsRoleEnable
    {
        get
        {
            return GetSelection() != 0;
        }
    }

    public IntroInfo Introinfo
    {
        get
        {
            return IntroInfo.GetIntroInfo(RoleId);
        }
    }

    public int Count
    {
        get
        {
            return countOption != null ? Mathf.RoundToInt(countOption.GetFloat()) : 1;
        }
    }

    public (int, int) Data
    {
        get
        {
            return (Rate, Count);
        }
    }

    public CustomRoleOption(int id, bool isSHROn, CustomOptionType type, string name, Color color, int max = 15, bool isHidden = false, RoleId? role = null) :
        base(id, isSHROn, type, CustomOptionHolder.Cs(color, name), CustomOptionHolder.rates, "", null, true, false, "", roleId: role)
    {
        if (!role.HasValue)
        {
            try
            {
                IntroData intro = IntroData.Intros.Values.FirstOrDefault((_) =>
                {
                    return _.NameKey + "Name" == name;
                });
                if (intro != null)
                {
                    this.RoleId = intro.RoleId;
                }
                else
                {
                    Logger.Info("RoleId取得できませんでした:" + name, "CustomRoleOption");
                }
            }
            catch
            {
                Logger.Info("RoleId取得でエラーが発生しました:" + name, "CustomRoleOption");
            }
        }
        else
        {
            RoleId = role.Value;
        }
        if (!RoleOptions.TryAdd(RoleId, this))
            Logger.Info(RoleId.ToString() + "を追加できんかったー：" + name);
        this.isHidden = isHidden;
        if (max > 1) countOption = CustomOption.Create(id + 10000, isSHROn, type, "roleNumAssigned", 1f, 1f, 15f, 1f, this, format: "unitPlayers");
    }
}

public class CustomOptionBlank : CustomOption
{
    public CustomOptionBlank(CustomOption parent, bool isSHROn = false) : base()
    {
        this.parent = parent;
        this.id = -1;
        this.name = "";
        this.isHeader = false;
        this.isHidden = true;
        this.children = new List<CustomOption>();
        this.isSHROn = isSHROn;
        this.CanShowByFunc = false;
        this.selections = new string[] { "" };
        this.RoleId = RoleId.DefaultRole;
        options.Add(this);
    }

    public override int GetSelection()
    {
        return 0;
    }

    public override bool GetBool()
    {
        return true;
    }

    public override float GetFloat()
    {
        return 0f;
    }

    public override string GetString()
    {
        return "";
    }

    public override void SetSelection(int set)
    {
        return;
    }

    public override void SelectionAddition(int addition)
    {
        return;
    }

    public override void ShareOption()
    {
        return;
    }
}

[HarmonyPatch(typeof(RoleOptionsData), nameof(RoleOptionsData.GetNumPerGame))]
class RoleOptionsDataGetNumPerGamePatch
{
    public static void Postfix(ref int __result, ref RoleTypes role)
    {
        if (role is RoleTypes.Crewmate or RoleTypes.Impostor) return;

        if (Mode.ModeHandler.IsBlockVanillaRole()) __result = 0;

        if (role != RoleTypes.GuardianAngel) return;

        if (Mode.ModeHandler.IsBlockGuardianAngelRole()) __result = 0;

    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
public class AmongUsClientOnPlayerJoinedPatch
{
    public static void Postfix(PlayerControl __instance)
    {
        CustomOption.ShareOptionSelections();
    }
}

static class GameOptionsMenuUpdatePatch
{
    /// <summary>現在, 封印処理のある設定を有しているか ( 此処をtrueにする事で封印処理が実行される )</summary>
    public const bool HasSealingOption = false;

    public static bool IsHidden(this CustomOption option, ModeId currentModeId)
    {
        return option.isHidden
            || (!option.isSHROn && currentModeId == ModeId.SuperHostRoles) // SHRモード時, SHR未対応の設定を隠す処理。
            || (HasSealingOption && IsSealingDatetimeControl(option)) // 解放条件が時間に依存する設定の 封印及び開放処理
            || (ModeHandler.EnableModeSealing && (option == ModeHandler.ModeSetting || option == ModeHandler.ThisModeSetting)); // モード設定封印処理
    }

    /// <summary>オプションが日時条件によって封印されているかを判定する。</summary>
    /// <param name="option">判定するオプション</param>
    /// <returns>true : 封印されている / false : 封印されていない</returns>
    private static bool IsSealingDatetimeControl(CustomOption option)
    {
        if (option.RoleId is RoleId.DefaultRole or RoleId.None) return false; // 役職以外はスキップ
        if (RoleInfoManager.GetRoleInfo(option.RoleId) == null) return false; // GetOptionInfoでlogを出さない様 RoleBase未移行役は先にスキップする。

        bool isHidden = false;

        OptionInfo optionInfo = OptionInfo.GetOptionInfo(option.RoleId);
        if (optionInfo != null) { isHidden = optionInfo.IsHidden; }

        return isHidden;
    }
}

class GameOptionsDataPatch
{
    public static string Tl(string key)
    {
        return ModTranslation.GetString(key);
    }

    public static string OptionToString(CustomOption option)
    {
        if (option == null) return "";

        string text = option.GetName() + ":" + option.GetString();
        var (isProcessingRequired, pattern) = ProcessingOptionCheck(option);

        if (isProcessingRequired)
            text += $"{ProcessingOptionString(option, "  ", pattern)}";

        return text;
    }

    /// <summary>
    /// CustomOptionに追記をする。
    /// </summary>
    /// <param name="option">追記が必要なオプション</param>
    /// <param name="indent">追記が必要なオプションのインデント</param>
    /// <param name="pattern">追記の内容指定</param>
    /// <returns>string : 追記した文字列(インデントは一つ追加している)</returns>
    internal static string ProcessingOptionString(CustomOption option, string indent = "", ProcessingPattern pattern = ProcessingPattern.None)
    {
        if (option == null) return "";
        string text = "";

        if (pattern == ProcessingPattern.GetTaskTriggerAbilityTaskNumber) // タスクの割合から, タスク数を求める
        {
            int AllTask = SelectTask.GetTotalTasks(option.RoleId);
            if (!int.TryParse(option.GetString().Replace("%", ""), out int percent)) return ""; // int変換できない物の場合, ブランクを返す
            float rate = percent / 100f;
            int activeTaskNum = (int)(AllTask * rate);

            text += "\n" + indent + "  " + "(" + $"{ModTranslation.GetString("TaskTriggerAbilityTaskNumber")}:";

            if (AllTask != 0)
                text += $"{AllTask} × {option.GetString()} => {activeTaskNum}{ModTranslation.GetString("UnitPieces")}" + ")";
            else
            {
                string errorText = $"{option.RoleId} のタスク数が取得できず、能力発動に必要なタスク数を計算する事ができませんでした。" + ")";
                text += $"=> {errorText}";
                Logger.Error($"{errorText}", "GetTaskTriggerAbilityTaskNumber");
            }
        }

        return text;
    }

    /// <summary>
    /// 追記対象のオプションか判定する
    /// </summary>
    /// <param name="option">判定対象</param>
    /// <returns> true : 対象 _ false: 対象外 / ProcessingPattern : 追記形式 </returns>
    internal static (bool, ProcessingPattern) ProcessingOptionCheck(CustomOption option)
    {
        string optionName = option.GetName();

        Dictionary<string, ProcessingPattern> targetString = new()
        {
            {ModTranslation.GetString("ParcentageForTaskTriggerSetting"), ProcessingPattern.GetTaskTriggerAbilityTaskNumber},
            {ModTranslation.GetString("SafecrackerKillGuardTaskSetting"), ProcessingPattern.GetTaskTriggerAbilityTaskNumber},
            {ModTranslation.GetString("SafecrackerUseVentTaskSetting"), ProcessingPattern.GetTaskTriggerAbilityTaskNumber},
            {ModTranslation.GetString("SafecrackerExiledGuardTaskSetting"), ProcessingPattern.GetTaskTriggerAbilityTaskNumber},
            {ModTranslation.GetString("SafecrackerUseSaboTaskSetting"), ProcessingPattern.GetTaskTriggerAbilityTaskNumber},
            {ModTranslation.GetString("SafecrackerIsImpostorLightTaskSetting"), ProcessingPattern.GetTaskTriggerAbilityTaskNumber},
            {ModTranslation.GetString("SafecrackerCheckImpostorTaskSetting"), ProcessingPattern.GetTaskTriggerAbilityTaskNumber},
        };

        if (targetString.ContainsKey(optionName)) return (true, targetString[optionName]);
        else return (false, ProcessingPattern.None);
    }

    /// <summary>
    /// 追記が必要なCustomOptionの種類
    /// </summary>
    internal enum ProcessingPattern
    {
        None,
        GetTaskTriggerAbilityTaskNumber,
    }
    private static List<string> ResultPages = null;
    public static void UpdateData()
    {
        bool hideSettings = AmongUsClient.Instance?.AmHost == false && CustomOptionHolder.hideSettings.GetBool();
        if (hideSettings) {
            ResultPages = new()
            {
                GameOptionsManager.Instance.CurrentGameOptions.ToHudString(PlayerControl.AllPlayerControls.Count)
            };
        }

        List<string> pages = new();

        StringBuilder entry = new();
        List<string> entries = new()
            {
                // First add the presets and the role counts
                OptionToString(CustomOptionHolder.presetSelection)
            };

        var optionName = CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), Tl("SettingCrewmateRoles"));
        var min = CustomOptionHolder.crewmateRolesCountMax.GetSelection();
        var max = CustomOptionHolder.crewmateRolesCountMax.GetSelection();
        if (min > max) min = max;
        var optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), Tl("SettingCrewmateGhostRoles"));
        min = CustomOptionHolder.crewmateGhostRolesCountMax.GetSelection();
        max = CustomOptionHolder.crewmateGhostRolesCountMax.GetSelection();
        if (min > max) min = max;
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), Tl("SettingNeutralRoles"));
        min = CustomOptionHolder.neutralRolesCountMax.GetSelection();
        max = CustomOptionHolder.neutralRolesCountMax.GetSelection();
        if (min > max) min = max;
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), Tl("SettingNeutralGhostRoles"));
        min = CustomOptionHolder.neutralGhostRolesCountMax.GetSelection();
        max = CustomOptionHolder.neutralGhostRolesCountMax.GetSelection();
        if (min > max) min = max;
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), Tl("SettingImpostorRoles"));
        min = CustomOptionHolder.impostorRolesCountMax.GetSelection();
        max = CustomOptionHolder.impostorRolesCountMax.GetSelection();
        if (min > max) min = max;
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        optionName = CustomOptionHolder.Cs(new Color(204f / 255f, 204f / 255f, 0, 1f), Tl("SettingImpostorGhostRoles"));
        min = CustomOptionHolder.impostorGhostRolesCountMax.GetSelection();
        max = CustomOptionHolder.impostorGhostRolesCountMax.GetSelection();
        if (min > max) min = max;
        optionValue = (min == max) ? $"{max}" : $"{min} - {max}";
        entry.AppendLine($"{optionName}: {optionValue}");

        entries.Add(entry.ToString().Trim('\r', '\n'));

        static void addChildren(CustomOption option, ref StringBuilder entry, ModeId modeId, bool indent = true)
        {
            if ((!option.Enabled) ||
                (modeId == ModeId.SuperHostRoles && !option.isSHROn)
            ) return;

            int openSelection = option.GetSelection();

            foreach (var child in option.children)
            {
                if (child.openSelection != -1 && child.openSelection != openSelection)
                    continue;
                if (child.HasCanShowAction && !child.CanShowByFunc)
                    continue;
                if (GameOptionsMenuUpdatePatch.IsHidden(option, modeId))
                    continue;
                entry.AppendLine((indent ? "    " : "") + OptionToString(child));
                addChildren(child, ref entry, modeId, indent);
            }
        }

        foreach (CustomOption option in CustomOption.options)
        {
            if ((option == CustomOptionHolder.presetSelection) ||
                (option == CustomOptionHolder.crewmateRolesCountMax) ||
                (option == CustomOptionHolder.crewmateGhostRolesCountMax) ||
                (option == CustomOptionHolder.neutralRolesCountMax) ||
                (option == CustomOptionHolder.neutralGhostRolesCountMax) ||
                (option == CustomOptionHolder.impostorRolesCountMax) ||
                (option == CustomOptionHolder.impostorGhostRolesCountMax) ||
                (option == CustomOptionHolder.hideSettings))
            {
                continue;
            }

            ModeId modeId = ModeHandler.GetMode(false);
            if (option.parent == null)
            {
                if ((!option.Enabled) || (modeId == ModeId.SuperHostRoles && !option.isSHROn))
                {
                    continue;
                }

                entry = new StringBuilder();
                if (!(GameOptionsMenuUpdatePatch.IsHidden(option, modeId) || option.type == CustomOptionType.MatchTag))
                {
                    entry.AppendLine(OptionToString(option));
                }
                addChildren(option, ref entry, modeId, !GameOptionsMenuUpdatePatch.IsHidden(option, modeId));
                string line = entry.ToString().Trim('\r', '\n');
                if (line is not "\r" and not "")
                {
                    entries.Add(line);
                }
            }
        }
        int maxLines = 28;
        int lineCount = 0;
        StringBuilder page = new();
        foreach (var e in entries)
        {
            int lines = e.Count(c => c == '\n') + 1;

            if (lineCount + lines > maxLines)
            {
                pages.Add(page.ToString());
                page.Clear();
                lineCount = 0;
            }

            page.Append(e).Append("\n\n");
            lineCount += lines + 1;
        }

        string lastpage = page.ToString().Trim('\r', '\n');
        if (lastpage != String.Empty)
        {
            pages.Add(lastpage);
        }

        ResultPages = new();
        int numPages = pages.Count + 1;
        ResultPages.Add($"{GameOptionsManager.Instance.CurrentGameOptions.ToHudString(PlayerControl.AllPlayerControls.Count).Trim('\r', '\n')}\n\n{Tl("SettingPressTabForMore")} (1/{numPages})");
        for (int i = 1; i < numPages; i++)
        {
            ResultPages.Add($"{pages[i - 1].Trim('\r', '\n')}\n\n{Tl("SettingPressTabForMore")} ({i + 1}/{numPages})");
        }

        SuperNewRolesPlugin.optionsMaxPage = numPages - 1;
        SuperNewRolesPlugin.optionsPage %= numPages;

        if (CustomOverlays.overlayShown && CustomOverlays.nowPattern == CustomOverlays.CustomOverlayPattern.Regulation) CustomOverlays.YoggleInfoOverlay(CustomOverlays.nowPattern, true);
    }
    public static string getHudString(int pagenum)
    {
        if (ResultPages == null) UpdateData();
        if (pagenum >= ResultPages.Count) return String.Empty;
        return ResultPages[pagenum];
    }
}

[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToHudString))]
public static class IGameOptionsExtensionsToHudStringPatch
{
    public static void Prefix(ref int numPlayers)
    {
        if (numPlayers > 15) numPlayers = 15;
    }
}

[HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
public static class GameOptionsGetAdjustedNumImpostorsPatch
{
    public static bool Prefix(ref int __result)
    {
        __result = GameManager.Instance.LogicOptions.currentGameOptions.NumImpostors;
        return false;
    }
}

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class GameOptionsNextPagePatch
{
    public static void Postfix(KeyboardJoystick __instance)
    {
        if (Input.GetKeyDown(KeyCode.Tab) || ConsoleJoystick.player.GetButtonDown(7))
        {
            // Regulationのoverlayを表示している時, 2ページ単位でページを送る
            if (CustomOverlays.nowPattern == CustomOverlays.CustomOverlayPattern.Regulation) SuperNewRolesPlugin.optionsPage += 2;

            // ページが最大ページを超えたら, ページを0に戻す
            if (SuperNewRolesPlugin.optionsPage > SuperNewRolesPlugin.optionsMaxPage)
                SuperNewRolesPlugin.optionsPage = 0;
        }
    }
}

[HarmonyPatch(typeof(RolesSettingsMenu))]
public static class RolesSettingsMenuPatch
{
    [HarmonyPatch(nameof(RolesSettingsMenu.Awake)), HarmonyPostfix]
    public static void SetAwakePostfix(RolesSettingsMenu __instance)
    {
        __instance.QuotaTabSelectables = new();
        __instance.advancedSettingChildren = new();
        __instance.roleChances = new();
    }
}

[HarmonyPatch(typeof(GameSettingMenu))]
public static class GameSettingMenuPatch
{
    public static ModSettingsMenu ModSettingsMenu;
    public static PassiveButton ModSettingsButton;

    [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPrefix]
    public static void StartPrefix(GameSettingMenu __instance) => __instance.GameSettingsTab.HideForOnline = new Transform[] { };

    [HarmonyPatch(nameof(GameSettingMenu.Start)), HarmonyPostfix]
    public static void Postfix(GameSettingMenu __instance)
    {
        __instance.MenuDescriptionText.transform.parent.gameObject.SetActive(false);
        __instance.GamePresetsButton.transform.position += new Vector3(0, 0.637f);
        __instance.GameSettingsButton.transform.position += new Vector3(0, 0.637f);
        __instance.RoleSettingsButton.transform.position += new Vector3(0, 0.637f);
        __instance.GameSettingsTab.scrollBar.ContentYBounds.max += 0.5f;

        ModSettingsMenu = new GameObject("MOD TAB").AddComponent<ModSettingsMenu>();
        ModSettingsMenu.transform.SetParent(__instance.RoleSettingsTab.transform.parent);
        ModSettingsMenu.transform.localPosition = new(0f, 0.16f, -4f);
        ModSettingsMenu.gameObject.layer = 5;
        ModSettingsMenu.gameObject.SetActive(false);

        GameObject mod_settings_button = Object.Instantiate(__instance.RoleSettingsButton.gameObject, __instance.RoleSettingsButton.transform.parent);
        mod_settings_button.name = "ModSettingsButton";
        mod_settings_button.transform.position -= new Vector3(0, 0.637f);
        new LateTask(() => mod_settings_button.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>().text = ModTranslation.GetString("ModSettingsButtonText"), 0f, "GameSettingMenu");
        ModSettingsButton = mod_settings_button.GetComponent<PassiveButton>();
        (ModSettingsButton.OnClick = new()).AddListener(() => { __instance.ChangeTab(3, previewOnly: false); });
        (ModSettingsButton.OnMouseOver = new()).AddListener(() => { __instance.ChangeTab(3, previewOnly: true); });

        //Start後、ModSettingsTabの表示前にRoleSettingsTabを経由すると表示バグが起こるので、あらかじめModSettingsTabを一度開くことで回避する
        __instance.ChangeTab(3, previewOnly: false);
        new LateTask(() => __instance.ChangeTab(1, previewOnly: false), 0f, "ChangeTab");
    }

    [HarmonyPatch(nameof(GameSettingMenu.Close)), HarmonyPostfix]
    public static void ClosePostfix()
    {
        GameOptionsDataPatch.UpdateData();
        if (CustomOption.IsValuesUpdated)
        {
            OptionSaver.WriteNowOptions();
            CustomOption.IsValuesUpdated = false;
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.ChangeTab)), HarmonyPrefix]
    public static bool ChangeTabPrefix(GameSettingMenu __instance, int tabNum, bool previewOnly)
    {
        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            __instance.PresetsTab.gameObject.SetActive(false);
            __instance.GameSettingsTab.gameObject.SetActive(false);
            __instance.RoleSettingsTab.gameObject.SetActive(false);
            ModSettingsMenu?.gameObject.SetActive(false);
            __instance.GamePresetsButton.SelectButton(false);
            __instance.GameSettingsButton.SelectButton(false);
            __instance.RoleSettingsButton.SelectButton(false);
            ModSettingsButton?.SelectButton(false);

            switch (tabNum)
            {
                case 0:
                    __instance.PresetsTab.gameObject.SetActive(true);
                    //__instance.MenuDescriptionText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GamePresetsDescription, Array.Empty<Il2CppSystem.Object>());
                    break;
                case 1:
                    __instance.GameSettingsTab.gameObject.SetActive(true);
                    //__instance.MenuDescriptionText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GameSettingsDescription, Array.Empty<Il2CppSystem.Object>());
                    __instance.GameSettingsTab.Children.Find(x => x.Title == StringNames.GameNumImpostors).Cast<NumberOption>().ValidRange = new(0f, 15f);
                    __instance.GameSettingsTab.Children.Find(x => x.Title == StringNames.GameKillCooldown).Cast<NumberOption>().ValidRange = new(2.5f, 60f);
                    __instance.GameSettingsTab.Children.Find(x => x.Title == StringNames.GamePlayerSpeed).Cast<NumberOption>().ValidRange = new(-5f, 5f);
                    __instance.GameSettingsTab.Children.Find(x => x.Title == StringNames.GameCommonTasks).Cast<NumberOption>().ValidRange = new(0f, 12f);
                    __instance.GameSettingsTab.Children.Find(x => x.Title == StringNames.GameLongTasks).Cast<NumberOption>().ValidRange = new(0f, 69f);
                    __instance.GameSettingsTab.Children.Find(x => x.Title == StringNames.GameShortTasks).Cast<NumberOption>().ValidRange = new(0f, 45f);
                    break;
                case 2:
                    __instance.RoleSettingsTab.gameObject.SetActive(true);
                    //__instance.MenuDescriptionText.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.RoleSettingsDescription, Array.Empty<Il2CppSystem.Object>());
                    break;
                case 3:
                    ModSettingsMenu.gameObject.SetActive(true);
                    break;
            }
        }
        if (previewOnly)
        {
            __instance.ToggleLeftSideDarkener(false);
            __instance.ToggleRightSideDarkener(true);
        }
        else
        {
            __instance.ToggleLeftSideDarkener(true);
            __instance.ToggleRightSideDarkener(false);
            switch (tabNum)
            {
                case 0:
                    __instance.GamePresetsButton.SelectButton(true);
                    __instance.PresetsTab.OpenMenu();
                    break;
                case 1:
                    __instance.GameSettingsButton.SelectButton(true);
                    __instance.GameSettingsTab.OpenMenu();
                    break;
                case 2:
                    __instance.RoleSettingsButton.SelectButton(true);
                    __instance.RoleSettingsTab.OpenMenu();
                    break;
                case 3:
                    ModSettingsButton.SelectButton(true);
                    ModSettingsMenu.OpenMenu();
                    break;
            }
        }
        return false;
    }
}