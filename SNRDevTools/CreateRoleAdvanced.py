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
            Template = "new(RGBCOLOR, byte.MaxValue)".replace(
                "RGBCOLOR", str(MainClass.HashToRGB()))
            print("ハッシュを取得しました:", Template)
            return Template
        elif (MainClass.GetBool("ImpoColor")):
            print("インポ色を取得しました")
            return "RoleClass.ImpostorRed"
        elif (MainClass.GetBool("CrewColor")):
            print("クルー色を取得しました")
            return "Palette.CrewmateBlue"
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
        # MainClass.Access = True
        MainClass.Num = int(MainClass.GetInput("OptionNumber"))
        if (MainClass.Num >= 100):
            Return = str(MainClass.Num)
        else:
            Return = "0"+str(MainClass.Num)
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

    # ROLENAME.csに記載するオプションを, 一時的置きする変数
    Using: str = ""
    CustomOption: str = ""
    CustomOptionCreate: str = ""
    RoleData: str = ""
    ClearAndReload: str = ""
    CustomButtons: str = ""

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
        if (MainClass.GetBool("A_PersonalWin")):
            MainClass.CreateOKWindow("独自勝利辞書追加は現在対応していません")
        # 一部値がかぶっていないか(例:インポ+キル可能)
        if (MainClass.GetBool("A_CanVent")):
            if (MainClass.GetBool("Impo")):
                MainClass.CreateOKWindow("警告", "インポスターはデフォルトで\nベントボタンが作成されます")
                return
        if (MainClass.GetBool("A_CanKill")):
            if (MainClass.GetBool("Impo")):
                MainClass.CreateOKWindow("警告", "インポスターはデフォルトで\nキルボタンが作成されます")
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
        # 翻訳
        MainClass.WriteCodes("Resources\Translate.csv", "\n#NewRoleTranslation",
                             """ROLENAMEName,ROLENAME,\nROLENAMETitle1,,\nROLENAMEDescription,,\n\n#NewRoleTranslation""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # CustomRPC/CustomRPC.cs
        MainClass.WriteCodes("Modules/CustomRPC.cs", "//RoleId",
                             MainClass.GetInput("RoleName")+",\n    //RoleId")

        # Roles/AllRoleSetClass.cs
        MainClass.WriteCodes("Roles/AllRoleSetClass.cs", "// プレイヤーカウント",
                             """RoleId.ROLENAME => ROLENAME.CustomOptionData.PlayerCount.GetFloat(),\n            // プレイヤーカウント""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # Roles/Role/RoleHelper.cs
        if (not MainClass.GetBool("TeamGhost")):
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// ロールチェック",
                                 """else if (ROLENAME.RoleData.Player.IsCheckListPlayerControl(player)) return RoleId.ROLENAME;
            // ロールチェック""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// ロールアド",
                             """case RoleId.ROLENAME:
                ROLENAME.RoleData.Player.Add(player);
                break;
            // ロールアド""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "    // ロールリモベ",
                             """case RoleId.ROLENAME:
                ROLENAME.RoleData.Player.RemoveAll(ClearRemove);
                break;
                // ロールリモベ""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        if (MainClass.GetBool("Neut")):
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", ";\n    // 第三か",
                                 """ or
        RoleId.ROLENAME;
    // 第三か""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        if (MainClass.GetBool("A_ClearTask")):
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// タスククリアか 個別表記",
                                 """case RoleId.ROLENAME:
            // タスククリアか 個別表記""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # Roles/Role/RoleClass.cs
        MainClass.WriteCodes("Roles/Role/RoleClass.cs", "// ロールクリア", MainClass.GetInput(
            "RoleName")+".RoleData.ClearAndReload();\n        // ロールクリア")

        # Intro/IntroData.cs
        if (MainClass.GetBool("Impo")):
            MainClass.WriteCodes("Modules/IntroData.cs", "// イントロオブジェ", """public static IntroData ROLENAMEIntro = new("ROLENAME", ROLENAME.RoleData.color, 1, RoleId.ROLENAME, TeamRoleType.ImpostorROLETYPE);
    // イントロオブジェ""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("ROLETYPE", MainClass.GetIntroSoundType()))
        elif (MainClass.GetBool("Crew")):
            MainClass.WriteCodes("Modules/IntroData.cs", "// イントロオブジェ", """public static IntroData ROLENAMEIntro = new("ROLENAME", ROLENAME.RoleData.color, 1, RoleId.ROLENAME, TeamRoleType.CrewmateROLETYPE);
    // イントロオブジェ""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("ROLETYPE", MainClass.GetIntroSoundType()))
        elif (MainClass.GetBool("Neut")):
            MainClass.WriteCodes("Modules/IntroData.cs", "// イントロオブジェ", """public static IntroData ROLENAMEIntro = new("ROLENAME", ROLENAME.RoleData.color, 1, RoleId.ROLENAME, TeamRoleType.NeutralROLETYPE);
    // イントロオブジェ""".replace("ROLENAME", MainClass.GetInput("RoleName")).replace("ROLETYPE", MainClass.GetIntroSoundType()))
        elif (MainClass.GetBool("TeamOne")):
            print()
        elif (MainClass.GetBool("TeamTwo")):
            print()
        elif (MainClass.GetBool("TeamGhost")):
            MainClass.WriteCodes("Modules/IntroData.cs", "// イントロオブジェ", """public static IntroData ROLENAMEIntro = new IntroData("ROLENAME", ROLENAME.color, 1, RoleId.ROLENAME, TeamRoleType.Crewmate, true);
    // イントロオブジェ""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # CustomOption/CustomOptionHolder.cs
        if (MainClass.GetBool("Impo")):
            MainClass.WriteCodes("Modules/CustomOptionHolder.cs", "// SetupImpostorCustomOptions",
                             """ROLENAME.CustomOptionData.SetupCustomOptions();\n\n        // SetupImpostorCustomOptions""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        elif (MainClass.GetBool("Crew")):
            MainClass.WriteCodes("Modules/CustomOptionHolder.cs", "// SetupCrewmateCustomOptions",
                             """ROLENAME.CustomOptionData.SetupCustomOptions();\n\n        // SetupCrewmateCustomOptions""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        elif (MainClass.GetBool("Neut")):
            MainClass.WriteCodes("Modules/CustomOptionHolder.cs", "// SetupNeutralCustomOptions",
                             """ROLENAME.CustomOptionData.SetupCustomOptions();\n\n        // SetupNeutralCustomOptions""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # シェリフキル
        if (MainClass.GetBool("A_CanSheriffKill_Mad")):
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", ";\n        // IsMads",
                                 """ or\n        RoleId.ROLENAME;\n        // IsMads""".replace("ROLENAME", MainClass.GetInput("RoleName")))
        elif (MainClass.GetBool("A_CanSheriffKill_Friends")):
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", ";\n        // IsFriends",
                                 """ or\n        RoleId.ROLENAME;\n        // IsFriends""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # ベントボタン
        if (MainClass.GetBool("A_CanVent")):
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// ベントが使える",
                                 """RoleId.ROLENAME => ROLENAME.CustomOptionData.IsUseVent.GetBool(),\n            // ベントが使える""".replace("ROLENAME", MainClass.GetInput("RoleName")))

            # Roles/Role/Team/ROLENAME.cs
            MainClass.CustomOption += """\n        public static CustomOption IsUseVent;"""
            MainClass.CustomOptionCreate += f"""\n            IsUseVent = CustomOption.Create(optionId, {MainClass.GetCBool("IsSHRON")}, CustomOptionType.TEAMTYPE, "MadmateUseVentSetting", false, Option); optionId++;"""

        # インポの視界設定
        if (MainClass.GetBool("A_ImpoVisible")):
            # Roles/Role/RoleHelper.cs
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// インポの視界",
                                 """RoleId.ROLENAME => ROLENAME.CustomOptionData.IsImpostorLight.GetBool(),\n                // インポの視界""".replace("ROLENAME", MainClass.GetInput("RoleName")))

            # Roles/Role/Team/ROLENAME.cs
            MainClass.CustomOption += """\n        public static CustomOption IsImpostorLight;"""
            MainClass.CustomOptionCreate += f"""\n            IsImpostorLight = CustomOption.Create(optionId, {MainClass.GetCBool("IsSHRON")}, CustomOptionType.TEAMTYPE, "MadmateImpostorLightSetting", false, Option); optionId++;"""

        # キルボタン

        # カスタムボタン と キルボタン
        if (MainClass.GetBool("A_CanKill") or MainClass.GetBool("A_CustomButton")):
            # 変数
            CustomButton: str = ""
            SetupCustomButtons: str = ""
            ResetButtonCool: str = ""
            ButtonSprite: str = ""

            # キルボタン
            if (MainClass.GetBool("A_CanKill")):
                # 翻訳
                MainClass.WriteCodes("Resources\Translate.csv", "\n#NewRoleTranslation", f"""{MainClass.GetInput("RoleName")}KillCooldownSetting,,\n\n#NewRoleTranslation""")

                # コード
                MainClass.CustomOption += """\n        public static CustomOption KillButtonCooldown;"""
                MainClass.CustomOptionCreate += f"""\n            KillButtonCooldown = CustomOption.Create(optionId, {MainClass.GetCBool("IsSHRON")}, CustomOptionType.TEAMTYPE, "ROLENAMEKillCooldownSetting", 30f, 2.5f, 60f, 2.5f,  Option); optionId++;"""
                MainClass.RoleData += """\n        public static float KillButtonCooldown;"""
                MainClass.ClearAndReload += """\n            KillButtonCooldown  = CustomOptionData.KillButtonCooldown.GetFloat();"""

                CustomButton += """\n        private static CustomButton KillButton;"""

                SetupCustomButtons += """\n            KillButton = new(
                () =>
                {
                    ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, HudManagerStartPatch.SetTarget());
                    ResetKillButtonCool();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.ROLENAME; },
                () => { return HudManagerStartPatch.SetTarget() && PlayerControl.LocalPlayer.CanMove; },
                () => { ResetKillButtonCool(); },
                hm.KillButton.graphic.sprite,
                new Vector3(0, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.Q,
                8,
                () => { return false; }
            )
            {
                buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
                showButtonText = true
            };"""

                ResetButtonCool += """\n
        private static void ResetKillButtonCool()
        {
            var cooldown = RoleClass.IsfirstResetCool ? 10f : RoleData.KillButtonCooldown;
            KillButton.MaxTimer = cooldown;
            KillButton.Timer = cooldown;
            RoleClass.IsfirstResetCool = false;
        }"""

            # カスタムボタン
            if (MainClass.GetBool("A_CustomButton")):
                # 翻訳
                MainClass.WriteCodes("Resources\Translate.csv", "\n#NewRoleTranslation", f"""{MainClass.GetInput("RoleName")}ButtonName,,\n\n#NewRoleTranslation""")
                MainClass.WriteCodes("Resources\Translate.csv", "\n#NewRoleTranslation", f"""{MainClass.GetInput("RoleName")}ButtonCooldownSetting,,\n\n#NewRoleTranslation""")

                # コード
                MainClass.CustomOption += """\n        public static CustomOption ROLENAMEButtonCooldown;"""
                MainClass.CustomOptionCreate += f"""\n            ROLENAMEButtonCooldown = CustomOption.Create(optionId, {MainClass.GetCBool("IsSHRON")}, CustomOptionType.TEAMTYPE, "ROLENAMEButtonCooldownSetting", 30f, 2.5f, 60f, 2.5f,  Option); optionId++;"""
                MainClass.RoleData += """\n        public static float ROLENAMEButtonCooldown;"""
                MainClass.ClearAndReload += """\n            ROLENAMEButtonCooldown  = CustomOptionData.ROLENAMEButtonCooldown.GetFloat();"""

                CustomButton += """\n        private static CustomButton ROLENAMEButton;"""

                ButtonSprite += """\n        private static Sprite GetButtonSprite() => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ROLENAMEButton.png", 115f);"""

                SetupCustomButtons += """\n            ROLENAMEButton = new(
                () =>
                {
                    // ここに能力のコードを記載する

                    ResetROLENAMEButtonCool();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.ROLENAME; },
                () => { return PlayerControl.LocalPlayer.CanMove; },
                () => { ResetROLENAMEButtonCool(); },
                GetButtonSprite(),
                new Vector3(-2f, 1, 0),
                hm,
                hm.AbilityButton,
                KeyCode.F,
                49,
                () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("ROLENAMEButtonName"),
                showButtonText = true
            };"""

                ResetButtonCool += """\n
        private static void ResetROLENAMEButtonCool()
        {
            ROLENAMEButton.MaxTimer = RoleData.ROLENAMEButtonCooldown;
            ROLENAMEButton.Timer = RoleData.ROLENAMEButtonCooldown;
        }"""

            # 以下共通部
            # Buttons/Buttons.cs
            MainClass.WriteCodes("Buttons/Buttons.cs", "// SetupCustomButtons",
                                 """ROLENAME.Button.SetupCustomButtons(__instance);\n\n        // SetupCustomButtons""".replace("ROLENAME", MainClass.GetInput("RoleName")))

            # Roles/Role/Team/ROLENAME.cs
            MainClass.Using += "\nusing SuperNewRoles.Buttons;"
            MainClass.CustomButtons += """\n    internal static class Button\n    {""" + CustomButton + ButtonSprite + """\n\n        internal static void SetupCustomButtons(HudManager hm)\n        {""" + SetupCustomButtons + """\n        }""" + ResetButtonCool + """\n    }"""

        # Roles/Role/RoleHelper.cs
        if (MainClass.GetBool("TeamGhost")):
            MainClass.WriteCodes("Roles/Role/RoleHelper.cs", "// ここが幽霊役職",
                                 """if (ROLENAME.RoleData.Player.IsCheckListPlayerControl(player))
                    {
                        return SuperNewRoles.RoleId.ROLENAME;
                    }\n                // ここが幽霊役職""".replace("ROLENAME", MainClass.GetInput("RoleName")))

        # Roles/Role/Team/ROLENAME.cs
        if (MainClass.GetBool("Impo")):
            namedata = teamtype = "Impostor"
            idnam = "2" + MainClass.PlusIDNum() + "00"
            playerstype = "CustomOptionHolder.ImpostorPlayers"
        elif (MainClass.GetBool("Neut")):
            namedata = teamtype = "Neutral"
            idnam = "3" + MainClass.PlusIDNum() + "00"
            playerstype = "CustomOptionHolder.CrewPlayers"
        elif (MainClass.GetBool("Crew")):
            namedata = teamtype = "Crewmate"
            idnam = "4" + MainClass.PlusIDNum() + "00"
            playerstype = "CustomOptionHolder.CrewPlayers"
        with open(BasePath+"Roles/"+namedata+"/ROLENAME.cs".replace("ROLENAME", MainClass.GetInput("RoleName")), mode="x", encoding='UTF-8') as x:
            x.write(
                ("""using System.Collections.Generic;
using UnityEngine;
// Using

namespace SuperNewRoles.Roles."""+namedata+""";

public static class ROLENAME
{
    public static class CustomOptionData
    {
        private static int optionId = """+idnam+""";
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        // Write CustomOption

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, SHRON, RoleId.ROLENAME); optionId++;
            PlayerCount = CustomOption.Create(optionId, SHRON, CustomOptionType."""+namedata+""", "SettingPlayerCountName", PLAYERSTYPE[0], PLAYERSTYPE[1], PLAYERSTYPE[2], PLAYERSTYPE[3], Option); optionId++;
            // Write CustomOption Create
        }
    }

    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static Color32 color = COLORS;
        // Write RoleData

        public static void ClearAndReload()
        {
            Player = new();
            // Write ClearAndReload
        }
    }

    // SetupCustomButtons

    // ここにコードを書きこんでください
}""")
                .replace("\n// Using", MainClass.Using)
                .replace("\n        // Write CustomOption", MainClass.CustomOption)
                .replace("\n            // Write CustomOption Create", MainClass.CustomOptionCreate)
                .replace("\n        // Write RoleData", MainClass.RoleData)
                .replace("\n            // Write ClearAndReload", MainClass.ClearAndReload)
                .replace("\n    // SetupCustomButtons", MainClass.CustomButtons)
                .replace("ROLENAME", MainClass.GetInput("RoleName"))
                .replace("SHRON", MainClass.GetCBool("IsSHRON"))
                .replace("PLAYERSTYPE", playerstype)
                .replace("TEAMTYPE", teamtype)
                .replace("COLORS", MainClass.GetRoleColor()))

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
    [psg.Text(), psg.Text("設定ID(3桁の固有Id)", key="OptionNumberIDText"), psg.Input("", key="OptionNumber", size=(10, 3))], ])
AdvanceTab = psg.Tab("詳細設定", [
    [psg.Check("タスクを削除する", key="A_ClearTask")],
    [psg.Check("シェリフキル(マッド)", key="A_CanSheriffKill_Mad")],
    [psg.Check("シェリフキル(フレンズ)", key="A_CanSheriffKill_Friends")],
    [psg.Text("ボタン")],
    [psg.Text(), psg.Check("ベント", key="A_CanVent")],
    #[psg.Text(),psg.Check("ベント設定の追加", key="A_CanVentOption")],
    [psg.Text(), psg.Check("キルができる", key="A_CanKill")],
    [psg.Text(), psg.Check("カスタムボタン", key="A_CustomButton")],
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
