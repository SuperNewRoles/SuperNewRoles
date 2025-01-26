using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SuperNewRoles.Modules;

public static class CustomOptionManager
{
    [CustomOption("TestInt")]
    public static int TestInt = 0;
    private static Dictionary<string, CustomOptionAttribute> CustomOptionAttributes = new();
    private static List<CustomOption> _customOptios = new();
    public static IReadOnlyList<CustomOption> CustomOptions => _customOptios;
    private static Dictionary<int, CustomOption> _customOptionIds = new();
    public static void Load()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            foreach (var field in type.GetFields())
            {
                if (field.GetCustomAttribute(typeof(CustomOptionAttribute)) is CustomOptionAttribute attribute)
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
    public CustomOptionAttribute Attribute { get; }
    public FieldInfo FieldInfo { get; }
    public object Value { get; private set; }
    public CustomOption(CustomOptionAttribute attribute, FieldInfo fieldInfo)
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
public class CustomOptionAttribute : Attribute
{
    private int Id { get; }
    [AllowNull]
    public FieldInfo fieldInfo { get; private set; }
    public string TranslationName { get; }
    public CustomOptionType OptionType { get; private set; } = CustomOptionType.None;
    public CustomOptionAttribute(string id, string? translationName = null)
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
}
#nullable disable
