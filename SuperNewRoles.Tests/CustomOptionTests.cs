using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using SuperNewRoles.Modules;
using SuperNewRoles.CustomOptions.Categories;
using Xunit;

namespace SuperNewRoles.Tests;

// カスタムオプション（Bool/Int）と親子関係、表示モード、
// 既定値/範囲/イベント通知、MD5 ユーティリティを検証するテスト。
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

    // 目的: Int オプションの既定値/インデックス境界/ValueChanged イベントを検証
    [Fact]
    public void CustomOptionInt_DefaultAndBounds_Behavior()
    {
        var attr = new CustomOptionIntAttribute("Level", 0, 10, 2, 4, translationName: "Level");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.Level)));

        // 既定値チェック: 既定値4はインデックス(4-0)/2=2に対応すること
        // 目的: 既定選択インデックスが期待通り(2)
        option.DefaultSelection.Should().Be(2);
        // 目的: 現在選択も既定と同じ(2)
        option.Selection.Should().Be(2);
        // 目的: バッキングフィールドへ値(4)が反映
        DummyOptions.Level.Should().Be(4);

        // 値変更イベントの通知確認: インデックス3へ変更すると値6が通知され、反映されること
        int? observed = null;
        attr.ValueChanged += v => observed = v;
        option.UpdateSelection(3);
        // 目的: 値変更イベントで 6 が通知
        observed.Should().Be(6);
        // 目的: バッキングフィールドも 6 に更新
        DummyOptions.Level.Should().Be(6);

        // 範囲外のインデックス指定は0(最小)にフォールバックすること
        option.UpdateSelection(200);
        // 目的: 範囲外指定は 0 にフォールバック
        option.Selection.Should().Be(0);
        // 目的: バッキングフィールドも 0 に更新
        DummyOptions.Level.Should().Be(0);
    }

    // 目的: 親の特定値（false）時のみ子が表示される指定が機能することを検証
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
        // 目的: 親が false のとき子は表示される
        child.ShouldDisplay().Should().BeTrue();

        // 親をtrueに切り替え: 子は非表示になる
        parent.UpdateSelection(1);
        // 目的: 親が true になったら子は非表示
        child.ShouldDisplay().Should().BeFalse();
    }

    // 目的: DisplayMode None/All の動作を検証
    [Fact]
    public void CustomOption_DisplayMode_All_And_None()
    {
        var attr = new CustomOptionBoolAttribute("AnyMode", true, translationName: "AnyMode");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.FeatureToggle)));

        // DisplayMode.None: 現在のモードに関係なく常に非表示
        option.SetDisplayMode(DisplayModeId.None);
        Categories.ModeOption = ModeId.Default;
        // 目的: DisplayMode.None では常に非表示
        option.ShouldDisplay().Should().BeFalse();
        Categories.ModeOption = ModeId.SuperHostRoles;
        // 目的: モード変更しても非表示のまま
        option.ShouldDisplay().Should().BeFalse();

        // DisplayMode.All: 常に表示
        option.SetDisplayMode(DisplayModeId.All);
        Categories.ModeOption = ModeId.Default;
        // 目的: DisplayMode.All では常に表示
        option.ShouldDisplay().Should().BeTrue();
        Categories.ModeOption = ModeId.SuperHostRoles;
        // 目的: モード変更後も表示
        option.ShouldDisplay().Should().BeTrue();
    }
    // 目的: Bool オプションの既定選択がフィールドへ反映され、変更で更新されることを検証
    [Fact]
    public void CustomOptionBool_DefaultSelection_AppliesToField()
    {
        var attr = new CustomOptionBoolAttribute("FeatureToggle", true, translationName: "FeatureToggle");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.FeatureToggle)));

        // 生成時に既定値(true)がフィールドへ反映されること
        // 目的: 既定選択が 1 (true) である
        option.DefaultSelection.Should().Be(1);
        // 目的: バッキングフィールドが true に設定
        DummyOptions.FeatureToggle.Should().BeTrue();

        // UpdateSelectionでフィールドも更新されること
        option.UpdateSelection(0);
        // 目的: UpdateSelection(0) で false へ更新
        DummyOptions.FeatureToggle.Should().BeFalse();
        // 目的: 現在選択が 0 へ更新
        option.Selection.Should().Be(0);
    }

    // 目的: 親の選択に応じて子の表示が切り替わることを検証
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
        // 目的: デフォルト(false=選択肢0)の親では子は非表示
        child.ShouldDisplay().Should().BeFalse();

        // 親を有効(true)にすると子は表示
        parent.UpdateSelection(1);
        // 目的: 親を true にすると子は表示
        child.ShouldDisplay().Should().BeTrue();
    }

    // 目的: DisplayMode が SuperHostRolesOnly の場合に対象モードのみ表示されることを検証
    [Fact]
    public void CustomOption_ShouldRespect_DisplayMode()
    {
        // DisplayModeがSuperHostRolesOnlyのとき、SuperHostRolesモードのみで表示されること
        var attr = new CustomOptionBoolAttribute("ShrOnly", true, translationName: "ShrOnly");
        var option = new CustomOption(attr, GetField(typeof(DummyOptions), nameof(DummyOptions.FeatureToggle)));
        option.SetDisplayMode(DisplayModeId.SuperHostRolesOnly);

        // デフォルトモード(Default)では非表示
        Categories.ModeOption = ModeId.Default;
        // 目的: Default モードでは非表示
        option.ShouldDisplay().Should().BeFalse();

        // SuperHostRolesモードに切り替えると表示
        Categories.ModeOption = ModeId.SuperHostRoles;
        // 目的: SuperHostRoles モードで表示
        option.ShouldDisplay().Should().BeTrue();
    }

    // 目的: MD5 ハッシュの決定論性と小文字化出力を検証
    [Fact]
    public void Md5Hash_IsDeterministic_AndLowercase()
    {
        // MD5ハッシュが決定論的で、常に小文字16進数になること
        // 目的: MD5 の既知値と一致
        ComputeMD5Hash.Compute("abc").Should().Be("900150983cd24fb0d6963f7d28e17f72");
        // 目的: 同一入力で同一ハッシュ
        ComputeMD5Hash.Compute("abc").Should().Be(ComputeMD5Hash.Compute("abc"));
    }
}
