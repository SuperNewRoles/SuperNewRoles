using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using SuperNewRoles.Modules;
using Xunit;

namespace SuperNewRoles.Tests;

// 低依存の JSON パーサ/シリアライザの網羅的な挙動を検証するテスト。
public class JsonParserTests
{
    // 目的: 全てのプリミティブ型/配列/ネストしたオブジェクトの基本構文を検証
    [Fact]
    public void Parse_SimpleObjectAndArray_AllPrimitiveTypes()
    {
        var json = "{\n  \"a\": 1,\n  \"b\": true,\n  \"c\": null,\n  \"d\": \"x\",\n  \"e\": [1, 2, 3],\n  \"f\": { \"nested\": \"y\" }\n}";

        var obj = JsonParser.Parse(json).Should().BeOfType<Dictionary<string, object>>().Subject;

        // 目的: 数値(long)が正しくパースされること
        obj.Should().ContainKey("a").WhoseValue.Should().BeOfType<long>().Which.Should().Be(1L);
        // 目的: 真偽値が正しくパースされること
        obj.Should().ContainKey("b").WhoseValue.Should().Be(true);
        // 目的: null が正しくパースされること
        obj.Should().ContainKey("c").WhoseValue.Should().BeNull();
        // 目的: 文字列が正しくパースされること
        obj.Should().ContainKey("d").WhoseValue.Should().Be("x");

        var arr = obj["e"].Should().BeOfType<List<object>>().Subject;
        // 目的: 配列の要素数が正しいこと
        arr.Should().HaveCount(3);
        // 目的: 配列要素0が1であること
        arr[0].Should().Be(1L);
        // 目的: 配列要素1が2であること
        arr[1].Should().Be(2L);
        // 目的: 配列要素2が3であること
        arr[2].Should().Be(3L);

        var nested = obj["f"].Should().BeOfType<Dictionary<string, object>>().Subject;
        // 目的: ネストしたオブジェクトの値が正しいこと
        nested.Should().ContainKey("nested").WhoseValue.Should().Be("y");
    }

    // 目的: 値の前後の空白/タブ/改行が無視されることを検証
    [Fact]
    public void Parse_WhitespaceAroundValue_Ignored()
    {
        var json = "  \n\t { \"k\" : 1 } \n\t  ";
        var obj = JsonParser.Parse(json).Should().BeOfType<Dictionary<string, object>>().Subject;
        // 目的: 前後の空白/タブ/改行が無視されて値が読めること
        obj.Should().ContainKey("k").WhoseValue.Should().Be(1L);
    }

    // 目的: 文字列のエスケープと Unicode エスケープを検証
    [Fact]
    public void Parse_StringEscapes_And_Unicode()
    {
        var json = "\"Hello\\n\\u263A\""; // "Hello\n☺"
        var s = JsonParser.Parse(json).Should().BeOfType<string>().Subject;
        // 目的: \n と \uXXXX が正しく展開されること
        s.Should().Be("Hello\n" + '\u263A');
    }

    // 目的: 数値の整数/浮動小数/指数表記の解釈を検証
    [Fact]
    public void Parse_Number_VariousForms()
    {
        // 目的: 整数0が long で解釈されること
        JsonParser.Parse("0").Should().Be(0L);
        // 目的: 負の整数が long で解釈されること
        JsonParser.Parse("-12").Should().Be(-12L);
        // 目的: 小数が double で解釈されること
        JsonParser.Parse("3.1415").Should().BeOfType<double>().Which.Should().BeApproximately(3.1415, 1e-12);
        // 目的: 指数表記(正)が double で解釈されること
        JsonParser.Parse("1e2").Should().BeOfType<double>().Which.Should().Be(100.0);
        // 目的: 指数表記(負)が double で解釈されること
        JsonParser.Parse("1.23e-2").Should().BeOfType<double>().Which.Should().BeApproximately(0.0123, 1e-12);
    }

    // 目的: long を超える大きな整数は decimal として扱われることを検証
    [Fact]
    public void Parse_Number_LargeIntegerBeyondLong_AsDecimal()
    {
        var val = JsonParser.Parse("9223372036854775808"); // long.MaxValue + 1
        // 目的: long を超える整数が decimal で表現されること
        val.Should().BeOfType<decimal>().Which.Should().Be(9223372036854775808M);
    }

    // 目的: 先頭 0 の不正な数値は例外となることを検証
    [Fact]
    public void Parse_InvalidNumber_LeadingZero_ShouldThrow()
    {
        var act = () => JsonParser.Parse("01");
        var ex = act.Should().Throw<JsonParseException>().Which;
        // 目的: 先頭ゼロのエラーメッセージを含むこと
        ex.Message.Should().Contain("Leading zeros");
        // 目的: エラー位置が先頭であること
        ex.Position.Should().Be(0);
    }

    // 目的: オブジェクト末尾の余分なカンマを検出することを検証
    [Fact]
    public void Parse_TrailingCommaInObject_ShouldThrow()
    {
        var act = () => JsonParser.Parse("{\"a\":1,}");
        var ex = act.Should().Throw<JsonParseException>().Which;
        // 目的: 末尾カンマ検出のメッセージを含むこと
        ex.Message.Should().Contain("Trailing comma");
    }

    // 目的: 配列末尾の余分なカンマを検出することを検証
    [Fact]
    public void Parse_TrailingCommaInArray_ShouldThrow()
    {
        var act = () => JsonParser.Parse("[1,]");
        var ex = act.Should().Throw<JsonParseException>().Which;
        // 目的: 配列末尾カンマ検出のメッセージを含むこと
        ex.Message.Should().Contain("Trailing comma");
    }

    // 目的: 不正なエスケープシーケンスを検出することを検証
    [Fact]
    public void Parse_InvalidEscape_ShouldThrow()
    {
        var act = () => JsonParser.Parse("\"\\x\"");
        var ex = act.Should().Throw<JsonParseException>().Which;
        // 目的: 不正エスケープのメッセージを含むこと
        ex.Message.Should().Contain("Invalid escape");
    }

    // 目的: 未終端の文字列を検出することを検証
    [Fact]
    public void Parse_UnterminatedString_ShouldThrow()
    {
        var act = () => JsonParser.Parse("\"abc");
        var ex = act.Should().Throw<JsonParseException>().Which;
        // 目的: 未終端文字列のメッセージを含むこと
        ex.Message.Should().Contain("unterminated string");
    }

    // 目的: 非エスケープの制御文字を含む文字列を検出することを検証
    [Fact]
    public void Parse_String_WithUnescapedControlCharacter_ShouldThrow()
    {
        // JSONでは制御文字はエスケープ必須。ここでは実際に U+0001 を混在させる
        var json = "\"A\u0001B\"".Replace("\\u0001", "\u0001");
        var act = () => JsonParser.Parse(json);
        var ex = act.Should().Throw<JsonParseException>().Which;
        // 目的: 制御文字はエスケープ必須のメッセージを含むこと
        ex.Message.Should().Contain("Control characters must be escaped");
    }

    // 目的: 値の直後に余分な文字がある場合に検出することを検証
    [Fact]
    public void Parse_ExtraCharactersAfterValue_ShouldThrow()
    {
        var act = () => JsonParser.Parse("truex");
        var ex = act.Should().Throw<JsonParseException>().Which;
        // 目的: 余剰文字検出のメッセージを含むこと
        ex.Message.Should().Contain("Unexpected character");
    }

    // 目的: プリミティブ/コレクションのシリアライズとエスケープの正しさを検証
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
        // 目的: 文字列エスケープ/最小化/順序維持が正しいこと
        json.Should().Be("{\"a\":1,\"b\":2,\"c\":true,\"d\":null,\"e\":\"A\\\"B\",\"f\":[1,\"x\",false],\"g\":{\"n\":10}} ".Trim());
    }

    // 目的: 文字列中の制御文字は \uXXXX へエスケープされることを検証
    [Fact]
    public void Serialize_String_ControlCharacters_AsUnicodeEscape()
    {
        var s = "A" + '\u0001' + "B";
        var json = JsonParser.Serialize(s);
        // 目的: 制御文字が Unicode エスケープへ変換されること
        json.Should().Be("\"A\\u0001B\"");
    }

    // 目的: NaN/Infinity は JSON 仕様に則り null で表現されることを検証
    [Fact]
    public void Serialize_FloatAndDouble_NaNAndInfinity_AsNull()
    {
        // 目的: double.NaN が null へ変換されること
        JsonParser.Serialize(double.NaN).Should().Be("null");
        // 目的: double.PositiveInfinity が null へ変換されること
        JsonParser.Serialize(double.PositiveInfinity).Should().Be("null");
        // 目的: float.NegativeInfinity が null へ変換されること
        JsonParser.Serialize(float.NegativeInfinity).Should().Be("null");
    }

    // 目的: Parse→Serialize の往復でデータが保持され、出力は最小化されることを検証
    [Fact]
    public void RoundTrip_ParseThenSerialize_MinifiesAndPreservesData()
    {
        var original = "{ \n  \"a\" : [1, 2, 3], \n \"b\" : { \"x\" : \"y\" } \n }";
        var parsed = JsonParser.Parse(original);
        var minified = JsonParser.Serialize(parsed);

        // 最小化されるが、再パースすると元と同等の構造になる
        var reparsed = JsonParser.Parse(minified);
        // 目的: 再パース後も辞書構造であること
        reparsed.Should().BeOfType<Dictionary<string, object>>();

        var obj = reparsed as Dictionary<string, object>;
        // 目的: キー a が存在すること
        obj!.Should().ContainKey("a");
        // 目的: キー b が存在すること
        obj!.Should().ContainKey("b");
        // 目的: a は配列であること
        var arr2 = obj!["a"].Should().BeOfType<List<object>>().Subject;
        // 目的: a の配列要素数が3であること
        arr2.Should().HaveCount(3);
        // 目的: b.x が "y" であること
        ((obj!["b"] as Dictionary<string, object>)!["x"]).Should().Be("y");
    }
}
