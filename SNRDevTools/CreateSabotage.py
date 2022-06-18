import string
from tkinter import getboolean
import PySimpleGUI as psg
import configparser
import sys
from pathlib import Path
from codecs import Codec
from encodings import utf_8
from importlib.resources import Resource
#import


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

    #MapId取得

    def GetMapId(self):
        MapId = []
        if (MainClass.GetBool("Skeld")):
            MapId.append("0")
        if (MainClass.GetBool("Mira")):
            MapId.append("1")
        if (MainClass.GetBool("Polus")):
            MapId.append("2")
        if (MainClass.GetBool("Airship")):
            MapId.append("4")
        if MapId == []:
            MainClass.CreateErrorWindow("エラーが発生しました。\n値が空白です\n")
            return
        print("MapId取得:" + str(MapId))
        return MapId
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
        MainWindow[key].Update(value=bool)
    # 上の表示板
    #def UpdateGUI(self, key, bool):
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


class AllCheck:
    def AllCheck(self):
        MainClass.GetInput("SabotageName")
        MainClass.GetMapId()
        if (MainClass.GetBool("AddSetting")):
            MainClass.GetInput("OptionNumber")
        AllActClass.AllWrite()
    def AllWrite(self):
        #Sabotage/SabotageManager
        MainClass.WriteCodes("Sabotage/SabotageManager.cs",
                             "//CustomSabotageId", MainClass.GetInput("SabotageName") + ",\n            //CustomSabotageId")
        for item in MainClass.GetMapId():
            if (item == MainClass.GetMapId()[0]):
                conditions = "PlayerControl.GameOptions.MapId is not " + item
            else:
                conditions = conditions + " and " + item
        MainClass.WriteCodes("Sabotage/SabotageManager.cs", "//カスタムサボタージュが使えるか",
                             "case CustomSabotage." + MainClass.GetInput("SabotageName") + ":\n                    if (" + conditions + ") return false;\n                    else return Options." + MainClass.GetInput("SabotageName") + "Setting.getBool();\n                //カスタムサボタージュが使えるか")
        # Sabotage/Options.cs.cs
        MainClass.WriteCodes("Sabotage/Options.cs", "//CustomOption(Sabotage)",
                            """public static CustomOption.CustomOption SabotageNAMESetting;\n        //CustomOption(Sabotage)""".replace("SabotageNAME", MainClass.GetInput("SabotageName")))
        if (MainClass.GetBool("AddSetting")):
            MainClass.WriteCodes("Sabotage/Options.cs", "//表示設定(Sabotage)",
                                """SabotageNAMESetting = new CustomOption.CustomOption.Create(IDNUM, false, CustomOptionType.Generic, "SabotageNAMESetting",false, SabotageSetting);
                                \n            //表示設定""".replace("SabotageNAME", MainClass.GetInput("SabotageName")).replace("IDNUM", MainClass.PlusIDNum()))

## 変数
DevPath = r"../SNRDevTools/"
BasePath = r"../SuperNewRoles/"
MainClass = ReturnClass()
AllActClass = AllCheck()

## レイアウト
# メイン画面
psg.theme("Default1")
layout = [
    [psg.Text("Sabotage名(英名):", key="RoleNameText"), psg.InputText("", size=(15, 1), key="SabotageName")],
    [psg.Text("マップ:    ", key="MapText"), psg.Checkbox("スケルド", key="Skeld", default=True), psg.Checkbox("ミラ", key="Mira"), psg.Checkbox("ポーラス", key="Polus"), psg.Checkbox("エアシップ", key="Airship")],
    [psg.Check("設定を追加する", key="AddSetting")],
    [psg.Text(), psg.Text("設定ID(int)", key="OptionNumberIDText"), psg.Input("", key="OptionNumber", size=(10, 3))],
    [psg.Button("作成", key="CreateButton", pad=((10, 10), (10, 10)), size=(15, 1))], ]

MainWindow = psg.Window(title="CreateSabotage", layout=layout, size=(500, 600), icon= r"CreateRoleAdvance/Resources/Pictures/icon.ico")

## イベントループ
while True:
    event, values = MainWindow.read()
    if event == psg.WIN_CLOSED:
        break

    elif event == "CreateButton":
        LastClicked = MainClass.CreateOKCancelWindow("確認", "サボタージュを作成します。よろしいですか？")
        if LastClicked == "OK":
            AllActClass.AllCheck()

    '''# チェックボックス、ラジオ検知
    if (MainClass.GetBool("AddSetting") == True):
        MainClass.UpdateGUI("OptionNumberIDText", True)
        MainClass.UpdateGUI("OptionNumber", True)
    else:
        MainClass.UpdateGUI("OptionNumberIDText", False)
        MainClass.UpdateGUI("OptionNumber", False)'''

MainWindow.close()
