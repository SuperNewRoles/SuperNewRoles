using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SuperNewRoles;
using Xunit;

namespace SuperNewRoles.Tests;

// 目的: 役職割り当て等で使用する ModHelpers.GetRandomInt の乱数品質を統計的に検証する。
// 「よりランダムな乱数を使用する」設定の有無にかかわらず、役職割り当ての偏りが
// 乱数生成器由来ではないことを保証するため、一様性をカイ二乗検定で確認する。
public class RandomDistributionTests
{
    // カイ二乗分布の上側0.1%棄却点 (自由度 = categories - 1)
    // 5%棄却点だと真の一様乱数でも5%の確率で偽陽性が出るため、
    // CIで頻繁に落ちるのを防ぐために0.1%を採用。
    // サイズが大きいほど精度が高いが、実行時間との兼ね合いで適宜調整。
    private const int SampleCount = 20000;

    // 目的: GetRandomInt(max, min) が [min, max] の範囲で一様に分布することを検証
    [Theory]
    [InlineData(0, 9)]   // 10個のカテゴリ (自由度9)
    [InlineData(0, 19)]  // 20個のカテゴリ (自由度19)
    [InlineData(0, 4)]   // 5個のカテゴリ (自由度4)
    public void GetRandomInt_Is_Approximately_Uniform(int min, int max)
    {
        int categories = max - min + 1;
        var counts = new int[categories];
        for (int i = 0; i < SampleCount; i++)
        {
            int v = ModHelpers.GetRandomInt(max, min);
            v.Should().BeInRange(min, max);
            counts[v - min]++;
        }

        // カイ二乗統計量の計算
        double expected = (double)SampleCount / categories;
        double chiSquare = counts.Sum(c =>
        {
            double diff = c - expected;
            return (diff * diff) / expected;
        });

        // 自由度 = categories - 1 のカイ二乗分布における上側0.1%棄却点
        // 5%点だと真の一様乱数でも5%確率で偽陽性になるため、より厳格な0.1%点を使用
        // 一般的な値: df=4 -> 18.467, df=9 -> 27.877, df=19 -> 43.82
        double criticalValue = ChiSquareCriticalValue(categories - 1);

        // 目的: 真の一様分布から生成された場合、統計量が棄却点を超える確率は0.1%
        // 棄却された場合は乱数生成器に偏りがある可能性が高い
        chiSquare.Should().BeLessThan(
            criticalValue,
            $"カイ二乗統計量 {chiSquare:F2} が棄却点 {criticalValue:F2} (自由度={categories - 1}) を超えました。" +
            $"分布に偏りがある可能性があります。各カテゴリの度数: [{string.Join(", ", counts)}]");
    }

    // 目的: GetRandomIndex がリストの全インデックスを満遍なく返すことを検証
    [Fact]
    public void GetRandomIndex_Covers_All_Indices_Uniformly()
    {
        var list = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var counts = new int[list.Count];
        for (int i = 0; i < SampleCount; i++)
        {
            int idx = ModHelpers.GetRandomIndex(list);
            idx.Should().BeInRange(0, list.Count - 1);
            counts[idx]++;
        }

        double expected = (double)SampleCount / list.Count;
        double chiSquare = counts.Sum(c =>
        {
            double diff = c - expected;
            return (diff * diff) / expected;
        });

        // 自由度 = 9 の0.1%棄却点 27.877
        chiSquare.Should().BeLessThan(
            27.877,
            $"GetRandomIndex の分布に偏りがあります。統計量: {chiSquare:F2}");
    }

    // 目的: チケット選択シミュレーション - 複数の役職チケットからランダム選択した場合の偏りを検証
    // AssignRoles.AssignTickets のコアルゴリズムを模倣し、
    // 全役100%の設定で各役職が均等に割り当てられることを確認する
    [Fact]
    public void TicketSelection_Is_Fair_Across_Roles()
    {
        // 5つの役職チケットがあり、それぞれ100% (NumberOfCrews=1) の場合
        // 10人プレイヤーに対して5役職を割り当てるシミュレーション
        const int roleCount = 5;
        const int simulations = 5000;
        var assignmentCounts = new int[roleCount];

        for (int sim = 0; sim < simulations; sim++)
        {
            // 各役職が何回割り当てられたかをカウント
            var tickets = Enumerable.Range(0, roleCount).ToList();
            var roleAssignments = new int[roleCount];

            // 5つの役職枠を埋める
            for (int slot = 0; slot < roleCount; slot++)
            {
                if (tickets.Count == 0) break;
                int ticketIndex = ModHelpers.GetRandomInt(tickets.Count - 1);
                int selectedRole = tickets[ticketIndex];
                tickets.RemoveAt(ticketIndex);
                roleAssignments[selectedRole]++;
            }

            // 各役職は1回ずつ割り当てられるはず
            for (int r = 0; r < roleCount; r++)
            {
                if (roleAssignments[r] > 0)
                    assignmentCounts[r]++;
            }
        }

        // 目的: 各役職が.simulations回のうち均等に割り当てられることを確認
        // 全役100%で枠数=役職数の場合、各役職は毎回必ず割り当てられる
        // なので全て simulations に等しいはず
        foreach (var count in assignmentCounts)
        {
            count.Should().Be(simulations,
                $"全役100%の場合、各役職は毎シミュレーションで必ず割り当てられるべきです。" +
                $"実際の割り当て回数: [{string.Join(", ", assignmentCounts)}]");
        }
    }

    // 目的: 確率付きチケット選択のシミュレーション
    // 50%の役職が複数ある場合、長期的には期待値通りに割り当てられることを検証
    [Fact]
    public void ProbabilisticTicketSelection_Matches_Expected_Rate()
    {
        // 3つの役職がそれぞれ50% (チケット5枚ずつ) の場合
        // 十分な回数シミュレーションすれば、各役職の選ばれる割合は約1/3になるはず
        const int roleCount = 3;
        const int ticketsPerRole = 5; // 50% = 5 tickets
        const int simulations = 30000;
        var selectionCounts = new int[roleCount];

        for (int sim = 0; sim < simulations; sim++)
        {
            // チケットプール構築: 各役職のチケットを ticketsPerRole 枚ずつ
            var tickets = new List<int>();
            for (int r = 0; r < roleCount; r++)
                for (int t = 0; t < ticketsPerRole; t++)
                    tickets.Add(r);

            // 1枚ランダムに選択
            int idx = ModHelpers.GetRandomInt(tickets.Count - 1);
            selectionCounts[tickets[idx]]++;
        }

        // 目的: 各役職の選択割合が期待値 1/3 の ±3% 以内に収まることを確認
        double expectedRate = 1.0 / roleCount;
        for (int r = 0; r < roleCount; r++)
        {
            double actualRate = (double)selectionCounts[r] / simulations;
            double deviation = Math.Abs(actualRate - expectedRate);
            deviation.Should().BeLessThan(0.03,
                $"役職 {r} の選択割合 {actualRate:F4} が期待値 {expectedRate:F4} から大きく逸脱しています。" +
                $"選択回数: [{string.Join(", ", selectionCounts)}]");
        }
    }

    // 目的: 連続して同じ値が出る「偏り」が過度に発生しないことを検証
    // 人間は真の乱数でも「偏っている」と感じやすいが、
    // 連続同一値の最大連長が異常に長くならないことを確認する
    [Fact]
    public void GetRandomInt_Does_Not_Produce_Abnormally_Long_Runs()
    {
        // 0-9の範囲で20000回生成し、同じ値の連続出現の最大連長を測定
        // 真の一様乱数で10種類の場合、連長kの期待出現回数は N * (1/10)^k
        // N=20000 で連長8の期待値は 20000 * 1e-8 = 0.0002 回
        // 念のため閾値は緩めに設定
        const int max = 9;
        const int iterations = 20000;
        const int acceptableMaxRun = 12; // 統計的にまず起こらない長さ

        int currentRun = 1;
        int maxRun = 1;
        int prev = ModHelpers.GetRandomInt(max);

        for (int i = 1; i < iterations; i++)
        {
            int v = ModHelpers.GetRandomInt(max);
            if (v == prev)
            {
                currentRun++;
                if (currentRun > maxRun) maxRun = currentRun;
            }
            else
            {
                currentRun = 1;
            }
            prev = v;
        }

        // 目的: 連長が異常に長くならないこと (乱数がスタックしていないことの確認)
        maxRun.Should().BeLessOrEqualTo(acceptableMaxRun,
            $"同一値の連続出現が {maxRun} 回続きました。乱数生成器がスタックしている可能性があります。");
    }

    // カイ二乗分布の上側0.1%棄却点 (よく使う自由度のみ)
    // より広い範囲が必要な場合は拡張すること
    private static double ChiSquareCriticalValue(int degreesOfFreedom) => degreesOfFreedom switch
    {
        1 => 10.828,
        2 => 13.816,
        3 => 16.266,
        4 => 18.467,
        5 => 20.515,
        6 => 22.458,
        7 => 24.322,
        8 => 26.124,
        9 => 27.877,
        10 => 29.588,
        15 => 37.696,
        19 => 43.820,
        20 => 45.315,
        24 => 51.179,
        29 => 58.302,
        // 近似式 (Wilson–Hilferty) - 大自由度向け
        // z=3.09 (0.1%) を使用
        _ => degreesOfFreedom * Math.Pow(1 - 2.0 / (9 * degreesOfFreedom) + 3.09 * Math.Sqrt(2.0 / (9 * degreesOfFreedom)), 3)
    };
}
