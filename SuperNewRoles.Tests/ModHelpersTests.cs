using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SuperNewRoles;
using Xunit;

namespace SuperNewRoles.Tests;

public class ModHelpersTests
{
    [Fact]
    public void HashMD5_And_SHA256_Are_Deterministic_Lowercase()
    {
        ModHelpers.HashMD5("abc").Should().Be("900150983cd24fb0d6963f7d28e17f72");
        ModHelpers.HashMD5("abc").Should().Be(ModHelpers.HashMD5("abc"));

        ModHelpers.HashSHA256("abc").Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
        ModHelpers.HashSHA256("abc").Should().Be(ModHelpers.HashSHA256("abc"));
    }

    [Fact]
    public void GetRandomInt_Inclusive_And_Swaps_When_MinGreaterThanMax()
    {
        // Inclusive range check
        for (int i = 0; i < 100; i++)
        {
            var v = ModHelpers.GetRandomInt(5, 5);
            v.Should().Be(5);
        }

        // When min > max, arguments are swapped internally
        for (int i = 0; i < 100; i++)
        {
            var v = ModHelpers.GetRandomInt(1, 3); // min=1, max=3 (already ordered)
            v.Should().BeInRange(1, 3);
        }
        for (int i = 0; i < 100; i++)
        {
            var v = ModHelpers.GetRandomInt(3, 1); // reversed
            v.Should().BeInRange(1, 3);
        }
    }

    [Fact]
    public void GetRandomFloat_InRange_And_Swaps_When_MinGreaterThanMax()
    {
        for (int i = 0; i < 20; i++)
        {
            var v = ModHelpers.GetRandomFloat(1.0f, 3.0f);
            v.Should().BeInRange(1.0f, 3.0f);
        }
        for (int i = 0; i < 20; i++)
        {
            var v = ModHelpers.GetRandomFloat(3.0f, 1.0f); // reversed
            v.Should().BeInRange(1.0f, 3.0f);
        }
    }

    [Fact]
    public void IsSuccessChance_Edges_Behavior()
    {
        ModHelpers.IsSuccessChance(0).Should().BeFalse();
        ModHelpers.IsSuccessChance(100).Should().BeTrue();
    }

    [Fact]
    public void Cs_Wraps_With_Argb_Hex()
    {
        // Use test-friendly overload to avoid UnityEngine.Color static initialization
        var colored = ModHelpers.Cs(1f, 0f, 0f, 1f, "X");
        colored.Should().Be("<color=#FF0000FF>X</color>");
    }

    [Fact]
    public void ToByte_Clamps_And_Scales()
    {
        ModHelpers.ToByte(0f).Should().Be(0);
        ModHelpers.ToByte(1f).Should().Be(255);
        ModHelpers.ToByte(2f).Should().Be(255);
        ModHelpers.ToByte(-1f).Should().Be(0);
        ModHelpers.ToByte(0.5f).Should().Be(128);
    }

    [Fact]
    public void WrapText_HalfWidth_Wraps_As_Expected()
    {
        // ASCII chars count as 0.5 width when halfIshalf=true
        var input = "abcdef";
        var wrapped = ModHelpers.WrapText(input, width: 2, halfIshalf: true);
        wrapped.Should().Be("abcd\nef");
    }

    [Fact]
    public void AspectRatio_Computes_Reduced_Form()
    {
        ModHelpers.GetAspectRatio(1920, 1080).Should().Be((16, 9));
        ModHelpers.GetAspectRatio(1280, 1024).Should().Be((5, 4));
    }

    [Fact]
    public void IsPositionDistance_Checks_Euclidean()
    {
        // Use overload that avoids UnityEngine.Vector2 construction
        ModHelpers.IsPositionDistance(0f, 0f, 3f, 4f, 5f).Should().BeTrue(); // distance 5
        ModHelpers.IsPositionDistance(0f, 0f, 3f, 4f, 4.9f).Should().BeFalse();
    }

    [Fact]
    public void GetRandom_Index_And_Element_Handle_Empty()
    {
        var empty = new List<int>();
        ModHelpers.GetRandomIndex(empty).Should().Be(-1);
        ModHelpers.GetRandom(empty).Should().Be(0); // default(int)

        var list = new List<int> { 10, 20, 30 };
        var idx = ModHelpers.GetRandomIndex(list);
        idx.Should().BeInRange(0, 2);
    }
}
