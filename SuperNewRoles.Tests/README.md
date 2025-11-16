# SuperNewRoles.Tests — Unity/IL2CPP dependencies

このテストプロジェクトは .NET 8 (xUnit) で実行され、製品本体 (net6.0) の一部ロジックを検証します。Among Us 本体や Unity/IL2CPP の実行環境を立ち上げずに、管理対象のスタブ DLL を参照してビルド・実行する方針です。

- 参照するのは「管理対象のスタブ DLL」のみです。
  - `Assembly-CSharp.dll`, `UnityEngine*.dll`, `Il2CppSystem.dll`, `Il2Cppmscorlib.dll`, `BepInEx.Core.dll`, `Il2CppInterop.Runtime.dll` 等の“managed” DLL を参照します。
  - ネイティブの `GameAssembly.dll` は .NET の参照対象ではなく、テスト実行にも不要です。参照させても解決になりません。
- 実行時の安全策
  - ロガーはプラグイン未初期化時に Console にフォールバックします。
  - `CustomOption` は `AmongUsClient` アクセスを try/catch で安全化しており、テスト環境ではローカル扱いで動作します。
  - `BaseDirectory` は BepInEx 未設定時に `AppContext.BaseDirectory` を使用します。
- IL2CPP/Unity に依存する重い経路（Harmony パッチ、ClassInjector、実ゲームオブジェクト操作）にはテストから到達しないようにしています。

## 実行方法 (ローカル)

一般的な開発環境では、製品のビルド要件と同様にシステム環境変数 `AmongUs` が既に設定されています。この場合、追加の `extracted_lib` は不要で、Debug/Release どちらでもテストを実行できます。

```
# AmongUs 環境変数が設定済みの場合
dotnet test -c Debug
# もしくは
dotnet test -c Release
```

テストプロジェクトは `AmongUs` のパスから自動で `$(AmongUs)/BepInEx` を検出し、必要な managed DLL を参照します。

環境変数が用意できない最小構成（CI など）では、従来通り `-p:Build=/path/to/BepInEx` を明示指定してください。

```
dotnet test -c Release -p:Build=/absolute/path/to/BepInEx
```

- 参照するのは managed DLL のみで、`GameAssembly.dll` の配置・参照は不要です。

## よくある質問

- Q: GameAssembly を参照すれば解決しますか？
  - A: いいえ。`GameAssembly.dll` はネイティブ (IL2CPP) バイナリのため .NET の参照対象ではありません。テストは managed スタブ DLL への参照で十分です。
- Q: IL2CPP ランタイムが無いと失敗しませんか？
  - A: テストはランタイム依存経路を避け、必要な型解決を managed スタブで満たす構成にしてあります。ユニットテストが触れる範囲では IL2CPP の初期化は不要です。さらに、`ModHelpers.IsAndroid()` などプラットフォーム判定は例外を握りつぶして安全にフォールバックするよう防御実装しています。
