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

        // 既定値チェック: 既定値4はインデックス(4-0)/2=2に対応すること
        option.DefaultSelection.Should().Be(2);
        option.Selection.Should().Be(2);
        DummyOptions.Level.Should().Be(4);

        // 値変更イベントの通知確認: インデックス3へ変更すると値6が通知され、反映されること
        int? observed = null;
        attr.ValueChanged += v => observed = v;
        option.UpdateSelection(3);
        observed.Should().Be(6);
        DummyOptions.Level.Should().Be(6);

        // 範囲外のインデックス指定は0(最小)にフォールバックすること
        option.UpdateSelection(200);
        option.Selection.Should().Be(0);
        DummyOptions.Level.Should().Be(0);
    }

    [Fact]
    public void CustomOption_ParentActiveValue_ShouldMatchOnlySpecificValue()
    {
        // 親オプションがfalse(既定)のときのみ子を表示する指定の検証
        var parentAttr = new CustomOptionBoolAttribute("ParentEnabled", false, translationName: "ParentEnabled");
        var parent = new CustomOption(parentAttr, GetField(typeof(DummyOptions), nameof(DummyOptions.ParentEnabled)));

        var childAttr = new CustomOptionIntAttribute("ChildValue", 0, 10, 1, 5, translationName: "ChildValue", parentFieldName: nameof(DummyOptions.ParentEnabled), parentActiveValue: false);
        var child = new CustomOption(childAttr, GetField(typeof(DummyOptions), nameof(DummyOptions.ChildValue)));
        child.SetParentOption(parent);

        // 親がfalseのとき: 子は表示される
        child.ShouldDisplay().Should().BeTrue();

        // 親をtrueに切り替え: 子は非表示になる
        parent.UpdateSelection(1);
        child.ShouldDisplay().Should().BeFalse();
    }

    [Fact]
    public void CustomOption_DisplayMode_All_And_None()
    {
        var attr = new CustomOptionBoolAttribute("AnyMode", true, translationName: "AnyMode");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.FeatureToggle)));

        // DisplayMode.None: 現在のモードに関係なく常に非表示
        option.SetDisplayMode(DisplayModeId.None);
        Categories.ModeOption = ModeId.Default;
        option.ShouldDisplay().Should().BeFalse();
        Categories.ModeOption = ModeId.SuperHostRoles;
        option.ShouldDisplay().Should().BeFalse();

        // DisplayMode.All: 常に表示
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

        // 生成時に既定値(true)がフィールドへ反映されること
        option.DefaultSelection.Should().Be(1);
        DummyOptions.FeatureToggle.Should().BeTrue();

        // UpdateSelectionでフィールドも更新されること
        option.UpdateSelection(0);
        DummyOptions.FeatureToggle.Should().BeFalse();
        option.Selection.Should().Be(0);
    }

    [Fact]
    public void CustomOption_ShouldDisplay_DependsOnParentSelection()
    {
        // 親(既定はfalse=選択肢0)と連動した表示/非表示の検証
        var parentAttr = new CustomOptionBoolAttribute("ParentEnabled", false, translationName: "ParentEnabled");
        var parent = new CustomOption(parentAttr, GetField(typeof(DummyOptions), nameof(DummyOptions.ParentEnabled)));

        var childAttr = new CustomOptionIntAttribute("ChildValue", 0, 10, 1, 5, translationName: "ChildValue");
        var child = new CustomOption(childAttr, GetField(typeof(DummyOptions), nameof(DummyOptions.ChildValue)));

        child.SetParentOption(parent);

        // 親が無効(false)の間は子は非表示
        child.ShouldDisplay().Should().BeFalse();

        // 親を有効(true)にすると子は表示
        parent.UpdateSelection(1);
        child.ShouldDisplay().Should().BeTrue();
    }

    [Fact]
    public void CustomOption_ShouldRespect_DisplayMode()
    {
        // DisplayModeがSuperHostRolesOnlyのとき、SuperHostRolesモードのみで表示されること
        var attr = new CustomOptionBoolAttribute("ShrOnly", true, translationName: "ShrOnly");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.FeatureToggle)));
        option.SetDisplayMode(DisplayModeId.SuperHostRolesOnly);

        // デフォルトモード(Default)では非表示
        Categories.ModeOption = ModeId.Default;
        option.ShouldDisplay().Should().BeFalse();

        // SuperHostRolesモードに切り替えると表示
        Categories.ModeOption = ModeId.SuperHostRoles;
        option.ShouldDisplay().Should().BeTrue();
    }

    [Fact]
    public void Md5Hash_IsDeterministic_AndLowercase()
    {
        // MD5ハッシュが決定論的で、常に小文字16進数になること
        ComputeMD5Hash.Compute("abc").Should().Be("900150983cd24fb0d6963f7d28e17f72");
        ComputeMD5Hash.Compute("abc").Should().Be(ComputeMD5Hash.Compute("abc"));
    }
}
