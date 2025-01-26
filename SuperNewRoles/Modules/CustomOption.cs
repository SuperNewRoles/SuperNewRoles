using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SuperNewRoles.Modules;

public static class CustomOptionManager
{
    private static Dictionary<string, CustomOptionBaseAttribute> CustomOptionAttributes = new();
    private static List<CustomOption> _customOptios = new();
    public static IReadOnlyList<CustomOption> CustomOptions => _customOptios;
    private static Dictionary<int, CustomOption> _customOptionIds = new();
    public static void Load()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var field in type.GetFields())
            {
                if (field.GetCustomAttribute(typeof(CustomOptionBaseAttribute)) is CustomOptionBaseAttribute attribute)
                {
                    CustomOptionAttributes[field.Name] = attribute;
                    attribute.SetFieldInfo(field);
                    _customOptios.Add(new CustomOption(attribute, field));
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
    public int Id { get; }
    public string[] selections { get; }
    public CustomOption(CustomOptionBaseAttribute attribute, FieldInfo fieldInfo)
    {
        Attribute = attribute;
        FieldInfo = fieldInfo;
        Id = attribute.Id;
    }
    public void UpdateValue(object value)
    {
        if (!ValidateValue(value))
        {
            Logger.Error($"Invalid value type: {value.GetType()}");
            return;
        }
        Value = value;
        FieldInfo.SetValue(this.FieldInfo, this.Value);
    }
    private bool ValidateValue(object value)
    {
        if (Attribute.OptionType == CustomOptionType.Float)
        {
            return value is float;
        }
        else if (Attribute.OptionType == CustomOptionType.Int)
        {
            return value is int;
        }
        else if (Attribute.OptionType == CustomOptionType.Bool)
        {
            return value is bool;
        }
        else if (Attribute.OptionType == CustomOptionType.String)
        {
            return value is string;
        }
        else if (Attribute.OptionType == CustomOptionType.Byte)
        {
            return value is byte;
        }
        else if (Attribute.OptionType == CustomOptionType.Select)
        {
            return value is Enum;
        }
        else
        {
            Logger.Error($"Invalid value type: {value.GetType()}");
            return false;
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
# nullable enable
// CustomOptionFloat Attribute
[AttributeUsage(AttributeTargets.Field)]
public abstract class CustomOptionBaseAttribute : Attribute
{
    public int Id { get; }
    [AllowNull]
    public FieldInfo fieldInfo { get; private set; }
    public string TranslationName { get; }
    public CustomOptionType OptionType { get; private set; } = CustomOptionType.None;
    public CustomOptionBaseAttribute(string id, string? translationName = null)
    {
        Id = id.GetHashCode();
        if (this.TranslationName == null)
            this.TranslationName = id;
        else
            this.TranslationName = translationName!;
    }
    public void SetFieldInfo(FieldInfo fieldInfo)
    {
        this.fieldInfo = fieldInfo;
        OptionType = fieldInfo.FieldType.IsEnum ? CustomOptionType.Select :
            fieldInfo.FieldType == typeof(float) ? CustomOptionType.Float :
            fieldInfo.FieldType == typeof(int) ? CustomOptionType.Int :
            fieldInfo.FieldType == typeof(bool) ? CustomOptionType.Bool :
            fieldInfo.FieldType == typeof(string) ? CustomOptionType.String :
            fieldInfo.FieldType == typeof(byte) ? CustomOptionType.Byte :
            CustomOptionType.None;
    }
    public abstract string[] GenerateSelections();
    public abstract ushort GenerateDefaultSelection();
}
[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionSelectAttribute : CustomOptionBaseAttribute
{
    public Enum[] Selections { get; }
    public CustomOptionSelectAttribute(string id, Enum[] selections, string? translationName = null) : base(id, translationName)
    {
        Selections = selections;
    }
    public override string[] GenerateSelections()
    {
        var selectionStrings = new List<string>();
        foreach (var selection in Selections)
        {
            selectionStrings.Add(selection.ToString());
        }
        return selectionStrings.ToArray();
    }
    public override ushort GenerateDefaultSelection()
    {
        return 0;
    }
}
[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionFloatAttribute : CustomOptionBaseAttribute
{
    public float Min { get; }
    public float Max { get; }
    public float Step { get; }
    public float DefaultValue { get; }

    public CustomOptionFloatAttribute(string id, float min, float max, float step, float defaultValue, string? translationName = null)
        : base(id, translationName)
    {
        Min = min;
        Max = max;
        Step = step;
        DefaultValue = defaultValue;
    }
    public override string[] GenerateSelections()
    {
        List<string> selections = new();
        for (float s = Min; s <= Max; s += Step)
            selections.Add(s.ToString());
        return selections.ToArray();
    }
    public override ushort GenerateDefaultSelection()
    {
        return (ushort)(DefaultValue / Step);
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionIntAttribute : CustomOptionBaseAttribute
{
    public int Min { get; }
    public int Max { get; }
    public int Step { get; }
    public int DefaultValue { get; }

    public CustomOptionIntAttribute(string id, int min, int max, int step, int defaultValue, string? translationName = null)
        : base(id, translationName)
    {
        Min = min;
        Max = max;
        Step = step;
        DefaultValue = defaultValue;
    }
    public override string[] GenerateSelections()
    {
        List<string> selections = new();
        for (int s = Min; s <= Max; s += Step)
            selections.Add(s.ToString());
        return selections.ToArray();
    }
    public override ushort GenerateDefaultSelection()
    {
        return (ushort)(DefaultValue / Step);
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class CustomOptionByteAttribute : CustomOptionBaseAttribute
{
    public byte Min { get; }
    public byte Max { get; }
    public byte Step { get; }
    public byte DefaultValue { get; }

    public CustomOptionByteAttribute(string id, byte min, byte max, byte step, byte defaultValue, string? translationName = null)
        : base(id, translationName)
    {
        Min = min;
        Max = max;
        Step = step;
        DefaultValue = defaultValue;
    }
    public override string[] GenerateSelections()
    {
        List<string> selections = new();
        for (byte s = Min; s <= Max; s += Step)
            selections.Add(s.ToString());
        return selections.ToArray();
    }
    public override ushort GenerateDefaultSelection()
    {
        return (ushort)(DefaultValue / Step);
    }
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
    public override string[] GenerateSelections()
    {
        return [ModTranslation.GetString("CustomOptionFalse"), ModTranslation.GetString("CustomOptionTrue")];
    }
    public override ushort GenerateDefaultSelection()
    {
        return (ushort)(DefaultValue ? 1 : 0);
    }
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
    public override string[] GenerateSelections()
    {
        return [DefaultValue];
    }
    public override ushort GenerateDefaultSelection()
    {
        return 0;
    }
}

#nullable disable
