using System;
using System.IO;
using FluentAssertions;
using SuperNewRoles;
using Xunit;
using AmongUs.Data;

namespace SuperNewRoles.Tests;

// 翻訳 CSV のロード/言語切替/エスケープ処理/クリーンアップ時の動作を検証するテスト。
[Collection("ModTranslation")]
public class ModTranslationTests : IDisposable
{
    private const string TestCsv =
        "# comment line should be ignored\n" +
        "Test.Close,閉じる,Close\n" +
        "Test.People,{0} 人,{0} people\n" +
        "Test.FormatMany,{0}-{1}-{2},{0}-{1}-{2}\n" +
        "Test.Newlines,一行目\\n二行目,first\\nsecond\n" +
        "Test.Langs4,日,英,简,繁\n" +
        "Test.TChineseComma,日,英,简,繁,後半\n";

    public void Dispose()
    {
        ModTranslation.Cleanup();
        ModTranslation.ClearTestTranslationCsv();
        ModTranslation.ClearTestLanguage();
        ModTranslation.ClearTestAndroid();
    }

    // 目的: 未知の言語では英語列が既定で使用されることを検証
    [Fact]
    public void Load_UsesTestCsv_EnglishByDefault()
    {
        // Arrange: unknown language => English fallback
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.ClearTestLanguage();
        ModTranslation.SetTestAndroid(false);
        ModTranslation.Cleanup();

        // Act
        ModTranslation.Load();

        // Assert
        // 目的: 未知言語では英語列が既定で取得されること
        ModTranslation.GetString("Test.Close").Should().Be("Close");
        ModTranslation.GetString("Test.Langs4").Should().Be("英");
        // 目的: TryGetString が true を返し、値が取得できること
        ModTranslation.TryGetString("Test.Close", out var v).Should().BeTrue();
        // 目的: 取得された値が英語列であること
        v.Should().Be("Close");
    }

    // 目的: 文字列のフォーマットと未知キーでのフォールバック動作を検証
    [Fact]
    public void GetString_Format_And_TryGet_UnknownKey_WithTestCsv()
    {
        // Arrange
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Act / Assert
        // 目的: 書式プレースホルダが置換されること
        ModTranslation.GetString("Test.People", 5).Should().Be("5 people");
        // 目的: 複数引数用オーバーロードで書式プレースホルダが置換されること
        ModTranslation.GetString("Test.FormatMany", "A", "B", "C").Should().Be("A-B-C");
        // 目的: object配列を明示した従来のparams呼び出しも維持されること
        object[] format = { "D", "E", "F" };
        ModTranslation.GetString("Test.FormatMany", format).Should().Be("D-E-F");
        // 目的: 未知キーでは false を返すこと
        ModTranslation.TryGetString("__unknown_key__", out var value).Should().BeFalse();
        // 目的: 未知キー時の out 値が null であること
        value.Should().BeNull();
        // 目的: GetString ではキー文字列をそのまま返すこと
        ModTranslation.GetString("__unknown_key__").Should().Be("__unknown_key__");
    }

    // 目的: エスケープされた \n が実際の改行に置換されることを検証
    [Fact]
    public void EscapedNewlines_Replaced_WithTestCsv()
    {
        // Arrange
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Act
        var s = ModTranslation.GetString("Test.Newlines");

        // Assert
        // 目的: 実際の改行が含まれること
        s.Should().Contain("\n");
        // 目的: エスケープ表記(\\n)は残らないこと
        s.Should().NotContain("\\n");
        // 目的: 期待する2行の文字列になること
        s.Should().Be("first\nsecond");
    }

    // 目的: 言語切替で適切な列が選択され、欠落時は英語へフォールバックすることを検証
    [Fact]
    public void LanguageSwitch_SelectsCorrectColumn_WithTestCsv()
    {
        // Arrange: Japanese (avoid touching IL2CPP DataManager in tests)
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.SetTestLanguage(SupportedLangs.Japanese);
        ModTranslation.Cleanup();
        ModTranslation.Load();
        // 目的: 日本語選択時は日本語列が取得されること
        ModTranslation.GetString("Test.Close").Should().Be("閉じる");

        // SChinese: prefers 4th column if exists, else falls back to English
        ModTranslation.SetTestLanguage(SupportedLangs.SChinese);
        ModTranslation.Cleanup();
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Load();
        // 目的: 簡体中国語選択時、4列目が取得されること
        ModTranslation.GetString("Test.Langs4").Should().Be("简");
        // 目的: 対応列が無い場合は英語へフォールバックすること
        ModTranslation.GetString("Test.Close").Should().Be("Close");

        // TChinese: 5列目以降のカンマを翻訳本文として維持する
        ModTranslation.SetTestLanguage(SupportedLangs.TChinese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Test.TChineseComma").Should().Be("繁,後半");
    }

    [Fact]
    public void LanguageSwitch_UpdatesLoadedDesktopTableWithoutReload()
    {
        // 準備
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.SetTestAndroid(false);
        ModTranslation.SetTestLanguage(SupportedLangs.English);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // 実行・検証
        ModTranslation.GetString("Test.Langs4").Should().Be("英");
        ModTranslation.SetTestLanguage(SupportedLangs.Japanese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Test.Langs4").Should().Be("日");
        ModTranslation.SetTestLanguage(SupportedLangs.SChinese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Test.Langs4").Should().Be("简");
        ModTranslation.SetTestLanguage(SupportedLangs.TChinese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Test.Langs4").Should().Be("繁");
    }

    [Fact]
    public void LanguageSwitch_ReloadsOnlyCurrentTableOnAndroid()
    {
        // 準備
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.SetTestAndroid(true);
        ModTranslation.SetTestLanguage(SupportedLangs.English);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // 実行・検証
        ModTranslation.GetString("Test.Langs4").Should().Be("英");
        ModTranslation.SetTestLanguage(SupportedLangs.Japanese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Test.Langs4").Should().Be("日");
        ModTranslation.SetTestLanguage(SupportedLangs.SChinese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Test.Langs4").Should().Be("简");
        ModTranslation.SetTestLanguage(SupportedLangs.TChinese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Test.TChineseComma").Should().Be("繁,後半");
    }

    [Fact]
    public void AndroidLanguageSwitch_RetriesAfterCsvReadFailure()
    {
        // 準備
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.SetTestAndroid(true);
        ModTranslation.SetTestLanguage(SupportedLangs.English);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        ModTranslation.SetTestTranslationStreamProvider(() => throw new IOException("test failure"));
        ModTranslation.SetTestLanguage(SupportedLangs.Japanese);

        // 実行
        Action firstUpdate = ModTranslation.UpdateCurrentTranslations;

        // 検証: 失敗時は旧言語を維持し、次回更新で再試行できる。
        firstUpdate.Should().Throw<IOException>();
        ModTranslation.GetString("Test.Langs4").Should().Be("英");

        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Test.Langs4").Should().Be("日");
    }

    // 目的: Cleanup 後はテーブルが解放され、LookUp が無効化されることを検証
    [Fact]
    public void Cleanup_ReleasesTables_DisablesLookup_WithTestCsv()
    {
        // Arrange
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // Sanity
        // 目的: 事前確認として値が取得できること
        ModTranslation.TryGetString("Test.Close", out var before).Should().BeTrue();
        // 目的: 取得された値が英語列であること
        before.Should().Be("Close");

        // Act
        ModTranslation.Cleanup();

        // Assert
        // 目的: Cleanup 後は検索できないこと
        ModTranslation.TryGetString("Test.Close", out var after).Should().BeFalse();
        // 目的: Cleanup 後の out 値は null であること
        after.Should().BeNull();
        // 目的: Cleanup 後はキーをそのまま返すこと
        ModTranslation.GetString("Test.Close").Should().Be("Test.Close");
    }

    [Fact]
    public void RepeatedLookup_ReusesStoredString_AndRepeatedLoadRemainsValid()
    {
        // 準備
        ModTranslation.SetTestTranslationCsv(TestCsv);
        ModTranslation.SetTestLanguage(SupportedLangs.English);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // 実行
        string first = ModTranslation.GetString("Test.Newlines");
        string second = ModTranslation.GetString("Test.Newlines");
        ModTranslation.Load();
        string afterReload = ModTranslation.GetString("Test.Newlines");

        // 検証: 既知の翻訳は検索ごとに文字列を再生成せず、格納済みインスタンスを返す。
        second.Should().BeSameAs(first);
        afterReload.Should().Be("first\nsecond");
    }

    [Fact]
    public void DuplicateKeys_FirstValueWins()
    {
        // 準備
        const string csv = "Test.Duplicate,最初,First\nTest.Duplicate,後,Second\n";
        ModTranslation.SetTestTranslationCsv(csv);
        ModTranslation.SetTestLanguage(SupportedLangs.English);
        ModTranslation.Cleanup();

        // 実行
        ModTranslation.Load();

        // 検証: 重複行は旧FastHashTableと同様に最初の値を採用する。
        ModTranslation.GetString("Test.Duplicate").Should().Be("First");
    }

    [Fact]
    public void CsvParser_PreservesDocumentedFieldBehavior()
    {
        // 準備: BOM、CRLF、値の空白、欠落列、明示空欄、追加カンマ、大小文字、終端改行なしを含む。
        const string csv =
            "\uFEFF Key , 日 , English , 简 , 繁,後半\r\n" +
            "Missing,日,English\r\n" +
            "Empty,日,English,,\r\n" +
            "Case,日,Upper\r\n" +
            "case,日,Lower\r\n" +
            "Final,日,FinalEnglish";
        ModTranslation.SetTestTranslationCsv(csv);
        ModTranslation.SetTestAndroid(false);
        ModTranslation.SetTestLanguage(SupportedLangs.English);
        ModTranslation.Cleanup();
        ModTranslation.Load();

        // 検証: キーだけをTrimし、値の空白と大小文字を維持する。
        ModTranslation.GetString("Key").Should().Be(" English ");
        ModTranslation.GetString("Case").Should().Be("Upper");
        ModTranslation.GetString("case").Should().Be("Lower");
        ModTranslation.GetString("Final").Should().Be("FinalEnglish");

        // 検証: 列欠落時のみ英語へフォールバックし、明示空欄は空文字として扱う。
        ModTranslation.SetTestLanguage(SupportedLangs.SChinese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Missing").Should().Be("English");
        ModTranslation.GetString("Empty").Should().BeEmpty();

        // 検証: 5列目以降のカンマを繁体字本文として維持する。
        ModTranslation.SetTestLanguage(SupportedLangs.TChinese);
        ModTranslation.UpdateCurrentTranslations();
        ModTranslation.GetString("Key").Should().Be(" 繁,後半");
    }

    [Fact]
    public void EmbeddedCsv_IncludesRequestInGameClearButtonKeys()
    {
        try
        {
            ModTranslation.ClearTestTranslationCsv();
            ModTranslation.SetTestLanguage(SupportedLangs.Japanese);
            ModTranslation.Cleanup();
            ModTranslation.Load();

            ModTranslation.GetString("RequestInGameClearButton").Should().Be("入力クリア");
            ModTranslation.GetString("RequestInGameClearConfirmButton").Should().Be("本当にクリアしますか？");
        }
        finally
        {
            ModTranslation.SetTestLanguage(SupportedLangs.English);
            ModTranslation.Cleanup();
        }
    }
}
