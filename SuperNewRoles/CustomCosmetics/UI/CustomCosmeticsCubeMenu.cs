namespace SuperNewRoles.CustomCosmetics.UI;

public class CustomCosmeticsCubeMenu : CustomCosmeticsMenuBase<CustomCosmeticsCubeMenu>
{
    public override CustomCosmeticsMenuType MenuType => CustomCosmeticsMenuType.cube;

    public override void Initialize()
    {
        PlayerCustomizationMenu instance = PlayerCustomizationMenu.Instance;
        instance.SetItemName("");
        instance.cubesTab.gameObject.SetActive(true);
        instance.cubeArea.SetActive(true);
        instance.cubesTab.transform.localPosition = new(instance.cubesTab.transform.localPosition.x, instance.cubesTab.transform.localPosition.y, -20);
        instance.cubesTab.transform.Find("Text").gameObject.SetActive(false);
        instance.cubesTab.maskArea.transform.localPosition = new(0.5178f, -2.425f, 0);
        instance.cubesTab.maskArea.transform.localScale = new(5.1807f, 4.16f, 1f);
    }
    public override void Update()
    {
        PlayerCustomizationMenu instance = PlayerCustomizationMenu.Instance;
        instance.cubesTab.transform.localPosition = new(instance.cubesTab.transform.localPosition.x, instance.cubesTab.transform.localPosition.y, -20);
    }
    public override void Hide()
    {
        PlayerCustomizationMenu instance = PlayerCustomizationMenu.Instance;
        instance.cubesTab.gameObject.SetActive(false);
        instance.cubeArea.SetActive(false);
        instance.PreviewArea.gameObject.SetActive(true);
    }

}

