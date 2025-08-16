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
