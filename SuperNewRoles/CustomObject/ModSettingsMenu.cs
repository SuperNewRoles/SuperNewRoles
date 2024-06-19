using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.CustomObject;

public class ModSettingsMenu : MonoBehaviour
{
    public GameObject RoleChancesSettings;
    public GameObject GenericSettings;
    public GameObject ImpostorSettings;
    public GameObject NeutralSettings;
    public GameObject CrewmateSettings;
    public GameObject ModifierSettings;
    public GameObject MatchTagSettings;

    public PassiveButton RoleChancesButton;
    public PassiveButton GenericButton;
    public PassiveButton ImpostorButton;
    public PassiveButton NeutralButton;
    public PassiveButton CrewmateButton;
    public PassiveButton ModifierButton;
    public PassiveButton MatchTagButton;

    public Scroller ScrollBar;
    public UiElement BackButton;
    public UiElement DefaultButtonSelected;
    public List<UiElement> ControllerSelectable = new();
    public UiElement[] RoleChancesTabSelectables = new UiElement[0];
    public UiElement[] GenericTabSelectables;
    public UiElement[] ImpostorTabSelectables;
    public UiElement[] NeutralTabSelectables;
    public UiElement[] CrewmateTabSelectables;
    public UiElement[] ModifierTabSelectables;
    public UiElement[] MatchTagSelectables;

    public List<RoleOptionSetting> RoleChances;

    public RoleOptionSetting RoleOptionSettingOrigin;
    public ToggleOption CheckboxOrigin;
    public NumberOption NumberOptionOrigin;
    public StringOption StringOptionOrigin;

    public void Start()
    {
        GameSettingMenu menu = GameSettingMenu.Instance;
        RolesSettingsMenu roles = menu.RoleSettingsTab;
        RoleOptionSettingOrigin = roles.roleOptionSettingOrigin;
        CheckboxOrigin = roles.checkboxOrigin;
        NumberOptionOrigin = roles.numberOptionOrigin;
        StringOptionOrigin = roles.stringOptionOrigin;

        UnityEvent mouse_out = roles.AllButton.OnMouseOut;
        UnityEvent mouse_over = roles.AllButton.OnMouseOver;

        Object.Instantiate(roles.transform.Find("Gradient").gameObject, transform).name = "Gradient";
        GameObject header = new("HeaderButtons");
        header.transform.SetParent(transform);
        header.transform.localPosition = Vector3.zero;
        Object.Instantiate(roles.transform.Find("HeaderButtons/DividerImage").gameObject, header.transform).name = "DividerImage";
        
        GameObject role_chances = new("RoleChamcesButton");
        role_chances.transform.SetParent(header.transform);
        role_chances.transform.localPosition = new(-2.8f, 2.3f, -2f);
        role_chances.transform.localScale = Vector3.one;
        role_chances.layer = 5;
        SpriteRenderer role_chances_renderer = role_chances.AddComponent<SpriteRenderer>();
        role_chances_renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.TabIcon.png", 100f);
        role_chances_renderer.drawMode = SpriteDrawMode.Sliced;
        role_chances_renderer.size = Vector2.one * 0.75f;
        CircleCollider2D role_chances_collider = role_chances.AddComponent<CircleCollider2D>();
        role_chances_collider.radius = 0.75f;
        RoleChancesButton = role_chances.AddComponent<PassiveButton>();
        RoleChancesButton.OnClick.AddListener(() =>
        {
            Logger.Info("RoleChancesButton Click", "ModSettingsMenu");
        });
        RoleChancesButton.Colliders = new Collider2D[] { role_chances_collider };
        RoleChancesButton.OnMouseOut = mouse_out;
        RoleChancesButton.OnMouseOver = mouse_over;
    }

    private void OnDisable() => CloseMenu();

    public void OpenMenu()
    {
        // ControllerManager.Instance.OpenOverlayMenu(name, BackButton, DefaultButtonSelected, ControllerSelectable.ToIl2CppList());
        OpenTab(0);
    }
    public void CloseMenu() => ControllerManager.Instance.CloseOverlayMenu(name);

    public void OpenTab(int id)
    {
        CloseAllTab();
        switch (id)
        {
            case 0:
                // RoleChancesSettings.SetActive(true);
                ControllerSelectable.AddRange(RoleChancesTabSelectables);
                // ScrollBar.CalculateAndSetYBounds(CustomRoleOption.RoleOptions.Count + 3, 1f, 6f, 0.43f);
                RoleChancesButton.SelectButton(true);
                // ControllerManager.Instance.SetDefaultSelection(RoleChances[0].ControllerSelectable[0]);
                break;
        }
        // ScrollBar.ScrollToTop();
        ControllerManager.Instance.CurrentUiState.SelectableUiElements = ControllerSelectable.ToIl2CppList();
    }

    public void CloseAllTab()
    {
        // RoleChancesSettings.SetActive(false);
        /*
        GenericSettings.SetActive(false);
        ImpostorSettings.SetActive(false);
        NeutralSettings.SetActive(false);
        CrewmateSettings.SetActive(false);
        ModifierSettings.SetActive(false);
        MatchTagSettings.SetActive(false);
        //*/

        RoleChancesButton.SelectButton(false);
        /*
        GenericButton.SelectButton(false);
        ImpostorButton.SelectButton(false);
        NeutralButton.SelectButton(false);
        CrewmateButton.SelectButton(false);
        ModifierButton.SelectButton(false);
        MatchTagButton.SelectButton(false);
        //*/

        ControllerSelectable.Clear();
    }
}
