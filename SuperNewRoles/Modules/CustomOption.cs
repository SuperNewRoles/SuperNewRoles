using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;

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
                    CustomOptions.Add(new CustomOption(attribute, field));
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
        Value = value;
        FieldInfo.SetValue(this.FieldInfo, Value);
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
    public abstract ushort GenerateDefaultSelection();
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

    public override ushort GenerateDefaultSelection() => 0;
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

    public override ushort GenerateDefaultSelection() =>
        (ushort)(Convert.ToDouble(DefaultValue) / Convert.ToDouble(Step));

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

    public override ushort GenerateDefaultSelection() =>
        (ushort)(DefaultValue ? 1 : 0);
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

    public override ushort GenerateDefaultSelection() => 0;
}

#nullable disable
