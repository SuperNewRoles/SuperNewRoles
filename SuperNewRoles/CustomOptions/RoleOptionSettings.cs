using System;
using System.Linq;
using SuperNewRoles.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomOptions
{
    public class RoleOptionSettings
    {
        private const int GenCount = 50;
        private const float DefaultRate = 4.5f;
        private const float DefaultLastY = 4f;
        private const float ElementXPosition = -0.22f;
        private const float ElementZPosition = -5f;
        private const float ElementScale = 2f;
        private const float ScrollerXPosition = 18f;

        private static readonly Color32 HoverColor = new(45, 235, 198, 255);

        private static GameObject CreateOptionElement(Transform parent, string optionName, ref float lastY, string prefabName)
        {
            var optionPrefab = AssetManager.GetAsset<GameObject>(prefabName);
            var optionInstance = UnityEngine.Object.Instantiate(optionPrefab, parent);
            optionInstance.transform.localPosition = new Vector3(ElementXPosition, lastY, ElementZPosition);
            lastY -= DefaultRate;
            optionInstance.transform.localScale = Vector3.one * ElementScale;
            optionInstance.transform.Find("Text").GetComponent<TextMeshPro>().text = optionName;
            return optionInstance;
        }

        private static void ConfigurePassiveButton(PassiveButton button, Action onClick, SpriteRenderer spriteRenderer = null)
        {
            button.OnClick = new();
            button.OnClick.AddListener(onClick);
            ConfigureButtonHoverEffects(button, spriteRenderer);
        }

        private static void ConfigureButtonHoverEffects(PassiveButton button, SpriteRenderer spriteRenderer)
        {
            button.OnMouseOver = new();
            button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = HoverColor;
            }));

            button.OnMouseOut = new();
            button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
            }));
        }

        private static GameObject CreateCheckBox(Transform parent, CustomOption option, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString(option.Name), ref lastY, "Option_Check");
            var passiveButton = optionInstance.AddComponent<PassiveButton>();
            var checkMark = optionInstance.transform.Find("CheckMark");

            SetupCheckBoxButton(passiveButton, checkMark, option);
            passiveButton.Colliders = new[] { optionInstance.GetComponent<BoxCollider2D>() };

            // チェックボックスの場合はSelectedTextの代わりにCheckMarkを使用するため、
            // 特別な処理としてCheckMarkをTextとして扱う
            var checkMarkTMP = checkMark.gameObject.AddComponent<TextMeshPro>();
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentOptionDisplays.Add((checkMarkTMP, option));

            return optionInstance;
        }

        private static void SetupCheckBoxButton(PassiveButton passiveButton, Transform checkMark, CustomOption option)
        {
            checkMark.gameObject.SetActive((bool)option.Value);
            var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

            ConfigurePassiveButton(passiveButton, () =>
            {
                bool newValue = !checkMark.gameObject.activeSelf;
                checkMark.gameObject.SetActive(newValue);
                option.UpdateSelection(newValue ? (byte)1 : (byte)0);

                // ホストの場合、他のプレイヤーに同期
                if (AmongUsClient.Instance.AmHost)
                {
                    CustomOptionManager.RpcSyncOption(option.Id, newValue ? (byte)1 : (byte)0);
                }
            }, spriteRenderer);
        }

        private static GameObject CreateSelect(Transform parent, CustomOption option, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString(option.Name), ref lastY, "Option_Select");
            var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
            selectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);

            SetupSelectButtons(optionInstance, selectedText, option);

            // 選択肢の表示とオプションをリストに追加
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentOptionDisplays.Add((selectedText, option));

            return optionInstance;
        }
        private static GameObject CreateNumberOfCrewsSelectAndPerSelect(Transform parent, RoleOptionManager.RoleOption roleOption, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString("NumberOfCrews"), ref lastY, "Option_Select");
            var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
            selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);

            var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
            var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
            var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
            bool isExist = RoleOptionMenu.RoleOptionMenuObjectData.RoleDetailButtonDictionary.TryGetValue(roleOption.RoleId, out var roleDetailButton);

            ConfigurePassiveButton(minusPassiveButton, () =>
            {
                byte playerCount = (byte)PlayerControl.AllPlayerControls.Count;
                if (15 >= playerCount)
                    playerCount = 15;
                roleOption.NumberOfCrews--;
                if (roleOption.NumberOfCrews < 0)
                    roleOption.NumberOfCrews = playerCount;
                if (roleOption.NumberOfCrews > playerCount)
                    roleOption.NumberOfCrews = playerCount;
                selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
                if (isExist)
                    RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
            }, minusSpriteRenderer);

            var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
            var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
            var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();

            // 確率設定のオプションを生成
            var perOption = CreateAssignPerSelect(parent, roleOption, ref lastY);
            var percentageText = perOption.transform.Find("SelectedText").GetComponent<TextMeshPro>();

            ConfigurePassiveButton(plusPassiveButton, () =>
            {
                byte playerCount = (byte)PlayerControl.AllPlayerControls.Count;
                if (15 >= playerCount)
                    playerCount = 15;
                byte oldValue = roleOption.NumberOfCrews;
                roleOption.NumberOfCrews++;
                if (roleOption.NumberOfCrews > playerCount)
                    roleOption.NumberOfCrews = 0;
                selectedText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
                if (isExist)
                    RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);

                if (oldValue == 0 && roleOption.NumberOfCrews > 0 && roleOption.Percentage == 0)
                {
                    roleOption.Percentage = 100;
                    percentageText.text = "100%";
                }
            }, plusSpriteRenderer);

            RoleOptionMenu.RoleOptionMenuObjectData.CurrentRoleNumbersOfCrewsText = selectedText;
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentRolePercentageText = percentageText;
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentRoleId = roleOption.RoleId;

            return optionInstance;
        }

        private static GameObject CreateAssignPerSelect(Transform parent, RoleOptionManager.RoleOption roleOption, ref float lastY)
        {
            GameObject optionInstance = CreateOptionElement(parent, ModTranslation.GetString("AssignPer"), ref lastY, "Option_Select");
            var selectedText = optionInstance.transform.Find("SelectedText").GetComponent<TextMeshPro>();
            selectedText.text = roleOption.Percentage + "%";

            var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
            var minusPassiveButton = minusButton.AddComponent<PassiveButton>();
            var minusSpriteRenderer = minusPassiveButton.GetComponent<SpriteRenderer>();
            bool isExist = RoleOptionMenu.RoleOptionMenuObjectData.RoleDetailButtonDictionary.TryGetValue(roleOption.RoleId, out var roleDetailButton);
            ConfigurePassiveButton(minusPassiveButton, () =>
            {
                roleOption.Percentage -= 10;
                if (roleOption.Percentage < 0)
                    roleOption.Percentage = 100;
                if (roleOption.Percentage > 100)
                    roleOption.Percentage = 100;
                selectedText.text = roleOption.Percentage + "%";
                if (isExist)
                    RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
            }, minusSpriteRenderer);

            var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
            var plusPassiveButton = plusButton.AddComponent<PassiveButton>();
            var plusSpriteRenderer = plusPassiveButton.GetComponent<SpriteRenderer>();

            ConfigurePassiveButton(plusPassiveButton, () =>
            {
                roleOption.Percentage += 10;
                if (roleOption.Percentage > 100)
                    roleOption.Percentage = 0;
                selectedText.text = roleOption.Percentage + "%";
                if (isExist)
                    RoleOptionMenu.UpdateRoleDetailButtonColor(roleDetailButton.GetComponent<SpriteRenderer>(), roleOption);
            }, plusSpriteRenderer);
            return optionInstance;
        }

        private static void SetupSelectButtons(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
        {
            SetupMinusButton(optionInstance, selectedText, option);
            SetupPlusButton(optionInstance, selectedText, option);
        }

        private static void SetupMinusButton(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
        {
            var minusButton = optionInstance.transform.Find("Button_Minus").gameObject;
            var passiveButton = minusButton.AddComponent<PassiveButton>();
            var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

            ConfigurePassiveButton(passiveButton, () =>
            {
                byte newSelection = option.Selection > 0 ? (byte)(option.Selection - 1) : (byte)(option.Selections.Length - 1);
                UpdateOptionSelection(option, newSelection, selectedText);
            }, spriteRenderer);
        }

        private static void SetupPlusButton(GameObject optionInstance, TextMeshPro selectedText, CustomOption option)
        {
            var plusButton = optionInstance.transform.Find("Button_Plus").gameObject;
            var passiveButton = plusButton.AddComponent<PassiveButton>();
            var spriteRenderer = passiveButton.GetComponent<SpriteRenderer>();

            ConfigurePassiveButton(passiveButton, () =>
            {
                byte newSelection = option.Selection < option.Selections.Length - 1 ? (byte)(option.Selection + 1) : (byte)0;
                UpdateOptionSelection(option, newSelection, selectedText);
            }, spriteRenderer);
        }

        private static void UpdateOptionSelection(CustomOption option, byte newSelection, TextMeshPro selectedText)
        {
            option.UpdateSelection(newSelection);
            selectedText.text = UIHelper.FormatOptionValue(option.Selections[option.Selection], option);

            // ホストの場合、他のプレイヤーに同期
            if (AmongUsClient.Instance.AmHost)
            {
                CustomOptionManager.RpcSyncOption(option.Id, newSelection);
            }
        }

        public static void SetupScroll(Transform parent)
        {
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller = parent.transform.Find("SettingsScroller").GetComponent<Scroller>();
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner = parent.transform.Find("SettingsScroller/InnerContent");
        }

        public static void ClickedRole(RoleOptionManager.RoleOption roleOption)
        {
            float lastY = DefaultLastY;
            // parentを破棄
            var parent = RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner.Find("Parent");
            if (parent != null)
                GameObject.Destroy(parent.gameObject);

            // 表示リストをクリア
            RoleOptionMenu.RoleOptionMenuObjectData.CurrentOptionDisplays.Clear();

            int index = CreateRoleOptions(roleOption, ref lastY);
            UpdateScrollerBounds(index);

            // スクロール位置をリセット
            ResetScrollPosition();
        }

        // スクロール位置をリセットするメソッド
        private static void ResetScrollPosition()
        {
            var settingsInner = RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner;
            var settingsScroller = RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller;

            if (settingsInner != null && settingsScroller != null)
            {
                // スクロール位置を一番上にリセット
                settingsInner.localPosition = new Vector3(settingsInner.localPosition.x, 0f, settingsInner.localPosition.z);
                settingsScroller.UpdateScrollBars();
            }
        }

        private static int CreateRoleOptions(RoleOptionManager.RoleOption roleOption, ref float lastY)
        {
            var parent = new GameObject("Parent");
            parent.transform.SetParent(RoleOptionMenu.RoleOptionMenuObjectData.SettingsInner);
            parent.transform.localScale = Vector3.one;
            parent.transform.localPosition = Vector3.zero;
            parent.layer = 5;
            CreateNumberOfCrewsSelectAndPerSelect(parent.transform, roleOption, ref lastY);
            int index = 2;
            Logger.Info($"roleOption: {roleOption.RoleId}");
            foreach (var option in roleOption.Options)
            {
                Logger.Info($"option: {option.Name}");
                if (option.IsBooleanOption)
                    CreateCheckBox(parent.transform, option, ref lastY);
                else
                    CreateSelect(parent.transform, option, ref lastY);
                index++;
            }
            return index;
        }

        private static void UpdateScrollerBounds(int index)
        {
            // スクロールバーの範囲を設定
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.ContentYBounds.max =
                index < 5 ? 0.1f : (index - 4) * DefaultRate; // 最小値を0.1fに設定して常にスクロールバーが表示されるようにする

            // スクロールバーを表示するために必要な設定
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.ScrollbarYBounds = new FloatRange(0, 1);

            // スクロールバーを更新
            RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.UpdateScrollBars();

            Logger.Info($"Updated Max {index} {RoleOptionMenu.RoleOptionMenuObjectData.SettingsScroller.ContentYBounds.max}");
        }

        /// <summary>
        /// 設定画面の表示を更新する
        /// </summary>
        /// <param name="roleOption">更新対象のロールオプション</param>
        public static void UpdateSettingsDisplay(RoleOptionManager.RoleOption roleOption)
        {
            var data = RoleOptionMenu.RoleOptionMenuObjectData;
            if (data == null) return;

            // 人数と確率の表示を更新
            if (data.CurrentRoleId == roleOption.RoleId)
            {
                if (data.CurrentRoleNumbersOfCrewsText != null)
                {
                    data.CurrentRoleNumbersOfCrewsText.text = ModTranslation.GetString("NumberOfCrewsSelected", roleOption.NumberOfCrews);
                }
                if (data.CurrentRolePercentageText != null)
                {
                    data.CurrentRolePercentageText.text = roleOption.Percentage + "%";
                }
            }

            // その他のオプションの表示を更新
            if (data.CurrentOptionDisplays == null) return;
            foreach (var (text, option) in data.CurrentOptionDisplays)
            {
                if (text == null) continue;

                if (option.IsBooleanOption)
                {
                    // チェックボックスの場合、TextMeshProの親オブジェクト（CheckMark）の表示を切り替え
                    text.transform.gameObject.SetActive((bool)option.Value);
                }
                else
                {
                    text.text = UIHelper.FormatOptionValue(option.Value, option);
                }
            }
        }

        public static void HideRoleSettings()
        {
            var parent = RoleOptionMenu.RoleOptionMenuObjectData?.SettingsInner?.Find("Parent");
            if (parent != null)
                GameObject.Destroy(parent.gameObject);

            // 表示リストをクリア
            if (RoleOptionMenu.RoleOptionMenuObjectData?.CurrentOptionDisplays != null)
                RoleOptionMenu.RoleOptionMenuObjectData.CurrentOptionDisplays.Clear();
        }
    }
}