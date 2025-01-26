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
                    CustomOption opt = new CustomOption(attribute, field);
                    CustomOptions.Add(opt);
                    opt.UpdateValue(attribute.GenerateDefaultSelection());
                }
            }
        }
    }
}

public class CustomOption
{
    public CustomOptionBaseAttribute Attribute { get; }
    public FieldInfo FieldInfo { get; }
    public object Value { get; private set; }
    public object Selection { get; private set; }
    public string Id => Attribute.Id;
    public string[] Selections => _selections ??= Attribute.GenerateSelections();
    private string[] _selections;

    public CustomOption(CustomOptionBaseAttribute attribute, FieldInfo fieldInfo)
    {
        Attribute = attribute;
        FieldInfo = fieldInfo;
    }

    public void UpdateValue(object value)
    {
        if (!ValidateValue(value))
        {
            Logger.Error($"Invalid value type: {value.GetType()}");
            return;
        }
        if (Attribute.OptionType == CustomOptionType.String)
        {
            Selection = value;
        }
        else
        {
            byte selec = 0;
            foreach (var selection in Selections)
            {
                if (selection == value.ToString())
                {
                    Selection = selec;
                    break;
                }
                selec++;
            }
        }
        Value = value;
        FieldInfo.SetValue(this.FieldInfo, Value);
        // TODO: 後で設定画面を閉じる所に移動する
        if (CustomOptionSaver.Loaded)
            CustomOptionSaver.Save();
    }

    private bool ValidateValue(object value) => Attribute.OptionType switch
    {
        CustomOptionType.Float => value is float,
        CustomOptionType.Int => value is int,
        CustomOptionType.Bool => value is bool,
        CustomOptionType.String => value is string,
        CustomOptionType.Byte => value is byte,
        CustomOptionType.Select => value is Enum,
        _ => false
    };
}
public static class CustomOptionSaver
{
    static readonly DirectoryInfo directory = new("./SuperNewRolesNext/SaveData/");
    public static readonly string OptionSaverFileName = $"{directory.FullName}/Options.{Extension}";
    public const string Extension = "data";
    public static readonly string PresetFileNameBase = $"{directory.FullName}/PresetOptions_";
    public const byte Version = 0;
    public static object FileLocker = new();
    public static bool Loaded { get; private set; } = false;
    public static void Load()
    {
        Logger.Info($"{directory.FullName}");
        if (!directory.Exists)
        {
            directory.Create();
            directory.Attributes |= FileAttributes.Hidden;
        }
        ReadAndSetOption();
        Loaded = true;
    }
    private static void ReadAndSetOption()
    {
        var (successed, version, preset) = LoadOptionDotData();
        if (!successed)
        {
            return;
        }
        if (version != Version)
        {
            switch (version)
            {
                default:
                    Logger.Error($"Unsupported version: {version}");
                    break;
            }
            return;
        }
        var (successed2, options) = LoadPresetData(preset);
        if (!successed2)
        {
            return;
        }
        foreach (var option in CustomOptionManager.GetCustomOptions())
        {
            if (options.TryGetValue(option.Id, out var value))
            {
                switch (option.Attribute.OptionType)
                {
                    case CustomOptionType.Select:
                        option.UpdateValue(option.Selections[(byte)value]);
                        break;
                    case CustomOptionType.String:
                        option.UpdateValue((string)value);
                        break;
                    case CustomOptionType.Float:
                        option.UpdateValue((float)value);
                        break;
                    case CustomOptionType.Int:
                        option.UpdateValue(Convert.ToInt32(value));
                        break;
                    case CustomOptionType.Bool:
                        option.UpdateValue((bool)value);
                        break;
                    case CustomOptionType.Byte:
                        option.UpdateValue((byte)value);
                        break;
                }
            }
        }
    }
    private static (bool successed, Dictionary<string, object> options) LoadPresetData(int preset)
    {
        lock (FileLocker)
        {
            string fileName = PresetFileNameBase + preset + "." + Extension;
            if (!File.Exists(fileName))
            {
                return (false, new());
            }
            using var fileStream = new FileStream(fileName, FileMode.Open);
            using var binaryReader = new BinaryReader(fileStream);
            if (!ReadCheckSum(binaryReader))
            {
                Logger.Error("Load PresetDataChecksum error");
                return (false, new());
            }
            int optionCount = binaryReader.ReadInt32();
            Dictionary<string, object> options = new();
            for (int i = 0; i < optionCount; i++)
            {
                string id = binaryReader.ReadString();
                bool isValueString = binaryReader.ReadBoolean();
                if (isValueString)
                {
                    options[id] = binaryReader.ReadString();
                }
                else
                {
                    options[id] = binaryReader.ReadByte();
                }
            }
            return (true, options);
        }
    }
    private static (bool successed, int version, int preset) LoadOptionDotData()
    {
        lock (FileLocker)
        {
            if (!File.Exists(OptionSaverFileName))
            {
                return (false, 0, 0);
            }
            using var fileStream = new FileStream(OptionSaverFileName, FileMode.Open);
            using var binaryReader = new BinaryReader(fileStream);
            byte version = binaryReader.ReadByte();
            if (!ReadCheckSum(binaryReader))
            {
                Logger.Error("Load OptionDotDataChecksum error");
                return (false, version, 0);
            }
            int preset = binaryReader.ReadInt32();
            return (true, version, preset);
        }
    }
    private static void WriteCheckSum(BinaryWriter writer)
    {
        int random = ModHelpers.GetRandomInt(15);
        writer.Write((byte)random);
        writer.Write((byte)(random * random));
    }
    private static bool ReadCheckSum(BinaryReader reader)
    {
        int random = reader.ReadByte();
        int random2 = reader.ReadByte();
        return (random * random) == random2;
    }
    public static void Save()
    {
        int Preset = 0;
        lock (FileLocker)
        {
            using var fileStream = new FileStream(OptionSaverFileName, FileMode.Create);
            using var binaryWriter = new BinaryWriter(fileStream);
            binaryWriter.Write(Version);
            WriteCheckSum(binaryWriter);
            binaryWriter.Write(Preset);
        }
        lock (FileLocker)
        {
            using var fileStream = new FileStream(PresetFileNameBase + Preset + "." + Extension, FileMode.Create);
            using var binaryWriter = new BinaryWriter(fileStream);
            WriteCheckSum(binaryWriter);
            binaryWriter.Write(CustomOptionManager.GetCustomOptions().Count);
            foreach (var option in CustomOptionManager.GetCustomOptions())
            {
                binaryWriter.Write(option.Id);
                if (option.Attribute.OptionType == CustomOptionType.String)
                {
                    binaryWriter.Write(true);
                    binaryWriter.Write(option.Value as string);
                }
                else
                {
                    binaryWriter.Write(false);
                    binaryWriter.Write((byte)option.Selection);
                }
            }
        }
    }
}
public enum CustomOptionType
{
    None,
    Float,
    Int,
    Bool,
    String,
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
            var t when t == typeof(string) => CustomOptionType.String,
            var t when t == typeof(byte) => CustomOptionType.Byte,
            _ => CustomOptionType.None
        };
    }

    public abstract string[] GenerateSelections();
    public abstract object GenerateDefaultSelection();
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

    public override string[] GenerateSelections() =>
        Selections.Select(s => s.ToString()).ToArray();

    public override object GenerateDefaultSelection() => GenerateSelections().FirstOrDefault();
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

    public override string[] GenerateSelections()
    {
        var selections = new List<string>();
        for (T s = Min; Comparer<T>.Default.Compare(s, Max) <= 0; s = Add(s, Step))
        {
            selections.Add(s.ToString());
        }
        return selections.ToArray();
    }

    public override object GenerateDefaultSelection() => DefaultValue;

    protected abstract T Add(T a, T b);
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionFloatAttribute : CustomOptionNumericAttribute<float>
{
    public CustomOptionFloatAttribute(string id, float min, float max, float step, float defaultValue, string? translationName = null)
        : base(id, min, max, step, defaultValue, translationName) { }

    protected override float Add(float a, float b) => a + b;
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionIntAttribute : CustomOptionNumericAttribute<int>
{
    public CustomOptionIntAttribute(string id, int min, int max, int step, int defaultValue, string? translationName = null)
        : base(id, min, max, step, defaultValue, translationName) { }

    protected override int Add(int a, int b) => a + b;
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionByteAttribute : CustomOptionNumericAttribute<byte>
{
    public CustomOptionByteAttribute(string id, byte min, byte max, byte step, byte defaultValue, string? translationName = null)
        : base(id, min, max, step, defaultValue, translationName) { }

    protected override byte Add(byte a, byte b) => (byte)(a + b);
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

    public override string[] GenerateSelections() =>
        [ModTranslation.GetString("CustomOptionFalse"), ModTranslation.GetString("CustomOptionTrue")];

    public override object GenerateDefaultSelection() => DefaultValue;
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionStringAttribute : CustomOptionBaseAttribute
{
    public string DefaultValue { get; }

    public CustomOptionStringAttribute(string id, string defaultValue, string? translationName = null)
        : base(id, translationName)
    {
        DefaultValue = defaultValue;
    }

    public override string[] GenerateSelections() => [DefaultValue];

    public override object GenerateDefaultSelection() => 0;
}
#nullable disable
