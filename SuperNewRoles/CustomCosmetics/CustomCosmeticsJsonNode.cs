using System;
using System.Collections.Generic;
using System.Globalization;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomCosmetics;

/// <summary>
/// カスタムコスメティックのメタデータを既存の低依存 JsonParser で読み取るための JSON ノード。
/// </summary>
public sealed class CustomCosmeticsJsonNode
{
    private readonly object value;
    private readonly List<object> parentArray;
    private readonly int arrayIndex;

    private CustomCosmeticsJsonNode(object value)
    {
        this.value = value;
    }

    private CustomCosmeticsJsonNode(object value, List<object> parentArray, int arrayIndex)
    {
        this.value = value;
        this.parentArray = parentArray;
        this.arrayIndex = arrayIndex;
    }

    public static CustomCosmeticsJsonNode Parse(string json)
        => new(JsonParser.Parse(json, allowComments: true, allowTrailingCommas: true));

    public CustomCosmeticsJsonNode this[string propertyName]
    {
        get
        {
            if (value is not Dictionary<string, object> properties
                || !properties.TryGetValue(propertyName, out object property))
                return null;

            return property == null ? null : new CustomCosmeticsJsonNode(property);
        }
    }

    public CustomCosmeticsJsonNode First
    {
        get
        {
            if (value is not List<object> array || array.Count == 0)
                return null;

            return new CustomCosmeticsJsonNode(array[0], array, 0);
        }
    }

    public CustomCosmeticsJsonNode Next
    {
        get
        {
            int nextIndex = arrayIndex + 1;
            if (parentArray == null || nextIndex >= parentArray.Count)
                return null;

            return new CustomCosmeticsJsonNode(parentArray[nextIndex], parentArray, nextIndex);
        }
    }

    public bool TryGetBoolean(out bool value)
    {
        if (this.value is bool boolean)
        {
            value = boolean;
            return true;
        }

        value = false;
        return false;
    }

    public override string ToString()
        => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;

    public static explicit operator bool(CustomCosmeticsJsonNode node)
    {
        if (node != null && node.TryGetBoolean(out bool value))
            return value;

        throw new InvalidCastException("JSON value is not a boolean.");
    }

    public static explicit operator int(CustomCosmeticsJsonNode node)
    {
        if (node == null)
            throw new InvalidCastException("JSON value is null.");
        if (node.value is long longValue && longValue is >= int.MinValue and <= int.MaxValue)
            return (int)longValue;
        if (node.value is int intValue)
            return intValue;
        if (node.value is string stringValue
            && int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue))
            return parsedValue;

        throw new InvalidCastException("JSON value is not a 32-bit integer.");
    }
}
