using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Modules;

public static class CustomOptionManager
{
    [CustomOptionInt("TestInt", 0, 100, 1, 5)]
    public static int TestInt;
    private static Dictionary<string, CustomOptionBaseAttribute> CustomOptionAttributes { get; } = new();
    private static List<CustomOption> CustomOptions { get; } = new();
    public static IReadOnlyList<CustomOption> GetCustomOptions() => CustomOptions.AsReadOnly();

    public static void Load()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var field in type.GetFields())
            {
                var attribute = field.GetCustomAttribute<CustomOptionBaseAttribute>();
                if (attribute != null)
                {
                    CustomOptionAttributes[field.Name] = attribute;
                    attribute.SetFieldInfo(field);
                    CustomOption opt = new(attribute, field);
                    CustomOptions.Add(opt);
                    opt.UpdateSelection(attribute.GenerateDefaultSelection());
                }
            }
        }
    }
}

public class CustomOption
{
    public CustomOptionBaseAttribute Attribute { get; }
    public FieldInfo FieldInfo { get; }
    private object _value;
    private object _selection;

    public object Value => _value;
    public object Selection => _selection;
    public string Id => Attribute.Id;
    public object[] Selections;

    public CustomOption(CustomOptionBaseAttribute attribute, FieldInfo fieldInfo)
    {
        Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
        FieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
        Selections = attribute.GenerateSelections();
        var defaultValue = attribute.GenerateDefaultSelection();
        UpdateSelection(defaultValue);
    }

    public void UpdateSelection(byte value)
    {
        try
        {
            _selection = value;
            _value = Selections[value];
            FieldInfo.SetValue(null, _value);
            if (CustomOptionSaver.Loaded)
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

    private bool ValidateValue(object value) => Attribute.OptionType switch
    {
        CustomOptionType.Float => value is float,
        CustomOptionType.Int => value is int,
        CustomOptionType.Bool => value is bool,
        CustomOptionType.Byte => value is byte,
        CustomOptionType.Select => value.GetType().IsEnum,
        _ => false
    };

    private string GetExpectedType() => Attribute.OptionType switch
    {
        CustomOptionType.Float => "float",
        CustomOptionType.Int => "int",
        CustomOptionType.Bool => "bool",
        CustomOptionType.Byte => "byte",
        CustomOptionType.Select => "enum",
        _ => "unknown"
    };
}
public static class CustomOptionSaver
{
    private static readonly IOptionStorage Storage;
    private const byte CurrentVersion = 0;
    public static bool Loaded { get; private set; } = false;

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
            ReadAndSetOption();
            Loaded = true;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load options: {ex.Message}");
        }
    }

    private static void ReadAndSetOption()
    {
        var (success, version, preset) = Storage.LoadOptionData();
        if (!success || version != CurrentVersion)
        {
            Logger.Error($"Failed to load option data or unsupported version: {version}");
            return;
        }

        var (optionsSuccess, options) = Storage.LoadPresetData(preset);
        if (!optionsSuccess)
        {
            Logger.Error("Failed to load preset data");
            return;
        }

        ApplyOptions(options);
    }

    private static void ApplyOptions(Dictionary<string, byte> options)
    {
        foreach (var option in CustomOptionManager.GetCustomOptions())
        {
            if (options.TryGetValue(option.Id, out var value))
            {
                try
                {
                    option.UpdateSelection(value);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to update option {option.Id}: {ex.Message}");
                }
            }
        }
    }

    private static object ConvertOptionValue(CustomOptionType optionType, object value) => optionType switch
    {
        CustomOptionType.Select => value,
        CustomOptionType.Float => Convert.ToSingle(value),
        CustomOptionType.Int => Convert.ToInt32(value),
        CustomOptionType.Bool => Convert.ToBoolean(value),
        CustomOptionType.Byte => Convert.ToByte(value),
        _ => throw new ArgumentException($"Unsupported option type: {optionType}")
    };

    public static void Save()
    {
        try
        {
            const int CurrentPreset = 0;
            Storage.SaveOptionData(CurrentVersion, CurrentPreset);
            Storage.SavePresetData(CurrentPreset, CustomOptionManager.GetCustomOptions());
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to save options: {ex.Message}");
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

    protected CustomOptionBaseAttribute(string id, string? translationName = null)
    {
        Id = ComputeMD5Hash.Compute(id);
        TranslationName = translationName ?? id;
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

    public CustomOptionSelectAttribute(string id, Enum[] selections, string? translationName = null)
        : base(id, translationName)
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

    protected CustomOptionNumericAttribute(string id, T min, T max, T step, T defaultValue, string? translationName = null)
        : base(id, translationName)
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
    public CustomOptionFloatAttribute(string id, float min, float max, float step, float defaultValue, string? translationName = null)
        : base(id, min, max, step, defaultValue, translationName) { }

    protected override float Add(float a, float b) => a + b;
    public override byte GenerateDefaultSelection() => (byte)(DefaultValue / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionIntAttribute : CustomOptionNumericAttribute<int>
{
    public CustomOptionIntAttribute(string id, int min, int max, int step, int defaultValue, string? translationName = null)
        : base(id, min, max, step, defaultValue, translationName) { }

    protected override int Add(int a, int b) => a + b;
    public override byte GenerateDefaultSelection() => (byte)(DefaultValue / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionByteAttribute : CustomOptionNumericAttribute<byte>
{
    public CustomOptionByteAttribute(string id, byte min, byte max, byte step, byte defaultValue, string? translationName = null)
        : base(id, min, max, step, defaultValue, translationName) { }

    protected override byte Add(byte a, byte b) => (byte)(a + b);
    public override byte GenerateDefaultSelection() => (byte)(DefaultValue / Step);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionBoolAttribute : CustomOptionBaseAttribute
{
    public bool DefaultValue { get; }

    public CustomOptionBoolAttribute(string id, bool defaultValue, string? translationName = null)
        : base(id, translationName)
    {
        DefaultValue = defaultValue;
    }

    public override object[] GenerateSelections() =>
        [false, true];

    public override byte GenerateDefaultSelection() => (byte)(DefaultValue ? 1 : 0);
}
