## import
import string
from tkinter import getboolean
import PySimpleGUI as psg
import configparser
import sys
from pathlib import Path
from codecs import Codec
from encodings import utf_8
from importlib.resources import Resource

## 関数
# オブジェクト指向用
class ReturnClass:
    def WriteCodes(self, Path, OldCode, NewCode):
    # w→書く　r→読む　a→合成　r+既存を読む　w+→新規で書く　a+→追加読み書き　t→テキストモード　b→バイナリモード
        with open(BasePath+Path, mode="r", encoding="utf-8") as r:
            Template = r.read()
            with open(BasePath+Path, mode="w", encoding="utf-8") as w:
                Template = Template.replace(OldCode, NewCode)
                print("ファイルを書き込みました:"+Template)
                print("パス:"+BasePath+Path)
                w.write(Template)
    #入力をゲット+戻り値として返す
    def GetInput(self, key):
        if values[key] == "":
            MainClass.CreateErrorWindow("エラーが発生しました。\n値が空白です\n" + "Key:" + key)
            return
        else:
            print("入力した値を読み込みました:" + values[key])
            return values[key]
    def GetBool(self, key):
        if values[key]:
            return True
        else:
            return False
    # Bool(C#用)
    def GetCBool(self, key):
        if values[key]:
            return "true"
        else:
            return "false"
    #画像読み込み
    def GetResource(self, ResourceName):
        print("画像を読み込みました:" + ResourcePath + ResourceName)
        return ResourcePath + ResourceName
    #Config読み込み
    def GetConfig(self, MainPath, SubPath):
        Modify = Config_ini[MainPath][SubPath]
        print("Configを読み込みました:" + Modify)
        return Modify
    #選択から色を返す
    def GetRoleColor(self):
        if (MainClass.GetInput("ColorHash") != "ImposterRed"):
            Template = "new Color32(RGBCOLOR, byte.MaxValue)".replace("RGBCOLOR", str(MainClass.HashToRGB()))
            print("ハッシュを取得しました:", Template)
            return Template
        elif (MainClass.GetBool("ImpoColor")):
            print("インポ色を取得しました")
            return "ImpostorRed"
        elif (MainClass.GetBool("CrewColor")):
            print("クルー色を取得しました")
            return "new Color32(0, 255, 0, byte.MaxValue)"
    #ハッシュをRGBに変換
    def HashToRGB(self):
        Hash = MainClass.GetInput("ColorHash")
        RGB = int(Hash[1:3],16), int(Hash[3:5],16), int(Hash[5:7],16)
        print(RGB)
        return RGB
    #チーム取得
    def GetTeam(self):
        if (MainClass.GetBool("Impo")):
            return "Impo"
        elif (MainClass.GetBool("Crew")):
            return "Crew"
        elif (MainClass.GetBool("Neut")):
            return "Neut"
    # CustomOption重複防止
    Num = 0
    Access = False
    def PlusIDNum(self):
        #if (MainClass.Access):
            #MainClass.Access = True
        PlusNum = MainClass.Num + 1
        MainClass.Num = PlusNum
        Return = str(int(MainClass.GetInput("OptionNumber"))+MainClass.Num)
        print("PlusID:", PlusNum)
        print("ID:", Return)
        return Return
    # チェックボックス、ラジオを更新
    def UpdateBool(self, key, bool):
        MainWindow[key].Update(value = bool)
    # 上の表示板
    #def UpdateGUI(self, key, bool):
        #MainWindow[key].Update(disabled = bool)

    # エラーウィンドウ作成
    def CreateErrorWindow(self, text):
        ErrorPop = psg.popup_error(text,title="エラー")
        print("エラー:"+text)
        while True:
            if ErrorPop == "Error":
                MainWindow.close()
                sys.exit()
    # 通知作成
    def CreateNotify(self, Title, Text):
        psg.popup_notify(Text,title=Title)
    # OK_Cancel系統
    def CreateOKCancelWindow(self, Title, Text):
        ComfirmPop = psg.popup_ok_cancel(Text, title=Title)
        while True:
            if ComfirmPop == "OK":
                return "OK"
            else:
                return "Cancel"
    # OK系統
    def CreateOKWindow(self, Title, Text):
        OKPop = psg.popup_ok(Text, title=Title)
        while True:
            if OKPop == "OK":
                return "OK"


# 戻り値なし
class AllCheck:

    # 確認(空白だったりしたらエラーを起こすように)
    def AllCheck(self):
        MainClass.GetInput("RoleName")
        MainClass.GetRoleColor()
        MainClass.GetTeam()
        if (MainClass.GetBool("AddSetting")):
            MainClass.GetInput("OptionNumber")
            #MainClass.PlusIDNum()

        MainClass.GetBool("A_CreateFile")
        MainClass.GetBool("A_ClearTask")

        # 未作成機能のブロック
        if (MainClass.GetBool("A_CanVisibleImpo")):
            MainClass.CreateOKWindow("インポの視認は現在対応していません")
            return
        if (MainClass.GetBool("TeamOne")):
            MainClass.CreateOKWindow("第三陣営(個人)は現在対応していません")
            return
        if (MainClass.GetBool("TeamTwo")):
            MainClass.CreateOKWindow("第三陣営(ペア)は現在対応していません")
            return
        if (MainClass.GetBool("A_CustomButton")):
            MainClass.CreateOKWindow("カスタムボタンは現在対応していません")
        if (MainClass.GetBool("A_PersonalWin")):
            MainClass.CreateOKWindow("独自勝利辞書追加は現在対応していません")
        # 一部値がかぶっていないか(例:インポ+キル可能)
        if (MainClass.GetBool("A_CanVent")):
            if (MainClass.GetBool("Impo")):
                MainClass.CreateOKWindow("警告", "インポスターはデフォルトで\nキルボタンが作成されます")
                return
        if (MainClass.GetBool("A_CanKill")):
            if (MainClass.GetBool("Impo")):
                MainClass.CreateOKWindow("警告", "インポスターはデフォルトで\nベントボタンが作成されます")
                return
        if (MainClass.GetBool("A_ClearTask")):
            if (MainClass.GetBool("Neut")):
                MainClass.CreateOKWindow("警告", "第三陣営はデフォルトで\nタスクが削除されます")
        if (MainClass.GetBool("A_CanSheriffKill")):
            if (MainClass.GetBool("Neut")):
                MainClass.CreateOKWindow("警告", "第三陣営はデフォルトで\nシェリフがキルできます")
        # 全部書く
        AllActClass.AllWrite()

    # すべて書く
    def AllWrite(self):
        # Roles/ROLENAME.cs
        if (MainClass.GetBool("A_CreateFile")):
            MainClass.WriteCodes("Roles/ROLENAME.cs".replace("ROLENAME", MainClass.GetInput("RoleName")), "",
"""using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Roles
{
    internal class ROLENAME
    {
        //ここにコードを書きこんでください
    }
}""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        # CustomRPC/CustomRPC.cs
        MainClass.WriteCodes("CustomRPC/CustomRPC.cs", "//RoleId", MainClass.GetInput("RoleName")+",\n        //RoleId")
        MainClass.WriteCodes("CustomRPC/CustomRPC.cs", "//新ロールクラス",
                                """public static class ROLENAME
        {
            public static List<PlayerControl> ROLENAMEPlayer;
            public static Color32 color = COLORS;
            //その他Option
            public static void ClearAndReload()
            {
                ROLENAMEPlayer = new List<PlayerControl>();
                //その他クリｱァ
            }
        }\n        //新ロールクラス""".replace("ROLENAME",MainClass.GetInput("RoleName")).replace("COLORS",MainClass.GetRoleColor()))

        # AllRoleSetClass.cs
        MainClass.WriteCodes("AllRoleSetClass.cs", "//セットクラス",
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
            }\n        //セットクラス""".replace("ROLEID!!",MainClass.GetInput("RoleName")).replace("TEAM",MainClass.GetTeam()))


        MainClass.WriteCodes("AllRoleSetClass.cs", "//プレイヤーカウント","""case (RoleId.ROLENAME):
                return CustomOption.CustomOptions.ROLENAMEPlayerCount.getFloat();\n                    //プレイヤーカウント""".replace("ROLENAME",MainClass.GetInput("RoleName")))

        # Roles/RoleHelper.cs
        if (not MainClass.GetBool("TeamGhost")):
            MainClass.WriteCodes("Roles/RoleHelper.cs", "//ロールチェック",
                                """else if (Roles.RoleClass.ROLENAME.ROLENAMEPlayer.IsCheckListPlayerControl(player))
                {
                    return CustomRPC.RoleId.ROLENAME;
                }\n                //ロールチェック""".replace("ROLENAME",MainClass.GetInput("RoleName")))
                            
        MainClass.WriteCodes("Roles/RoleHelper.cs", "//ロールアド",
                                """case (CustomRPC.RoleId.ROLENAME):
                    Roles.RoleClass.ROLENAME.ROLENAMEPlayer.Add(player);
                    break;\n                //ロールアド""".replace("ROLENAME",MainClass.GetInput("RoleName")))
        MainClass.WriteCodes("Roles/RoleHelper.cs", "//ロールリモベ",
                                """case (CustomRPC.RoleId.ROLENAME):
                    Roles.RoleClass.ROLENAME.ROLENAMEPlayer.RemoveAll(ClearRemove);
                    break;\n                //ロールリモベ""".replace("ROLENAME",MainClass.GetInput("RoleName")))
        if (MainClass.GetBool("Neut")):
            MainClass.WriteCodes("Roles/RoleHelper.cs", "//第三か",
                                """case (RoleId.ROLENAME):
                    IsNeutral = true;
                    break;\n                //第三か""".replace("ROLENAME",MainClass.GetInput("RoleName")))
            MainClass.WriteCodes("Roles/RoleHelper.cs", "//タスククリアか",
                                """case (RoleId.ROLENAME):
                    IsTaskClear = true;
                    break; \n                //タスククリアか""".replace("ROLENAME",MainClass.GetInput("RoleName")))
        if (MainClass.GetBool("A_ClearTask")):
            MainClass.WriteCodes("Roles/RoleHelper.cs", "//タスククリアか",
                                """case (RoleId.ROLENAME):
                    IsTaskClear = true;
                    break;
                //タスククリアか""".replace("ROLENAME",MainClass.GetInput("RoleName")))

        # Roles/RoleClass.cs
        MainClass.WriteCodes("Roles/RoleClass.cs", "//ロールクリア", MainClass.GetInput("RoleName")+".ClearAndReload();\n            //ロールクリア")
        MainClass.WriteCodes("Roles/RoleClass.cs", "//新ロールクラス",
                                """public static class ROLENAME
        {
            public static List<PlayerControl> ROLENAMEPlayer;
            public static Color32 color = COLORS;
            public static void ClearAndReload()
            {
                ROLENAMEPlayer = new List<PlayerControl>();
                //くりあぁあんどりろぉどぉ
            }
        }\n        //新ロールクラス""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("COLORS", MainClass.GetRoleColor()))
        
        # Intro/IntroDate.cs
        if (MainClass.GetBool("Impo")):
            MainClass.WriteCodes("Intro/IntroDate.cs", "//イントロオブジェ","""public static IntroDate ROLENAMEIntro = new IntroDate("ROLENAME", RoleClass.ROLENAME.color, 1, CustomRPC.RoleId.ROLENAME, TeamRoleType.Impostor);
            //イントロオブジェ""".replace("ROLENAME",MainClass.GetInput("RoleName")))
        elif (MainClass.GetBool("Crew")):
             MainClass.WriteCodes("Intro/IntroDate.cs", "//イントロオブジェ","""public static IntroDate ROLENAMEIntro = new IntroDate("ROLENAME", RoleClass.ROLENAME.color, 1, CustomRPC.RoleId.ROLENAME, TeamRoleType.Crewmate);
            //イントロオブジェ""".replace("ROLENAME",MainClass.GetInput("RoleName")))
        elif (MainClass.GetBool("Neut")):
             MainClass.WriteCodes("Intro/IntroDate.cs", "//イントロオブジェ","""public static IntroDate ROLENAMEIntro = new IntroDate("ROLENAME", RoleClass.ROLENAME.color, 1, CustomRPC.RoleId.ROLENAME, TeamRoleType.Neutral);
            //イントロオブジェ""".replace("ROLENAME",MainClass.GetInput("RoleName")))
             '''MainClass.WriteCodes("Intro/IntroDate.cs", "//イントロ検知","""case (CustomRPC.RoleId.ROLENAME):
                    return ROLENAMEIntro;
                //イントロ検知""".replace("ROLENAME",MainClass.GetInput("RoleName")))'''#⇐なにこれ？
        elif (MainClass.GetBool("TeamOne")):
            print()
        elif (MainClass.GetBool("TeamTwo")):
            print()
        elif (MainClass.GetBool("TeamGhost")):
            MainClass.WriteCodes("Intro/IntroDate.cs", "//イントロオブジェ","""public static IntroDate ROLENAMEIntro = new IntroDate("ROLENAME", RoleClass.ROLENAME.color, 1, CustomRPC.RoleId.ROLENAME, TeamRoleType.Crewmate, true);
            //イントロオブジェ""".replace("ROLENAME",MainClass.GetInput("RoleName")))



        # CustomOption/CustomOptionDate.cs
        MainClass.WriteCodes("CustomOption/CustomOptionDate.cs", "//CustomOption",
        """\n        public static CustomRoleOption ROLENAMEOption;
        public static CustomOption ROLENAMEPlayerCount;\n        //CustomOption""".replace("ROLENAME",MainClass.GetInput("RoleName")))
        if (MainClass.GetBool("AddSetting")):
            if (MainClass.GetBool("TeamImpo") or MainClass.GetBool("TeamGhost")):
                MainClass.WriteCodes("CustomOption/CustomOptionDate.cs", "//表示設定",
                """\n            ROLENAMEOption = new CustomRoleOption(IDNUM, SHRON, CustomOptionType.Impostor, "ROLENAMEName",RoleClass.ROLENAME.color, 1);
            ROLENAMEPlayerCount = CustomOption.Create(IDNUM2, SHRON, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], ROLENAMEOption);\n            //表示設定""".replace("ROLENAME",MainClass.GetInput("RoleName")).replace("IDNUM",MainClass.PlusIDNum()).replace("IDNUM2", MainClass.PlusIDNum()).replace("SHRON",MainClass.GetCBool("IsSHRON")))
            elif (MainClass.GetBool("TeamCrew")):
                MainClass.WriteCodes("CustomOption/CustomOptionDate.cs", "//表示設定",
                """\n            ROLENAMEOption = new CustomRoleOption(IDNUM, SHRON, CustomOptionType.Crewmate, "ROLENAMEName",RoleClass.ROLENAME.color, 1);
            ROLENAMEPlayerCount = CustomOption.Create(IDNUM2, SHRON, CustomOptionType.Crewmate, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ROLENAMEOption);\n            //表示設定""".replace("ROLENAME",MainClass.GetInput("RoleName")).replace("IDNUM",MainClass.PlusIDNum()).replace("IDNUM2", MainClass.PlusIDNum()).replace("SHRON",MainClass.GetCBool("IsSHRON")))
            elif (MainClass.GetBool("TeamNeut")):
                MainClass.WriteCodes("CustomOption/CustomOptionDate.cs", "//表示設定",
                """\n            ROLENAMEOption = new CustomRoleOption(IDNUM, SHRON, CustomOptionType.Neutral, "ROLENAMEName",RoleClass.ROLENAME.color, 1);
            ROLENAMEPlayerCount = CustomOption.Create(IDNUM2, SHRON, CustomOptionType.Neutral, "SettingPlayerCountName", CrewPlayers[0], CrewPlayers[1], CrewPlayers[2], CrewPlayers[3], ROLENAMEOption);\n            //表示設定""".replace("ROLENAME",MainClass.GetInput("RoleName")).replace("IDNUM",MainClass.PlusIDNum()).replace("IDNUM2", MainClass.PlusIDNum()).replace("SHRON",MainClass.GetCBool("IsSHRON")))
            elif (MainClass.GetBool("TeamOne")):
                print("まだできてねぇ")
            elif (MainClass.GetBool("TeamTwo")):
                print("まだできてねぇ")

            #else:
                #MainClass.CreateErrorWindow("設定タブの値が空白です")
        ## シェリフキル
        if (MainClass.GetBool("A_CanSheriffKill") or MainClass.GetBool("Neut")):
            # Roles/Sheriff.cs
            MainClass.WriteCodes("Roles/Sheriff.cs", "//シェリフキルゥ",
            """if (Target.isRole(CustomRPC.RoleId.ROLENAME) && RoleClass.Sheriff.IsMadRoleKill) return true;""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            MainClass.WriteCodes("Roles/Sheriff.cs", "//リモシェリフキルゥ",
            """if (Target.isRole(CustomRPC.RoleId.ROLENAME) && RoleClass.Sheriff.IsMadRoleKill) return true;""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        ## キルボタン
        if (MainClass.GetBool("A_CanKill")):
            # Buttons/CustomButton.cs
            MainClass.WriteCodes("Buttons/Buttons.cs", "//カスタムなボタン達",
            """        public static CustomButton ROLENAMEKillButton""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            MainClass.WriteCodes("Buttons/Buttons.cs", "//クールダウンリセット",
            """        ROLENAME.resetCoolDown();\n        //クールダウンリセット""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            # Roles/ROLENAME.cs
            MainClass.WriteCodes("Roles/ROLENAME.cs".replace("ROLENAME", MainClass.GetInput("RoleName")), "//ここにコードを書きこんでください",
            """        public static void resetCoolDown() {
            HudManagerStartPatch.ROLENAMEKillButton.MaxTimer = RoleClass.ROLENAME.KillCoolDown;
            HudManagerStartPatch.ROLENAMEKillButton.Timer = RoleClass.ROLENAME.KillCoolDown;
        }
        public static void EndMeeting() {
            resetCoolDown();
        }\n        //ここにコードを書き込んでください""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        ## ベントボタン
        if (MainClass.GetBool("A_CanVent")):
            if (MainClass.GetBool("A_CanVentOption")):
                # CustomOption/CustomOptionDate.cs
                MainClass.WriteCodes("CustomOption/CustomOptionDate.cs", "//CustomOption",
                """public static CustomOption ROLENAMEIsUseVent;\n        //CustomOption""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                if (MainClass.GetBool("TeamImpo")):
                    MainClass.WriteCodes("CustomOption/CustomOptionDate.cs", "//表示設定",
                """ROLENAMEIsUseVent = CustomOption.Create(IDNUM, SHRON, CustomOptionType.Impostor, "MadMateUseVentSetting", false, ROLENAMEOption);\n            //表示設定""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("IDNUM",MainClass.PlusIDNum()).replace("SHRON", MainClass.GetCBool("IsSHRON")))
                elif (MainClass.GetBool("TeamCrew")):
                    MainClass.WriteCodes("CustomOption/CustomOptionDate.cs", "//表示設定",
                """ROLENAMEIsUseVent = CustomOption.Create(IDNUM, SHRON, CustomOptionType.Crewmate, "MadMateUseVentSetting", false, ROLENAMEOption);\n            //表示設定""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("IDNUM",MainClass.PlusIDNum()).replace("SHRON", MainClass.GetCBool("IsSHRON")))
                elif (MainClass.GetInput("TeamNeut")):
                    MainClass.WriteCodes("CustomOption/CustomOptionDate.cs", "//表示設定",
                """ROLENAMEIsUseVent = CustomOption.Create(IDNUM, SHRON, CustomOptionType.Neutral, "MadMateUseVentSetting", false, ROLENAMEOption);\n            //表示設定""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("IDNUM",MainClass.PlusIDNum()).replace("SHRON", MainClass.GetCBool("IsSHRON")))
                # Roles/RoleHelper.cs
                if (MainClass.GetBool("TeamGhost")):
                    MainClass.WriteCodes("Roles/RoleHelper.cs", "//ここが幽霊役職",
                 """if (SuperNewRoles.Roles.RoleClass.ROLENAME.ROLENAMEPlayer.IsCheckListPlayerControl(player))
                    {
                        return SuperNewRoles.CustomRPC.RoleId.ROLENAME;
                    }\n                //ここが幽霊役職""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                MainClass.WriteCodes("Roles/RoleHelper.cs", "//ベントが使える",
                 """case RoleId.ROLENAME:
                    return RoleClass.ROLENAME.IsUseVent;\n                //ベントが使える""".replace("ROLENAME", MainClass.GetInput("RoleName")))

                # Roles/RoleClass.cs
                MainClass.WriteCodes("Roles/RoleClass.cs", "//その他Option",
                """public static bool IsUseVent;\n            //その他Option""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                MainClass.WriteCodes("Roles/RoleClass.cs", "//くりあぁあんどりろぉどぉ",
                """IsUseVent = true\n                //くりあぁあんどりろぉどぉ""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            '''else:
                # Roles/RoleHelper.cs
                MainClass.WriteCodes("Roles/RoleHelper.cs", "//ベントが使える",
                """case RoleId.ROLENAME:
                    return RoleClass.ROLENAME.IsUseVent;\n                //ベントが使える""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                # Roles/RoleClass.cs
                MainClass.WriteCodes("Roles/RoleClass.cs", "//その他Option",
                """public static bool IsUseVent;\n            //その他Option""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                # Roles/RoleHelper.cs
                MainClass.WriteCodes("Roles/RoleHelper.cs", "//ベント設定可視化",
                """case RoleId.ROLENAME:
                    returntext = CustomOptions.FoxIsUseVent.name + ":" + CustomOptions.ROLENAMEIsUseVent.getString() + "\n";
                    break;\n                //ベント設定可視化""".replace("ROLENAME", MainClass.GetInput("RoleName")))'''
        ## インポの視界設定
        if (MainClass.GetBool("A_ImpoVisible")):
            # Roles/RoleClass.cs
            MainClass.WriteCodes("Roles/RoleClass.cs", "//その他Option",
                """public static bool IsImpostorLight;\n            //その他Option""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            MainClass.WriteCodes("Roles/RoleClass.cs", "//くりあぁあんどりろぉどぉ",
                 "IsImpostorLight = CustomOptions.MayorFriendsIsImpostorLight.getBool();\n                //くりあぁあんどりろぉどぉ")
            # Roles/RoleHelper.cs
            MainClass.WriteCodes("Roles.RoleHelper.cs", "                //インポの視界",
            """case RoleId.ROLENAME:
                    return RoleClass.ROLENAME.IsImpostorLight;\n                //インポの視界""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # いらないやつ(次実行するときに複数書いてしまうため)の削除　(例:Jackal→//その他Option, NewRole→//その他Optionの場合、二つに書かれてしまうため重複する)
        MainClass.WriteCodes("Roles/RoleHelper.cs", "//ベント設定可視化", "")
        MainClass.WriteCodes("Roles/RoleHelper.cs", "//その他Option", "")
        MainClass.WriteCodes("Roles/RoleHelper.cs", "//くりあぁあんどりろぉどぉ", "")
        #MainClass.WriteCodes("Roles/RoleHelper.cs", "", "")
        # 終了報告
        MainClass.CreateNotify("CreateRoleAdvance.py", "役職の作成が終了しました")

## 変数
'''DevPath = Path(__file__).parent
DevPath /= "../SNRDevTools/"
BasePath = Path(__file__).parent
BasePath /= "../SuperNewRoles/"
ConfigPath = Path(__file__).parent
ConfigPath /= "../SNRDevTools/CreateRoleAdvance/"
ResourcePath = Path(__file__).parent
ResourcePath /= "../SNRDevTools/CreateRoleAdvance/Resources/"'''
DevPath = r"../SNRDevTools/"
BasePath = r"../SuperNewRoles/"
ConfigPath = r"CreateRoleAdvance/"
ResourcePath = r"CreateRoleAdvance/Resources/"
MainClass = ReturnClass()
AllActClass = AllCheck()
## Config
# 宣言
Config_ini = configparser.ConfigParser()
Config_ini.read((ConfigPath+"Config.ini"),encoding="utf_8")








## レイアウト
# メイン画面
psg.theme(MainClass.GetConfig("Main", "Theme"))
MainTab = psg.Tab("メイン", [
                [psg.Text("Role名(英名):",key="RoleNameText"),psg.InputText(MainClass.GetConfig("MainDefaultSetting", "RoleName"),size=(15,1),key="RoleName")],
                [psg.Text("イントロ:",key="IntroText"), psg.Combo(("役職のみ表示","陣営でも表示"),size=(30,2),default_value=MainClass.GetConfig("MainDefaultSetting", "Intro"))],
                [psg.Text("陣営:    ",key="TeamText"),psg.Radio("インポ陣営","TeamName",key="Impo",default=True),psg.Radio("クルー陣営","TeamName",key="Crew"),psg.Radio("第三陣営","TeamName",key="Neut")],
                [psg.Radio("重複陣営(ペア)",group_id="TeamName",key="TeamTwo"), psg.Radio("重複陣営(個人)",group_id="TeamName",key="TeamOne"), psg.Radio("幽霊役職",group_id="TeamName",key="TeamGhost")],
                [psg.Text("役職カラー:",key="ColorText"), psg.Radio("インポ色","RoleColor",key="ImpoColor",default=True), psg.Radio("ナイス緑色","RoleColor",key="CrewColor"),  psg.ColorChooserButton("色選択",key="ColorButton",target="ColorHash")],
                [psg.Text("取得ハッシュ:",key="ColorHashText"), psg.Input("ImposterRed",key="ColorHash")],
                [psg.Text()],
                [psg.Check("設定を追加する",key="AddSetting")],
                [psg.Text(), psg.Text("タブ:",key="SettingTabText"), psg.Radio("インポスター",group_id="OptionTab",key="TeamImpo"), psg.Radio("クルー",group_id="OptionTab",key="TeamCrew"), psg.Radio("第三陣営",group_id="OptionTab",key="TeamNeut")],
                [psg.Text(), psg.Check("SHR対応",key="IsSHRON")],
                [psg.Text(), psg.Text("設定ID(int)",key="OptionNumberIDText"), psg.Input("",key="OptionNumber",size=(10,3))], ])
AdvanceTab = psg.Tab("詳細設定", [
                [psg.Check("役職ファイルを作成する", key="A_CreateFile")],
                [psg.Check("タスクを削除する", key="A_ClearTask")],
                [psg.Check("シェリフにキルされる", key="A_CanSheriffKill")],
                [psg.Text("ボタン")],
                [psg.Text(),psg.Check("ベント", key="A_CanVent")],
                #[psg.Text(),psg.Check("ベント設定の追加", key="A_CanVentOption")],
                [psg.Text(),psg.Check("キルができる", key="A_CanKill")],
                [psg.Text(),psg.Check("カスタムボタン",key="A_CustomButton")],
                [psg.Text("    画像名:"),psg.InputText("", size=(15,1))],
                [psg.Check("インポの視界", key="A_ImpoVisible")],
                [psg.Check("インポを視認可能", key="A_CanVisibleImpo")],
                [psg.Check("独自勝利辞書追加", key="A_PersonalWin")],
                [], ])
'''TeachingTab = psg.Tab("即席コードチェック", [
                [psg.Button("Harmony一覧")],
                [], ])'''#←いらなくね？
CreateTab = psg.Tab("作成", [
                [psg.Button("作成",key="Main_CreateButton", pad=((10,10),(10,10)), size=(15,2))] ])
MainLayOut = [[psg.TabGroup ([[MainTab, AdvanceTab, CreateTab]])]]

MainWindow = psg.Window(title=MainClass.GetConfig("Main", "WindowName"), layout=MainLayOut, size=(MainClass.GetConfig("Main", "SizeX"), MainClass.GetConfig("Main", "SizeY")), icon=MainClass.GetResource("pictures/icon.png"))

## イベントループ
while True:
    event, values = MainWindow.read()
    if event == psg.WIN_CLOSED:
        break

    elif event == "Main_CreateButton":
        LastClicked = MainClass.CreateOKCancelWindow("確認", "役職を作成します。よろしいですか？")
        if LastClicked == "OK":
            AllActClass.AllCheck()

    '''# チェックボックス、ラジオ検知
    if (MainClass.GetBool("AddSetting") == True):
        MainClass.UpdateGUI("SettingTabText", True)
        MainClass.UpdateGUI("TeamImpo", True)
        MainClass.UpdateGUI("TeamCrew", True)
        MainClass.UpdateGUI("TeamNeut", True)
        MainClass.UpdateGUI("IsSHRON", True)
        MainClass.UpdateGUI("OptionNumberIDText", True)
        MainClass.UpdateGUI("OptionNumber", True)
    else:
        MainClass.UpdateGUI("SettingTabText", False)
        MainClass.UpdateGUI("TeamImpo", False)
        MainClass.UpdateGUI("TeamCrew", False)
        MainClass.UpdateGUI("TeamNeut", False)
        MainClass.UpdateGUI("IsSHRON", False)
        MainClass.UpdateGUI("OptionNumberIDText", False)
        MainClass.UpdateGUI("OptionNumber", False)'''

MainWindow.close()