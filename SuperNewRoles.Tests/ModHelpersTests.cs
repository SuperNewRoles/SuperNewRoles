using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SuperNewRoles;
using Xunit;

namespace SuperNewRoles.Tests;

// 乱数/確率/ハッシュ/文字列整形/テキスト折り返し/比率計算など、
// ModHelpers の純粋関数群を中心に検証するテスト。
public class ModHelpersTests
{
    // 目的: MD5/SHA256 が決定論的かつ小文字16進で出力されることを検証
    [Fact]
    public void HashMD5_And_SHA256_Are_Deterministic_Lowercase()
    {
        // 目的: MD5 の既知値と一致
        ModHelpers.HashMD5("abc").Should().Be("900150983cd24fb0d6963f7d28e17f72");
        // 目的: 同一入力で同一ハッシュ
        ModHelpers.HashMD5("abc").Should().Be(ModHelpers.HashMD5("abc"));

        // 目的: SHA256 の既知値と一致
        ModHelpers.HashSHA256("abc").Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
        // 目的: 同一入力で同一ハッシュ
        ModHelpers.HashSHA256("abc").Should().Be(ModHelpers.HashSHA256("abc"));
    }

    // 目的: GetRandomInt が両端含みで動作し、min > max の場合は内部で入れ替えることを検証
    [Fact]
    public void GetRandomInt_Inclusive_And_Swaps_When_MinGreaterThanMax()
    {
        // Inclusive range check
        for (int i = 0; i < 100; i++)
        {
            var v = ModHelpers.GetRandomInt(5, 5);
            // 目的: min=max の場合は常にその値
            v.Should().Be(5);
        }

        // When min > max, arguments are swapped internally
        for (int i = 0; i < 100; i++)
        {
            var v = ModHelpers.GetRandomInt(1, 3); // min=1, max=3 (already ordered)
            // 目的: 正順の範囲内
            v.Should().BeInRange(1, 3);
        }
        for (int i = 0; i < 100; i++)
        {
            var v = ModHelpers.GetRandomInt(3, 1); // reversed
            // 目的: 逆順でも範囲内
            v.Should().BeInRange(1, 3);
        }
    }

    // 目的: GetRandomFloat が範囲内に収まり、min > max の場合でも機能することを検証
    [Fact]
    public void GetRandomFloat_InRange_And_Swaps_When_MinGreaterThanMax()
    {
        for (int i = 0; i < 20; i++)
        {
            var v = ModHelpers.GetRandomFloat(1.0f, 3.0f);
            // 目的: 正順の範囲内
            v.Should().BeInRange(1.0f, 3.0f);
        }
        for (int i = 0; i < 20; i++)
        {
            var v = ModHelpers.GetRandomFloat(3.0f, 1.0f); // reversed
            // 目的: 逆順でも範囲内
            v.Should().BeInRange(1.0f, 3.0f);
        }
    }

    // 目的: 成功確率 0% と 100% の端点挙動を検証
    [Fact]
    public void IsSuccessChance_Edges_Behavior()
    {
        // 目的: 0% は必ず失敗
        ModHelpers.IsSuccessChance(0).Should().BeFalse();
        // 目的: 100% は必ず成功
        ModHelpers.IsSuccessChance(100).Should().BeTrue();
    }

    // 目的: Cs(r,g,b,a,text) が ARGB の 8 桁 16 進で color タグを生成することを検証
    [Fact]
    public void Cs_Wraps_With_Argb_Hex()
    {
        // Use test-friendly overload to avoid UnityEngine.Color static initialization
        var colored = ModHelpers.Cs(1f, 0f, 0f, 1f, "X");
        // 目的: ARGB を 8 桁 16 進で整形
        colored.Should().Be("<color=#FF0000FF>X</color>");
    }

    // 目的: ToByte が [0,1] を 0..255 にクランプ/スケールすることを検証
    [Fact]
    public void ToByte_Clamps_And_Scales()
    {
        // 目的: 0 は 0
        ModHelpers.ToByte(0f).Should().Be(0);
        // 目的: 1 は 255
        ModHelpers.ToByte(1f).Should().Be(255);
        // 目的: 1 超過はクランプされ 255
        ModHelpers.ToByte(2f).Should().Be(255);
        // 目的: 負値はクランプされ 0
        ModHelpers.ToByte(-1f).Should().Be(0);
        // 目的: 0.5 は 約 128
        ModHelpers.ToByte(0.5f).Should().Be(128);
    }

    // 目的: 半角幅計算に基づく折り返しが期待通りに行われることを検証
    [Fact]
    public void WrapText_HalfWidth_Wraps_As_Expected()
    {
        // ASCII chars count as 0.5 width when halfIshalf=true
        var input = "abcdef";
        var wrapped = ModHelpers.WrapText(input, width: 2, halfIshalf: true);
        // 目的: 半角換算で 2 列幅ごとに改行
        wrapped.Should().Be("abcd\nef");
    }

    // 目的: アスペクト比の既約分数が正しく求まることを検証
    [Fact]
    public void AspectRatio_Computes_Reduced_Form()
    {
        // 目的: 1920x1080 -> 16:9
        ModHelpers.GetAspectRatio(1920, 1080).Should().Be((16, 9));
        // 目的: 1280x1024 -> 5:4
        ModHelpers.GetAspectRatio(1280, 1024).Should().Be((5, 4));
    }

    // 目的: ユークリッド距離による判定が期待通りであることを検証
    [Fact]
    public void IsPositionDistance_Checks_Euclidean()
    {
        // Use overload that avoids UnityEngine.Vector2 construction
        // 目的: 距離 5 以下の判定が true
        ModHelpers.IsPositionDistance(0f, 0f, 3f, 4f, 5f).Should().BeTrue(); // distance 5
        // 目的: 距離 4.9 の判定が false
        ModHelpers.IsPositionDistance(0f, 0f, 3f, 4f, 4.9f).Should().BeFalse();
    }

    // 目的: 空コレクション時の挙動と、非空時のインデックス範囲が守られることを検証
    [Fact]
    public void GetRandom_Index_And_Element_Handle_Empty()
    {
        var empty = new List<int>();
        // 目的: 空コレクションのインデックスは -1
        ModHelpers.GetRandomIndex(empty).Should().Be(-1);
        // 目的: 空コレクションでの要素取得は default 値
        ModHelpers.GetRandom(empty).Should().Be(0); // default(int)

        var list = new List<int> { 10, 20, 30 };
        var idx = ModHelpers.GetRandomIndex(list);
        // 目的: 非空コレクションでは範囲内のインデックス
        idx.Should().BeInRange(0, 2);
    }
}
