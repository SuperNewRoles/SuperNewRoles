using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomCosmetics.UI;

public class CustomCosmeticsCostumeMenu : CustomCosmeticsMenuBase<CustomCosmeticsCostumeMenu>
{
    public override CustomCosmeticsMenuType MenuType => CustomCosmeticsMenuType.costume;
    private GameObject kisekae;
    public override void Initialize()
    {
        // CosmeticMenuKisekae
        var obj = GameObject.FindObjectOfType<PlayerCustomizationMenu>();
        kisekae = GameObject.Instantiate(AssetManager.GetAsset<GameObject>("CosmeticMenuKisekae"), obj.transform);
        kisekae.transform.localPosition = new(0, 0, -10);
        kisekae.transform.localScale = Vector3.one * 0.27f;
    }

    public override void Update()
    {
    }
    public override void Hide()
    {
        GameObject.Destroy(kisekae);
    }
}