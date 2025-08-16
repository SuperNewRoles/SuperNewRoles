using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

public class JsonParserTests
{
    [Fact]
    public void Parse_SimpleObjectAndArray_AllPrimitiveTypes()
    {
        var json = "{\n  \"a\": 1,\n  \"b\": true,\n  \"c\": null,\n  \"d\": \"x\",\n  \"e\": [1, 2, 3],\n  \"f\": { \"nested\": \"y\" }\n}";

        var obj = JsonParser.Parse(json).Should().BeOfType<Dictionary<string, object>>().Subject;

        obj.Should().ContainKey("a").WhoseValue.Should().BeOfType<long>().Which.Should().Be(1L);
        obj.Should().ContainKey("b").WhoseValue.Should().Be(true);
        obj.Should().ContainKey("c").WhoseValue.Should().BeNull();
        obj.Should().ContainKey("d").WhoseValue.Should().Be("x");

        var arr = obj["e"].Should().BeOfType<List<object>>().Subject;
        arr.Should().HaveCount(3);
        arr[0].Should().Be(1L);
        arr[1].Should().Be(2L);
        arr[2].Should().Be(3L);

        var nested = obj["f"].Should().BeOfType<Dictionary<string, object>>().Subject;
        nested.Should().ContainKey("nested").WhoseValue.Should().Be("y");
    }

    [Fact]
    public void Parse_WhitespaceAroundValue_Ignored()
    {
        var json = "  \n\t { \"k\" : 1 } \n\t  ";
        var obj = JsonParser.Parse(json).Should().BeOfType<Dictionary<string, object>>().Subject;
        obj.Should().ContainKey("k").WhoseValue.Should().Be(1L);
    }

    [Fact]
    public void Parse_StringEscapes_And_Unicode()
    {
        var json = "\"Hello\\n\\u263A\""; // "Hello\n☺"
        var s = JsonParser.Parse(json).Should().BeOfType<string>().Subject;
        s.Should().Be("Hello\n" + '\u263A');
    }

    [Fact]
    public void Parse_Number_VariousForms()
    {
        JsonParser.Parse("0").Should().Be(0L);
        JsonParser.Parse("-12").Should().Be(-12L);
        JsonParser.Parse("3.1415").Should().BeOfType<double>().Which.Should().BeApproximately(3.1415, 1e-12);
        JsonParser.Parse("1e2").Should().BeOfType<double>().Which.Should().Be(100.0);
        JsonParser.Parse("1.23e-2").Should().BeOfType<double>().Which.Should().BeApproximately(0.0123, 1e-12);
    }

    [Fact]
    public void Parse_Number_LargeIntegerBeyondLong_AsDecimal()
    {
        var val = JsonParser.Parse("9223372036854775808"); // long.MaxValue + 1
        val.Should().BeOfType<decimal>().Which.Should().Be(9223372036854775808M);
    }

    [Fact]
    public void Parse_InvalidNumber_LeadingZero_ShouldThrow()
    {
        var act = () => JsonParser.Parse("01");
        var ex = act.Should().Throw<JsonParseException>().Which;
        ex.Message.Should().Contain("Leading zeros");
        ex.Position.Should().Be(0);
    }

    [Fact]
    public void Parse_TrailingCommaInObject_ShouldThrow()
    {
        var act = () => JsonParser.Parse("{\"a\":1,}");
        var ex = act.Should().Throw<JsonParseException>().Which;
        ex.Message.Should().Contain("Trailing comma");
    }

    [Fact]
    public void Parse_TrailingCommaInArray_ShouldThrow()
    {
        var act = () => JsonParser.Parse("[1,]");
        var ex = act.Should().Throw<JsonParseException>().Which;
        ex.Message.Should().Contain("Trailing comma");
    }

    [Fact]
    public void Parse_InvalidEscape_ShouldThrow()
    {
        var act = () => JsonParser.Parse("\"\\x\"");
        var ex = act.Should().Throw<JsonParseException>().Which;
        ex.Message.Should().Contain("Invalid escape");
    }

    [Fact]
    public void Parse_UnterminatedString_ShouldThrow()
    {
        var act = () => JsonParser.Parse("\"abc");
        var ex = act.Should().Throw<JsonParseException>().Which;
        ex.Message.Should().Contain("unterminated string");
    }

    [Fact]
    public void Parse_String_WithUnescapedControlCharacter_ShouldThrow()
    {
        // JSONでは制御文字はエスケープ必須。ここでは実際に U+0001 を混在させる
        var json = "\"A\u0001B\"".Replace("\\u0001", "\u0001");
        var act = () => JsonParser.Parse(json);
        var ex = act.Should().Throw<JsonParseException>().Which;
        ex.Message.Should().Contain("Control characters must be escaped");
    }

    [Fact]
    public void Parse_ExtraCharactersAfterValue_ShouldThrow()
    {
        var act = () => JsonParser.Parse("truex");
        var ex = act.Should().Throw<JsonParseException>().Which;
        ex.Message.Should().Contain("Unexpected character");
    }

    [Fact]
    public void Serialize_Primitives_And_Collections()
    {
        var dict = new Dictionary<string, object>
        {
            ["a"] = 1, // int
            ["b"] = 2L, // long
            ["c"] = true,
            ["d"] = null!,
            ["e"] = "A\"B", // string with quote
            ["f"] = new List<object> { 1, "x", false },
            ["g"] = new Dictionary<string, object> { ["n"] = 10 }
        };

        var json = JsonParser.Serialize(dict);

        // 順序は挿入順に従う。エスケープや数値が期待通りかを検証
        json.Should().Be("{\"a\":1,\"b\":2,\"c\":true,\"d\":null,\"e\":\"A\\\"B\",\"f\":[1,\"x\",false],\"g\":{\"n\":10}} ".Trim());
    }

    [Fact]
    public void Serialize_String_ControlCharacters_AsUnicodeEscape()
    {
        var s = "A" + '\u0001' + "B";
        var json = JsonParser.Serialize(s);
        json.Should().Be("\"A\\u0001B\"");
    }

    [Fact]
    public void Serialize_FloatAndDouble_NaNAndInfinity_AsNull()
    {
        JsonParser.Serialize(double.NaN).Should().Be("null");
        JsonParser.Serialize(double.PositiveInfinity).Should().Be("null");
        JsonParser.Serialize(float.NegativeInfinity).Should().Be("null");
    }

    [Fact]
    public void RoundTrip_ParseThenSerialize_MinifiesAndPreservesData()
    {
        var original = "{ \n  \"a\" : [1, 2, 3], \n \"b\" : { \"x\" : \"y\" } \n }";
        var parsed = JsonParser.Parse(original);
        var minified = JsonParser.Serialize(parsed);

        // 最小化されるが、再パースすると元と同等の構造になる
        var reparsed = JsonParser.Parse(minified);
        reparsed.Should().BeOfType<Dictionary<string, object>>();

        var obj = reparsed as Dictionary<string, object>;
        obj!.Should().ContainKey("a");
        obj!.Should().ContainKey("b");
        var arr2 = obj!["a"].Should().BeOfType<List<object>>().Subject;
        arr2.Should().HaveCount(3);
        ((obj!["b"] as Dictionary<string, object>)!["x"]).Should().Be("y");
    }
}
