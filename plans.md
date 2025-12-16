# 役職移植計画（OldSuperNewRoles → SuperNewRoles）

対象: **イカ(Squid)** / **サイコメトラ(Psychometrist)** / **遺言伝達者(DyingMessenger)** / **警察医(PoliceSurgeon)**

---

## 1. 旧実装（OldSuperNewRoles）の仕様まとめ

### イカ（Squid） / Crew
- **ボタン**: 警戒( Vigilance )ボタン
- **効果**:
  - 押すと一定時間「警戒状態」になる
  - 警戒中にキルされそうになると **キルを無効化**
  - 成功時:
    - 警戒解除
    - 自分(イカ)に **速度上昇**（一定時間）
    - **フラッシュ**が発生
    - 相手(キラー)側に:
      - 一定時間 **キル不可**（キルクール相当）
      - 一定時間 **視界低下**（設定値）
      - 画面に **イカ墨(インク)エフェクト**
- **設定**（README/旧オプションより）:
  - 警戒クールダウン
  - 警戒持続時間
  - 反撃成功時の移動速度倍率
  - 反撃成功時の速度上昇時間
  - 反撃成功時のキル不可時間
  - 奪う視界（視界倍率）
  - 視界妨害（インク表示）時間

### サイコメトラ（Psychometrist） / Crew
- **ボタン**: 死体を「よみとり」（読取時間あり）
- **効果**（死体に近い状態で使用）:
  - 死体上にテキスト表示（更新され続ける）
    - 任意設定で **死因(FinalStatus)** を表示
    - 任意設定で **死亡推定時間** を表示（誤差あり）
  - 任意設定で **犯人の足跡** を表示（キル後一定時間追跡できる範囲のログから生成）
  - 任意設定で **よみとり後も通報可能**（無効の場合は通報不可にする）
- **設定**（旧オプションより）:
  - クールダウン
  - よみとり時間
  - 死亡推定時間の表示ON/OFF
  - 推定誤差（±秒）
  - 死因表示ON/OFF
  - 足跡表示ON/OFF
  - 足跡追跡可能時間
  - よみとり後も通報可能

### 遺言伝達者（DyingMessenger） / Crew
- **トリガー**: 死体通報時
- **効果**:
  - キル発生から一定時間内に通報できた場合、通報者（遺言伝達者）だけが
    - キラーの **役職**
    - キラーの **色の明暗（Light/Dark）**
    をチャットで受け取る
  - 役職取得の猶予時間と、明暗取得の猶予時間は別設定
- **設定**:
  - 役職を読み取れる時間
  - 明暗を読み取れる時間

### 警察医（PoliceSurgeon） / Crew
- **会議開始時**: 警察医に「死体検案書(Post Mortem Certificate)」が送信される
- **会議UI**:
  - MeetingHudの各プレイヤーネームプレートにボタンを追加
  - 押すと個人の検案書をオーバーレイ表示（自分ボタンは全文をチャット表示）
- **死亡情報**:
  - 「タスク中の死亡(キル)」は **死亡推定秒数**（通報時点からの経過秒）を表示
  - 会議中死亡/追放/その他は「不詳(頃)」扱い
  - 任意で **誤差(±秒)** を含める
- **設定**（旧オプションより）:
  - タスク中に携帯バイタルを所持（Scientist相当）
  - 検案書の再送可否
  - 死亡ターン以降も表記するか
  - 何ターン前に死亡したかを表示するか
  - 誤差を含める/誤差幅
  - 台湾暦表記（TChineseのみ想定）

---

## 2. 現行（SuperNewRoles）の役職実装方式（移植先の設計）

### 役職の基本
- `Roles/RoleBase.cs` の `RoleBase<T>` 実装を追加すると `CustomRoleManager.Load()`（Reflection）で自動ロードされる
- 役職能力は `Abilities: List<Func<AbilityBase>>` で付与し、`ExPlayerControl.AddAbility()` により
  - ローカルプレイヤー: `AttachToLocalPlayer()`
  - 他プレイヤー: `AttachToOthers()`
  - 全員共通: `AttachToAlls()`
  が呼ばれる

### ボタン能力
- `CustomButtonBase` + `IButtonEffect` で
  - クールダウン
  - 効果時間（押下後のカウントダウン）
  - 途中キャンセル可否
  を実装できる

### キル判定フック（キルガード/キル禁止に必須）
- `CustomDeathExtensions.CustomDeath(CustomDeathType.Kill, source)` 内で `TryKillEvent.Invoke(source, ref target)` が呼ばれる
- `TryKillEvent` の `data.RefSuccess=false` にすると **キルそのものを不成立**にできる
  - 既存例: `StuntmanAbility` など

### 死亡情報
- `MurderDataManager.AddMurderData(killer, target)` が成功キル時に呼ばれる（現状は時刻無し）
  - 遺言伝達者/サイコメトラのため **死亡時刻** が必要 → `MurderData` に時刻を追加する

### 通報フック（遺言伝達者に必須）
- `Events/PCEvents/ReportDeadBodyEvent.cs` の `ReportDeadBodyHostEvent` が **ホスト時のみ**発火する
  - ここで通報者の役職判定 → 条件成立なら通報者にのみメッセージを送る

### 会議フック（警察医に必須）
- `Events/MeetingEvent.cs` の `MeetingStartEvent` / `MeetingCloseEvent` を利用して
  - 会議開始時に「検案書送信」
  - 会議開始/終了タイミングで死亡情報の記録
  - MeetingHud上のネームプレートへボタン追加
- オーバーレイ表示は `HudManager.Instance.FullScreen` / `TaskPanel.taskText` の複製で簡易実装可能

### 視界フック（イカの視界妨害に必須）
- `ShipStatus.CalculateLightRadius` が `ShipStatusLightEvent.Invoke(player, radius)` を通る
  - `ShipStatusLightEvent` にリスナーを追加し、特定プレイヤーのみ `data.lightRadius` を倍率で下げる

---

## 3. 実装方針（移植作業）

### 共通
- `Roles/RoleEnums.cs` の `RoleId` に `Squid` / `DyingMessenger` を **末尾追加**（既存値の破壊を避ける）
- `Resources/TranslationData.csv` に
  - 役職名、Intro、Description
  - ボタン名/メッセージ
  - オプション表示名
  を追加
- `Modules/MurderData.cs` の `MurderData` に `DeathTimeUtc` を追加（通報/推定死亡時間用）

### イカ（Squid）
- 新規: `SuperNewRoles/Roles/Crewmate/Squid.cs`
  - `SquidVigilanceAbility : CustomButtonBase, IButtonEffect`
    - ボタンで警戒開始/終了
    - `TryKillEvent` を監視し、警戒中の自分が狙われたら `RefSuccess=false`（キルガード）
    - 成功時の副作用:
      - 自分: 速度上昇タイマー開始、フラッシュ
      - 相手: キル不可タイマー開始、視界低下タイマー開始、インクオーバーレイ表示
  - 視界低下: `ShipStatusLightEvent` で該当キラーのみ倍率適用
  - インク: HUD上にランダムな黒いスプライト（なければ簡易生成スプライト）を一定時間表示

### サイコメトラ（Psychometrist）
- 新規: `SuperNewRoles/Roles/Crewmate/Psychometrist.cs`
  - `PsychometristReadAbility : CustomButtonBase, IButtonEffect`
    - 近くの死体をターゲットにして読取開始（読取時間）
    - 読取完了で死体上にTextMeshProを生成し、以後更新
    - オプションで:
      - 死因表示（`FinalStatus` → `FinalStatus.*` 翻訳）
      - 死亡推定（`DeathTimeUtc` との差分 + 乱数誤差）
      - 足跡表示（MurderEventで一定時間だけキラー位置をサンプリングしたログから生成）
      - よみとり後通報不可（ホスト通報Patch側でブロック & DeadBody.Reported をRPCで反映）

### 遺言伝達者（DyingMessenger）
- 新規: `SuperNewRoles/Roles/Crewmate/DyingMessenger.cs`
  - ボタン無しのパッシブ
  - `ReportDeadBodyHostEvent`（ホスト）で
    - 通報者が遺言伝達者なら死亡時刻を参照して判定
    - 条件成立分だけ「通報者本人にだけ」チャット/警告メッセージを送信（CustomRPCで対象IDチェック）

### 警察医（PoliceSurgeon）
- 新規: `SuperNewRoles/Roles/Crewmate/PoliceSurgeon.cs`
  - `PoliceSurgeonMeetingAbility : AbilityBase`
    - `MeetingStartEvent`:
      - 未記録の死亡者を走査して死亡情報を記録（タスク中キルは `MurderData.DeathTimeUtc` から経過秒を算出）
      - 設定に応じて「検案書全文」をチャット表示
      - MeetingHud上に「検案ボタン」を生成（対象: 死亡者 + 設定で自分）
    - `MeetingCloseEvent`:
      - 会議中死亡/追放を記録（次会議で参照）
    - `HudUpdateEvent`:
      - Esc/チャット表示などでオーバーレイを閉じる
  - 表示文言は旧 `PostMortemCertificate_*` を `TranslationData.csv` に移植して流用
  - 携帯バイタルは設定ON時のみ有効化（まずはボタン実装/必要なら後から拡張）

---

## 4. 作業順
1) `RoleId` / 翻訳 / MurderData 拡張（基盤）
2) Squid 実装（TryKill + 視界 + オーバーレイ）
3) Psychometrist 実装（読取ボタン + 表示 + 足跡 + 通報ブロック）
4) DyingMessenger 実装（通報ホストフック + 個別メッセージ）
5) PoliceSurgeon 実装（会議送信 + MeetingHudボタン + オーバーレイ）
6) `dotnet build` / テストプロジェクトがあれば実行
