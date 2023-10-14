using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Sentry.Unity.NativeUtils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SuperNewRoles.SuperNewRolesWeb
{
    public static class AccountLoginMenu
    {
        static PassiveButton LoginButton;
        static TextMeshPro LoginButtonText;
        static TextMeshPro SuperNewRolesWebText;
        static TextMeshPro UserIdText;

        static EditName LoginPopup;
        static TextMeshPro LoginPopupTitle;
        static TextMeshPro LoginPopupCurrentTitle;
        static PassiveButton LoginPopupSubmitButton;
        static PassiveButton LoginPopupBackButton;

        static string CurrentUserId;
        static bool IsUserIdInputNow;
        [HarmonyPatch(typeof(TextBoxTMP), nameof(TextBoxTMP.SetText))]
        class TextBoxTMPSetTextPatch
        {
            public static bool Prefix(TextBoxTMP __instance, ref string input, string inputCompo)
            {
                if (!IsUserIdInputNow && LoginPopup != null && LoginPopup.nameText != null && LoginPopup.nameText.nameSource.GetInstanceID() == __instance.GetInstanceID())
                {
                    string inputtep = "";
                    char c = ' ';
                    for (int i = 0; i < input.Length; i++)
                    {
                        char c2 = input[i];
                        if (c != ' ' || c2 != ' ')
                        {
                            if (c2 == '\b')
                            {
                                inputtep = inputtep[..(inputtep.Length - 1)];
                                continue;
                            }
                            inputtep += c2;
                            c = c2;
                        }
                    }
                    input = inputtep;
                    string tempresult = "";
                    for (int i = 0; i < input.Length; i++)
                    {
                        tempresult += "*";
                    }
                    __instance.text = input;
                    input = tempresult;
                    __instance.outputText.text = input;
                    if (__instance.Pipe)
                    {
                        __instance.Pipe.transform.localPosition = __instance.outputText.CursorPos();
                    }
                    return false;
                }
                return true;
            }
        }
        public static void ReloadObjects()
        {
            if (LoginButton == null)
            {
                var BaseButton = FastDestroyableSingleton<AccountManager>.Instance.accountTab.transform.FindChild("AccountWindow/Card/CardContents/Buttons/OfflineMode/SignIn").GetComponent<PassiveButton>();
                LoginButton = GameObject.Instantiate(BaseButton);
                LoginButtonText = LoginButton.GetComponentInChildren<TextMeshPro>();
                SuperNewRolesWebText = GameObject.Instantiate(LoginButtonText);
                SuperNewRolesWebText.transform.localPosition = new(0, 2.2f, 0);
                SuperNewRolesWebText.transform.localScale = Vector3.one * 3;
                SuperNewRolesWebText.GetComponent<TextTranslatorTMP>().enabled = false;
                SuperNewRolesWebText.enableWordWrapping = false;
                UserIdText = GameObject.Instantiate(LoginButtonText);
                UserIdText.transform.localPosition = new(0, 1.9f, 0);
                UserIdText.transform.localScale = Vector3.one * 0.5f;
                UserIdText.GetComponent<TextTranslatorTMP>().enabled = false;
                UserIdText.enableWordWrapping = false;
                LoginButton.transform.localScale = Vector3.one * 0.75f;
                LoginButton.transform.FindChild("Background").localScale = new(0.8f, 0.75f, 1);
                LoginButton.OnClick = new();
                LoginButton.OnClick.AddListener((Action)(() =>
                {
                    if (WebAccountManager.IsLogined)
                    {
                        WebAccountManager.LogOut();
                        ReloadObjects();
                    }
                    else
                    {
                        IsUserIdInputNow = true;
                        LoginPopupCurrentTitle.text = ModTranslation.GetString("SNRWebLogin");
                        CurrentUserId = "";
                        new LateTask(() => LoginPopup.nameText.nameSource.SetText(""), 0f);
                        LoginPopup.gameObject.SetActive(true);
                        LoginPopup.nameText.gameObject.SetActive(true);
                        LoginPopupBackButton.gameObject.SetActive(true);
                        LoginPopupSubmitButton.gameObject.SetActive(true);
                    }
                }));
                LoginPopup = GameObject.Instantiate(FastDestroyableSingleton<AccountManager>.Instance.accountTab.editNameScreen);
                LoginPopupTitle = LoginPopup.transform.FindChild("TitleText_TMP").GetComponent<TextMeshPro>();
                LoginPopupTitle.GetComponent<TextTranslatorTMP>().enabled = false;
                LoginPopup.nameText.nameSource.characterLimit = 0;
                LoginPopup.nameText.nameSource.transform.localPosition = new(0, 0.1f, 0);
                LoginPopupCurrentTitle = LoginPopup.transform.FindChild("ChangeNameTitle_TMP").GetComponent<TextMeshPro>();
                LoginPopupCurrentTitle.transform.localPosition = new(-1.1f, 0.625f, 0);
                LoginPopupCurrentTitle.GetComponent<TextTranslatorTMP>().enabled = false;
                LoginPopupCurrentTitle.text = ModTranslation.GetString("");
                LoginPopupCurrentTitle.enableWordWrapping = false;
                LoginPopupSubmitButton = LoginPopup.transform.FindChild("SubmitButton").GetComponent<PassiveButton>();
                LoginPopupSubmitButton.OnClick = new();
                LoginPopupSubmitButton.OnClick.AddListener((Action)OnSubmitClick);
                LoginPopupBackButton = LoginPopup.transform.FindChild("BackButton").GetComponent<PassiveButton>();
                LoginPopupBackButton.OnClick = new();
                LoginPopupBackButton.OnClick.AddListener((Action)OnBackClick);
                LoginPopup.nameText.nameSource.SetText("");
                LoginPopup.transform.FindChild("RandomizeName").gameObject.SetActive(false);
                LoginPopup.nameText.nameSource.allowAllCharacters = true;
                LoginPopup.nameText.nameSource.AllowEmail = true;
                LoginPopup.nameText.nameSource.AllowSymbols = true;
            }
            IsUserIdInputNow = true;
            LoginPopup.gameObject.SetActive(false);
            LoginPopupTitle.text = ModTranslation.GetString("SNRWebLoginPopupTitle");
            LoginPopupCurrentTitle.transform.localPosition = new(-1.1f, 0.625f, 0);
            LoginPopupCurrentTitle.transform.localScale = Vector3.one;

            if (WebAccountManager.IsLogined)
            {
                SuperNewRolesWebText.text = string.Format(ModTranslation.GetString("SNRWebMMOnlineWelcomeText"), WebAccountManager.MyPlayerName);
                UserIdText.text = "@" + WebAccountManager.MyUserId;
                LoginButtonText.GetComponent<TextTranslatorTMP>().enabled = false;
                LoginButtonText.text = ModTranslation.GetString("SNRWebLogoutText");
                LoginButton.transform.localPosition = new(0, 1.6f, 0);
            }
            else
            {
                SuperNewRolesWebText.text = ModTranslation.GetString("SNRWebMMOnlineNoneLoginText");
                UserIdText.text = "";
                TextTranslatorTMP textTranslatorTMP = LoginButtonText.GetComponent<TextTranslatorTMP>();
                textTranslatorTMP.enabled = true;
                textTranslatorTMP.ResetText();
                LoginButton.transform.localPosition = new(0, 1.75f, 0);
            }
            LoginPopup.transform.localPosition = new(0, 0, -10);
            /*
            var textBox = NameText.GetComponent<TextBoxTMP>();
            textBox.outputText.alignment = TextAlignmentOptions.CenterGeoAligned;
            textBox.outputText.transform.position = NameText.transform.position;
            textBox.outputText.fontSize = 4f;
            textBox.OnChange.AddListener((Action)(() =>
            {
                DataManager.Player.Customization.Name = textBox.text;
            }));
            textBox.OnEnter = textBox.OnFocusLost = textBox.OnChange;
            textBox.Pipe.GetComponent<TextMeshPro>().fontSize = 4f;*/
        }
        public static void OnBackClick()
        {
            //今がユーザーId
            if (IsUserIdInputNow)
            {
                LoginPopup.Close();
            }
            //今がパスワード
            else
            {
                IsUserIdInputNow = true;
                LoginPopupCurrentTitle.text = ModTranslation.GetString("SNRWebLogin");
                LoginPopup.nameText.nameSource.SetText(CurrentUserId);
            }
        }
        public static void OnSubmitClick()
        {
            //次がパスワード
            if (IsUserIdInputNow)
            {
                LoginPopupCurrentTitle.text = ModTranslation.GetString("SNRWebPassword");
                CurrentUserId = LoginPopup.nameText.nameSource.text;
                LoginPopup.nameText.nameSource.SetText("");
                IsUserIdInputNow = false;
            }
            //全部入力終了
            else
            {
                LoginPopup.nameText.gameObject.SetActive(false);
                LoginPopupBackButton.gameObject.SetActive(false);
                LoginPopupSubmitButton.gameObject.SetActive(false);
                LoginPopupCurrentTitle.text = ModTranslation.GetString("SNRWebLoginNow");
                LoginPopupCurrentTitle.transform.localPosition = new(0.5f, 0, 0);
                LoginPopupCurrentTitle.transform.localScale = Vector3.one * 3;
                WebApi.Login(CurrentUserId, LoginPopup.nameText.nameSource.text, (code, handler) =>
                {
                    if (code != 200)
                    {
                        LoginError();
                    }
                    else
                    {
                        WebAccountManager.SetToken(handler.text, (IsSuc) =>
                        {
                            Logger.Info($"{IsSuc}", "IsSuc");
                            if (IsSuc)
                            {
                                LoginPopup.Close();
                                ReloadObjects();
                            }
                            else
                            {
                                LoginError();
                            }
                        });
                    }
                });
            }
        }
        public static void LoginError()
        {
            LoginPopup.nameText.gameObject.SetActive(true);
            LoginPopupBackButton.gameObject.SetActive(true);
            LoginPopupSubmitButton.gameObject.SetActive(true);
            OnBackClick();
            LoginPopupCurrentTitle.text = "<size=200%>" + ModTranslation.GetString("SNRWebLoginNotSuc") + "\n" + LoginPopupCurrentTitle.text + "\n\n</size>";
            LoginPopupCurrentTitle.transform.localPosition = new(-1.1f, 0.625f, 0);
            LoginPopupCurrentTitle.transform.localScale = Vector3.one;
        }
        public static void Initialize()
        {
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
            {
                if (!scene.name.Equals("MMOnline")) return;
                if (!TryMoveObjects()) return;
                ReloadObjects();
            }));
        }

        private static bool TryMoveObjects()
        {
            var toMove = new List<string>
            {
                "HostGameButton",
                "FindGameButton",
                "JoinGameButton"
            };
            var yStart = Vector3.up * 0.83f;
            var yOffset = Vector3.down * 1.575f;
            var gameObjects = toMove.Select(x => GameObject.Find("NormalMenu/Buttons/" + x));
            if (gameObjects.Any(x => x == null)) return false;
            int index = 0;
            foreach (GameObject obj in gameObjects)
            {
                obj.transform.position = yStart + (yOffset * index);
                index++;
            }
            return true;
        }
    }
}