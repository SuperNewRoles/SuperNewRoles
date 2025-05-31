using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.CustomOptions.Categories;
using SuperNewRoles.Roles;
using UnityEngine;
namespace SuperNewRoles.Modules;

public static class CustomOptionManager
{


    // [CustomOptionBool("DebugMode", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool DebugMode = true;
    [CustomOptionBool("DebugModeNoGameEnd", false)]
    public static bool DebugModeNoGameEnd;
    [CustomOptionBool("SkipStartGameCountdown", false, parentFieldName: nameof(Categories.GeneralSettings))]
    public static bool SkipStartGameCountdown;

    private static Dictionary<string, CustomOptionBaseAttribute> CustomOptionAttributes { get; } = new();
    public static List<CustomOption> CustomOptions { get; } = new();
    public static List<CustomOptionCategory> OptionCategories { get; } = new();
    public static Dictionary<string, CustomOptionCategory> CategoryByFieldName { get; } = new();
    public static IReadOnlyList<CustomOption> GetCustomOptions() => CustomOptions.AsReadOnly();
    public static IReadOnlyList<CustomOptionCategory> GetOptionCategories() => OptionCategories.AsReadOnly();

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public static class AmongUsClientOnPlayerJoinedPatch
    {
        private static LateTask _lateTask;
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            // ゲーム終了後の復帰からの同期で大量に通信を送るのを防ぐために
            // 2秒内に来たプレイヤーは同時に同期する
            if (_lateTask != null)
                _lateTask.UpdateDelay(1.5f);
            else
                _lateTask = new LateTask(() =>
                {
                    RpcSyncOptionsAll();
                    RoleOptionManager.RpcSyncRoleOptionsAll();
                    _lateTask = null;
                }, 2f, "CustomOptionManager.RpcSyncOptionsAll");
        }
    }
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
    public static class AmongUsClientStartGamePatch
    {
        public static void Postfix()
        {
            // ゲーム開始時に一度だけ同期する(AmongUsClient.StartGameが呼ばれるのはホストのみ)
            if (!AmongUsClient.Instance.AmHost) return;
            RpcSyncOptionsAll();
            RoleOptionManager.RpcSyncRoleOptionsAll();
        }
    }
    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.GetAdjustedNumImpostors))]
    public static class FixAdjustedNumImpostors
    {
        public static bool Prefix(ref int __result)
        {
            __result = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
            return false;
        }

    }
    [CustomRPC]
    public static void RpcSyncOption(string optionId, byte selection)
    {
        var option = CustomOptions.FirstOrDefault(o => o.Id == optionId);
        if (option == null)
        {
            Logger.Warning($"オプションが見つかりません: {optionId}");
            return;
        }
        option.UpdateSelection(selection);
    }
    [CustomRPC]
    public static void _RpcSyncOptionsAll(Dictionary<ushort, byte> options, bool resetToDefault)
    {
        Logger.Info("RpcSyncOptionsAll");
        if (resetToDefault)
        {
            foreach (var option in CustomOptions)
            {
                option.UpdateSelection(option.DefaultSelection);
            }
        }
        foreach (var option in CustomOptions)
        {
            if (options.TryGetValue(option.IndexId, out var selection))
                option.UpdateSelection(selection);
        }
    }
    public static void RpcSyncOptionsAll()
    {
        var options = CustomOptions.Where(o => !o.IsDefaultValue).ToDictionary(o => o.IndexId, o => o.Selection);
        int keysMax = options.Keys.Count > 0 ? options.Keys.Max() : 0;
        for (int i = 0; i <= keysMax; i += 30)
        {
            var partialOptions = options
                .Where(kvp => kvp.Key >= i && kvp.Key < i + 30)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            _RpcSyncOptionsAll(partialOptions, i == 0);
        }
    }
    // カスタムオプションをロードするメソッド
    public static void Load()
    {
        LoadCustomOptions();
        LinkParentOptions();
        SortAndAssignIntIds();
        RoleOptionManager.RoleOptionLoad();
        CustomOptionSaver.SaverLoad();
    }

    // 各フィールドからカスタムオプションを走査・生成してリストに追加する処理
    private static void LoadCustomOptions()
    {
        var fieldNames = new HashSet<string>();
        SuperNewRolesPlugin.Logger.LogInfo("[Splash] Loading CustomOptions...");
        foreach (var type in SuperNewRolesPlugin.Assembly.GetTypes())
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                // カテゴリーフィールドの場合、staticフィールドのみを対象にする
                if (field.FieldType == typeof(CustomOptionCategory))
                {
                    if (!fieldNames.Add(field.Name))
                        throw new InvalidOperationException($"Category field name is duplicated: {field.Name}");
                    var assignFilter = field.GetCustomAttribute<AssignFilterAttribute>();
                    var modifierAttribute = field.GetCustomAttribute<ModifierAttribute>();
                    var category = new CustomOptionCategory(
                        field.Name,
                        isModifier: modifierAttribute != null,
                        modifierRoleId: modifierAttribute?.ModifierRoleId ?? ModifierRoleId.None,
                        hasModifierAssignFilter: assignFilter != null,
                        modifierAssignFilterTeam: assignFilter?.AssignedTeamTypes,
                        modifierDoNotAssignRoles: assignFilter?.HiddenRoleIds);
                    field.SetValue(null, category);
                    CategoryByFieldName[field.Name] = category;
                    continue;
                }

                var attribute = field.GetCustomAttribute<CustomOptionBaseAttribute>();
                if (attribute == null)
                {
                    continue;
                }
                if (attribute is CustomOptionTaskAttribute taskAttribute)
                {
                    taskAttribute.SetupAttributes(field, ref fieldNames, CustomOptions, CustomOptionAttributes);
                    continue;
                }
                if (!fieldNames.Add(field.Name))
                {
                    throw new InvalidOperationException($"Field name is duplicated: {field.Name}");
                }

                // カスタムオプション属性を辞書に追加
                CustomOptionAttributes[field.Name] = attribute;
                // フィールド情報を設定
                attribute.SetFieldInfo(field);
                RoleId? parentRole = null;
                GhostRoleId? parentGhostRole = null;
                ModifierRoleId? parentModifierRole = null;
                if (field.DeclaringType.GetInterfaces().Contains(typeof(IRoleBase)))
                {
                    var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(field.DeclaringType);
                    var instanceProperty = baseSingletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var roleInstance = instanceProperty.GetValue(null);
                    parentRole = ((IRoleBase)roleInstance).Role;
                }
                if (field.DeclaringType.GetInterfaces().Contains(typeof(IGhostRoleBase)))
                {
                    var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(field.DeclaringType);
                    var instanceProperty = baseSingletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var roleInstance = instanceProperty.GetValue(null);
                    parentGhostRole = ((IGhostRoleBase)roleInstance).Role;
                }
                if (field.DeclaringType.GetInterfaces().Contains(typeof(IModifierBase)))
                {
                    var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(field.DeclaringType);
                    var instanceProperty = baseSingletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var roleInstance = instanceProperty.GetValue(null);
                    parentModifierRole = ((IModifierBase)roleInstance).ModifierRole;
                }
                CustomOption option = new(attribute: attribute, fieldInfo: field, parentRole: parentRole, parentGhostRole: parentGhostRole, parentModifierRole: parentModifierRole);
                CustomOptions.Add(option);
            }
        }
        SuperNewRolesPlugin.Logger.LogInfo($"[Splash] {CustomOptions.Count} CustomOptions loaded");
    }

    // 属性で指定された親フィールド名があるオプションについて、親オプションと紐付ける処理
    private static void LinkParentOptions()
    {
        SuperNewRolesPlugin.Logger.LogInfo("[Splash] Linking parent options...");
        // 各カスタムオプションをフィールド名をキーとするディクショナリに変換してキャッシュ
        var taskOptionNames = new[]
        {
            nameof(CustomOptionTaskAttribute.CommonValue),
            nameof(CustomOptionTaskAttribute.LongValue),
            nameof(CustomOptionTaskAttribute.ShortValue)
        };

        var optionsByFieldName = CustomOptions
            .Where(o => !taskOptionNames.Contains(o.FieldInfo.Name))
            .ToDictionary(o => o.FieldInfo.Name);

        foreach (var option in CustomOptions)
        {
            if (!string.IsNullOrEmpty(option.Attribute.ParentFieldName))
            {
                if (CategoryByFieldName.TryGetValue(option.Attribute.ParentFieldName, out var category))
                {
                    category.AddOption(option);
                    Logger.Info($"オプションをカテゴリーに追加: {option.Name} -> {category.Name}");
                }
                else if (optionsByFieldName.TryGetValue(option.Attribute.ParentFieldName, out var parentOption))
                {
                    option.SetParentOption(parentOption);
                    Logger.Info($"親オプションを設定: {option.Name} -> {parentOption.Name}");
                }
                else
                {
                    Logger.Warning($"親オプションまたはカテゴリーが見つかりませんでした: {option.Attribute.ParentFieldName}");
                }
            }
        }
        SuperNewRolesPlugin.Logger.LogInfo("[Splash] Parent options linked");
    }

    internal static void RegisterOptionCategory(CustomOptionCategory category)
    {
        OptionCategories.Add(category);
    }

    public static CustomOption? GetCustomOptionByFieldName(string fieldName)
    {
        return CustomOptions.FirstOrDefault(option => option.FieldInfo.Name == fieldName);
    }

    public static CustomOption? GetCustomOption<T>(System.Linq.Expressions.Expression<Func<T>> expression)
    {
        if (expression.Body is System.Linq.Expressions.MemberExpression memberExpression)
        {
            return GetCustomOptionByFieldName(memberExpression.Member.Name);
        }
        return null;
    }

    private static void SortAndAssignIntIds()
    {
        var sortedOptions = CustomOptions.OrderBy(x => x.Id).ToList();
        for (int i = 0; i < sortedOptions.Count; i++)
        {
            sortedOptions[i].IndexId = (ushort)i;
        }
    }
}

public class CustomOption
{
    public CustomOptionBaseAttribute Attribute { get; }
    public FieldInfo FieldInfo { get; }
    public string Name { get; }
    protected object _value_My;
    protected byte _selection_My;
    protected object _value_Host;
    protected byte _selection_Host;
    private readonly object _defaultValue;
    private readonly byte _defaultSelection;

    public object Value => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _value_Host : _value_My;
    public byte Selection => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _selection_Host : _selection_My;
    public byte MySelection => _selection_My;
    public string Id => Attribute.Id;
    public ushort IndexId { get; internal set; }
    public object[] Selections { get; }
    public RoleId? ParentRole { get; private set; }
    public GhostRoleId? ParentGhostRole { get; private set; }
    public ModifierRoleId? ParentModifierRole { get; private set; }
    public CustomOption? ParentOption { get; private set; }
    public List<CustomOption> ChildrenOption { get; } = new();
    public DisplayModeId DisplayMode { get; private set; } = DisplayModeId.All;
    public bool IsDefaultValue => Selection == _defaultSelection;
    public object DefaultValue => _defaultValue;
    public byte DefaultSelection => _defaultSelection;
    public bool IsTaskOption { get; }
    /// <summary>
    /// このオプションがブール値（true/false）のオプションかどうかを示します。
    /// CustomOptionBoolAttributeが設定されている場合にtrueを返します。
    /// </summary>
    public bool IsBooleanOption { get; }

    public string GetCurrentSelectionString()
    {
        if (Attribute is CustomOptionFloatAttribute or CustomOptionIntAttribute or CustomOptionByteAttribute or CustomOptionSelectAttribute)
        {
            return UIHelper.FormatOptionValue(Value, this);
        }
        else if (Attribute is CustomOptionBoolAttribute boolAttr)
        {
            return ModTranslation.GetString(Selection == 0 ? "CustomOptionFalse" : "CustomOptionTrue");
        }
        return Selections[Selection].ToString();
    }

    public CustomOption(CustomOptionBaseAttribute attribute, FieldInfo fieldInfo, RoleId? parentRole = null, GhostRoleId? parentGhostRole = null, ModifierRoleId? parentModifierRole = null, bool isTaskOption = false)
    {
        try
        {
            Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            FieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));

            Selections = attribute.GenerateSelections();
            var defaultSelection = attribute.GenerateDefaultSelection();
            Name = attribute.TranslationName;
            ParentRole = parentRole;
            ParentGhostRole = parentGhostRole;
            ParentModifierRole = parentModifierRole;
            IsBooleanOption = attribute is CustomOptionBoolAttribute;
            DisplayMode = attribute.DisplayMode;
            _defaultValue = Selections[defaultSelection];
            _defaultSelection = defaultSelection;
            IsTaskOption = isTaskOption;
            UpdateSelection(defaultSelection);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to create CustomOption: {attribute.TranslationName}");
            throw;
        }
    }

    public override string ToString()
    {
        return $"{Name} ({Id})";
    }

    public virtual void UpdateSelection(byte value)
    {
        if (value >= Selections.Length)
        {
            Logger.Warning($"Invalid selection value {value} for option {Id}. Using default.");
            value = 0;
        }

        try
        {
            // isHostであれば保存されなくなる
            bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
            if (isHost)
            {
                _selection_Host = value;
                _value_Host = Selections[value];
            }
            else
            {
                _selection_My = value;
                _value_My = Selections[value];
            }
            if (!IsTaskOption)
                FieldInfo.SetValue(null, Value);

            // 値が変更されたときにイベントを発火
            if (Attribute is CustomOptionNumericAttribute<int> intAttr)
            {
                intAttr.OnValueChanged((int)Value);
            }
            else if (Attribute is CustomOptionNumericAttribute<float> floatAttr)
            {
                floatAttr.OnValueChanged((float)Value);
            }
            else if (Attribute is CustomOptionNumericAttribute<byte> byteAttr)
            {
                byteAttr.OnValueChanged((byte)Value);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to update option {Id}: {ex.Message}");
            throw;
        }
    }

    public void SetParentOption(CustomOption parent)
    {
        ParentOption = parent;
        parent.ChildrenOption.Add(this);
    }

    public bool ShouldDisplay()
    {
        // 親オプションがある場合、親オプションの値をチェック
        if (ParentOption != null)
        {
            // ParentActiveValueが設定されている場合、親の値がそれと一致するかチェック
            if (Attribute.ParentActiveValue != null)
            {
                // 親の値が指定された値と一致しない場合は表示しない
                if (!ParentOption.Value.Equals(Attribute.ParentActiveValue))
                {
                    return false;
                }
            }
            else if (ParentOption.Selection == 0)
            {
                return false;
            }

            // 親オプションをチェック
            var parent = ParentOption;
            while (parent != null)
            {
                // 親オプションが無効（Selectionが0）ならfalse
                if (!parent.ShouldDisplay())
                    return false;
                parent = parent.ParentOption;
            }
        }

        return Modules.DisplayMode.HasMode(
            Modules.DisplayMode.GetCurrentMode(),
            this.DisplayMode);
    }

    public void SetDisplayMode(DisplayModeId mode)
    {
        DisplayMode = mode;
    }
}

public static class RoleOptionManager
{
    // 遅延同期用のディクショナリとLateTaskを追加
    public static readonly Dictionary<RoleId, LateTask> DelayedSyncTasks = new();
    public static readonly Dictionary<ModifierRoleId, LateTask> DelayedSyncTasksModifier = new();
    public static readonly Dictionary<GhostRoleId, LateTask> DelayedSyncTasksGhost = new(); // GhostRole用 RoleId -> GhostRoleId
    private static readonly float SyncDelay = 0.5f; // 0.5秒の遅延

    public class RoleOption
    {
        public RoleId RoleId { get; }
        public AssignedTeamType AssignTeam { get; }
        private byte _numberOfCrews_My;
        private byte _numberOfCrews_Host;
        private int _percentage_My;
        private int _percentage_Host;
        public byte NumberOfCrews
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _numberOfCrews_Host : _numberOfCrews_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _numberOfCrews_Host = value;
                }
                else
                {
                    _numberOfCrews_My = value;
                }
            }
        }
        public int Percentage
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _percentage_Host : _percentage_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _percentage_Host = value;
                }
                else
                {
                    _percentage_My = value;
                    // ホストの場合は、変更を他のプレイヤーに同期
                }
            }
        }
        public CustomOption[] Options { get; }
        public Color32 RoleColor { get; }
        public RoleOption(RoleId roleId, byte numberOfCrews, int percentage, CustomOption[] options)
        {
            RoleId = roleId;
            _numberOfCrews_My = numberOfCrews;
            _percentage_My = percentage;
            Options = options;
            // ロールの色情報を取得
            var roleBase = CustomRoleManager.AllRoles.FirstOrDefault(r => r.Role == roleId);
            if (roleBase == null)
                throw new Exception($"Role {roleId} not found");
            RoleColor = roleBase?.RoleColor ?? new Color32(255, 255, 255, 255);
            AssignTeam = roleBase?.AssignedTeam ?? AssignedTeamType.Crewmate;
        }

        public void UpdateValues(byte numberOfCrews, int percentage)
        {
            bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
            if (isHost)
            {
                _numberOfCrews_Host = numberOfCrews;
                _percentage_Host = percentage;
            }
            else
            {
                _numberOfCrews_My = numberOfCrews;
                _percentage_My = percentage;
            }
        }
    }
    public class ModifierRoleOption
    {
        public ModifierRoleId ModifierRoleId { get; }
        private byte _numberOfCrews_My;
        private byte _numberOfCrews_Host;
        private int _percentage_My;
        private int _percentage_Host;
        public byte NumberOfCrews
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _numberOfCrews_Host : _numberOfCrews_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _numberOfCrews_Host = value;
                }
                else
                {
                    _numberOfCrews_My = value;
                }
            }
        }
        public int Percentage
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _percentage_Host : _percentage_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _percentage_Host = value;
                }
                else
                {
                    _percentage_My = value;
                    // ホストの場合は、変更を他のプレイヤーに同期
                }
            }
        }
        public CustomOption[] Options { get; }
        public Color32 RoleColor { get; }
        public List<RoleId> AssignFilterList { get; set; } = new();
        private int _maxImpostors_My;
        private int _maxImpostors_Host;
        public int MaxImpostors
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _maxImpostors_Host : _maxImpostors_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _maxImpostors_Host = value;
                }
                else
                {
                    _maxImpostors_My = value;
                }
            }
        }
        private int _impostorChance_My;
        private int _impostorChance_Host;
        public int ImpostorChance
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _impostorChance_Host : _impostorChance_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _impostorChance_Host = value;
                }
                else
                {
                    _impostorChance_My = value;
                }
            }
        }
        private int _maxNeutrals_My;
        private int _maxNeutrals_Host;
        public int MaxNeutrals
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _maxNeutrals_Host : _maxNeutrals_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _maxNeutrals_Host = value;
                }
                else
                {
                    _maxNeutrals_My = value;
                }
            }
        }
        private int _neutralChance_My;
        private int _neutralChance_Host;
        public int NeutralChance
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _neutralChance_Host : _neutralChance_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _neutralChance_Host = value;
                }
                else
                {
                    _neutralChance_My = value;
                }
            }
        }
        private int _maxCrewmates_My;
        private int _maxCrewmates_Host;
        public int MaxCrewmates
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _maxCrewmates_Host : _maxCrewmates_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _maxCrewmates_Host = value;
                }
                else
                {
                    _maxCrewmates_My = value;
                }
            }
        }
        private int _crewmateChance_My;
        private int _crewmateChance_Host;
        public int CrewmateChance
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _crewmateChance_Host : _crewmateChance_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _crewmateChance_Host = value;
                }
                else
                {
                    _crewmateChance_My = value;
                }
            }
        }
        public ModifierRoleOption(ModifierRoleId modifierRoleId, byte numberOfCrews, int percentage, CustomOption[] options)
        {
            ModifierRoleId = modifierRoleId;
            _numberOfCrews_My = numberOfCrews;
            _percentage_My = percentage;
            Options = options;
            // ロールの色情報を取得
            var roleBase = CustomRoleManager.AllModifiers.FirstOrDefault(r => r.ModifierRole == modifierRoleId);
            if (roleBase == null)
                throw new Exception($"Role {modifierRoleId} not found");
            RoleColor = roleBase?.RoleColor ?? new Color32(255, 255, 255, 255);
            // AssignFilterが有効な場合は初期値をセット
            if (roleBase != null && roleBase.AssignFilter)
            {
                AssignFilterList = new List<RoleId>();
            }
        }

        public void UpdateValues(byte numberOfCrews, int percentage, int maxImpostors, int impostorChance, int maxNeutrals, int neutralChance, int maxCrewmates, int crewmateChance)
        {
            bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
            if (isHost)
            {
                _numberOfCrews_Host = numberOfCrews;
                _percentage_Host = percentage;
                _maxImpostors_Host = maxImpostors;
                _impostorChance_Host = impostorChance;
                _maxNeutrals_Host = maxNeutrals;
                _neutralChance_Host = neutralChance;
                _maxCrewmates_Host = maxCrewmates;
                _crewmateChance_Host = crewmateChance;
            }
            else
            {
                _numberOfCrews_My = numberOfCrews;
                _percentage_My = percentage;
                _maxImpostors_My = maxImpostors;
                _impostorChance_My = impostorChance;
                _maxNeutrals_My = maxNeutrals;
                _neutralChance_My = neutralChance;
                _maxCrewmates_My = maxCrewmates;
                _crewmateChance_My = crewmateChance;
            }
        }
    }
    public class GhostRoleOption
    {
        public GhostRoleId RoleId { get; } // RoleId -> GhostRoleId
        private byte _numberOfCrews_My;
        private byte _numberOfCrews_Host;
        private int _percentage_My;
        private int _percentage_Host;
        public byte NumberOfCrews
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _numberOfCrews_Host : _numberOfCrews_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _numberOfCrews_Host = value;
                }
                else
                {
                    _numberOfCrews_My = value;
                }
            }
        }
        public int Percentage
        {
            get => (AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost) ? _percentage_Host : _percentage_My;
            set
            {
                bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
                if (isHost)
                {
                    _percentage_Host = value;
                }
                else
                {
                    _percentage_My = value;
                    // ホストの場合は、変更を他のプレイヤーに同期
                }
            }
        }
        public CustomOption[] Options { get; }
        public Color32 RoleColor { get; }
        public GhostRoleOption(GhostRoleId roleId, byte numberOfCrews, int percentage, CustomOption[] options) // RoleId -> GhostRoleId
        {
            RoleId = roleId;
            _numberOfCrews_My = numberOfCrews;
            _percentage_My = percentage;
            Options = options;
            // ロールの色情報を取得
            var roleBase = CustomRoleManager.AllGhostRoles.FirstOrDefault(r => r.Role == roleId);
            if (roleBase == null)
                throw new Exception($"GhostRole {roleId} not found");
            RoleColor = roleBase?.RoleColor ?? new Color32(255, 255, 255, 255);
        }

        public void UpdateValues(byte numberOfCrews, int percentage)
        {
            bool isHost = AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected && !AmongUsClient.Instance.AmHost;
            if (isHost)
            {
                _numberOfCrews_Host = numberOfCrews;
                _percentage_Host = percentage;
            }
            else
            {
                _numberOfCrews_My = numberOfCrews;
                _percentage_My = percentage;
            }
        }
    }

    public static RoleOption[] RoleOptions { get; private set; }
    public static ModifierRoleOption[] ModifierRoleOptions { get; private set; }
    public static GhostRoleOption[] GhostRoleOptions { get; private set; } // GhostRole用
    public static List<ExclusivityData> ExclusivitySettings { get; private set; } = new();

    // キャッシュ用のディクショナリ
    private static Dictionary<ushort, RoleOption> _roleOptionsByByte = new();
    private static Dictionary<ushort, ModifierRoleOption> _modifierRoleOptionsByByte = new();
    private static Dictionary<ushort, GhostRoleOption> _ghostRoleOptionsByByte = new();

    /// <summary>
    /// RoleIdをbyteにキャストしたものからRoleOptionを取得します
    /// </summary>
    /// <param name="roleIdByte">RoleIdをbyteにキャストした値</param>
    /// <returns>対応するRoleOption、見つからない場合はnull</returns>
    public static RoleOption? GetRoleOption(RoleId role)
    {
        return _roleOptionsByByte.TryGetValue((ushort)role, out var roleOption) ? roleOption : null;
    }

    /// <summary>
    /// ModifierRoleIdをbyteにキャストしたものからModifierRoleOptionを取得します
    /// </summary>
    /// <param name="modifierRoleIdByte">ModifierRoleIdをbyteにキャストした値</param>
    /// <returns>対応するModifierRoleOption、見つからない場合はnull</returns>
    public static ModifierRoleOption? GetModifierRoleOption(ModifierRoleId modifierRoleId)
    {
        return _modifierRoleOptionsByByte.TryGetValue((ushort)modifierRoleId, out var modifierRoleOption) ? modifierRoleOption : null;
    }

    /// <summary>
    /// GhostRoleIdをbyteにキャストしたものからGhostRoleOptionを取得します
    /// </summary>
    /// <param name="ghostRoleIdByte">GhostRoleIdをbyteにキャストした値</param>
    /// <returns>対応するGhostRoleOption、見つからない場合はnull</returns>
    public static GhostRoleOption? GetGhostRoleOption(GhostRoleId ghostRoleId)
    {
        return _ghostRoleOptionsByByte.TryGetValue((ushort)ghostRoleId, out var ghostRoleOption) ? ghostRoleOption : null;
    }

    public static bool TryGetRoleOption(RoleId roleId, out RoleOption roleOption)
    {
        return _roleOptionsByByte.TryGetValue((ushort)roleId, out roleOption);
    }

    public static bool TryGetModifierRoleOption(ModifierRoleId modifierRoleId, out ModifierRoleOption modifierRoleOption)
    {
        return _modifierRoleOptionsByByte.TryGetValue((ushort)modifierRoleId, out modifierRoleOption);
    }

    public static bool TryGetGhostRoleOption(GhostRoleId ghostRoleId, out GhostRoleOption ghostRoleOption)
    {
        return _ghostRoleOptionsByByte.TryGetValue((ushort)ghostRoleId, out ghostRoleOption);
    }

    /// <summary>
    /// キャッシュを更新します。RoleOptionLoadの後に呼び出される必要があります。
    /// </summary>
    private static void UpdateCaches()
    {
        // RoleOptionsのキャッシュを更新
        _roleOptionsByByte.Clear();
        if (RoleOptions != null)
        {
            foreach (var roleOption in RoleOptions)
            {
                _roleOptionsByByte[(byte)roleOption.RoleId] = roleOption;
            }
        }

        // ModifierRoleOptionsのキャッシュを更新
        _modifierRoleOptionsByByte.Clear();
        if (ModifierRoleOptions != null)
        {
            foreach (var modifierRoleOption in ModifierRoleOptions)
            {
                _modifierRoleOptionsByByte[(byte)modifierRoleOption.ModifierRoleId] = modifierRoleOption;
            }
        }

        // GhostRoleOptionsのキャッシュを更新
        _ghostRoleOptionsByByte.Clear();
        if (GhostRoleOptions != null)
        {
            foreach (var ghostRoleOption in GhostRoleOptions)
            {
                _ghostRoleOptionsByByte[(byte)ghostRoleOption.RoleId] = ghostRoleOption;
            }
        }
    }

    [CustomRPC]
    public static void RpcSyncRoleOption(RoleId roleId, byte numberOfCrews, int percentage)
    {
        var roleOption = RoleOptions.FirstOrDefault(o => o.RoleId == roleId);
        if (roleOption == null)
        {
            Logger.Warning($"ロールオプションが見つかりません: {roleId}");
            return;
        }
        roleOption.UpdateValues(numberOfCrews, percentage);
    }

    [CustomRPC]
    public static void RpcSyncModifierRoleOption(ModifierRoleId modifierRoleId, byte numberOfCrews, int percentage, int maxImpostors, int impostorChance, int maxNeutrals, int neutralChance, int maxCrewmates, int crewmateChance)
    {
        var roleOption = ModifierRoleOptions.FirstOrDefault(o => o.ModifierRoleId == modifierRoleId);
        if (roleOption == null)
        {
            Logger.Warning($"モディファイアロールオプションが見つかりません: {modifierRoleId}");
            return;
        }
        roleOption.UpdateValues(numberOfCrews, percentage, maxImpostors, impostorChance, maxNeutrals, neutralChance, maxCrewmates, crewmateChance);
    }

    /// <summary>
    /// 遅延付きでロールオプションを同期します。
    /// 同じロールに対する複数の変更は、最後の変更のみが送信されます。
    /// </summary>
    /// <param name="roleId">ロールID</param>
    /// <param name="numberOfCrews">クルー数</param>
    /// <param name="percentage">確率</param>
    public static void RpcSyncRoleOptionDelay(RoleId roleId, byte numberOfCrews, int percentage)
    {
        // ホストでない場合は何もしない
        if (!AmongUsClient.Instance.AmHost) return;

        // 既存のタスクがあれば遅延をリセット
        if (DelayedSyncTasks.TryGetValue(roleId, out var existingTask))
        {
            existingTask.UpdateDelay(SyncDelay);
        }
        else
        {
            // 新しいタスクを作成
            var task = new LateTask(() =>
            {
                var opt = RoleOptions.FirstOrDefault(o => o.RoleId == roleId);
                if (opt == null) return;
                // 実際の同期を実行
                RpcSyncRoleOption(roleId, opt.NumberOfCrews, opt.Percentage);
                // タスクとペンディング変更を削除
                DelayedSyncTasks.Remove(roleId);
            }, SyncDelay, $"SyncRoleOption_{roleId}");

            // タスクを保存
            DelayedSyncTasks[roleId] = task;
        }
    }

    public static void RpcSyncModifierRoleOptionDelay(ModifierRoleId modifierRoleId, byte numberOfCrews, int percentage)
    {
        // ホストでない場合は何もしない
        if (!AmongUsClient.Instance.AmHost) return;

        // 既存のタスクがあれば遅延をリセット
        if (DelayedSyncTasksModifier.TryGetValue(modifierRoleId, out var existingTask))
        {
            existingTask.UpdateDelay(SyncDelay);
        }
        else
        {
            // 新しいタスクを作成
            var task = new LateTask(() =>
            {
                var opt = ModifierRoleOptions.FirstOrDefault(o => o.ModifierRoleId == modifierRoleId);
                if (opt == null) return;
                // 実際の同期を実行
                RpcSyncModifierRoleOption(modifierRoleId, opt.NumberOfCrews, opt.Percentage, opt.MaxImpostors, opt.ImpostorChance, opt.MaxNeutrals, opt.NeutralChance, opt.MaxCrewmates, opt.CrewmateChance);
                // タスクとペンディング変更を削除
                DelayedSyncTasksModifier.Remove(modifierRoleId);
            }, SyncDelay, $"SyncModifierRoleOption_{modifierRoleId}");

            // タスクを保存
            DelayedSyncTasksModifier[modifierRoleId] = task;
        }
    }

    public static void RpcSyncGhostRoleOptionDelay(GhostRoleId roleId, byte numberOfCrews, int percentage) // RoleId -> GhostRoleId
    {
        // ホストでない場合は何もしない
        if (!AmongUsClient.Instance.AmHost) return;

        // 既存のタスクがあれば遅延をリセット
        if (DelayedSyncTasksGhost.TryGetValue(roleId, out var existingTask))
        {
            existingTask.UpdateDelay(SyncDelay);
        }
        else
        {
            // 新しいタスクを作成
            var task = new LateTask(() =>
            {
                var opt = GhostRoleOptions.FirstOrDefault(o => o.RoleId == roleId);
                if (opt == null) return;
                // 実際の同期を実行 (GhostRoleも通常のRoleOptionとして同期)
                RpcSyncGhostRoleOption(roleId, opt.NumberOfCrews, opt.Percentage); // Change to RpcSyncGhostRoleOption
                // タスクとペンディング変更を削除
                DelayedSyncTasksGhost.Remove(roleId);
            }, SyncDelay, $"SyncGhostRoleOption_{roleId}");

            // タスクを保存
            DelayedSyncTasksGhost[roleId] = task;
        }
    }

    [CustomRPC]
    public static void RpcSyncGhostRoleOption(GhostRoleId roleId, byte numberOfCrews, int percentage) // RoleId -> GhostRoleId
    {
        var roleOption = GhostRoleOptions.FirstOrDefault(o => o.RoleId == roleId);
        if (roleOption == null)
        {
            Logger.Warning($"ゴーストロールオプションが見つかりません: {roleId}");
            return;
        }
        roleOption.UpdateValues(numberOfCrews, percentage);
    }

    [CustomRPC]
    public static void _RpcSyncRoleOptionsAll(Dictionary<byte, (byte, int)> options)
    {
        foreach (var roleOption in RoleOptions)
        {
            if (options.TryGetValue((byte)roleOption.RoleId, out var values))
            {
                roleOption.UpdateValues(values.Item1, values.Item2);
            }
        }
    }
    [CustomRPC]
    public static void _RpcSyncModifierRoleOptionsAll(Dictionary<byte, (byte, int, int, int, int, int, int, int)> options)
    {
        foreach (var modifierRoleOption in ModifierRoleOptions)
        {
            if (options.TryGetValue((byte)modifierRoleOption.ModifierRoleId, out var values))
            {
                modifierRoleOption.UpdateValues(values.Item1, values.Item2, values.Item3, values.Item4, values.Item5, values.Item6, values.Item7, values.Item8);
            }
        }
    }
    [CustomRPC]
    public static void _RpcSyncGhostRoleOptionsAll(Dictionary<byte, (byte, int)> options) // GhostRole用
    {
        foreach (var ghostRoleOption in GhostRoleOptions)
        {
            if (options.TryGetValue((byte)ghostRoleOption.RoleId, out var values))
            {
                ghostRoleOption.UpdateValues(values.Item1, values.Item2);
            }
        }
    }

    public static void RpcSyncRoleOptionsAll()
    {
        // 変更されたロールオプションだけを送信
        var roleOptions = RoleOptions.ToDictionary(
            o => (byte)o.RoleId,
            o => (o.NumberOfCrews, o.Percentage));
        _RpcSyncRoleOptionsAll(roleOptions);
        var modifierRoleOptions = ModifierRoleOptions.ToDictionary(
            o => (byte)o.ModifierRoleId,
            o => (o.NumberOfCrews, o.Percentage, o.MaxImpostors, o.ImpostorChance, o.MaxNeutrals, o.NeutralChance, o.MaxCrewmates, o.CrewmateChance));
        _RpcSyncModifierRoleOptionsAll(modifierRoleOptions);
        var ghostRoleOptions = GhostRoleOptions.ToDictionary( // GhostRole用
            o => (byte)o.RoleId,
            o => (o.NumberOfCrews, o.Percentage));
        _RpcSyncGhostRoleOptionsAll(ghostRoleOptions); // GhostRole用
    }

    public static void RoleOptionLoad()
    {
        // パフォーマンス向上のため、カスタムオプションを一度だけ取得
        var customOptions = CustomOptionManager.CustomOptions; // IReadOnlyListではなく直接Listを使用

        // RoleIdがNoneでなく、Vanillaロールでないロールだけを集める
        var validRoles = CustomRoleManager.AllRoles
            .Where(role => role.Role != RoleId.None && !role.IsVanillaRole)
            .OrderBy(role => role.OptionTeam)  // まずOptionTeamで並び替え
            .ThenBy(role => role.Role);        // 次にRoleIdで並び替え

        // 各ロールに対応するカスタムオプションをフィルタリングしてRoleOptionを生成
        RoleOptions = validRoles
            .Select(role =>
            {
                // CustomOptionsの順序を保持したまま、該当するロールのオプションを取得
                var optionsForRole = customOptions
                    .Where(option => option.ParentRole == role.Role) // GhostRoleでないもののみ
                    .OrderBy(option => customOptions.IndexOf(option))  // CustomOptionsの順序を維持
                    .ToArray();
                return new RoleOption(role.Role, 0, 0, optionsForRole);
            })
            .ToArray();

        var validModifiers = CustomRoleManager.AllModifiers
            .Where(role => role.ModifierRole != ModifierRoleId.None)
            .OrderBy(role => role.ModifierRole)
            .ThenBy(role => role.ModifierRole)
            .ToArray();
        ModifierRoleOptions = validModifiers
            .Select(role =>
            {
                var optionsForRole = customOptions
                    .Where(option => option.ParentModifierRole == role.ModifierRole)
                    .ToArray();
                return new ModifierRoleOption(role.ModifierRole, 0, 0, optionsForRole);
            })
            .ToArray();

        // GhostRoleのオプションを読み込む
        var validGhostRoles = CustomRoleManager.AllGhostRoles
            .Where(role => role.Role != GhostRoleId.None && !role.IsVanillaRole)
            .OrderBy(role => role.Role); // RoleIdで並び替え
        GhostRoleOptions = validGhostRoles
            .Select(role =>
            {
                var optionsForRole = customOptions
                    .Where(option => option.ParentGhostRole == role.Role) // Use ParentGhostRole
                    .OrderBy(option => customOptions.IndexOf(option))
                    .ToArray();
                return new GhostRoleOption(role.Role, 0, 0, optionsForRole);
            })
            .ToArray();

        UpdateCaches();
    }

    public static void AddExclusivitySetting(int maxAssign, string[] roles)
    {
        ExclusivitySettings.Add(new ExclusivityData(maxAssign, roles));
    }

    public static void ClearExclusivitySettings()
    {
        ExclusivitySettings.Clear();
    }

    /// <summary>
    /// 排他設定を適用します。
    /// 役職がアサインされた際に、同じ排他グループ内の他の役職をアサインテーブルから除外します。
    /// </summary>
    /// <param name="assignedRoleIds">既にアサインされた役職のIDリスト</param>
    /// <param name="ticketsToUpdate">更新するチケットリスト</param>
    public static void ApplyExclusivitySettings(List<RoleId> assignedRoleIds, List<AssignTickets>[] ticketsToUpdatesNotHandred, List<AssignTickets>[] ticketsToUpdatesHundred)
    {
        if (ExclusivitySettings.Count == 0 || assignedRoleIds.Count == 0 || ticketsToUpdatesNotHandred.Length == 0 || ticketsToUpdatesHundred.Length == 0)
            return;

        // チケットを削除する役職IDのセットを作成
        HashSet<RoleId> rolesToRemove = new();

        // 各排他設定をチェック
        foreach (var exclusivity in ExclusivitySettings)
        {
            // この排他グループ内の既にアサインされた役職の数をカウント
            var assignedInGroupCount = exclusivity.Roles.Count(roleId => assignedRoleIds.Contains(roleId));

            // 設定された最大数に達した場合、このグループの残りの役職をアサインから除外
            if (assignedInGroupCount >= exclusivity.MaxAssign)
            {
                foreach (var roleId in exclusivity.Roles)
                {
                    rolesToRemove.Add(roleId);
                }
            }
        }

        // 除外すべき役職のチケットをリストから削除
        if (rolesToRemove.Count > 0)
        {
            foreach (var ticketsToUpdate in ticketsToUpdatesNotHandred)
            {
                ticketsToUpdate.RemoveAll(ticket => rolesToRemove.Contains(ticket.RoleOption.RoleId));
            }
            foreach (var ticketsToUpdate in ticketsToUpdatesHundred)
            {
                ticketsToUpdate.RemoveAll(ticket => rolesToRemove.Contains(ticket.RoleOption.RoleId));
            }
        }
    }
}

public static class CustomOptionSaver
{
    private static readonly IOptionStorage Storage;
    private const byte CurrentVersion = 1;
    private static int currentPreset = 0;
    public static bool IsLoaded { get; private set; } = false;
    public static Dictionary<int, string> presetNames = new();
    public static IReadOnlyDictionary<int, string> PresetNames => presetNames;

    public static int CurrentPreset
    {
        get => currentPreset;
        set => currentPreset = value;
    }

    public static string GetPresetName(int preset)
    {
        if (presetNames.TryGetValue(preset, out var name))
        {
            return name;
        }
        return ModTranslation.GetString("PresetDefault", preset + 1);
    }

    public static void SetPresetName(int preset, string name)
    {
        if (preset < 0 || preset > 9) return;
        presetNames[preset] = name;
        Save();
    }

    public static void RemovePreset(int preset)
    {
        if (preset < 0 || preset > 9) return;
        if (!presetNames.ContainsKey(preset)) return;

        string _;
        presetNames.Remove(preset, out _);

        Save();
        // 削除対象が現在のプリセットの場合
        if (preset == CurrentPreset)
        {
            // プリセットIDの中で、削除対象より小さい中で一番大きいものを探す
            // なければ、削除対象より大きい中で一番小さいものを探す
            int newPreset = presetNames.Keys
                .Where(id => id < preset)
                .DefaultIfEmpty(
                    presetNames.Keys
                        .Where(id => id > preset)
                        .DefaultIfEmpty(0)
                        .Min()
                )
                .Max();
            Logger.Info($"新しいプリセットID: {newPreset}");
            LoadPreset(newPreset);
        }
    }

    static CustomOptionSaver()
    {
        Storage = new FileOptionStorage(
            new DirectoryInfo(SuperNewRolesPlugin.BaseDirectory + "/SaveData/"),
            "Options.data",
            "PresetOptions_"
        );
    }

    public static void SaverLoad()
    {
        try
        {
            Storage.EnsureStorageExists();
            // ReadAndApplyLoadedOptionsAndGetPresetIdから、読み込み成功の可否と、成功した場合のプリセット番号を取得
            var (loadSuccessful, loadedPresetId) = ReadAndApplyLoadedOptionsAndGetPresetId();
            if (loadSuccessful)
            {
                CurrentPreset = loadedPresetId; // 正常にロードできた場合のみCurrentPresetを更新
            }

            // presetNamesのロードは、Storage.LoadOptionDataが呼ばれた後に行う必要がある
            // なぜなら、FileOptionStorage.LoadOptionData内でpresetNamesが設定されるため
            LoadPresetNames();
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Option loading failed: {ex.Message}");
            IsLoaded = true; // 元のコードに合わせてtrueにするが、falseの方が安全かもしれない
        }
    }

    // 元の ReadAndApplyOptions を改名し、戻り値で成功/失敗と読み込んだプリセットIDを返す
    // (bool success, int loadedPresetId)
    private static (bool, int) ReadAndApplyLoadedOptionsAndGetPresetId()
    {
        var (optionDataSuccess, version, presetIdFromOptionData) = Storage.LoadOptionData();

        if (!optionDataSuccess) // LoadOptionData自体が失敗 (ファイルなし、チェックサムエラーなど)
        {
            Logger.Warning($"Loading OptionData failed. Using default settings.");
            ApplyLoadedOptions(new Dictionary<string, byte>()); // 全オプションをデフォルトに
            // presetNames は Storage.LoadOptionData 内でクリアされているはず
            return (false, 0); // 失敗。返すプリセットIDは便宜上0 (CurrentPresetの初期値と同じ)
        }
        if (version != CurrentVersion) // バージョン不一致
        {
            Logger.Warning($"Invalid option data version: {version}. Expected: {CurrentVersion}. Using default settings.");
            ApplyLoadedOptions(new Dictionary<string, byte>()); // 全オプションをデフォルトに
            // presetNames は Storage.LoadOptionData 内で読み込めているかもしれないが、バージョンが違うので使わない方が安全か。
            // Storage.LoadOptionData は version が違っても presetIdFromOptionData と presetNames を返すが、
            // ここではバージョン不一致の場合はプリセットデータも読まず、失敗扱いとする。
            return (false, presetIdFromOptionData); // 失敗。読み込もうとしたプリセットIDは返す。
        }

        // Options.data は正常に読み込めた。次に、記録されていたプリセットのデータを読み込む。
        var (presetDataSuccess, options) = Storage.LoadPresetData(presetIdFromOptionData);
        if (!presetDataSuccess)
        {
            Logger.Warning($"Preset data loading failed for preset {presetIdFromOptionData}. Using default settings.");
            ApplyLoadedOptions(new Dictionary<string, byte>()); // 該当プリセットのデータが読めなかったのでデフォルトに
            return (false, presetIdFromOptionData); // 失敗。読み込もうとしたプリセットIDは返す。
        }

        ApplyLoadedOptions(options); // 正常に読み込めたオプションを適用
        return (true, presetIdFromOptionData); // 成功。読み込んだプリセットIDを返す。
    }

    private static void LoadPresetNames()
    {
        var (success, names) = Storage.LoadPresetNames();
        if (success)
        {
            presetNames = names;
        }
    }

    public static void LoadPreset(int preset)
    {
        if (preset < 0 || preset > 9) return;
        CurrentPreset = preset;
        try
        {
            var (optionsSuccess, options) = Storage.LoadPresetData(preset);
            if (!optionsSuccess)
            {
                Logger.Warning("Preset data loading failed. Using default settings.");
                return;
            }
            ApplyLoadedOptions(options);
            Storage.SaveOptionData(CurrentVersion, CurrentPreset);
        }
        catch (Exception ex)
        {
            Logger.Error($"Preset loading failed: {ex.Message}");
        }
    }

    private static void ApplyLoadedOptions(Dictionary<string, byte> options)
    {
        foreach (var option in CustomOptionManager.GetCustomOptions())
        {
            if (options.TryGetValue(option.Id, out var value))
            {
                try
                {
                    option.UpdateSelection(value);
                }
                catch
                {
                    Logger.Warning($"Failed to apply option {option.Id}. Using default.");
                }
            }
            else
            {
                option.UpdateSelection(option.DefaultSelection);
            }
        }
    }

    public static void Save()
    {
        try
        {
            Storage.SaveOptionData(CurrentVersion, CurrentPreset);
            Storage.SavePresetData(CurrentPreset, CustomOptionManager.GetCustomOptions());
        }
        catch (Exception ex)
        {
            Logger.Error($"Option saving failed: {ex.Message}");
        }
    }
}

public interface IOptionStorage
{
    void EnsureStorageExists();
    (bool success, byte version, int preset) LoadOptionData();
    (bool success, Dictionary<string, byte> options) LoadPresetData(int preset);
    (bool success, Dictionary<int, string> names) LoadPresetNames();
    void SaveOptionData(byte version, int preset);
    void SavePresetData(int preset, IEnumerable<CustomOption> options);
}

public class FileOptionStorage : IOptionStorage
{
    private readonly DirectoryInfo _directory;
    private readonly string _optionFileName;
    private readonly string _presetFileNameBase;
    private static readonly object FileLocker = new();

    public FileOptionStorage(DirectoryInfo directory, string optionFileName, string presetFileNameBase)
    {
        _directory = directory;
        _optionFileName = Path.Combine(directory.FullName, optionFileName);
        _presetFileNameBase = Path.Combine(directory.FullName, presetFileNameBase);
    }

    public void EnsureStorageExists()
    {
        if (!_directory.Exists)
        {
            _directory.Create();
            _directory.Attributes |= FileAttributes.Hidden;
        }
    }

    public (bool success, byte version, int preset) LoadOptionData()
    {
        lock (FileLocker)
        {
            if (!File.Exists(_optionFileName))
            {
                CustomOptionSaver.presetNames.Clear(); // ファイルがない場合は内部のpresetNamesもクリア
                return (false, 0, 0);
            }

            using var fileStream = new FileStream(_optionFileName, FileMode.Open);
            using var reader = new BinaryReader(fileStream);

            byte version = reader.ReadByte();
            if (!ValidateChecksum(reader))
            {
                CustomOptionSaver.presetNames.Clear(); // チェックサム不正でも内部のpresetNamesをクリア
                return (false, version, 0);
            }

            int preset = reader.ReadInt32();

            // プリセット名を読み込む (this.presetNames を更新)
            CustomOptionSaver.presetNames.Clear();
            int nameCount = reader.ReadInt32();
            for (int i = 0; i < nameCount; i++)
            {
                int presetId = reader.ReadInt32();
                string name = reader.ReadString();
                CustomOptionSaver.presetNames[presetId] = name;
            }

            return (true, version, preset);
        }
    }

    public (bool success, Dictionary<string, byte> options) LoadPresetData(int preset)
    {
        lock (FileLocker)
        {
            string fileName = $"{_presetFileNameBase}{preset}.data";
            if (!File.Exists(fileName))
            {
                return (false, new());
            }

            using var fileStream = new FileStream(fileName, FileMode.Open);
            using var reader = new BinaryReader(fileStream);

            if (!ValidateChecksum(reader))
            {
                return (false, new());
            }

            Dictionary<string, byte> options = ReadOptions(reader);

            // RoleOption, Exclusivity, ModifierRole, GhostRoleの情報を読み込む
            if (fileStream.Position < fileStream.Length)
            {
                LoadRoleOptionsData(reader);
                if (fileStream.Position < fileStream.Length)
                {
                    LoadExclusivitySettingsData(reader);
                    if (fileStream.Position < fileStream.Length)
                    {
                        LoadModifierRoleOptionsData(reader);
                        if (fileStream.Position < fileStream.Length)
                        {
                            LoadGhostRoleOptionsData(reader);
                            if (fileStream.Position < fileStream.Length)
                            {
                                LoadCategoryAssignFilterData(reader);
                            }
                        }
                    }
                }
            }
            return (true, options);
        }
    }

    private void LoadRoleOptionsData(BinaryReader reader)
    {
        int roleOptionCount = reader.ReadInt32();
        for (int i = 0; i < roleOptionCount; i++)
        {
            string roleIdStr = reader.ReadString();
            byte numberOfCrews = reader.ReadByte();
            int percentage = reader.ReadInt32();

            if (Enum.TryParse(typeof(RoleId), roleIdStr, out var roleIdObj) && roleIdObj is RoleId roleId)
            {
                var roleOption = RoleOptionManager.RoleOptions.FirstOrDefault(x => x.RoleId == roleId);
                if (roleOption != null)
                {
                    roleOption.NumberOfCrews = numberOfCrews;
                    roleOption.Percentage = percentage;
                }
            }
        }
    }

    private void LoadExclusivitySettingsData(BinaryReader reader)
    {
        int exclusivityCount = reader.ReadInt32();
        RoleOptionManager.ClearExclusivitySettings();
        for (int i = 0; i < exclusivityCount; i++)
        {
            int maxAssign = reader.ReadInt32();
            int rolesCount = reader.ReadInt32();
            string[] roles = new string[rolesCount];
            for (int j = 0; j < rolesCount; j++)
            {
                roles[j] = reader.ReadString();
            }
            RoleOptionManager.AddExclusivitySetting(maxAssign, roles);
        }
    }

    private void LoadModifierRoleOptionsData(BinaryReader reader)
    {
        int modifierRoleCount = reader.ReadInt32();
        for (int i = 0; i < modifierRoleCount; i++)
        {
            string modifierRoleIdStr = reader.ReadString();
            byte numberOfCrews = reader.ReadByte();
            int percentage = reader.ReadInt32();

            // 陣営別設定を読み込み
            int maxImpostors = reader.ReadInt32();
            int impostorChance = reader.ReadInt32();
            int maxNeutrals = reader.ReadInt32();
            int neutralChance = reader.ReadInt32();
            int maxCrewmates = reader.ReadInt32();
            int crewmateChance = reader.ReadInt32();

            if (Enum.TryParse(typeof(ModifierRoleId), modifierRoleIdStr, out var modifierRoleIdObj) && modifierRoleIdObj is ModifierRoleId modifierRoleId)
            {
                var modifierRoleOption = RoleOptionManager.ModifierRoleOptions.FirstOrDefault(x => x.ModifierRoleId == modifierRoleId);
                if (modifierRoleOption != null)
                {
                    modifierRoleOption.NumberOfCrews = numberOfCrews;
                    modifierRoleOption.Percentage = percentage;
                    modifierRoleOption.MaxImpostors = maxImpostors;
                    modifierRoleOption.ImpostorChance = impostorChance;
                    modifierRoleOption.MaxNeutrals = maxNeutrals;
                    modifierRoleOption.NeutralChance = neutralChance;
                    modifierRoleOption.MaxCrewmates = maxCrewmates;
                    modifierRoleOption.CrewmateChance = crewmateChance;

                    // AssignFilterListの復元
                    var roleBase = CustomRoleManager.AllModifiers.FirstOrDefault(r => r.ModifierRole == modifierRoleId);
                    if (roleBase != null && roleBase.AssignFilter)
                    {
                        int assignFilterCount = reader.ReadInt32();
                        modifierRoleOption.AssignFilterList.Clear();
                        for (int j = 0; j < assignFilterCount; j++)
                        {
                            string roleIdStr = reader.ReadString();
                            if (Enum.TryParse(typeof(RoleId), roleIdStr, out var roleIdObj) && roleIdObj is RoleId roleId)
                            {
                                modifierRoleOption.AssignFilterList.Add(roleId);
                            }
                        }
                    }
                }
            }
        }
    }

    private void LoadGhostRoleOptionsData(BinaryReader reader)
    {
        int ghostRoleCount = reader.ReadInt32();
        for (int i = 0; i < ghostRoleCount; i++)
        {
            string ghostRoleIdStr = reader.ReadString();
            byte numberOfCrews = reader.ReadByte();
            int percentage = reader.ReadInt32();
            if (Enum.TryParse(typeof(GhostRoleId), ghostRoleIdStr, out var ghostRoleIdObj) && ghostRoleIdObj is GhostRoleId ghostRoleId)
            {
                var ghostRoleOption = RoleOptionManager.GhostRoleOptions.FirstOrDefault(x => x.RoleId == ghostRoleId);
                if (ghostRoleOption != null)
                {
                    ghostRoleOption.NumberOfCrews = numberOfCrews;
                    ghostRoleOption.Percentage = percentage;
                }
            }
        }
    }

    private void LoadCategoryAssignFilterData(BinaryReader reader)
    {
        int categoryCount = reader.ReadInt32();
        for (int i = 0; i < categoryCount; i++)
        {
            string categoryName = reader.ReadString();
            int assignFilterCount = reader.ReadInt32();
            var category = CustomOptionManager.OptionCategories.FirstOrDefault(c => c.Name == categoryName && c.HasModifierAssignFilter);
            if (category != null)
            {
                category.ModifierAssignFilter.Clear();
                for (int j = 0; j < assignFilterCount; j++)
                {
                    string roleIdStr = reader.ReadString();
                    if (Enum.TryParse(typeof(RoleId), roleIdStr, out var roleIdObj) && roleIdObj is RoleId roleId)
                    {
                        category.ModifierAssignFilter.Add(roleId);
                    }
                }
            }
            else
            {
                // 読み飛ばし
                for (int j = 0; j < assignFilterCount; j++)
                {
                    reader.ReadString();
                }
            }
        }
    }

    public (bool success, Dictionary<int, string> names) LoadPresetNames()
    {
        // LoadOptionData() によって更新された可能性のある内部の presetNames フィールドのコピーを返す
        lock (FileLocker) // presetNamesへのアクセスを保護
        {
            return (true, new Dictionary<int, string>(CustomOptionSaver.presetNames));
        }
    }

    public void SaveOptionData(byte version, int preset)
    {
        lock (FileLocker)
        {
            using var fileStream = new FileStream(_optionFileName, FileMode.Create);
            using var writer = new BinaryWriter(fileStream);

            writer.Write(version);
            WriteChecksum(writer);
            writer.Write(preset);

            // プリセット名を保存
            writer.Write(CustomOptionSaver.presetNames.Count);
            foreach (KeyValuePair<int, string> pair in CustomOptionSaver.presetNames)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }
    }

    public void SavePresetData(int preset, IEnumerable<CustomOption> options)
    {
        lock (FileLocker)
        {
            string fileName = $"{_presetFileNameBase}{preset}.data";
            using var fileStream = new FileStream(fileName, FileMode.Create);
            using var writer = new BinaryWriter(fileStream);

            WriteChecksum(writer);
            WriteOptions(writer, options);

            WriteRoleOptionsData(writer);
            WriteExclusivitySettingsData(writer);
            WriteModifierRoleOptionsData(writer);
            WriteGhostRoleOptionsData(writer);
            WriteCategoryAssignFilterData(writer);
        }
    }

    private void WriteRoleOptionsData(BinaryWriter writer)
    {
        var roleOptions = RoleOptionManager.RoleOptions;
        writer.Write(roleOptions.Length);

        foreach (var roleOption in roleOptions)
        {
            writer.Write(roleOption.RoleId.ToString());
            writer.Write(roleOption.NumberOfCrews);
            writer.Write(roleOption.Percentage);
        }
    }

    private void WriteExclusivitySettingsData(BinaryWriter writer)
    {
        var exclusivitySettings = RoleOptionManager.ExclusivitySettings;
        writer.Write(exclusivitySettings.Count);
        foreach (var setting in exclusivitySettings)
        {
            writer.Write(setting.MaxAssign);
            writer.Write(setting.Roles.Count);
            foreach (var role in setting.Roles)
            {
                writer.Write(role.ToString());
            }
        }
    }

    private void WriteModifierRoleOptionsData(BinaryWriter writer)
    {
        var modifierRoleOptions = RoleOptionManager.ModifierRoleOptions;
        writer.Write(modifierRoleOptions.Length);
        foreach (var roleOption in modifierRoleOptions)
        {
            writer.Write(roleOption.ModifierRoleId.ToString());
            writer.Write(roleOption.NumberOfCrews);
            writer.Write(roleOption.Percentage);

            // 陣営別設定を保存
            writer.Write(roleOption.MaxImpostors);
            writer.Write(roleOption.ImpostorChance);
            writer.Write(roleOption.MaxNeutrals);
            writer.Write(roleOption.NeutralChance);
            writer.Write(roleOption.MaxCrewmates);
            writer.Write(roleOption.CrewmateChance);

            // AssignFilterListの保存
            var roleBase = CustomRoleManager.AllModifiers.FirstOrDefault(r => r.ModifierRole == roleOption.ModifierRoleId);
            if (roleBase != null && roleBase.AssignFilter)
            {
                writer.Write(roleOption.AssignFilterList.Count);
                foreach (var roleId in roleOption.AssignFilterList)
                {
                    writer.Write(roleId.ToString());
                }
            }
        }
    }

    private void WriteGhostRoleOptionsData(BinaryWriter writer)
    {
        var ghostRoleOptions = RoleOptionManager.GhostRoleOptions;
        writer.Write(ghostRoleOptions.Length);
        foreach (var roleOption in ghostRoleOptions)
        {
            writer.Write(roleOption.RoleId.ToString());
            writer.Write(roleOption.NumberOfCrews);
            writer.Write(roleOption.Percentage);
        }
    }

    private void WriteCategoryAssignFilterData(BinaryWriter writer)
    {
        var categories = CustomOptionManager.OptionCategories.Where(c => c.HasModifierAssignFilter).ToList();
        writer.Write(categories.Count);
        foreach (var category in categories)
        {
            writer.Write(category.Name);
            writer.Write(category.ModifierAssignFilter.Count);
            foreach (var roleId in category.ModifierAssignFilter)
            {
                writer.Write(roleId.ToString());
            }
        }
    }

    private static Dictionary<string, byte> ReadOptions(BinaryReader reader)
    {
        int optionCount = reader.ReadInt32();
        var options = new Dictionary<string, byte>();

        for (int i = 0; i < optionCount; i++)
        {
            string id = reader.ReadString();
            options[id] = reader.ReadByte();
        }

        return options;
    }

    private static void WriteOptions(BinaryWriter writer, IEnumerable<CustomOption> options)
    {
        var optionsList = options.Where(o => !o.IsDefaultValue).ToList();
        writer.Write(optionsList.Count);

        foreach (var option in optionsList)
        {
            writer.Write(option.Id);
            writer.Write((byte)option.MySelection);
        }
    }

    private static void WriteChecksum(BinaryWriter writer)
    {
        int random = ModHelpers.GetRandomInt(15);
        writer.Write((byte)random);
        writer.Write((byte)(random * random));
    }

    private static bool ValidateChecksum(BinaryReader reader)
    {
        int random = reader.ReadByte();
        int random2 = reader.ReadByte();
        return (random * random) == random2;
    }
}

public enum CustomOptionType
{
    None,
    Float,
    Int,
    Bool,
    Byte,
    Select
}

public static class ComputeMD5Hash
{
    private static readonly MD5 md5 = MD5.Create();
    public static string Compute(string str)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(str);
        var hashBytes = md5.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant(); // ハッシュを16進数の文字列に変換
    }
}

[AttributeUsage(AttributeTargets.Field)]
public abstract class CustomOptionBaseAttribute : Attribute
{
    public string Id { get; }
    [AllowNull]
    public FieldInfo FieldInfo { get; private set; }
    public string TranslationName { get; }
    public CustomOptionType OptionType { get; private set; } = CustomOptionType.None;
    public string? ParentFieldName { get; }
    public DisplayModeId DisplayMode { get; }
    public object? ParentActiveValue { get; }

    protected CustomOptionBaseAttribute(string id, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All, object? parentActiveValue = null)
    {
        Id = ComputeMD5Hash.Compute(id);
        TranslationName = translationName ?? id;
        ParentFieldName = parentFieldName;
        DisplayMode = displayMode;
        ParentActiveValue = parentActiveValue;
    }

    public void SetFieldInfo(FieldInfo fieldInfo)
    {
        this.FieldInfo = fieldInfo;
        OptionType = DetermineOptionType(fieldInfo);
    }

    private static CustomOptionType DetermineOptionType(FieldInfo fieldInfo)
    {
        return fieldInfo.FieldType switch
        {
            var t when t.IsEnum => CustomOptionType.Select,
            var t when t == typeof(float) => CustomOptionType.Float,
            var t when t == typeof(int) => CustomOptionType.Int,
            var t when t == typeof(bool) => CustomOptionType.Bool,
            var t when t == typeof(byte) => CustomOptionType.Byte,
            _ => CustomOptionType.None
        };
    }

    public abstract object[] GenerateSelections();
    public abstract byte GenerateDefaultSelection();
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionSelectAttribute : CustomOptionBaseAttribute
{
    private readonly string[] _selectionNames;
    private readonly Type _enumType;
    private readonly object _defaultValue;
    public string TranslationPrefix { get; }

    public CustomOptionSelectAttribute(string id, Type enumType, string translationPrefix, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All, object? parentActiveValue = null, object defaultValue = null)
        : base(id, translationName, parentFieldName, displayMode, parentActiveValue)
    {
        if (!enumType.IsEnum) throw new ArgumentException("Type must be an enum", nameof(enumType));
        _enumType = enumType;
        _selectionNames = Enum.GetNames(enumType);
        TranslationPrefix = translationPrefix;
        _defaultValue = defaultValue;
    }

    public override object[] GenerateSelections() =>
        _selectionNames.Select(name => Enum.Parse(_enumType, name)).ToArray();

    public override byte GenerateDefaultSelection()
    {
        if (_defaultValue == null) return 0;
        var selections = GenerateSelections();
        for (byte i = 0; i < selections.Length; i++)
        {
            if (selections[i].Equals(_defaultValue))
            {
                return i;
            }
        }
        Logger.Warning($"Default value {_defaultValue} not found for enum type {_enumType}. Using index 0.");
        return 0;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public abstract class CustomOptionNumericAttribute<T> : CustomOptionBaseAttribute
    where T : struct, IComparable<T>
{
    public T Min { get; }
    public T Max { get; }
    public T Step { get; }
    public T DefaultValue { get; }
    public string? Suffix { get; }
    public event Action<T> ValueChanged;

    protected CustomOptionNumericAttribute(string id, T min, T max, T step, T defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All, object? parentActiveValue = null, string? suffix = null)
        : base(id, translationName, parentFieldName, displayMode, parentActiveValue)
    {
        Min = min;
        Max = max;
        Step = step;
        DefaultValue = defaultValue;
        Suffix = suffix;
    }

    public override object[] GenerateSelections()
    {
        var selections = new List<object>();
        T currentValue = Min;
        while (Comparer<T>.Default.Compare(currentValue, Max) <= 0)
        {
            selections.Add(currentValue);
            currentValue = Add(currentValue, Step);
        }
        return selections.ToArray();
    }

    protected abstract T Add(T a, T b);

    public void OnValueChanged(T value)
    {
        ValueChanged?.Invoke(value);
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionFloatAttribute : CustomOptionNumericAttribute<float>
{
    public CustomOptionFloatAttribute(string id, float min, float max, float step, float defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All, object? parentActiveValue = null, string? suffix = null)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName, displayMode, parentActiveValue, suffix) { }

    protected override float Add(float a, float b) => a + b;
    public override byte GenerateDefaultSelection() => (byte)((DefaultValue - Min) / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionIntAttribute : CustomOptionNumericAttribute<int>
{
    public CustomOptionIntAttribute(string id, int min, int max, int step, int defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All, object? parentActiveValue = null, string? suffix = null)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName, displayMode, parentActiveValue, suffix) { }

    protected override int Add(int a, int b) => a + b;
    public override byte GenerateDefaultSelection() => (byte)((DefaultValue - Min) / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionByteAttribute : CustomOptionNumericAttribute<byte>
{
    public CustomOptionByteAttribute(string id, byte min, byte max, byte step, byte defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All, object? parentActiveValue = null, string? suffix = null)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName, displayMode, parentActiveValue, suffix) { }

    protected override byte Add(byte a, byte b) => (byte)(a + b);
    public override byte GenerateDefaultSelection() => (byte)((DefaultValue - Min) / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionBoolAttribute : CustomOptionBaseAttribute
{
    public bool DefaultValue { get; }

    public CustomOptionBoolAttribute(string id, bool defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All, object? parentActiveValue = null)
        : base(id, translationName, parentFieldName, displayMode, parentActiveValue)
    {
        DefaultValue = defaultValue;
    }

    public override object[] GenerateSelections() =>
        [false, true];

    public override byte GenerateDefaultSelection() => (byte)(DefaultValue ? 1 : 0);
}

[AttributeUsage(AttributeTargets.Field)]
public class ModifierAttribute : Attribute
{
    public ModifierRoleId ModifierRoleId { get; }
    public ModifierAttribute(ModifierRoleId modifierRoleId = ModifierRoleId.None)
    {
        ModifierRoleId = modifierRoleId;
    }
}
[AttributeUsage(AttributeTargets.Field)]
public class AssignFilterAttribute : Attribute
{
    public AssignedTeamType[] AssignedTeamTypes { get; }
    public RoleId[] HiddenRoleIds { get; }
    public AssignFilterAttribute(AssignedTeamType[] assignedTeamTypes, RoleId[] hiddenRoleIds)
    {
        AssignedTeamTypes = assignedTeamTypes;
        HiddenRoleIds = hiddenRoleIds;
    }
}
public class CustomOptionCategory
{
    public string Id { get; }
    public string Name { get; }
    public CustomOption[] Options { get; private set; } = Array.Empty<CustomOption>();
    private List<CustomOption> _options = new();
    public bool IsModifier { get; }
    public bool HasModifierAssignFilter { get; }
    public List<RoleId> ModifierAssignFilter { get; set; } = new();
    public ModifierRoleId ModifierRoleId { get; }
    public AssignedTeamType[] ModifierAssignFilterTeam { get; set; } = Array.Empty<AssignedTeamType>();
    public RoleId[] ModifierDoNotAssignRoles { get; set; } = Array.Empty<RoleId>();

    public CustomOptionCategory(string name, bool isModifier = false, ModifierRoleId modifierRoleId = ModifierRoleId.None, bool hasModifierAssignFilter = false, List<RoleId> modifierAssignFilter = null, AssignedTeamType[] modifierAssignFilterTeam = null, RoleId[] modifierDoNotAssignRoles = null)
    {
        Id = ComputeMD5Hash.Compute(name);
        Name = name; // 後でTranslationを使用して翻訳する
        RegisterCategory(this);
        IsModifier = isModifier;
        HasModifierAssignFilter = hasModifierAssignFilter;
        ModifierRoleId = modifierRoleId;
        ModifierAssignFilter = modifierAssignFilter ?? new();
        ModifierAssignFilterTeam = modifierAssignFilterTeam ?? Array.Empty<AssignedTeamType>();
        ModifierDoNotAssignRoles = modifierDoNotAssignRoles ?? Array.Empty<RoleId>();
    }

    private static void RegisterCategory(CustomOptionCategory category)
    {
        CustomOptionManager.RegisterOptionCategory(category);
    }

    public void AddOption(CustomOption option)
    {
        if (!_options.Contains(option))
        {
            _options.Add(option);
            Options = _options.ToArray();
        }
    }
}

public interface ICustomOptionCategory
{
    string CategoryName { get; }
}

public class ExclusivityData
{
    public int MaxAssign { get; set; }
    public List<RoleId> Roles { get; set; }

    public ExclusivityData(int maxAssign, string[] roles)
    {
        MaxAssign = maxAssign;
        Roles = roles.Select(role => Enum.Parse<RoleId>(role)).ToList();
    }
}