while True:
    rolename = input("Role名:")
    intronum = int(input("イントロ数:"))
    team = input("陣営(0:インポ,1:第三陣営,2:クルー):")
    baseurl = r"C:\\Users\\kazuk\\source\\repos\\SuperNewRoles\\SuperNewRoles\\"
    if team=="0":
        color ="ImpostorRed"
    else:
        color = "new Color32(0, 255, 0, byte.MaxValue)"
    if team=="0":
        ARolename = "Impo"
    elif team=="1":
        ARolename = "Neut"
    elif team=="2":
        ARolename = "Crew"
    if team=="1":
        isneut=True
    else:
        isneut=False
    if team=="0":
        isimpo = True
    else:
        isimpo = False
    with open(baseurl+"CustomRPC\\CustomRPC.cs",mode="r") as r:
        temp = r.read()
        with open(baseurl+"CustomRPC\\CustomRPC.cs",mode="w") as f:
            temp = temp.replace("//RoleId",rolename+",\n        //RoleId")
            f.write(temp)
    with open(baseurl+"Roles\\RoleClass.cs",mode="r",encoding="utf-8") as r:
        temp = r.read()
        with open(baseurl+"Roles\\RoleClass.cs",mode="w",encoding="utf-8") as f:
            temp = temp.replace("//ロールクリア",rolename+".ClearAndReload();\n            //ロールクリア")
            temp = temp.replace("//新ロールクラス",
                                """public static class ROLE!!
        {
            public static List<PlayerControl> ROLE!!Player;
            public static Color32 color = COLORS;
            public static void ClearAndReload()
            {
                ROLE!!Player = new List<PlayerControl>();
            }
        }\n        //新ロールクラス""".replace("ROLE!!",rolename).replace("COLORS",color))
            f.write(temp)
    with open(baseurl+"AllRoleSetClass.cs",mode="r",encoding="utf-8") as r:
        temp = r.read()
        with open(baseurl+"AllRoleSetClass.cs",mode="w",encoding="utf-8") as f:
            temp = temp.replace("//セットクラス",
                                """if (!(CustomOption.CustomOptions.ROLEID!!Option.getString().Replace("0%", "") == ""))
            {
                int OptionDate = int.Parse(CustomOption.CustomOptions.ROLEID!!Option.getString().Replace("0%", ""));
                RoleId ThisRoleId = RoleId.ROLEID!!;
                if (OptionDate == 10)
                {
                    TEAMonepar.Add(ThisRoleId);
                }
                else
                {
                    for (int i = 1; i <= OptionDate; i++)
                    {
                        TEAMnotonepar.Add(ThisRoleId);
                    }
                }
            }\n        //セットクラス""".replace("ROLEID!!",rolename).replace("TEAM",ARolename))
            temp = temp.replace("//プレイヤーカウント","""case (RoleId.ROLENAME):
                    return CustomOption.CustomOptions.ROLENAMEPlayerCount.getFloat();\n                    //プレイヤーカウント""".replace("ROLENAME",rolename))
            f.write(temp)
    with open(baseurl+"Roles\\RoleHelper.cs",mode="r",encoding="utf-8") as r:
        temp = r.read()
        with open(baseurl+"Roles\\RoleHelper.cs",mode="w",encoding="utf-8") as f:
            temp = temp.replace("//ロールチェック",
                                """else if (Roles.RoleClass.ROLENAME.ROLENAMEPlayer.IsCheckListPlayerControl(player))
            {
                return CustomRPC.RoleId.ROLENAME;
            }\n            //ロールチェック""".replace("ROLENAME",rolename))
            temp = temp.replace("//ロールアド",
                                """case (CustomRPC.RoleId.ROLENAME):
                    Roles.RoleClass.ROLENAME.ROLENAMEPlayer.Add(player);
                    break;\n                //ロールアド""".replace("ROLENAME",rolename))
            temp = temp.replace("//ロールリモベ",
                                """case (CustomRPC.RoleId.ROLENAME):
                    Roles.RoleClass.ROLENAME.ROLENAMEPlayer.RemoveAll(ClearRemove);
                    break;\n                //ロールリモベ""".replace("ROLENAME",rolename))
            if isneut:
                temp = temp.replace("//第三か",
                                """case (RoleId.ROLENAME):
                    IsNeutral = true;
                    break;\n                //第三か""".replace("ROLENAME",rolename))
                temp = temp.replace("//タスククリアか",
                                """case (RoleId.ROLENAME):
                    IsTaskClear = true;
                    break; 
                //タスククリアか""".replace("ROLENAME",rolename))
            f.write(temp)
    with open(baseurl+"Intro\\IntroDate.cs",mode="r",encoding="utf-8") as r:
        temp = r.read()
        with open(baseurl+"Intro\\IntroDate.cs",mode="w",encoding="utf-8") as f:
            temp = temp.replace("//イントロオブジェ","""public static IntroDate ROLENAMEIntro = new IntroDate("ROLENAME", RoleClass.ROLENAME.color, 1, CustomRPC.RoleId.ROLENAME);
        //イントロオブジェ""".replace("ROLENAME",rolename))
            temp = temp.replace("//イントロ検知","""case (CustomRPC.RoleId.ROLENAME):
                    return ROLENAMEIntro;
                //イントロ検知""".replace("ROLENAME",rolename))
            f.write(temp)
    print("完了！")
