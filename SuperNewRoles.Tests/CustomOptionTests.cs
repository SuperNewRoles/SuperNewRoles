using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions.Categories;
using Xunit;

namespace SuperNewRoles.Tests;

public class CustomOptionTests
{
    private static FieldInfo GetField(Type t, string name) => t.GetField(name, BindingFlags.Public | BindingFlags.Static)!;

    public static class DummyOptions
    {
        // Values replaced by CustomOption.UpdateSelection via reflection
        public static bool FeatureToggle = false;
        public static int Level = 0;
        public static bool ParentEnabled = false;
        public static int ChildValue = 0;
    }

    [Fact]
    public void CustomOptionInt_DefaultAndBounds_Behavior()
    {
        var attr = new CustomOptionIntAttribute("Level", 0, 10, 2, 4, translationName: "Level");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.Level)));

        // Default maps to index (4-0)/2 = 2
        option.DefaultSelection.Should().Be(2);
        option.Selection.Should().Be(2);
        DummyOptions.Level.Should().Be(4);

        // Subscribe to change notification and move to next value (index 3 -> value 6)
        int? observed = null;
        attr.ValueChanged += v => observed = v;
        option.UpdateSelection(3);
        observed.Should().Be(6);
        DummyOptions.Level.Should().Be(6);

        // Out of range falls back to index 0
        option.UpdateSelection(200);
        option.Selection.Should().Be(0);
        DummyOptions.Level.Should().Be(0);
    }

    [Fact]
    public void CustomOption_ParentActiveValue_ShouldMatchOnlySpecificValue()
    {
        // Parent defaults to false; child visible only when parent == false
        var parentAttr = new CustomOptionBoolAttribute("ParentEnabled", false, translationName: "ParentEnabled");
        var parent = new CustomOption(parentAttr, GetField(typeof(DummyOptions), nameof(DummyOptions.ParentEnabled)));

        var childAttr = new CustomOptionIntAttribute("ChildValue", 0, 10, 1, 5, translationName: "ChildValue", parentFieldName: nameof(DummyOptions.ParentEnabled), parentActiveValue: false);
        var child = new CustomOption(childAttr, GetField(typeof(DummyOptions), nameof(DummyOptions.ChildValue)));
        child.SetParentOption(parent);

        // Visible when parent == false
        child.ShouldDisplay().Should().BeTrue();

        // Flip parent to true -> now hidden
        parent.UpdateSelection(1);
        child.ShouldDisplay().Should().BeFalse();
    }

    [Fact]
    public void CustomOption_DisplayMode_All_And_None()
    {
        var attr = new CustomOptionBoolAttribute("AnyMode", true, translationName: "AnyMode");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.FeatureToggle)));

        // None: never displays regardless of current mode
        option.SetDisplayMode(DisplayModeId.None);
        Categories.ModeOption = ModeId.Default;
        option.ShouldDisplay().Should().BeFalse();
        Categories.ModeOption = ModeId.SuperHostRoles;
        option.ShouldDisplay().Should().BeFalse();

        // All: always displays
        option.SetDisplayMode(DisplayModeId.All);
        Categories.ModeOption = ModeId.Default;
        option.ShouldDisplay().Should().BeTrue();
        Categories.ModeOption = ModeId.SuperHostRoles;
        option.ShouldDisplay().Should().BeTrue();
    }
    [Fact]
    public void CustomOptionBool_DefaultSelection_AppliesToField()
    {
        var attr = new CustomOptionBoolAttribute("FeatureToggle", true, translationName: "FeatureToggle");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.FeatureToggle)));

        // Constructor applies default
        option.DefaultSelection.Should().Be(1);
        DummyOptions.FeatureToggle.Should().BeTrue();

        // UpdateSelection reflects to backing field
        option.UpdateSelection(0);
        DummyOptions.FeatureToggle.Should().BeFalse();
        option.Selection.Should().Be(0);
    }

    [Fact]
    public void CustomOption_ShouldDisplay_DependsOnParentSelection()
    {
        // Parent defaults to false (selection 0)
        var parentAttr = new CustomOptionBoolAttribute("ParentEnabled", false, translationName: "ParentEnabled");
        var parent = new CustomOption(parentAttr, GetField(typeof(DummyOptions), nameof(DummyOptions.ParentEnabled)));

        var childAttr = new CustomOptionIntAttribute("ChildValue", 0, 10, 1, 5, translationName: "ChildValue");
        var child = new CustomOption(childAttr, GetField(typeof(DummyOptions), nameof(DummyOptions.ChildValue)));

        child.SetParentOption(parent);

        // Parent disabled -> child hidden
        child.ShouldDisplay().Should().BeFalse();

        // Enable parent -> child visible
        parent.UpdateSelection(1);
        child.ShouldDisplay().Should().BeTrue();
    }

    [Fact]
    public void CustomOption_ShouldRespect_DisplayMode()
    {
        // Option visible only in SuperHostRoles mode
        var attr = new CustomOptionBoolAttribute("ShrOnly", true, translationName: "ShrOnly");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.FeatureToggle)));
        option.SetDisplayMode(DisplayModeId.SuperHostRolesOnly);

        // Default mode is ModeId.Default
        Categories.ModeOption = ModeId.Default;
        option.ShouldDisplay().Should().BeFalse();

        // Switch mode -> now visible
        Categories.ModeOption = ModeId.SuperHostRoles;
        option.ShouldDisplay().Should().BeTrue();
    }

    [Fact]
    public void Md5Hash_IsDeterministic_AndLowercase()
    {
        ComputeMD5Hash.Compute("abc").Should().Be("900150983cd24fb0d6963f7d28e17f72");
        ComputeMD5Hash.Compute("abc").Should().Be(ComputeMD5Hash.Compute("abc"));
    }
}
