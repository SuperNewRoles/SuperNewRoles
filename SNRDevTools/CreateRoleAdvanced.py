# import
import string
from tkinter import getboolean
import PySimpleGUI as psg
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
            MainClass.CreateOKWindow("値が空白です\n" + "Key:" + key)
            return
        else:
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
        return ResourcePath + ResourceName

    # 選択から色を返す
    def GetRoleColor(self):
        if (MainClass.GetInput("ColorHash") != "ImpostorRed"):
            Template = "new(RGBCOLOR, byte.MaxValue)".replace("RGBCOLOR", str(MainClass.HashToRGB()))
            return Template
        elif (MainClass.GetBool("ImpoColor")):
            return "RoleClass.ImpostorRed"
        elif (MainClass.GetBool("CrewColor")):
            return "Palette.CrewmateBlue"

    # ハッシュをRGBに変換
    def HashToRGB(self):
        Hash = MainClass.GetInput("ColorHash")
        RGB = str(int(Hash[1:3], 16))+", " + \
            str(int(Hash[3:5], 16))+", " + str(int(Hash[5:7], 16))
        #RGB = str(RGB).strip()
        return RGB

    # イントロのサウンド取得
    def GetIntroSoundType(self):
        if (MainClass.GetBool("CrewIntroSound")):
            return "RoleTypes.Crewmate"
        elif (MainClass.GetBool("EngineerIntroSound")):
            return "RoleTypes.Engineer"
        elif (MainClass.GetBool("ScientistIntroSound")):
            return "RoleTypes.Scientist"
        elif (MainClass.GetBool("ImpoIntroSound")):
            return "RoleTypes.Impostor"
        elif (MainClass.GetBool("ShapeIntroSound")):
            return "RoleTypes.Shapeshifter"

    # チーム取得
    def GetTeam(self):
        if (MainClass.GetBool("Impostor")):
            return "Impostor"
        elif (MainClass.GetBool("Crewmate")):
            return "Crewmate"
        elif (MainClass.GetBool("Neutral")):
            return "Neutral"

    # CustomOption重複防止
    '''Num = 0
    def PlusIDNum(self):
        MainClass.Num = int(MainClass.GetInput("OptionNumber"))
        if (MainClass.Num >= 100):
            Return = str(MainClass.Num)
        else:
            Return = "0"+str(MainClass.Num)
        print("ID:", Return)
        return Return'''

    # チェックボックス、ラジオを更新
    def UpdateBool(self, key, bool):
        MainWindow[key].Update(value=bool)

    # エラーウィンドウ作成
    '''def CreateErrorWindow(self, text):
        ErrorPop = psg.popup_error(text, title="エラー")
        print("エラー:"+text)
        while True:
            if ErrorPop == "Error":
                MainWindow.close()
                sys.exit()'''

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

    # 変数
    Interfaces: str = ""
    InterfacesCodes: str = ""

# 戻り値なし
class AllCheck:
    # 確認(空白だったりしたらエラーを起こすように)
    def AllCheck(self):
        MainClass.GetInput("RoleName")
        MainClass.GetTeam()
        MainClass.GetIntroSoundType()
        MainClass.GetRoleColor()
        MainClass.GetInput("OptionNumber")

        # 一部値がかぶっていないか(例:インポ+キル可能)
        if (MainClass.GetBool("A_ITaskHolder")):
            if (MainClass.GetBool("Crewmate")):
                MainClass.CreateOKWindow("警告", "クルーはデフォルトでタスクが割り振られます")
                return
        if (MainClass.GetBool("A_CanSheriffKill_Mad") or MainClass.GetBool("A_CanSheriffKill_Friends")):
            if(not MainClass.GetBool("Crewmate")):
                MainClass.CreateOKWindow("警告", "CRAではクルー以外はマッド、フレンドとして認識されません")
                return
        if (MainClass.GetBool("Madmate")):
            if (not MainClass.GetBool("Crewmate")):
                MainClass.CreateOKWindow("警告", "属性がMadmateの場合は陣営がクルーメイトである必要があります")
                return
        if (MainClass.GetBool("Jackal")):
            if (not MainClass.GetBool("Neutral")):
                MainClass.CreateOKWindow("警告", "属性がJackalの場合は陣営が第三陣営である必要があります")
                return
        if (MainClass.GetBool("A_IVentAvailable")):
            if (MainClass.GetBool("Impostor")):
                MainClass.CreateOKWindow("警告", "インポスターはデフォルトでベントボタンが作成されます")
                return
        if (MainClass.GetBool("A_IKiller")):
            if (MainClass.GetBool("Impostor")):
                MainClass.CreateOKWindow("警告", "インポスターはデフォルトでキルボタンが作成されます")
                return
        # 全部書く
        AllActClass.AllWrite()

    # すべて書く
    def AllWrite(self):
        # 翻訳
        MainClass.WriteCodes("Resources\Translate.csv", "\n#NewRoleTranslation",
                            """ROLENAMEName,ROLENAME,\nROLENAMETitle1,,\nROLENAMEDescription,,\n\n#NewRoleTranslation""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        # CustomRPC/CustomRPC.cs
        MainClass.WriteCodes("Modules/CustomRPC.cs", "//RoleId",
                             MainClass.GetInput("RoleName")+",\n    //RoleId")

        # シェリフキル
        if (MainClass.GetBool("A_CanSheriffKill_Mad")):
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", ";\n        // IsMads",
                                 """ or\n        RoleId.ROLENAME;\n        // IsMads""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        if (MainClass.GetBool("A_CanSheriffKill_Friends")):
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", ";\n        // IsFriends",
                                 """ or\n        RoleId.ROLENAME;\n        // IsFriends""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        MainClass.Interfaces += ", I TEAM".replace(" TEAM", MainClass.GetTeam())
        if (MainClass.GetBool("ISupportSHR")):
            MainClass.Interfaces += ", ISupportSHR"
        ## 基本インターフェース
        # 追加勝利
        if (MainClass.GetBool("A_IAdditionalWinner")):
            MainClass.InterFaces += ", IAdditionalWinner"
        # カスタムボタン
        if (MainClass.GetBool("A_ICustomButton")):
            MainClass.Interfaces += ", ICustomButton"
        # タスクを持つか
        if (MainClass.GetBool("A_ITaskHolder")):
            MainClass.Interfaces += ", ITaskHolder"
        # ベントボタン
        if (MainClass.GetBool("A_IVentAvailable")):
            MainClass.InterFaces += ", IVentAvailable"
        # キルボタン
        if (MainClass.GetBool("A_IKiller")):
            MainClass.WriteCodes("Resources\Translate.csv", "\n#NewRoleTranslation", f"""{MainClass.GetInput("RoleName")}KillCooldownSetting,,\n\n#NewRoleTranslation""")
            MainClass.Interfaces += ", IKiller"
        ## イベント系インターフェース
        # 死亡
        if (MainClass.GetBool("E_IDeathHandler")):
            MainClass.Interfaces += ", IDeathHandler"
        # FixedUpdaterAll
        if (MainClass.GetBool("E_IFixedUpdaterAll")):
            MainClass.Interfaces += ", IFixedUpdaterAll"
        # FixedUpdaterMe
        if (MainClass.GetBool("E_IFixedUpdaterMe")):
            MainClass.Interfaces += ", IFexedUpdaterMe"
        # 切断
        if (MainClass.GetBool("E_IHandleDisconnect")):
            MainClass.Interfaces += ", IHandleDiconnect"
        # イントロ
        if (MainClass.GetBool("E_IIntroHandler")):
            MainClass.Interfaces += ", IIntroHandler"
        # 緊急会議
        if (MainClass.GetBool("E_IMeetingHandler")):
            MainClass.Interfaces += ", IMeetingHandler"
        # RPC
        if (MainClass.GetBool("E_IRpcHandler")):
            MainClass.Interfaces += ", IRpcHandler"
        # シェイプシフト
        if (MainClass.GetBool("E_IShapeshift")):
            MainClass.Interfaces += ", IShapeshift"
        # 追放開始
        if (MainClass.GetBool("E_IWrapUpHandler")):
            MainClass.Interfaces += ", IWrapUpHandler"

        # Roles/Role/TEAM/ROLENAME.cs
        if (MainClass.GetBool("Impostor")):
            idnum = f"""2{MainClass.GetInput("OptionNumber")}00"""
        elif (MainClass.GetBool("Neutral")):
            idnum = f"""3{MainClass.GetInput("OptionNumber")}00"""
        elif (MainClass.GetBool("Crewmate")):
            idnum = f"""4{MainClass.GetInput("OptionNumber")}00"""

        with open(BasePath+"Roles/TEAM/ROLENAME.cs".replace("TEAM", MainClass.GetTeam()).replace("ROLENAME", MainClass.GetInput("RoleName")), mode="x", encoding='UTF-8') as x:
            x.write("""
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.TEAM.ROLENAME;

public class ROLENAME : RoleBaseINTERFACES
{
    public static new RoleInfo Roleinfo = new(
        typeof(ROLENAME),
        (p) => new ROLENAME(p),
        RoleId.ROLENAME,
        "ROLENAME",
        COLORS,
        new(RoleId.ROLENAME, TeamTag.TEAM),
        TeamRoleType.TEAM,
        TeamType.TEAM
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.ROLENAME, OPTIONID, SHRSUPPORT,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.ROLENAME, introSound: INTRO);
    private static void CreateOption()
    {

    }
    public ROLENAME(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }
INTERFACECODE
}"""
            .replace("TEAM", MainClass.GetTeam())
            .replace("ROLENAME", MainClass.GetInput("RoleName"))
            .replace("INTERFACES", MainClass.Interfaces)
            .replace("OPTIONID", idnum)
            .replace("SHRSUPPORT", MainClass.GetCBool("ISupportSHR"))
            .replace("INTRO", MainClass.GetIntroSoundType())
            .replace("COLORS", MainClass.GetRoleColor())
            .replace("INTERFACECODE", MainClass.InterfacesCodes)
            )

        # 終了報告
        #MainClass.CreateNotify("CreateRoleAdvanced.py", "役職の作成が終了しました")


# 変数
'''DevPath = Path(__file__).parent
DevPath /= "../SNRDevTools/"
BasePath = Path(__file__).parent
BasePath /= "../SuperNewRoles/Role/"
ResourcePath = Path(__file__).parent
ResourcePath /= "../SNRDevTools/CreateRoleAdvance/Resources/"'''
DevPath = r"../SNRDevTools/"
BasePath = r"../SuperNewRoles/"
ResourcePath = r"CreateRoleAdvance/Resources/"
MainClass = ReturnClass()
AllActClass = AllCheck()


# レイアウト
# メイン画面
psg.theme('Default1')
MainTab = psg.Tab("メイン", [
    [psg.Text("Role名(英名):", key="RoleNameText"), psg.InputText("", size=(15, 1), key="RoleName")],
    [psg.Text("陣営:    "), psg.Radio("インポスター", "TeamName", key="Impostor", default=True), psg.Radio("クルー", "TeamName", key="Crewmate"), psg.Radio("第三陣営", "TeamName", key="Neutral")],
    [psg.Text("属性:    "), psg.Radio("なし", group_id="Attribute", key="None", default=True), psg.Radio("幽霊役職", group_id="Attribute", key="GhostRole"), psg.Radio("ジャッカル", group_id="Attribute", key="Jackal"), psg.Radio("マッドメイト", group_id="Attribute", key="Madmate")],
    [psg.Text("イントロ音声:", key="IntroSoundText"), psg.Radio("クルー", "IntroSound", key="CrewIntroSound", default=True), psg.Radio("エンジニア", "IntroSound", key="EngineerIntroSound"), psg.Radio("科学者", "IntroSound", key="ScientistIntroSound"), psg.Radio("インポ", "IntroSound", key="ImpoIntroSound"), psg.Radio("シェイプ", "IntroSound", key="ShapeIntroSound")],
    [psg.Text("役職カラー:", key="ColorText"), psg.Radio("インポ色", "RoleColor", key="ImpoColor", default=True), psg.Radio("ナイス緑色", "RoleColor", key="CrewColor"),  psg.ColorChooserButton("色選択", key="ColorButton", target="ColorHash")],
    [psg.Text("取得ハッシュ:", key="ColorHashText"), psg.Input("ImpostorRed", key="ColorHash")],
    [psg.Text()],
    [psg.Text(), psg.Text("タブ:", key="SettingTabText"), psg.Radio("インポスター", group_id="OptionTab", key="TeamImpo", default=True), psg.Radio("クルー", group_id="OptionTab", key="TeamCrew"), psg.Radio("第三陣営", group_id="OptionTab", key="TeamNeut")],
    [psg.Text(), psg.Check("SHR対応", key="ISupportSHR")],
    [psg.Text(), psg.Text("設定ID(3桁の固有Id)", key="OptionNumberIDText"), psg.Input("", key="OptionNumber", size=(10, 3))],
])
AdvanceTab = psg.Tab("基本インターフェース", [
    [psg.Check("追加勝利", key="A_IAdditionalWinner")],
    [psg.Check("カスタムボタン", key="A_ICustomButton")],
    [psg.Check("タスクを持つ", key="A_ITaskHolder")],
    [psg.Check("シェリフキル(マッド)", key="A_CanSheriffKill_Mad")],
    [psg.Check("シェリフキル(フレンズ)", key="A_CanSheriffKill_Friends")],
    [psg.Check("ベント", key="A_IVentAvailable")],
    [psg.Check("キルができる", key="A_IKiller")],
])
EventTab = psg.Tab("イベント", [
    [psg.Check("死亡", key="E_IDeathHandler")],
    [psg.Check("FixedUpdaterAll", key="E_IFixedUpdaterAll")],
    [psg.Check("FixedUpdaterMe", key="E_IFixedUpdaterMe")],
    [psg.Check("切断", key="E_IHandleDisconnect")],
    [psg.Check("イントロ", key="E_IIntroHandler")],
    [psg.Check("緊急会議", key="E_IMeetingHandler")],
    [psg.Check("RPC", key="E_IRpcHandler")],
    [psg.Check("シェイプシフト", key="E_IShapeshift")],
    [psg.Check("追放", key="E_IWrapUpHandler")],
])
CreateTab = psg.Tab("作成", [
    [psg.Button("作成", key="Main_CreateButton", pad=((10, 10), (10, 10)), size=(15, 2))]])
MainLayOut = [[psg.TabGroup([[MainTab, AdvanceTab, EventTab, CreateTab]])]]
MainWindow = psg.Window(title="CreateRoleAdvanced", layout=MainLayOut, size=(600, 500), icon=MainClass.GetResource("pictures/icon.png"))

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
