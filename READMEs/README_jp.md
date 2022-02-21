# SuperNewRoles
![SNRImage](/images/SNRImage.png)

<center>
This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.
</center><br><center>訳:これはInnerslothが作ったものじゃなくて個人が作ったものなのでInnerslothは関係ないよ!あとこの中にある素材の一部はInnerslothのものだよ！</center>

## READMEs in other languages(他の言語のREADME)

[English](https://github.com/ykundesu/SuperNewRoles/blob/master/README.md)

[日本語←](https://github.com/ykundesu/SuperNewRoles/blob/master/READMEs/README_jp.md)

## クレジット

[TheOtherRoles](https://github.com/Eisbison/TheOtherRoles) modの作り方の参考にさせていただきました。**Eisbision**さんありがとうございます！

[TheOtherRolesGM](https://github.com/yukinogatari/TheOtherRoles-GM) オプションやボタンなどのソースコードを参考にさせていただきました。**yukinogatari**さんありがとうございます！

[Essentials](https://github.com/DorCoMaNdO/Reactor-Essentials) - カスタムゲームオプション
**DorCoMaNdO**: 
- v1.6より前：デフォルトのEssentialsリリースを使用しました
- v1.6-v1.8：デフォルトのEssentialsを少し変更しました。 変更 この ブランチに は、フォークの あります。
- v2.0.0以降：Reactorを使用しなくなったため、 実装に触発された独自の実装を使用しています。 **DorCoMaNdO**の

(TheOtherRolesより&Google翻訳より)

[BepInEx](https://github.com/BepInEx) - modを適用するために使いました。<br>
[JackalとSidekick](https://www.twitch.tv/dhalucard) - 元のアイデアは **Dhalucard**から来ています。(TheOtherRolesより)<br>
[Town Of Host](https://github.com/tukasa0001/TownOfHost) - デバッグ用に使用。DEBUGモードを使用させてもらいました。**tukasa0001**さんありがとうございます！<br>
[Jester](https://github.com/Maartii/Jester) - Jester(てるてる)のアイデアは、 **Maartii** から来ています。(TheOtherRolesから引用)<br>
[Among-Us-Sheriff-Mod](https://github.com/Woodi-dev/Among-Us-Sheriff-Mod) - Sheriffのアイデアは **Woodi-dev** から来ています。(TheOtherRolesから引用)<br>
[au.libhalt.net](https://au.libhalt.net) - ナイス猫又/イビル猫又のアイデアは **au.libhalt.net** から来ています。

## 連絡について

Discordサーバーからお願いします:[Discordサーバー](https://discord.gg/95YuUZp4kM)

## 役職一覧
Discordサーバーにはすでに乗っています。
| インポスター | クルーメイト | 第三陣営 | 重複陣営 |
|----------|-------------|-----------------|----------------|
| [テレポーター](#テレポーター) | [ライター](#Lighter) | [てるてる](#Jester) | [クラード](#Quarreled) |
| [イビルスピードブースター](#イビルスピードブースター) | [シェリフ](#Sheriff) | [オポチュニスト](#Opportunist) |  |
| [イビルドアー](#イビルドアー) | [スピードブースター](#SpeedBooster) | [ジャッカル](#Jackal) |  |
| [イビルギャンブラー](#イビルギャンブラー) | [ドアー](#Doorr) | [サイドキック](#Sidekick) |  |
| [自爆魔](#自爆魔) | [聖職者](#Clergyman) | [神](#God) | |
| [イビル猫又](#EvilNekomata) | [マッドメイト](#Madmate) | | |
|  | [ベイト](#Bait) |  |  |
|  | [自宅警備員](#HomeSecurityGuard) |  |  |
|  | [スタントマン](#Stuntman) |  |  |
|  | [ムービング](#Moving) |  |  |
|  | [ベスト冤罪ヤー](#BestFalseCharge) |  |  |
|  | [ナイス猫又](#NiceNekomata) |  |  |

# 役職の詳細

## 注意
人数は設定から省いています。

## テレポーター

テレポートボタンを押すことで、
全員をランダムに選ばれた１人にテレポートさせます。

### ゲーム設定
| 名前 | 説明 |
|----------|:-------------:|
| クールダウン | ボタンのクールダウンです。
-----------------------

## イビルスピードブースター

スピードブーストボタンを押すことで、
自分のスピードを一定時間早くすることができます。

### ゲーム設定
| 名前 | 説明 |
|----------|:-------------:|
| クールダウン | ボタンのクールダウンです。
| 継続時間 | スピードブースト継続時間です。
| スピードの倍速 | ブースト中のスピードです。
| この役職が選ばれるとスピードブースターが選ばれない | この役職が選ばれると、スピードブースターが選ばれなくなるようにできます。
-----------------------

## イビルドアー

ドアーボタンをクリックすることで、
扉を開閉することができます。クールタイムがあります。

### ゲーム設定
| 名前 | 説明 |
|----------|:-------------:|
| ドアクールタイム | ボタンのクールダウンです。
-----------------------

## イビルギャンブラー

キルをすると設定した確率で成功、失敗が決められます。<br>
成功になると設定の成功時のキルクールダウンに、<br>
失敗になると設定の失敗時のキルクールダウンになります。<br>
成功時には短いクール、失敗時には長いクールを設定することをおすすめします。<br>

### ゲーム設定
| 名前 | 説明 |
|----------|:-------------:|
| 成功時のキルクールタイム | 成功したときのキルクールタイムです。
| 失敗時のキルクールタイム | 失敗したときのキルクールタイムです。
| 成功確率 | ギャンブルに成功する確率です。
-----------------------

## 自爆魔

自爆ボタンを押すと、
自分の周りにいる人と自分が死亡します。<br>
別のインポスターも対象に選ばれるので注意です。

### ゲーム設定
| 名前 | 説明 |
|----------|:-------------:|
| 範囲 | 自爆の範囲です。
-----------------------

## イビル猫又

自分が会議で追放されると、
その役職関係なく誰か一人を道連れにします。<br>
猫又同士が連鎖するかはナイス猫又の方の設定で変更できます。
-----------------------
