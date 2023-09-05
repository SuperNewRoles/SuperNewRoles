これはCreateRoleAdvance.py(CRA)です

<<--使えないときの対処法-->>
・一部ソースコードのコメントが欠如している
・CRAはコメントを基準にしてソースを書き込んでいます。なのでここが欠如しているとうまく動作しない可能性があります

・コメントの場所
    ・CustomRPC/CustomRPC.cs        →//RoleId

    ・Roles/Role/RoleHelper.cs      →// ロールチェック          GetRoleの後ろ
    ・Roles/Role/RoleHelper.cs      →// ロールアド              SetRoleの後ろ
    ・Roles/Role/RoleHelper.cs      →``    // ロールリモベ``    ClearRoleの後ろ		
    ・Roles/Role/RoleHelper.cs      →``;\n    // 第三か``       IsNeutralの後ろ
    ・Roles/Role/RoleHelper.cs      →// ベントが使える          IsUseVentの後ろ (``_ => player.IsImpostor(),`` の上)
    ・Roles/Role/RoleHelper.cs      →// インポの視界            IsImpostorLightの後ろ

    ・Roles/Role/RoleClass.cs       →// ロールクリア            ``Quarreled.ClearAndReload();``の上

    ・Modules/IntroData.cs            →// イントロオブジェ         一番下

    ・Modules/CustomOptionHolder.cs→// SetupImpostorCustomOptions      /* |: ========================= Neutral Settings ========================== :| */ の上(間に空行を入れる)
    ・Modules/CustomOptionHolder.cs→// SetupNeutralCustomOptions       /* |: ========================= Crewmate Settings ========================== :| */ の上(間に空行を入れる)
    ・Modules/CustomOptionHolder.cs→// SetupCrewmateCustomOptions      /* |: ========================= Modifiers Settings ========================== :| */ の上(間に空行を入れる)

    ・Buttons/Button.cs             →// SetupCustomButtons       ``SetCustomButtonCooldowns();`` の上(間に空行を入れる)

    ・Roles/Sheriff.cs              →//シェリフキルゥ           IsSheriffKillの下(return false;の前)
    ・Roles/Sheriff.cs              →//リモシェリフキルゥ       IsRemoteSheriffKillの下(return false;の前)

    ・Resources/Translate.csv       →``\n#NewRoleTranslation``      \n#ConfigRoles の上