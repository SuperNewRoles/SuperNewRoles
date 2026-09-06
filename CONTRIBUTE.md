# SuperNewRoles への貢献ガイド

このガイドでは、SuperNewRoles プロジェクトへの貢献方法について説明します。

## 開発環境のセットアップ

### 必要条件
- .NET 8.0 SDK以上（通常ビルドで実行するRPC Weaverが必要とします。.NET 6.0 SDKだけではビルドできません）
- .NET 8.0ランタイム（RPC Weaverの実行と.NET 8テストに必要。SDK 8.0には同梱されています）
- .NET 6.0ランタイム（PC向けHarmonyテストを実行する場合）
- C#を編集できる環境(Visual Studio 2022 もしくは Visual Studio Codeを推奨)
- Among Us（Steam版もしくはEpic Games版）
- BepInEx

### プロジェクトごとの.NET要件

| プロジェクト | ターゲット |
|---|---|
| SuperNewRoles | .NET 6.0。ビルド中に.NET 8のRPC Weaverを実行します |
| SuperNewRoles.RpcWeaver / RpcWeaver.Tests / RpcWeaver.Fixture / SuperNewRoles.Tests | .NET 8.0 |
| SuperNewRoles.Harmony.Tests | 通常は.NET 6.0。Android APK抽出coreの検証を`TestRuntime=net10.0`で行う場合は.NET 10 SDK・ランタイムも必要です |

新しいSDKだけをインストールする場合も、実行対象の.NET 8／.NET 6ランタイムを別途確認してください。ゲーム用DLLのターゲットは.NET 6.0のままです。

### 環境構築手順

1. リポジトリをクローン
```bash
git clone https://github.com/SuperNewRoles/SuperNewRoles.git
cd SuperNewRoles
```

2. Among Usのインストールパス設定
「AmongUs」というシステム環境変数にAmong Usのインストールパスを設定してください。
例:
```
C:\Program Files (x86)\Steam\steamapps\common\Among Us_mymod
```

3. nugetにbepinex.devを追加
以下のコマンドを実行してください。
```
dotnet nuget add source https://nuget.bepinex.dev/v3/index.json -n bepinex.dev
```


## ビルド方法

1. Visual Studio 2022 もしくは Visual Studio Code でソリューションを開く
2. ビルドを実行
3. 成功すると自動的に Among Us の BepInEx/plugins フォルダにDLLがコピーされます

## ライセンス

このプロジェクトのライセンスについては、LICENSEファイルを参照してください。 

## フォルダ構造
- SuperNewRoles
  - SuperNewRolesのメインプロジェクトです。役職や機能関連など、ほとんどのコードが含まれています。
- SuperNewRoles.Tests
  - SuperNewRolesのロジックテスト用プロジェクトです。
- SuperNewRoles.Harmony.Tests
  - SuperNewRolesのHarmonyパッチ事前生成のテスト用プロジェクトです。
- SuperNewRoles.RpcWeaver
  - 起動の高速化のため、RPCの事前処理を行うプロジェクトです。
- SuperNewRoles.RpcWeaver.Tests / SuperNewRoles.RpcWeaver.Fixture
  - SuperNewRoles.RpcWeaverのロジックテスト用プロジェクトです。
- SuperTools
  - SuperNewRolesの開発に使用する / 使用してたツール群です。
  - ログ暗号化用に鍵を生成するツール・ログの復号化を行うツールが含まれています。