# import
import string
from tkinter import getboolean
import PySimpleGUI as psg
import configparser
import sys
from pathlib import Path
from codecs import Codec
from encodings import utf_8
from importlib.resources import Resource

# 関数
# オブジェクト指向用


class ReturnClass:
    def WriteCodes(self, Path, OldCode, NewCode):
        # w→書く　r→読む　a→合成　r+既存を読む　w+→新規で書く　a+→追加読み書き　t→テキストモード　b→バイナリモード x→ファイル作成
        with open(BasePath+Path, mode="r", encoding="utf-8") as r:
            Template = r.read()
            with open(BasePath+Path, mode="w", encoding="utf-8") as w:
                Template = Template.replace(OldCode, NewCode)
                print("ファイルを書き込みました:"+Template)
                print("パス:"+BasePath+Path)
                w.write(Template)
    # 入力をゲット+戻り値として返す

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
    # 画像読み込み

    def GetResource(self, ResourceName):
        print("画像を読み込みました:" + ResourcePath + ResourceName)
        return ResourcePath + ResourceName
    # Config読み込み

    def GetConfig(self, MainPath, SubPath):
        #print("Config:"+MainPath + SubPath)
        Modify = Config_ini[MainPath][SubPath]
        print("Configを読み込みました:" + Modify)
        return Modify
    # 選択から色を返す

    def GetRoleColor(self):
        if (MainClass.GetInput("ColorHash") != "ImpostorRed"):
            Template = "new Color32(RGBCOLOR, byte.MaxValue)".replace(
                "RGBCOLOR", str(MainClass.HashToRGB()))
            print("ハッシュを取得しました:", Template)
            return Template
        elif (MainClass.GetBool("ImpoColor")):
            print("インポ色を取得しました")
            return "RoleClass.ImpostorRed"
        elif (MainClass.GetBool("CrewColor")):
            print("クルー色を取得しました")
            return "new Color32(0, 255, 0, byte.MaxValue)"
    # ハッシュをRGBに変換

    def HashToRGB(self):
        Hash = MainClass.GetInput("ColorHash")
        RGB = str(int(Hash[1:3], 16))+", " + \
            str(int(Hash[3:5], 16))+", " + str(int(Hash[5:7], 16))
        #RGB = str(RGB).strip()
        print(RGB)
        return RGB
        print(RGB.strip())
        return RGB.strip()

    # イントロのサウンド取得
    def GetIntroSoundType(self):
        if (MainClass.GetBool("CrewIntroSound")):
            return ""
        elif (MainClass.GetBool("EngineerIntroSound")):
            return ", IntroSound : RoleTypes.Engineer"
        elif (MainClass.GetBool("ScientistIntroSound")):
            return ", IntroSound : RoleTypes.Scientist"
        elif (MainClass.GetBool("ImpoIntroSound")):
            return ", IntroSound : RoleTypes.Impostor"
        elif (MainClass.GetBool("ShapeIntroSound")):
            return ", IntroSound : RoleTypes.Shapeshifter"

    # チーム取得

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
        # if (MainClass.Access):
        #MainClass.Access = True
        PlusNum = MainClass.Num + 1
        MainClass.Num = PlusNum
        Return = str(int(MainClass.GetInput("OptionNumber"))+MainClass.Num)
        print("PlusID:", PlusNum)
        print("ID:", Return)
        return Return
    # チェックボックス、ラジオを更新

    def UpdateBool(self, key, bool):
        MainWindow[key].Update(value=bool)
    # 上の表示板
    # def UpdateGUI(self, key, bool):
        #MainWindow[key].Update(disabled = bool)

    # エラーウィンドウ作成
    def CreateErrorWindow(self, text):
        ErrorPop = psg.popup_error(text, title="エラー")
        print("エラー:"+text)
        while True:
            if ErrorPop == "Error":
                MainWindow.close()
                sys.exit()
    # 通知作成

    def CreateNotify(self, Title, Text):
        psg.popup_notify(Text, title=Title)
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
        MainClass.GetInput("OptionNumber")
        # MainClass.PlusIDNum()

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
        if (MainClass.GetBool("A_CanSheriffKill_Mad") or MainClass.GetBool("A_CanSheriffKill_Friends")):
            if(not MainClass.GetBool("Crew")):
                MainClass.CreateOKWindow("警告", "CRAではクルー以外はマッド、フレンドとして認識されません")
        # 全部書く
        AllActClass.AllWrite()

    # すべて書く
    def AllWrite(self):
        # Roles/Role/ROLENAME.cs
        if (MainClass.GetBool("Impo")):
            namedata = "Impostor"
            playerstype = "CustomOptionHolder.ImpostorPlayers"
        elif (MainClass.GetBool("Neut")):
            namedata = "Neutral"
            playerstype = "CustomOptionHolder.CrewPlayers"
        elif (MainClass.GetBool("Crew")):
            namedata = "Crewmate"
            playerstype = "CustomOptionHolder.CrewPlayers"
        with open(BasePath+"Roles/"+namedata+"/ROLENAME.cs".replace("ROLENAME", MainClass.GetInput("RoleName")), mode="x") as x:
            x.write(
                ("""using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles."""+namedata+""";

public static class ROLENAME
{
    private const int OptionId = IDNUM;
    public static CustomRoleOption ROLENAMEOption;
    public static CustomOption ROLENAMEPlayerCount;
    public static void SetupCustomOptions()
    {
        ROLENAMEOption = CustomOption.SetupCustomRoleOption(OptionId, SHRON, RoleId.ROLENAME);
        ROLENAMEPlayerCount = CustomOption.Create(OptionId + 1, SHRON, CustomOptionType."""+namedata+""", "SettingPlayerCountName", PLAYERSTYPE[0], PLAYERSTYPE[1], PLAYERSTYPE[2], PLAYERSTYPE[3], ROLENAMEOption);
    }
    
    public static List<PlayerControl> ROLENAMEPlayer;
    public static Color32 color = COLORS;
    public static void ClearAndReload()
    {
        ROLENAMEPlayer = new();
    }
    
    // ここにコードを書きこんでください
}""").replace("ROLENAME", MainClass.GetInput("RoleName")).replace("IDNUM", MainClass.PlusIDNum()).replace("SHRON", MainClass.GetCBool("IsSHRON")).replace("PLAYERSTYPE", playerstype).replace("COLORS", MainClass.GetRoleColor()))

        # CustomRPC/CustomRPC.cs
        MainClass.WriteCodes("Modules/CustomRPC.cs", "//RoleId",
                             MainClass.GetInput("RoleName")+",\n    //RoleId")

        # Roles/AllRoleSetClass.cs
        MainClass.WriteCodes("Roles/AllRoleSetClass.cs", "// プレイヤーカウント",
                             """RoleId.ROLENAME => ROLENAME.ROLENAMEPlayerCount.GetFloat(),\n            // プレイヤーカウント""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # Roles/Role/RoleHelper.cs
        if (not MainClass.GetBool("TeamGhost")):
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// ロールチェック",
                                 """else if (ROLENAME.ROLENAMEPlayer.IsCheckListPlayerControl(player)) return RoleId.ROLENAME;
            // ロールチェック""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// ロールアド",
                             """case RoleId.ROLENAME:
                ROLENAME.ROLENAMEPlayer.Add(player);
                break;
            // ロールアド""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// ロールリモベ",
                             """case RoleId.ROLENAME:
                ROLENAME.ROLENAMEPlayer.RemoveAll(ClearRemove);
                break;
            // ロールリモベ""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        if (MainClass.GetBool("Neut")):
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", ";\n        // 第三か",
                                 """ or
        RoleId.ROLENAME;
        // 第三か""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        if (MainClass.GetBool("A_ClearTask")):
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// タスククリアか 個別表記",
                                 """case RoleId.ROLENAME:
            // タスククリアか 個別表記""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # Roles/Role/RoleClass.cs
        MainClass.WriteCodes("Roles/Role/RoleClass.cs", "// ロールクリア", MainClass.GetInput(
            "RoleName")+".ClearAndReload();\n        // ロールクリア")

        # Intro/IntroData.cs
        if (MainClass.GetBool("Impo")):
            MainClass.WriteCodes("Modules/IntroData.cs", "// イントロオブジェ", """public static IntroData ROLENAMEIntro = new("ROLENAME", RoleClass.ROLENAME.color, 1, RoleId.ROLENAME, TeamRoleType.ImpostorROLETYPE);
    // イントロオブジェ""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("ROLETYPE", MainClass.GetIntroSoundType()))
        elif (MainClass.GetBool("Crew")):
            MainClass.WriteCodes("Modules/IntroData.cs", "// イントロオブジェ", """public static IntroData ROLENAMEIntro = new("ROLENAME", RoleClass.ROLENAME.color, 1, RoleId.ROLENAME, TeamRoleType.CrewmateROLETYPE);
    // イントロオブジェ""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("ROLETYPE", MainClass.GetIntroSoundType()))
        elif (MainClass.GetBool("Neut")):
            MainClass.WriteCodes("Modules/IntroData.cs", "// イントロオブジェ", """public static IntroData ROLENAMEIntro = new("ROLENAME", RoleClass.ROLENAME.color, 1, RoleId.ROLENAME, TeamRoleType.NeutralROLETYPE);
    // イントロオブジェ""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("ROLETYPE", MainClass.GetIntroSoundType()))
        elif (MainClass.GetBool("TeamOne")):
            print()
        elif (MainClass.GetBool("TeamTwo")):
            print()
        elif (MainClass.GetBool("TeamGhost")):
            MainClass.WriteCodes("Modules/IntroData.cs", "// イントロオブジェ", """public static IntroData ROLENAMEIntro = new IntroData("ROLENAME", ROLENAME.color, 1, RoleId.ROLENAME, TeamRoleType.Crewmate, true);
    // イントロオブジェ""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # CustomOption/CustomOptionHolder.cs
        MainClass.WriteCodes("Modules/CustomOptionHolder.cs", "// 表示設定",
                             """ROLENAME.SetupCustomOptions();\n\n        // 表示設定""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # シェリフキル
        if (MainClass.GetBool("A_CanSheriffKill_Mad")):
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", ";\n        // IsMads",
                                 """ or\n        RoleId.ROLENAME;\n        // IsMads""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        elif(MainClass.GetBool("A_CanSheriffKill_Friends")):
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", ";\n        // IsFriends",
                                 """ or\n        RoleId.ROLENAME;\n        // IsFriends""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # キルボタン
        '''if (MainClass.GetBool("A_CanKill")):
            # Buttons/CustomButton.cs
            MainClass.WriteCodes("Buttons/Buttons.cs", "//カスタムなボタン達",
                                 """        public static CustomButton ROLENAMEKillButton""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            MainClass.WriteCodes("Buttons/Buttons.cs", "//クールダウンリセット",
                                 """        ROLENAME.resetCooldown();\n        //クールダウンリセット""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            # Roles/Role/ROLENAME.cs
            MainClass.WriteCodes("Roles/Role/ROLENAME.cs".replace("ROLENAME", MainClass.GetInput("RoleName")), "//ここにコードを書きこんでください",
                                 """        public static void resetCooldown() {
            HudManagerStartPatch.ROLENAMEKillButton.MaxTimer = RoleClass.ROLENAME.KillCooldown;
            HudManagerStartPatch.ROLENAMEKillButton.Timer = RoleClass.ROLENAME.KillCooldown;
        }
        public static void EndMeeting() {
            resetCooldown();
        }\n        //ここにコードを書き込んでください""".replace("ROLENAME", MainClass.GetInput("RoleName")))'''

        # ベントボタン
        if (MainClass.GetBool("A_CanVent")):
            if (MainClass.GetBool("A_CanVentOption")):
                # CustomOption/CustomOptionHolder.cs
                MainClass.WriteCodes("CustomOption/CustomOptionHolder.cs", "//CustomOption",
                                     """public static CustomOption ROLENAMEIsUseVent;\n        //CustomOption""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                if (MainClass.GetBool("TeamImpo")):
                    MainClass.WriteCodes("CustomOption/CustomOptionHolder.cs", "//表示設定",
                                         """ROLENAMEIsUseVent = CustomOption.Create(IDNUM, SHRON, CustomOptionType.Impostor, "MadmateUseVentSetting", false, ROLENAMEOption);\n            //表示設定""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("IDNUM", MainClass.PlusIDNum()).replace("SHRON", MainClass.GetCBool("IsSHRON")))
                elif (MainClass.GetBool("TeamCrew")):
                    MainClass.WriteCodes("CustomOption/CustomOptionHolder.cs", "//表示設定",
                                         """ROLENAMEIsUseVent = CustomOption.Create(IDNUM, SHRON, CustomOptionType.Crewmate, "MadmateUseVentSetting", false, ROLENAMEOption);\n            //表示設定""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("IDNUM", MainClass.PlusIDNum()).replace("SHRON", MainClass.GetCBool("IsSHRON")))
                elif (MainClass.GetInput("TeamNeut")):
                    MainClass.WriteCodes("CustomOption/CustomOptionHolder.cs", "//表示設定",
                                         """ROLENAMEIsUseVent = CustomOption.Create(IDNUM, SHRON, CustomOptionType.Neutral, "MadmateUseVentSetting", false, ROLENAMEOption);\n            //表示設定""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("IDNUM", MainClass.PlusIDNum()).replace("SHRON", MainClass.GetCBool("IsSHRON")))
                # Roles/Role/RoleHelper.cs
                if (MainClass.GetBool("TeamGhost")):
                    MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "//ここが幽霊役職",
                                         """if (SuperNewRoles.RoleClass.ROLENAME.ROLENAMEPlayer.IsCheckListPlayerControl(player))
                    {
                        return SuperNewRoles.RoleId.ROLENAME;
                    }\n                //ここが幽霊役職""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "//ベントが使える",
                                     """case RoleId.ROLENAME:
                    return RoleClass.ROLENAME.IsUseVent;\n                //ベントが使える""".replace("ROLENAME", MainClass.GetInput("RoleName")))

                # Roles/Role/RoleClass.cs
                MainClass.WriteCodes("Roles/Role/RoleClass.cs", "//その他Option",
                                     """public static bool IsUseVent;\n            //その他Option""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                MainClass.WriteCodes("Roles/Role/RoleClass.cs", "//くりあぁあんどりろぉどぉ",
                                     """IsUseVent = true\n                //くりあぁあんどりろぉどぉ""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            '''else:
                # Roles/Role/RoleHelper.cs
                MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "//ベントが使える",
                """case RoleId.ROLENAME:
                    return RoleClass.ROLENAME.IsUseVent;\n                //ベントが使える""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                # Roles/Role/RoleClass.cs
                MainClass.WriteCodes("Roles/Role/RoleClass.cs", "//その他Option",
                """public static bool IsUseVent;\n            //その他Option""".replace("ROLENAME", MainClass.GetInput("RoleName")))
                # Roles/Role/RoleHelper.cs
                MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "//ベント設定可視化",
                """case RoleId.ROLENAME:
                    returntext = CustomOptionHolder.FoxIsUseVent.name + ":" + CustomOptionHolder.ROLENAMEIsUseVent.GetString() + "\n";
                    break;\n                //ベント設定可視化""".replace("ROLENAME", MainClass.GetInput("RoleName")))'''
        # インポの視界設定
        if (MainClass.GetBool("A_ImpoVisible")):
            # Roles/Role/RoleClass.cs
            MainClass.WriteCodes("Roles/Role/RoleClass.cs", "//その他Option",
                                 """public static bool IsImpostorLight;\n            //その他Option""".replace("ROLENAME", MainClass.GetInput("RoleName")))
            MainClass.WriteCodes("Roles/Role/RoleClass.cs", "//くりあぁあんどりろぉどぉ",
                                 "IsImpostorLight = CustomOptionHolder.MayorFriendsIsImpostorLight.GetBool();\n                //くりあぁあんどりろぉどぉ")
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles.RoleHelper.cs", "                //インポの視界",
                                 """case RoleId.ROLENAME:
                    return RoleClass.ROLENAME.IsImpostorLight;\n                //インポの視界""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # いらないやつ(次実行するときに複数書いてしまうため)の削除　(例:Jackal→//その他Option, NewRole→//その他Optionの場合、二つに書かれてしまうため重複する)
        #MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "//ベント設定可視化", "")
        MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "//その他Option", "")
        MainClass.WriteCodes("Roles/Role/RoleClass.cs", "//くりあぁあんどりろぉどぉ", "")
        #MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "", "")
        # 終了報告
        MainClass.CreateNotify("CreateRoleAdvanced.py", "役職の作成が終了しました")


# 変数
'''DevPath = Path(__file__).parent
DevPath /= "../SNRDevTools/"
BasePath = Path(__file__).parent
BasePath /= "../SuperNewRoles/Role/"
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
# Config
# 宣言
Config_ini = configparser.ConfigParser()
Config_ini.read((ConfigPath+"Config.ini"), encoding="utf_8")


# レイアウト
# メイン画面
psg.theme(MainClass.GetConfig("Main", "Theme"))
MainTab = psg.Tab("メイン", [
    [psg.Text("Role名(英名):", key="RoleNameText"), psg.InputText(MainClass.GetConfig(
        "MainDefaultSetting", "RoleName"), size=(15, 1), key="RoleName")],
    [psg.Text("イントロ:", key="IntroText"), psg.Combo(("役職のみ表示", "陣営でも表示"), size=(
        30, 2), default_value=MainClass.GetConfig("MainDefaultSetting", "Intro"))],
    [psg.Text("陣営:    ", key="TeamText"), psg.Radio("インポ陣営", "TeamName", key="Impo", default=True),
     psg.Radio("クルー陣営", "TeamName", key="Crew"), psg.Radio("第三陣営", "TeamName", key="Neut")],
    [psg.Radio("重複陣営(ペア)", group_id="TeamName", key="TeamTwo"), psg.Radio(
        "重複陣営(個人)", group_id="TeamName", key="TeamOne"), psg.Radio("幽霊役職", group_id="TeamName", key="TeamGhost")],
    [psg.Text("イントロ音声:", key="IntroSoundText"), psg.Radio("クルー", "IntroSound", key="CrewIntroSound", default=True), psg.Radio("エンジニア", "IntroSound", key="EngineerIntroSound", default=False), psg.Radio("科学者", "IntroSound", key="ScientistIntroSound", default=False)],
     [psg.Radio("インポ", "IntroSound", key="ImpoIntroSound", default=False), psg.Radio("シェイプ", "IntroSound", key="ShapeIntroSound", default=False)],
    [psg.Text("役職カラー:", key="ColorText"), psg.Radio("インポ色", "RoleColor", key="ImpoColor", default=True), psg.Radio(
        "ナイス緑色", "RoleColor", key="CrewColor"),  psg.ColorChooserButton("色選択", key="ColorButton", target="ColorHash")],
    [psg.Text("取得ハッシュ:", key="ColorHashText"),
     psg.Input("ImpostorRed", key="ColorHash")],
    [psg.Text()],
    [psg.Text(), psg.Text("タブ:", key="SettingTabText"), psg.Radio("インポスター", group_id="OptionTab", key="TeamImpo"),
     psg.Radio("クルー", group_id="OptionTab", key="TeamCrew"), psg.Radio("第三陣営", group_id="OptionTab", key="TeamNeut")],
    [psg.Text(), psg.Check("SHR対応", key="IsSHRON")],
    [psg.Text(), psg.Text("設定ID(int)", key="OptionNumberIDText"), psg.Input("", key="OptionNumber", size=(10, 3))], ])
AdvanceTab = psg.Tab("詳細設定", [
    [psg.Check("タスクを削除する", key="A_ClearTask")],
    [psg.Check("シェリフキル(マッド)", key="A_CanSheriffKill_Mad")],
    [psg.Check("シェリフキル(フレンズ)", key="A_CanSheriffKill_Friends")],
    [psg.Text("ボタン")],
    [psg.Text(), psg.Check("ベント", key="A_CanVent")],
    #[psg.Text(),psg.Check("ベント設定の追加", key="A_CanVentOption")],
    [psg.Text(), psg.Check("キルができる", key="A_CanKill")],
    [psg.Text(), psg.Check("カスタムボタン", key="A_CustomButton")],
    [psg.Text("    画像名:"), psg.InputText("", size=(15, 1))],
    [psg.Check("インポの視界", key="A_ImpoVisible")],
    [psg.Check("インポを視認可能", key="A_CanVisibleImpo")],
    [psg.Check("独自勝利辞書追加", key="A_PersonalWin")],
    [], ])
'''TeachingTab = psg.Tab("即席コードチェック", [
                [psg.Button("Harmony一覧")],
                [], ])'''  # ←いらなくね？
CreateTab = psg.Tab("作成", [
    [psg.Button("作成", key="Main_CreateButton", pad=((10, 10), (10, 10)), size=(15, 2))]])
MainLayOut = [[psg.TabGroup([[MainTab, AdvanceTab, CreateTab]])]]

MainWindow = psg.Window(title=MainClass.GetConfig("Main", "WindowName"), layout=MainLayOut, size=(MainClass.GetConfig(
    "Main", "SizeX"), MainClass.GetConfig("Main", "SizeY")), icon=MainClass.GetResource("pictures/icon.png"))

# イベントループ
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
