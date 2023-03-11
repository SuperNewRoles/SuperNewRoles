これはCreateRoleAdvance.py(CRA)です

<<--使えないときの対処法-->>
・一部ソースコードのコメントが欠如している
　・CRAはコメントを基準にしてソースを書き込んでいます。なのでここが欠如しているとうまく動作しない可能性があります
　・コメントの場所
　　・Roles/ロール名.cs　　  　 　　　→//ここにソースを書き込んでください
　　・CustomRPC/CustomRPC.cs 　 　　　→//新ロールクラス
　　・CustomRPC/CustomRPC.cs 　 　　　→//セットクラス

　　・Roles/RoleHelper.cs　　　 　　　→//ロールチェック		GetRoleの後ろ
　　・Roles/RoleHelper.cs　　　 　　　→//ロールアド			SetRoleの後ろ
　　・Roles/RoleHelper.cs　　　 　　　→//ロールリモベ　　　　　　ClearRoleの後ろ		
　　・Roles/RoleHelper.cs　　　 　　　→//第三か　　　　　　　　　isNeutralの後ろ
　　・Roles/RoleHelper.cs　　　　　　 →//ベント設定可視化        GetOptionsTextの後ろ
　　・Roles/RoleHelper.cs　　　　　   →//ベントが使える　　　　　IsUseVentの後ろ
　　・Roles/RoleHelper.cs　　　　　　 →//インポの視界　　　　　　IsImpostorLightの後ろ

　　・Roles/RoleClass.cs　　 　　 　　→//新ロールクラス　　　　　ClearAndReloadsの後ろ
　　・Roles/RoleClass.cs　 　　　　 　→//ロールクリア　　　　　　下のほうにあるラバーズソースコードの上

　　・Intro/IntroData.cs 　　　　　　 →//イントロオブジェ　　　　一番下
　　・Intro/IntroData.cs 　　　　　　 →//イントロ検知　　　　　　GetIntroDataの後ろ

　　・CustomOption/CustomOptionHolder.cs→//CustomOption　　　　　　クラードの上(上に空行はいれない)
　　・CustomOption/CustomOptionHolder.cs→//表示設定			下のほうのラバーズの上

　　・Buttons/Button.cs　　　　　　　 →//カスタムなボタン達　　	CustomButtonの下
　　・Buttons/Button.cs　　　　　　　 →//クールダウンリセット　　setCustomButtonCooldownsの下
　　
　　・Roles/Sheriff.cs　　　　　　　　→//シェリフキルゥ　　　　　　IsSheriffKillの下(return false;の前)
    ・Roles/Sheriff.cs　　　　　　　　→//リモシェリフキルゥ　　　　IsRemoteSheriffKillの下(return false;の前)