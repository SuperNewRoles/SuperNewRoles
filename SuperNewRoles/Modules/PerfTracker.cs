// 処理時間の計測、記録、集計を行う静的クラスです。

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SuperNewRoles.Modules;

/// <summary>
/// 処理のパフォーマンス（実行時間）を計測、記録、集計するための静的クラス。
/// 使用例:
/// PerfTracker.Begin("処理A");
/// // 計測したい処理
/// PerfTracker.End("処理A");
/// </summary>
public static class PerfTracker
{
    /// <summary>
    /// 各計測対象のデータを格納する内部クラス。
    /// </summary>
    private class Data
    {
        // 時間計測用のストップウォッチ
        public readonly Stopwatch Sw = new();
        // 直近の計測時間 (ミリ秒)
        public double LastMs;
        // 計測時間の合計 (ミリ秒)
        public double TotalMs;
        // 計測回数
        public int Count;
    }

    // 計測データを保持する辞書。キーは処理名(string)、値は計測データ(Data)。
    private static readonly Dictionary<string, Data> _data = new();

    /// <summary>
    /// 指定されたキーの処理の計測を開始します。
    /// </summary>
    /// <param name="key">計測対象を識別する一意のキー（処理名など）</param>
    public static void Begin(string key)
    {
        // ベータ版でない場合は計測を行わない
        if (Statics.IsBeta == false)
        {
            return;
        }
        // 辞書にキーが存在しない場合は新しくDataオブジェクトを作成して追加
        if (!_data.ContainsKey(key))
        {
            _data[key] = new Data();
        }
        // ストップウォッチをリスタートして計測を開始
        _data[key].Sw.Restart();
    }

    /// <summary>
    /// 指定されたキーの処理の計測を終了し、結果を記録・集計します。
    /// </summary>
    /// <param name="key">計測対象を識別する一意のキー</param>
    public static void End(string key)
    {
        if (Statics.IsBeta == false)
        {
            return;
        }
        // 辞書にキーが存在しない場合は何もしない
        if (!_data.TryGetValue(key, out var d))
        {
            return;
        }

        // ストップウォッチを停止
        d.Sw.Stop();
        // 経過時間をミリ秒で取得
        var elapsedMs = d.Sw.Elapsed.TotalMilliseconds;

        // 各種データを更新
        d.LastMs = elapsedMs;
        d.TotalMs += elapsedMs;
        d.Count++;
    }

    /// <summary>
    /// 記録されている全てのパフォーマンス統計情報を取得します。
    /// </summary>
    /// <returns>処理名、最新の処理時間、平均処理時間を含むタプルのシーケンス</returns>
    public static IEnumerable<(string Key, double LastMs, double AvgMs)> GetStats()
    {
        // 辞書からデータを抽出し、キーで昇順にソートしてから統計情報を生成
        return _data.OrderBy(kv => kv.Key)
                    .Select(kv => (
                        kv.Key,
                        kv.Value.LastMs,
                        // 平均時間を計算（計測回数が0の場合は0を返す）
                        kv.Value.Count > 0 ? kv.Value.TotalMs / kv.Value.Count : 0.0
                    ));
    }

    /// <summary>
    /// 計測データをリセットします。
    /// </summary>
    /// <param name="key">リセットするデータのキー。nullの場合は全てのデータをリセットします。</param>
    public static void Reset(string key = null)
    {
        if (Statics.IsBeta == false)
        {
            return;
        }
        if (string.IsNullOrEmpty(key))
        {
            // 全てのデータをクリア
            _data.Clear();
        }
        else if (_data.ContainsKey(key))
        {
            // 指定されたキーのデータのみを削除
            _data.Remove(key);
        }
    }
}
