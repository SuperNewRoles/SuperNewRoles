# SuperNewRoles への貢献ガイド

このガイドでは、SuperNewRoles プロジェクトへの貢献方法について説明します。

## 開発環境のセットアップ

### 必要条件
- .NET 6.0 SDK
- C#を編集できる環境(Visual Studio 2022 もしくは Visual Studio Codeを推奨)
- Among Us（Steam版もしくはEpic Games版）
- BepInEx

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