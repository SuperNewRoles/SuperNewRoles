using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SuperNewRoles.Modules;

public class AddressableReleaseOnDestroy : MonoBehaviour
{
    private List<AssetReference> assetReferences = new();
    public void Init(List<AssetReference> assetReferences)
    {
        this.assetReferences = assetReferences;
    }
    public void OnDestroy()
    {
        foreach (var assetReference in assetReferences)
        {
            if (assetReference != null && assetReference.IsValid())
            {
                assetReference.ReleaseAsset();
            }
        }
    }
}
