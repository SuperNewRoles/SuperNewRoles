using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using HarmonyLib;
using SuperNewRoles.Roles;
using UnityEngine;
namespace SuperNewRoles.Modules;

public static class CustomOptionManager
{
    public static CustomOptionCategory PresetSettings;
    public static CustomOptionCategory GeneralSettings;
    public static CustomOptionCategory ModeSettings;
    public static CustomOptionCategory GameSettings;
    public static CustomOptionCategory MapSettings;
    public static CustomOptionCategory MapEditSettings;

    [CustomOptionSelect("ModeOption", typeof(ModeId), "ModeId.", parentFieldName: nameof(ModeSettings))]
    public static ModeId ModeOption;

    [CustomOptionBool("DebugMode", false, parentFieldName: nameof(GeneralSettings))]
    public static bool DebugMode;
    [CustomOptionBool("DebugModeNoGameEnd", false, parentFieldName: nameof(DebugMode))]
    public static bool DebugModeNoGameEnd;
    [CustomOptionBool("SkipStartGameCountdown", false, parentFieldName: nameof(GeneralSettings))]
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
            if (AmongUsClient.Instance.AmHost)
            {
                RpcSyncOptionsAll();
                RoleOptionManager.RpcSyncRoleOptionsAll();
            }
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
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var field in type.GetFields())
            {
                // カテゴリーフィールドの場合、staticフィールドのみを対象にする
                if (field.IsStatic && field.FieldType == typeof(CustomOptionCategory))
                {
                    if (!fieldNames.Add(field.Name))
                    {
                        throw new InvalidOperationException($"フィールド名が重複しています: {field.Name}");
                    }
                    var category = new CustomOptionCategory(field.Name);
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
                    throw new InvalidOperationException($"フィールド名が重複しています: {field.Name}");
                }

                // カスタムオプション属性を辞書に追加
                CustomOptionAttributes[field.Name] = attribute;
                // フィールド情報を設定
                attribute.SetFieldInfo(field);
                RoleId? role = null;
                if (field.DeclaringType.GetInterfaces().Contains(typeof(IRoleBase)))
                {
                    var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(field.DeclaringType);
                    var instanceProperty = baseSingletonType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    var roleInstance = instanceProperty.GetValue(null);
                    role = ((IRoleBase)roleInstance).Role;
                }
                // カスタムオプションを作成し、リストに追加
                CustomOption option = new(attribute, field, role);
                CustomOptions.Add(option);
            }
        }
        Logger.Info("CustomOptions");
        foreach (var option in CustomOptions)
        {
            Logger.Info($"option: {option.Name}");
        }
    }

    // 属性で指定された親フィールド名があるオプションについて、親オプションと紐付ける処理
    private static void LinkParentOptions()
    {
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
        if (Attribute is CustomOptionFloatAttribute floatAttribute)
        {
            float step = floatAttribute.Step;
            if (step >= 1f) return string.Format("{0:F0}", Value);
            else if (step >= 0.1f) return string.Format("{0:F1}", Value);
            else return string.Format("{0:F2}", Value);
        }
        else if (Attribute is CustomOptionSelectAttribute selectAttr)
        {
            return ModTranslation.GetString($"{selectAttr.TranslationPrefix}{Selections[Selection]}");
        }
        return Selections[Selection].ToString();
    }

    public CustomOption(CustomOptionBaseAttribute attribute, FieldInfo fieldInfo, RoleId? parentRole = null, bool isTaskOption = false)
    {
        Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        FieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));

        Selections = attribute.GenerateSelections();
        var defaultSelection = attribute.GenerateDefaultSelection();
        Name = attribute.TranslationName;
        ParentRole = parentRole;
        IsBooleanOption = attribute is CustomOptionBoolAttribute;
        DisplayMode = attribute.DisplayMode;
        _defaultValue = Selections[defaultSelection];
        _defaultSelection = defaultSelection;
        IsTaskOption = isTaskOption;
        UpdateSelection(defaultSelection);
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
                    // ホストの場合は、変更を他のプレイヤーに同期
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                    {
                        RpcSyncRoleOption(RoleId, _numberOfCrews_My, _percentage_My);
                    }
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
                    if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                    {
                        RpcSyncRoleOption(RoleId, _numberOfCrews_My, _percentage_My);
                    }
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
    public static RoleOption[] RoleOptions { get; private set; }
    public static List<ExclusivityData> ExclusivitySettings { get; private set; } = new();

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

    public static void RpcSyncRoleOptionsAll()
    {
        // 変更されたロールオプションだけを送信
        var roleOptions = RoleOptions.ToDictionary(
            o => (byte)o.RoleId,
            o => (o.NumberOfCrews, o.Percentage));
        _RpcSyncRoleOptionsAll(roleOptions);
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
                    .Where(option => option.ParentRole == role.Role)
                    .OrderBy(option => customOptions.IndexOf(option))  // CustomOptionsの順序を維持
                    .ToArray();
                return new RoleOption(role.Role, 0, 0, optionsForRole);
            })
            .ToArray();
    }

    public static void AddExclusivitySetting(int maxAssign, string[] roles)
    {
        ExclusivitySettings.Add(new ExclusivityData(maxAssign, roles));
    }

    public static void ClearExclusivitySettings()
    {
        ExclusivitySettings.Clear();
    }
}

public static class CustomOptionSaver
{
    private static readonly IOptionStorage Storage;
    private const byte CurrentVersion = 1;
    private static int currentPreset = 0;
    public static bool IsLoaded { get; private set; } = false;
    private static Dictionary<int, string> presetNames = new();
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
            new DirectoryInfo("./SuperNewRolesNext/SaveData/"),
            "Options.data",
            "PresetOptions_"
        );
    }

    public static void SaverLoad()
    {
        try
        {
            Storage.EnsureStorageExists();
            ReadAndApplyOptions();
            LoadPresetNames();
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Option loading failed: {ex.Message}");
        }

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

    private static void ReadAndApplyOptions()
    {
        var (success, version, preset) = Storage.LoadOptionData();
        if (!success || version != CurrentVersion)
        {
            Logger.Warning($"Invalid option data version: {version}. Using default settings.");
            return;
        }

        var (optionsSuccess, options) = Storage.LoadPresetData(preset);
        if (!optionsSuccess)
        {
            Logger.Warning("Preset data loading failed. Using default settings.");
            return;
        }

        ApplyLoadedOptions(options);
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
    private readonly Dictionary<int, string> presetNames = new();

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
                return (false, 0, 0);
            }

            using var fileStream = new FileStream(_optionFileName, FileMode.Open);
            using var reader = new BinaryReader(fileStream);

            byte version = reader.ReadByte();
            if (!ValidateChecksum(reader))
            {
                return (false, version, 0);
            }

            int preset = reader.ReadInt32();

            // プリセット名を読み込む
            presetNames.Clear();
            int nameCount = reader.ReadInt32();
            for (int i = 0; i < nameCount; i++)
            {
                int presetId = reader.ReadInt32();
                string name = reader.ReadString();
                presetNames[presetId] = name;
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

            // RoleOptionの情報が存在すれば読み込む
            if (fileStream.Position < fileStream.Length)
            {
                int roleOptionCount = reader.ReadInt32();
                for (int i = 0; i < roleOptionCount; i++)
                {
                    string roleIdStr = reader.ReadString();
                    byte numberOfCrews = reader.ReadByte();
                    int percentage = reader.ReadInt32();

                    // RoleIdの文字列から RoleId 列挙体へ変換
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

                // 排他設定の読み込み
                if (fileStream.Position < fileStream.Length)
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
            }

            return (true, options);
        }
    }

    public (bool success, Dictionary<int, string> names) LoadPresetNames()
    {
        return (true, presetNames);
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
            writer.Write(presetNames.Count);
            foreach (KeyValuePair<int, string> pair in presetNames)
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

            // RoleOptionのデータを書き出す
            var roleOptions = RoleOptionManager.RoleOptions;
            writer.Write(roleOptions.Length);

            foreach (var roleOption in roleOptions)
            {
                writer.Write(roleOption.RoleId.ToString());
                writer.Write(roleOption.NumberOfCrews);
                writer.Write(roleOption.Percentage);
            }

            // 排他設定の書き出し
            var exclusivitySettings = RoleOptionManager.ExclusivitySettings;
            writer.Write(exclusivitySettings.Count);
            foreach (var setting in exclusivitySettings)
            {
                writer.Write(setting.MaxAssign);
                writer.Write(setting.Roles.Length);
                foreach (var role in setting.Roles)
                {
                    writer.Write(role);
                }
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

    protected CustomOptionBaseAttribute(string id, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All)
    {
        Id = ComputeMD5Hash.Compute(id);
        TranslationName = translationName ?? id;
        ParentFieldName = parentFieldName;
        DisplayMode = displayMode;
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
    public string TranslationPrefix { get; }

    public CustomOptionSelectAttribute(string id, Type enumType, string translationPrefix, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All)
        : base(id, translationName, parentFieldName, displayMode)
    {
        if (!enumType.IsEnum) throw new ArgumentException("Type must be an enum", nameof(enumType));
        _enumType = enumType;
        _selectionNames = Enum.GetNames(enumType);
        TranslationPrefix = translationPrefix;
    }

    public override object[] GenerateSelections() =>
        _selectionNames.Select(name => Enum.Parse(_enumType, name)).ToArray();

    public override byte GenerateDefaultSelection() => 0;
}

[AttributeUsage(AttributeTargets.Field)]
public abstract class CustomOptionNumericAttribute<T> : CustomOptionBaseAttribute
    where T : struct, IComparable<T>
{
    public T Min { get; }
    public T Max { get; }
    public T Step { get; }
    public T DefaultValue { get; }
    public event Action<T> ValueChanged;

    protected CustomOptionNumericAttribute(string id, T min, T max, T step, T defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All)
        : base(id, translationName, parentFieldName, displayMode)
    {
        Min = min;
        Max = max;
        Step = step;
        DefaultValue = defaultValue;
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
    public CustomOptionFloatAttribute(string id, float min, float max, float step, float defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName, displayMode) { }

    protected override float Add(float a, float b) => a + b;
    public override byte GenerateDefaultSelection() => (byte)((DefaultValue - Min) / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionIntAttribute : CustomOptionNumericAttribute<int>
{
    public CustomOptionIntAttribute(string id, int min, int max, int step, int defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName, displayMode) { }

    protected override int Add(int a, int b) => a + b;
    public override byte GenerateDefaultSelection() => (byte)((DefaultValue - Min) / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionByteAttribute : CustomOptionNumericAttribute<byte>
{
    public CustomOptionByteAttribute(string id, byte min, byte max, byte step, byte defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName, displayMode) { }

    protected override byte Add(byte a, byte b) => (byte)(a + b);
    public override byte GenerateDefaultSelection() => (byte)((DefaultValue - Min) / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionBoolAttribute : CustomOptionBaseAttribute
{
    public bool DefaultValue { get; }

    public CustomOptionBoolAttribute(string id, bool defaultValue, string? translationName = null, string? parentFieldName = null, DisplayModeId displayMode = DisplayModeId.All)
        : base(id, translationName, parentFieldName, displayMode)
    {
        DefaultValue = defaultValue;
    }

    public override object[] GenerateSelections() =>
        [false, true];

    public override byte GenerateDefaultSelection() => (byte)(DefaultValue ? 1 : 0);
}

public class CustomOptionCategory
{
    public string Id { get; }
    public string Name { get; }
    public List<CustomOption> Options { get; } = new();

    public CustomOptionCategory(string name)
    {
        Id = ComputeMD5Hash.Compute(name);
        Name = name; // 後でTranslationを使用して翻訳する
        RegisterCategory(this);
    }

    private static void RegisterCategory(CustomOptionCategory category)
    {
        CustomOptionManager.RegisterOptionCategory(category);
    }

    public void AddOption(CustomOption option)
    {
        if (!Options.Contains(option))
        {
            Options.Add(option);
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
    public string[] Roles { get; set; }

    public ExclusivityData(int maxAssign, string[] roles)
    {
        MaxAssign = maxAssign;
        Roles = roles;
    }
}