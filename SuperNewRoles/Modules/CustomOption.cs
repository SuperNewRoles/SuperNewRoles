using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

public static class CustomOptionManager
{
    [CustomOptionInt("TestInt", 0, 100, 1, 5)]
    public static int TestInt;
    private static Dictionary<string, CustomOptionBaseAttribute> CustomOptionAttributes { get; } = new();
    private static List<CustomOption> CustomOptions { get; } = new();
    public static IReadOnlyList<CustomOption> GetCustomOptions() => CustomOptions.AsReadOnly();

    // カスタムオプションをロードするメソッド
    public static void Load()
    {
        // 実行中のアセンブリ内のすべての型を取得
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            // 各型のフィールドを取得
            foreach (var field in type.GetFields())
            {
                RoleId? role = null;
                // フィールドにカスタムオプション属性があるか確認
                var attribute = field.GetCustomAttribute<CustomOptionBaseAttribute>();
                if (attribute != null)
                {
                    // カスタムオプション属性を辞書に追加
                    CustomOptionAttributes[field.Name] = attribute;
                    // フィールド情報を設定
                    attribute.SetFieldInfo(field);
                    // フィールドがIRoleBaseインターフェースを実装しているか確認
                    if (field.DeclaringType.GetInterfaces().Contains(typeof(IRoleBase)))
                    {
                        // 基底シングルトン型を取得
                        var baseSingletonType = typeof(BaseSingleton<>).MakeGenericType(field.DeclaringType);
                        // インスタンスプロパティを取得
                        var instanceProperty = baseSingletonType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        // インスタンスを取得
                        var roleInstance = instanceProperty.GetValue(null);
                        // 役職を取得
                        role = ((IRoleBase)roleInstance).Role;
                    }
                    // カスタムオプションを作成し、リストに追加
                    CustomOption opt = new(attribute, field, role);
                    CustomOptions.Add(opt);
                    // デフォルトの値に更新
                    opt.UpdateSelection(attribute.GenerateDefaultSelection());
                }
            }
        }

        // 追加：各オプションについて、属性で指定された親フィールド名があれば関連付ける
        foreach (var option in CustomOptions)
        {
            if (!string.IsNullOrEmpty(option.Attribute.ParentFieldName))
            {
                // 指定されたフィールド名と一致するオプションを親として検索
                var parentOption = CustomOptions.FirstOrDefault(o => o.FieldInfo.Name == option.Attribute.ParentFieldName);
                if (parentOption != null)
                {
                    option.SetParentOption(parentOption);
                    Logger.Info($"親オプションを設定: {option.Name} -> {parentOption.Name}");
                }
                else
                {
                    Logger.Warning($"親オプションが見つかりませんでした: {option.Attribute.ParentFieldName}");
                }
            }
        }
    }
}

public class CustomOption
{
    public CustomOptionBaseAttribute Attribute { get; }
    public FieldInfo FieldInfo { get; }
    public string Name { get; }
    private object _value;
    private object _selection;

    public object Value => _value;
    public object Selection => _selection;
    public string Id => Attribute.Id;
    public object[] Selections { get; }
    public RoleId? ParentRole { get; private set; }
    public CustomOption? ParentOption { get; private set; }
    public CustomOption(CustomOptionBaseAttribute attribute, FieldInfo fieldInfo, RoleId? parentRole = null)
    {
        Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        FieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
        Selections = attribute.GenerateSelections();
        var defaultValue = attribute.GenerateDefaultSelection();
        UpdateSelection(defaultValue);
        Name = attribute.TranslationName;
    }

    public void UpdateSelection(byte value)
    {
        if (value >= Selections.Length)
        {
            Logger.Warning($"Invalid selection value {value} for option {Id}. Using default.");
            value = 0;
        }

        try
        {
            _selection = value;
            _value = Selections[value];
            FieldInfo.SetValue(null, _value);

            if (CustomOptionSaver.IsLoaded)
            {
                CustomOptionSaver.Save();
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
    }
}
public static class RoleOptionManager
{
    public class RoleOption
    {
        public RoleId RoleId { get; }
        public byte NumberOfCrews { get; }
        public CustomOption[] Options { get; }
        public RoleOption(RoleId roleId, byte numberOfCrews, CustomOption[] options)
        {
            RoleId = roleId;
            NumberOfCrews = numberOfCrews;
            Options = options;
        }
    }
    public static RoleOption[] RoleOptions { get; private set; }
    public static void Load()
    {
        RoleOptions = CustomRoleManager.AllRoles
        .Select(role =>
        {
            var options = CustomOptionManager.GetCustomOptions()
            .Where(option => option.ParentRole == role.Role)
            .ToArray();
            return new RoleOption(role.Role, 0, options);
        }).ToArray();
    }
}
public static class CustomOptionSaver
{
    private static readonly IOptionStorage Storage;
    private const byte CurrentVersion = 1;
    private const int CurrentPreset = 0;
    public static bool IsLoaded { get; private set; } = false;

    static CustomOptionSaver()
    {
        Storage = new FileOptionStorage(
            new DirectoryInfo("./SuperNewRolesNext/SaveData/"),
            "Options.data",
            "PresetOptions_"
        );
    }

    public static void Load()
    {
        try
        {
            Storage.EnsureStorageExists();
            ReadAndApplyOptions();
            IsLoaded = true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Option loading failed: {ex.Message}");
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

            return (true, ReadOptions(reader));
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
        var optionsList = options.ToList();
        writer.Write(optionsList.Count);

        foreach (var option in optionsList)
        {
            writer.Write(option.Id);
            writer.Write((byte)option.Selection);
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
    private static MD5 md5 = MD5.Create();
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

    protected CustomOptionBaseAttribute(string id, string? translationName = null, string? parentFieldName = null)
    {
        Id = ComputeMD5Hash.Compute(id);
        TranslationName = translationName ?? id;
        ParentFieldName = parentFieldName;
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
    public Enum[] Selections { get; }

    public CustomOptionSelectAttribute(string id, Enum[] selections, string? translationName = null, string? parentFieldName = null)
        : base(id, translationName, parentFieldName)
    {
        Selections = selections;
    }

    public override object[] GenerateSelections() => Selections.Select(s => (object)s).ToArray();

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

    protected CustomOptionNumericAttribute(string id, T min, T max, T step, T defaultValue, string? translationName = null, string? parentFieldName = null)
        : base(id, translationName, parentFieldName)
    {
        Min = min;
        Max = max;
        Step = step;
        DefaultValue = defaultValue;
    }

    public override object[] GenerateSelections()
    {
        var selections = new List<object>();
        for (T s = Min; Comparer<T>.Default.Compare(s, Max) <= 0; s = Add(s, Step))
        {
            selections.Add(s);
        }
        return selections.ToArray();
    }

    protected abstract T Add(T a, T b);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionFloatAttribute : CustomOptionNumericAttribute<float>
{
    public CustomOptionFloatAttribute(string id, float min, float max, float step, float defaultValue, string? translationName = null, string? parentFieldName = null)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName) { }

    protected override float Add(float a, float b) => a + b;
    public override byte GenerateDefaultSelection() => (byte)(DefaultValue / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionIntAttribute : CustomOptionNumericAttribute<int>
{
    public CustomOptionIntAttribute(string id, int min, int max, int step, int defaultValue, string? translationName = null, string? parentFieldName = null)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName) { }

    protected override int Add(int a, int b) => a + b;
    public override byte GenerateDefaultSelection() => (byte)(DefaultValue / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionByteAttribute : CustomOptionNumericAttribute<byte>
{
    public CustomOptionByteAttribute(string id, byte min, byte max, byte step, byte defaultValue, string? translationName = null, string? parentFieldName = null)
        : base(id, min, max, step, defaultValue, translationName, parentFieldName) { }

    protected override byte Add(byte a, byte b) => (byte)(a + b);
    public override byte GenerateDefaultSelection() => (byte)(DefaultValue / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionBoolAttribute : CustomOptionBaseAttribute
{
    public bool DefaultValue { get; }

    public CustomOptionBoolAttribute(string id, bool defaultValue, string? translationName = null, string? parentFieldName = null)
        : base(id, translationName, parentFieldName)
    {
        DefaultValue = defaultValue;
    }

    public override object[] GenerateSelections() =>
        [false, true];

    public override byte GenerateDefaultSelection() => (byte)(DefaultValue ? 1 : 0);
}
